using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRGIN.Core;
using VRGIN.Controls;
using UnityEngine;

namespace KoikatuVR.Interpreters
{
    class TalkSceneInterpreter : SceneInterpreter
    {
        public override void OnDisable()
        {
            SetTalkSceneHandlerEnabled(false);
        }

        public override void OnStart()
        {
            SetTalkSceneHandlerEnabled(true);
        }


        public override void OnUpdate()
        {
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
