
/*
Author: liyonghelpme
Email: 233242872@qq.com
*/

/*
Author: liyonghelpme
Email: 233242872@qq.com
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace KBEngine
{
    /*
	 * Player In Room
	 * ID Assigned By Server
	 */
    /// <summary>
    /// 服务器对象
    /// </summary>
    public class KBPlayer
    {
        public int ID = -1;
    }

    /*
	 * Allocated By Local For NetworkIdentity
	 */
    /// <summary>
    /// 服务器对象下面衍生的 本地对象 或者服务器对象
    /// 例如单人副本里面的怪物 为本地对象
    ///	多人副本里面的怪物则为 服务器对象
    /// 
    /// 我的服务器对象对应多个本地对象 玩家实体和怪物实体
    /// </summary>
    public class KBViewID
    {
        KBPlayer internalPlayer;

        public KBPlayer owner
        {
            get
            {
                return internalPlayer;
            }
        }

        public KBViewID(int id, KBPlayer player)
        {
            internalPlayer = player;
        }
    }

    public class MonoBehaviour : UnityEngine.MonoBehaviour
    {
        protected List<ChuMeng.MyEvent.EventType> regEvt = null;
        protected List<ChuMeng.MyEvent.EventType> regLocalEvt = null;
        protected List<EvtCbPair> regLocalEvtCallback = null;

        protected bool regYet = false;

        /// <summary>
        /// BloodBar 继承UIInterface
        /// UIInterface在Awake时添加事件 Enable时注册事件 Disable时取消事件
        ///  
        /// BloodBar需要在Start时动态加入Local事件
        /// </summary>
        /// <param name="force">If set to <c>true</c> force.</param>
        public void RegEvent(bool force = false)
        {
            if (regYet && !force)
            {
                return;
            }
            regYet = true;
            if (regEvt != null)
            {
                foreach (ChuMeng.MyEvent.EventType t in regEvt)
                {
                    ChuMeng.MyEventSystem.myEventSystem.RegisterEvent(t, OnEvent);
                }
            }

            if (regLocalEvt != null)
            {
                foreach (ChuMeng.MyEvent.EventType t in regLocalEvt)
                {
                    Log.Sys("Reglocalevent " + t + " view " + photonView + " myevent " + ChuMeng.MyEventSystem.myEventSystem);
                    ChuMeng.MyEventSystem.myEventSystem.RegisterLocalEvent(photonView.GetLocalId(), t, OnLocalEvent);
                }
            }

        }

        protected virtual void OnLocalEvent(ChuMeng.MyEvent evt)
        {
		
        }

        public void DropEvent()
        {
            if (!regYet)
            {
                return;
            }
            regYet = false;
            if (regEvt != null)
            {
                foreach (ChuMeng.MyEvent.EventType t in regEvt)
                {
                    ChuMeng.MyEventSystem.myEventSystem.dropListener(t, OnEvent);
                }
            }

            if (regLocalEvt != null)
            {
                foreach (ChuMeng.MyEvent.EventType t in regLocalEvt)
                {
                    ChuMeng.MyEventSystem.myEventSystem.DropLocalListener(photonView.GetLocalId(), t, OnLocalEvent);
                }
            }
            if (regLocalEvtCallback != null)
            {
                foreach (var t in regLocalEvtCallback)
                {
                    ChuMeng.MyEventSystem.myEventSystem.DropLocalListener(photonView.GetLocalId(), t.t, t.cb);
                }
            }
        }

        protected virtual void OnEvent(ChuMeng.MyEvent evt)
        {
        }

        protected virtual void OnDestroy()
        {
            DropEvent();
        }

        public KBNetworkView photonView
        {
            get
            {
                return GetComponent<KBNetworkView>();
            }
        }

        /// <summary>
        /// 注册一个全局事件 
        /// 在RegEvent之后添加事件
        /// </summary>
        /// <param name="t">T.</param>
        protected void AddEvent(ChuMeng.MyEvent.EventType t)
        {
            regEvt.Add(t);
            ChuMeng.MyEventSystem.myEventSystem.RegisterEvent(t, OnEvent);
        }

        public class EvtCbPair
        {
            public ChuMeng.MyEvent.EventType t;
            public ChuMeng.EventDel cb;
        }

        public void AddCallBackLocalEvent(ChuMeng.MyEvent.EventType t, ChuMeng.EventDel cb)
        {
            regYet = true;
            if (regLocalEvtCallback == null)
            {
                regLocalEvtCallback = new List<EvtCbPair>();
            }

            regLocalEvtCallback.Add(new EvtCbPair()
            {
                t = t,
                cb = cb,
            });
            ChuMeng.MyEventSystem.myEventSystem.RegisterLocalEvent(photonView.GetLocalId(), t, cb);
        }

        public void DropCallBackLocalEvent(ChuMeng.MyEvent.EventType t, ChuMeng.EventDel cb)
        {
            ChuMeng.MyEventSystem.myEventSystem.DropLocalListener(photonView.GetLocalId(), t, cb);
            foreach(var e in regLocalEvtCallback) {
                if(e.t == t && e.cb == cb) {
                    regLocalEvtCallback.Remove(e);
                    break;
                }
            }
        }


    }
    /*
	 * Player ---> Multiple View
	 * ViewID ---> Owner Player Owner ID
	 */
    public class KBNetworkView : MonoBehaviour
    {
        /*
		 * Client Object ID
		 */
        //Awake的初始化在 私有变量 int 赋值之后？还是之前Public 遗留的问题？
        static int LocalID = 0;

        public void SetLocalId(int lid)
        {
            localId = lid;
        }


        int localId = -1;

        public int GetLocalId()
        {
            if (localId == -1)
            {
                localId = LocalID++;
                //Log.Sys("Initial local Id "+localId);
            }
            //Log.Sys ("GetLocalId of "+gameObject.name+" "+localId);
            return localId;
        }
        /*
		 * Allocate by Master or local
		 * For Local allocate new ID
		 * 
		 * Server ID 
		 * Server Object Player
		 */
        KBViewID ID = new KBViewID(0, null);

        public int GetServerID()
        {
            if (ID.owner == null)
            {
                Debug.Log("KBNetworkView:: not net connection ");
                return -1;
            }
            return ID.owner.ID;
        }
        public void SetServerID(int id) {
            ID.owner.ID = id;
        }


        public void SetID(KBViewID id)
        {
            ID = id;
        }

        /// <summary>
        /// 是玩家还是怪物或者宠物等对象
        /// </summary>
        public bool IsPlayer = true;

        public bool IsMe
        {
            get
            {
                return localId == ChuMeng.ObjectManager.objectManager.GetMyLocalId(); 
                //return ID.owner.ID 
                //return true;
            }
        }

        //是否是本地玩家
        public bool IsMine
        {
            get
            {
                if (ID.owner == null)
                {
                    Debug.Log("KBNetworkView:: No NetworkConnect Init Player Is Mine");
                    return true;
                }
                //return ID.owner == ChuMeng.ObjectManager.objectManager.myPlayer && IsPlayer;
                return ID.owner == ChuMeng.ObjectManager.objectManager.myPlayer;
            }
        }

        void Start()
        {
            //localId = LocalID++;
            Log.Sys("Implement Local ID " + localId + " " + gameObject.name);
        }

        void Awake()
        {
            //localId = LocalID++;
        }

    }

}