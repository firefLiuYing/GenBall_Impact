using GenBall.Utils.Countdown;

namespace GenBall.Player
{
    public partial class Player
    {
        internal CountdownController Countdown;

        private void InitCountdown()
        {
            Countdown = new CountdownController();
            RegisterCountdowns();
        }
        private void RegisterCountdowns()
        {
            Countdown.AddCountdownEvent("Dash",4f);
        }

        private void CountdownUpdate(float deltaTime)
        {
            Countdown.Update(deltaTime);
        }
    }
}