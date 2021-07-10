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
    class TalkSceneInterpreter : SceneInterpreter
    {
        Canvas _canvasBack;

        public override void OnDisable()
        {
            SetTalkSceneHandlerEnabled(false);
            if (_canvasBack != null)
            {
                _canvasBack.enabled = true;
            }
        }

        public override void OnStart()
        {
            SetTalkSceneHandlerEnabled(true);

            var talkScene = GameObject.FindObjectOfType<TalkScene>();
            if (talkScene == null)
            {
                VRLog.Warn("TalkScene object not found");
                return;
            }

            talkScene.otherInitialize += () =>
            {
                // The default camera location is a bit too far for a friendly
                // conversation.
                var heroine = talkScene.targetHeroine.transform;
                VRMover.Instance.MoveTo(
                    heroine.TransformPoint(new Vector3(0, 1.4f, 0.55f)),
                    heroine.rotation * Quaternion.Euler(0, 180f, 0),
                    keepHeight: true);
            };

            _canvasBack = new Traverse(talkScene).Field<Canvas>("canvasBack").Value;
        }

        public override void OnUpdate()
        {
            // We don't need the background image because we directly see
            // background objects.
            if (_canvasBack != null)
            {
                _canvasBack.enabled = false;
            }
        }

        private void SetTalkSceneHandlerEnabled(bool enabled)
        {
            SetTalkSceneHandlerEnabledFor(VR.Mode.Left, enabled);
            SetTalkSceneHandlerEnabledFor(VR.Mode.Right, enabled);
        }

        private void SetTalkSceneHandlerEnabledFor(Controller controller, bool enabled)
        {
            controller.GetComponent<TalkSceneHandler>().enabled = enabled;
        }
    }
}
