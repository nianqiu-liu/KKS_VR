using UnityEngine;
using VRGIN.Core;
using HarmonyLib;
using System.Collections.Generic;

namespace KoikatuVR.Interpreters
{
    class HSceneInterpreter : SceneInterpreter
    {
        private bool _initialized;
        Caress.VRMouth _vrMouth;

        public override void OnStart()
        {
        }

        public override void OnDisable()
        {
            GameObject.Destroy(_vrMouth);
        }

        public override void OnUpdate()
        {
            if (!_initialized &&
                GameObject.FindObjectOfType<HSceneProc>() is HSceneProc proc
                && proc.enabled)
            {
                _vrMouth = VR.Camera.gameObject.AddComponent<Caress.VRMouth>();
                _initialized = true;
            }
        }
    }
}
