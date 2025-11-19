using GenBall.Utils.EntityCreator;
using UnityEngine;

namespace GenBall.UI
{
    public interface IUserInterface : IEntity
    {
        public void OnInit(object args=null);
        public void OnOpen(object args=null);
        public void OnClose(object args=null);
        public void OnUnfocus();
        public void OnFocus();
        public void OnPause(object args=null);
        public void OnResume(object args=null);
        public Canvas Canvas { get;}
    }
}