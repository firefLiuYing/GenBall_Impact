// using System;
// using Yueyn.Utils;
// using Object = UnityEngine.Object;
//
// namespace Yueyn.Demo.Resource
// {
//     public class ResourceManager:Singleton<ResourceManager>
//     {
//         protected override void Init()
//         {
//             
//         }
//
//         private IResourceHelper _helper;
//         public void SetResourceHelper(IResourceHelper helper)
//         {
//             _helper = helper;
//         }
//
//         /// <summary>
//         /// 异步加载资源
//         /// </summary>
//         void Load(string path, Action<object> onLoadSuccess, Action<string> onLoadFailed)
//         {
//             _helper.Load(path, onLoadSuccess, onLoadFailed);
//         }
//
//         /// <summary>
//         /// 异步加载资源（带进度）
//         /// </summary>
//         void Load(string path, Action<object> onLoadSuccess, Action<string> onLoadFailed, Action<float> onProgress)
//         {
//             _helper.Load(path, onLoadSuccess, onLoadFailed, onProgress);
//         }
//
//         /// <summary>
//         /// 同步加载资源
//         /// </summary>
//         T LoadSync<T>(string path) where T : UnityEngine.Object
//         {
//             return _helper.LoadSync<T>(path);
//         }
//
//         /// <summary>
//         /// 卸载资源
//         /// </summary>
//         void Unload(string path, bool unloadAllLoadedObjects = false)
//         {
//             _helper.Unload(path, unloadAllLoadedObjects);
//         }
//     }
//
//     public interface IResourceHelper
//     {
//         /// <summary>
//         /// 异步加载资源
//         /// </summary>
//         void Load(string path, Action<object> onLoadSuccess, Action<string> onLoadFailed);
//
//         /// <summary>
//         /// 异步加载资源（带进度）
//         /// </summary>
//         void Load(string path, Action<object> onLoadSuccess, Action<string> onLoadFailed, Action<float> onProgress);
//
//         /// <summary>
//         /// 同步加载资源
//         /// </summary>
//         T LoadSync<T>(string path) where T : UnityEngine.Object;
//
//         /// <summary>
//         /// 卸载资源
//         /// </summary>
//         void Unload(string path, bool unloadAllLoadedObjects = false);
//     }
//     /// <summary>
//     /// 对标之前的ResourceSystemAssetBundle
//     /// </summary>
//     public class ResourceHelperAssetBundle:IResourceHelper
//     {
//         public void Load(string path, Action<object> onLoadSuccess, Action<string> onLoadFailed)
//         {
//             throw new NotImplementedException();
//         }
//
//         public void Load(string path, Action<object> onLoadSuccess, Action<string> onLoadFailed, Action<float> onProgress)
//         {
//             throw new NotImplementedException();
//         }
//
//         public T LoadSync<T>(string path) where T : Object
//         {
//             throw new NotImplementedException();
//         }
//
//         public void Unload(string path, bool unloadAllLoadedObjects = false)
//         {
//             throw new NotImplementedException();
//         }
//     }
//     /// <summary>
//     /// 对标之前的ResourceSystemDefault，但是需要注意他使用的是只能在Editor使用的AssetDatabase.LoadAssetAtPath，使用这个的场景必须使用宏来隔离以免打包时无法正常通过编译，有这个类的目的是在编辑器可以不打包就正常加载资源
//     /// </summary>
//     public class ResourceHelperEditor:IResourceHelper
//     {
//         public void Load(string path, Action<object> onLoadSuccess, Action<string> onLoadFailed)
//         {
//             throw new NotImplementedException();
//         }
//
//         public void Load(string path, Action<object> onLoadSuccess, Action<string> onLoadFailed, Action<float> onProgress)
//         {
//             throw new NotImplementedException();
//         }
//
//         public T LoadSync<T>(string path) where T : Object
//         {
//             throw new NotImplementedException();
//         }
//
//         public void Unload(string path, bool unloadAllLoadedObjects = false)
//         {
//             throw new NotImplementedException();
//         }
//     }
// }