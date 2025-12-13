using System;
using System.Collections.Generic;

namespace GenBall.Utils.Singleton
{
    public static class SingletonManager
    {
        private static readonly Dictionary<Type, ISingleton> Singletons = new();
        public static T GetSingleton<T>()  where T : class, ISingleton,new()
        {
            if (Singletons.ContainsKey(typeof(T)))
            {
                return Singletons[typeof(T)] as T;
            }
            var singleton = new T();
            Singletons[typeof(T)] = singleton;
            return singleton;
        }
    }
}