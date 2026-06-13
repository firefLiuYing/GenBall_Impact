namespace GenBall.Event
{
    /// <summary>
    /// Unified global event IDs for CEventRouter.
    /// Range allocation: Launch 1-99, Player 1000-1999, Input 2000-2999,
    /// Weapon 3000-3999, Enemy 4000-4999, System 5000-5999.
    /// </summary>
    public enum GlobalEventId
    {
        // === Launch / Procedure (1-99) ===
        StartupLoadingBegin = 1,
        StartupLoadingComplete = 2,
        StartFormBegin = 3,
        GameLaunch = 4,
        LoadingProgress = 5,
        LoadingComplete = 6,
        SceneReady = 7,
        InGameUIReady = 8,

        // === Player (1000-1999) ===
        HealthChanged = 1000,
        MaxHealthChanged = 1001,
        ArmorChanged = 1002,
        KillPointsChanged = 1003,
        DataPointsChanged = 1004,
        PositionChanged = 1005,

        // === Input (2000-2999) ===
        MoveInput = 2000,
        ViewInput = 2001,
        FireInput = 2002,
        JumpInput = 2003,
        DashInput = 2004,
        ReloadInput = 2005,
        UpgradeInput = 2006,

        // === Weapon (3000-3999) ===
        UnlockLevel = 3000,
        MagazineInfoChange = 3001,
        LevelChanged = 3002,
        AbilityCooldownChanged = 3010,
        AbilityWeaponActivated = 3011,
        AbilityWeaponDeactivated = 3012,

        // === Wheel (3013-3019) ===
        WheelOpened = 3013,
        WheelConfirmed = 3014,
        WheelCancelled = 3015,

        // === Enemy (4000-4999) ===
        EnemyDeath = 4000,
        EnemySpawned = 4001,

        // === System (5000-5999) ===
        Pause = 5000,
        Resume = 5001,
        GameStart = 5002,
        GameOver = 5003,
        /// <summary>Fired when pause state changes (menu/cutscene/resume). Args: none (read IPauseSystem).</summary>
        PauseChanged = 5004,
        CombatStateChanged = 5005,
    }
}
