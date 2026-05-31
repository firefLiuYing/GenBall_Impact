using System.Runtime.InteropServices;

namespace GenBall.BattleSystem.Command
{
    [StructLayout(LayoutKind.Auto)]
    public struct WeaponVisibilityCommand : IArbitratedCommand
    {
        public readonly bool Visible;

        public int InterruptPriority => 4;
        public int AntiInterruptPriority => 4;
        public bool Bufferable => false;
        public bool BlocksMove => true;
        public bool BlocksRotate => true;

        public WeaponVisibilityCommand(bool visible)
        {
            Visible = visible;
        }
    }
}
