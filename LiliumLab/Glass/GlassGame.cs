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
	public class GlassGame : Game
	{
		
		protected override void OnStart()
		{
			ResourceManager.SearchPaths.Add("../../Glass");

			SkyBox = new SkyBox(this, ResourceManager.Tex2D.Load(InternalResources.CUBE_SNOW));

			var entity = new Entity(InternalResources.MESH_TEAPOT);
			entity.SetMaterial(0, "Glass.lm");
			MainScene.Entities.Add(entity);
			AddObject(entity);
		}

		protected override void OnUpdate()
		{
			
		}
	}
}
