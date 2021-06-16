using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRGIN.Controls;
using VRGIN.Controls.Tools;
using VRGIN.Core;
using VRGIN.Helpers;
using VRGIN.Modes;

namespace KoikatuVR
{
    class GenericStandingMode : StandingMode
    {
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
    }
}
