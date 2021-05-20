using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRGIN.Core;
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
        KoikatuInterpreter _interpreter;
        AibuColliderTracker _aibuTracker; // may be null

        protected override void OnAwake()
        {
            base.OnAwake();
            _interpreter = VR.Interpreter as KoikatuInterpreter;
        }

        protected override void OnUpdate()
        {
            _aibuTracker = AibuColliderTracker.CreateOrDestroy(_aibuTracker, _interpreter, referencePoint: transform);
        }

        protected void OnTriggerEnter(Collider other)
        {
            if (_aibuTracker != null && _aibuTracker.AddIfRelevant(other))
            {
                UpdateSelectKindTouch();
            }
        }

        protected void OnTriggerExit(Collider other)
        {
            if (_aibuTracker != null && _aibuTracker.RemoveIfRelevant(other))
            {
                UpdateSelectKindTouch();
            }
        }

        private void UpdateSelectKindTouch()
        {
            var colliderKind = _aibuTracker.GetCurrentColliderKind(out int femaleIndex);
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
