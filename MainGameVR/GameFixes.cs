using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using ActionGame;
using ADV;
using KKAPI.Utilities;
using VRGIN.Core;
using UnityEngine;
using KoikatuVR.Interpreters;
using Sirenix.Serialization.Utilities;
using StrayTech;
using UnityStandardAssets.ImageEffects;
using Object = UnityEngine.Object;

// Fixes issues that are in the base game but are only relevant in VR.

namespace KoikatuVR
{
    /// <summary>
    /// Avoid triggering resource unload when loading UI-only scenes.
    /// todo move into illusionfixes?
    /// </summary>
    [HarmonyPatch]
    internal class ScenePatches
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return CoroutineUtils.GetMoveNext(AccessTools.Method(typeof(Manager.Scene), nameof(Manager.Scene.LoadStart)));
        }

        private static AsyncOperation MaybeUnloadUnusedAssets()
        {
            var shouldUnload = Manager.Scene.IsFadeNow;
            if (shouldUnload)
            {
                return Resources.UnloadUnusedAssets();
            }
            else
            {
                VRLog.Info("Skipping unload");
                return null;
            }
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, MethodBase __originalMethod)
        {
            foreach (var inst in insts)
                if (inst.opcode == OpCodes.Call &&
                    inst.operand is MethodInfo method &&
                    method.Name == "UnloadUnusedAssets")
                {
                    yield return CodeInstruction.Call(() => MaybeUnloadUnusedAssets());
                    VRPlugin.Logger.LogDebug("Patched UnloadUnusedAssets in " + __originalMethod.GetFullName());
                }
                else
                {
                    yield return inst;
                }
        }
    }

    /// <summary>
    /// Suppress character update for invisible characters in some sub-scenes of Roaming.
    /// </summary>
    [HarmonyPatch(typeof(ChaControl))]
    public class ChaControlPatches1
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ChaControl.LateUpdateForce))]
        private static bool PreLateUpdateForce(ChaControl __instance)
        {
            return !SafeToSkipUpdate(__instance);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ChaControl.UpdateForce))]
        private static bool PreUpdateForce(ChaControl __instance)
        {
            return !SafeToSkipUpdate(__instance);
        }

        public static bool SafeToSkipUpdate(ChaControl control)
        {
            return
                VR.Settings is KoikatuSettings settings &&
                settings.OptimizeHInsideRoaming &&
                control.objTop?.activeSelf == false &&
                VR.Interpreter is KoikatuInterpreter interpreter &&
                (interpreter.CurrentScene == KoikatuInterpreter.SceneType.HScene ||
                 interpreter.CurrentScene == KoikatuInterpreter.SceneType.TalkScene);
        }
    }

    [HarmonyPatch(typeof(SunLightInfo))]
    public class FogHack1
    {
        // todo hack, handle properly
        [HarmonyFinalizer]
        [HarmonyPatch(nameof(SunLightInfo.Set))]
        private static Exception PreLateUpdateForce(Exception __exception)
        {
            if (__exception != null) VRPlugin.Logger.LogDebug("Caught expected crash: " + __exception);
            return null;
        }
    }

    [HarmonyPatch(typeof(ActionMap))]
    public class FogHack2
    {
        // todo hack, handle properly
        [HarmonyFinalizer]
        [HarmonyPatch(nameof(ActionMap.UpdateCameraFog))]
        private static Exception PreLateUpdateForce(Exception __exception)
        {
            if (__exception != null) VRPlugin.Logger.LogDebug("Caught expected crash: " + __exception);
            return null;
        }
    }

    // [HarmonyPatch(typeof(ADVScene))]
    // public class ADVSceneFix1
    // {
    //     // todo hack, handle properly
    //     [HarmonyPrefix]
    //     [HarmonyPatch(nameof(ADVScene.blur),MethodType.Getter)]
    //     private static bool PreLateUpdateForce(ADVScene __instance)
    //     {
    //         // If null, don't run the method so it returns null by default
    //         return __instance.cameraEffector != null;
    //     }
    // }
    [HarmonyPatch(typeof(Manager.Game))]
    public class ADVSceneFix1
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Manager.Game.cameraEffector), MethodType.Getter)]
        private static void FixMissingCameraEffector(Manager.Game __instance, ref CameraEffector __result)
        {
            if (__result == null && __instance.isCameraChanged)
                // vr camera doesn't have this component on it, which crashes game code with nullref. Use the component on original advcamera instead
                __instance._cameraEffector = __result = Object.FindObjectOfType<CameraEffector>();
        }
    }

    [HarmonyPatch(typeof(CameraSystem))]
    public class ADVSceneFix2
    {
        // fixes being unable to do some actions in roaming mode
        [HarmonyPrefix]
        [HarmonyPatch(nameof(CameraSystem.SystemStatus), MethodType.Getter)]
        private static bool FixNeverEndingTransition(ref CameraSystem.CameraSystemStatus __result)
        {
            __result = CameraSystem.CameraSystemStatus.Inactive;
            return false;
        }
    }

    //[HarmonyPatch(typeof(Transform))]
    //public class debugss
    //{
    //    private static HashSet<string> _seen = new HashSet<string>();
    //
    //    [HarmonyPrefix]
    //    [HarmonyPatch(typeof(Transform))]
    //    [HarmonyPatch(nameof(Transform.SetPositionAndRotation))]
    //    private static void FixNeverEndingTransition(Transform __instance)
    //    {
    //        var stackTrace = new StackTrace().ToString();
    //        var str = @$"TRANSFORM GET: {__instance.GetFullPath()}\n{stackTrace}";
    //        if (_seen.Add(str)) Console.WriteLine(str);
    //    }
    //}

    [HarmonyPatch]
    public class ADVSceneFix3
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return CoroutineUtils.GetMoveNext(AccessTools.Method(typeof(TalkScene), nameof(TalkScene.Setup)));
        }

        private static Camera GetOriginalMainCamera()
        {
            // vr camera doesn't have this component on it
            var originalMainCamera = (Manager.Game.instance.cameraEffector ?? Object.FindObjectOfType<CameraEffector>()).GetComponent<Camera>();
            VRPlugin.Logger.LogDebug($"GetOriginalMainCamera called, cam found: {originalMainCamera?.GetFullPath()}\n{new StackTrace()}");
            return originalMainCamera;
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, MethodBase __originalMethod)
        {
            var targert = AccessTools.PropertyGetter(typeof(Camera), nameof(Camera.main));
            var replacement = AccessTools.Method(typeof(ADVSceneFix3), nameof(GetOriginalMainCamera));
            return insts.Manipulator(
                instr => instr.opcode == OpCodes.Call && (MethodInfo)instr.operand == targert,
                instr =>
                {
                    instr.operand = replacement;
                    VRPlugin.Logger.LogDebug("Patched Camera.main in " + __originalMethod.GetNiceName());
                });
        }
    }

    [HarmonyPatch]
    public class ADVSceneFix4
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(ADVScene), nameof(ADVScene.Init));
        }

        private static Camera GetOriginalMainCamera()
        {
            // vr camera doesn't have this component on it
            var originalMainCamera = (Manager.Game.instance.cameraEffector ?? Object.FindObjectOfType<CameraEffector>()).GetComponent<Camera>();
            VRPlugin.Logger.LogDebug($"GetOriginalMainCamera called, cam found: {originalMainCamera?.GetFullPath()}\n{new StackTrace()}");
            return originalMainCamera;
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, MethodBase __originalMethod)
        {
            var targert = AccessTools.PropertyGetter(typeof(Camera), nameof(Camera.main));
            var replacement = AccessTools.Method(typeof(ADVSceneFix4), nameof(GetOriginalMainCamera));
            return insts.Manipulator(
                instr => instr.opcode == OpCodes.Call && (MethodInfo)instr.operand == targert,
                instr =>
                {
                    instr.operand = replacement;
                    VRPlugin.Logger.LogDebug("Patched Camera.main in " + __originalMethod.GetNiceName());
                });
        }

        private static void Postfix(ADVScene __instance)
        {
            Manager.Sound.Listener = Camera.main.transform;
        }
    }

    [HarmonyPatch]
    public class CycleCrossFadeFix1
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return CoroutineUtils.GetMoveNext(AccessTools.Method(typeof(Cycle), nameof(Cycle.WakeUp)));
        }

        private static bool IsProcessWithNullcheck(CrossFade instance)
        {
            return instance != null && instance.isProcess;
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, MethodBase __originalMethod)
        {
            var targert = AccessTools.PropertyGetter(typeof(CrossFade), nameof(CrossFade.isProcess));
            var replacement = AccessTools.Method(typeof(CycleCrossFadeFix1), nameof(IsProcessWithNullcheck));
            return insts.Manipulator(
                instr => instr.opcode == OpCodes.Callvirt && (MethodInfo)instr.operand == targert,
                instr =>
                {
                    instr.opcode = OpCodes.Call;
                    instr.operand = replacement;
                    VRPlugin.Logger.LogDebug("Patched CrossFade.isProcess in " + __originalMethod.GetFullName());
                });
        }
    }
}
