using System;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
namespace FsmTool;

[BepInAutoPlugin(id: "capitalistspz.fsmtool")]
public partial class FsmToolPlugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger;
    private void Awake()
    {
        Logger = base.Logger;
        var menuObj = new GameObject("FsmToolGUI");
        menuObj.AddComponent<FsmToolGUI>();
        DontDestroyOnLoad(menuObj);
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
    }
    
}