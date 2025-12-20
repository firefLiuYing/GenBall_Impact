using GenBall.Accessory;
using GenBall.Enemy;
using GenBall.Utils.Singleton;
using Yueyn.Event;

namespace GenBall.Player
{
    public class PlayerController:ISingleton
    {
        public static PlayerController Instance => SingletonManager.GetSingleton<PlayerController>();

        private readonly ActorInfo _actor = new();
        public void Init()
        {
            // todo gzp 后续修改为可配置
            _actor.MaxHealth = 6;
            _actor.Health = _actor.MaxHealth;
            _actor.Armor = _actor.MaxHealth;
            _actor.KillPoints = 0;


            GameEntry.GetModule<EventManager>().Subscribe(EnemyDeadEventArgs.Index,OnEnemyDead);
        }

        public void ApplyDamage(int damage)
        {
            if (_actor.Armor >= damage)
            {
                _actor.Armor -= damage;
            }
            else
            {
                damage-=_actor.Armor;
                _actor.Armor = 0;
                _actor.Health -= damage;
            }
        }
        

        private void OnEnemyDead(object sender, GameEventArgs eventArgs)
        {
            // todo gzp 可能会有范围判定
            if(eventArgs is not EnemyDeadEventArgs args) return;
            _actor.KillPoints += args.KillPoints;
        }
    }
}
