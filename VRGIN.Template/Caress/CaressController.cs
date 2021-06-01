using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRGIN.Core;
using VRGIN.Controls;
using HarmonyLib;
using UnityEngine;
using KoikatuVR.Interpreters;

namespace KoikatuVR.Caress
{
    /// <summary>
    /// An extra component to be attached to each controller, providing the caress
    /// functionality in H scenes.
    /// </summary>
    class CaressController : ProtectedBehaviour
    {
        // Basic plan:
        //
        // * In an H scene, keep track of the potential caress points
        //   near this controller. _aibuTracker is responsible for this.
        // * While there is at least one such point, lock the controller
        //   to steal any trigger events.
        // * When the trigger is pulled, initiate caress.
        // * Delay releasing of the lock until the trigger is released.

        KoikatuInterpreter _interpreter;
        Controller _controller;
        AibuColliderTracker _aibuTracker; // may be null
        Controller.Lock _lock; // may be null but never invalid
        bool _triggerPressed; // Whether the trigger is currently pressed. false if _lock is null.

        protected override void OnAwake()
        {
            base.OnAwake();
            _interpreter = VR.Interpreter as KoikatuInterpreter;
            _controller = GetComponent<Controller>();
        }

        protected override void OnUpdate()
        {
            _aibuTracker = AibuColliderTracker.CreateOrDestroy(_aibuTracker, _interpreter, referencePoint: transform);
            if (_lock != null)
            {
                if (_aibuTracker != null)
                {
                    HandleTrigger();
                    HandleToolChange();
                }
                else
                {
                    ReleaseLock();
                }
            }
        }

        protected void OnTriggerEnter(Collider other)
        {
            if (_aibuTracker != null && _aibuTracker.AddIfRelevant(other))
            {
                UpdateLock();
            }
        }

        protected void OnTriggerExit(Collider other)
        {
            if (_aibuTracker != null && _aibuTracker.RemoveIfRelevant(other))
            {
                UpdateLock();
            }
        }

        private void UpdateLock()
        {
            bool shouldHaveLock = _aibuTracker.IsIntersecting();
            if (shouldHaveLock && _lock == null)
            {
                _controller.TryAcquireFocus(out _lock);
            }
            else if (!shouldHaveLock && _lock != null && !_triggerPressed)
            {
                ReleaseLock();
            }
        }

        private void HandleTrigger()
        {
            var device = SteamVR_Controller.Input((int)_controller.Tracking.index);
            if (!_triggerPressed && device.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger))
            {
                UpdateSelectKindTouch();
                HandCtrlHooks.InjectMouseButtonDown(0);
                _triggerPressed = true;
            }
            else if (_triggerPressed && device.GetPressUp(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger))
            {
                HandCtrlHooks.InjectMouseButtonUp(0);
                _triggerPressed = false;
                if (!_aibuTracker.IsIntersecting())
                {
                    ReleaseLock();
                }
            }
        }

        private void HandleToolChange()
        {
            var device = SteamVR_Controller.Input((int)_controller.Tracking.index);
            if (device.GetPressUp(Valve.VR.EVRButtonId.k_EButton_ApplicationMenu))
            {
                UpdateSelectKindTouch();
                HandCtrlHooks.InjectMouseScroll(1f);
            }

        }

        private void ReleaseLock()
        {
            SetSelectKindTouch(0, HandCtrl.AibuColliderKind.none);
            if (_triggerPressed)
                HandCtrlHooks.InjectMouseButtonUp(0);
            _triggerPressed = false;
            _lock.Release();
            _lock = null;
        }

        private void UpdateSelectKindTouch()
        {
            var colliderKind = _aibuTracker.GetCurrentColliderKind(out int femaleIndex);
            SetSelectKindTouch(femaleIndex, colliderKind);
        }

        /// <summary>
        /// Modify the internal state of the hand controls so that subsequent mouse button
        /// presses are interpreted to point to the specified (female, point) pair.
        /// </summary>
        /// <param name="femaleIndex"></param>
        /// <param name="colliderKind"></param>
        private void SetSelectKindTouch(int femaleIndex, HandCtrl.AibuColliderKind colliderKind)
        {
            HSceneProc proc = _aibuTracker.Proc;
            for (int i = 0; i < proc.flags.lstHeroine.Count; i++)
            {
                var hand = i == 0 ? proc.hand : proc.hand1;
                var kind = i == femaleIndex ? colliderKind : HandCtrl.AibuColliderKind.none;
                new Traverse(hand).Field("selectKindTouch").SetValue(kind);
            }
        }
    }
}
