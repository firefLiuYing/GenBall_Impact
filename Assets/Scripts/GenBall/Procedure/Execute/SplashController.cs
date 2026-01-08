using GenBall.UI;
using GenBall.Utils.Singleton;
using UnityEngine;
using Yueyn.Base.Variable;

namespace GenBall.Procedure.Execute
{
    public class SplashController : ISingleton
    {
        public static SplashController Instance=>SingletonManager.GetSingleton<SplashController>();

        public void OpenSplashForm()
        {
            SplashForm.Open();
            SetSplashProcess(0f);
        }

        public Variable<float> SplashProcess=Variable<float>.Create();
        public void SetSplashProcess(float process)
        {
            process = Mathf.Clamp01(process);
            SplashProcess.PostValue(process);
        }
    }
}