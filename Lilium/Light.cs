using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Lilium
{
	public class Light : ISelectable
	{
		public static Light MainLight;

		public string Name;

		public bool DrawLight = true;

		public Vector3 LightDirection = new Vector3(0, 1, 0);
		public Vector4 LightDir4 { get { return new Vector4(LightDirection, 0); } }
		public Vector3 LightPos;

		public Vector4 AmbientColor = new Vector4(0.4f, 0.4f, 0.4f, 1);
		public Vector4 DiffuseColor = new Vector4(0.6f, 0.6f, 0.6f, 1);

		public float LightDistance = 10;
		public float LightYaw = 160;
		public float LightPitch = -45;

		public Light()
		{
			Name = "Light " + Debug.NextObjectId;
			CreateControls();
		}
		
		public void Update()
		{
			var game = Game.Instance;
			if (this == MainLight)
			{
				if (game.Input.GetMouseButton(System.Windows.Forms.MouseButtons.Right))
				{
					var mouseLocDelta = game.Input.MouseLocationDelta;
					var dx = mouseLocDelta.Width;
					var dy = mouseLocDelta.Height;

					LightYaw -= dx * 0.2f;
					LightPitch += dy * 0.2f;
					if (LightPitch > 89) LightPitch = 89;
					if (LightPitch < -89) LightPitch = -89;
				}
			}

			var lightYawR = MathUtil.DegreesToRadians(LightYaw);
			var lightPitchR = MathUtil.DegreesToRadians(LightPitch);
			LightPos = Vector3.TransformCoordinate(Vector3.ForwardLH, Matrix.RotationYawPitchRoll(lightYawR, lightPitchR, 0)) * LightDistance;
			LightDirection = LightPos;
			LightDirection.Normalize();

			if (DrawLight)
			{
				var dir1 = -LightDirection;
				var dir2 = dir1;
				var temp = dir2.X;
				dir2.X = dir2.Y; dir2.Y = dir2.Z; dir2.Z = temp;

				var normal = Vector3.Cross(dir1, dir2);

				var lightLineStartPoints = new Vector3[6];
				for (int i = 0; i < 6; ++i)
				{
					Quaternion q = Quaternion.RotationAxis(dir1, i * MathUtil.Pi * 2 / 6);
					var mat = Matrix.RotationQuaternion(q);
					var dir3 = Vector3.TransformNormal(normal, mat);
					lightLineStartPoints[i] = LightPos + dir3 * LightDistance * 0.01f;
				}
				{
					for (int i = 0; i < 6; ++i)
					{
						var start = lightLineStartPoints[i];
						Debug.Line(start, start - LightDirection * LightDistance * 0.1f, Color.Yellow);
					}
				}
			}
		}

		#region Selectable

		private Controls.Control[] controls;

		void CreateControls()
		{
			var lightInfo = new Lilium.Controls.Label("Light Dir", () => LightDirection.ToString("0.000"));
			var lightSlider = new Lilium.Controls.Slider("Light Distance", 1, 100, () => LightDistance, val => LightDistance = val);
			var lightToggle = new Lilium.Controls.Toggle("Draw Light", () => DrawLight, val => DrawLight = val);
			var ambient = new Lilium.Controls.ColorPicker("Ambient Color", () => AmbientColor, val => AmbientColor = val);
			var diffuse = new Lilium.Controls.ColorPicker("Diffuse Color", () => DiffuseColor, val => DiffuseColor = val);
			controls = new Controls.Control[] { lightInfo, lightSlider, lightToggle, ambient, diffuse };
		}
		public Controls.Control[] Controls { get { return controls; } }
		public string TextOnList { get { return Name; } }

		#endregion
	}
}
