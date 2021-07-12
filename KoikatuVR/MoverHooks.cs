using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRGIN.Core;
using UnityEngine;
using HarmonyLib;
using System.Collections;

// This file is a collection of hooks to move the VR camera at appropriate
// points of the game.

namespace KoikatuVR
{
    [HarmonyPatch(typeof(ADV.TextScenario))]
    class TextScenarioPatches1
    {
        [HarmonyPatch("ADVCameraSetting")]
        [HarmonyPostfix]
        static void PostADVCameraSetting(ADV.TextScenario __instance)
        {
            if (Manager.Scene.IsInstance() && Manager.Scene.Instance.NowSceneNames[0] == "Talk")
            {
                // Talk scenes are handled separately.
                return;
            }

            VRLog.Debug("PostADVCameraSetting");
            var backTrans = __instance.BackCamera.transform;
            VRMover.Instance.MaybeMoveADV(__instance, backTrans.position, backTrans.rotation, keepHeight: false);
        }

        [HarmonyPatch("_RequestNextLine")]
        [HarmonyPostfix]
        static void Post_RequestNextLine(ADV.TextScenario __instance, ref IEnumerator __result)
        {
            if (Manager.Scene.IsInstance() && Manager.Scene.Instance.NowSceneNames[0] == "Talk")
            {
                // Talk scenes are handled separately.
                return;
            }

            __result = new[] { __result, Postfix() }.GetEnumerator();

            IEnumerator Postfix()
            {
                VRMover.Instance.HandleTextScenarioProgress(__instance);
                yield break;
            }
        }
    }

    [HarmonyPatch(typeof(ADV.Program))]
    class ProgramPatches1
    {
        [HarmonyPatch(nameof(ADV.Program.SetNull))]
        [HarmonyPostfix]
        static void PostSetNull(Transform transform)
        {
            VRLog.Debug("PostSetNull");
            VRMover.Instance.MaybeMoveTo(transform.position, transform.rotation, keepHeight: false);
        }
    }

    [HarmonyPatch(typeof(ADV.EventCG.Data))]
    class EventCGDataPatches1
    {
        [HarmonyPatch(nameof(ADV.EventCG.Data.camRoot), MethodType.Setter)]
        [HarmonyPostfix]
        static void PostSetCamRoot(ADV.EventCG.Data __instance)
        {
            VRLog.Debug("PostSetCamRoot");
            VRMover.Instance.MaybeMoveTo(__instance.camRoot.position, __instance.camRoot.rotation, keepHeight: false);
        }

        [HarmonyPatch(nameof(ADV.EventCG.Data.Restore))]
        [HarmonyPostfix]
        static void PostRestore(ADV.EventCG.Data __instance)
        {
            VRLog.Debug("PostRestore");
            VRMover.Instance.MaybeMoveTo(__instance.camRoot.position, __instance.camRoot.rotation, keepHeight: false);
        }
    }

}
