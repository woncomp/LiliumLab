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
	public class ScreenSpaceMetaballGame : Game
	{
		class Motion
		{
			public Vector3 Position;
			public Vector4 Color;

			public Vector3 Offset;
			public Vector3 Speed;
			public Vector3 Scale;

			public float time;

			public Motion(Random r)
			{
				Offset = r.NextVector3(Vector3.Zero, Vector3.One) * 2 * MathUtil.TwoPi;
				Speed = r.NextVector3(Vector3.Zero, Vector3.One) + Vector3.One * 0.5f;
				Scale = r.NextVector3(Vector3.Zero, Vector3.One) + Vector3.One * 0.5f;
			}

			public void Update(float deltaTime)
			{
				time += deltaTime;

				var t2 = Offset + Speed * time;
				Position.X = (float)(Math.Cos(t2.X) * Scale.X);
				Position.Y = (float)(Math.Cos(t2.Y) * Scale.Y);
				Position.Z = (float)(Math.Cos(t2.Z) * Scale.Z);
			}
		};

		struct ScreenVertex
		{
			public Vector3 Position;
			public Vector2 Curr;
			public Vector2 Back;
			public Vector3 Color;
			public float Size;
		}
		
		const int GAUSSIAN_TEXSIZE   = 64;
		const int GAUSSIAN_HEIGHT    = 1;
		const float GAUSSIAN_DEVIATION = 0.125f;
		static readonly Color[] COLORS = new [] { Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Orange, Color.Magenta };

		private Motion[] motions;

		private ShaderResourceView cubeMap;

		private ScreenVertex[] blobVertices;
		private Buffer blobVertexBuffer;
		private MaterialPass blobPass;
		private RenderTexture blobRT;
		private RenderTexture blobCopy;
		private ShaderResourceView blobGaussian;
		private ShaderResourceView blobBlack;
		private Postprocess blobPP;

		private Entity[] sphereEntities;
		private Buffer sphereShaderBuffer;

		[Slider(0, 5)]
		float Origin = 4;

		[Slider(1, 10)]
		float Distance = 4;

		[Slider(1, 10)]
		float BlobSize = 2;

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

			cubeMap = ResourceManager.Tex2D.Load("LobbyCube.dds");

			//CreateSphereEntities();
			CreateBlobs();
		}

		void CreateBlobs()
		{
			GenerateGaussianTexture();

			blobVertices = new ScreenVertex[COLORS.Length * 6];
			blobVertexBuffer = new Buffer(Device, Utilities.SizeOf<ScreenVertex>() * blobVertices.Length, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
			AutoDispose(blobVertexBuffer);

			var blobPassDesc = new MaterialPassDesc();
			blobPassDesc.ManualConstantBuffers = true;
			blobPassDesc.ShaderFile = "Blobs.hlsl";
			blobPassDesc.RasteriazerStates.CullMode = CullMode.None;
			blobPassDesc.DepthStencilStates.IsDepthEnabled = false;
			blobPassDesc.InputElements = new InputElement[]{
				new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
				new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0),
				new InputElement("TEXCOORD", 1, Format.R32G32_Float, 20, 0),
				new InputElement("COLOR", 0, Format.R32G32B32_Float, 28, 0),
				new InputElement("COLOR", 1, Format.R32_Float, 40, 0),
			};
			blobPass = new MaterialPass(Device, blobPassDesc, "BlobPass");
			AutoDispose(blobPass);

			blobRT = new RenderTexture(this, 2, "BlobRT");
			AutoDispose(blobRT);
			blobRT.SetClearColor(0, new Color4(0, 0, 0, 0));
			blobRT.SetClearColor(1, new Color4(0, 0, 0, 0));

			blobCopy = new RenderTexture(this, 2, "BlobCopyRT");
			AutoDispose(blobCopy);
			blobCopy.SetClearColor(0, new Color4(0, 0, 0, 0));
			blobCopy.SetClearColor(1, new Color4(0, 0, 0, 0));

			blobPP = new Postprocess(this, "BlobsPP.hlsl");
			AutoDispose(blobPP);
			blobPP.SetShaderResourceViews(blobRT.GetShaderResourceViews().Concat(new[] { cubeMap }).ToArray());
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
					float I = (float)(GAUSSIAN_HEIGHT * Math.Exp(-(dx*dx+dy*dy)/GAUSSIAN_DEVIATION));
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

			blobBlack = ResourceManager.Tex2D.Load("black.png");
			AutoDispose(blobBlack);
		}

		void CreateSphereEntities()
		{
			sphereEntities = new Entity[COLORS.Length];

			MaterialDesc desc = new MaterialDesc();
			desc.Passes[0].ShaderFile = "ColorPhong.hlsl";

			for (int i = 0; i < COLORS.Length; ++i)
			{
				sphereEntities[i] = new Entity(InternalResources.MESH_SPHERE);
				sphereEntities[i].SetMaterial(0, desc);
				sphereEntities[i].Scale = Vector3.One * 0.2f;
				AddObject(sphereEntities[i]);
			}

			sphereShaderBuffer = Material.CreateBuffer<Vector4>();
			AutoDispose(sphereShaderBuffer);
		}

		protected override void OnUpdate()
		{
			for (int i = 0; i < COLORS.Length; ++i)
			{
				motions[i].Update((float)DeltaTime);
			}

			//DrawSphereEntities();
			DrawBlobs();
		}

		void DrawBlobs()
		{
			FillBlobVB();
			blobRT.Clear();

			blobPass.Apply();
			for (int i = 0; i < COLORS.Length; ++i)
			{
				DeviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
				DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(blobVertexBuffer, Utilities.SizeOf<ScreenVertex>(), 0));

				var tex2 = blobRT.GetShaderResourceViews();
				var texClear = new ShaderResourceView[]{null, null, null};
				blobCopy.Begin(false);
				DeviceContext.PixelShader.SetShaderResources(0, blobBlack, tex2[0], tex2[1]);
				DeviceContext.Draw(6, i * 6);
				DeviceContext.PixelShader.SetShaderResources(0, texClear);
				blobCopy.End();
				//-------------------------------------
				tex2 = blobCopy.GetShaderResourceViews();
				blobRT.Begin(false);
				DeviceContext.PixelShader.SetShaderResources(0, blobGaussian, tex2[0], tex2[1]);
				DeviceContext.Draw(6, i * 6);
				DeviceContext.PixelShader.SetShaderResources(0, texClear);
				blobRT.End();
			}
			blobPass.Clear();

			DeviceContext.OutputMerger.SetBlendState(AlphaBlendState);
			blobPP.Draw();
			DeviceContext.OutputMerger.SetBlendState(DefaultBlendState);
		}

		void FillBlobVB()
		{
			var vTexCoords = new []
			{
				new Vector2(0.0f,0.0f),
				new Vector2(1.0f,0.0f),
				new Vector2(0.0f,1.0f),
				new Vector2(0.0f,1.0f),
				new Vector2(1.0f,0.0f),
				new Vector2(1.0f,1.0f),
			};
			
			var origin = new Vector3(0, Origin, 0);
			for (int i = 0; i < COLORS.Length; ++i)
			{
				var m = motions[i];

				var posVS = Vector3.TransformCoordinate(origin + m.Position * Distance, Camera.ActiveCamera.ViewMatrix);

				// Transform to screenspace
				var matProj = Camera.ActiveCamera.ProjectionMatrix;
				var BlobscreenPos = Vector4.Transform(new Vector4(posVS, 1), matProj);
				var billOfsScreen = Vector4.Transform(new Vector4(BlobSize, BlobSize, posVS.Z, 1), matProj);

				// Project
				BlobscreenPos /= BlobscreenPos.W;
				billOfsScreen /= billOfsScreen.W;

				//
				var vPosOffset = new []{
					new Vector4(-billOfsScreen.X,-billOfsScreen.Y,0.0f,0.0f),
					new Vector4( billOfsScreen.X,-billOfsScreen.Y,0.0f,0.0f),
					new Vector4(-billOfsScreen.X, billOfsScreen.Y,0.0f,0.0f),
					new Vector4(-billOfsScreen.X, billOfsScreen.Y,0.0f,0.0f),
					new Vector4( billOfsScreen.X,-billOfsScreen.Y,0.0f,0.0f),
					new Vector4( billOfsScreen.X, billOfsScreen.Y,0.0f,0.0f),
				};

				float width = RenderViewSize.Width;
				float height = RenderViewSize.Height;
				// 填充
				for (int j = 0; j < 6; ++j)
				{
					var index = i * 6 + j;

					// Scale to pixels
					var pos = BlobscreenPos + vPosOffset[j];

					blobVertices[index].Position = new Vector3(pos.ToArray().Take(3).ToArray());

					var texCoordBack = new Vector2();
					texCoordBack.X = 0.5f + 0.5f * pos.X;
					texCoordBack.Y = 0.5f - 0.5f * pos.Y;

					blobVertices[index].Curr = vTexCoords[j];
					blobVertices[index].Back = texCoordBack;
					blobVertices[index].Size = BlobSize;
					blobVertices[index].Color = COLORS[i].ToVector3();
				}
			}

			DeviceContext.UpdateSubresource(blobVertices, blobVertexBuffer);
		}

		void DrawSphereEntities()
		{
			var origin = new Vector3(0, Origin, 0);
			for (int i = 0; i < COLORS.Length; ++i)
			{
				sphereEntities[i].Position = origin + motions[i].Position * Distance;

				DeviceContext.UpdateSubresource(ref motions[i].Color, sphereShaderBuffer);
				DeviceContext.PixelShader.SetConstantBuffer(0, sphereShaderBuffer);

				sphereEntities[i].Draw();
			}
		}
	}
}
