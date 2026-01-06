using GenBall.Event.Generated;
using GenBall.Utils.Singleton;

namespace GenBall.Economic
{
    public class EconomicController : ISingleton
    {
        public EconomicController Instance => SingletonManager.GetSingleton<EconomicController>();
        private int _dataPoints;
        /// <summary>
        /// Ô²ÐÎÊý¾Ý
        /// </summary>
        public int DataPoints
        {
            get => _dataPoints;
            private set
            {
                _dataPoints = value;
                GameEntry.Event.FirePlayerDataPoints(_dataPoints);
            }
        }
    }
}