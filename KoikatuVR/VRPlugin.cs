using BepInEx;
using System;
using VRGIN.Helpers;
using VRGIN.Core;
using VRGIN.Native;
using System.Collections;
using UnityEngine;
using HarmonyLib;

namespace KoikatuVR
{

    /// <summary>
    /// This is an example for a VR plugin. At the same time, it also functions as a generic one.
    /// </summary>
    [BepInPlugin(GUID: GUID, Name: "Main Game VR", Version: "0.9.0")]
    [BepInProcess("Koikatu")]
    [BepInProcess("Koikatsu Party")]
    public class VRPlugin : BaseUnityPlugin
    {
        public const string GUID = "mosirnik.kk-main-game-vr";

        /// <summary>
        /// Determines when to boot the VR code. In most cases, it makes sense to do the check as described here.
        /// </summary>
        void Awake()
        {
            bool vrDeactivated = Environment.CommandLine.Contains("--novr");
            bool vrActivated = Environment.CommandLine.Contains("--vr");
            var settings = SettingsManager.Create(Config);

			bool enabled = vrActivated || (!vrDeactivated && SteamVRDetector.IsRunning);
			StartCoroutine(LoadDevice(enabled, settings));
        }

		private const string DeviceOpenVR = "OpenVR";
		private const string DeviceNone = "None";

		IEnumerator LoadDevice(bool vrMode, KoikatuSettings settings)
		{
			var newDevice = vrMode ? DeviceOpenVR : DeviceNone;

			if (UnityEngine.VR.VRSettings.loadedDeviceName != newDevice)
			{
				// 指定されたデバイスの読み込み.
				UnityEngine.VR.VRSettings.LoadDeviceByName(newDevice);
				// 次のフレームまで待つ.
				yield return null;
			}
			// VRモードを有効にする.
			UnityEngine.VR.VRSettings.enabled = vrMode;
			// 次のフレームまで待つ.
			yield return null;

			// デバイスの読み込みが完了するまで待つ.
			while (UnityEngine.VR.VRSettings.loadedDeviceName != newDevice || UnityEngine.VR.VRSettings.enabled != vrMode)
			{
				yield return null;
			}

			while (true)
			{
				var rect = WindowManager.GetClientRect();
				if (rect.Right - rect.Left > 0)
				{
					break;
				}
				VRLog.Info("waiting for the window rect to be non-empty");
				yield return null;
			}
			VRLog.Info("window rect is not empty!");

			if (vrMode)
			{
                new Harmony(VRPlugin.GUID).PatchAll();
				// Boot VRManager!
				// Note: Use your own implementation of GameInterpreter to gain access to a few useful operatoins
				// (e.g. characters, camera judging, colliders, etc.)
				VRManager.Create<Interpreters.KoikatuInterpreter>(new KoikatuContext(settings));
				// VRGIN doesn't update the near clip plane until a first "main" camera is created, so we set it here.
				VR.Camera.gameObject.GetComponent<Camera>().nearClipPlane = VR.Context.NearClipPlane;
				VR.Manager.SetMode<KoikatuStandingMode>();
				VRFade.Create();
			}
		}
    }
}
