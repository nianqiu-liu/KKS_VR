using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRGIN.Core;
using VRGIN.Controls;
using UnityEngine;
using HarmonyLib;
using System.Collections;

namespace KoikatuVR.Interpreters
{
    internal class TalkSceneInterpreter : SceneInterpreter
    {
        private Canvas _canvasBack;

        public override void OnDisable()
        {
            DestroyControllerComponent<Controls.TalkSceneHandler>();
            if (_canvasBack != null) _canvasBack.enabled = true;
        }

        public override void OnStart()
        {
            AddControllerComponent<Controls.TalkSceneHandler>();

            if (!TalkScene.initialized)
            {
                VRLog.Warn("TalkScene object not found");
                return;
            }

            VRLog.Warn("TalkScene init");

            var talkScene = TalkScene.instance;

            talkScene.otherInitialize += () =>
            {
                VRLog.Warn("talkScene.otherInitialize");

                // The default camera location is a bit too far for a friendly
                // conversation.
                var heroine = talkScene.targetHeroine.transform;
                VRCameraMover.Instance.MoveTo(
                    heroine.TransformPoint(new Vector3(0, 1.4f, 0.55f)),
                    heroine.rotation * Quaternion.Euler(0, 180f, 0),
                    true);

                // talkscene messes with camera settings
                Camera.main.clearFlags = CameraClearFlags.Skybox;

                talkScene.backGround.visible = false;
            };

            _canvasBack = talkScene.canvasBack;
        }

        public override void OnUpdate()
        {
            // We don't need the background image because we directly see
            // background objects.
            if (_canvasBack != null) _canvasBack.enabled = false;
        }
    }
}
