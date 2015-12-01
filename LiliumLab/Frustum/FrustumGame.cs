using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lilium;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace LiliumLab
{
	public class FrustumGame : Game
	{
		FrustumRenderer f;
		Matrix viewProjMatrix;

		float Distance = 5;
		Vector4 positionCS;

		protected override void OnStart()
		{
			f = new FrustumRenderer(this, new BoundingFrustum(viewProjMatrix));
			f.Create3D();
			AutoDispose(f);

			var slider = new Lilium.Controls.Slider("Distance", 0, 100, () => Distance, val => Distance = val);
			AddControl(slider);
			var label = new Lilium.Controls.Label("Clip Space", () => positionCS.ToString("0.000"));
			AddControl(label);
		}

		protected override void OnUpdate()
		{
			var light = Light.MainLight;
			var view = Matrix.LookAtLH(light.LightPos, light.LightPos - light.LightDirection, Vector3.Up);
			var proj = Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1, 0.1f, 100);
			viewProjMatrix = Matrix.Multiply(view, proj);

			Vector4 v = new Vector4(light.LightPos - Distance * light.LightDirection, 1);
			positionCS = Vector4.Transform(v, viewProjMatrix);

			f.UpdateFrustum(new BoundingFrustum(viewProjMatrix));
			f.Draw();
		}
	}
}
