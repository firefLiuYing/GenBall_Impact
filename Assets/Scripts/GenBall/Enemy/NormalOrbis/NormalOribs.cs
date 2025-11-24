namespace GenBall.Enemy.NormalOrbis
{
    public class NormalOrbis : BaseOrbis.BaseOrbis
    {
        public override void Initialize()
        {
            base.Initialize();
            gameObject.SetActive(true);
            StartFsm();
        }
    }
}