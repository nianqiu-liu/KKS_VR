using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using System.Collections;
using VRGIN.Core;
using VRGIN.Visuals;
using KoikatuVR.Interpreters;
using HarmonyLib;
using VRGIN.Native;

namespace KoikatuVR
{
	class VRLoader : ProtectedBehaviour
	{
		private static string DeviceOpenVR = "OpenVR";
		private static string DeviceNone = "None";

		private static bool _isVREnable = false;
		private static VRLoader _Instance;
		public static VRLoader Instance
		{
			get
			{
				if (_Instance == null)
				{
					throw new InvalidOperationException("VR Loader has not been created yet!");
				}
				return _Instance;
			}
		}

		public static VRLoader Create(bool isEnable)
		{
			_isVREnable = isEnable;
			_Instance = new GameObject("VRLoader").AddComponent<VRLoader>();

			return _Instance;
		}

		protected override void OnAwake()
		{
			if (_isVREnable)
			{
				StartCoroutine(LoadDevice(DeviceOpenVR));
			}
			else
			{
				StartCoroutine(LoadDevice(DeviceNone));
			}
		}

		/// <summary>
		/// VRデバイスのロード。
		/// </summary>
		IEnumerator LoadDevice(string newDevice)
		{
			bool vrMode = newDevice != DeviceNone;

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
				VRManager.Create<KoikatuInterpreter>(new KoikatuContext());
				// VRGIN doesn't update the near clip plane until a first "main" camera is created, so we set it here.
				VR.Camera.gameObject.GetComponent<Camera>().nearClipPlane = VR.Context.NearClipPlane;
				VR.Manager.SetMode<GenericStandingMode>();
				VRFade.Create();
			}
		}
	}
}
