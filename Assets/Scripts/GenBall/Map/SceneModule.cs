using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using GenBall.Procedure.Execute;
using GenBall.Procedure.Game;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yueyn.Main;

namespace GenBall.Map
{
    public class SceneModule : MonoBehaviour, IComponent
    {
        public int Priority => 1000;

        public void LoadScene([NotNull] string sceneName)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }

        private IEnumerator LoadSceneAsync([NotNull] string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("gzp 缺少场景名字");
                yield break;
            }
            var operation = SceneManager.LoadSceneAsync(sceneName);
            if (operation == null)
            {
                Debug.LogError("gzp 加载失败");
                yield break;
            }
            operation.allowSceneActivation = false;
            // SplashController.Instance.OpenSplashForm();
            while (operation.progress<0.9f)
            {
                yield return null;
            }
            operation.allowSceneActivation = true;
        }
        public void Init()
        {
            
        }

        public void OnUnregister()
        {
            
        }

        public void ComponentUpdate(float elapsedSeconds, float realElapseSeconds)
        {
            
        }

        public void ComponentFixedUpdate(float fixedDeltaTime)
        {
            
        }

        public void Shutdown()
        {
            
        }
    }

}