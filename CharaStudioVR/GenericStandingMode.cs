using System;
using System.Collections.Generic;
using System.Linq;
using KKS_VR.Controls;
using VRGIN.Modes;

namespace KKS_VR
{
    internal class GenericStandingMode : StandingMode
    {
        public override IEnumerable<Type> Tools => base.Tools.Concat(new Type[3]
        {
            typeof(BetterMenuTool),
            typeof(BetterWarpTool),
            typeof(GripMoveStudioNEOV2Tool)
        });
    }
}
