using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;
using VRGIN.Core;

namespace KKSCharaStudioVR
{
    internal class VRLoader : ProtectedBehaviour
    {
        private static string DeviceOpenVR = "OpenVR";

        private static string DeviceNone = "None";

        private static bool _isVREnable = false;

        private static VRLoader _Instance;

        public static VRLoader Instance
        {
            get
            {
                if (_Instance == null) throw new InvalidOperationException("VR Loader has not been created yet!");
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
                StartCoroutine(LoadDevice(DeviceOpenVR));
            else
                StartCoroutine(LoadDevice(DeviceNone));
        }

        private IVRManagerContext CreateContext(string path)
        {
            var xmlSerializer = new XmlSerializer(typeof(ConfigurableContext));
            if (File.Exists(path))
            {
                using var stream = File.OpenRead(path);
                try
                {
                    return xmlSerializer.Deserialize(stream) as ConfigurableContext;
                }
                catch (Exception)
                {
                    VRLog.Error("Failed to deserialize {0} -- using default", path);
                }
            }

            var configurableContext = new ConfigurableContext();
            try
            {
                using var streamWriter = new StreamWriter(path);
                streamWriter.BaseStream.SetLength(0L);
                xmlSerializer.Serialize(streamWriter, configurableContext);
                return configurableContext;
            }
            catch (Exception)
            {
                VRLog.Error("Failed to write {0}", path);
                return configurableContext;
            }
        }

        private IEnumerator LoadDevice(string newDevice)
        {
            KKSCharaStudioVRPlugin.PluginLogger.LogInfo("Loading Device " + newDevice);
            var vrMode = newDevice != DeviceNone;
            if (vrMode)
            {
                var settings = OpenVRSettings.GetSettings(true);
                settings.StereoRenderingMode = OpenVRSettings.StereoRenderingModes.MultiPass;
                settings.InitializationType = OpenVRSettings.InitializationTypes.Scene;
                settings.EditorAppKey = "kss.charastudio.exe";
                var instance = SteamVR_Settings.instance;
                instance.autoEnableVR = true;
                instance.editorAppKey = "kss.charastudio.exe";
                var openVRLoader = ScriptableObject.CreateInstance<OpenVRLoader>();
                if (!openVRLoader.Initialize())
                {
                    KKSCharaStudioVRPlugin.PluginLogger.LogInfo("Failed to Initialize " + newDevice + ".");
                    yield break;
                }

                if (!openVRLoader.Start())
                {
                    KKSCharaStudioVRPlugin.PluginLogger.LogInfo("Failed to Start " + newDevice + ".");
                    yield break;
                }

                try
                {
                    SteamVR_Behaviour.Initialize(false);
                }
                catch (Exception data)
                {
                    KKSCharaStudioVRPlugin.PluginLogger.LogError(data);
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
                            KKSCharaStudioVRPlugin.PluginLogger.LogInfo("Failed to initialize SteamVR.");
                            yield break;
                        default:
                            KKSCharaStudioVRPlugin.PluginLogger.LogInfo($"Unknow SteamVR initializeState {initializedState}.");
                            yield break;
                    }

                    break;
                }

                KKSCharaStudioVRPlugin.PluginLogger.LogInfo("Steam VR initialization completed.");
            }

            if (vrMode)
            {
                VRManager.Create<KKSCharaStudioInterpreter>(CreateContext("KKS_CharaStudioVRContext.xml"));
                VR.Manager.SetMode<GenericStandingMode>();
                var obj = new GameObject("KKSCharaStudioVR");
                DontDestroyOnLoad(obj);
                IKTool.Create(obj);
                VRControllerMgr.Install(obj);
                VRCameraMoveHelper.Install(obj);
                VRItemObjMoveHelper.Install(obj);
                obj.AddComponent<KKSCharaStudioVRGUI>();
                DontDestroyOnLoad(VRCamera.Instance.gameObject);
            }
        }

        private IEnumerator LoadDevice_XR_Management(string newDevice)
        {
            var vrMode = newDevice != DeviceNone;
            XRSettings.LoadDeviceByName(newDevice);
            yield return null;
            XRSettings.enabled = vrMode;
            yield return null;
            while (XRSettings.loadedDeviceName != newDevice || XRSettings.enabled != vrMode) yield return null;
            var list = new List<XRNodeState>();
            InputTracking.GetNodeStates(list);
            foreach (var item in list)
            {
                var nodeName = InputTracking.GetNodeName(item.uniqueID);
                var position = default(Vector3);
                var flag = item.TryGetPosition(out position);
                VRLog.Info("XRNode {0}, position available {1} {2}", nodeName, flag, position);
            }

            if (vrMode)
            {
                VRManager.Create<KKSCharaStudioInterpreter>(CreateContext("KKS_CharaStudioVRContext.xml"));
                VR.Manager.SetMode<GenericStandingMode>();
                var obj = new GameObject("KKSCharaStudioVR");
                DontDestroyOnLoad(obj);
                IKTool.Create(obj);
                VRControllerMgr.Install(obj);
                VRCameraMoveHelper.Install(obj);
                VRItemObjMoveHelper.Install(obj);
                obj.AddComponent<KKSCharaStudioVRGUI>();
                DontDestroyOnLoad(VRCamera.Instance.gameObject);
            }
        }
    }
}
