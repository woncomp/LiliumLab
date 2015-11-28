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
		struct ShadowMapData
		{
			public Matrix LightViewMatrix;
			public Matrix LightProjectionMatrix;
			public float ShadowBias;
			public Vector3 LightPosV;
			public int ShadowMapSize;
			public Vector3 ___;
		}

		const int MAX_KERNEL_SIZE = 64;
		const int SHADOW_MAP_SIZE = 1024;

		Entity entityStatue;
		Entity entityPlane;

		// Defered
		RenderTexture rtGBuffer;
		ShaderResourceView gBufferPosW;
		ShaderResourceView gBufferPosV;
		ShaderResourceView gBufferNormalV;

		Material materialGBufer;

		// ShadowMapping
		Material materialShadowMap;
		Buffer bufferShadowMap;
		RenderTexture rtShadowMap;

		ShadowMapData shadowMapData;

		Postprocess ppShadowMapping;

		// SSAO
		RenderTexture rtSSAO;
		Postprocess ppSSAO;

		RenderTexture rtBlur;
		Postprocess ppBlur;

		float[] ppSSAOData;
		Buffer ppSSAObuffer;

		Random r = new Random();

		[Slider(0f, 0.5f)]
		float SSAORadius = 0.2f;

		[Slider(0f, 5f)]
		float ShadowBias = 3;

		protected override void OnStart()
		{
			ResourceManager.SearchPaths.Add("../../Statue/");
			Light.MainLight.AmbientColor = new Vector4(0.5f, 0.5f, 0.5f, 1);
			Light.MainLight.DiffuseColor = new Vector4(0.5f, 0.5f, 0.5f, 1);
			Light.MainLight.LightDistance = 30;

			// GBuffer
			rtGBuffer = new RenderTexture(this, 3, "GBuffers");
			AutoDispose(rtGBuffer);
			var gBuffers = rtGBuffer.GetShaderResourceViews();
			gBufferPosW = gBuffers[0];
			gBufferPosV = gBuffers[1];
			gBufferNormalV = gBuffers[2];

			materialGBufer = ResourceManager.Material.Load("GBuffer.lm");

			// ShadowMap
			materialShadowMap = ResourceManager.Material.Load("ShadowMap.lm");
			bufferShadowMap = Material.CreateBuffer<ShadowMapData>();
			AutoDispose(bufferShadowMap);
			rtShadowMap = new RenderTexture(this, SHADOW_MAP_SIZE, SHADOW_MAP_SIZE, 1, "ShadowMap");
			AutoDispose(rtShadowMap);

			ppShadowMapping = new Postprocess(this, "Shadow.hlsl");
			AutoDispose(ppShadowMapping);
			{
				var views = new ShaderResourceView[4];
				views[0] = gBufferPosW;
				views[1] = rtShadowMap.ShaderResourceView;
				views[2] = gBufferNormalV;
				views[3] = gBufferPosV;
				ppShadowMapping.SetShaderResourceViews(views);

				var desc = SamplerStateDescription.Default();
				desc.Filter = Filter.MinMagMipLinear;
				ppShadowMapping.SetSamplerState(0, desc);
				ppShadowMapping.SetSamplerState(1, desc);
				ppShadowMapping.SetSamplerState(2, desc);
				ppShadowMapping.SetSamplerState(3, desc);
			}
			{
				var desc = BlendStateDescription.Default();
				desc.RenderTarget[0].IsBlendEnabled = true;
				desc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
				desc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
				desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
				desc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
				desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
				desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
				desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

				ppShadowMapping.Pass.BlendState.Dispose();
				ppShadowMapping.Pass.BlendState = new BlendState(Device, desc);
			}

			// SSAO
			rtSSAO = new RenderTexture(this, 1, "SSAO");
			AutoDispose(rtSSAO);

			rtBlur = new RenderTexture(this, 1, "Blur");
			AutoDispose(rtBlur);

			CreateSSAOPass();

			CreateKernelVectors();
			ppSSAObuffer = new Buffer(Device, 4 * ppSSAOData.Length, ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
			AutoDispose(ppSSAObuffer);

			ppBlur = new Postprocess(this, "SSAOBlur.hlsl", rtBlur);
			AutoDispose(ppBlur);
			ppBlur.SetShaderResourceViews(rtSSAO.GetShaderResourceViews());

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
			ppSSAO = new Postprocess(this, "SSAOPostprocess.hlsl", rtSSAO);
			AutoDispose(ppSSAO);
			var views = new ShaderResourceView[3];
			views[0] = gBufferPosV;
			views[1] = gBufferNormalV;
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
			// Update light matrices;
			{
				var light = Light.MainLight;
				shadowMapData.LightViewMatrix = Matrix.LookAtLH(light.LightPos, light.LightPos - light.LightDirection, Vector3.Up);
				//shadowMapData.LightProjectionMatrix = Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1, 0.01f, 1000);
				shadowMapData.LightProjectionMatrix = Matrix.OrthoLH(50, 50, 0.01f, 100);
				shadowMapData.ShadowBias = (float)Math.Pow(0.1, ShadowBias);
				shadowMapData.LightPosV = Vector3.TransformCoordinate(light.LightPos, Camera.ActiveCamera.ViewMatrix);
				shadowMapData.ShadowMapSize = SHADOW_MAP_SIZE;
				shadowMapData.___ = Vector3.Zero;
			}

			// Render G-Buffers
			rtGBuffer.Begin();
			entityStatue.DrawWithMaterial(materialGBufer);
			entityPlane.DrawWithMaterial(materialGBufer);
			rtGBuffer.End();

			// Render shadow map
			rtShadowMap.Begin();
			DeviceContext.UpdateSubresource(ref shadowMapData, bufferShadowMap);
			DeviceContext.VertexShader.SetConstantBuffer(0, bufferShadowMap);
			entityPlane.DrawWithMaterial(materialShadowMap);
			entityStatue.DrawWithMaterial(materialShadowMap);
			rtShadowMap.End();

			// Render SSAO map
			ppSSAOData[0] = SSAORadius;
			float[] proj = Camera.ActiveCamera.ProjectionMatrix.ToArray();
			Array.Copy(proj, 0, ppSSAOData, 4, 16);
			DeviceContext.UpdateSubresource(ppSSAOData, ppSSAObuffer);
			DeviceContext.PixelShader.SetConstantBuffer(0, ppSSAObuffer);
			ppSSAO.Draw();

			// Blur SSAO map
			ppBlur.Draw();

			// Draw Scene with SSAO
			entityStatue.SubmeshMaterials[0].Passes[0].TextureList[1] = rtBlur.ShaderResourceView;
			entityStatue.Draw();
			entityPlane.SubmeshMaterials[0].Passes[0].TextureList[1] = rtBlur.ShaderResourceView;
			entityPlane.Draw();

			// Draw Shadow
			DeviceContext.PixelShader.SetConstantBuffer(0, bufferShadowMap);
			ppShadowMapping.Draw();
		}
	}
}
