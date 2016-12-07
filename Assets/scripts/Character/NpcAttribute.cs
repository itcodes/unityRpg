
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

namespace ChuMeng
{
    public enum CharacterState
    {
        Idle,
        Running,
        Attacking,
        Around,
        Stunned,
        Dead,
        Birth,
        CastSkill,
        Story,
        Patrol,

        Flee,
    };


    /// <summary>
    /// 其它组件访问对象的数据 都通过 NpcAttribute 进行
    /// </summary>
    public class NpcAttribute : MonoBehaviour
    {
        public CharacterState _characterState = CharacterState.Idle;
        public int OwnerId = -1;

        /// <summary>
        /// Monster SpawnObject 
        /// </summary>
        public GameObject spawnTrigger;

        public Vector3 OriginPos
        {
            get;
            private set;
        }

        public void SetOwnerId(int ownerId)
        {
            OwnerId = ownerId;
        }

        public GameObject GetOwner()
        {
            return ObjectManager.objectManager.GetLocalPlayer(OwnerId);
        }

        public float FastRotateSpeed
        {
            get
            {
                return 10;
            }
        }

        public float WalkSpeed
        {
            get
            {
                return ObjUnitData.MoveSpeed;
            }
        }

        //[NpcAttributeAtt()]
        public float ApproachDistance
        {
            get
            {
                if (_ObjUnitData != null)
                {
                    return _ObjUnitData.ApproachDistance;
                }
                Debug.LogError("not init ObjData " + gameObject);
                return 0;
            }
        }

        /// <summary>
        /// 远程网络直接设置控制HP 
        /// </summary>
        /// <value>The H.</value>
        public int HP
        {
            get
            {
                return GetComponent<CharacterInfo>().GetProp(CharAttribute.CharAttributeEnum.HP);
            }
            set
            {
                GetComponent<CharacterInfo>().SetProp(CharAttribute.CharAttributeEnum.HP, value);
            }
        }

        public int TeamColor = 0;

        public void SetTeamColorNet(int teamColor) {
            TeamColor = teamColor;
            MyEventSystem.PushLocalEventStatic(GetLocalId(), MyEvent.EventType.TeamColor);
        }

        public void SetHPNet(int hp)
        {
            GetComponent<CharacterInfo>().SetProp(CharAttribute.CharAttributeEnum.HP, hp);
            var evt1 = new MyEvent(MyEvent.EventType.UnitHP);
            evt1.localID = GetLocalId();
            MyEventSystem.myEventSystem.PushLocalEvent(evt1.localID, evt1);
        }

        public int HP_Max
        {
            get
            {
                return GetComponent<CharacterInfo>().GetProp(CharAttribute.CharAttributeEnum.HP_MAX);
            }
            set
            {
                GetComponent<CharacterInfo>().SetProp(CharAttribute.CharAttributeEnum.HP_MAX, value);
            }
        }

        public int MP
        {
            get
            {
                return GetComponent<CharacterInfo>().GetProp(CharAttribute.CharAttributeEnum.MP);
            }
            set
            {
                GetComponent<CharacterInfo>().SetProp(CharAttribute.CharAttributeEnum.MP, value);
            }
        }

        public int MP_Max
        {
            get
            {
                return GetComponent<CharacterInfo>().GetProp(CharAttribute.CharAttributeEnum.MP_MAX);
            }
            set
            {
                GetComponent<CharacterInfo>().SetProp(CharAttribute.CharAttributeEnum.MP_MAX, value);
            }
        }


        //TODO::调整人物属性采用当前游戏的数据设定
    

        public int Exp
        {
            get
            {
                return GetComponent<CharacterInfo>().GetProp(CharAttribute.CharAttributeEnum.EXP);
            }
        
            private set
            {
                GetComponent<CharacterInfo>().SetProp(CharAttribute.CharAttributeEnum.EXP, value);
            }

        }

        //TODO: 技能点应该属于Skill系统
        //public int AttributePoint = 0;


        bool _isDead = false;

        public delegate void SetDead(GameObject g);

        public SetDead SetDeadDelegate;

        //玩家升级后设置等级
        //int _Level = 1;
        public int Level
        {
            get
            {
                return GetComponent<CharacterInfo>().GetProp(CharAttribute.CharAttributeEnum.LEVEL);
            }
            set
            {
                GetComponent<CharacterInfo>().SetProp(CharAttribute.CharAttributeEnum.LEVEL, value);
                SetLevel();
            }
        }

        public bool IsDead
        {
            get
            {
                return _isDead;
            }
            set
            {
                if (_isDead == value)
                {
                    return;
                }
                _isDead = value;
                if (SetDeadDelegate != null)
                {
                    SetDeadDelegate(gameObject);
                }

                if (ObjectManager.objectManager != null)
                {
                    if (ObjectManager.objectManager.killEvent != null)
                    {
                        ObjectManager.objectManager.killEvent(gameObject);
                    }
                }

                //DropTreasure();
            }
        }

        int _Damage
        {
            get
            {
                return _ObjUnitData.Damage;
            }
        }

        public int Damage
        {
            get
            {
                return GetAllDamage();
            }
        }

        //int _PoisonDefense = 0;
        public int PoisonDefense
        {
            get
            {
                return GetWaterDefense();
            }
        }

        int _Armor
        {
            get
            {
                return _ObjUnitData.Armor;
            }
        }

        public int Armor
        {
            get
            {
                return GetAllArmor();
            }
        }

        UnitData _ObjUnitData;

        public UnitData ObjUnitData
        {
            set
            {
                _ObjUnitData = value;
                if (_ObjUnitData.TextureReplace.Length > 0)
                {
                    SetTexture(_ObjUnitData.TextureReplace);
                }
            }
            get
            {
                return _ObjUnitData;
            }
        }

        NpcEquipment npcEquipment;

        //TODO:攻击距离由当前激活的技能决定 而不是 由 角色属性决定
        //TODO:简化 人物的攻击距离 分成远程和进展两种攻击距离即可，进展不能变成远程，远程也不能变成近战, 攻击距离主要对近战有效
        //TODO: 火炬之光里面 攻击距离是由武器决定的
        public float AttackRange
        {
            get
            {
                if (_ObjUnitData != null)
                {
                    return _ObjUnitData.AttackRange;
                }
                Debug.LogError("not init ObjData " + gameObject);
                return 0;
            }
        }

        public float ReachRange
        {
            get
            {
                return 2;
            }
        }

        public float PatrolRange
        {
            get
            {
                return 5;
            }
        }



        //根据配置文件初始化属性
        //TODO: 初始化其它玩家的属性 PlayerOther  PlayerSelf Monster Boss
        void InitData()
        {
            Log.Important("Initial Object HP " + gameObject.name);
            var characterInfo = GetComponent<CharacterInfo>();
            if (ObjUnitData != null && characterInfo != null)
            {
                var view = GetComponent<KBEngine.KBNetworkView>(); 


                Log.Important("Player View State " + gameObject.name + " " + view.IsPlayer + " " + view.IsMine);
                HP_Max = _ObjUnitData.HP;
                HP = HP_Max;
                MP_Max = _ObjUnitData.MP;
                MP = MP_Max;
                Log.Important("Init Obj Data  " + gameObject.name + " " + HP + " " + _ObjUnitData.HP);
                ChangeHP(0);
                ChangeMP(0);
            }
        }

        public KBEngine.KBNetworkView GetNetView() {
            return GetComponent<KBEngine.KBNetworkView>();
        }

        //玩家升级后设置等级 调整对应UnitData
        //TODO: 单人副本中调整属性  多人副本中网络同步属性 城市中网络同步属性  属性调整都是通过 CharacterInfo 来做的
        void SetLevel()
        {
            _ObjUnitData = Util.GetUnitData(_ObjUnitData.GetIsPlayer(), _ObjUnitData.ID, Level);
            charInfo.SetProp(CharAttribute.CharAttributeEnum.EXP_MAX, (int)_ObjUnitData.MaxExp);
            charInfo.SetProp(CharAttribute.CharAttributeEnum.HP, _ObjUnitData.HP);
            charInfo.SetProp(CharAttribute.CharAttributeEnum.HP_MAX, _ObjUnitData.HP);
            charInfo.SetProp(CharAttribute.CharAttributeEnum.MP, _ObjUnitData.MP);
            charInfo.SetProp(CharAttribute.CharAttributeEnum.MP_MAX, _ObjUnitData.MP);
            ChangeHP(0);
        }

        CharacterInfo charInfo;

        void Awake()
        {
        }


        public void SetObjUnitData(UnitData ud)
        {
            ObjUnitData = ud;
            InitData();
        }

        /*
         * Player Equipment PoisonDefense
         * Monster Define in UnitData
         */
        int GetWaterDefense()
        {
            int d = 0;
            if (npcEquipment != null)
            {
                d += npcEquipment.GetPoisonDefense();
            }
            return d;
        }

        /*
         * BaseWeapon Damage
         * Fire Element Damage  Ice Element Electric
         */
        int GetAllDamage()
        {
            int d = _Damage;
            if (npcEquipment != null)
            {
                d += npcEquipment.GetDamage();
            }
            Log.Sys("Damage is what  " + d + " g " + gameObject);
            return d;
        }

        public float GetSpeedCoff()
        {
            return GetComponent<BuffComponent>().GetSpeedCoff();
        }

        public int GetCriticalRate()
        {
            return ObjUnitData.CriticalHit + GetComponent<BuffComponent>().GetCriticalRate();
        }

        int GetAllArmor()
        {
            int a = _Armor;
            if (npcEquipment != null)
            {
                a += npcEquipment.GetArmor();
            }
            a += GetComponent<BuffComponent>().GetArmor();
            return a;
        }

        public void Init()
        {
            npcEquipment = GetComponent<NpcEquipment>();
            charInfo = GetComponent<CharacterInfo>();
        }

        void Start()
        {
            Init();
            OriginPos = transform.position;
            StartCoroutine(AdjustOri());
            gameObject.name += "_" + GetLocalId();
        }

        /// <summary>
        /// 等人物掉 地面上再初始化 
        /// </summary>
        /// <returns>The ori.</returns>
        IEnumerator AdjustOri()
        {
            yield return new WaitForSeconds(0.5f);
            OriginPos = transform.position;
        }
        
        /// <summary>
        /// 是否是本地玩家控制对象 
        /// </summary>
        /// <returns><c>true</c> if this instance is me; otherwise, <c>false</c>.</returns>
        public bool IsMine()
        {
            return GetComponent<KBEngine.KBNetworkView>().IsMine;
        }

        /// <summary>
        /// 不是自己控制的对象则是代理
        /// 代理释放的技能不会产生伤害
        /// </summary>
        /// <returns><c>true</c> if this instance is proxy; otherwise, <c>false</c>.</returns>
        public bool IsProxy()
        {
            return !GetComponent<KBEngine.KBNetworkView>().IsMine;
        }

        public int GetLocalId()
        {
            return GetComponent<KBEngine.KBNetworkView>().GetLocalId();
        }

        /// <summary>
        /// 属性的修改都是对象自己负责自己的 其它人不能修改 
        /// 属性是可以同步的
        /// </summary>
        /// <param name="c">C.</param>
        public void ChangeHP(int c)
        {
            if (IsMine())
            { 
                HP += c;
                HP = Mathf.Min(Mathf.Max(0, HP), HP_Max);
                Log.Important("Init GameObject HP " + gameObject.name);

                var evt1 = new MyEvent(MyEvent.EventType.UnitHP);
                evt1.localID = GetLocalId();
                evt1.intArg = HP;
                evt1.intArg1 = HP_Max;
                MyEventSystem.myEventSystem.PushLocalEvent(evt1.localID, evt1);

                if (GetLocalId() == ObjectManager.objectManager.GetMyLocalId())
                {
                    MyEventSystem.myEventSystem.PushEvent(MyEvent.EventType.UpdateMainUI);
                }
            }
        }

        public void ChangeMP(int c)
        {
            if (IsMine())
            {
                MP += c;
                MP = Mathf.Min(Mathf.Max(0, MP), MP_Max);
                var rate = MP * 1.0f / MP_Max * 1.0f;

                var evt = new MyEvent(MyEvent.EventType.UnitMPPercent);
                evt.localID = GetLocalId();

                evt.floatArg = rate;
                MyEventSystem.myEventSystem.PushEvent(evt);

                var evt1 = new MyEvent(MyEvent.EventType.UnitMP);
                evt1.localID = GetLocalId();
                evt1.intArg = MP;
                evt1.intArg1 = MP_Max;
                MyEventSystem.myEventSystem.PushEvent(evt1);

                if (GetLocalId() == ObjectManager.objectManager.GetMyLocalId())
                {
                    MyEventSystem.myEventSystem.PushEvent(MyEvent.EventType.UpdateMainUI);
                }
            }
        }

        /*
         * Damage Type 
         */
        public void DoHurt(int v, bool isCritical, SkillData.DamageType dt = SkillData.DamageType.Physic)
        {
            Debug.Log("NpcAttribute::DoHurt Name:" + gameObject.name + " hurtValue:" + v + " Armor:" + Armor + " DamageType " + dt);
            if (dt == SkillData.DamageType.Physic)
            {
                int hurt = v - Armor;
                Log.Important("Get Hurt is " + hurt);
                if (hurt > 0)
                {
                    if (!isCritical)
                    {
                        PopupTextManager.popTextManager.ShowRedText("-" + hurt.ToString(), transform);
                    } else
                    {
                        PopupTextManager.popTextManager.ShowPurpleText("-" + hurt.ToString(), transform);
                    }
                    ChangeHP(-hurt);
                } else
                {
                    Log.Important("Armor too big for player " + Armor);
                }
            } else if (dt == SkillData.DamageType.Water)
            {
                var d = GetWaterDefense();
                int hurt = (int)(v * (1 - d / 100.0f));
                if (hurt > 0)
                {
                    ChangeHP(-hurt);
                }
            }
        }

        //calculate Hurt event in stunned

        public bool CheckDead()
        {
            return (HP <= 0);
        }

        //精英怪或者怪物变种 需要替换纹理
        void SetTexture(string tex)
        {
            var skins = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
            skins.renderer.material.mainTexture = Resources.Load<Texture>(tex);

        }

        public void SetExp(int e)
        {
            Exp = e;
            if (IsMine())
            {
                MyEventSystem.PushEventStatic(MyEvent.EventType.UpdatePlayerData);
            }
        }

        //TODO: 单人副本中需要判断是否升级以及升级相关处理
        public void ChangeExp(int e)
        {
            Exp += e;
            var maxExp = _ObjUnitData.MaxExp;

            if (Exp >= maxExp)
            {
                LevelUp();
            } else
            {
                if (IsMine())
                {
                    var sync = CGAddProp.CreateBuilder();
                    sync.Key = (int)CharAttribute.CharAttributeEnum.EXP;
                    sync.Value = e;
                    KBEngine.Bundle.sendImmediate(sync);
                }
            }

            var evt = new MyEvent(MyEvent.EventType.UpdatePlayerData);
            evt.localID = GetLocalId();
            MyEventSystem.myEventSystem.PushEvent(evt);
            if (IsMine())
            {
                MyEventSystem.PushEventStatic(MyEvent.EventType.UpdatePlayerData);
            }
        }

        public void ChangeLevel(int lev)
        {
            Level = lev;
            if (GetLocalId() == ObjectManager.objectManager.GetMyLocalId())
            {
                MyEventSystem.myEventSystem.PushEvent(MyEvent.EventType.UpdatePlayerData);
            }
        }

        //TODO:玩家升级的逻辑处理  技能点
        void LevelUp()
        {
            //Modify Hp Mp
            Level += 1;
            Exp = 0;

            Log.Net("AddLevelUp " + IsMine());
            if (IsMine())
            {
                var setSync = CGSetProp.CreateBuilder();
                setSync.Key = (int)CharAttribute.CharAttributeEnum.EXP;
                setSync.Value = 0;
                KBEngine.Bundle.sendImmediate(setSync);

                var sync = CGAddProp.CreateBuilder();
                sync.Key = (int)CharAttribute.CharAttributeEnum.LEVEL;
                sync.Value = 1;
                KBEngine.Bundle.sendImmediate(sync);
            }

            Util.ShowLevelUp(Level);
            var par = Instantiate(Resources.Load<GameObject>("particles/events/levelUp")) as GameObject;
            NGUITools.AddMissingComponent<RemoveSelf>(par);
            par.transform.parent = ObjectManager.objectManager.transform;
            par.transform.position = transform.position;

            if (IsMine())
            {
                //MyEventSystem.myEventSystem.PushEvent(MyEvent.EventType.UpdateMainUI);
                MyEventSystem.myEventSystem.PushEvent(MyEvent.EventType.UpdatePlayerData);
            }
        }



        //TODO: 掉落物品机制重新设计 掉落物品和掉落黄金
        public List<List<float>> GetDropTreasure()
        {
            var myLev = _ObjUnitData.Level;
            var pLev = ObjectManager.objectManager.GetMyAttr().Level;
            var num = (pLev - myLev) / 10;
            var mod = 100;
            if (num > 0)
            {
                mod = mod >> num;
            }
            Log.Sys("DropMod " + mod + " lev " + pLev + " mlev " + myLev);

            return _ObjUnitData.GetRandomDrop(mod / 100.0f);
        }


        private SkillData GetDeadSkill()
        {
            return GetComponent<SkillInfoComponent>().GetDeadSkill();
        }

        IEnumerator AddHpProgress(float duration, float totalAdd)
        {
            float addRate = totalAdd / duration;
            float goneTime = 0;
            int count = 0;
            int tc = Mathf.RoundToInt(duration / 0.1f);
            while (count < tc)
            {
                if (goneTime > 0.1f)
                {
                    HP += Mathf.RoundToInt(addRate * 0.1f);
                    HP = Mathf.Min(HP_Max, HP);
                    ChangeHP(0);
                    goneTime -= 0.1f;
                }
                goneTime += Time.deltaTime;
                count++;
                yield return null;
            }
        }

        //TODO: 吃个药瓶
        public void AddHp(float duration, float totalAdd)
        {
            StartCoroutine(AddHpProgress(duration, totalAdd));
        }

        IEnumerator AddMpProgress(float duration, float totalAdd)
        {
            float addRate = totalAdd / duration;
            float goneTime = 0;
            int count = 0;
            int tc = Mathf.RoundToInt(duration / 0.1f);
            while (count < tc)
            {
                if (goneTime > 0.1f)
                {
                    MP += Mathf.RoundToInt(addRate * 0.1f);
                    MP = Mathf.Min(MP_Max, MP);
                    ChangeMP(0);
                    goneTime -= 0.1f;
                }
                goneTime += Time.deltaTime;
                count++;
                yield return null;
            }
        }

        public void AddMp(float duration, float totalAdd)
        {
            StartCoroutine(AddMpProgress(duration, totalAdd));
        }

        public void OnlyShowDeadEffect()
        {
            _characterState = CharacterState.Dead;
            var sdata = GetDeadSkill();
            if (sdata != null)
            {
                StartCoroutine(SkillLogic.MakeSkill(gameObject, sdata, transform.position));
            }
            
        }

        /// <summary>
        /// 死亡时一系列操作 
        /// </summary>
        public void ShowDead()
        {
            DeadIgnoreCol();
            OnlyShowDeadEffect();
        }

        public void DeadIgnoreCol()
        {
            IsDead = true;
            if (ObjectManager.objectManager != null && ObjectManager.objectManager.myPlayer != null)
            {
                Physics.IgnoreCollision(GetComponent<CharacterController>(), ObjectManager.objectManager.GetMyPlayer().GetComponent<CharacterController>());
            }
        }

        /// <summary>
        /// 复活时操作
        /// </summary>
        public void Relive()
        {
        }

        public bool CheckAni(string name)
        {
            return animation.GetClip(name) != null; 
        }

    }

}