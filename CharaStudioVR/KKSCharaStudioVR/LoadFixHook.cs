using System;
using BepInEx.Logging;
using HarmonyLib;
using Manager;
using Studio;
using VRGIN.Core;

namespace KKSCharaStudioVR
{
	public static class LoadFixHook
	{
		public static bool forceSetStandingMode;

		private static bool standingMode;

		public static void InstallHook()
		{
			new Harmony("HS2VRStudioNEOV2VR.LoadFixHook").PatchAll(typeof(LoadFixHook));
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SceneLoadScene), "OnClickLoad", new Type[] { })]
		public static bool LoadScenePreHook(global::Studio.Studio __instance)
		{
			try
			{
				KKSCharaStudioVRPlugin.PluginLogger.Log(LogLevel.Debug, "Start Scene Loading.");
				if (VRManager.Instance.Mode is GenericStandingMode)
				{
					(VR.Manager.Interpreter as KKSCharaStudioInterpreter).ForceResetVRMode();
				}
			}
			catch (Exception obj)
			{
				VRLog.Error(obj);
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Scene), "UnloadBaseScene", new Type[] { })]
		public static void UnloadBaseScenePreHook()
		{
			try
			{
				KKSCharaStudioVRPlugin.PluginLogger.Log(LogLevel.Debug, "Start Scene or Map Unloading.");
				if (VRManager.Instance.Mode is GenericStandingMode)
				{
					standingMode = true;
				}
				else
				{
					standingMode = false;
				}
			}
			catch (Exception obj)
			{
				VRLog.Error(obj);
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Map), "OnLoadAfter", new Type[] { typeof(string) })]
		public static void OnLoadAfter(Map __instance, string levelName)
		{
			if (standingMode)
			{
				KKSCharaStudioVRPlugin.PluginLogger.Log(LogLevel.Debug, "Start Scene or Map Unloading.");
				(VR.Manager.Interpreter as KKSCharaStudioInterpreter).ForceResetVRMode();
			}
		}
	}
}
