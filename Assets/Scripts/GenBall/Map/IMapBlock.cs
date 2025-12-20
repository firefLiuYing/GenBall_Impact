using GenBall.Utils.EntityCreator;

namespace GenBall.Map
{
    public interface IMapBlock : IEntity
    {
        public void OnLoad();
        public void OnUnload();
        public void OnPlayerEnter();
        public void OnPlayerExit();
    }
}