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
	public class StatueGame : Game
	{
		Entity entity;
		RenderTexture rt;

		protected override void OnStart()
		{
			ResourceManager.SearchPaths.Add("../../Statue/");

			rt = new RenderTexture(this);
			AutoDispose(rt);

			entity = new Entity("knight_statue.obj");
			AutoDispose(entity);
		}

		protected override void OnUpdate()
		{
			rt.Begin();
			entity.Draw();
			rt.End();
		}
	}
}
