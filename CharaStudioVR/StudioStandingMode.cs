using System;
using System.Collections.Generic;
using KKS_VR.Controls;
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
    }
}
