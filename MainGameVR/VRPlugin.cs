using BepInEx;
using System;
using VRGIN.Core;
using System.Collections;
using UnityEngine;
using HarmonyLib;
using System.Runtime.InteropServices;
using BepInEx.Logging;
using KKAPI;
using KoikatuVR.Fixes;
using KoikatuVR.Settings;
using Shared;
using Unity.XR.OpenVR;
using Valve.VR;

namespace KoikatuVR
{
    [BepInPlugin(GUID, PluginName, Version)]
    [BepInProcess(KoikatuAPI.GameProcessName)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)] //todo unnecessary?
    public class VRPlugin : BaseUnityPlugin
    {
        public const string GUID = "KKS_MainGameVR";
        public const string PluginName = "Main Game VR";
        public const string Version = Constants.Version;

        internal static new ManualLogSource Logger;

        private void Awake()
        {
            Logger = base.Logger;

            // could use SteamVRDetector.IsRunning and add Environment.CommandLine.Contains("--novr");
            var vrActivated = Environment.CommandLine.Contains("--vr");
            if (vrActivated)
            {
                OpenVRHelperTempfixHook.Patch();

                var settings = SettingsManager.Create(Config);
                StartCoroutine(LoadDevice(settings));
            }
        }

        private IEnumerator LoadDevice(KoikatuSettings settings)
        {
            yield return new WaitUntil(() => Manager.Scene.initialized && Manager.Scene.LoadSceneName == "Title");

            Logger.LogInfo("Loading OpenVR...");

            var ovrsettings = OpenVRSettings.GetSettings(true);
            ovrsettings.StereoRenderingMode = OpenVRSettings.StereoRenderingModes.MultiPass;
            ovrsettings.InitializationType = OpenVRSettings.InitializationTypes.Scene;
            ovrsettings.EditorAppKey = "kss.charastudio.exe";
            var instance = SteamVR_Settings.instance;
            instance.autoEnableVR = true;
            instance.editorAppKey = "kss.charastudio.exe";
            var openVRLoader = ScriptableObject.CreateInstance<OpenVRLoader>();
            if (!openVRLoader.Initialize())
            {
                Logger.LogInfo("Failed to Initialize OpenVR.");
                yield break;
            }

            if (!openVRLoader.Start())
            {
                Logger.LogInfo("Failed to Start OpenVR.");
                yield break;
            }

            Logger.LogInfo("Initializing SteamVR...");

            try
            {
                SteamVR_Behaviour.Initialize(false);
            }
            catch (Exception data)
            {
                Logger.LogError(data);
            }

            while (true)
            {
                var initializedState = SteamVR.initializedState;
                switch (initializedState)
                {
                    case SteamVR.InitializedStates.Initializing:
                        yield return new WaitForSeconds(0.1f);
                        continue;
                    case SteamVR.InitializedStates.InitializeSuccess:
                        break;
                    case SteamVR.InitializedStates.InitializeFailure:
                        Logger.LogInfo("Failed to initialize SteamVR.");
                        yield break;
                    default:
                        Logger.LogInfo($"Unknown SteamVR initializeState {initializedState}.");
                        yield break;
                }

                break;
            }

            Logger.LogInfo("Initializing the plugin...");

            new Harmony(GUID).PatchAll(typeof(VRPlugin).Assembly);
            // Boot VRManager!
            VRManager.Create<Interpreters.KoikatuInterpreter>(new KoikatuContext(settings));
            // VRGIN doesn't update the near clip plane until a first "main" camera is created, so we set it here.
            UpdateNearClipPlane(settings);
            settings.AddListener("NearClipPlane", (_, _1) => UpdateNearClipPlane(settings));
            VR.Manager.SetMode<KoikatuStandingMode>();
            VRFade.Create();
            PrivacyScreen.Initialize();
            GraphicRaycasterPatches.Initialize();
            // It's been reported in #28 that the game window defocues when
            // the game is under heavy load. We disable window ghosting in
            // an attempt to counter this.
            NativeMethods.DisableProcessWindowsGhosting();
            DontDestroyOnLoad(VRCamera.Instance.gameObject);

            Logger.LogInfo("Finished loading into VR mode!");
        }

        private void UpdateNearClipPlane(KoikatuSettings settings)
        {
            VR.Camera.gameObject.GetComponent<Camera>().nearClipPlane = settings.NearClipPlane;
        }

        private static class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern void DisableProcessWindowsGhosting();
        }
    }
}
