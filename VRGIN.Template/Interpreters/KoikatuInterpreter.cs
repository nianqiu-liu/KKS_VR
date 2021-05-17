using UnityEngine;
using VRGIN.Core;

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
                    //nextInterpreter = new TalkSceneInterpreter(); 特有の処理がないため不要
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
    }
}
