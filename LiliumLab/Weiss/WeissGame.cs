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
	public class WeissGame : Game
	{
		protected override void OnStart()
		{
			ResourceManager.SearchPaths.Add("../../Weiss");

			Light.MainLight.AmbientColor = Vector4.One * 0.5f;
			Light.MainLight.DiffuseColor = Vector4.One * 0.5f;
		}

		protected override void OnUpdate()
		{

		}
	}
}
