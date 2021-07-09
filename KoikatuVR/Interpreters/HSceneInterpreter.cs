using UnityEngine;
using VRGIN.Core;
using HarmonyLib;
using System.Collections.Generic;

namespace KoikatuVR.Interpreters
{
    class HSceneInterpreter : SceneInterpreter
    {
        public override void OnStart()
        {
        }

        public override void OnDisable()
        {
            // nothing to do.
        }

        public override void OnUpdate()
        {
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
