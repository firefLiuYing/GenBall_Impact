namespace GenBall.Utils.EntityCreator
{
    public interface IEntity
    {
        public void EntityUpdate(float deltaTime);
        public void EntityFixedUpdate(float fixedDeltaTime);
        public void OnRecycle();
    }
}