using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class testDotween : MonoBehaviour
{

    public void DoSpin(float duration = 0.5f)
    {
        // 使用局部坐标系旋转，保证不会受父级影响
        transform.DOLocalRotate(
            new Vector3(0, 0, 360),   // 绕自身 z 轴转 360°
            duration,                 // 动画时长
            RotateMode.LocalAxisAdd   // 在现有角度基础上累加
        ).SetRelative(true);          // 相对模式，确保总是“再转一圈”
    }

    /* —— 可选：给 Inspector 一个快速测试按钮 —— */
    [ContextMenu("Test Spin")]
    private void TestSpin() => DoSpin();

    // Start is called before the first frame update
    void Start()
    {
        DoSpin();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
