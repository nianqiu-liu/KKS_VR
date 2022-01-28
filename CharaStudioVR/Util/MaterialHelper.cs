using System;
using System.IO;
using System.Reflection;
using KKAPI.Utilities;
using UnityEngine;
using VRGIN.Core;

namespace KKS_VR.Util
{
    internal class MaterialHelper
    {
        private static Shader _colorZOrderShader;

        public static Shader GetColorZOrderShader()
        {
            if (_colorZOrderShader == null)
            {
                try
                {
                    var bundle = AssetBundle.LoadFromMemory(ResourceUtils.GetEmbeddedResource("ColorZOrderShader"));
                    _colorZOrderShader = bundle.LoadAsset<Shader>("ColorZOrder");
                    bundle.Unload(false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return null;
                }
            }

            return _colorZOrderShader;
        }

        public static Texture2D LoadImage(string filePath)
        {
            filePath = Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Images"), filePath);
            Texture2D texture2D = null;
            if (File.Exists(filePath))
            {
                var data = File.ReadAllBytes(filePath);
                texture2D = new Texture2D(2, 2);
                texture2D.LoadImage(data);
            }
            else
            {
                VRLog.Warn("File " + filePath + " does not exist");
            }

            return texture2D;
        }
    }
}
