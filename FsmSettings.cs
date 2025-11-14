using System.Collections.Generic;
using HutongGames.PlayMaker;

namespace FsmTool;

public static class FsmSettings
{
    public class FsmOptions
    {
        public bool LogTransitions = false;
        public bool Freeze = false;
    }
    
    private static Dictionary<Fsm, FsmOptions> _fsmOptions = new();

    public static FsmOptions? GetFsmOptions(Fsm fsm, bool createIfMissing)
    {
        var options = _fsmOptions.GetValueOrDefault(fsm);
        if (createIfMissing && options == null)
        {
            options = new FsmOptions();
            _fsmOptions.Add(fsm, options);
        }
        return options;
    }
}