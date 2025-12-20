using GenBall.Utils.Singleton;

namespace GenBall.Accessory
{
    public class AccessoryController : ISingleton
    {
        public static AccessoryController Instance => SingletonManager.GetSingleton<AccessoryController>();
        
        private readonly AccessoryInfo _accessory = new();

        public void Init()
        {
            _accessory.Level = 0;
        }
    }
}