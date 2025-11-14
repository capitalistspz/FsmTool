using HutongGames.PlayMaker;

namespace FsmTool;

public static class FsmExtensions
{
    public static void Freeze(this Fsm fsm, bool doFreeze)
    {
        if (doFreeze)
        {
            fsm.SendDisableEvent();
            fsm.Finished = true;
        }
        else
        {
            fsm.Finished = false;
            fsm.switchToState = fsm.ActiveState;
        }
    }
}