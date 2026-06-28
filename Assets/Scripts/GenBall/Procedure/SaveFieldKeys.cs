namespace GenBall.Procedure
{
    /// <summary>
    /// Central registry of all save data field names.
    /// Grouped by provider (DataKey). Use these constants to prevent typos
    /// when calling GameManager.UpdateSaveFields().
    /// </summary>
    public static class SaveFieldKeys
    {
        public static class Player
        {
            public const string LastSavePointIndex = "lastSavePointIndex";
            public const string LastSceneName = "lastSceneName";
        }

        public static class Map
        {
            public const string UnlockedScenes = "unlockedScenes";
        }
    }
}
