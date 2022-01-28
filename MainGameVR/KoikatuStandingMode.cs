using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR;
using VRGIN.Controls;
using VRGIN.Modes;

namespace KoikatuVR
{
    /// <summary>
    /// Initialize controllers and custom tools
    /// </summary>
    internal class KoikatuStandingMode : StandingMode
    {
        public override IEnumerable<Type> Tools
        {
            get { return new[] { typeof(Controls.KoikatuMenuTool), typeof(Controls.KoikatuWarpTool), typeof(Controls.SchoolTool) }; }
        }

        protected override IEnumerable<IShortcut> CreateShortcuts()
        {
            // Disable all VRGIN shortcuts. We'll define necessary shortcuts
            // (if any) by ourselves.
            return Enumerable.Empty<IShortcut>();
        }

        protected override Controller CreateLeftController()
        {
            var controller = base.CreateLeftController();
            AddComponents(controller, EyeSide.Left);
            return controller;
        }

        protected override Controller CreateRightController()
        {
            var controller = base.CreateRightController();
            AddComponents(controller, EyeSide.Right);
            controller.ToolIndex = 1;
            return controller;
        }

        private static void AddComponents(Controller controller, EyeSide controllerSide)
        {
            controller.gameObject.AddComponent<Controls.LocationPicker>();
            VRBoop.Initialize(controller, controllerSide);
        }

        protected override void SyncCameras()
        {
            // Do nothing. CameraControlControl and friends take care of this.
        }

        protected override void InitializeScreenCapture()
        {
            // Don't enable CapturePanorama because it looks broken (throws an
            // exception).
        }
    }
}
