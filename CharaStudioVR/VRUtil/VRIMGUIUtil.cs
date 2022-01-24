using UnityEngine;

namespace VRUtil
{
	public class VRIMGUIUtil
	{
		private static GUISkin _guiSkin = null;

		private static Color windowTextColor = new Color(0.1f, 0.5f, 0.1f, 1f);

		private static Color buttonTextColor = new Color(0.3f, 0.9f, 0.3f, 1f);

		public static GUISkin VRGUISkin
		{
			get
			{
				if (_guiSkin == null)
				{
					_guiSkin = CreateVRGUISkin(GUI.skin);
				}
				return _guiSkin;
			}
			set
			{
				if (value != null)
				{
					_guiSkin = value;
				}
			}
		}

		public static GUISkin CreateVRGUISkin(GUISkin cloneFrom)
		{
			GUISkin gUISkin = Object.Instantiate(cloneFrom);
			GUIStyle gUIStyle = new GUIStyle(gUISkin.window);
			Texture2D texture2D = new Texture2D(1, 1);
			texture2D.SetPixel(0, 0, new Color(0.9f, 0.9f, 0.9f, 1f));
			texture2D.Apply();
			gUIStyle.normal.textColor = windowTextColor;
			gUIStyle.normal.background = texture2D;
			gUIStyle.onNormal.textColor = windowTextColor;
			gUIStyle.onNormal.background = texture2D;
			gUISkin.window = gUIStyle;
			gUISkin.button.normal.textColor = buttonTextColor;
			gUISkin.button.onNormal.textColor = buttonTextColor;
			gUISkin.button.normal.background = CreateColorInvertedTexture(gUISkin.button.normal.background);
			gUISkin.button.onNormal.background = CreateColorInvertedTexture(gUISkin.button.onNormal.background);
			gUISkin.label.normal.textColor = windowTextColor;
			gUISkin.label.onNormal.textColor = windowTextColor;
			gUISkin.toggle.normal.textColor = windowTextColor;
			gUISkin.toggle.onNormal.textColor = windowTextColor;
			gUISkin.toggle.normal.background = CreateColorInvertedTexture(gUISkin.toggle.normal.background);
			gUISkin.toggle.onNormal.background = CreateColorInvertedTexture(gUISkin.toggle.normal.background);
			gUISkin.settings.selectionColor = windowTextColor;
			gUISkin.textField.normal.textColor = buttonTextColor;
			gUISkin.textField.onNormal.textColor = buttonTextColor;
			gUISkin.textField.focused.textColor = buttonTextColor;
			gUISkin.textField.onFocused.textColor = buttonTextColor;
			gUISkin.textField.normal.background = CreateColorInvertedTexture(gUISkin.textField.normal.background);
			gUISkin.textField.onNormal.background = CreateColorInvertedTexture(gUISkin.textField.onNormal.background);
			gUISkin.textField.focused.background = CreateColorInvertedTexture(gUISkin.textField.focused.background);
			gUISkin.textField.onFocused.background = CreateColorInvertedTexture(gUISkin.textField.onFocused.background);
			gUISkin.textArea.normal.textColor = buttonTextColor;
			gUISkin.textArea.onNormal.textColor = buttonTextColor;
			gUISkin.textArea.focused.textColor = buttonTextColor;
			gUISkin.textArea.onFocused.textColor = buttonTextColor;
			gUISkin.textArea.normal.background = CreateColorInvertedTexture(gUISkin.textArea.normal.background);
			gUISkin.textArea.onNormal.background = CreateColorInvertedTexture(gUISkin.textArea.onNormal.background);
			gUISkin.textArea.focused.background = CreateColorInvertedTexture(gUISkin.textArea.focused.background);
			gUISkin.textArea.onFocused.background = CreateColorInvertedTexture(gUISkin.textArea.onFocused.background);
			gUISkin.box.normal.background = texture2D;
			gUISkin.box.onNormal.background = texture2D;
			gUISkin.box.normal.textColor = windowTextColor;
			gUISkin.box.onNormal.textColor = windowTextColor;
			gUISkin.horizontalSlider.normal.background = CreateColorInvertedTexture(gUISkin.horizontalSlider.normal.background);
			gUISkin.horizontalSlider.onNormal.background = CreateColorInvertedTexture(gUISkin.horizontalSlider.onNormal.background);
			gUISkin.verticalSlider.normal.background = CreateColorInvertedTexture(gUISkin.verticalSlider.normal.background);
			gUISkin.verticalSlider.onNormal.background = CreateColorInvertedTexture(gUISkin.verticalSlider.onNormal.background);
			gUISkin.horizontalSliderThumb.normal.background = CreateColorInvertedTexture(gUISkin.horizontalSliderThumb.normal.background);
			gUISkin.horizontalSliderThumb.onNormal.background = CreateColorInvertedTexture(gUISkin.horizontalSliderThumb.onNormal.background);
			gUISkin.verticalSliderThumb.normal.background = CreateColorInvertedTexture(gUISkin.verticalSliderThumb.normal.background);
			gUISkin.verticalSliderThumb.onNormal.background = CreateColorInvertedTexture(gUISkin.verticalSliderThumb.onNormal.background);
			return gUISkin;
		}

		public static Texture2D CreateColorInvertedTexture(Texture tex, bool isLiner = false)
		{
			float num = 0.5f;
			float num2 = 0.5f;
			if (tex == null)
			{
				return null;
			}
			RenderTextureReadWrite readWrite = RenderTextureReadWrite.Linear;
			if (!isLiner)
			{
				readWrite = RenderTextureReadWrite.sRGB;
			}
			RenderTexture renderTexture = new RenderTexture(tex.width, tex.height, 0, RenderTextureFormat.ARGB32, readWrite);
			bool sRGBWrite = GL.sRGBWrite;
			GL.sRGBWrite = !isLiner;
			Graphics.SetRenderTarget(renderTexture);
			GL.Clear(false, true, new Color(0f, 0f, 0f, 0f));
			Graphics.SetRenderTarget(null);
			Graphics.Blit(tex, renderTexture);
			GL.sRGBWrite = sRGBWrite;
			Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, isLiner);
			RenderTexture.active = renderTexture;
			texture2D.ReadPixels(new Rect(0f, 0f, renderTexture.width, renderTexture.height), 0, 0);
			texture2D.Apply();
			RenderTexture.active = null;
			renderTexture.Release();
			Color[] pixels = texture2D.GetPixels();
			for (int i = 0; i < pixels.Length; i++)
			{
				pixels[i].r = Mathf.Clamp01(pixels[i].r + num);
				pixels[i].g = Mathf.Clamp01(pixels[i].g + num);
				pixels[i].b = Mathf.Clamp01(pixels[i].b + num);
				if (pixels[i].a != 0f)
				{
					pixels[i].a = Mathf.Clamp01(pixels[i].a + num2);
				}
			}
			texture2D.SetPixels(pixels);
			texture2D.Apply();
			texture2D.wrapMode = tex.wrapMode;
			return texture2D;
		}
	}
}
