using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRGIN.Controls;
using VRGIN.Controls.Tools;
using VRGIN.Core;
using VRGIN.Helpers;
using static SteamVR_Controller;
using WindowsInput.Native;
using KoikatuVR.Interpreters;

namespace KoikatuVR
{
    public class SchoolTool : Tool
    {
        private KoikatuInterpreter _Interpreter;
        private KoikatuSettings _Settings;
        private KeySet _KeySet;
        private int _KeySetIndex = 0;
        private bool _InHScene = false;

        // 手抜きのためNumpad方式で方向を保存
        private int _PrevTouchDirection = -1;
        private bool _Pl2Cam = false;

        /// <summary>
        /// The set of keys for which we've sent a down message but not a
        /// corresponding up message.
        /// </summary>
        private readonly HashSet<string> _SentUnmatchedDown = new HashSet<string>();

        private void ChangeKeySet()
        {
            List<KeySet> keySets = KeySets();

            _KeySetIndex = (_KeySetIndex + 1) % keySets.Count;
            _KeySet = keySets[_KeySetIndex];
        }

        private List<KeySet> KeySets()
        {
            return _InHScene ? _Settings.HKeySets : _Settings.KeySets;
        }

        private void ResetKeys()
        {
            SetScene(_InHScene);
        }

        private void SetScene(bool inHScene)
        {
            CleanupDowns();
            _InHScene = inHScene;
            var keySets = KeySets();
            _KeySetIndex = 0;
            _KeySet = keySets[0];
        }

        public override Texture2D Image
        {
            get
            {
                return UnityHelper.LoadImage("icon_school.png");
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();

            _Settings = (VR.Context.Settings as KoikatuSettings);
            SetScene(inHScene: false);
            _Settings.AddListener("KeySets", (_, _1) => ResetKeys());
            _Settings.AddListener("HKeySets", (_, _1) => ResetKeys());
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnDestroy()
        {
            // nothing to do.
        }

        protected override void OnDisable()
        {
            CleanupDowns();
            base.OnDisable();
        }

        /// <summary>
        /// Send PressUp for keys we sent PressDown for, so that no key
        /// is left pressed indefinitely.
        /// </summary>
        private void CleanupDowns()
        {
            // Make a copy because the loop below will modify the HashSet.
            var todo = _SentUnmatchedDown.ToList();
            foreach (var key in todo)
            {
                InputKey(key, KeyMode.PressUp);
            }
            _SentUnmatchedDown.Clear();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _Interpreter = VR.Interpreter as KoikatuInterpreter;
        }

        protected override void OnLevel(int level)
        {
            base.OnLevel(level);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            var device = this.Controller;

            var inHScene = _Interpreter.CurrentScene == KoikatuInterpreter.HScene;
            if (inHScene != _InHScene)
            {
                SetScene(inHScene);
            }

            if (device.GetPressDown(ButtonMask.Trigger))
            {
                InputKey(_KeySet.Trigger, KeyMode.PressDown);
            }

            if (device.GetPressUp(ButtonMask.Trigger))
            {
                InputKey(_KeySet.Trigger, KeyMode.PressUp);
            }

            if (device.GetPressDown(ButtonMask.Grip))
            {
                InputKey(_KeySet.Grip, KeyMode.PressDown);
            }

            if (device.GetPressUp(ButtonMask.Grip))
            {
                InputKey(_KeySet.Grip, KeyMode.PressUp);
            }

            if (device.GetPressDown(ButtonMask.Touchpad))
            {
                Vector2 touchPosition = device.GetAxis();
                {
                    float threshold = _Settings.TouchpadThreshold;

                    if (touchPosition.y > threshold) // up
                    {
                        InputKey(_KeySet.Up, KeyMode.PressDown);
                        _PrevTouchDirection = 8;
                    }
                    else if (touchPosition.y < -threshold) // down
                    {
                        InputKey(_KeySet.Down, KeyMode.PressDown);
                        _PrevTouchDirection = 2;
                    }
                    else if (touchPosition.x > threshold) // right
                    {
                        InputKey(_KeySet.Right, KeyMode.PressDown);
                        _PrevTouchDirection = 6;
                    }
                    else if (touchPosition.x < -threshold)// left
                    {
                        InputKey(_KeySet.Left, KeyMode.PressDown);
                        _PrevTouchDirection = 4;
                    }
                    else
                    {
                        InputKey(_KeySet.Center, KeyMode.PressDown);
                        _PrevTouchDirection = 5;
                    }
                }
             }

            // 上げたときの位置によらず、押したボタンを離す
            if (device.GetPressUp(ButtonMask.Touchpad))
            {
                Vector2 touchPosition = device.GetAxis();
                {
                    if (_PrevTouchDirection == 8) // up
                    {
                        InputKey(_KeySet.Up, KeyMode.PressUp);
                    }
                    else if (_PrevTouchDirection == 2) // down
                    {
                        InputKey(_KeySet.Down, KeyMode.PressUp);
                    }
                    else if (_PrevTouchDirection == 6) // right
                    {
                        InputKey(_KeySet.Right, KeyMode.PressUp);
                    }
                    else if (_PrevTouchDirection == 4)// left
                    {
                        InputKey(_KeySet.Left, KeyMode.PressUp);
                    }
                    else if (_PrevTouchDirection == 5)
                    {
                        InputKey(_KeySet.Center, KeyMode.PressUp);
                    }
                }
            }

            if (_Pl2Cam)
            {
                IfActionScene(interpreter => interpreter.MovePlayerToCamera());
            }
        }

        private void InputKey(string keyName, KeyMode mode)
        {
            if (mode == KeyMode.PressDown)
            {
                switch (keyName)
                {
                    case "NONE":
                        break;
                    case "WALK":
                        IfActionScene(interpreter => interpreter.StartWalking());
                        break;
                    case "DASH":
                        IfActionScene(interpreter => interpreter.StartWalking(true));
                        break;
                    case "PL2CAM":
                        _Pl2Cam = true;
                        break;
                    case "LBUTTON":
                        VR.Input.Mouse.LeftButtonDown();
                        break;
                    case "RBUTTON":
                        VR.Input.Mouse.RightButtonDown();
                        break;
                    case "MBUTTON":
                        VR.Input.Mouse.MiddleButtonDown();
                        break;
                    case "LROTATION":
                    case "RROTATION":
                    case "NEXT":
                    case "SCROLLUP":
                    case "SCROLLDOWN":
                        // ここでは何もせず、上げたときだけ処理する
                        break;
                    case "CROUCH":
                        IfActionScene(interpreter => interpreter.Crouch());
                        break;
                    default:
                        VR.Input.Keyboard.KeyDown((VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), keyName));
                        break;
                }
                _SentUnmatchedDown.Add(keyName);
            }
            else
            {
                switch (keyName)
                {
                    case "NONE":
                        break;
                    case "WALK":
                        IfActionScene(interpreter => interpreter.StopWalking());
                        break;
                    case "DASH":
                        IfActionScene(interpreter => interpreter.StopWalking());
                        break;
                    case "PL2CAM":
                        _Pl2Cam = false;
                        break;
                    case "LBUTTON":
                        VR.Input.Mouse.LeftButtonUp();
                        break;
                    case "RBUTTON":
                        VR.Input.Mouse.RightButtonUp();
                        break;
                    case "MBUTTON":
                        VR.Input.Mouse.MiddleButtonUp();
                        break;
                    case "LROTATION":
                        IfActionScene(interpreter => interpreter.RotatePlayer(-_Settings.RotationAngle));
                        break;
                    case "RROTATION":
                        IfActionScene(interpreter => interpreter.RotatePlayer(_Settings.RotationAngle));
                        break;
                    case "SCROLLUP":
                        VR.Input.Mouse.VerticalScroll(1);
                        break;
                    case "SCROLLDOWN":
                        VR.Input.Mouse.VerticalScroll(-1);
                        break;
                    case "NEXT":
                        ChangeKeySet();
                        break;
                    case "CROUCH":
                        IfActionScene(interpreter => interpreter.StandUp());
                        break;
                    default:
                        VR.Input.Keyboard.KeyUp((VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), keyName));
                        break;
                }
                _SentUnmatchedDown.Remove(keyName);
            }
        }

        private void IfActionScene(Action<ActionSceneInterpreter> a)
        {
            if (_Interpreter.SceneInterpreter is ActionSceneInterpreter actInterpreter)
            {
                a(actInterpreter);
            }
        }

        public override List<HelpText> GetHelpTexts()
        {
            return new List<HelpText>();
        }
    }
}
