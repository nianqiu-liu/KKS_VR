using System.Collections.Generic;
using System.Linq;
using VRGIN.Controls;
using VRGIN.Controls.Tools;

namespace KoikatuVR.Controls
{
    internal class KoikatuMenuTool : MenuTool
    {
        public override List<HelpText> GetHelpTexts()
        {
            return new List<HelpText>(new[]
            {
                ToolUtil.HelpTrackpadCenter(Owner, "Tap to click"),
                ToolUtil.HelpTrackpadRight(Owner, "Slide to move cursor"),
                ToolUtil.HelpTrigger(Owner, "Click"),
                ToolUtil.HelpGrip(Owner, "Take/release screen")
            }.Where(x => x != null));
        }
    }
}
