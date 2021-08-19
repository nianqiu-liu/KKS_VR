using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using VRGIN.Core;
using UnityEngine;
using KoikatuVR.Interpreters;

// Fixes issues that are in the base game but are only relevant in VR.

namespace KoikatuVR
{
    /// <summary>
    /// Avoid triggering resource unload when loading UI-only scenes.
    /// </summary>
    [HarmonyPatch]
    class ScenePatches
    {
        /// <summary>
        /// Return a list of all methods that constitutes the implementation of
        /// the specified coroutine.
        ///
        /// Not well-tested, use with care.
        /// </summary>
        private static List<MethodInfo> GetImplementationMethodsForCoroutine(Type type, string name)
        {
            List<MethodInfo> ret = new List<MethodInfo>();

            ret.Add(type.GetMethod(name, AccessTools.all));
            var nested = typeof(Manager.Scene).GetNestedTypes(BindingFlags.NonPublic)
                .Where(nested1 => nested1.Name.StartsWith("<" + name + ">c__Iterator"))
                .FirstOrDefault();
            if (nested == null)
            {
                VRLog.Warn($"Cannot find iterator for {name}");
                return ret;
            }
            ret.Add(nested.GetMethod("MoveNext", AccessTools.all));
            return ret;
        }

        private static IEnumerable<MethodBase> TargetMethods()
        {
            return GetImplementationMethodsForCoroutine(typeof(Manager.Scene), "LoadStart")
                .Cast<MethodBase>();
        }

        private static AsyncOperation MaybeUnloadUnusedAssets()
        {
            bool shouldUnload = Manager.Scene.Instance.IsFadeNow;
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
            {
                if (inst.opcode == OpCodes.Call &&
                    inst.operand is MethodInfo method &&
                    method.Name == "UnloadUnusedAssets")
                {
                    yield return CodeInstruction.Call(() => MaybeUnloadUnusedAssets());
                }
                else
                {
                    yield return inst;
                }
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
}
