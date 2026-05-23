using GenBall.Interact;
using GenBall.Utils.Trigger;
using Yueyn.Main;
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
                Debug.LogError("gzp �浵��û�󶨴�����");
            }
        }
        private void Start()
        {
            _triggerObject.onTriggerEnter.AddListener(OnEnter);
            _triggerObject.onTriggerExit.AddListener(OnExit);
        }
        private void OnEnter()
        {
            SystemRepository.Instance.GetSystem<IInteractSystem>().AddInteractable(this);
        }

        private void OnExit()
        {
            SystemRepository.Instance.GetSystem<IInteractSystem>().RemoveInteractable(this);
        }

        public string OperationDescription => _savePointConfig.DisplayName;
        public void Interact()
        {
            Debug.Log($"��ʱӦ�ô򿪴浵��:{_savePointConfig.DisplayName}�����˵�");
        }
    }
}