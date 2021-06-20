using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.Configuration;
using System.ComponentModel;

namespace KoikatuVR
{
    /// <summary>
    /// Manages configuration and keeps it up to date.
    /// 
    /// BepInEx wants us to store the config in a bunch of ConfigEntry objects,
    /// but VRGIN wants it stored inside a class inheriting VRSettings. So
    /// our plan is:
    /// 
    /// * We have both ConfigEntry objects and KoikatuSettings around.
    /// * The ConfigEntry objects are the master copy and the KoikatuSettings
    ///   object is a mirror.
    /// * SettingsManager is responsible for keeping KoikatuSettings up to date.
    /// * No other parts of code should modify KoikatuSettings. In fact, there
    ///   are code paths where VRGIN tries to modify it. We simply attempt
    ///   to avoid executing those code paths.
    /// </summary>
    class SettingsManager
    {
        /// <summary>
        /// Create config entries under the given ConfigFile. Also create a fresh
        /// KoikatuSettings object and arrange that it be synced with the config
        /// entries.
        /// </summary>
        /// <param name="config"></param>
        /// <returns>The new KoikatuSettings object.</returns>
        public static KoikatuSettings Create(ConfigFile config)
        {
            var settings = new KoikatuSettings();

            const string sectionGeneral = "0. General";
            const string sectionRoaming = "1. Roaming";
            const string sectionCaress = "1. Caress";

            var ipdScale = config.Bind(sectionGeneral, "IPD Scale", 1f,
                new ConfigDescription(
                    "Scale of the camera. The higher, the more gigantic the player is.",
                    new AcceptableValueRange<float>(0.25f, 4f)));
            Tie(ipdScale, v => settings.IPDScale = v);

            var rumble = config.Bind(sectionGeneral, "Rumble", true,
                "Whether or not rumble is activated.");
            Tie(rumble, v => settings.Rumble = v);

            var grabRotationImmediateMode = config.Bind(sectionGeneral, "Immediate rotation", true,
                new ConfigDescription(
                    "Determines the rotation mode. If enabled, pulling the trigger while grabbing will immediately rotate you. When disabled, doing the same thing will let you 'drag' the view.",
                    null,
                    new ConfigurationManagerAttributes { Order = -1 }));
            Tie(grabRotationImmediateMode, v => settings.GrabRotationImmediateMode = v);

            var rotationMultiplier = config.Bind(sectionGeneral, "Rotation multiplier", 1f,
                new ConfigDescription(
                    "How quickly the the view should rotate when doing so with the controllers.",
                    new AcceptableValueRange<float>(-4f, 4f),
                    new ConfigurationManagerAttributes { Order = -1 }));
            Tie(rotationMultiplier, v => settings.RotationMultiplier = v);

            var touchpadThreshold = config.Bind(sectionGeneral, "Touchpad direction threshold", 0.8f,
                new ConfigDescription(
                    "Touchpad presses within this radius are considered center clicks rather than directional ones.",
                    new AcceptableValueRange<float>(0f, 1f)));
            Tie(touchpadThreshold, v => settings.TouchpadThreshold = v);

            var usingHeadPos = config.Bind(sectionRoaming, "Use head position", false,
                new ConfigDescription(
                    "Place the camera exactly at the protagonist's head (may cause motion sickness). If disabled, use a fixed height from the floor.",
                    null,
                    new ConfigurationManagerAttributes { Order = -1 }));
            Tie(usingHeadPos, v => settings.UsingHeadPos = v);

            var standingCameraPos = config.Bind(sectionRoaming, "Camera height", 1.5f,
                new ConfigDescription(
                    "Default camera height for when not using the head position.",
                    new AcceptableValueRange<float>(0.2f, 3f),
                    new ConfigurationManagerAttributes { Order = -2 }));
            Tie(standingCameraPos, v => settings.StandingCameraPos = v);

            var crouchingCameraPos = config.Bind(sectionRoaming, "Crouching camera height", 0.7f,
                new ConfigDescription(
                    "Crouching camera height for when not using the head position",
                    new AcceptableValueRange<float>(0.2f, 3f),
                    new ConfigurationManagerAttributes { Order = -2 }));
            Tie(crouchingCameraPos, v => settings.CrouchingCameraPos = v);

            var crouchByHMDPos = config.Bind(sectionRoaming, "Crouch by HMD position", true,
                new ConfigDescription(
                    "Crouch when the HMD position is below some threshold.",
                    null,
                    new ConfigurationManagerAttributes { Order = -3 }));
            Tie(crouchByHMDPos, v => settings.CrouchByHMDPos = v);

            var crouchThreshold = config.Bind(sectionRoaming, "Crouch threshold", 0.15f,
                new ConfigDescription(
                    "Trigger crouching when the height difference between the camera and the HMD is this much",
                    new AcceptableValueRange<float>(0.05f, 3f),
                    new ConfigurationManagerAttributes { Order = -4 }));
            Tie(crouchThreshold, v => settings.CrouchThrethould = v);

            var standUpThreshold = config.Bind(sectionRoaming, "Stand up threshold", -0.55f,
                new ConfigDescription(
                    "End crouching when the hight difference between the camera and the HMD is this much",
                    new AcceptableValueRange<float>(-3f, -0.05f),
                    new ConfigurationManagerAttributes { Order = -4 }));
            Tie(standUpThreshold, v => settings.StandUpThrethould = v);

            var rotationAngle = config.Bind(sectionRoaming, "Rotation angle", 45f,
                new ConfigDescription(
                    "Angle of rotation, in degrees",
                    new AcceptableValueRange<float>(0f, 180f)));
            Tie(rotationAngle, v => settings.RotationAngle = v);

            var automaticTouching = config.Bind(sectionCaress, "Automatic touching", false,
                "Touching the female's body with controllers triggers reaction");
            Tie(automaticTouching, v => settings.AutomaticTouching = v);

            var automaticKissing = config.Bind(sectionCaress, "Automatic kissing", false,
                "Initiate kissing by moving the HMD");
            Tie(automaticKissing, v => settings.AutomaticKissing = v);

            KeySetsConfig keySetsConfig = null;
            void updateKeySets()
            {
                keySetsConfig.CurrentKeySets(out var keySets, out var hKeySets);
                settings.KeySets = keySets;
                settings.HKeySets = hKeySets;
            }
            
            keySetsConfig = new KeySetsConfig(config, updateKeySets);
            updateKeySets();

            return settings;
        }

        private static void Tie<T>(ConfigEntry<T> entry, Action<T> set)
        {
            set(entry.Value);
            entry.SettingChanged += (_, _1) => set(entry.Value);
        }
    }

    class KeySetsConfig
    {
        private readonly KeySetConfig _main;
        private readonly KeySetConfig _main1;
        private readonly KeySetConfig _h;
        private readonly KeySetConfig _h1;

        private readonly ConfigEntry<bool> _useMain1;
        private readonly ConfigEntry<bool> _useH1;

        public KeySetsConfig(ConfigFile config, Action onUpdate)
        {
            const string sectionP = "2. Non-H button assignments (primary)";
            const string sectionS = "2. Non-H button assignments (secondary)";
            const string sectionHP = "3. H button assignments (primary)";
            const string sectionHS = "3. H button assignments (secondary)";

            _main = new KeySetConfig(config, onUpdate, sectionP, isH: false, advanced: false);
            _main1 = new KeySetConfig(config, onUpdate, sectionS, isH: false, advanced: true);
            _h = new KeySetConfig(config, onUpdate, sectionHP, isH: true, advanced: false);
            _h1 = new KeySetConfig(config, onUpdate, sectionHS, isH: true, advanced: true);

            _useMain1 = config.Bind(sectionS, "Use secondary assignments", false,
                new ConfigDescription("", null, new ConfigurationManagerAttributes { IsAdvanced = true }));
            _useMain1.SettingChanged += (_, _1) => onUpdate();
            _useH1 = config.Bind(sectionHS, "Use secondary assignments", false,
                new ConfigDescription("", null, new ConfigurationManagerAttributes { IsAdvanced = true }));
            _useH1.SettingChanged += (_, _1) => onUpdate();
        }

        public void CurrentKeySets(out List<KeySet> keySets, out List<KeySet> hKeySets)
        {
            keySets = new List<KeySet>();
            keySets.Add(_main.CurrentKeySet());
            if (_useMain1.Value)
            {
                keySets.Add(_main1.CurrentKeySet());
            }

            hKeySets = new List<KeySet>();
            hKeySets.Add(_h.CurrentKeySet());
            if (_useH1.Value)
            {
                hKeySets.Add(_h1.CurrentKeySet());
            }
        }
    }

    class KeySetConfig
    {
        private readonly ConfigEntry<AssignableFunction> _trigger;
        private readonly ConfigEntry<AssignableFunction> _grip;
        private readonly ConfigEntry<AssignableFunction> _up;
        private readonly ConfigEntry<AssignableFunction> _down;
        private readonly ConfigEntry<AssignableFunction> _right;
        private readonly ConfigEntry<AssignableFunction> _left;
        private readonly ConfigEntry<AssignableFunction> _center;

        public KeySetConfig(ConfigFile config, Action onUpdate, string section, bool isH, bool advanced)
        {
            int order = -1;
            ConfigEntry<AssignableFunction> create(string name, AssignableFunction def)
            {
                var entry = config.Bind(section, name, def, new ConfigDescription("", null,
                    new ConfigurationManagerAttributes { Order = order, IsAdvanced = advanced }));
                entry.SettingChanged += (_, _1) => onUpdate();
                order -= 1;
                return entry;
            }
            if (isH)
            {
                _trigger = create("Trigger", AssignableFunction.LBUTTON);
                _grip = create("Grip", AssignableFunction.MBUTTON);
                _up = create("Up", AssignableFunction.SCROLLUP);
                _down = create("Down", AssignableFunction.SCROLLDOWN);
                _left = create("Left", AssignableFunction.NONE);
                _right = create("Right", AssignableFunction.NONE);
                _center = create("Center", AssignableFunction.RBUTTON);
            }
            else
            {
                _trigger = create("Trigger", AssignableFunction.WALK);
                _grip = create("Grip", AssignableFunction.MBUTTON);
                _up = create("Up", AssignableFunction.F3);
                _down = create("Down", AssignableFunction.PL2CAM);
                _left = create("Left", AssignableFunction.LROTATION);
                _right = create("Right", AssignableFunction.RROTATION);
                _center = create("Center", AssignableFunction.RBUTTON);
            }
        }

        public KeySet CurrentKeySet()
        {
            return new KeySet(
                trigger: _trigger.Value.ToString(),
                grip: _grip.Value.ToString(),
                Up: _up.Value.ToString(),
                Down: _down.Value.ToString(),
                Right: _right.Value.ToString(),
                Left: _left.Value.ToString(),
                Center: _center.Value.ToString());
        }

    }
    enum AssignableFunction
    {
        [Description("None")]
        NONE,
        [Description("Walk (Roam mode)")]
        WALK,
        [Description("Dash (Roam mode)")]
        DASH,
        [Description("Move protagonist to camera (Roam mode)")]
        PL2CAM,
        [Description("Turn left (Roam mode)")]
        LROTATION,
        [Description("Turn right (Roam mode)")]
        RROTATION,
        [Description("Crouch (Roam mode)")]
        CROUCH,
        [Description("Left mouse button")]
        LBUTTON,
        [Description("Right mouse button")]
        RBUTTON,
        [Description("Middle mouse button")]
        MBUTTON,
        [Description("Mouse wheel scroll up")]
        SCROLLUP,
        [Description("Mouse wheel scroll down")]
        SCROLLDOWN,
        [Description("Switch button assignments")]
        NEXT,
        [Description("Keyboard F1")]
        F1,
        [Description("Keyboard F2")]
        F2,
        [Description("Keyboard F3")]
        F3,
        [Description("Keyboard F4")]
        F4,
        [Description("Keyboard F5")]
        F5,
        [Description("Keyboard F6")]
        F6,
        [Description("Keyboard F7")]
        F7,
        [Description("Keyboard F8")]
        F8,
        [Description("Keyboard F9")]
        F9,
        [Description("Keyboard F10")]
        F10,
        [Description("Keyboard F11")]
        F11,
        [Description("Keyboard F12")]
        F12,
        [Description("Keyboard A")]
        VK_A,
        [Description("Keyboard B")]
        VK_B,
        [Description("Keyboard C")]
        VK_C,
        [Description("Keyboard D")]
        VK_D,
        [Description("Keyboard E")]
        VK_E,
        [Description("Keyboard F")]
        VK_F,
        [Description("Keyboard G")]
        VK_G,
        [Description("Keyboard H")]
        VK_H,
        [Description("Keyboard I")]
        VK_I,
        [Description("Keyboard J")]
        VK_J,
        [Description("Keyboard K")]
        VK_K,
        [Description("Keyboard L")]
        VK_L,
        [Description("Keyboard M")]
        VK_M,
        [Description("Keyboard N")]
        VK_N,
        [Description("Keyboard O")]
        VK_O,
        [Description("Keyboard P")]
        VK_P,
        [Description("Keyboard Q")]
        VK_Q,
        [Description("Keyboard R")]
        VK_R,
        [Description("Keyboard S")]
        VK_S,
        [Description("Keyboard T")]
        VK_T,
        [Description("Keyboard U")]
        VK_U,
        [Description("Keyboard V")]
        VK_V,
        [Description("Keyboard W")]
        VK_W,
        [Description("Keyboard X")]
        VK_X,
        [Description("Keyboard Y")]
        VK_Y,
        [Description("Keyboard Z")]
        VK_Z,
        [Description("Keyboard 0")]
        VK_0,
        [Description("Keyboard 1")]
        VK_1,
        [Description("Keyboard 2")]
        VK_2,
        [Description("Keyboard 3")]
        VK_3,
        [Description("Keyboard 4")]
        VK_4,
        [Description("Keyboard 5")]
        VK_5,
        [Description("Keyboard 6")]
        VK_6,
        [Description("Keyboard 7")]
        VK_7,
        [Description("Keyboard 8")]
        VK_8,
        [Description("Keyboard 9")]
        VK_9,
    }
}
