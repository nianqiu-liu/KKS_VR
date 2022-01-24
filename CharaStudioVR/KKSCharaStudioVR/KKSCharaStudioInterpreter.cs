using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using Manager;
using Studio;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRGIN.Core;

namespace KKSCharaStudioVR
{
	internal class KKSCharaStudioInterpreter : GameInterpreter
	{
		private List<KKSCharaStudioActor> _Actors = new List<KKSCharaStudioActor>();

		private Camera _SubCamera;

		private StudioScene studioScene;

		private int additionalCullingMask;

		private GameObject CommonSpaceGo;

		public override IEnumerable<IActor> Actors => _Actors.Cast<IActor>();

		protected override void OnAwake()
		{
			base.OnAwake();
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
		{
			if (!CommonSpaceGo)
			{
				CommonSpaceGo = Manager.Scene.commonSpace;
			}
			FixMenuCanvasLayers();
		}

		protected override void OnStart()
		{
			base.OnStart();
			studioScene = UnityEngine.Object.FindObjectOfType<StudioScene>();
			FixMenuCanvasLayers();
		}

		public override Camera FindCamera()
		{
			return null;
		}

		public override IActor FindNextActorToImpersonate()
		{
			List<IActor> list = Actors.ToList();
			IActor actor = FindImpersonatedActor();
			if (actor == null)
			{
				return list.FirstOrDefault();
			}
			return list[(list.IndexOf(actor) + 1) % list.Count];
		}

		protected override void OnUpdate()
		{
			try
			{
				if ((bool)VR.Manager)
				{
					RefreshActors();
					UpdateMainCameraCullingMask();
					SyncVRCameraSkybox();
					FixMissingMode();
				}
			}
			catch (Exception)
			{
			}
		}

		private void FixMissingMode()
		{
			if (VR.Mode == null)
			{
				VRLog.Error("VR.Mode is missing. Force reload as Standing Mode.");
				ForceResetAsStandingMode();
			}
		}

		private void SyncVRCameraSkybox()
		{
			if ((bool)VR.Camera)
			{
				VR.Camera.SyncSkybox();
			}
		}

		private void UpdateMainCameraCullingMask()
		{
			Camera component = VR.Camera.SteamCam.GetComponent<Camera>();
			List<string> list = new List<string>();
			List<string> obj = new List<string> { "Studio/Col", "Studio/Select" };
			if (Singleton<global::Studio.Studio>.Instance.workInfo.visibleAxis)
			{
				if (global::Studio.Studio.optionSystem.selectedState == 0)
				{
					list.Add("Studio/Col");
				}
				list.Add("Studio/Select");
			}
			int mask = LayerMask.GetMask(list.ToArray());
			int mask2 = LayerMask.GetMask(obj.ToArray());
			component.cullingMask &= ~mask2;
			component.cullingMask |= mask;
		}

		private void RefreshActors()
		{
			_Actors.Clear();
			foreach (ChaControl value in Character.dictEntryChara.Values)
			{
				if ((bool)value.objBodyBone)
				{
					AddActor(DefaultActorBehaviour<ChaControl>.Create<KKSCharaStudioActor>(value));
				}
			}
		}

		private void AddActor(KKSCharaStudioActor actor)
		{
			if (!actor.Eyes)
			{
				actor.Head.Reinitialize();
			}
			else
			{
				_Actors.Add(actor);
			}
		}

		public void ForceResetVRMode()
		{
			StartCoroutine(ForceResetVRModeCo());
		}

		private IEnumerator ForceResetVRModeCo()
		{
			KKSCharaStudioVRPlugin.PluginLogger.Log(LogLevel.Debug, "Check and reset to StandingMode if not.");
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			if (!VRManager.Instance.Mode)
			{
				KKSCharaStudioVRPlugin.PluginLogger.Log(LogLevel.Debug, "Mode is not StandingMode. Force reset as Standing Mode.");
				ForceResetAsStandingMode();
			}
			else
			{
				KKSCharaStudioVRPlugin.PluginLogger.Log(LogLevel.Debug, "Is Standing Mode. Skip to setting force.");
			}
		}

		public static void DisableDefaultAudioListener()
		{
			if (SingletonInitializer<Manager.Sound>.instance != null)
			{
				AudioListener componentInChildren = SingletonInitializer<Manager.Sound>.instance.transform.GetComponentInChildren<AudioListener>(true);
				if (componentInChildren != null)
				{
					componentInChildren.enabled = false;
				}
			}
		}

		public static void ForceResetAsStandingMode()
		{
			try
			{
				VR.Manager.SetMode<GenericStandingMode>();
				if ((bool)VR.Camera)
				{
					_ = VR.Camera.Blueprint;
					Camera mainCmaera = Singleton<global::Studio.Studio>.Instance.cameraCtrl.mainCmaera;
					KKSCharaStudioVRPlugin.PluginLogger.Log(LogLevel.Debug, $"Force replace blueprint camera with {mainCmaera}");
					Camera camera = VR.Camera.SteamCam.camera;
					Camera camera2 = mainCmaera;
					camera.nearClipPlane = VR.Context.NearClipPlane;
					camera.farClipPlane = Mathf.Max(camera2.farClipPlane, 10f);
					camera.clearFlags = ((camera2.clearFlags == CameraClearFlags.Skybox) ? CameraClearFlags.Skybox : CameraClearFlags.Color);
					camera.renderingPath = camera2.renderingPath;
					camera.clearStencilAfterLightingPass = camera2.clearStencilAfterLightingPass;
					camera.depthTextureMode = camera2.depthTextureMode;
					camera.layerCullDistances = camera2.layerCullDistances;
					camera.layerCullSpherical = camera2.layerCullSpherical;
					camera.useOcclusionCulling = camera2.useOcclusionCulling;
					camera.allowHDR = camera2.allowHDR;
					camera.backgroundColor = camera2.backgroundColor;
					Skybox component = camera2.GetComponent<Skybox>();
					if (component != null)
					{
						Skybox skybox = camera.gameObject.GetComponent<Skybox>();
						if (skybox == null)
						{
							skybox = skybox.gameObject.AddComponent<Skybox>();
						}
						skybox.material = component.material;
					}
					VR.Camera.CopyFX(camera2);
					AmplifyColorEffect component2 = VR.Camera.gameObject.GetComponent<AmplifyColorEffect>();
					if ((bool)component2)
					{
						component2.enabled = true;
					}
				}
				else
				{
					KKSCharaStudioVRPlugin.PluginLogger.Log(LogLevel.Debug, "VR.Camera is null");
				}
			}
			catch (Exception value)
			{
				Console.WriteLine(value);
			}
		}

		public void FixMenuCanvasLayers()
		{
			foreach (Canvas item in ((IDictionary)typeof(VRGUI).GetField("_Registry", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(VRGUI.Instance)).Keys as ICollection<Canvas>)
			{
				if (!item.transform.IsChildOf(VR.Camera.Origin.transform) && !IsIgnoredCanvas(item))
				{
					Transform[] componentsInChildren = item.GetComponentsInChildren<Transform>(true);
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].gameObject.layer = LayerMask.NameToLayer(VR.Context.UILayer);
					}
				}
			}
		}

		public override bool IsIgnoredCanvas(Canvas canvas)
		{
			if ((bool)CommonSpaceGo && canvas.transform.IsChildOf(CommonSpaceGo.transform))
			{
				return true;
			}
			return base.IsIgnoredCanvas(canvas);
		}
	}
}
