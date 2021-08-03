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

        private ButtonsSubtool _buttonsSubtool; // null iff disabled

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
            _buttonsSubtool.Destroy();
            _buttonsSubtool = new ButtonsSubtool(_Interpreter, _Settings);
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
        }

        protected override void OnStart()
        {
            base.OnStart();

            _Settings = (VR.Context.Settings as KoikatuSettings);
            SetScene(inHScene: false);
            _Settings.AddListener("KeySets", (_, _1) => ResetKeys());
            _Settings.AddListener("HKeySets", (_, _1) => ResetKeys());
        }

        protected override void OnDestroy()
        {
            // nothing to do.
        }

        protected override void OnDisable()
        {
            _buttonsSubtool?.Destroy();
            _buttonsSubtool = null;
            base.OnDisable();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _Interpreter = VR.Interpreter as KoikatuInterpreter;
            _buttonsSubtool = new ButtonsSubtool(_Interpreter, _Settings);
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

            _buttonsSubtool.Update();
        }

        private void InputKey(AssignableFunction fun, KeyMode mode)
        {
            if (mode == KeyMode.PressDown)
            {
                switch(fun)
                {
                    case AssignableFunction.NEXT:
                        break;
                    default:
                        _buttonsSubtool.ButtonDown(fun);
                        break;
                }
            }
            else
            {
                switch (fun)
                {
                    case AssignableFunction.NEXT:
                        ChangeKeySet();
                        break;
                    default:
                        _buttonsSubtool.ButtonUp(fun);
                        break;
                }
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
