using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using VRGIN.Core;
using VRGIN.Helpers;

namespace KoikatuVR
{
    /// <summary>
    /// A VR fader that synchronizes with the fader of the base game.
    /// </summary>
    class VRFade : ProtectedBehaviour
    {
        /// <summary>
        /// Reference to the image used by the vanilla SceneFade object.
        /// </summary>
        Graphic _vanillaImage;
        Material _fadeMaterial;
        int _fadeMaterialColorID;
        float _alpha = 0f;

        public static void Create()
        {
            VR.Camera.gameObject.AddComponent<VRFade>();
        }

        protected override void OnAwake()
        {
            _vanillaImage = Manager.Scene.sceneFadeCanvas.fadeImage;
            _fadeMaterial = new Material(UnityHelper.GetShader("Custom/SteamVR_Fade"));
            _fadeMaterialColorID = Shader.PropertyToID("fadeColor");
        }

        private void OnPostRender()
        {
            if (_vanillaImage != null)
            {
                var fadeColor = _vanillaImage.color;
                _alpha = Mathf.Max(_alpha - 0.05f, fadeColor.a); // Use at least 20 frames to fade out.
                fadeColor.a = _alpha;
                if (_alpha > 0.0001f)
                {
                    _fadeMaterial.SetColor(_fadeMaterialColorID, fadeColor);
                    _fadeMaterial.SetPass(0);
                    GL.Begin(GL.QUADS);

                    GL.Vertex3(-1, -1, 0);
                    GL.Vertex3( 1, -1, 0);
                    GL.Vertex3(1, 1, 0);
                    GL.Vertex3(-1, 1, 0);
                    GL.End();
                }
            }
        }
    }
}
