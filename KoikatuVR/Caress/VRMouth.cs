using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VRGIN.Core;
using HarmonyLib;
using KoikatuVR.Interpreters;

namespace KoikatuVR.Caress
{
    /// <summary>
    /// An object that allows the user to kiss in H scenes by moving
    /// their head.
    /// </summary>
    class VRMouth : ProtectedBehaviour
    {
        KoikatuInterpreter _interpreter;
        KoikatuSettings _settings;
        AibuColliderTracker _aibuTracker; // may be null

        VRMouthColliderObject _small, _medium, _large;
        bool _inCaressMode = true;

        /// <summary>
        /// Indicates whether the currently running KissCo should end.
        /// null if KissCo is not running.
        /// </summary>
        bool? _kissCoShouldEnd;

        /// <summary>
        /// Create necessary game objects.
        /// </summary>
        public static void Init()
        {
            VR.Camera.gameObject.AddComponent<VRMouth>();
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            _interpreter = VR.Interpreter as KoikatuInterpreter;
            _settings = VR.Context.Settings as KoikatuSettings;

            // Create 3 colliders, small ones for entering and a large one for exiting.
            _small = VRMouthColliderObject
                .Create("VRMouthSmall", new Vector3(0, 0, 0.05f), new Vector3(0.05f, 0.05f, 0.07f));
            _small.TriggerEnter += HandleTriggerEnter;
            _medium = VRMouthColliderObject
                .Create("VRMouthMedium", new Vector3(0, 0, 0.09f), new Vector3(0.05f, 0.05f, 0.18f));
            _medium.TriggerEnter += HandleTriggerEnter;
            _large = VRMouthColliderObject
                .Create("VRMouthLarge", new Vector3(0, 0, 0.05f), new Vector3(0.1f, 0.1f, 0.2f));
            _large.TriggerExit += HandleTriggerExit;

            _small.gameObject.SetActive(false);
        }
        protected override void OnUpdate()
        {
            _aibuTracker = AibuColliderTracker.CreateOrDestroy(_aibuTracker, _interpreter, referencePoint: transform);
            if (_aibuTracker != null)
            {
                SwitchColliders();
            }
        }

        private void SwitchColliders()
        {
            var inCaressMode = _aibuTracker.Proc.flags.mode == HFlag.EMode.aibu;
            if (!_inCaressMode & inCaressMode)
            {
                _small.gameObject.SetActive(false);
                _medium.gameObject.SetActive(true);
            }
            else if (_inCaressMode & !inCaressMode)
            {
                _small.gameObject.SetActive(true);
                _medium.gameObject.SetActive(false);

            }
            _inCaressMode = inCaressMode;
        }

        private void HandleTriggerEnter(Collider other)
        {
            if (_aibuTracker != null && _aibuTracker.AddIfRelevant(other))
            {
                UpdateKissLick();
            }
        }

        private void HandleTriggerExit(Collider other)
        {
            if (_aibuTracker != null && _aibuTracker.RemoveIfRelevant(other))
            {
                UpdateKissLick();
            }
        }

        private void UpdateKissLick()
        {
            var colliderKind = _aibuTracker.GetCurrentColliderKind(out int _);
            if (colliderKind == HandCtrl.AibuColliderKind.mouth && _settings.AutomaticKissing)
            {
                StartKiss();
            }
            else
            {
                FinishKiss();
            }
        }

        /// <summary>
        /// Attempt to start a kiss.
        /// </summary>
        private void StartKiss()
        {
            if (_kissCoShouldEnd != null || new Traverse(_aibuTracker.Proc.hand).Field<bool>("isKiss").Value)
            {
                // Already kissing.
                return;
            }

            _kissCoShouldEnd = false;
            StartCoroutine(KissCo());
        }

        private IEnumerator KissCo()
        {
            StopLicking();

            var hand = _aibuTracker.Proc.hand;
            var handTrav = new Traverse(hand);
            var selectKindTouchTrav = handTrav.Field<HandCtrl.AibuColliderKind>("selectKindTouch");

            var prevKindTouch = selectKindTouchTrav.Value;
            selectKindTouchTrav.Value = HandCtrl.AibuColliderKind.mouth;
            bool messageDelivered = false;
            HandCtrlHooks.InjectMouseButtonDown(0, () => messageDelivered = true);
            while (!messageDelivered)
            {
                yield return null;
            }
            yield return new WaitForEndOfFrame();

            // Try to restore the old value of selectKindTouch.
            if (selectKindTouchTrav.Value == HandCtrl.AibuColliderKind.mouth)
            {
                selectKindTouchTrav.Value = prevKindTouch;
            }

            var isKissTrav = handTrav.Field<bool>("isKiss");
            while (_kissCoShouldEnd == false && isKissTrav.Value)
            {
                yield return null;
            }

            HandCtrlHooks.InjectMouseButtonUp(0);
            _kissCoShouldEnd = null;
        }

        private void FinishKiss()
        {
            if (_kissCoShouldEnd == false)
            {
                _kissCoShouldEnd = true;
            }
        }

        private void StopLicking()
        {
            _aibuTracker.Proc.hand.DetachItemByUseItem(2);
        }

        class VRMouthColliderObject : ProtectedBehaviour
        {
            public delegate void TriggerHandler(Collider other);
            public event TriggerHandler TriggerEnter;
            public event TriggerHandler TriggerExit;

            public static VRMouthColliderObject Create(string name, Vector3 center, Vector3 size)
            {
                var gameObj = new GameObject(name);
                gameObj.transform.localPosition = -0.07f * Vector3.up;
                gameObj.transform.parent = VR.Camera.transform;

                var collider = gameObj.AddComponent<BoxCollider>();
                collider.size = size;
                collider.center = center;
                collider.isTrigger = true;

                gameObj.AddComponent<Rigidbody>().isKinematic = true;
                return gameObj.AddComponent<VRMouthColliderObject>();
            }

            protected void OnTriggerEnter(Collider other)
            {
                TriggerEnter?.Invoke(other);
            }

            protected void OnTriggerExit(Collider other)
            {
                TriggerExit?.Invoke(other);
            }
        }
    }
}
