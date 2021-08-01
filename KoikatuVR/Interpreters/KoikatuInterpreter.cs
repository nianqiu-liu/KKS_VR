using UnityEngine;
using VRGIN.Core;
using System.Collections;
using UnityEngine.SceneManagement;

namespace KoikatuVR.Interpreters
{
    class KoikatuInterpreter : GameInterpreter
    {
        public const int NoScene = -1;
        public const int OtherScene = 0;
        public const int ActionScene = 1;
        public const int TalkScene = 2;
        public const int HScene = 3;
        public const int NightMenuScene = 4;
        public const int CustomScene = 5;

        public int CurrentScene { get; private set; }
        public SceneInterpreter SceneInterpreter;

        private Mirror.Manager _mirrorManager;
        private int _kkapiCanvasHackWait;
        private Canvas _kkSubtitlesCaption;

        protected override void OnAwake()
        {
            base.OnAwake();

            CurrentScene = NoScene;
            SceneInterpreter = new OtherSceneInterpreter();
            SceneManager.sceneLoaded += OnSceneLoaded;
            _mirrorManager = new Mirror.Manager();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            DetectScene();
            SceneInterpreter.OnUpdate();
        }

        protected override void OnLateUpdate()
        {
            base.OnLateUpdate();
            if (_kkSubtitlesCaption != null)
            {
                FixupKkSubtitles();
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            VRLog.Info($"OnSceneLoaded {scene.name}");
            foreach (var reflection in GameObject.FindObjectsOfType<MirrorReflection>())
            {
                _mirrorManager.Fix(reflection);
            }
        }

        /// <summary>
        /// Fix up scaling of subtitles added by KK_Subtitles. See
        /// https://github.com/IllusionMods/KK_Plugins/pull/91 for details.
        /// </summary>
        private void FixupKkSubtitles()
        {
            foreach (Transform child in _kkSubtitlesCaption.transform)
            {
                if (child.localScale != Vector3.one)
                {
                    VRLog.Info($"Fixing up scale for {child}");
                    child.localScale = Vector3.one;
                }
            }
        }

        public override bool IsIgnoredCanvas(Canvas canvas)
        {
            if (canvas.name == "CvsMenuTree")
            {
                // Here, we attempt to avoid some unfortunate conflict with
                // KKAPI.
                //
                // In order to support plugin-defined subcategories in Maker,
                // KKAPI clones some UI elements out of CvsMenuTree when the
                // canvas is created, then uses them as templates for custom
                // UI items.
                //
                // At the same time, VRGIN attempts to claim the canvas by
                // setting its mode to ScreenSpaceCamera, which changes
                // localScale of the canvas by a factor of 100 or so. If this
                // happens between KKAPI's cloning out and cloning in, the
                // resulting UI items will have the wrong scale, 72x the correct
                // size to be precise.
                //
                // So our solution here is to hide the canvas from VRGIN for a
                // couple of frames. Crude but works.

                if (_kkapiCanvasHackWait == 0)
                {
                    _kkapiCanvasHackWait = 3;
                    return true;
                }
                else
                {
                    _kkapiCanvasHackWait -= 1;
                    return 0 < _kkapiCanvasHackWait;
                }
            }
            else if (canvas.name == "KK_Subtitles_Caption")
            {
                _kkSubtitlesCaption = canvas;
            }

            return false;
        }

        // 前回とSceneが変わっていれば切り替え処理をする
        private void DetectScene()
        {
            int nextSceneType = NoScene;
            SceneInterpreter nextInterpreter = new OtherSceneInterpreter();

            if (GameObject.Find("TalkScene") != null)
            {
                if (CurrentScene != TalkScene)
                {
                    nextSceneType = TalkScene;
                    nextInterpreter = new TalkSceneInterpreter();
                    VRLog.Info("Start TalkScene");
                }
            }

            else if (GameObject.Find("HScene") != null)
            {
                if (CurrentScene != HScene)
                {
                    nextSceneType = HScene;
                    nextInterpreter = new HSceneInterpreter();
                    VRLog.Info("Start HScene");
                }
            }

            else if (GameObject.Find("NightMenuScene") != null)
            {
                if (CurrentScene != NightMenuScene)
                {
                    nextSceneType = NightMenuScene;
                    nextInterpreter = new NightMenuSceneInterpreter();
                    VRLog.Info("Start NightMenuScene");
                }
            }

            else if (GameObject.Find("ActionScene") != null)
            {
                if (CurrentScene != ActionScene)
                {
                    nextSceneType = ActionScene;
                    nextInterpreter = new ActionSceneInterpreter();
                    VRLog.Info("Start ActionScene");
                }
            }

            else if (GameObject.Find("CustomScene") != null)
            {
                if (CurrentScene != CustomScene)
                {
                    nextSceneType = CustomScene;
                    nextInterpreter = new CustomSceneInterpreter();
                    VRLog.Info("Start CustomScene");
                }
            }

            else
            {
                if (CurrentScene != OtherScene)
                {
                    nextSceneType = OtherScene;
                    //nextInterpreter = new OtherSceneInterpreter();
                    VRLog.Info("Start OtherScene");
                }
            }

            if (nextSceneType != NoScene)
            {
                SceneInterpreter.OnDisable();

                CurrentScene = nextSceneType;
                SceneInterpreter = nextInterpreter;
                SceneInterpreter.OnStart();
            }
        }

        protected override CameraJudgement JudgeCameraInternal(Camera camera)
        {
            if (camera.CompareTag("MainCamera"))
            {
                StartCoroutine(HandleMainCameraCo(camera));
            }
            return base.JudgeCameraInternal(camera);
        }

        /// <summary>
        /// A coroutine to be called when a new main camera is detected.
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        private IEnumerator HandleMainCameraCo(Camera camera)
        {
            // Unity might have messed with the camera transform for this frame,
            // so we wait for the next frame to get clean data.
            yield return null;

            if (camera.name == "ActionCamera" || camera.name == "FrontCamera")
            {
                VRLog.Info("Adding ActionCameraControl");
                camera.gameObject.AddComponent<ActionCameraControl>();
            }
            else if (camera.GetComponent<CameraControl_Ver2>() != null)
            {
                VRLog.Info("New main camera detected: moving to {0} {1}", camera.transform.position, camera.transform.eulerAngles);
                VRMover.Instance.MoveTo(camera.transform.position, camera.transform.rotation, keepHeight: false);
                VRLog.Info("moved to {0} {1}", VR.Camera.Head.position, VR.Camera.Head.eulerAngles);
                VRLog.Info("Adding CameraControlControl");
                camera.gameObject.AddComponent<CameraControlControl>();
            }
            else
            {
                VRLog.Warn($"Unknown kind of main camera was added: {camera.name}");
            }
        }

        public override bool ApplicationIsQuitting => Manager.Scene.isGameEnd;
    }
}
