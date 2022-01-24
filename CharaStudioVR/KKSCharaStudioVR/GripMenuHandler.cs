using System.Linq;
using UnityEngine;
using Valve.VR;
using VRGIN.Controls;
using VRGIN.Core;
using VRGIN.Native;
using VRGIN.Visuals;

namespace KKSCharaStudioVR
{
	public class GripMenuHandler : ProtectedBehaviour
	{
		private class ResizeHandler : ProtectedBehaviour
		{
			private GUIQuad _Gui;

			private Vector3? _StartLeft;

			private Vector3? _StartRight;

			private Vector3? _StartScale;

			private Quaternion? _StartRotation;

			private Vector3? _StartPosition;

			private Quaternion _StartRotationController;

			private Vector3? _OffsetFromCenter;

			public bool IsDragging { get; private set; }

			protected override void OnStart()
			{
				base.OnStart();
				_Gui = GetComponent<GUIQuad>();
			}

			protected override void OnFixedUpdate()
			{
				base.OnFixedUpdate();
				IsDragging = GetDevice(VR.Mode.Left).GetPress(EVRButtonId.k_EButton_Axis1) && GetDevice(VR.Mode.Right).GetPress(EVRButtonId.k_EButton_Axis1);
				if (IsDragging)
				{
					if (!_StartScale.HasValue)
					{
						Initialize();
					}
					Vector3 position = VR.Mode.Left.transform.position;
					Vector3 position2 = VR.Mode.Right.transform.position;
					float num = Vector3.Distance(position, position2);
					float num2 = Vector3.Distance(_StartLeft.Value, _StartRight.Value);
					Vector3 vector = position2 - position;
					Vector3 vector2 = position + vector * 0.5f;
					Quaternion quaternion = Quaternion.Inverse(VR.Camera.SteamCam.origin.rotation);
					Quaternion averageRotation = GetAverageRotation();
					Quaternion quaternion2 = quaternion * averageRotation * Quaternion.Inverse(quaternion * _StartRotationController);
					_Gui.transform.localScale = num / num2 * _StartScale.Value;
					_Gui.transform.localRotation = quaternion2 * _StartRotation.Value;
					_Gui.transform.position = vector2 + averageRotation * Quaternion.Inverse(_StartRotationController) * _OffsetFromCenter.Value;
				}
				else
				{
					_StartScale = null;
				}
			}

			private Quaternion GetAverageRotation()
			{
				Vector3 position = VR.Mode.Left.transform.position;
				Vector3 normalized = (VR.Mode.Right.transform.position - position).normalized;
				Vector3 vector = Vector3.Lerp(VR.Mode.Left.transform.forward, VR.Mode.Right.transform.forward, 0.5f);
				return Quaternion.LookRotation(Vector3.Cross(normalized, vector).normalized, vector);
			}

			private void Initialize()
			{
				_StartLeft = VR.Mode.Left.transform.position;
				_StartRight = VR.Mode.Right.transform.position;
				_StartScale = _Gui.transform.localScale;
				_StartRotation = _Gui.transform.localRotation;
				_StartPosition = _Gui.transform.position;
				_StartRotationController = GetAverageRotation();
				Vector3.Distance(_StartLeft.Value, _StartRight.Value);
				Vector3 vector = _StartRight.Value - _StartLeft.Value;
				Vector3 vector2 = _StartLeft.Value + vector * 0.5f;
				_OffsetFromCenter = base.transform.position - vector2;
			}

			private DeviceLegacyAdapter GetDevice(Controller controller)
			{
				return controller.Input;
			}
		}

		private Controller _Controller;

		private const float RANGE = 0.25f;

		private float scaledRange = 0.25f;

		private const int MOUSE_STABILIZER_THRESHOLD = 30;

		private LineRenderer Laser;

		private Vector2? mouseDownPosition;

		private GUIQuad _Target;

		private ResizeHandler _ResizeHandler;

		private Vector3 _ScaleVector;

		protected DeviceLegacyAdapter Device => _Controller.Input;

		private bool IsResizing
		{
			get
			{
				if ((bool)_ResizeHandler)
				{
					return _ResizeHandler.IsDragging;
				}
				return false;
			}
		}

		public bool LaserVisible
		{
			get
			{
				if ((bool)Laser)
				{
					return Laser.gameObject.activeSelf;
				}
				return false;
			}
			set
			{
				if ((bool)Laser)
				{
					Laser.gameObject.SetActive(value);
					if (value)
					{
						Laser.SetPosition(0, Laser.transform.position);
						Laser.SetPosition(1, Laser.transform.position);
					}
					else
					{
						mouseDownPosition = null;
					}
				}
			}
		}

		public bool IsPressing { get; private set; }

		protected override void OnStart()
		{
			base.OnStart();
			_Controller = base.gameObject.GetComponentInChildren<Controller>(true);
			scaledRange = 0.25f * VR.Context.Settings.IPDScale;
			_ScaleVector = new Vector2((float)VRGUI.Width / (float)Screen.width, (float)VRGUI.Height / (float)Screen.height);
			InitLaser();
		}

		private void InitLaser()
		{
			Laser = new GameObject().AddComponent<LineRenderer>();
			Laser.transform.SetParent(base.transform, false);
			Laser.material = new Material(VR.Context.Materials.Sprite);
			Laser.material.renderQueue += 5000;
			Laser.SetColors(Color.cyan, Color.cyan);
			Laser.transform.localRotation = Quaternion.Euler(60f, 0f, 0f);
			Laser.transform.position += Laser.transform.forward * 0.07f * VR.Context.Settings.IPDScale;
			Laser.SetVertexCount(2);
			Laser.useWorldSpace = true;
			float num = 0.002f * VR.Context.Settings.IPDScale;
			Laser.SetWidth(num, num);
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			if (!VR.Camera.gameObject.activeInHierarchy)
			{
				return;
			}
			if (LaserVisible)
			{
				if (IsResizing)
				{
					Laser.SetPosition(0, Laser.transform.position);
					Laser.SetPosition(1, Laser.transform.position);
				}
				else
				{
					UpdateLaser();
				}
			}
			else if (_Controller.CanAcquireFocus())
			{
				CheckForNearMenu();
			}
			CheckInput();
		}

		private void OnDisable()
		{
			LaserVisible = false;
		}

		private void EnsureResizeHandler()
		{
			if (!_ResizeHandler)
			{
				_ResizeHandler = _Target.GetComponent<ResizeHandler>();
				if (!_ResizeHandler)
				{
					_ResizeHandler = _Target.gameObject.AddComponent<ResizeHandler>();
				}
			}
		}

		private void EnsureNoResizeHandler()
		{
			if ((bool)_ResizeHandler)
			{
				Object.DestroyImmediate(_ResizeHandler);
			}
			_ResizeHandler = null;
		}

		protected void CheckInput()
		{
			IsPressing = false;
			if (LaserVisible && (bool)_Target && !IsResizing)
			{
				if (Device.GetPressDown(EVRButtonId.k_EButton_Axis1))
				{
					IsPressing = true;
					MouseOperations.MouseEvent(WindowsInterop.MouseEventFlags.LeftDown);
					mouseDownPosition = Vector2.Scale(new Vector2(Input.mousePosition.x, (float)Screen.height - Input.mousePosition.y), _ScaleVector);
				}
				if (Device.GetPress(EVRButtonId.k_EButton_Axis1))
				{
					IsPressing = true;
				}
				if (Device.GetPressUp(EVRButtonId.k_EButton_Axis1))
				{
					IsPressing = true;
					MouseOperations.MouseEvent(WindowsInterop.MouseEventFlags.LeftUp);
					mouseDownPosition = null;
				}
			}
		}

		private void CheckForNearMenu()
		{
			_Target = GUIQuadRegistry.Quads.FirstOrDefault(IsLaserable);
			if ((bool)_Target)
			{
				LaserVisible = true;
			}
		}

		private bool IsLaserable(GUIQuad quad)
		{
			RaycastHit hit;
			if (IsWithinRange(quad))
			{
				return Raycast(quad, out hit);
			}
			return false;
		}

		private float GetRange(GUIQuad quad)
		{
			return Mathf.Clamp(quad.transform.localScale.magnitude * scaledRange, scaledRange, scaledRange * 5f);
		}

		private bool IsWithinRange(GUIQuad quad)
		{
			if (quad.transform.parent == base.transform)
			{
				return false;
			}
			Vector3 lhs = -quad.transform.forward;
			_ = quad.transform.position;
			Vector3 position = Laser.transform.position;
			Vector3 forward = Laser.transform.forward;
			float num = (0f - quad.transform.InverseTransformPoint(position).z) * quad.transform.localScale.magnitude;
			if (num > 0f && num < GetRange(quad))
			{
				return Vector3.Dot(lhs, forward) < 0f;
			}
			return false;
		}

		private bool Raycast(GUIQuad quad, out RaycastHit hit)
		{
			Vector3 position = Laser.transform.position;
			Vector3 forward = Laser.transform.forward;
			Collider component = quad.GetComponent<Collider>();
			if ((bool)component)
			{
				Ray ray = new Ray(position, forward);
				return component.Raycast(ray, out hit, GetRange(quad));
			}
			hit = default(RaycastHit);
			return false;
		}

		private void UpdateLaser()
		{
			Laser.SetPosition(0, Laser.transform.position);
			Laser.SetPosition(1, Laser.transform.position + Laser.transform.forward);
			if ((bool)_Target && _Target.gameObject.activeInHierarchy)
			{
				if (IsWithinRange(_Target) && Raycast(_Target, out var hit))
				{
					Laser.SetPosition(1, hit.point);
					if (!IsOtherWorkingOn(_Target))
					{
						Vector2 b = new Vector2(hit.textureCoord.x * (float)VRGUI.Width, (1f - hit.textureCoord.y) * (float)VRGUI.Height);
						if (!mouseDownPosition.HasValue || Vector2.Distance(mouseDownPosition.Value, b) > 30f)
						{
							MouseOperations.SetClientCursorPosition((int)b.x, (int)b.y);
							mouseDownPosition = null;
						}
					}
				}
				else
				{
					LaserVisible = false;
				}
			}
			else
			{
				LaserVisible = false;
			}
		}

		private bool IsOtherWorkingOn(GUIQuad target)
		{
			return false;
		}
	}
}
