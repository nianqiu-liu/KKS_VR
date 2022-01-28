using System;
using BepInEx;
using BepInEx.Logging;
using KKAPI;
using Shared;
using UnityEngine.SceneManagement;
using VRGIN.Core;

namespace KKSCharaStudioVR
{
    /// <summary>
    /// Studio code was forked from KKS_CharaStudioVR at https://vr-erogamer.com/archives/1065
    /// </summary>
    [BepInProcess(KoikatuAPI.StudioProcessName)]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class KKSCharaStudioVRPlugin : BaseUnityPlugin
    {
        public const string GUID = "KKS_CharaStudioVR";

        public const string NAME = "KKS_CharaStudioVR";

        public const string VERSION = Constants.Version;

        private static ManualLogSource defaultLogger;

        public readonly bool vrActivated;

        public static ManualLogSource PluginLogger => defaultLogger;

        public KKSCharaStudioVRPlugin()
        {
            vrActivated = Environment.CommandLine.Contains("--vr") || Environment.CommandLine.Contains("--studiovr");
            defaultLogger = Logger;
            if (vrActivated)
            {
                OpenVRHelperTempfixHook.Patch();
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                //base.Logger.LogInfo("Ignore loading VR. To load VR, specify '--studiovr' in commandline option.");
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            //defaultLogger.LogDebug($"Scene Loaded {scene.name}, {mode}");
            if (mode == LoadSceneMode.Single)
                //VRLog.Debug("Loaded Scene is " + scene.name);
                if (vrActivated && "Studio" == scene.name)
                {
                    defaultLogger.LogInfo("Loading VR mode...");

                    VRLoader.Create(true);
                    SaveLoadSceneHook.InstallHook();
                    LoadFixHook.InstallHook();
                    VRLog.Level = VRLog.LogMode.Info;

                    SceneManager.sceneLoaded -= OnSceneLoaded;
                }
        }
    }
}
