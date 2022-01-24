using System;
using System.Collections;
using System.Collections.Generic;
using Studio;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace KKSCharaStudioVR
{
	public class IKTool : MonoBehaviour
	{
		private bool markerShowOverlay = true;

		private float markerOverlayAlpha = 0.4f;

		private float markerSize = 0.06f;

		private float markerSizeBend = 0.05f;

		private Material markerSharedMaterial;

		private Material markerSharedMaterial_IKTarget;

		private Material markerSharedMaterial_IKBendTarget;

		private GameObject handle;

		public static IKTool instance;

		private const float DEFAUTL_SCALE_POS = 0.25f;

		private float DEFAULT_SCALE_POS_XYZ_DIST = Mathf.Sqrt(0.1875f);

		public static IKTool Create(GameObject container)
		{
			if (instance != null)
			{
				return instance;
			}
			instance = container.AddComponent<IKTool>();
			return instance;
		}

		private void Awake()
		{
			SceneManager.sceneLoaded += OnSceneWasLoaded;
		}

		private void Start()
		{
			StartWatch();
		}

		private IEnumerator InstallMoveableObjectCo()
		{
			global::Studio.Studio studio = Singleton<global::Studio.Studio>.Instance;
			_ = Singleton<GuideObjectManager>.Instance;
			if (studio == null)
			{
				yield break;
			}
			while (true)
			{
				yield return new WaitForSeconds(1f);
				try
				{
					foreach (KeyValuePair<int, ObjectCtrlInfo> item in studio.dicObjectCtrl)
					{
						if (item.Value == null)
						{
							continue;
						}
						ObjectCtrlInfo value = item.Value;
						if (!(value.guideObject != null) || !(value.guideObject.gameObject != null))
						{
							continue;
						}
						MakeObjectMoveable(value.guideObject, true, true);
						if (!(value is OCIChar))
						{
							continue;
						}
						OCIChar oCIChar = value as OCIChar;
						foreach (OCIChar.IKInfo item2 in oCIChar.listIKTarget)
						{
							MakeObjectMoveable(item2.guideObject, true);
						}
						foreach (OCIChar.BoneInfo listBone in oCIChar.listBones)
						{
							MakeObjectMoveable(listBone.guideObject, true);
						}
					}
				}
				catch (Exception value2)
				{
					Console.WriteLine(value2);
				}
			}
		}

		private void OnSceneWasLoaded(Scene scene, LoadSceneMode sceneMode)
		{
			StartWatch();
		}

		private void StartWatch()
		{
			StopAllCoroutines();
			StartCoroutine(InstallMoveableObjectCo());
			if (handle == null)
			{
				handle = new GameObject("handle");
				handle.transform.parent = base.gameObject.transform;
			}
		}

		private void MakeObjectMoveable(GuideObject guideObject, bool replaceMaterial = false, bool installToCenter = false)
		{
			if (!(guideObject.transformTarget == null))
			{
				if (installToCenter)
				{
					InstallGripMoveMarker(guideObject.gameObject, OnObjectMove, guideObject, replaceMaterial, installToCenter);
				}
				else
				{
					GameObject target = guideObject.gameObject.transform.Find("Sphere").gameObject;
					InstallGripMoveMarker(target, OnObjectMove, guideObject, replaceMaterial, installToCenter);
				}
				if (guideObject.enableScale)
				{
					InstallScaleMoveMarker(guideObject);
				}
			}
		}

		private bool InstallGripMoveMarker(GameObject target, Action<MonoBehaviour> moveHandler, GuideObject guideObject, bool replaceMaterial, bool installToCenter)
		{
			if (target.transform.Find("_gripmovemarker") == null)
			{
				Renderer visibleReference = null;
				GameObject gameObject;
				if (installToCenter)
				{
					gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					gameObject.name = "_gripmovemarker";
					gameObject.layer = LayerMask.NameToLayer("Studio/Col");
					Renderer component = gameObject.GetComponent<Renderer>();
					Material material = new Material(MaterialHelper.GetColorZOrderShader());
					material.color = new Color(0f, 1f, 0f, 0.3f);
					material.SetFloat("_AlphaRatio", 0.5f);
					material.renderQueue = 3800;
					component.material = material;
					component.shadowCastingMode = ShadowCastingMode.Off;
					component.receiveShadows = false;
					Transform transform = target.transform.Find("move/XYZ");
					if (transform != null)
					{
						visibleReference = transform.gameObject.GetComponent<Renderer>();
					}
				}
				else
				{
					gameObject = new GameObject("_gripmovemarker");
					if (replaceMaterial)
					{
						Renderer component2 = target.GetComponent<Renderer>();
						if (component2 != null)
						{
							Material material2 = new Material(MaterialHelper.GetColorZOrderShader());
							material2.CopyPropertiesFromMaterial(component2.material);
							material2.SetFloat("_AlphaRatio", 0.5f);
							material2.renderQueue = 3800;
							component2.material = material2;
							component2.shadowCastingMode = ShadowCastingMode.Off;
							component2.receiveShadows = false;
							visibleReference = component2;
						}
					}
				}
				SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
				Transform obj = gameObject.transform;
				obj.transform.parent = target.transform;
				obj.transform.localPosition = Vector3.zero;
				obj.transform.rotation = guideObject.transformTarget.rotation;
				obj.transform.localScale = Vector3.one;
				sphereCollider.isTrigger = true;
				MoveableGUIObject moveableGUIObject = gameObject.AddComponent<MoveableGUIObject>();
				moveableGUIObject.guideObject = guideObject;
				moveableGUIObject.onMoveLister.Add(moveHandler);
				moveableGUIObject.visibleReference = visibleReference;
				if (installToCenter)
				{
					moveableGUIObject.isMoveObj = true;
				}
				return true;
			}
			return false;
		}

		private bool InstallScaleMoveMarker(GuideObject guideObject)
		{
			Transform transform = guideObject.gameObject.transform.Find("scale");
			if (transform.transform.Find("X/_gripmovemarker_scale") == null)
			{
				string[] array = new string[4] { "XYZ", "X", "Y", "Z" };
				foreach (string n in array)
				{
					Transform transform2 = transform.Find(n);
					GuideScale component = transform2.gameObject.GetComponent<GuideScale>();
					if (component != null)
					{
						GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
						obj.name = "_gripmovemarker_scale";
						obj.layer = LayerMask.NameToLayer("Studio/Col");
						Renderer component2 = obj.GetComponent<Renderer>();
						Material material = new Material(MaterialHelper.GetColorZOrderShader());
						material.color = new Color(0f, 1f, 1f, 0.3f);
						material.SetFloat("_AlphaRatio", 0.5f);
						material.renderQueue = 3800;
						component2.material = material;
						component2.shadowCastingMode = ShadowCastingMode.Off;
						component2.receiveShadows = false;
						Renderer renderer = null;
						renderer = transform2.gameObject.GetComponent<Renderer>();
						SphereCollider sphereCollider = obj.AddComponent<SphereCollider>();
						Transform obj2 = obj.transform;
						obj2.transform.parent = transform2;
						obj2.transform.localPosition = CalcScaleHandleDefaultPos(component);
						obj2.transform.rotation = guideObject.transformTarget.rotation;
						obj2.transform.localScale = Vector3.one;
						sphereCollider.isTrigger = true;
						MoveableGUIObject moveableGUIObject = obj.AddComponent<MoveableGUIObject>();
						moveableGUIObject.guideObject = guideObject;
						moveableGUIObject.guideScale = component;
						moveableGUIObject.onMoveLister.Add(OnScaleMove);
						moveableGUIObject.onReleasedLister.Add(OnScaleReleased);
						moveableGUIObject.visibleReference = renderer;
					}
				}
				return true;
			}
			return false;
		}

		private void OnObjectMove(MonoBehaviour marker)
		{
			DoOnMove(marker, marker.transform.parent);
		}

		private void OnObjectCubeMoveNoRotation(MonoBehaviour marker)
		{
			DoOnMove(marker, marker.transform.parent.parent, false);
		}

		private void OnObjectRotationNoMove(MonoBehaviour marker)
		{
			DoOnMove(marker, marker.transform.parent, true, false);
		}

		private void OnRawObjectMove(MonoBehaviour marker)
		{
			DoOnMove(marker, marker.transform.parent);
		}

		private void DoOnMove(MonoBehaviour marker, Transform target, bool rotation = true, bool pos = true)
		{
			MoveableGUIObject component = marker.GetComponent<MoveableGUIObject>();
			Transform parent = marker.transform.parent;
			GuideObject guideObject = component.guideObject;
			pos &= guideObject.enablePos;
			rotation &= guideObject.enableRot;
			if (pos)
			{
				target.position += marker.transform.position - parent.transform.position;
				guideObject.transformTarget.transform.position = target.position;
				guideObject.changeAmount.pos = guideObject.transformTarget.localPosition;
			}
			marker.transform.localPosition = Vector3.zero;
			if (rotation)
			{
				guideObject.transformTarget.rotation = marker.transform.rotation;
				guideObject.changeAmount.rot = guideObject.transformTarget.localEulerAngles;
			}
		}

		private void OnScaleMove(MonoBehaviour marker)
		{
			MoveableGUIObject component = marker.GetComponent<MoveableGUIObject>();
			_ = marker.transform.parent;
			GuideObject guideObject = component.guideObject;
			GuideScale guideScale = component.guideScale;
			if (!guideObject.enableScale || !component.guideScale)
			{
				return;
			}
			float magnitude = marker.transform.localPosition.magnitude;
			if (magnitude > 0f)
			{
				float num = magnitude / 0.25f;
				Vector3 oldScale = component.oldScale;
				switch (guideScale.axis)
				{
				case GuideScale.ScaleAxis.XYZ:
					oldScale *= num;
					break;
				case GuideScale.ScaleAxis.X:
					oldScale.x *= num;
					break;
				case GuideScale.ScaleAxis.Y:
					oldScale.y *= num;
					break;
				case GuideScale.ScaleAxis.Z:
					oldScale.z *= num;
					break;
				}
				oldScale.x = Mathf.Max(oldScale.x, 0.01f);
				oldScale.y = Mathf.Max(oldScale.y, 0.01f);
				oldScale.z = Mathf.Max(oldScale.z, 0.01f);
				guideObject.changeAmount.scale = oldScale;
			}
		}

		private void OnScaleReleased(MonoBehaviour marker)
		{
			MoveableGUIObject component = marker.GetComponent<MoveableGUIObject>();
			GuideObject guideObject = component.guideObject;
			GuideScale guideScale = component.guideScale;
			if (guideObject.enableScale && (bool)guideScale)
			{
				marker.transform.localPosition = CalcScaleHandleDefaultPos(guideScale);
			}
		}

		private Vector3 CalcScaleHandleDefaultPos(GuideScale guideScale)
		{
			return guideScale.axis switch
			{
				GuideScale.ScaleAxis.XYZ => new Vector3(0.25f, 0.25f, 0.25f) * 0.25f / DEFAULT_SCALE_POS_XYZ_DIST, 
				GuideScale.ScaleAxis.X => new Vector3(0.25f, 0f, 0f), 
				GuideScale.ScaleAxis.Y => new Vector3(0f, 0.25f, 0f), 
				GuideScale.ScaleAxis.Z => new Vector3(0f, 0f, 0.25f), 
				_ => Vector3.zero, 
			};
		}

		private Material CreateForceDrawMaterial()
		{
			try
			{
				Material material = new Material(MaterialHelper.GetColorZOrderShader());
				Color red = Color.red;
				red.a = markerOverlayAlpha;
				material.SetColor("_Color", red);
				return material;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return null;
			}
		}
	}
}
