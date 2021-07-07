using UnityEngine;
using VRGIN.Core;
using System.Collections;

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

        protected override void OnAwake()
        {
            base.OnAwake();

            CurrentScene = NoScene;
            SceneInterpreter = new OtherSceneInterpreter();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            DetectScene();
            SceneInterpreter.OnUpdate();
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
                VR.Mode.MoveToPosition(camera.transform.position, camera.transform.rotation, ignoreHeight: true);
                VRLog.Info("moved to {0} {1}", VR.Camera.Head.position, VR.Camera.Head.eulerAngles);
                VRLog.Info("Adding CameraControlControl");
                camera.gameObject.AddComponent<CameraControlControl>();
            }
            else
            {
                VRLog.Warn($"Unknown kind of main camera was added: {camera.name}");
            }
        }
    }
}
