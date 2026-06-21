using System.Collections.Generic;
using UnityEngine;
using Yueyn.Base.Variable;
using Yueyn.Main;

namespace GenBall.Interact
{
    public interface IInteractSystem : ISystem
    {
        Variable<List<IInteractable>> Interactables { get; }
        Variable<int> CurrentSelectionIndex { get; }
        void NextSelection();
        void LastSelection();
        void TriggerInteractable();
        void AddInteractable(IInteractable interactable);
        void RemoveInteractable(IInteractable interactable);

        /// <summary>
        /// 配置锥形视线检测参数。由 PlayerEntityFactory 在装配时调用。
        /// </summary>
        void Configure(float coneHalfAngle, float maxDistance, LayerMask interactableLayer);
    }
}
