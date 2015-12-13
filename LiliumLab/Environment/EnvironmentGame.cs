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
	public class EnvironmentGame : Game
	{
		ShaderResourceView tex;
		RenderCubemap cubemap;
		
		protected override void OnStart()
		{
			ResourceManager.SearchPaths.Add("../../Environment");

			tex = ResourceManager.Tex2D.Load("Yokohama.dds");

			this.SkyBox = new SkyBox(this, tex);

			{
				var entity = new Entity(InternalResources.MESH_TEAPOT);
				entity.SetMaterial(0, "Wood.lm");
				entity.Position = new Vector3(15, 4, 7);
				MainScene.Entities.Add(entity);
				AddObject(entity);
			}
			{
				var entity = new Entity(InternalResources.MESH_CUBE);
				entity.SetMaterial(0, "NormalMapping.lm");
				entity.Position = new Vector3(-9, 5, 10);
				MainScene.Entities.Add(entity);
				AddObject(entity);
			}
			{
				cubemap = new RenderCubemap(this, 256);
				AutoDispose(cubemap);
				AddObject(cubemap);
				var entity = new Entity(InternalResources.MESH_SPHERE);
				entity.SetMaterial(0, "Environment.lm");
				entity.Cubemap = cubemap;
				entity.SubmeshMaterials[0].Passes[0].BindRealtimeCubemap(0, entity);
				entity.Scale = Vector3.One * 0.5f;
				MainScene.Entities.Add(entity);
				AddObject(entity);
			}
		}

		protected override void OnUpdate()
		{
			
		}
	}
}
