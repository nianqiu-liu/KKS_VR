using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Collection of patches that are to be enabled only during development.

namespace KoikatuVR
{
#if false
    /// Report every time TransformDebug.targetTransform is rotated.
    [HarmonyPatch]
    class TransformPatches
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.PropertySetter(typeof(Transform), "rotation");
            yield return AccessTools.PropertySetter(typeof(Transform), "localRotation");
            yield return AccessTools.PropertySetter(typeof(Transform), "eulerAngles");
            yield return AccessTools.Method(typeof(Transform), "SetPositionAndRotation");
            yield return AccessTools.Method(typeof(Transform), "Rotate", new[] { typeof(Vector3), typeof(Space) });
            yield return AccessTools.Method(typeof(Transform), "Rotate", new[] { typeof(float), typeof(float), typeof(float), typeof(Space) });
            yield return AccessTools.Method(typeof(Transform), "Rotate", new[] { typeof(Vector3), typeof(float), typeof(Space) });
            yield return AccessTools.Method(typeof(Transform), "RotateAround", new[] { typeof(Vector3), typeof(Vector3), typeof(float) });
            yield return AccessTools.Method(typeof(Transform), "LookAt", new[] { typeof(Transform), typeof(Vector3) });
            yield return AccessTools.Method(typeof(Transform), "LookAt", new[] { typeof(Vector3), typeof(Vector3) });
            yield break;
        }

        static void Prefix(Transform __instance)
        {
            if (__instance == TransformDebug.targetTransform)
            {
                VRLog.Info("Someone is modifying the target transform!");
                VRLog.Info(new StackTrace(false));
            }
        }
    }
#endif
}
