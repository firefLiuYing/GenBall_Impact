using System;
using GenBall.Utils.Trigger;
using UnityEngine;

namespace GenBall.Procedure
{
    public class SavePoint : MonoBehaviour
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
            Debug.Log("此时应该弹出交互按钮");
        }

        private void OnExit()
        {
            Debug.Log("此时应该关闭交互按钮");
        }
    }
}