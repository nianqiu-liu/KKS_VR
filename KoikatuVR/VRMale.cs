using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRGIN.Core;
using HarmonyLib;

namespace KoikatuVR
{
    /// <summary>
    /// A component to be attached to every male character.
    /// </summary>
    class VRMale : ProtectedBehaviour
    {
        private ChaControl _control;

        protected override void OnAwake()
        {
            _control = GetComponent<ChaControl>();
        }

        protected override void OnUpdate()
        {
            // Hide the head iff the VR camera is inside it.
            // This also essentially negates the effect of scenairo-controlled
            // head hiding, which is found in some ADV scenes.
            var head = _control.objHead?.transform;
            if (_control.objTop?.activeSelf == true && head != null)
            {
                var vrEye = VR.Camera.transform;
                var headCenter = head.TransformPoint(0, 0.12f, -0.04f);
                var sqrDistance = (vrEye.position - headCenter).sqrMagnitude;
                _control.fileStatus.visibleHeadAlways = 0.0361f < sqrDistance; // 19 centimeters
            }
            else
            {
                _control.fileStatus.visibleHeadAlways = true;
            }
        }
    }

    [HarmonyPatch(typeof(ChaControl))]
    class ChaControlPatches
    {
        [HarmonyPatch(nameof(ChaControl.Initialize))]
        [HarmonyPostfix]
        static void PostInitialize(ChaControl __instance)
        {
            if (__instance.sex == 0)
            {
                __instance.GetOrAddComponent<VRMale>();
            }
        }
    }
}
