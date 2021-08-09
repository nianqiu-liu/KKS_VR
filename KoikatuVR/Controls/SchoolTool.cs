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
        private Controller.Lock _lock = VRGIN.Controls.Controller.Lock.Invalid;

        private Controller.TrackpadDirection? _PrevTouchDirection;

        // When eneabled, exactly one of the below is non-null.
        private ButtonsSubtool _buttonsSubtool;
        private GrabAction _grab;

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
            if (_buttonsSubtool != null)
            {
                _buttonsSubtool.Destroy();
                _buttonsSubtool = new ButtonsSubtool(_Interpreter, _Settings);
            }
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
            _grab?.Destroy();
            _grab = null;
            if (_lock.IsValid)
            {
                _lock.Release();
            }
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

            UpdateLock();

            var inHScene = _Interpreter.CurrentScene == KoikatuInterpreter.HScene;
            if (inHScene != _InHScene)
            {
                SetScene(inHScene);
            }

            if (_grab != null)
            {
                if (_grab.HandleGrabbing() != GrabAction.Status.Continue)
                {
                    _grab.Destroy();
                    _grab = null;
                    _buttonsSubtool = new ButtonsSubtool(_Interpreter, _Settings);
                }
            }

            if (_buttonsSubtool != null)
            {
                HandleButtons();
            }
        }

        private void UpdateLock()
        {
            bool wantLock = _grab != null || _buttonsSubtool?.WantLock() == true;
            if (wantLock && !_lock.IsValid)
            {
                _lock = Owner.AcquireFocus(keepTool: true);
            }
            else if(!wantLock && _lock.IsValid)
            {
                _lock.Release();
            }
        }

        private void HandleButtons()
        {
            var device = this.Controller;

            if (device.GetPressDown(ButtonMask.Trigger))
            {
                InputDown(_KeySet.Trigger, ButtonMask.Trigger);
            }

            if (device.GetPressUp(ButtonMask.Trigger))
            {
                InputUp(_KeySet.Trigger);
            }

            if (device.GetPressDown(ButtonMask.Grip))
            {
                InputDown(_KeySet.Grip, ButtonMask.Grip);
            }

            if (device.GetPressUp(ButtonMask.Grip))
            {
                InputUp(_KeySet.Grip);
            }

            if (device.GetPressDown(ButtonMask.Touchpad))
            {
                _PrevTouchDirection = Owner.GetTrackpadDirection();
                switch (_PrevTouchDirection)
                {
                    case VRGIN.Controls.Controller.TrackpadDirection.Up:
                        InputDown(_KeySet.Up, ButtonMask.Touchpad);
                        break;
                    case VRGIN.Controls.Controller.TrackpadDirection.Down:
                        InputDown(_KeySet.Down, ButtonMask.Touchpad);
                        break;
                    case VRGIN.Controls.Controller.TrackpadDirection.Left:
                        InputDown(_KeySet.Left, ButtonMask.Touchpad);
                        break;
                    case VRGIN.Controls.Controller.TrackpadDirection.Right:
                        InputDown(_KeySet.Right, ButtonMask.Touchpad);
                        break;
                    case VRGIN.Controls.Controller.TrackpadDirection.Center:
                        InputDown(_KeySet.Center, ButtonMask.Touchpad);
                        break;
                }
            }

            if (_buttonsSubtool == null)
            {
                return;
            }

            // 上げたときの位置によらず、押したボタンを離す
            if (device.GetPressUp(ButtonMask.Touchpad))
            {
                switch (_PrevTouchDirection)
                {
                    case VRGIN.Controls.Controller.TrackpadDirection.Up:
                        InputUp(_KeySet.Up, ButtonMask.Touchpad);
                        break;
                    case VRGIN.Controls.Controller.TrackpadDirection.Down:
                        InputUp(_KeySet.Down, ButtonMask.Touchpad);
                        break;
                    case VRGIN.Controls.Controller.TrackpadDirection.Left:
                        InputUp(_KeySet.Left, ButtonMask.Touchpad);
                        break;
                    case VRGIN.Controls.Controller.TrackpadDirection.Right:
                        InputUp(_KeySet.Right, ButtonMask.Touchpad);
                        break;
                    case VRGIN.Controls.Controller.TrackpadDirection.Center:
                        InputUp(_KeySet.Center, ButtonMask.Touchpad);
                        break;
                }
            }

            _buttonsSubtool.Update();
        }

        private void InputDown(AssignableFunction fun, ulong buttonMask)
        {
            switch (fun)
            {
                case AssignableFunction.NEXT:
                    break;
                case AssignableFunction.GRAB:
                    _buttonsSubtool.Destroy();
                    _buttonsSubtool = null;
                    _grab = new GrabAction(Owner, Controller, buttonMask);
                    break;
                default:
                    _buttonsSubtool.ButtonDown(fun);
                    break;
            }
        }

        private void InputUp(AssignableFunction fun)
        {
            switch (fun)
            {
                case AssignableFunction.NEXT:
                    ChangeKeySet();
                    break;
                case AssignableFunction.GRAB:
                    break;
                default:
                    _buttonsSubtool.ButtonUp(fun);
                    break;
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
