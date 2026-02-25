using System;
using GenBall.Event.Generated;
using GenBall.Utils.Singleton;
using UnityEngine;

namespace GenBall.Procedure.Game
{
    public class PauseManager : ISingleton
    {
        public static PauseManager Instance => SingletonManager.GetSingleton<PauseManager>();
        public PauseState State { get;private set; } = PauseState.Unpaused;

        public void SetPauseState(PauseState state)
        {
            State = state;
            Debug.Log($"gzp 游戏暂停状态修改：{state}");
            GameEntry.Event.FireSystemPause(state);
        }
    }

    [Flags]
    public enum PauseState
    {
        /// <summary>
        /// 未暂停
        /// </summary>
        Unpaused = 0,
        /// <summary>
        /// 逻辑暂停：各种伤害判定，实体的状态机等，一般和物理暂停一起
        /// </summary>
        LogicPaused = 1,
        /// <summary>
        /// 物理暂停：实体的逻辑位置，逻辑旋转变化暂停，一般和逻辑暂停一起
        /// </summary>
        PhysicsPaused = 2,
        /// <summary>
        /// 游戏暂停：还没定好是什么意思，先插个眼
        /// </summary>
        GamePaused = 4,
        /// <summary>
        /// 动画暂停：字面意思，一般到动画暂停时其他也都暂停了
        /// </summary>
        AnimationPaused = 8,
        /// <summary>
        /// 声音暂停：字面意思，一般到声音暂停时其他也暂停了
        /// </summary>
        AudioPaused = 16,
    }
}