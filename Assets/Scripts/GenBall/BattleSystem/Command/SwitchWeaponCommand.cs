using System.Runtime.InteropServices;

namespace GenBall.BattleSystem.Command
{
    /// <summary>
    /// Switches to a different weapon. Has duration (equip animation), blocks
    /// Attack and Reload during the animation. Dash can interrupt.
    ///
    /// Priority: 3/4 — can interrupt Attack (2/2) and Reload (2/3),
    /// blocked by Dash (5/5). Not bufferable.
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct SwitchWeaponCommand : IArbitratedCommand
    {
        public readonly int WeaponSlot;

        public int InterruptPriority => 3;
        public int AntiInterruptPriority => 4;
        public bool Bufferable => false;

        /// <param name="weaponSlot">Target weapon slot index, or -1 for "next weapon"</param>
        public SwitchWeaponCommand(int weaponSlot = -1)
        {
            WeaponSlot = weaponSlot;
        }
    }
}
