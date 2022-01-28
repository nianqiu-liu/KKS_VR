using UnityEngine;
using VRGIN.Core;
using VRGIN.Helpers;

namespace KKS_VR.Features
{
    /// <summary>
    /// A VR fader that synchronizes with the fader of the base game.
    /// </summary>
    internal class VRFade : ProtectedBehaviour
    {
        /// <summary>
        /// Reference to the image used by the vanilla SceneFade object.
        /// </summary>
        private CanvasGroup _vanillaImage;

        private Material _fadeMaterial;
        private int _fadeMaterialColorID;
        private float _alpha = 0f;

        public static void Create()
        {
            VR.Camera.gameObject.AddComponent<VRFade>();
        }

        protected override void OnAwake()
        {
            _vanillaImage = Manager.Scene.sceneFadeCanvas.canvasGroup;
            _fadeMaterial = new Material(UnityHelper.GetShader("Custom/SteamVR_Fade"));
            _fadeMaterialColorID = Shader.PropertyToID("fadeColor");
        }

        private void OnPostRender()
        {
            if (_vanillaImage != null)
            {
                var fadeColor = _vanillaImage.alpha;
                _alpha = Mathf.Max(_alpha - 0.05f, fadeColor); // Use at least 20 frames to fade out.
                fadeColor = _alpha;
                if (_alpha > 0.0001f)
                {
                    _fadeMaterial.SetColor(_fadeMaterialColorID, new Color(1, 1, 1, fadeColor));
                    _fadeMaterial.SetPass(0);
                    GL.Begin(GL.QUADS);

                    GL.Vertex3(-1, -1, 0);
                    GL.Vertex3(1, -1, 0);
                    GL.Vertex3(1, 1, 0);
                    GL.Vertex3(-1, 1, 0);
                    GL.End();
                }
            }
        }
    }
}
