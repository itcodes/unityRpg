﻿using UnityEngine;
using System.Collections;

namespace ChuMeng
{
	public class GraphInit : MonoBehaviour
	{
        public static GraphInit Instance;
        void Awake() {
            Instance = this;
        }

        public Texture lightMap;
        //public Vector3 camPos = Vector3.zero;

        public Vector3 ambient = Vector3.one;
        public Texture lightMask;
        public float lightCoff;
		
        void InitAll() {
            var lc = Resources.Load<GameObject>("LightCamera").camera;
            var lightCamera = lc.GetComponent<LightCamera>();
            //New Shader lightMapxxx need these Set
            //var lc = GameObject.FindGameObjectWithTag("LightCamera").camera;
            
            var camSize = lc.orthographicSize;
            Shader.SetGlobalTexture("_LightMap", lightMap);
            Shader.SetGlobalVector("_CamPos", lightCamera.CamPos);
            Shader.SetGlobalFloat("_CameraSize", camSize);
            
            Shader.SetGlobalVector("_AmbientCol", ambient);
            Shader.SetGlobalTexture("_LightMask", lightMask);
            Shader.SetGlobalFloat("_LightCoff", lightCoff);
            
            
            
            Shader.SetGlobalColor ("_OverlayColor", new Color(68/255.0f, 227/255.0f, 237/255.0f, 0.5f));
            Shader.SetGlobalColor ("_ShadowColor", new Color (28/255.0f, 25/255.0f, 25/255.0f, 1));
            Shader.SetGlobalVector ("_LightDir", new Vector3 (-1, -1, -1));
            Shader.SetGlobalVector ("_HighLightDir", new Vector3 (-1, -1, -1));
            Shader.SetGlobalColor ("_LightDiffuseColor", new Color (223/255.0f, 248/255.0f, 255/255.0f, 1));
            
            Shader.SetGlobalColor ("_GhostColor", new Color(68/255.0f, 227/255.0f, 68/255.0f, 0.5f));
            
            var res = Screen.currentResolution;
            Log.GUI ("Screen Attribute resolution "+res.width + " "+res.height+" "+res.refreshRate);
            Log.GUI ("Screen Attribute dpi "+Screen.dpi);
            Log.GUI ("Screen Attribute height "+Screen.height);
            Log.GUI ("Screen Attribute width "+Screen.width); 
        }
        // Use this for initialization
		void Start ()
		{
            InitAll();
		}
	
        public Color testAmbient;
        [ButtonCallFunc()]
        public bool InitAmbient;
        public void InitAmbientMethod() {
            Shader.SetGlobalVector("_AmbientCol", testAmbient);
            Shader.SetGlobalFloat("_LightCoff", lightCoff);
        }

        [ButtonCallFunc()]
        public bool InitNow;
        public void InitNowMethod() {
            InitAll();
        }

        public bool IsBlind = false;
        public void SetBlind(bool b) {
            IsBlind = b;
            if(IsBlind) {
                Blind();
            }else {
                Clear();
            }
        }

        //当前Room的Light和Props关闭
        private void Blind() {
            Shader.SetGlobalVector("_AmbientCol", Vector3.zero);
            var zone = BattleManager.battleManager.Zones;
            var cz = BattleManager.battleManager.currentZone;
            if(cz >= 0 && cz < zone.Count) {
                //var z = zone[cz];
                var z = GameObject.Find("Root_"+cz);
                if(z != null) {
                    var props = z.transform.Find("Props");
                    var light = z.transform.Find("Light");
                    if(props != null && light != null) {
                        props.gameObject.SetActive(false);
                        light.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void Clear() {
            Shader.SetGlobalVector("_AmbientCol", ambient);
            var zone = BattleManager.battleManager.Zones;
            var cz = BattleManager.battleManager.currentZone;
            if(cz >= 0 && cz < zone.Count) {
                var z = GameObject.Find("Root_"+cz);
                if(z != null) {
                    var props = z.transform.Find("Props");
                    var light = z.transform.Find("Light");
                    if(props != null && light != null) {
                        props.gameObject.SetActive(true);
                        light.gameObject.SetActive(true);
                    }
                }
            }
        }

	}
}
