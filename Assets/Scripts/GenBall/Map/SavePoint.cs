using GenBall.Interact;
using GenBall.Utils.Trigger;
using UnityEngine;

namespace GenBall.Map
{
    public class SavePoint : MonoBehaviour,IInteractable
    {
        private TriggerObject _triggerObject;
        private SavePointConfig _savePointConfig;
        private void Awake()
        {
            _triggerObject = GetComponentInChildren<TriggerObject>();
            _savePointConfig = GetComponent<SavePointConfig>();
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

        public string OperationDescription => _savePointConfig.DisplayName;
        public void Interact()
        {
            Debug.Log($"此时应该打开存档点:{_savePointConfig.DisplayName}交互菜单");
        }
    }
}