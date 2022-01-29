using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using VRGIN.Core;
using VRGIN.Visuals;

namespace KKS_VR.Settings
{
    [XmlRoot("Context")]
    public class CharaStudioContext : IVRManagerContext
    {
        private static string _contextSavePath = Path.Combine(BepInEx.Paths.ConfigPath, "KKS_CharaStudioVRContext.xml");
        private static string _settingsSavePath = Path.Combine(BepInEx.Paths.ConfigPath, "KKS_CharaStudioVRSettings.xml");

        private DefaultMaterialPalette _Materials;

        public CharaStudioContext()
        {
            _Materials = new DefaultMaterialPalette();
            Settings = CharaStudioSettings.Load(_settingsSavePath);
            ConfineMouse = true;
            EnforceDefaultGUIMaterials = false;
            GUIAlternativeSortingMode = false;
            GuiLayer = "Default";
            GuiFarClipPlane = 1000f;
            GuiNearClipPlane = -1000f;
            IgnoreMask = 0;
            InvisibleLayer = "Ignore Raycast";
            PrimaryColor = Color.cyan;
            SimulateCursor = true;
            UILayer = "UI";
            UILayerMask = LayerMask.GetMask(UILayer);
            UnitToMeter = 1f;
            NearClipPlane = 0.001f;
            PreferredGUI = GUIType.uGUI;
        }

        public string Version { get; set; }

        public float MaxFarClipPlane { get; set; }

        public int GuiMaterialRenderQueue { get; set; }

        [XmlIgnore] public IMaterialPalette Materials => _Materials;

        [XmlIgnore] public VRSettings Settings { get; }

        public bool ConfineMouse { get; set; }

        public bool EnforceDefaultGUIMaterials { get; set; }

        public bool GUIAlternativeSortingMode { get; set; }

        public float GuiFarClipPlane { get; set; }

        public string GuiLayer { get; set; }

        public float GuiNearClipPlane { get; set; }

        public int IgnoreMask { get; set; }

        public string InvisibleLayer { get; set; }

        public Color PrimaryColor { get; set; }

        public bool SimulateCursor { get; set; }

        public string UILayer { get; set; }

        public int UILayerMask { get; set; }

        public float UnitToMeter { get; set; }

        public float NearClipPlane { get; set; }

        public GUIType PreferredGUI { get; set; }

        Type IVRManagerContext.VoiceCommandType { get; }

        public bool ForceIMGUIOnScreen { get; set; }

        public static IVRManagerContext GetContext()
        {
            var path = _contextSavePath;
            var xmlSerializer = new XmlSerializer(typeof(CharaStudioContext));
            if (File.Exists(path))
            {
                using var stream = File.OpenRead(path);
                try
                {
                    return xmlSerializer.Deserialize(stream) as CharaStudioContext;
                }
                catch (Exception)
                {
                    VRLog.Error("Failed to deserialize {0} -- using default", path);
                }
            }

            var configurableContext = new CharaStudioContext();
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
    }
}
