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

        public float TouchpadThreshold { get { return _TouchpadThreshold; } set { _TouchpadThreshold = value; } }
        private float _TouchpadThreshold = 0.8f;

        public float RotationAngle { get { return _RotationAngle; } set { _RotationAngle = value; } }
        private float _RotationAngle = 45f;

        public bool AutomaticTouching { get { return _AutomaticTouching; } set { _AutomaticTouching = value; } }
        private bool _AutomaticTouching = false;
    }

    public class KeySet
    {
        public KeySet()
        {
            Trigger = "WALK";
            Grip = "PL2CAM";
            Up = "F3";
            Down = "F4";
            Right = "RROTATION";
            Left = "LROTATION";
            Center = "RBUTTON";
        }

        public KeySet(string trigger, string grip, string Up, string Down, string Right, string Left, string Center)
        {
            this.Trigger = trigger;
            this.Grip = grip;
            this.Up = Up;
            this.Down = Down;
            this.Right = Right;
            this.Left = Left;
            this.Center = Center;
        }

        public String Trigger { get; set; }

        public String Grip { get; set; }

        public String Up { get; set; }

        public String Down { get; set; }

        public String Right { get; set; }

        public String Left { get; set; }

        public String Center { get; set; }
    }
}
