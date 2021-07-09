using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VRGIN.Core;
using HarmonyLib;

namespace KoikatuVR
{
    /// <summary>
    /// A class responsible for moving the VR camera.
    /// </summary>
    class VRMover
    {
        public static VRMover Instance {
            get {
                if (_instance == null)
                {
                    _instance = new VRMover();
                }
                return _instance;
            }
        }
        private static VRMover _instance;

        private Vector3 _lastPosition;
        private Quaternion _lastRotation;

        public VRMover()
        {
            _lastPosition = Vector3.zero;
            _lastRotation = Quaternion.identity;
        }

        /// <summary>
        /// Move the camera to the specified pose.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="keepHeight"></param>
        public void MoveTo(Vector3 position, Quaternion rotation, bool keepHeight)
        {
            VRLog.Info("Moving camera!");
            _lastPosition = position;
            _lastRotation = rotation;
            VR.Mode.MoveToPosition(position, rotation, ignoreHeight: keepHeight);
        }

        /// <summary>
        /// Move to the specified pose if doing so seems appropriate.
        ///
        /// Current heurestics are:
        ///
        /// * Move if the scene fade is currently active.
        /// * Move if the specified pose is sufficiently different from that of
        ///   the previous move.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="keepHeight"></param>
        public void MaybeMoveTo(Vector3 position, Quaternion rotation, bool keepHeight)
        {
            if (Singleton<Manager.Scene>.Instance.IsFadeNow ||
                IsDestinationFar(position, rotation))
            {
                MoveTo(position, rotation, keepHeight);
            }
            else
            {
                VRLog.Info("Not moving because heurestic conditions are not met");
            }
        }

        /// <summary>
        /// Similar to MaybeMoveTo, but also considers the ADV fade state.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="keepHeight"></param>
        public void MaybeMoveToADV(ADV.TextScenario textScenario, Vector3 position, Quaternion rotation, bool keepHeight)
        {
            var advFade = new Traverse(textScenario).Field<ADVFade>("advFade").Value;
            if (!advFade.IsEnd)
            {
                MoveTo(position, rotation, keepHeight);
            }
            else
            {
                MaybeMoveTo(position, rotation, keepHeight);
            }
        }

        private bool IsDestinationFar(Vector3 position, Quaternion rotation)
        {
            var distance = (position - _lastPosition).magnitude;
            var angleDistance = Mathf.DeltaAngle(rotation.eulerAngles.y, _lastRotation.eulerAngles.y);
            return 1f < distance / 2f + angleDistance / 90f;
        }
    }
}
