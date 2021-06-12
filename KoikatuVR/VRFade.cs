using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using VRGIN.Core;

namespace KoikatuVR
{
    /// <summary>
    /// An object that blocks the player's view during scene loading to prevent massive
    /// frame drops from being visible.
    /// </summary>
    class VRFade : ProtectedBehaviour
    {
        /// <summary>
        /// Reference to the image used by the vanilla SceneFade object.
        /// </summary>
        Image vanillaImage;
        Texture2D texture;
        Renderer renderer;

        public static void Create()
        {
            var fade = GameObject.CreatePrimitive(PrimitiveType.Quad).AddComponent<VRFade>();
            fade.name = "VRFade";
            fade.transform.parent = VR.Camera.transform;
            // Place the fade object just behind the near clip plane.
            fade.transform.localPosition = Vector3.forward * (VR.Context.NearClipPlane * 1.1f);
            fade.transform.localRotation = Quaternion.identity;
            Destroy(fade.GetComponent<Collider>());
        }

        protected override void OnAwake()
        {
            var fades = Resources.FindObjectsOfTypeAll<SceneFade>();

            if (fades.Length == 1)
            {
                vanillaImage = fades[0].GetComponent<Image>();
            }
            else
            {
                VRLog.Warn("VRFade: failed to find the vanilla fade: {0} candidates found", fades.Length);
            }
            renderer = gameObject.GetComponent<Renderer>();
            renderer.material.shader = VR.Context.Materials.UnlitTransparent.shader;
            texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, new Color(1, 1, 1, 0));
            texture.Apply();
            renderer.material.mainTexture = texture;
            // renderQueue needs to be exactly 5000, otherwise water (in the
            // school's pool) is drawn in front of the fade.
            renderer.material.renderQueue = 5000;
        }

        protected override void OnUpdate()
        {
            if (vanillaImage != null)
            {
                var fadeColor = vanillaImage.color;
                if (fadeColor.a > 0.0001f)
                {
                    texture.SetPixel(0, 0, fadeColor);
                    texture.Apply();
                    renderer.enabled = true;
                }
                else
                {
                    renderer.enabled = false;
                }
            }
        }
    }
}
