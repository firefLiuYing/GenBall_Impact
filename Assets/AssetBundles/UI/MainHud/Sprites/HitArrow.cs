using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using DG.Tweening;
using System.Diagnostics;
using UnityEngine.UI;

public class HitArrow : MonoBehaviour
{
    RectTransform rt;
    Image image;
    Tween moveTween;

    public enum ShootResult
    {
        Hit,
        Miss,
    }

    [SerializeField] private float inTime;
    [SerializeField] private float outTime;
    [SerializeField] private float distance;
    
    public void FirstHit()
    {
        rt = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        
        Color c = image.color;
        c.a = 0;
        image.color = c;
        
        Vector3 pivotWorld = rt.TransformPoint(rt.pivot);

        UnityEngine.Debug.Log(rt.position);

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Vector3 startPoint = pivotWorld + (pivotWorld - screenCenter) * distance;
        Vector3 endPoint = pivotWorld;
        
        // UnityEngine.Debug.Log(screenCenter);
        // UnityEngine.Debug.Log(rt.position);
        
        rt.position = startPoint;

        // UnityEngine.Debug.Log(startPoint);
        // UnityEngine.Debug.Log(endPoint);

        rt.DOMove(endPoint, inTime).SetEase(Ease.InQuad);
        image.DOFade(1, inTime).SetEase(Ease.InQuad);
    }

    public void endHit()
    {
        rt = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        
        Vector3 pivotWorld = rt.TransformPoint(rt.pivot);

        UnityEngine.Debug.Log(rt.position);

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Vector3 startPoint = pivotWorld + (pivotWorld - screenCenter) * distance;
        Vector3 endPoint = pivotWorld;
        
        rt.position = endPoint;

        rt.DOMove(startPoint, inTime).SetEase(Ease.OutQuad);
        image.DOFade(0,inTime).SetEase(Ease.OutQuad);
    }

    public void shake()
    {
        rt = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        
        Vector3 pivotWorld = rt.TransformPoint(rt.pivot);
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        
        Vector3 shakeAxis = pivotWorld -  screenCenter;
        
        //rt.DOShakePosition(1f,shakeAxis * 0.1f,10,90f,true,false );  这个属于方案一感觉勉强可用
        //shakeAxis = new Vector3(shakeAxis.y, shakeAxis.x, shakeAxis.z);

        rt.DOShakePosition(0.3f, shakeAxis * 0.08f, 12, 90f, true, false);
        rt.DOShakeScale(0.3f, shakeAxis * 0.001f, 12, 0f, false);
    }

    public void hitArrowAnimation(ShootResult shootResult)
    {
        // switch (shootResult)
        // {
        //     
        // }
    }

    // Start is called before the first frame update
    void Start()
    {
        
        shake();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
