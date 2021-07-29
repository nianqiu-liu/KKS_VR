using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;

namespace KoikatuVR.Caress
{
    public class CaressUtil
    {
        /// <summary>
        /// Modify the internal state of the hand controls so that subsequent mouse button
        /// presses are interpreted to point to the specified (female, point) pair.
        /// </summary>
        public static void SetSelectKindTouch(HSceneProc proc, int femaleIndex, HandCtrl.AibuColliderKind colliderKind)
        {
            for (int i = 0; i < proc.flags.lstHeroine.Count; i++)
            {
                var hand = i == 0 ? proc.hand : Compat.HSceenProc_hand1(proc);
                var kind = i == femaleIndex ? colliderKind : HandCtrl.AibuColliderKind.none;
                new Traverse(hand).Field("selectKindTouch").SetValue(kind);
            }
        }
    }
}
