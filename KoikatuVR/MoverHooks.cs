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

    [HarmonyPatch(typeof(HSceneProc))]
    class HSceneProcPatches
    {
        [HarmonyPatch("ChangeAnimator")]
        [HarmonyPostfix]
        public static void PostChangeAnimator(HSceneProc __instance, bool _isForceCameraReset, List<ChaControl> ___lstFemale)
        {
            if (_isForceCameraReset)
            {
                UpdateVRCamera(__instance, ___lstFemale);
            }
        }

        /*
        We could also update the camera after each location change, but this may be more confusing than useful.

        [HarmonyPatch("ChangeCategory")]
        [HarmonyPostfix]
        public static void PostChangeCategory(HSceneProc __instance, List<ChaControl> ___lstFemale)
        {
            UpdateVRCamera(__instance, ___lstFemale);
        }
        */


        /// <summary>
        /// Update the transform of the VR camera.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="lstFemale"></param>
        static void UpdateVRCamera(HSceneProc instance, List<ChaControl> lstFemale)
        {
            var baseTransform = lstFemale[0].objTop.transform;
            var camDat = new Traverse(instance.flags.ctrlCamera).Field<BaseCameraControl_Ver2.CameraData>("CamDat").Value;
            var cameraRotation = baseTransform.rotation * Quaternion.Euler(camDat.Rot);
            var cameraPosition = cameraRotation * camDat.Dir + baseTransform.TransformPoint(camDat.Pos);
            // TODO: the height calculation below assumes standing mode.
            var cameraHeight = lstFemale[0].transform.position.y + VR.Camera.transform.localPosition.y;
            VRMover.Instance.MoveTo(new Vector3(cameraPosition.x, cameraHeight, cameraPosition.z), cameraRotation, keepHeight: false);
        }
    }
}
