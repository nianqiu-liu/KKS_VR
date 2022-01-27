using System;
using HarmonyLib;
using Unity.XR.OpenVR;

namespace KKSCharaStudioVR
{
    public static class OpenVRHelperTempfixHook
    {
        public static void Patch()
        {
            new Harmony("OpenVRHelperTempfixHook").PatchAll(typeof(OpenVRHelperTempfixHook));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(OpenVRHelpers), "IsUsingSteamVRInput", new Type[] { })]
        public static bool PreIsUsingSteamVRInput(ref bool __result)
        {
            __result = true;
            return false;
        }
    }
}
