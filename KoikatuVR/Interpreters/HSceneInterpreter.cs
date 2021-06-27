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
            var control = GameObject.FindObjectOfType<CameraControl_Ver2>();

            if (control != null)
            {
                CameraControlKiller.Execute(control);
                _NeedsResetCamera = false;
            }
        }
    }
}
