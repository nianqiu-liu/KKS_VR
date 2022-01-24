using System;
using UnityEngine;
using UnityEngine.Rendering;
using VRGIN.Core;

namespace VRGIN.Visuals
{
	public class SubGUIQuad : ProtectedBehaviour
	{
		private Renderer renderer;

		public bool IsOwned;

		private int left;

		private int bottom;

		private int width;

		private int height;

		private int virtualScreenWidth;

		private int virtualScreenHeight;

		public static SubGUIQuad Create(int left, int bottom, int width, int height, int virtualScreenWidth, int virtualScreenHeight)
		{
			VRLog.Info("Create SubGUI");
			SubGUIQuad subGUIQuad = GameObject.CreatePrimitive(PrimitiveType.Quad).AddComponent<SubGUIQuad>();
			subGUIQuad.name = "SubGUIQuad";
			subGUIQuad.left = left;
			subGUIQuad.bottom = bottom;
			subGUIQuad.width = width;
			subGUIQuad.height = height;
			subGUIQuad.virtualScreenWidth = virtualScreenWidth;
			subGUIQuad.virtualScreenHeight = virtualScreenHeight;
			subGUIQuad.UpdateMesh();
			subGUIQuad.UpdateGUI();
			if (VR.GUI.SoftCursor != null)
			{
				VR.GUI.SoftCursor.enabled = false;
			}
			return subGUIQuad;
		}

		protected override void OnAwake()
		{
			renderer = GetComponent<Renderer>();
			base.transform.localPosition = Vector3.zero;
			base.transform.localRotation = Quaternion.identity;
			base.gameObject.layer = LayerMask.NameToLayer(VRManager.Instance.Context.GuiLayer);
		}

		protected override void OnStart()
		{
			base.OnStart();
			UpdateAspect();
		}

		protected virtual void OnEnable()
		{
			VRLog.Info("Listen!");
			VRGUI.Instance.Listen();
		}

		public void UpdateMesh()
		{
			MeshFilter component = GetComponent<MeshFilter>();
			Mesh mesh = component.mesh;
			float x = (float)left / (float)virtualScreenWidth;
			float y = (float)bottom / (float)virtualScreenHeight;
			float x2 = (float)(left + width) / (float)virtualScreenWidth;
			float y2 = (float)(bottom + height) / (float)virtualScreenHeight;
			VRLog.Info("Mesh UV for SubGUI");
			for (int i = 0; i < mesh.uv.Length; i++)
			{
				VRLog.Info(mesh.uv[i]);
			}
			VRLog.Info("Mesh UV for SubGUI (Updated)");
			mesh.uv = new Vector2[4]
			{
				new Vector2(x, y),
				new Vector2(x2, y2),
				new Vector2(x2, y),
				new Vector2(x, y2)
			};
			for (int j = 0; j < mesh.uv.Length; j++)
			{
				VRLog.Info(mesh.uv[j]);
			}
			component.mesh = mesh;
		}

		protected virtual void OnDisable()
		{
			VRLog.Info("Unlisten!");
			VRGUI.Instance.Unlisten();
		}

		public virtual void UpdateAspect()
		{
			float y = base.transform.localScale.y;
			float x = y / (float)height * (float)width;
			base.transform.localScale = new Vector3(x, y, 1f);
		}

		public virtual void UpdateGUI()
		{
			UpdateAspect();
			if (!renderer)
			{
				VRLog.Warn("No renderer!");
			}
			try
			{
				renderer.receiveShadows = false;
				renderer.shadowCastingMode = ShadowCastingMode.Off;
				renderer.material = VR.Context.Materials.UnlitTransparentCombined;
				renderer.material.SetTexture("_MainTex", VRGUI.Instance.uGuiTexture);
				renderer.material.SetTexture("_SubTex", VRGUI.Instance.IMGuiTexture);
			}
			catch (Exception obj)
			{
				VRLog.Info(obj);
			}
		}
	}
}
