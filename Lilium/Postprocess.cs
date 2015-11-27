using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Lilium
{
	public class Postprocess : IDisposable
	{
		const int VERTEX_FLOAT_COUNT = 5;
		const int VERTEX_COUNT = 6;

		readonly SamplerStateDescription defaultSamplerStateDescription;

		public string Name { get; private set; }

		Game game;

		RenderTexture renderTexture;

		float[] vertices = new float[VERTEX_FLOAT_COUNT * VERTEX_COUNT];
		MaterialPass pass;
		Buffer vertexBuffer;
		VertexBufferBinding vertexBufferBinding;

		SamplerState[] samplerStates = new SamplerState[0];
		ShaderResourceView[] shaderResourceViews = new ShaderResourceView[0];
		ShaderResourceView[] shaderResourceViewsClear;

		public Postprocess(Game game, string shaderFile, RenderTexture rt = null, string name = null)
		{
			this.game = game;
			this.renderTexture = rt;
			this.Name = name ?? "Postprocess " + Debug.NextObjectId;

			var desc = SamplerStateDescription.Default();
			desc.Filter = Filter.MinMagMipPoint;
			defaultSamplerStateDescription = desc;

			var passDesc = new MaterialPassDesc();
			passDesc.ManualConstantBuffers = true;
			passDesc.ShaderFile = shaderFile;
			passDesc.InputElements = new InputElement[]{
				new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0),
				new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 12, 0),
			};
			pass = new MaterialPass(Game.Instance.Device, passDesc, Name);

			BuildVertexBuffer();
		}

		public void SetShaderResourceViews(ShaderResourceView[] shaderResourceViews)
		{
			if(shaderResourceViews == null) return;
			this.shaderResourceViews = shaderResourceViews;
			this.shaderResourceViewsClear = shaderResourceViews.Select<ShaderResourceView, ShaderResourceView>( x => null).ToArray();
			if(samplerStates.Length < shaderResourceViews.Length)
			{
				Array.Resize(ref samplerStates, shaderResourceViews.Length);
				for (int i = 0; i < samplerStates.Length; ++i)
				{
					if (samplerStates[i] == null) samplerStates[i] = new SamplerState(game.Device, defaultSamplerStateDescription);
				}
			}
		}

		public void Draw()
		{
			var dc = game.DeviceContext;

			if (renderTexture != null)
				renderTexture.Begin();
			else
				dc.ClearDepthStencilView(game.DefaultDepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);

			pass.Apply();

			dc.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
			dc.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);

			if (shaderResourceViews.Length > 0)
			{
				dc.PixelShader.SetSamplers(0, samplerStates);
				dc.PixelShader.SetShaderResources(0, shaderResourceViews);
			}
			dc.Draw(6, 0);
			if (shaderResourceViews.Length > 0)
			{
				dc.PixelShader.SetShaderResources(0, shaderResourceViewsClear);
			}

			if(renderTexture != null)
				renderTexture.End();
		}

		void BuildVertexBuffer()
		{
			var game = Game.Instance;

			float w = 1f;
			float h = 1f;

			int start = 0;
			DefineVertex(ref start, w, -h, 1, 1);
			DefineVertex(ref start, -w, -h, 0, 1);
			DefineVertex(ref start, w, h, 1, 0);

			DefineVertex(ref start, w, h, 1, 0);
			DefineVertex(ref start, -w, -h, 0, 1);
			DefineVertex(ref start, -w, h, 0, 0);

			var desc = new BufferDescription();
			desc.BindFlags = BindFlags.VertexBuffer;
			desc.Usage = ResourceUsage.Default;
			desc.CpuAccessFlags = CpuAccessFlags.None;
			desc.SizeInBytes = Utilities.SizeOf<float>() * vertices.Length;
			desc.StructureByteStride = 0;
			desc.OptionFlags = ResourceOptionFlags.None;

			vertexBuffer = Buffer.Create(game.Device, vertices, desc);
			vertexBuffer.DebugName = Name;

			vertexBufferBinding = new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<float>() * VERTEX_FLOAT_COUNT, 0);
		}

		private void DefineVertex(ref int start, float x, float y, float u, float v)
		{
			vertices[start + 0] = x;
			vertices[start + 1] = y;
			vertices[start + 2] = 0;
			vertices[start + 3] = u;
			vertices[start + 4] = v;
			start += VERTEX_FLOAT_COUNT;
		}

		public void Dispose()
		{
			for (int i = 0; i < samplerStates.Length; ++i)
			{
				Utilities.Dispose(ref samplerStates[i]);
			}
			Utilities.Dispose(ref pass);
			Utilities.Dispose(ref vertexBuffer);
			//Utilities.Dispose(ref shaderResourceView);
			//shaderResourceView = null;
		}
	}
}
