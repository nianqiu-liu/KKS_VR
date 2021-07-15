using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRGIN.Core;
using UnityEngine;
using HarmonyLib;
using System.Collections;
using System.Reflection;

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
    }

    [HarmonyPatch]
    class RequestNextLinePatches
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            // In some versions of the base game, MainScenario._RequestNextLine
            // duplicates the logic found in TextScenario._RequestNextLine.
            // In other versions, the former simply calls the latter.
            // We want to patch both methods or the latter alone depending on
            // the version.
            yield return AccessTools.Method(typeof(ADV.TextScenario), "_RequestNextLine");
            if (AccessTools.Field(typeof(ADV.MainScenario), "textHash") == null)
            {
                yield return AccessTools.Method(typeof(ADV.MainScenario), "_RequestNextLine");
            }
        }

        static void Postfix(ADV.TextScenario __instance, ref IEnumerator __result)
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
            Vector3 dir;
            switch (instance.flags.mode)
            {
                case HFlag.EMode.masturbation:
                case HFlag.EMode.peeping:
                case HFlag.EMode.lesbian:
                    // Use the default distance for 3rd-person scenes.
                    dir = camDat.Dir;
                    break;
                default:
                    // Start closer otherwise.
                    dir = Vector3.back * 0.8f;
                    break;
            }
            var cameraPosition = cameraRotation * dir + baseTransform.TransformPoint(camDat.Pos);
            // TODO: the height calculation below assumes standing mode.
            var cameraHeight = lstFemale[0].transform.position.y + VR.Camera.transform.localPosition.y;
            VRMover.Instance.MoveTo(new Vector3(cameraPosition.x, cameraHeight, cameraPosition.z), cameraRotation, keepHeight: false);
        }
    }
}
