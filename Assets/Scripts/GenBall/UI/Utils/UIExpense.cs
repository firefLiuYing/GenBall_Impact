using UnityEngine;
using UnityEngine.UI;

namespace GenBall.UI
{
    public static class UIExpense
    {
        public static void SA(this Image image, bool active)=>image.gameObject.SetActive(active);
        public static void SA(this Text text, bool active)=>text.gameObject.SetActive(active);
        public static void SA(this RectTransform  rect, bool active)=>rect.gameObject.SetActive(active);
        public static void SA(this Button button,bool active)=>button.gameObject.SetActive(active);
    }
}