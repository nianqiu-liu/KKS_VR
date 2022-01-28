using UnityEngine;
using Valve.VR;
using VRGIN.Controls;
using VRGIN.Core;
using VRGIN.Helpers;

namespace KoikatuVR.Controls
{
    internal class GrabAction
    {
        private readonly Controller _controller;
        private readonly EVRButtonId _button;

        public GrabAction(Controller controller, EVRButtonId button)
        {
            _controller = controller;
            _button = button;

            _PrevControllerPos = controller.transform.position;
            _GripStartTime = Time.unscaledTime;
            _PrevControllerPos = controller.transform.position;
            _PrevControllerRot = controller.transform.rotation;

            _TravelRumble = new TravelDistanceRumble(500, 0.1f, controller.transform);
            _TravelRumble.UseLocalPosition = true;
            _TravelRumble.Reset();
            //controller.StartRumble(_TravelRumble);
        }

        public void Destroy()
        {
            _TravelRumble.Close();
        }
        
        //private PlayArea _ProspectedPlayArea = new PlayArea();
        private TravelDistanceRumble _TravelRumble;
        private float? _GripStartTime;
        private Vector3 _PrevControllerPos;
        private Quaternion _PrevControllerRot;
        
        public bool HandleGrabbing()
        {
            float? gripStartTime;
            float? nullable;
            var isPressed = _controller.Input.GetPress(_button);
            if (isPressed)
            {
                var vector3 = _controller.transform.position - _PrevControllerPos;
                var quaternion = Quaternion.Inverse(_PrevControllerRot * Quaternion.Inverse(_controller.transform.rotation)) *
                                 (_controller.transform.rotation * Quaternion.Inverse(_controller.transform.rotation));
                var unscaledTime = Time.unscaledTime;
                gripStartTime = _GripStartTime;
                nullable = unscaledTime - gripStartTime;
                if (nullable.GetValueOrDefault() > 0.1f & nullable.HasValue || Calculator.Distance(vector3.magnitude) > 0.01f)
                {
                    var num2 = Calculator.Angle(Vector3.forward, quaternion * Vector3.forward) * VR.Settings.RotationMultiplier;
                    VR.Camera.SteamCam.origin.transform.position -= vector3;
                    //_ProspectedPlayArea.Height -= vector3.y;
                    //if (!VR.Settings.GrabRotationImmediateMode && _controller.Input.GetPress(12884901888UL))
                    //{
                    //    VR.Camera.SteamCam.origin.transform.RotateAround(VR.Camera.Head.position, Vector3.up, -num2);
                    //    //_ProspectedPlayArea.Rotation -= num2;
                    //}

                    _GripStartTime = 0.0f;
                }
            }
            if (_controller.Input.GetPressUp(_button))
            {
                //this.EnterState(WarpTool.WarpState.None);
                var unscaledTime = Time.unscaledTime;
                gripStartTime = _GripStartTime;
                nullable = unscaledTime - gripStartTime;
                var num = 0.1f;
                if (nullable.GetValueOrDefault() < num & nullable.HasValue)
                {
                    _controller.StartRumble(new RumbleImpulse(800));
                    //_ProspectedPlayArea.Height = 0.0f;
                    //_ProspectedPlayArea.Scale = _IPDOnStart;
                }
            }/*
            if (VRGIN.Core.VR.Settings.GrabRotationImmediateMode && _controller.Input.GetPressUp(12884901888UL))
            {
                float angle = Calculator.Angle(Vector3.ProjectOnPlane(_controller.transform.position - VRGIN.Core.VR.Camera.Head.position, Vector3.up).normalized, Vector3.ProjectOnPlane(VRGIN.Core.VR.Camera.Head.forward, Vector3.up).normalized);
                VRGIN.Core.VR.Camera.SteamCam.origin.transform.RotateAround(VRGIN.Core.VR.Camera.Head.position, Vector3.up, angle);
                this._ProspectedPlayArea.Rotation = angle;
            }*/
            _PrevControllerPos = _controller.transform.position;
            _PrevControllerRot = _controller.transform.rotation;
            //this.CheckRotationalPress();

            return isPressed;
        }
        
    }
}
