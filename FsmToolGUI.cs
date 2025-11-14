using System;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using Steamworks;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace FsmTool;

public class FsmToolGUI : MonoBehaviour
{
    private const int MainWindowId = 0xFA78981;
    private Rect _mainWindowRect = new(40, 40, 300, 400);
    private Vector2 _mainWindowScrollViewVector = Vector2.zeroVector;

    private const int FsmListWindowId = 0xFA78982;
    private Rect _fsmListWindowRect = new(300, 40, 200, 400);
    private Vector2 _fsmListWindowScrollViewVector;

    private const int FsmEditWindowId = 0xFA78983;
    private Rect _fsmEditWindowRect = new(300, 40, 400, 400);
    private Vector2 _fsmEditWindowScrollViewVector;
    private readonly string[] _fsmEditPageNames = ["Variables", "States", "Events"];
    private int _fsmEditPage = 0;

    private const int FsmStateInfoWindowId = 0xFA78984;
    private Rect _fsmStateInfoRect = new(40, 300, 400, 400);
    private Vector2 _fsmStateInfoScrollViewVector;
    private readonly string[] _fsmStateInfoPageNames = ["Transitions", "Actions"];
    private int _fsmStateInfoPage = 0;

    private GameObject? _selectedGameObject;
    private Fsm? _selectedFsm;
    private FsmState? _selectedFsmState;

    private Dictionary<string, Ref<string>> _editedStrings = new();
    private Dictionary<string, Ref<string>> _editedTransitionEvents = new();
    private Dictionary<string, Ref<string>> _editedTransitionState = new();

    private static string GetHierarchyPath(GameObject o)
    {
        if (!o)
            return String.Empty;
        var transform = o.transform;
        string path = o.name;
        while (transform.parent)
        {
            transform = transform.parent;
            path = $"{transform.name}/{path}";
        }

        return path;
    }

    private string ShowFsmVar(NamedVariable fsmVar, string? altString = null)
    {
        GUILayout.Space(10);

        GUILayout.Label(fsmVar.Name, GUILayout.ExpandWidth(true));

        var editedVal = _editedStrings.GetValueOrDefault(fsmVar.Name);
        if (editedVal == null)
        {
            editedVal = new Ref<string>(altString ?? fsmVar.RawValue.ToString());
            _editedStrings.Add(fsmVar.Name, editedVal);
        }

        editedVal.Value = GUILayout.TextField(editedVal);
        return editedVal.Value;
    }

    private void OnGUI()
    {
        _mainWindowRect = GUI.Window(MainWindowId, _mainWindowRect, MainWindow, "FSM GameObjects");
        if (_selectedGameObject && _selectedFsm == null)
        {
            _fsmListWindowRect = GUI.Window(FsmListWindowId, _fsmListWindowRect, FsmListWindow, "FSMs");
        }

        if (_selectedFsm != null)
        {
            _fsmEditWindowRect = GUI.Window(FsmEditWindowId, _fsmEditWindowRect, FsmEditWindow, _selectedFsm.name);
        }

        if (_selectedFsmState != null)
        {
            _fsmStateInfoRect = GUI.Window(FsmStateInfoWindowId, _fsmStateInfoRect, FsmStateInfoWindow,
                _selectedFsmState.Name);
        }
    }


    private void FsmEditWindow(int windowId)
    {
        if (_selectedFsm == null || !_selectedFsm.GameObject)
        {
            _selectedFsm = null;
            return;
        }

        _fsmEditPage = GUILayout.Toolbar(_fsmEditPage, _fsmEditPageNames);

        GUILayout.BeginVertical(new GUIStyle { padding = new RectOffset(10, 10, 10, 10) });

        GUILayout.BeginHorizontal();
        GUILayout.Label("Current State");
        GUILayout.Label(_selectedFsm.activeStateName);
        GUILayout.EndHorizontal();

        _fsmEditWindowScrollViewVector = GUILayout.BeginScrollView(_fsmEditWindowScrollViewVector);
        GUILayout.BeginVertical();
        switch (_fsmEditPage)
        {
            case 0:
                FsmEditWindowVariablePage(_selectedFsm);
                break;
            case 1:
                FsmEditWindowStatePage(_selectedFsm);
                break;
            default:
                break;
        }

        GUILayout.EndVertical();
        GUILayout.EndScrollView();

        GUILayout.Label("Options");
        var options = FsmSettings.GetFsmOptions(_selectedFsm, createIfMissing: true)!;
        var newFreeze = GUILayout.Toggle(options.Freeze, "Freeze");

        if (!options.Freeze && newFreeze)
        {
            _selectedFsm.Freeze(true);
        }
        else if (options.Freeze && !newFreeze)
        {
            _selectedFsm.Freeze(false);
        }

        options.Freeze = newFreeze;
        options.LogTransitions = GUILayout.Toggle(options.LogTransitions, "Log Transitions");

        GUILayout.EndVertical();

        GUI.DragWindow();
    }

    private void MainWindow(int windowId)
    {
        GUILayout.BeginVertical();
        _mainWindowScrollViewVector = GUILayout.BeginScrollView(_mainWindowScrollViewVector);

        foreach (var fsmGameObject in PlayMakerFSM.FsmList.Select(fsm => fsm.gameObject).Distinct())
        {
            if (GUILayout.Button(fsmGameObject.name))
            {
                _selectedGameObject = fsmGameObject;
                _selectedFsm = null;
                _editedStrings.Clear();
                break;
            }
        }

        GUILayout.EndScrollView();
        GUILayout.EndHorizontal();
        Fsm.BreakpointsEnabled = GUILayout.Toggle(Fsm.BreakpointsEnabled, "Enable Breakpoints");
        FsmLog.LoggingEnabled = GUILayout.Toggle(FsmLog.LoggingEnabled, "Enable Logs");
        if (Fsm.HitBreakpoint && GUILayout.Button("Resume FSMs"))
            Fsm.BreakAtFsm.Continue();
        if (GUILayout.Button("Close"))
            gameObject.SetActive(false);
        GUI.DragWindow();
    }

    private void FsmListWindow(int windowId)
    {
        if (_selectedGameObject)
        {
            _fsmListWindowScrollViewVector = GUILayout.BeginScrollView(_fsmListWindowScrollViewVector);
            GUILayout.BeginVertical();
            foreach (var fsm in _selectedGameObject.GetComponents<PlayMakerFSM>())
            {
                if (GUILayout.Button(fsm.fsm.name))
                {
                    _selectedFsm = fsm.fsm;
                    _editedStrings.Clear();
                    break;
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
        else
            GUILayout.Label("GameObject is no longer available");

        GUI.DragWindow();
    }

    private void FsmEditWindowVariablePage(Fsm fsm)
    {
        if (fsm.Variables.BoolVariables.Length != 0)
        {
            GUILayout.Label("Bool");
            foreach (var variable in fsm.Variables.BoolVariables)
            {
                variable.Value = GUILayout.Toggle(variable.Value, variable.name);
            }
        }

        if (fsm.Variables.StringVariables.Length != 0)
        {
            GUILayout.Label("String");
            foreach (var variable in fsm.Variables.StringVariables)
            {
                GUILayout.BeginHorizontal();
                var res = ShowFsmVar(variable);
                if (GUILayout.Button("Set"))
                    variable.Value = res;
                GUILayout.EndHorizontal();
            }
        }

        if (fsm.Variables.IntVariables.Length != 0)
        {
            GUILayout.Label("Int");
            foreach (var variable in fsm.Variables.IntVariables)
            {
                GUILayout.BeginHorizontal();
                var res = ShowFsmVar(variable);
                if (Int32.TryParse(res, out var value))
                {
                    if (GUILayout.Button("Set"))
                        variable.Value = value;
                }
                else
                {
                    GUILayout.Button("-X-");
                }

                GUILayout.EndHorizontal();
            }
        }

        if (fsm.Variables.FloatVariables.Length != 0)
        {
            GUILayout.Label("Float");
            foreach (var variable in fsm.Variables.FloatVariables)
            {
                GUILayout.BeginHorizontal();
                var res = ShowFsmVar(variable);
                if (Single.TryParse(res, out var value))
                {
                    if (GUILayout.Button("Set"))
                        variable.Value = value;
                }
                else
                {
                    GUILayout.Button("-X-");
                }

                GUILayout.EndHorizontal();
            }
        }

        if (fsm.Variables.GameObjectVariables.Length != 0)
        {
            GUILayout.Label("GameObject");
            foreach (var variable in fsm.Variables.GameObjectVariables)
            {
                GUILayout.BeginHorizontal();
                var goPath = GetHierarchyPath(variable.Value);
                var res = ShowFsmVar(variable, goPath);
                if (GUILayout.Button("Set"))
                {
                    var go = GameObject.Find(res);
                    if (!go)
                    {
                        FsmToolPlugin.Logger.LogInfo($"Failed to find game object '{goPath}'");
                    }
                    else
                    {
                        FsmToolPlugin.Logger.LogInfo($"Set '{variable.Name}' to '{goPath}'");
                        variable.Value = go;
                    }
                }

                GUILayout.EndHorizontal();
            }
        }
    }

    private void FsmEditWindowStatePage(Fsm fsm)
    {
        foreach (var state in fsm.States)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(state.name);
            if (GUILayout.Button("Switch"))
            {
                fsm.SwitchState(state);
            }

            if (Fsm.BreakpointsEnabled && !state.IsBreakpoint)
            {
                state.IsBreakpoint = GUILayout.Button("Set Breakpoint");
            }
            else if (state.IsBreakpoint)
            {
                state.IsBreakpoint = !GUILayout.Button("Remove Breakpoint");
            }

            GUILayout.EndHorizontal();
        }
    }

    private void FsmStateInfoWindow(int windowId)
    {
        if (_selectedFsmState == null)
            return;
        _fsmStateInfoPage = GUILayout.Toolbar(_fsmStateInfoPage, _fsmStateInfoPageNames);
        GUILayout.BeginVertical(new GUIStyle { padding = new RectOffset(10, 10, 10, 10) });

        _fsmStateInfoScrollViewVector = GUILayout.BeginScrollView(_fsmStateInfoScrollViewVector);
        switch (_fsmStateInfoPage)
        {
            case 0:
                FsmTransitionInfo(_selectedFsmState);
                break;
            case 1:
                FsmActionInfo(_selectedFsmState);
                break;
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUI.DragWindow();
    }

    private void FsmTransitionInfo(FsmState state)
    {
        foreach (var transition in state.Transitions)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Event");
            transition.fsmEvent = FsmEvent.GetFsmEvent(GUILayout.TextField(transition.FsmEvent.Name));
            GUILayout.Label("To State");
            transition.toState = GUILayout.TextField(transition.toState);
            transition.ToFsmState = state.fsm.GetState(transition.toState);
            GUILayout.EndHorizontal();
        }
    }

    private void FsmActionInfo(FsmState state)
    {
        foreach (var action in state.Actions)
        {
            GUILayout.Label(action.Name, new GUIStyle
            {
                normal = new GUIStyleState { textColor = action.Enabled ? Color.red : Color.white }
            });
        }
    }
}