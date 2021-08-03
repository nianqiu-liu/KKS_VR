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
using System.ComponentModel;

namespace KoikatuVR.Controls
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
        private readonly HashSet<AssignableFunction> _SentUnmatchedDown
            = new HashSet<AssignableFunction>();

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

        private void InputKey(AssignableFunction fun, KeyMode mode)
        {
            if (mode == KeyMode.PressDown)
            {
                switch (fun)
                {
                    case AssignableFunction.NONE:
                        break;
                    case AssignableFunction.WALK:
                        IfActionScene(interpreter => interpreter.StartWalking());
                        break;
                    case AssignableFunction.DASH:
                        IfActionScene(interpreter => interpreter.StartWalking(true));
                        break;
                    case AssignableFunction.PL2CAM:
                        _Pl2Cam = true;
                        break;
                    case AssignableFunction.LBUTTON:
                        VR.Input.Mouse.LeftButtonDown();
                        break;
                    case AssignableFunction.RBUTTON:
                        VR.Input.Mouse.RightButtonDown();
                        break;
                    case AssignableFunction.MBUTTON:
                        VR.Input.Mouse.MiddleButtonDown();
                        break;
                    case AssignableFunction.LROTATION:
                    case AssignableFunction.RROTATION:
                    case AssignableFunction.NEXT:
                    case AssignableFunction.SCROLLUP:
                    case AssignableFunction.SCROLLDOWN:
                        // ここでは何もせず、上げたときだけ処理する
                        break;
                    case AssignableFunction.CROUCH:
                        IfActionScene(interpreter => interpreter.Crouch());
                        break;
                    default:
                        VR.Input.Keyboard.KeyDown((VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), fun.ToString()));
                        break;
                }
                _SentUnmatchedDown.Add(fun);
            }
            else
            {
                switch (fun)
                {
                    case AssignableFunction.NONE:
                        break;
                    case AssignableFunction.WALK:
                        IfActionScene(interpreter => interpreter.StopWalking());
                        break;
                    case AssignableFunction.DASH:
                        IfActionScene(interpreter => interpreter.StopWalking());
                        break;
                    case AssignableFunction.PL2CAM:
                        _Pl2Cam = false;
                        break;
                    case AssignableFunction.LBUTTON:
                        VR.Input.Mouse.LeftButtonUp();
                        break;
                    case AssignableFunction.RBUTTON:
                        VR.Input.Mouse.RightButtonUp();
                        break;
                    case AssignableFunction.MBUTTON:
                        VR.Input.Mouse.MiddleButtonUp();
                        break;
                    case AssignableFunction.LROTATION:
                        IfActionScene(interpreter => interpreter.RotatePlayer(-_Settings.RotationAngle));
                        break;
                    case AssignableFunction.RROTATION:
                        IfActionScene(interpreter => interpreter.RotatePlayer(_Settings.RotationAngle));
                        break;
                    case AssignableFunction.SCROLLUP:
                        VR.Input.Mouse.VerticalScroll(1);
                        break;
                    case AssignableFunction.SCROLLDOWN:
                        VR.Input.Mouse.VerticalScroll(-1);
                        break;
                    case AssignableFunction.NEXT:
                        ChangeKeySet();
                        break;
                    case AssignableFunction.CROUCH:
                        IfActionScene(interpreter => interpreter.StandUp());
                        break;
                    default:
                        VR.Input.Keyboard.KeyUp((VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), fun.ToString()));
                        break;
                }
                _SentUnmatchedDown.Remove(fun);
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
            return new List<HelpText>(new[] {
                ToolUtil.HelpTrigger(Owner, DescriptionFor(_KeySet.Trigger)),
                ToolUtil.HelpGrip(Owner, DescriptionFor(_KeySet.Grip)),
                ToolUtil.HelpTrackpadCenter(Owner, DescriptionFor(_KeySet.Center)),
                ToolUtil.HelpTrackpadLeft(Owner, DescriptionFor(_KeySet.Left)),
                ToolUtil.HelpTrackpadRight(Owner, DescriptionFor(_KeySet.Right)),
                ToolUtil.HelpTrackpadUp(Owner, DescriptionFor(_KeySet.Up)),
                ToolUtil.HelpTrackpadDown(Owner, DescriptionFor(_KeySet.Down)),
            }.Where(x => x != null));
        }

        private static string DescriptionFor(AssignableFunction fun)
        {
            var member = typeof(AssignableFunction).GetMember(fun.ToString()).FirstOrDefault();
            var descr = member?.GetCustomAttributes(typeof(DescriptionAttribute), false).Cast<DescriptionAttribute>().FirstOrDefault()?.Description;
            return descr ?? fun.ToString();
        }
    }
}
