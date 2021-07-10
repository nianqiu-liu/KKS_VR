using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRGIN.Core;
using UnityEngine;
using HarmonyLib;

// This file is a collection of hooks to move the VR camera at appropriate
// points of the game.

namespace KoikatuVR
{
    [HarmonyPatch(typeof(ADV.Commands.Base.NullSet))]
    class NullSetPatches
    {
        [HarmonyPatch("Do")]
        [HarmonyPostfix]
        static void PostDo(ADV.Commands.Base.NullSet __instance)
        {
            VRLog.Info("PostDo target={0}", __instance.args[1]);
            if (__instance.args[1] == "Camera" &&
                __instance.scenario.AdvCamera.GetComponent<ActionCameraControl>()?.VRIdealCamera.transform is Transform cameraTrans)
            {
                VRMover.Instance.MaybeMoveToADV(__instance.scenario, cameraTrans.position, cameraTrans.rotation, keepHeight: false);
            }
        }
    }

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

            VRLog.Info("PostADVCameraSetting");
            var backTrans = __instance.BackCamera.transform;
            VRMover.Instance.MaybeMoveToADV(__instance, backTrans.position, backTrans.rotation, keepHeight: false);
        }
    }

    [HarmonyPatch(typeof(ADV.Program))]
    class ProgramPatches1
    {
        [HarmonyPatch(nameof(ADV.Program.SetNull))]
        [HarmonyPostfix]
        static void PostSetNull(Transform transform)
        {
            VRLog.Info("PostSetNull");
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
            VRLog.Info("PostSetCamRoot");
            VRMover.Instance.MaybeMoveTo(__instance.camRoot.position, __instance.camRoot.rotation, keepHeight: false);
        }

        [HarmonyPatch(nameof(ADV.EventCG.Data.Restore))]
        [HarmonyPostfix]
        static void PostRestore(ADV.EventCG.Data __instance)
        {
            VRLog.Info("PostRestore");
            VRMover.Instance.MaybeMoveTo(__instance.camRoot.position, __instance.camRoot.rotation, keepHeight: false);
        }
    }

}
