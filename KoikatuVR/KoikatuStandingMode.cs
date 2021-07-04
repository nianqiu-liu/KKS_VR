using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRGIN.Controls;
using VRGIN.Controls.Tools;
using VRGIN.Core;
using VRGIN.Helpers;
using VRGIN.Modes;
using UnityEngine;

namespace KoikatuVR
{
    class KoikatuStandingMode : StandingMode
    {
        private Interpreters.KoikatuInterpreter _interpreter;

        public override IEnumerable<Type> Tools
        {
            get
            {
                return base.Tools.Concat(new Type[] { typeof(MenuTool), typeof(WarpTool), typeof(SchoolTool)});
            }
        }

        protected override IEnumerable<IShortcut> CreateShortcuts()
        {
            // Disable all VRGIN shortcuts. We'll define necessary shortcuts
            // (if any) by ourselves.
            return Enumerable.Empty<IShortcut>();
        }

        protected override void OnStart()
        {
            base.OnStart();
            Caress.VRMouth.Init();
            _interpreter = VR.Interpreter as Interpreters.KoikatuInterpreter;
        }

        protected override Controller CreateLeftController()
        {
            return AddComponents(base.CreateLeftController());
        }

        protected override Controller CreateRightController()
        {
            return AddComponents(base.CreateRightController());
        }

        private static Controller AddComponents(Controller controller)
        {
            controller.gameObject.AddComponent<Caress.CaressController>();
            controller.gameObject.AddComponent<LocationPicker>();
            controller.gameObject.AddComponent<TalkSceneHandler>().enabled = false;
            return controller;
        }

        protected override void SyncCameras()
        {
            var scene = _interpreter?.CurrentScene;
            if (scene == Interpreters.KoikatuInterpreter.HScene || scene == Interpreters.KoikatuInterpreter.CustomScene)
            {
                /* Do nothing. CameraControlContrl takes care of this */
            }
            else
            {
                base.SyncCameras();
            }
        }
    }
}
