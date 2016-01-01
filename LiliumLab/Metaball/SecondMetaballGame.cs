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
using Motion = LiliumLab.ScreenSpaceMetaballGame.Motion;

namespace LiliumLab
{
	public class SecondMetaballGame : Game
	{
		const int GAUSSIAN_TEXSIZE = 64;
		const int GAUSSIAN_HEIGHT = 1;
		const float GAUSSIAN_DEVIATION = 0.125f;
		static readonly Color[] COLORS = new[] { Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Orange, Color.Magenta };

		private Motion[] motions;

		private Mesh mesh;

		private ShaderResourceView blobGaussian;
		private ShaderResourceView cubeMap;

		private Material material;

		private RenderTexture rt;
		private Postprocess pp;

		private Buffer shaderBufferRT;
		private Buffer shaderBufferPP;

		[Slider(0, 1)]
		float AnimationSpeed = 1;

		[Slider(0, 5)]
		float Origin = 4;

		[Slider(1, 10)]
		float Distance = 4;

		[Slider(1, 10)]
		float BlobSize = 7;

		[Slider(0.001f, 0.1f)]
		float Threshold = 0.04f;

		[Slider(0, 4)]
		float RimPower = 1;

		[Slider(0, 3)]
		float RimIndensity = 1;
		
		protected override void OnStart()
		{
			ResourceManager.SearchPaths.Add("../../Metaball");

			Random r = new Random();

			motions = new Motion[COLORS.Length];

			for (int i = 0; i < COLORS.Length; ++i)
			{
				motions[i] = new Motion(r);
				motions[i].Color = COLORS[i].ToVector4();
			}

			mesh = Mesh.CreateQuad(1);
			AutoDispose(mesh);

			GenerateGaussianTexture();
			cubeMap = ResourceManager.Tex2D.Load("LobbyCube.dds");

			material = this.ResourceManager.Material.Load("Meta2.lm");
			material.Passes[0].BindShaderResource(0, blobGaussian);

			rt = new RenderTexture(this, 1, "MetaRT");
			AutoDispose(rt);
			rt.SetClearColor(0, new Color4(0, 0, 0, 0));

			pp = new Postprocess(this, "Meta2PP.hlsl");
			AutoDispose(pp);
			var ppTextures = rt.GetShaderResourceViews().Concat(new[] { cubeMap }).ToArray();
			pp.SetShaderResourceViews(ppTextures);

			shaderBufferRT = Material.CreateBuffer<Vector4>();
			AutoDispose(shaderBufferRT);
			shaderBufferPP = Material.CreateBuffer<Vector4>();
			AutoDispose(shaderBufferPP);
		}

		void GenerateGaussianTexture()
		{
			var stream = new DataStream(GAUSSIAN_TEXSIZE * GAUSSIAN_TEXSIZE * Utilities.SizeOf<float>(), false, true);

			for (int v = 0; v < GAUSSIAN_TEXSIZE; ++v)
			{
				for (int u = 0; u < GAUSSIAN_TEXSIZE; ++u)
				{
					float dx = 2.0f * u / GAUSSIAN_TEXSIZE - 1.0f;
					float dy = 2.0f * v / GAUSSIAN_TEXSIZE - 1.0f;
					float r2 = (dx * dx + dy * dy);
					float I = (float)(GAUSSIAN_HEIGHT * Math.Exp(-r2 / GAUSSIAN_DEVIATION));
					//if (dx * dx + dy * dy > 1) I = 0;

					stream.Write(I);
				}
			}

			Texture2DDescription desc = new Texture2DDescription();
			desc.BindFlags = BindFlags.ShaderResource;
			desc.CpuAccessFlags = CpuAccessFlags.None;
			desc.Usage = ResourceUsage.Default;
			desc.OptionFlags = ResourceOptionFlags.None;
			desc.ArraySize = 1;
			desc.Format = Format.R32_Float;
			desc.MipLevels = 1;
			desc.Width = GAUSSIAN_TEXSIZE;
			desc.Height = GAUSSIAN_TEXSIZE;
			desc.SampleDescription.Count = 1;
			desc.SampleDescription.Quality = 0;
			var tex = new Texture2D(Device, desc, new[] { new DataBox(stream.DataPointer, GAUSSIAN_TEXSIZE * Utilities.SizeOf<float>(), 0) });
			AutoDispose(tex);
			blobGaussian = new ShaderResourceView(Device, tex);
			AutoDispose(blobGaussian);
			blobGaussian.DebugName = "Gaussian";
			AddResource(blobGaussian);
		}

		float GaussianRadius(float indensity)
		{
			return (float)-Math.Log(indensity / GAUSSIAN_HEIGHT) * GAUSSIAN_DEVIATION;
		}

		protected override void OnUpdate()
		{
			for (int i = 0; i < COLORS.Length; ++i)
			{
				motions[i].Update((float)DeltaTime * AnimationSpeed);
			}

			Vector4 shaderData = new Vector4();
			shaderData[0] = Threshold; // Threshold
			shaderData[1] = (float)(1.0f / Math.Sqrt( GaussianRadius(Threshold))); // UVScale
			shaderData[2] = RimPower;
			shaderData[3] = RimIndensity;
			DeviceContext.UpdateSubresource(ref shaderData, shaderBufferRT);
			DeviceContext.PixelShader.SetConstantBuffer(0, shaderBufferRT);

			rt.Begin(true);
			var origin = new Vector3(0, Origin, 0);
			for (int i = 0; i < motions.Length; ++i)
			{
				var m = motions[i];
				var pos = origin + m.Position * Distance;
				var mat = Matrix.BillboardLH(pos, Camera.ActiveCamera.Position, Vector3.Up, Camera.ActiveCamera.FocusPoint - Camera.ActiveCamera.Position);
				UpdatePerObjectBuffer(Matrix.Scaling(BlobSize) * mat);

				material.Passes[0].UpdateConstantBuffers();
				material.Passes[0].Apply();
				mesh.DrawBegin();
				mesh.DrawSubmesh(0);
			}
			rt.End();

			
			pp.Draw();
		}
	}
}
