using System.Collections;
using IllusionUtility.GetUtility;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRGIN.Core;
using VRGIN.Modes;

namespace KKSCharaStudioVR
{
    public class VRControllerMgr : MonoBehaviour
    {
        private static VRControllerMgr _instance;

        private bool isOculusTouchMode;

        private bool touchModeCheckCompleted;

        public static bool IsOculusTouchMode => _instance.isOculusTouchMode;

        public static VRControllerMgr Install(GameObject container)
        {
            if (_instance == null) _instance = container.AddComponent<VRControllerMgr>();
            return _instance;
        }

        private void Start()
        {
        }

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneWasLoaded;
        }

        private void OnSceneWasLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            StopAllCoroutines();
            touchModeCheckCompleted = false;
            StartCoroutine(CheckTouchMode());
        }

        private IEnumerator CheckTouchMode()
        {
            while (!touchModeCheckCompleted)
            {
                CheckControllerType();
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void CheckControllerType()
        {
            if (isOculusTouchMode)
            {
                touchModeCheckCompleted = true;
            }
            else
            {
                if (!(VR.Mode is StandingMode)) return;
                if (VR.Mode.Left != null && VR.Mode.Left.IsTracking)
                {
                    if (VR.Mode.Left.transform.FindLoop("touchpad") != null)
                    {
                        isOculusTouchMode = false;
                        touchModeCheckCompleted = true;
                        return;
                    }

                    if (VR.Mode.Left.transform.FindLoop("thumbstick") != null)
                    {
                        isOculusTouchMode = true;
                        touchModeCheckCompleted = true;
                    }
                }

                if (VR.Mode.Right != null && VR.Mode.Right.IsTracking)
                {
                    if (VR.Mode.Right.transform.FindLoop("touchpad") != null)
                    {
                        isOculusTouchMode = false;
                        touchModeCheckCompleted = true;
                    }
                    else if (VR.Mode.Right.transform.FindLoop("thumbstick") != null)
                    {
                        isOculusTouchMode = true;
                        touchModeCheckCompleted = true;
                    }
                }
            }
        }
    }
}
