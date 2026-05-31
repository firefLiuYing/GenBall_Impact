using System.Runtime.InteropServices;
using GenBall.Player;

namespace GenBall.BattleSystem.Command
{
    [StructLayout(LayoutKind.Auto)]
    public struct AbilitySecondaryCommand : IArbitratedCommand
    {
        public readonly ButtonState TriggerState;

        public int InterruptPriority { get; }
        public int AntiInterruptPriority { get; }
        public bool Bufferable => true;
        public bool BlocksMove => false;
        public bool BlocksRotate => false;

        public AbilitySecondaryCommand(ButtonState triggerState, int interruptPriority = 2, int antiInterruptPriority = 2)
        {
            TriggerState = triggerState;
            InterruptPriority = interruptPriority;
            AntiInterruptPriority = antiInterruptPriority;
        }
    }
}
