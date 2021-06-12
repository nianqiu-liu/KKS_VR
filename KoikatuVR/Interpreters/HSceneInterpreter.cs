using UnityEngine;
using VRGIN.Core;

namespace KoikatuVR.Interpreters
{
    class HSceneInterpreter : SceneInterpreter
    {
        private bool _NeedsResetCamera;

        public override void OnStart()
        {
            _NeedsResetCamera = true;
        }

        public override void OnDisable()
        {
            // nothing to do.
        }

        public override void OnUpdate()
        {
            if (_NeedsResetCamera)
            {
                ResetCamera();
            }
        }

        private void ResetCamera()
        {
            VRLog.Info("HScene ResetCamera");

            var camCollider = GameObject.FindObjectOfType<CameraControl_Ver2>()?.GetComponent<CapsuleCollider>();

            if (camCollider != null)
            {
                // Completely disable Vanish (map masking) regardless of user settings.
                // It doesn't make sense in VR and causes weird effects
                // (parts of walls in front of you randomly disappear).
                camCollider.enabled = false;
                _NeedsResetCamera = false;

                VRLog.Info("succeeded");
            }
        }
    }
}
