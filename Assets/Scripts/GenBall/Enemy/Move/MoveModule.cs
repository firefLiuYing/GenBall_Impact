using UnityEngine;

namespace GenBall.Enemy.Move
{
    public abstract class MoveModule : Module
    {
        /// <summary>
        /// 指定移动目的地的世界坐标
        /// </summary>
        /// <param name="target"></param>
        public abstract void MoveTo(Vector3 target);
        public abstract void StopMove();
    }
}