using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using VRGIN.Core;
using UnityEngine;

namespace KoikatuVR
{
    /// <summary>
    /// CameraControlKiller disables most functionalities of a
    /// CameraControl_Ver2 object.
    ///
    /// The CameraControl_Ver2 implments a camera that can be rotated,
    /// zoomed and moved with a mouse, and is used in H scenes and in Maker.
    /// It's also responsible for "map masking", a feature that hides
    /// walls that blocks the view of the character.
    /// 
    /// In VR, these features are not only unnecessary but are harmful,
    /// because the control features fight with VRGIN's attempt to
    /// set the camera position, and map masking causes random parts
    /// of walls to dissappear. For this reason we want to disable
    /// almost all of its functionalities, except that we want to keep
    /// its ability to unlock cursor.
    /// </summary>
    class CameraControlKiller
    {
        public static void Execute(CameraControl_Ver2 control)
        {
            if (control.enabled)
            {
                control.gameObject.AddComponent<CursorUnlocker>();
                control.enabled = false;
            }
        }
    }

    internal class CursorUnlocker : ProtectedBehaviour
    {
        private CameraControl_Ver2 _control;

        protected override void OnStart()
        {
            _control = GetComponent<CameraControl_Ver2>();
        }

        protected override void OnLateUpdate()
        {
            base.OnLateUpdate();
            if (_control.isCursorLock)
            {
                Singleton<GameCursor>.Instance.SetCursorLock(false);
            }
        }
    }
}
