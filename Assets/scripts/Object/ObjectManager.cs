
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
using System.Reflection;
using System;
using System.Linq;

namespace ChuMeng
{
	public class ObjectManager : MonoBehaviour
	{
		Dictionary<int, GameObject> fakeObjects = new Dictionary<int,GameObject> ();
		public static ObjectManager objectManager;
		public KBEngine.KBPlayer myPlayer;
		/*
		 * Player And Player Related GameObject
		 */ 
		public Dictionary<int, KBEngine.KBPlayer> Actors = new Dictionary<int, KBEngine.KBPlayer> ();
		List<KBEngine.KBNetworkView> photonViewList = new List<KBEngine.KBNetworkView> ();
		/*
		 * Monster Killed By Player
		 */ 
		//TODO: 使用MyEventSystem 来发送事件
		public VoidDelegate killEvent;

		/// <summary>
		/// 获得玩家实体对象
		/// </summary>
		public KBEngine.KBNetworkView GetPhotonView (int viewID)
		{
			//KBEngine.KBNetworkView result = null;
			foreach (KBEngine.KBNetworkView view in photonViewList) {
				if (view.GetServerID () == viewID && view.IsPlayer) {
					return view;
				}
			}
			return null;
		}

		public GameObject GetFakeObj (int localId)
		{
			GameObject g;
			if (fakeObjects.TryGetValue (localId, out g)) {
				return g;
			}
			return null;
		}

		//对象不一定是服务器对象因此通过localId来区分
		public GameObject GetLocalPlayer (int localId)
		{
			foreach (KBEngine.KBNetworkView p in photonViewList) {
				if (p.GetLocalId () == localId) {
					return p.gameObject;
				}
			}
			return null;
		}
		public int GetMyProp(CharAttribute.CharAttributeEnum prop) {
			if (myPlayer != null) {
				var vi = GetPhotonView (myPlayer.ID);
				if(vi != null) {
					return vi.GetComponent<CharacterInfo>().GetProp(prop);
				}
			}
			return 0;
		}
		public CharacterInfo GetMyData() {
			if (myPlayer != null) {
				var vi = GetPhotonView (myPlayer.ID);
				if(vi != null) {
					return vi.GetComponent<CharacterInfo>();
				}
			}
			return null;
		}
        public NpcAttribute GetMyAttr(){
            var myplayer = GetMyPlayer();
            if(myplayer != null){
                return myplayer.GetComponent<NpcAttribute>();
            }
            return null;
        }
		public GameObject GetMyPlayer ()
		{
			if (myPlayer != null) {
				var vi = GetPhotonView (myPlayer.ID);
				if (vi != null) {
					return vi.gameObject;
				}
			}
			return null;
		}

		//获得玩家自身或者其它玩家的属性数据
		public GameObject GetPlayer (int playerId)
		{
			var view = GetPhotonView (playerId);
			if (view != null) {
				return view.gameObject;
			}
			return null;
		}

		//登录的时候 从SelectChar中获取等级数据 
		//平时从 CharacterInfo 数据源获取数据
		//因为CharacterInfo 数据的初始化是在CreateMyPlayer 之后的
		//CharacterInfo SetProps Level 这里的值也要修改
		int GetMyLevel ()
		{
			return SaveGame.saveGame.selectChar.Level;
		}

		/*
		 * User Data:
		 * 		1 SaveGame  static Data Not Changed in Game
		 * 		2 CharacterData   Dynamic Data Changed in Game
		 * 		3 SaveGame InitPosition use once 
		 */ 

		/*
		 * SaveGame selectChar has My PlayerID
		 * But Here just Return -1 As My Id
		 */ 
		public int GetMyServerID ()
		{
			if (myPlayer != null) {
				return myPlayer.ID;
			}
			return -1;
		}

		/*
		 * Local Allocated NpcID
		 */ 
		public int GetMyLocalId ()
		{
			if (myPlayer != null) {
				//Debug.Log("MyLocal Id Get Error "+);
				//Log.Sys ("Local PlayerID " + myPlayer.ID);
				var view = GetPhotonView (myPlayer.ID);
				if(view != null) {
					var lid = view.GetLocalId ();
					return lid;
				}
			}

			//Debug.LogError("Not Found MyPlayer "+myPlayer);
			return -1;
		}

		/// <summary>
		/// 返回玩家上一次离开主城的初始位置 存储在数据池中
		/// </summary>
		/// <returns>The my init position.</returns>
		private Vector3 GetMyInitPos ()
		{
			var x = SaveGame.saveGame.bindSession.X;
			//var y = SaveGame.saveGame.bindSession.Y;
			var z = SaveGame.saveGame.bindSession.Z;

			var coord = Util.GridToCoord (x, z);
			//Get Floor Y Offset By RayCast
			//Current Scene Height

			//var AStar = GameObject.Find ("AStar").GetComponent<AstarPath> ();
			var AStar = AstarPath.active;
			//Scene Height Data 
			var gridGraph = AStar.graphs [0] as Pathfinding.GridGraph;
			var gridIndex = (int)(z) * gridGraph.width + (int)(x);
			Debug.Log ("ObjectManager::GetMyInitPos GridIndex" + gridIndex);
			var n = gridGraph.nodes [gridIndex];

			var hei = (Vector3)(n.position);
			var ret = new Vector3 (coord.x, hei.y + 0.3f, coord.y);
			Debug.Log ("Pos " + ret);
			return ret;
		}

		public float GetSceneHeight (int x, int z)
		{
			var AStar = GameObject.Find ("AStar").GetComponent<AstarPath> ();
			//Scene Height Data 
			var gridGraph = AStar.graphs [0] as Pathfinding.GridGraph;
			var gridIndex = (int)(z) * gridGraph.width + (int)(x);
			Debug.Log ("ObjectManager::GetMyInitPos GridIndex" + gridIndex);
			var n = gridGraph.nodes [gridIndex];
			
			var hei = (Vector3)(n.position);
			return hei.y + 0.1f;
		}

		private Vector3 GetMyInitRot ()
		{
			var dir = SaveGame.saveGame.bindSession.Direction;
			return Quaternion.Euler (new Vector3 (0, dir, 0)) * Vector3.forward;
		}

		public string GetMyName ()
		{
			return SaveGame.saveGame.selectChar.Name;
		}




		public int GetMyJob ()
		{
			return (int)SaveGame.saveGame.selectChar.Job;
		}

		void Awake ()
		{
			objectManager = this;
			DontDestroyOnLoad (this.gameObject);
		}

		//增加一个玩家实体对象
		private void AddObject (long unitId, KBEngine.KBNetworkView view)
		{
			photonViewList.Add (view);
		}

		/*
		 * PlayerId MySelf self is -1
		 * TODO:如果WorldManager正在进入新的场景，则缓存当前的服务器推送的Player,等待彻底进入场景再初始化Player
		 * TODO:CScene 进入场景之后解开缓存的Player数据
		 */
		private void AddPlayer (int unitId, KBEngine.KBPlayer player)
		{
			if (WorldManager.worldManager.station == WorldManager.WorldStation.Enter) {
				Actors.Add (unitId, player);
			} else {
				throw new SystemException ("正在切换场景，增加新的Player失败");
			}
		}




		public void DestroyByLocalId (int localId)
		{
			var keys = photonViewList.Where (f => true).ToArray ();
			foreach (KBEngine.KBNetworkView v in keys) {
				if (v.GetLocalId () == localId) {
					photonViewList.Remove (v);
					DestroyFakeObj(v.GetLocalId ());
					GameObject.Destroy (v.gameObject);
					break;
				}
			}
		}

		//删除Player和PhotonView
		public void DestroyPlayer (int playerID)
		{
			///<summary>
			/// 删除自己玩家的时候需要保存玩家的位置和角度
			/// </summary>
			if (myPlayer != null && myPlayer.ID == playerID) {
				myPlayer = null;
			}

			//摧毁某个玩家所有的PhotonView对象  Destroy Fake object Fist Or Send Event ?
			var keys = photonViewList.Where (f => true).ToArray ();
			foreach (KBEngine.KBNetworkView v in keys) {
				if (v.GetServerID () == playerID) {
					photonViewList.Remove (v);
					DestroyFakeObj (v.GetLocalId ());
					GameObject.Destroy (v.gameObject);
					//break;

				}
			}

			if (Actors.ContainsKey (playerID)) {
				Actors.Remove (playerID);
			} else {
				//Debug.LogError ("ObjectManager::clearPlayer No Such Player " + playerID);
			}

		}
		/// <summary>
		/// 摧毁自己玩家对象和单人副本怪物对象
		/// </summary>
		public void DestroyMySelf ()
		{
			MyEventSystem.myEventSystem.PushEvent (MyEvent.EventType.PlayerLeaveWorld);
			//删除我自己玩家
            if(myPlayer !=null) {
			    DestroyPlayer (myPlayer.ID);
            }

		}


		/*
		 * 显示私聊人物信息
		 */ 
		public void ShowCharInfo (GCLoadMChatShowInfo info)
		{
		}

		//加载对应模型的基础职业骨架 FakeObject
		public GameObject NewFakeObject (int localId)
		{
			//每次显示都要初始化一下FakeObj的装备信息
			if (fakeObjects.ContainsKey (localId)) {
				//fakeObjects [localId].GetComponent<NpcEquipment> ().InitDefaultEquip();
				fakeObjects [localId].GetComponent<NpcEquipment> ().InitFakeEquip ();
				return fakeObjects [localId];
			}

			var player = GetLocalPlayer (localId);
			var job = player.GetComponent<NpcAttribute> ().ObjUnitData.job;
			Log.Sys ("DialogPlayer is " + job.ToString ());
			//var fakeObject = Instantiate (Resources.Load<GameObject> ("DialogPlayer/" + job.ToString ())) as GameObject;
			var fakeObject = SelectChar.ConstructChar (job);
			fakeObject.name = fakeObject.name + "_fake";


			fakeObject.SetActive (false);
			fakeObjects [localId] = fakeObject;
			fakeObject.GetComponent<NpcEquipment> ().SetFakeObj ();
			fakeObject.GetComponent<NpcEquipment> ().SetLocalId (localId);
			fakeObject.GetComponent<NpcEquipment> ().InitFakeEquip ();

			Util.SetLayer (fakeObject, GameLayer.PlayerCamera);
			return fakeObject;
		}

		//当玩家对象被删除的时候,删除对应的玩家的FakeObj
		public void DestroyFakeObj (int localId)
		{
			var fake = GetFakeObj (localId);
			if (fake != null) {
				fakeObjects.Remove (localId);
				GameObject.Destroy (fake);
			}
		}

		/// <summary> 
		/// 副本内 :: 我方玩家构建流程 
		/// 
		/// 角色的初始化位置不同
		/// </summary>
		public GameObject CreateMyPlayerInCopy ()
		{
			var player = CreateMyPlayerInternal ();
			SetStartPointPosition (player);
			return player;
		}

		GameObject CreateMyPlayerInternal() {
			var kbplayer = new KBEngine.KBPlayer ();
            kbplayer.ID = (int)SaveGame.saveGame.selectChar.PlayerId;
			if (myPlayer != null) {
				throw new System.Exception ("myPlayer not null");
			}
			
			myPlayer = kbplayer;
			
			var job = (ChuMeng.Job)GetMyJob ();
			var udata = Util.GetUnitData (true, (int)job, GetMyLevel ());
			var player = Instantiate (Resources.Load<GameObject> (udata.ModelName)) as GameObject;
			
			NetDebug.netDebug.AddConsole ("Init Player tag layer transform");
			NGUITools.AddMissingComponent<NpcAttribute> (player);
			NGUITools.AddMissingComponent<PlayerAIController> (player);
			player.tag = "Player";
			player.layer = (int)GameLayer.Npc;
			player.transform.parent = transform;
			

			//设置自己玩家的View属性
			var view = player.GetComponent<KBEngine.KBNetworkView> ();
			NetDebug.netDebug.AddConsole ("SelectCharID " + SaveGame.saveGame.selectChar.PlayerId);
            view.SetID (new KBEngine.KBViewID ((int)SaveGame.saveGame.selectChar.PlayerId, kbplayer));
			
			NetDebug.netDebug.AddConsole ("Set UnitData of Certain Job " + udata);
			player.GetComponent<NpcAttribute> ().SetObjUnitData (udata);
			player.GetComponent<NpcEquipment> ().InitDefaultEquip ();
			player.GetComponent<NpcEquipment> ().InitPlayerEquipmentFromBackPack ();
			
			player.name = "player_me";
			
			ObjectManager.objectManager.AddObject (SaveGame.saveGame.selectChar.PlayerId, view);
			
            var light = GameObject.Instantiate(Resources.Load<GameObject>("light/playerLight")) as GameObject;
            light.transform.parent = player.transform;
            light.transform.localPosition = Vector3.zero;

			NetDebug.netDebug.AddConsole("LevelInit::Awake Initial kbplayer Initial KbNetowrkView " + kbplayer + " " + view);
			return player;
		}

		void SetStartPointPosition(GameObject player) {
			var startPoint = GameObject.Find ("PlayerStart");
			player.transform.position = startPoint.transform.position;
			player.transform.forward = startPoint.transform.forward;
		}

		void SetCityStartPos(GameObject player) {
            SetStartPointPosition(player);
		}
		///<summary>
		/// 主城内
		/// 
		/// 我方玩家构建流程 野外 
		/// 	可能从副本退出    从BackPack 初始化装备
		/// 	也可能是刚登陆    需要等待BackPack 初始化结束 通知 穿戴装备
		/// 从副本退出则初始位置在刚才进入副本的位置
		/// 初次登陆的初始位置在登陆的属性中
		/// 
		/// 角色的初始化位置不同
		///</summary> 
		public GameObject CreateMyPlayerInCity ()
		{
			NetDebug.netDebug.AddConsole ("LoginMyPlayer");
			var player = CreateMyPlayerInternal ();
			SetCityStartPos (player);

			NetDebug.netDebug.AddConsole("ObjectManager Init Player Over");
			return player;
		}

		class MonsterInit
		{
			public UnitData unitData;
			public SpawnTrigger spawn;
            public GameObject spawnObj;

			public MonsterInit (UnitData ud, SpawnTrigger sp)
			{
				unitData = ud;
				spawn = sp;
			}

            public MonsterInit (UnitData ud, GameObject obj)
            {
                unitData = ud;
                spawnObj = obj;
            }
		}
		List<MonsterInit> cacheMonster = new List<MonsterInit> ();
        List<MonsterInit> cacheNpc = new List<MonsterInit>();

		public void InitCache ()
		{
			InitCachePlayer ();
			InitCacheMonster ();
            InitCacheNpc();
		}

		void InitCachePlayer ()
		{
            /*
			foreach (ViewPlayer vp in cacheInitPlayer) {
				CreatePlayer (vp);
			}
            */
		}

		void InitCacheMonster ()
		{
			foreach (MonsterInit m in cacheMonster) {
				CreateMonster (m.unitData, m.spawn);
			}
			cacheMonster.Clear ();
		}
        void InitCacheNpc() {
            foreach (MonsterInit m in cacheNpc) {
               CreateNpc(m.unitData, m.spawnObj);
            }
           cacheNpc .Clear ();
        }

        /// <summary>
        /// 创建其它玩家
        /// 玩家所在场景
        /// </summary>
        /// <param name="ainfo">Ainfo.</param>
        public void CreateOtherPlayer(AvatarInfo ainfo) {
            if (WorldManager.worldManager.station == WorldManager.WorldStation.Enter) {
                Log.Sys("CreateOtherPlayer: "+ainfo);
                var oldPlayer = GetPlayer(ainfo.Id);
                if(oldPlayer != null) {
                    Debug.LogError("PlayerExists: "+ainfo);
                    return;
                }

                if(myPlayer != null && myPlayer.ID == ainfo.Id) {
                    Debug.LogError("CreateMeAgain");
                    return;
                }

                var kbplayer = new KBEngine.KBPlayer ();
                kbplayer.ID = ainfo.Id;

                var udata = Util.GetUnitData (true, (int)1, 1);
                var player = GameObject.Instantiate (Resources.Load<GameObject> (udata.ModelName)) as GameObject;

                var attr = NGUITools.AddMissingComponent<NpcAttribute> (player);
                //状态机类似 之后可能需要修改为其它玩家状态机
                NGUITools.AddMissingComponent<OtherPlayerAI> (player);

                player.tag = "Player";
                player.layer = (int)GameLayer.Npc;

                NGUITools.AddMissingComponent<SkillInfoComponent> (player);
                player.GetComponent<NpcAttribute> ().SetObjUnitData (udata);
                player.GetComponent<NpcEquipment> ().InitDefaultEquip ();

                player.name = "player_" + ainfo.Id;
                player.transform.parent = gameObject.transform;

                var netview = player.GetComponent<KBEngine.KBNetworkView> ();
                netview.SetID (new KBEngine.KBViewID (kbplayer.ID, kbplayer));
                

                AddPlayer (kbplayer.ID, kbplayer);
                AddObject (netview.GetServerID (), netview);
                attr.Init();
                var sync = player.GetComponent<ChuMeng.PlayerSync> ();
                sync.SetPositionAndDir(ainfo);
                sync.SetLevel(ainfo);
            }else {
                 
            }
        }

        public void RefreshMyServerId(int id) {
            if(myPlayer != null) {
                myPlayer.ID = id;
            }
        }


        public void CreateNpc(UnitData unitData, GameObject spawn) {
            if (WorldManager.worldManager.station == WorldManager.WorldStation.Enter) {
                var Resource = Resources.Load<GameObject> (unitData.ModelName);
                GameObject g = Instantiate (Resource) as GameObject;
                if(g.GetComponent<CharacterController>() == null) {
                    var c = g.AddComponent<CharacterController>();
                    c.center = new Vector3(0, 0.7f, 0);
                    c.height = 1.6f;
                }

                NpcAttribute npc = NGUITools.AddMissingComponent<NpcAttribute> (g);

                var type = Type.GetType ("ChuMeng." + unitData.AITemplate);
                var t = typeof(NGUITools);
                var m = t.GetMethod ("AddMissingComponent");
                Log.AI ("Monster Create Certain AI  " + unitData.AITemplate + " " + type);
                var geMethod = m.MakeGenericMethod (type);
                geMethod.Invoke (null, new object[]{g});// as AIBase;

                g.transform.parent = transform;
                g.tag = GameTag.Npc; //Player Or Npc 
                g.layer = (int)GameLayer.IgnoreCollision;

                //Register Unique Id To Npc 
                var netView = g.GetComponent<KBEngine.KBNetworkView> ();
                netView.SetID (new KBEngine.KBViewID (myPlayer.ID, myPlayer));
                netView.IsPlayer = false;

                npc.SetObjUnitData (unitData);
                AddObject (netView.GetServerID (), netView);

                npc.transform.position = spawn.transform.position;
                var rotY = spawn.transform.localRotation.eulerAngles.y;
                npc.transform.localRotation = Quaternion.Euler(new Vector3(0, rotY, 0)); 

                NpcManager.Instance.RegNpc(unitData.name, g);
            }else {
                cacheNpc.Add (new MonsterInit (unitData,spawn ));
            }
        }

        public void CreateChest(UnitData unitData, SpawnChest spawn) {
            Log.Sys ("Create Chest Unit " + unitData.name);
            var Resource = Resources.Load<GameObject> (unitData.ModelName);
            GameObject g = Instantiate (Resource) as GameObject;
            NpcAttribute npc = NGUITools.AddMissingComponent<NpcAttribute> (g);
            npc.spawnTrigger = spawn.gameObject;

            var type = Type.GetType ("ChuMeng." + unitData.AITemplate);
            var t = typeof(NGUITools);
            var m = t.GetMethod ("AddMissingComponent");
            Log.AI ("Monster Create Certain AI  " + unitData.AITemplate + " " + type);
            var geMethod = m.MakeGenericMethod (type);
            geMethod.Invoke (null, new object[]{g});// as AIBase;

            g.transform.parent = transform;
            g.tag = GameTag.Enemy;
            g.layer = (int)GameLayer.Npc;

            var netView = g.GetComponent<KBEngine.KBNetworkView> ();
            netView.SetID (new KBEngine.KBViewID (myPlayer.ID, myPlayer));
            netView.IsPlayer = false;

            npc.SetObjUnitData (unitData);
            AddObject (netView.GetServerID (), netView);

            //不算怪物允许不去打
            npc.transform.position = spawn.transform.position;

            BattleManager.battleManager.AddEnemy (npc.gameObject);
            npc.SetDeadDelegate = BattleManager.battleManager.EnemyDead;
        }


		///<summary>
		/// 副本内怪物构建流程
		/// 单人副本通过Scene配置来产生怪物
		/// 多人副本通过服务器推送消息产生怪物
		/// </summary>
		//TODO: 多人副本怪物产生机制
		public void CreateMonster (UnitData unitData, SpawnTrigger spawn)
		{
			if (WorldManager.worldManager.station == WorldManager.WorldStation.Enter) {
                Log.Sys("UnityDataIs "+unitData.ID);
                if(unitData.Config == null) {
                    Debug.LogError("NotFoundMonster "+unitData.ID);
                    return;
                }

				Log.Sys ("Create Monster Unit " + unitData.name);
				var Resource = Resources.Load<GameObject> (unitData.ModelName);
				//本地怪兽不需要Player信息
				GameObject g = Instantiate (Resource) as GameObject;
				NpcAttribute npc = NGUITools.AddMissingComponent<NpcAttribute> (g);
                npc.spawnTrigger = spawn.gameObject;

				var type = Type.GetType ("ChuMeng." + unitData.AITemplate);
				var t = typeof(NGUITools);
				var m = t.GetMethod ("AddMissingComponent");
				Log.AI ("Monster Create Certain AI  " + unitData.AITemplate + " " + type);
				var geMethod = m.MakeGenericMethod (type);
				//var petAI = 
                geMethod.Invoke (null, new object[]{g});// as AIBase;


				g.transform.parent = transform;
				g.tag = GameTag.Enemy;
				g.layer = (int)GameLayer.Npc;
                spawn.FirstMonster = g;

				var netView = g.GetComponent<KBEngine.KBNetworkView> ();
				netView.SetID (new KBEngine.KBViewID (myPlayer.ID, myPlayer));
				netView.IsPlayer = false;

				npc.SetObjUnitData (unitData);
				AddObject (netView.GetServerID (), netView);

			
				float angle = UnityEngine.Random.Range (0, 360);
				Vector3 v = Vector3.forward;
				v = Quaternion.Euler (new Vector3 (0, angle, 0)) * v;
				float rg = UnityEngine.Random.Range (0, spawn.Radius);

				npc.transform.position = spawn.transform.position + v * rg;
                if(unitData.IsElite) {
                    npc.transform.localScale = new Vector3(2, 2, 2);
                }

                BattleManager.battleManager.AddEnemy (npc.gameObject);
				npc.SetDeadDelegate = BattleManager.battleManager.EnemyDead;
				//npc.Level = spawn.Level;
			} else {
				cacheMonster.Add (new MonsterInit (unitData, spawn));
			}
		}

		public void CreatePet (int monsterId, GameObject owner, Affix affix, Vector3 pos)
		{
			Log.Sys ("Create Pet " + monsterId + " " + owner + " " + pos);
            if(owner == null) {
                Debug.LogError("Own NotExist Pet Not Born");
                return;
            }
			var unitData = Util.GetUnitData (false, monsterId, 1);

			var Resource = Resources.Load<GameObject> (unitData.ModelName);

			GameObject g = Instantiate (Resource) as GameObject;
			NpcAttribute npc = NGUITools.AddMissingComponent<NpcAttribute> (g);
			var type = Type.GetType ("ChuMeng." + unitData.AITemplate);
			var t = typeof(NGUITools);
			var m = t.GetMethod ("AddMissingComponent");
			Log.AI ("Create Certain AI  " + unitData.AITemplate + " " + type);
			var geMethod = m.MakeGenericMethod (type);
			//var petAI = 
            geMethod.Invoke (null, new object[]{g});// as AIBase;
			//var petAI = 
            //NGUITools.AddMissingComponent<type> (g);

			g.transform.parent = transform;
			g.tag = owner.tag;
			g.layer = (int)GameLayer.Npc;


			npc.SetOwnerId (owner.GetComponent<KBEngine.KBNetworkView> ().GetLocalId ());
            npc.spawnTrigger = owner.GetComponent<NpcAttribute>().spawnTrigger;

			//不可移动Buff
			//持续时间Buff
			//无敌不可被攻击Buff
			//火焰陷阱的特点 特点组合
			g.GetComponent<BuffComponent> ().AddBuff (affix);

			var netView = NGUITools.AddMissingComponent<KBEngine.KBNetworkView> (g);
			netView.SetID (new KBEngine.KBViewID (myPlayer.ID, myPlayer));
			netView.IsPlayer = false;
            //owner.GetComponent<NpcAttribute>().AddSummon(netView.gameObject);
			
			npc.SetObjUnitData (unitData);
			AddObject (netView.GetServerID (), netView);

			npc.transform.position = pos;	

            if(unitData.IsElite) {
                npc.transform.localScale = new Vector3(2, 2, 2);
            }

            if(npc.tag == GameTag.Enemy) {
                BattleManager.battleManager.AddEnemy (npc.gameObject);
                npc.SetDeadDelegate = BattleManager.battleManager.EnemyDead;
            }
		}

        public List<GameObject> GetSummons(int localId) {
            var ret = new List<GameObject>();
            foreach(var g in photonViewList){
                if(g.GetComponent<NpcAttribute>().OwnerId == localId) {
                    ret.Add(g.gameObject);
                }
            }
            return ret;
        }
	}

}