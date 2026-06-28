namespace GenBall.UI
{
    public class SaveSlotFormViewData
    {
        public System.Collections.Generic.List<SaveSlotItemInfo> Slots = new();
    }

    public class SaveSlotItemInfo
    {
        public int SaveIndex;
        public bool IsEmpty;
        public string SceneName;
        public string PlayTimeText;
        public string CreateTimeText;
    }
}

