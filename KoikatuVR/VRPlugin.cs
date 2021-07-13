using BepInEx;
using System;
using VRGIN.Helpers;

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

            if (vrActivated || (!vrDeactivated && SteamVRDetector.IsRunning))
            {
				VRLoader.Create(true, settings);
            }
			else
			{
				VRLoader.Create(false, settings);
            }
        }
    }
}
