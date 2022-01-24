using System;
using BepInEx;
using BepInEx.Logging;
using UnityEngine.SceneManagement;
using VRGIN.Core;

namespace KKSCharaStudioVR
{
	[BepInProcess("CharaStudio")]
	[BepInPlugin("KKS_CharaStudioVR", "KKS_CharaStudioVR", "0.0.2")]
	public class KKSCharaStudioVRPlugin : BaseUnityPlugin
	{
		public const string GUID = "KKS_CharaStudioVR";

		public const string NAME = "KKS_CharaStudioVR";

		public const string VERSION = "0.0.2";

		private static ManualLogSource defaultLogger;

		public bool vrActivated;

		public static ManualLogSource PluginLogger => defaultLogger;

		public KKSCharaStudioVRPlugin()
		{
			vrActivated = Environment.CommandLine.Contains("--studiovr");
			defaultLogger = base.Logger;
			if (vrActivated)
			{
				OpenVRHelperTempfixHook.Patch();
				SceneManager.sceneLoaded += OnSceneLoaded;
			}
			else
			{
				base.Logger.LogInfo("Ignore loading VR. To load VR, specify '--studiovr' in commandline option.");
			}
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			defaultLogger.LogDebug($"Scene Loaded {scene.name}, {mode}");
			if (mode == LoadSceneMode.Single)
			{
				VRLog.Debug("Loaded Scene is " + scene.name);
				if (vrActivated && "Studio" == scene.name)
				{
					VRLoader.Create(true);
					SaveLoadSceneHook.InstallHook();
					LoadFixHook.InstallHook();
					VRLog.Level = VRLog.LogMode.Info;
				}
			}
		}
	}
}
