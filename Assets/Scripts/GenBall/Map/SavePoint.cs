using GenBall.Interact;
using GenBall.Utils.Trigger;
using UnityEngine;

namespace GenBall.Map
{
    public class SavePoint : MonoBehaviour,IInteractable
    {
        private TriggerObject _triggerObject;
        private void Awake()
        {
            _triggerObject = GetComponentInChildren<TriggerObject>();
            if (_triggerObject == null)
            {
                Debug.LogError("gzp 存档点没绑定触发器");
            }
        }
        private void Start()
        {
            _triggerObject.onTriggerEnter.AddListener(OnEnter);
            _triggerObject.onTriggerExit.AddListener(OnExit);
        }
        private void OnEnter()
        {
            InteractSystem.Instance.AddInteractable(this);
        }

        private void OnExit()
        {
            InteractSystem.Instance.RemoveInteractable(this);
        }

        public string OperationDescription => "和存档点交互";
        public void Interact()
        {
            // todo gzp 打开存档点菜单
        }
    }
}