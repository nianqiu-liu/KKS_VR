using System;
using UnityEngine;

namespace KKSCharaStudioVR
{
	public class GUIUtils
	{
		private static bool isVR;

		private static Texture2D windowBG;

		static GUIUtils()
		{
			isVR = false;
			windowBG = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			if (Environment.CommandLine.Contains("--vr") || Environment.CommandLine.Contains("--studiovr"))
			{
				isVR = true;
			}
			windowBG.SetPixel(0, 0, Color.black);
			windowBG.Apply();
		}

		public static GUIStyle GetWindowStyle()
		{
			GUIStyle gUIStyle = new GUIStyle(GUI.skin.window);
			if (isVR)
			{
				GUI.backgroundColor = Color.black;
				gUIStyle.onNormal.background = windowBG;
				gUIStyle.normal.background = windowBG;
				gUIStyle.hover.background = windowBG;
				gUIStyle.focused.background = windowBG;
				gUIStyle.active.background = windowBG;
				gUIStyle.hover.textColor = Color.blue;
				gUIStyle.onHover.textColor = Color.blue;
			}
			return gUIStyle;
		}
	}
}
