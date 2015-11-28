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
		const int MAX_KERNEL_SIZE = 64;

		RenderTexture rtSSAOGeometry;
		RenderTexture rtAmbientIndensity;
		RenderTexture rtBlur;

		Material materialSSAOBuffer;
		Postprocess ppSSAO;

		float[] ppSSAOData;
		Buffer ppSSAObuffer;

		Postprocess ppBlur;

		Entity entityStatue;
		Entity entityPlane;

		int kernelIndex = 0;
		Random r = new Random();

		[Slider(0f, 0.5f)]
		float SSAORadius = 0.2f;

		protected override void OnStart()
		{
			ResourceManager.SearchPaths.Add("../../Statue/");
			Light.MainLight.AmbientColor = new Vector4(0.5f, 0.5f, 0.5f, 1);
			Light.MainLight.DiffuseColor = new Vector4(0.5f, 0.5f, 0.5f, 1);

			rtSSAOGeometry = new RenderTexture(this, 2);
			AutoDispose(rtSSAOGeometry);

			rtAmbientIndensity = new RenderTexture(this);
			AutoDispose(rtAmbientIndensity);

			rtBlur = new RenderTexture(this);
			AutoDispose(rtBlur);

			materialSSAOBuffer = ResourceManager.Material.Load("SSAOBuffer.lm");

			CreateSSAOPass();

			CreateKernelVectors();
			ppSSAObuffer = new Buffer(Device, 4 * ppSSAOData.Length, ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
			AutoDispose(ppSSAObuffer);

			ppBlur = new Postprocess(this, "SSAOBlur.hlsl", rtBlur);
			AutoDispose(ppBlur);
			ppBlur.SetShaderResourceViews(rtAmbientIndensity.GetShaderResourceViews());

			entityStatue = new Entity("knight_statue.obj");
			entityStatue.Position = new Vector3(0, -0.33f, 0);
			AutoDispose(entityStatue);
			AddObject(entityStatue);

			for (int i = 0; i < entityStatue.Mesh.SubmeshCount; ++i)
			{
				entityStatue.SetMaterial(i, "Statue.lm");
			}

			entityPlane = new Entity(InternalResources.MESH_PLANE);
			entityPlane.Scale = new Vector3(2, 2, 2);
			AutoDispose(entityPlane);
			AddObject(entityPlane);

			for (int i = 0; i < entityPlane.Mesh.SubmeshCount; ++i)
			{
				entityPlane.SetMaterial(i, "Ground.lm");
			}

			//this.AddControl(new Lilium.Controls.Button("Next Kernel", () =>
			//{
			//	float segment = 1.0f / 64;
			//	int i = ++kernelIndex;
			//	int s = 4 + 16;
			//	Vector3 v;
			//	v.X = r.NextFloat(-1, 1);
			//	v.Y = r.NextFloat(-1, 1);
			//	v.Z = r.NextFloat(0, 1);
			//	v.Normalize();
			//	float scale = i * segment;
			//	scale = MathUtil.Lerp(0.1f, 1.0f - segment, scale * scale) + r.NextFloat(0, 1) * segment;
			//	ppSSAOData[s + 0] = v[0];
			//	ppSSAOData[s + 1] = v[1];
			//	ppSSAOData[s + 2] = v[2];
			//	ppSSAOData[s + 3] = 0;
			//}));
		}

		void CreateSSAOPass()
		{
			ppSSAO = new Postprocess(this, "SSAOPostprocess.hlsl", rtAmbientIndensity);
			AutoDispose(ppSSAO);
			var views = rtSSAOGeometry.GetShaderResourceViews();
			Array.Resize(ref views, 3);
			views[2] = ResourceManager.Tex2D.Load("kernel_rotation.png");
			ppSSAO.SetShaderResourceViews(views);

			var desc = SamplerStateDescription.Default();
			desc.Filter = Filter.MinMagMipPoint;
			desc.AddressU = TextureAddressMode.Wrap;
			desc.AddressV = TextureAddressMode.Wrap;
			ppSSAO.SetSamplerState(2, desc);
		}

		void CreateKernelVectors()
		{
			ppSSAOData = new float[MAX_KERNEL_SIZE * 4 + 4 + 16];
			float segment = 1.0f / MAX_KERNEL_SIZE;
			for (int i = 0; i < MAX_KERNEL_SIZE; ++i)
			{
				int s = 4 + 16 + i * 4;

				Vector3 v;
				v.X = r.NextFloat(-1, 1);
				v.Y = r.NextFloat(-1, 1);
				v.Z = r.NextFloat(0.1f, 1);
				v.Normalize();
				float scale = i * segment;
				scale = MathUtil.Lerp(0.1f, 1.0f - segment, scale * scale) + r.NextFloat(0, 1) * segment;
				ppSSAOData[s + 0] = v[0];
				ppSSAOData[s + 1] = v[1];
				ppSSAOData[s + 2] = v[2];
				ppSSAOData[s + 3] = 0;
			}
		}

		protected override void OnUpdate()
		{
			rtSSAOGeometry.Begin();
			entityStatue.DrawWithMaterial(materialSSAOBuffer);
			entityPlane.DrawWithMaterial(materialSSAOBuffer);
			rtSSAOGeometry.End();

			ppSSAOData[0] = SSAORadius;
			float[] proj = Camera.ActiveCamera.ProjectionMatrix.ToArray();
			Array.Copy(proj, 0, ppSSAOData, 4, 16);
			DeviceContext.UpdateSubresource(ppSSAOData, ppSSAObuffer);
			DeviceContext.PixelShader.SetConstantBuffer(0, ppSSAObuffer);
			ppSSAO.Draw();

			ppBlur.Draw();

			entityStatue.SubmeshMaterials[0].Passes[0].TextureList[1] = rtBlur.ShaderResourceView;
			entityStatue.Draw();
			entityPlane.SubmeshMaterials[0].Passes[0].TextureList[1] = rtBlur.ShaderResourceView;
			entityPlane.Draw();
		}
	}
}
