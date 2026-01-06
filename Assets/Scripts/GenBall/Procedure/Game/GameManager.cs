using GenBall.Utils.Singleton;

namespace GenBall.Procedure.Game
{
    public class GameManager : ISingleton
    {
        public static GameManager Instance => SingletonManager.GetSingleton<GameManager>();
        
    }
}