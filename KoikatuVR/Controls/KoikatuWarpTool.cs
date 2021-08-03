using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRGIN.Controls;
using UnityEngine;

namespace KoikatuVR.Controls
{
    class KoikatuWarpTool : VRGIN.Controls.Tools.WarpTool
    {
        public override List<HelpText> GetHelpTexts()
        {
            return new List<HelpText>(new HelpText[] {
                ToolUtil.HelpTrackpadCenter(Owner, "Press to teleport"),
                ToolUtil.HelpGrip(Owner, "Hold to move"),
            }.Where(x => x != null));
        }
    }
}
