using UnityEngine;
using Yueyn.Utils;

namespace Yueyn.Demo.UI
{
    // UI新框架分为两个层次
    // 第一个层次是对Unity的UI系统的封装，提供一个更方便的接口，用于创建和管理UI
    // 第二个层次是在第一个层次的基础上，抽象出来MVP三层架构，用于实现业务逻辑
    // 具体来说第一个层次的思路就是把原生的UI抽象出来界面和组件的概念进行管理
    public class UIManager:Singleton<UIManager>
    {
        protected override void Init()
        {
            
        }
        // 主要管理UIFromScript的生命周期，比如打开关闭，显示隐藏，层级管理，动画管理，事件管理，通用管理等
        // 为每个UIFromScript实例提供一个唯一的ID，用于管理UIFromScript的生命周期
        // 可以持有一个事件分发器
        // 可以持有一个UIFromScript的池，用于复用
        // 可以通过路径来打开UIFormScript
        // 可以通过预制体路径、ID关闭UIFormScript
    }
    public sealed class UIFormScript:MonoBehaviour
    {
        // 处理生命周期事件，参考现在已有的UIFormScript
        // 进行一些针对页面的通用管理比如渐隐渐显，层级
        // 管理组件的生命周期
    }

    public class UIComponent : MonoBehaviour
    {
        // 具有生命周期
        // 实际实现功能
        // 通过public方法和protected virtual配合实现子类可以重写逻辑但是无法修改生命周期管理
        public void DoStart(){}
        protected virtual void DoBusinessStart(){}
    }

    public class UIBusinessFormBase:UIComponent
    {  
        // UI页面的基类，在这里实现MVP架构里面的V
        // 在这里管理UIBusinessFormBase<TViewData>的生命周期，例如下面的DoStart
    }

    public class UIBusinessFormBase<TViewData>:UIBusinessFormBase where TViewData:new()
    {
        // 通过TViewData来从架构上强制要求必须MVP
        public TViewData viewData;

        public void SetViewData(TViewData data)
        {
            viewData = data;
            RefreshView();
        }
        protected virtual void RefreshView(){}
    }
    
    // 上面是表现层，下面是逻辑层
    // 表现层对逻辑层完全无感，不能对逻辑层有任何引用依赖
    
    public class BusinessLogicManager
    {
        // 管理BusinessLogic的一个容器，用于管理BusinessLogic的生命周期，提供创建和销毁BusinessLogic的接口
    }
    public class BusinessLogicBase
    {
        // 业务逻辑的基类，其实现类不一定是BusinessFormLogic或者BusinessPartLogic，可以是任何一个具体的业务逻辑类
        // 可能是一个常驻的业务逻辑，不一定和页面绑定，也可能是一个页面上的业务逻辑，和页面绑定
    }

    public class BusinessPartLogicContainer : BusinessLogicBase
    {
        // BusinessPartLogic的管理者，管理BusinessPartLogic的生命周期，提供创建和销毁BusinessPartLogic的接口
    }

    public class BusinessFormLogic : BusinessPartLogicContainer
    {
        public virtual string PrefabPath{get;}
        // 页面业务逻辑的基类，同时承担了管理页面上所有直接子页面业务组件的生命周期的责任
        // 和一个页面相绑定，注意在页面打开之后也可以动态添加子页面，此时需要对其生命周期进行时序补偿
        // 当BusinessFormLogic被销毁时，其管理的所有子页面业务逻辑也被销毁，其绑定的页面也被关闭
        // 当BusinessFormLogic绑定的页面被关闭时，自己会自动销毁（通过事件监听而非直接被UIManager体系所引用）
        // 所以需要为每个实现的页面提供一个唯一的ID，用于管理页面的生命周期，以及注册开闭事件
        // 当BusinessFormLogic被创建时，会根据路径打开绑定的页面
    }

    public class BusinessPartLogic : BusinessPartLogicContainer
    {
        // 子页面业务逻辑的基类，同时承担了管理页面上所有直接子页面业务组件的生命周期的责任
        // 和一个子页面绑定，注意在子页面打开之后也可以动态添加子页面，此时需要对其生命周期进行时序补偿
    }
    
    // 另外实现一个组件用来在页面打开之后动态添加子页面，继承UIComponent
    // 其余常用组件后续要用的时候再实现
}