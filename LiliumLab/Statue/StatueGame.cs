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
		RenderTexture rtSSAOGeometry;
		RenderTexture rtAmbientIndensity;

		Material materialSSAOBuffer;
		Postprocess ppSSAO;

		protected override void OnStart()
		{
			ResourceManager.SearchPaths.Add("../../Statue/");

			rtSSAOGeometry = new RenderTexture(this, 2);
			AutoDispose(rtSSAOGeometry);

			rtAmbientIndensity = new RenderTexture(this, 1);
			AutoDispose(rtAmbientIndensity);

			entity = new Entity("knight_statue.obj");
			AutoDispose(entity);

			materialSSAOBuffer = ResourceManager.Material.Load("SSAOBuffer.lm");

			ppSSAO = new Postprocess(this, "SSAOPostprocess.hlsl", rtAmbientIndensity);
			AutoDispose(ppSSAO);
			ppSSAO.SetShaderResourceViews(rtSSAOGeometry.GetShaderResourceViews());
		}

		protected override void OnUpdate()
		{
			rtSSAOGeometry.Begin();
			entity.DrawWithMaterial(materialSSAOBuffer);
			rtSSAOGeometry.End();

			ppSSAO.Draw();
		}
	}
}
