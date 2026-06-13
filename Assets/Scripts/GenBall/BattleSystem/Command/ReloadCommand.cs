using System.Runtime.InteropServices;

namespace GenBall.BattleSystem.Command
{
    /// <summary>
    /// Reloads the current weapon. Has duration (reload time), blocks Attack
    /// and SwitchWeapon during the animation. Dash can interrupt.
    ///
    /// Priority: 2/3 — can interrupt Attack (2/2), blocked by Dash (5/5).
    /// Not bufferable — if the player mashes reload, only the first one counts.
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct ReloadCommand : IArbitratedCommand
    {
        public int InterruptPriority => 2;
        public int AntiInterruptPriority => 3;
        public bool Bufferable => false;
        public bool BlocksMove => true;
        public bool BlocksRotate => false;
        public bool BlocksGravity => false;
    }
}

