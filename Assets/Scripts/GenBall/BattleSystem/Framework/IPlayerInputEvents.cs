using System;
using GenBall.Player;
using UnityEngine;

namespace GenBall.BattleSystem.Framework
{
    public interface IPlayerInputEvents
    {
        event Action<ButtonState> OnJump;
        event Action<ButtonState> OnDash;
        event Action<ButtonState> OnFire;
        event Action<ButtonState> OnReload;
        event Action<ButtonState> OnSwitchWeapon;
        event Action OnInteract;
        event Action<float> OnScroll;
        event Action<ButtonState> OnAbilitySecondary;
        event Action<ButtonState> OnAbilityWheel;
        Vector3 MoveDirection { get; }
        Vector2 ViewDelta { get; }
    }
}
