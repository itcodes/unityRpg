﻿using UnityEngine;
using System.Collections;
using ChuMeng;

public class TestKnightage : MonoBehaviour {

	void Start()
	{

		StartCoroutine(initPage ());
	}
	private IEnumerator initPage()
	{
		if (SaveGame.saveGame == null) {
			var g = new GameObject();
			var s = g.AddComponent<SaveGame>();
			s.InitData();
		}
		yield return new WaitForSeconds (1);
		Log.Net ("等待数据初始化结束 才显示 UI");
		yield return StartCoroutine(GuildController.guildController.LoadGuildInfo());

		//yield return new WaitForSeconds (5);
		Log.Net ("load data over");
		WindowMng.windowMng.PushView ("UI/Knightage");
		//GameObject page = Instantiate (mailpannel) as GameObject;
		//page.transform.parent = this.transform;
		//page.transform.localScale = Vector3.one;
	}
}
