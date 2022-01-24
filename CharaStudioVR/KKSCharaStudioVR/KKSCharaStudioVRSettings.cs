using System.Xml.Serialization;
using VRGIN.Core;

namespace KKSCharaStudioVR
{
	[XmlRoot("Settings")]
	public class KKSCharaStudioVRSettings : VRSettings
	{
		private bool _LockRotXZ = true;

		private float _MaxVoiceDistance = 300f;

		private float _MinVoiceDistance = 7f;

		[XmlComment("Lock XZ Axis (pitch / roll) rotation.")]
		public bool LockRotXZ
		{
			get
			{
				return _LockRotXZ;
			}
			set
			{
				_LockRotXZ = value;
				TriggerPropertyChanged("LockRotXZ");
			}
		}

		[XmlComment("Max Voice distance (in unit. 300 = 30m in real (HS2 uses 10 unit = 1m scale).")]
		public float MaxVoiceDistance
		{
			get
			{
				return _MaxVoiceDistance;
			}
			set
			{
				_MaxVoiceDistance = value;
				TriggerPropertyChanged("MaxVoiceDistance");
			}
		}

		[XmlComment("Min Voice distance (in unit. 7 = 70 cm in real (HS2 uses 10 unit = 1m scale).")]
		public float MinVoiceDistance
		{
			get
			{
				return _MinVoiceDistance;
			}
			set
			{
				_MinVoiceDistance = value;
				TriggerPropertyChanged("MinVoiceDistance");
			}
		}

		[XmlIgnore]
		public override Shortcuts Shortcuts
		{
			get
			{
				return base.Shortcuts;
			}
			protected set
			{
				base.Shortcuts = value;
			}
		}

		public KKSCharaStudioVRSettings()
		{
			base.IPDScale = 1f;
			base.GrabRotationImmediateMode = false;
		}

		public static KKSCharaStudioVRSettings Load(string path)
		{
			return VRSettings.Load<KKSCharaStudioVRSettings>(path);
		}
	}
}
