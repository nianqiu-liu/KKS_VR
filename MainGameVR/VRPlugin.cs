using BepInEx;
using System;
using VRGIN.Helpers;
using VRGIN.Core;
using VRGIN.Native;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using HarmonyLib;
using System.Runtime.InteropServices;
using BepInEx.Logging;
using KKAPI;
using Unity.XR.OpenVR;
using UnityEngine.XR;
using Valve.VR;

namespace KoikatuVR
{
    [BepInPlugin(GUID: GUID, Name: PluginName, Version: Version)]
    [BepInProcess(KoikatuAPI.GameProcessName)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)] //todo unnecessary?
    public class VRPlugin : BaseUnityPlugin
    {
        public const string GUID = "KKS_MainGameVR";
        public const string PluginName = "Main Game VR";
        public const string Version = "0.0.3";

        internal static new ManualLogSource Logger;

        void Awake()
        {
            Logger = base.Logger;

            //VRLog.Backend = new BepInExLoggerBackend(Logger);
            //bool vrDeactivated = Environment.CommandLine.Contains("--novr");
            bool vrActivated = Environment.CommandLine.Contains("--vr");

            bool enabled = vrActivated;// || (!vrDeactivated && SteamVRDetector.IsRunning);
            if (enabled)
            {
                KKSCharaStudioVR.OpenVRHelperTempfixHook.Patch();

                var settings = SettingsManager.Create(Config);
                StartCoroutine(LoadDevice(enabled, settings));
            }
        }

        private const string DeviceOpenVR = "OpenVR";
        private const string DeviceNone = "None";

        IEnumerator LoadDevice(bool vrMode, KoikatuSettings settings)
        {
            yield return new WaitUntil(() => Manager.Scene.initialized && Manager.Scene.LoadSceneName == "Title");

            var newDevice = vrMode ? DeviceOpenVR : DeviceNone;
            Logger.LogInfo("Loading Device " + newDevice);

            //if (XRSettings.loadedDeviceName != newDevice)
            //{
            //    // 指定されたデバイスの読み込み.
            //    XRSettings.LoadDeviceByName(newDevice);
            //    // 次のフレームまで待つ.
            //    yield return null;
            //}
            //// VRモードを有効にする.
            //XRSettings.enabled = vrMode;
            //// 次のフレームまで待つ.
            //yield return null;
            //
            //// デバイスの読み込みが完了するまで待つ.
            //while (XRSettings.loadedDeviceName != newDevice || XRSettings.enabled != vrMode)
            //{
            //    yield return null;
            //}
            //
            //while (true)
            //{
            //    var rect = WindowManager.GetClientRect();
            //    if (rect.Right - rect.Left > 0)
            //    {
            //        break;
            //    }
            //    VRLog.Info("waiting for the window rect to be non-empty");
            //    yield return null;
            //}
            //VRLog.Info("window rect is not empty!");

            if (vrMode)
            {
                // added from charastudio vr
                OpenVRSettings ovrsettings = OpenVRSettings.GetSettings(true);
                ovrsettings.StereoRenderingMode = OpenVRSettings.StereoRenderingModes.MultiPass;
                ovrsettings.InitializationType = OpenVRSettings.InitializationTypes.Scene;
                ovrsettings.EditorAppKey = "kss.charastudio.exe";
                SteamVR_Settings instance = SteamVR_Settings.instance;
                instance.autoEnableVR = true;
                instance.editorAppKey = "kss.charastudio.exe";
                OpenVRLoader openVRLoader = ScriptableObject.CreateInstance<OpenVRLoader>();
                if (!openVRLoader.Initialize())
                {
                    Logger.LogInfo("Failed to Initialize " + newDevice + ".");
                    yield break;
                }
                if (!openVRLoader.Start())
                {
                    Logger.LogInfo("Failed to Start " + newDevice + ".");
                    yield break;
                }
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
                    SteamVR.InitializedStates initializedState = SteamVR.initializedState;
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
                            Logger.LogInfo($"Unknow SteamVR initializeState {initializedState}.");
                            yield break;
                    }
                    break;
                }
                Logger.LogInfo("Steam VR initialization completed.");

                // original kk vr
                new Harmony(VRPlugin.GUID).PatchAll();
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
				UnityEngine.Object.DontDestroyOnLoad(VRCamera.Instance.gameObject);
            }
        }

        private void UpdateNearClipPlane(KoikatuSettings settings)
        {
            VR.Camera.gameObject.GetComponent<Camera>().nearClipPlane = settings.NearClipPlane;
        }
    }

    class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern void DisableProcessWindowsGhosting();
    }
}
