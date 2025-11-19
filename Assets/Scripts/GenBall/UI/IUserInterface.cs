using GenBall.Utils.EntityCreator;
using UnityEngine;

namespace GenBall.UI
{
    public interface IUserInterface : IEntity
    {
        public void Init(object args=null);
        public void Open(object args=null);
        public void Close(object args=null);
        public void Unfocus();
        public void Focus();
        public void Pause(object args=null);
        public void Resume(object args=null);
        public Canvas Canvas { get;}
    }
}