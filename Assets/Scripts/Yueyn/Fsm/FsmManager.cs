using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Yueyn.Main;
using Yueyn.Utils;

namespace Yueyn.Fsm
{
    public sealed class FsmManager:MonoBehaviour,IComponent
    {
        public int Priority => 10;
        private readonly Dictionary<TypeNamePair, IFsm> _fsms=new();
        private readonly List<IFsm> _tempFsms=new();
        public void Init()
        {
            
        }

        public void OnUnregister()
        {
            
        }

        public void ComponentUpdate(float elapsedSeconds, float realElapseSeconds)
        {
            _tempFsms.Clear();
            if(_fsms.Count<=0) return;
            _tempFsms.AddRange(_fsms.Values);
            foreach (var fsm in _tempFsms)
            {
                if(fsm.IsDestroyed) continue;
                fsm.Update(elapsedSeconds, realElapseSeconds);
            }
        }

        public void ComponentFixedUpdate(float fixedDeltaTime)
        {
            _tempFsms.Clear();
            if(_fsms.Count<=0) return;
            _tempFsms.AddRange(_fsms.Values);
            foreach (var fsm in _tempFsms)
            {
                if(fsm.IsDestroyed) continue;
                fsm.FixedUpdate(fixedDeltaTime);
            }
        }

        public void Shutdown()
        {
            foreach (var fsm in _fsms.Values)
            {
                fsm.Shutdown();
            }
            _fsms.Clear();
            _tempFsms.Clear();
        }

        #region GetFsm
        public Fsm<T> GetFsm<T>() where T : class =>(Fsm<T>)InternalGetFsm(new TypeNamePair(typeof(T)));
        public IFsm GetFsm(Type ownerType)
        {
            if (ownerType == null)
            {
                throw new Exception("OwnerType is null");
            }
            return InternalGetFsm(new TypeNamePair(ownerType));
        }
        public Fsm<T> GetFsm<T>(string name) where T : class =>(Fsm<T>)InternalGetFsm(new TypeNamePair(typeof(T), name));   
        public IFsm GetFsm(Type ownerType, string name)
        {
            if (ownerType == null)
            {
                throw new Exception("ownerType is null");
            }
            return InternalGetFsm(new TypeNamePair(ownerType, name));
        }
        public IFsm[] GetAllFsms()=>_fsms.Values.ToArray();
        public void GetAllFsms(List<IFsm> results)
        {
            if (results == null)
            {
                throw new Exception("results is invalid");
            }
            results.Clear();
            results.AddRange(_fsms.Values);
        }
        #endregion
        #region HasFsm
        public bool HasFsm<T>() where T : class => InternalHasFsm(new TypeNamePair(typeof(T)));
        public bool HasFsm(Type ownerType)
        {
            if (ownerType == null)
            {
                throw new Exception("OwnerType is null");
            }
            return InternalHasFsm(new TypeNamePair(ownerType));
        }
        public bool HasFsm<T>(string name) where T : class => InternalHasFsm(new TypeNamePair(typeof(T), name));
        
        public bool HaFsm(Type ownerType, string name)
        {
            if (ownerType == null)
            {
                throw new Exception("OwnerType is null");
            }
            return InternalHasFsm(new TypeNamePair(ownerType, name));
        }
        #endregion
        #region CreateFsm
        public Fsm<T> CreateFsm<T>(T owner,params FsmState<T>[] states) where T : class
            =>CreateFsm(string.Empty, owner, states.ToList());
        public Fsm<T> CreateFsm<T>(string name, T owner, params FsmState<T>[] states) where T : class
            => CreateFsm(name, owner, states.ToList());
        public Fsm<T> CreateFsm<T>(T owner,List<FsmState<T>> states) where T : class
        =>CreateFsm(string.Empty,owner,states);
        public Fsm<T> CreateFsm<T>(string name, T owner, List<FsmState<T>> states) where T : class
        {
            var typeNamePair=new TypeNamePair(typeof(T), name);
            if (HasFsm<T>(name))
            {
                throw new Exception($"Fsm {name} already exists");
            }
            var fsm=Fsm<T>.Create(name,owner,states);
            _fsms.Add(typeNamePair,fsm);
            return fsm;
        }

        #endregion
        #region DestroyFsm

        public bool DestroyFsm<T>() where T : class => InternalDestroyFsm(new TypeNamePair(typeof(T)));
        public bool DestroyFsm(Type ownerType)
        {
            if (ownerType == null)
            {
                throw new Exception("OwnerType is null");
            }

            return InternalDestroyFsm(new TypeNamePair(ownerType));
        }
        public bool DestroyFsm<T>(string name) where T : class =>InternalDestroyFsm(new TypeNamePair(typeof(T), name));
        public bool DestroyFsm(Type ownType, string name)
        {
            if (ownType == null)
            {
                throw new Exception("OwnType is null");
            }
            return InternalDestroyFsm(new TypeNamePair(ownType, name));
        }
        public bool DestroyFsm<T>(Fsm<T> fsm) where T : class
        {
            if (fsm == null)
            {
                throw new Exception("fsm is null");
            }
            return InternalDestroyFsm(new TypeNamePair(typeof(T), fsm.Name));
        }
        public bool DestroyFsm(IFsm fsm)
        {
            if(fsm == null) throw new Exception("fsm is null");
            return InternalDestroyFsm(new TypeNamePair(fsm.OwnerType, fsm.Name));
        }
        
        private bool InternalDestroyFsm(TypeNamePair typeNamePair)
        {
            if (!_fsms.TryGetValue(typeNamePair, out var fsm)) return false;
            fsm.Shutdown();
            return _fsms.Remove(typeNamePair);
        }

        #endregion
                
        private bool InternalHasFsm(TypeNamePair typeNamePair)=>_fsms.ContainsKey(typeNamePair);
        private IFsm InternalGetFsm(TypeNamePair typeNamePair) => _fsms.GetValueOrDefault(typeNamePair);

    }
}