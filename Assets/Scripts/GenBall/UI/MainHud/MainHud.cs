namespace GenBall.UI
{
    public partial class MainHud : FormBase
    {
        protected override void OnInit(object args = null)
        {
            base.OnInit(args);
            Bind();
        }

        protected override void OnOpen(object args = null)
        {
            base.OnOpen(args);
            // _autoImgImage.gameObject.SetActive(false);
        }
    }
}