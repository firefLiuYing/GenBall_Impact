using UnityEngine;

namespace GenBall.UI
{
    [RequireComponent(typeof(Canvas))]
    public abstract class FormBase : MonoBehaviour, IUserInterface
    {
        private Canvas _canvas;
        public Canvas Canvas=> _canvas ??= GetComponent<Canvas>();
        public virtual void OnInit(object args = null)
        {
            
        }
        public virtual void EntityUpdate(float deltaTime)
        {
            
        }

        public virtual void EntityFixedUpdate(float fixedDeltaTime)
        {
            
        }

        public virtual void OnRecycle()
        {
            
        }


        public virtual void OnOpen(object args = null)
        {
            
        }

        public virtual void OnClose(object args = null)
        {
            
        }

        public virtual void OnUnfocus()
        {
            
        }

        public virtual void OnFocus()
        {
            
        }

        public virtual void OnPause(object args = null)
        {
            
        }

        public virtual void OnResume(object args = null)
        {
            
        }

    }
}