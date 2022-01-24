using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using VRGIN.Core;

namespace KKSCharaStudioVR
{
	internal class MaterialHelper
	{
		private static AssetBundle _GripMovePluginResources;

		private static Shader _ColorZOrderShader;

		public static Shader GetColorZOrderShader()
		{
			if (_ColorZOrderShader != null)
			{
				return _ColorZOrderShader;
			}
			try
			{
				if (_GripMovePluginResources == null)
				{
					_GripMovePluginResources = AssetBundle.LoadFromMemory(Resource.KKSCharaStudioVRShader);
				}
				_ColorZOrderShader = _GripMovePluginResources.LoadAsset<Shader>("ColorZOrder");
				return _ColorZOrderShader;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return null;
			}
		}

		public static Texture2D LoadImage(string filePath)
		{
			filePath = Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Images"), filePath);
			Texture2D texture2D = null;
			if (File.Exists(filePath))
			{
				byte[] data = File.ReadAllBytes(filePath);
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
