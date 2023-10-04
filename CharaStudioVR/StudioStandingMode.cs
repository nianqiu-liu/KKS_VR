using System;
using System.Collections.Generic;
using KKS_VR.Controls;
using KKS_VR.Features;
using UnityEngine.XR;
using VRGIN.Controls;
using VRGIN.Modes;

namespace KKS_VR
{
    internal class StudioStandingMode : StandingMode
    {
        public override IEnumerable<Type> Tools { get; } = new[]
        {
            typeof(BetterMenuTool),
            typeof(BetterWarpTool),
            typeof(GripMoveStudioNEOV2Tool)
        };

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
            VRBoop.Initialize(controller, controllerSide);
        }
    }
}
