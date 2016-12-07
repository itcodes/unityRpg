﻿using UnityEngine;
using System.Collections;

namespace ChuMeng
{
    public static class NetDateInterface
    {
        /// <summary>
        ///相同的技能 Skill Configure来触发Buff 但是不要触发 Buff修改非表现属性
        /// </summary>
        /// <param name="affix">Affix.</param>
        /// <param name="attacker">Attacker.</param>
        /// <param name="target">Target.</param>
        public static void FastAddBuff(Affix affix, GameObject attacker, GameObject target, int skillId, int evtId)
        {
            var cg = CGPlayerCmd.CreateBuilder();
            var binfo = BuffInfo.CreateBuilder();
            binfo.BuffType = (int)affix.effectType;
            binfo.Attacker = attacker.GetComponent<KBEngine.KBNetworkView>().GetServerID();
            binfo.Target = target.GetComponent<KBEngine.KBNetworkView>().GetServerID();
            binfo.SkillId = skillId;
            binfo.EventId = evtId;
            var pos = attacker.transform.position;
            binfo.AddAttackerPos((int)(pos.x * 100));
            binfo.AddAttackerPos((int)(pos.y * 100));
            binfo.AddAttackerPos((int)(pos.z * 100));
            //Log.Net(binfo.AttackerPosCount);

            cg.BuffInfo = binfo.Build();
            cg.Cmd = "Buff";
            var sc = WorldManager.worldManager.GetActive();
            sc.BroadcastMsg(cg);
        }

        public static void FastUseSkill(int skillId)
        {
            var sc = WorldManager.worldManager.GetActive();
            if (sc.IsNet)
            {
                var cg = CGPlayerCmd.CreateBuilder();
                var skInfo = SkillAction.CreateBuilder();
                skInfo.Who = ObjectManager.objectManager.GetMyServerID(); 
                skInfo.SkillId = skillId;
                cg.SkillAction = skInfo.Build();
                cg.Cmd = "Skill";
                sc.BroadcastMsg(cg);
            }
        }

        public static void FastDamage(int attackerId, int enemyId, int damage, bool isCritical)
        {
            var cg = CGPlayerCmd.CreateBuilder();
            var dinfo = DamageInfo.CreateBuilder();
            dinfo.Attacker = attackerId;
            dinfo.Enemy = enemyId;
            dinfo.Damage = damage;
            dinfo.IsCritical = isCritical;
            cg.DamageInfo = dinfo.Build();
            cg.Cmd = "Damage";
            WorldManager.worldManager.GetActive().BroadcastMsg(cg);
        }

        public static void FastMoveAndPos()
        {
            var s = WorldManager.worldManager.GetActive();
            if (s.IsNet)
            {
                var me = ObjectManager.objectManager.GetMyPlayer();
                if (me == null)
                {
                    return;
                }
                var pos = me.transform.position;
                var dir = (int)me.transform.localRotation.eulerAngles.y;

                var cg = CGPlayerCmd.CreateBuilder();
                cg.Cmd = "Move";
                var ainfo = AvatarInfo.CreateBuilder();
                ainfo.X = (int)(pos.x * 100);
                ainfo.Z = (int)(pos.z * 100);
                ainfo.Y = (int)(pos.y * 100);
                ainfo.Dir = dir;
                cg.AvatarInfo = ainfo.Build();

                s.BroadcastMsg(cg);
            }
        }

        public static void SyncPosDirHP()
        {
            var me = ObjectManager.objectManager.GetMyPlayer();
            if (me == null)
            {
                return;
            }
            var pos = me.transform.position;
            var dir = (int)me.transform.localRotation.eulerAngles.y;
            var meAttr = me.GetComponent<NpcAttribute>();

            var cg = CGPlayerCmd.CreateBuilder();
            cg.Cmd = "UpdateData";
            var ainfo = AvatarInfo.CreateBuilder();
            ainfo.X = (int)(pos.x * 100);
            ainfo.Z = (int)(pos.z * 100);
            ainfo.Y = (int)(pos.y * 100);
            ainfo.Dir = dir;
            ainfo.HP = meAttr.HP;

            cg.AvatarInfo = ainfo.Build();

            var s = WorldManager.worldManager.GetActive();
            s.BroadcastMsg(cg);
        }

        public static PlayerSync GetPlayer(int id)
        {
            var player = ObjectManager.objectManager.GetPlayer(id);
            if (player != null)
            {
                var sync = player.GetComponent<PlayerSync>();
                return sync;
            }
            return null;
        }
    }
}
