namespace GenBall.UI
{
    public partial class SplashForm : FormBase
    {
        private SplashFormVm _splashFormVm;
        protected override void OnInit(object args = null)
        {
            base.OnInit(args);
            Bind();
            
            _isOpen = true;
        }

        protected override void OnOpen(object args = null)
        {
            base.OnOpen(args);
            
            _splashFormVm=GetVm<SplashFormVm>();
            
            RegisterEvents();
        }

        protected override void OnClose(object args = null)
        {
            UnRegisterEvents();
            
            base.OnClose(args);
            _isOpen = false;
        }

        private void RegisterEvents()
        {
            _splashFormVm.SplashProcess.Observe(OnProcessChanged);
        }

        private void UnRegisterEvents()
        {
            _splashFormVm.SplashProcess.Unobserve(OnProcessChanged);
        }

        private void OnProcessChanged(float process)
        {
            _autoTxtProcess.text = $"初始化中... 当前进度：{process:P} .";
        }

        private static bool _isOpen = false;
        public static void Open()
        {
            if(_isOpen) return;
            GameEntry.UI.OpenForm<SplashForm>();
        }
    }
}