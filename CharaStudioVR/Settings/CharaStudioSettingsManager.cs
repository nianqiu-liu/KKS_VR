using System;
using BepInEx.Configuration;
using KKAPI.Utilities;
using VRGIN.Core;

namespace KKS_VR.Settings
{
    /// <summary>
    /// Manages configuration and keeps it up to date.
    /// 
    /// BepInEx wants us to store the config in a bunch of ConfigEntry objects,
    /// but VRGIN wants it stored inside a class inheriting VRSettings. So
    /// our plan is:
    /// 
    /// * We have both ConfigEntry objects and CharaStudioSettings around.
    /// * The ConfigEntry objects are the master copy and the CharaStudioSettings
    ///   object is a mirror.
    /// * CharaStudioSettingsManager is responsible for keeping CharaStudioSettings up to date.
    /// * No other parts of code should modify CharaStudioSettings. In fact, there
    ///   are code paths where VRGIN tries to modify it. We simply attempt
    ///   to avoid executing those code paths.
    /// </summary>
    internal class CharaStudioSettingsManager
    {
        public const string SectionGeneral = "0. General";

        /// <summary>
        /// Create config entries under the given ConfigFile. Also create a fresh
        /// CharaStudioSettings object and arrange that it be synced with the config
        /// entries.
        /// </summary>
        /// <returns>The new CharaStudioSettings object.</returns>
        public static CharaStudioSettings Create(ConfigFile config)
        {
            var settings = new CharaStudioSettings();

            var ipdScale = config.Bind(SectionGeneral, "IPD Scale", 1f,
                new ConfigDescription(
                    "Scale of the camera. The higher, the more gigantic the player is.",
                    new AcceptableValueRange<float>(0.25f, 4f)));
            Tie(ipdScale, v => settings.IPDScale = v);

            var rumble = config.Bind(SectionGeneral, "Rumble", true,
                "Whether or not rumble is activated.");
            Tie(rumble, v => settings.Rumble = v);

            var rotationMultiplier = config.Bind(SectionGeneral, "Rotation multiplier", 1f,
                new ConfigDescription(
                    "How quickly the the view should rotate when doing so with the controllers.",
                    new AcceptableValueRange<float>(-4f, 4f),
                    new ConfigurationManagerAttributes { Order = -1 }));
            Tie(rotationMultiplier, v => settings.RotationMultiplier = v);

            var logLevel = config.Bind(SectionGeneral, "Log level", VRLog.LogMode.Info,
                new ConfigDescription(
                    "The minimum severity for a message to be logged.",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = true }));
            Tie(logLevel, v => VRLog.Level = v);

            var nearClipPlane = config.Bind(SectionGeneral, "Near clip plane", 0.002f,
                new ConfigDescription(
                    "Minimum distance from camera for an object to be shown (causes visual glitches on some maps when set too small)",
                    new AcceptableValueRange<float>(0.001f, 0.2f)));
            Tie(nearClipPlane, v => settings.NearClipPlane = v);

            // not used for anything
            var lockRotXZ = config.Bind(SectionGeneral, "Lock XZ Axis rotation", true,
                new ConfigDescription("Lock XZ Axis (pitch / roll) rotation."));
            Tie(lockRotXZ, v => settings.LockRotXZ = v);

            var maxVoiceDistance = config.Bind(SectionGeneral, "Max Voice distance", 300f,
                new ConfigDescription(
                    "Max Voice distance (in unit. 300 = 30m in real (HS2 uses 10 unit = 1m scale).",
                    new AcceptableValueRange<float>(100f, 600f)));
            Tie(maxVoiceDistance, v => settings.MaxVoiceDistance = v);

            var minVoiceDistance = config.Bind(SectionGeneral, "Min Voice distance", 7f,
                new ConfigDescription(
                    "Min Voice distance (in unit. 7 = 70 cm in real (HS2 uses 10 unit = 1m scale).",
                    new AcceptableValueRange<float>(1f, 70f)));
            Tie(minVoiceDistance, v => settings.MinVoiceDistance = v);

            return settings;
        }

        private static void Tie<T>(ConfigEntry<T> entry, Action<T> set)
        {
            set(entry.Value);
            entry.SettingChanged += (_, _1) => set(entry.Value);
        }
    }
}
