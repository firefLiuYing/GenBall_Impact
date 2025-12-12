using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class KillArrow : MonoBehaviour
{
    RectTransform rt;
    Image image;
    
    public void Kill()
    {
        rt = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        
        Color c = image.color;
        c.a = 0;
        image.color = c;
        
        Vector3 pivotWorld = rt.TransformPoint(rt.pivot);

        UnityEngine.Debug.Log(rt.position);

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Vector3 startPoint = pivotWorld + (pivotWorld - screenCenter) * 0.8f;
        Vector3 endPoint = pivotWorld;
        
        // UnityEngine.Debug.Log(screenCenter);
        // UnityEngine.Debug.Log(rt.position);
        
        rt.position = startPoint;

        // UnityEngine.Debug.Log(startPoint);
        // UnityEngine.Debug.Log(endPoint);

        image.DOFade(1, 0.3f);
        rt.DOMove(endPoint, 0.3f).SetEase(Ease.InQuad) .OnComplete(() =>
        {
            image.DOFade(0, 0.2f)   
                .SetDelay(0.5f)        // 移动完再等 0.5 s
                .SetEase(Ease.InQuad);
        });
    }
    
    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        
        Color c = image.color;
        c.a = 0;
        image.color = c;
        Kill();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
