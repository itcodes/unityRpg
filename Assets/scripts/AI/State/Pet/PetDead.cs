﻿using UnityEngine;
using System.Collections;

namespace ChuMeng
{
    public class PetDead : DeadState
    {
        public override void EnterState()
        {
            base.EnterState();
            SetAni("die", 1, WrapMode.Once);
            GetAttr ().ShowDead ();
        }

        public override IEnumerator RunLogic()
        {
            while(GetAttr().animation.isPlaying) {
                yield return null;
            }
            ObjectManager.objectManager.DestroyByLocalId (GetAttr().GetComponent<KBEngine.KBNetworkView>().GetLocalId());
        }
    }
}