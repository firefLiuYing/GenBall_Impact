namespace GenBall.UI
{
    public partial class HeartItem:ItemBase,ICellView
    {
        public class Args
        {
            public int Armor;
            public int Health;
        }

        private Args _args;
        public void OnRefresh(int index = 0, object args = null)
        {
            Bind();
            if(args is not Args heartArgs) return;
            _args = heartArgs;
            UpdateHeart(_args);
        }

        private void UpdateHeart(Args heartArgs)
        {
            _autoImgFullHeart.SA(heartArgs.Health==2);
            _autoImgFullArmor.SA(heartArgs.Armor==2);
            _autoImgHalfHeart.SA(heartArgs.Health==1);
            _autoImgHalfArmor.SA(heartArgs.Armor==1);
            _autoImgOutHeart.SA(heartArgs.Health==0&&heartArgs.Armor==0);
        }
    }
}