namespace GenBall.UI
{
    public partial class InteractTipItem : ItemBase,ICellView
    {
        public class Args
        {
            public string OperationDescription;
        }
        private Args _args;
        private int _index;
        private bool _haveBind=false;
        public void OnRefresh(int index = 0, object args = null)
        {
            if (!_haveBind)
            {
                Bind();
                _haveBind = true;
            }
            _index = index;
            _args = args as Args;
            if(_args == null) return;
            Refresh();
        }

        private void Refresh()
        {
            if(_args == null) return;
            _autoTxtDiscription.text = _args.OperationDescription;
        }
    }
}