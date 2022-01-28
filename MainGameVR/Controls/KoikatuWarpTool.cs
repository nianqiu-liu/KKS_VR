﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ActionGame.Chara;
using HarmonyLib;
using KoikatuVR.Interpreters;
using KoikatuVR.Settings;
using UnityEngine;
using Valve.VR;
using VRGIN.Controls;
using VRGIN.Controls.Tools;
using VRGIN.Core;

namespace KoikatuVR.Controls
{
    internal class KoikatuWarpTool : WarpTool
    {
        private KoikatuInterpreter _interpreter;
        private GameObject _protagonistToFollow;
        private KoikatuSettings _settings;

        protected override void OnStart()
        {
            base.OnStart();
            _interpreter = VR.Interpreter as KoikatuInterpreter;
            _settings = VR.Settings as KoikatuSettings;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            VRCameraMover.Instance.OnMove += OnCameraMove;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            VRCameraMover.Instance.OnMove -= OnCameraMove;
        }

        protected override void OnUpdate()
        {
            // If current state is moving/rotating the teleport target, cancel it so that grab moving
            // can be done (by default it's stuck in teleport mode until you switch tools)
            if (Controller.GetPressDown(EVRButtonId.k_EButton_Grip))
            {
                var tv = Traverse.Create(this);
                var state = tv.Field("State");
                var stateVal = state.GetValue<int>();
                if (stateVal == 1 || stateVal == 2)
                {
                    state.SetValue(0);
                    tv.Method("SetVisibility", new[] { typeof(bool) }).GetValue(false);
                }
            }

            var origin = VR.Camera.Origin;
            var oldOriginPosition = origin.position;
            var oldOriginRotation = origin.rotation;
            var oldCameraPos = VR.Camera.transform.position;

            base.OnUpdate();

            // Detect teleporting in Roam mode.
            if (_settings.TeleportWithProtagonist &&
                (origin.position - oldOriginPosition).sqrMagnitude > 0.04f &&
                _interpreter.SceneInterpreter is ActionSceneInterpreter act &&
                GameObject.Find("ActionScene/Player") is GameObject player &&
                player.activeInHierarchy)
            {
                // It looks like we either just teleported or become upright.
                var diffRotation = origin.rotation * Quaternion.Inverse(oldOriginRotation);
                var nonHorizontal = Vector3.Angle(Vector3.up, diffRotation * Vector3.up);
                if (nonHorizontal < 0.1f)
                {
                    // Looks like we teleported.
                    act.MovePlayerToCamera();
                    // We undo the camera movement because we want the actual
                    // teleportation to happen after the game has a chance
                    // to correct the protagonist's position.
                    origin.SetPositionAndRotation(oldOriginPosition, oldOriginRotation);
                    _protagonistToFollow = player;
                }
            }
        }

        protected override void OnLateUpdate()
        {
            base.OnLateUpdate();
            if (_protagonistToFollow != null)
            {
                var player = _protagonistToFollow.GetComponent<Player>();
                StartCoroutine(FollowDelayedCo(player));
                _protagonistToFollow = null;
            }
        }


        private IEnumerator FollowDelayedCo(Player player)
        {
            // Temporarily hide the protagonist.
            var oldActive = player.chaCtrl.objTop.activeSelf;
            player.chaCtrl.objTop.SetActive(false);
            // Wait for the game to correct the protagonist's position.
            yield return null;
            if (_interpreter.SceneInterpreter is ActionSceneInterpreter act)
            {
                VRLog.Debug("Following player");
                act.MoveCameraToPlayer();
            }

            player.chaCtrl.objTop.SetActive(oldActive);
        }

        private void OnCameraMove()
        {
            OnPlayAreaUpdated();
        }

        public override List<HelpText> GetHelpTexts()
        {
            return new List<HelpText>(new[]
            {
                ToolUtil.HelpTrackpadCenter(Owner, "Press to teleport"),
                ToolUtil.HelpGrip(Owner, "Hold to move"),
            }.Where(x => x != null));
        }
    }
}
