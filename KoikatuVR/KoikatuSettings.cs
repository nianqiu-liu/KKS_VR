using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine;
using VRGIN.Controls;
using VRGIN.Core;
using VRGIN.Helpers;
using static VRGIN.Visuals.GUIMonitor;

namespace KoikatuVR
{
    /// <summary>
    /// User settings. SettingsManager is responsible for updating this.
    /// </summary>
    public class KoikatuSettings : VRSettings
    {
        public List<KeySet> KeySets { get { return _KeySets; } set { _KeySets = value; TriggerPropertyChanged("KeySets"); } }
        private List<KeySet> _KeySets = null;

        public List<KeySet> HKeySets { get { return _HKeySets; } set { _HKeySets = value; TriggerPropertyChanged("HKeySets"); } }
        private List<KeySet> _HKeySets = null;

        public bool UsingHeadPos { get { return _UsingHeadPos; } set { _UsingHeadPos = value; } }
        private bool _UsingHeadPos = false;

        public float StandingCameraPos { get { return _StandingCameraPos; } set { _StandingCameraPos = value; } }
        private float _StandingCameraPos = 1.5f;

        public float CrouchingCameraPos { get { return _CrouchingCameraPos; } set { _CrouchingCameraPos = value; } }
        private float _CrouchingCameraPos = 0.7f;

        public bool CrouchByHMDPos { get { return _CrouchByHMDPos; } set { _CrouchByHMDPos = value; } }
        private bool _CrouchByHMDPos = true;

        public float CrouchThrethould { get { return _CrouchThrethould; } set { _CrouchThrethould = value; } }
        private float _CrouchThrethould = 0.15f;

        public float StandUpThrethould { get { return _StandUpThrethould; } set { _StandUpThrethould = value; } }
        private float _StandUpThrethould = -0.55f;

        public float RotationAngle { get { return _RotationAngle; } set { _RotationAngle = value; } }
        private float _RotationAngle = 45f;

        public bool AutomaticTouching { get { return _AutomaticTouching; } set { _AutomaticTouching = value; } }
        private bool _AutomaticTouching = false;

        public bool AutomaticTouchingByHmd { get; set; } = false;

        public bool AutomaticKissing { get { return _AutomaticKissing; } set { _AutomaticKissing = value; } }
        private bool _AutomaticKissing = false;

        public bool AutomaticLicking { get; set; }

        public bool FirstPersonADV { get; set; }
    }

    public class KeySet
    {
        public KeySet(
            AssignableFunction trigger,
            AssignableFunction grip,
            AssignableFunction Up,
            AssignableFunction Down,
            AssignableFunction Right,
            AssignableFunction Left,
            AssignableFunction Center)
        {
            this.Trigger = trigger;
            this.Grip = grip;
            this.Up = Up;
            this.Down = Down;
            this.Right = Right;
            this.Left = Left;
            this.Center = Center;
        }

        public AssignableFunction Trigger { get; set; }

        public AssignableFunction Grip { get; set; }

        public AssignableFunction Up { get; set; }

        public AssignableFunction Down { get; set; }

        public AssignableFunction Right { get; set; }

        public AssignableFunction Left { get; set; }

        public AssignableFunction Center { get; set; }
    }
    public enum AssignableFunction
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
        [Description("Grab space to move")]
        GRAB,
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
