using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using VRGIN.Core;
using UnityEngine;

namespace KoikatuVR.Caress
{
    /// <summary>
    /// Allows injecting simulated user inputs to HandCtrl. This is similar to
    /// faking mouse clicks using VR.Input, but is safer because it doesn't
    /// accidentally interact with the game UI.
    /// </summary>
    class HandCtrlHooks
    {
        // one instance for each button
        private static readonly Dictionary<int, HandCtrlHooks> _instances = new Dictionary<int, HandCtrlHooks>();

        private readonly int _button;
        private int _lastUpdate = -1;
        // [0] for down, [1] for up.
        private readonly Queue<Action>[] _queues = new Queue<Action>[2]
        {
            new Queue<Action>(),
            new Queue<Action>(),
        };
        private bool _pressed = false;
        private bool _pressedSelf = false;
        private bool _down = false;
        private bool _up = false;

        private HandCtrlHooks(int button)
        {
            _button = button;
        }

        /// <summary>
        /// Inject a synthetic ButtonDown message into the hand ctrl.
        /// If an action is given, it will be invoked just before the message is
        /// handed over to the ctrl.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="action"></param>
        public static void InjectMouseButtonDown(int button, Action action = null)
        {
            GetInstance(button)._queues[0].Enqueue(action);
        }

        /// <summary>
        /// Inject a synthetic ButtonUp message into the hand ctrl.
        /// If an action is given, it will be invoked just before the message is
        /// handed over to the ctrl.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="action"></param>
        public static void InjectMouseButtonUp(int button, Action action = null)
        {
            GetInstance(button)._queues[1].Enqueue(action);
        }


        // Used by the patched version of HandCtrl.
        public static bool GetMouseButtonDown(int button)
        {
            return GetInstance(button).UpdateForFrame()._down;
        }

        // Used by the patched version of HandCtrl.
        public static bool GetMouseButtonUp(int button)
        {
            return GetInstance(button).UpdateForFrame()._up;
        }

        // Used by the patched version of HandCtrl.
        public static bool GetMouseButton(int button)
        {
            return GetInstance(button).UpdateForFrame()._pressed;
        }

        /// <summary>
        /// Update _down, _up, _pressed, and _pressedSelf if necessary.
        /// </summary>
        /// <returns>this</returns>
        private HandCtrlHooks UpdateForFrame()
        {
            if (Time.frameCount == _lastUpdate)
            {
                return this;
            }

            _lastUpdate = Time.frameCount;
            var pressedNative = Input.GetMouseButton(_button);
            var downNative = Input.GetMouseButtonDown(_button);
            var upNative = Input.GetMouseButtonUp(_button);

            if (downNative | upNative)
            {
                _down = downNative;
                _up = upNative;
            }
            else if (!_pressedSelf && TryDequeue(0))
            {
                _pressedSelf = true;
                _down = true;
                _up = false;
            }
            else if (_pressedSelf && TryDequeue(1))
            {
                _pressedSelf = false;
                _down = false;
                _up = true;
            }
            else
            {
                _down = false;
                _up = false;
            }

            _pressed = _pressedSelf | pressedNative;
            return this;
        }

        private bool TryDequeue(int downUp)
        {
            if (_queues[downUp].Count == 0)
            {
                return false;
            }

            _queues[downUp].Dequeue()?.Invoke();
            return true;
        }

        private static HandCtrlHooks GetInstance(int button)
        {
            if (!_instances.ContainsKey(button))
            {
                _instances.Add(button, new HandCtrlHooks(button));
            }
            return _instances[button];
        }

    }

    [HarmonyPatch]
    class HandCtrlPatches
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(HandCtrl), "ClickAction");
            yield return AccessTools.Method(typeof(HandCtrl), "DragAction");
            yield return AccessTools.Method(typeof(HandCtrl), "KissAction");
            yield return AccessTools.Method(typeof(HandCtrl), "OnCollision");
            yield return AccessTools.Method(typeof(HandCtrl), "HitReaction");
        }

        /// <summary>
        /// Replace calls to UnityEngine.Input.GetMouseButton{,Down,Up} with calls to
        /// HandCtrlHooks.GetMouseButton{,Down,Up}.
        /// </summary>
        /// <param name="insts"></param>
        /// <returns></returns>
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts)
        {
            var methodsToReplace = new string[] { "GetMouseButtonDown", "GetMouseButtonUp", "GetMouseButton" };
            foreach (var inst in insts)
            {
                if (inst.opcode == OpCodes.Call &&
                        inst.operand is MethodInfo method &&
                        method.ReflectedType == typeof(Input) &&
                        methodsToReplace.Contains(method.Name))
                {
                    var newMethod = AccessTools.Method(typeof(HandCtrlHooks), method.Name);
                    yield return new CodeInstruction(OpCodes.Call, newMethod);
                }
                else
                {
                    yield return inst;
                }
            }
        }

    }
}
