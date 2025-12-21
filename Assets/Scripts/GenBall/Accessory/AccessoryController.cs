using System.Collections.Generic;
using GenBall.Utils.Singleton;

namespace GenBall.Accessory
{
    public class AccessoryController : ISingleton
    {
        public static AccessoryController Instance => SingletonManager.GetSingleton<AccessoryController>();
        
        private readonly AccessoryInfo _accessory = new();

        private readonly List<BaseModule> _baseModulePackage = new();
        private readonly List<AccessoryInfo> _accessoryPackage = new();

        public void Init()
        {
            _accessory.Level = 0;
        }
        
        public void ApplyUpgrade()
        {
            
        }

        private int KillPointsToLevel(int killPoints)
        {
            return killPoints / 10;
        }
    }
}