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
	[CustomSelectedTypeName("Texture2D")]
	class TexturePreview : IPreviewable, ISelectable
	{
		const int VERTEX_FLOAT_COUNT = 5;
		const int VERTEX_COUNT = 6;

		public string Name;

		Texture2D tex;
		
		ShaderResourceView shaderResourceView;

		float[] vertices = new float[VERTEX_FLOAT_COUNT * VERTEX_COUNT];
		MaterialPass pass;
		Buffer vertexBuffer;
		VertexBufferBinding vertexBufferBinding;

		public TexturePreview(ShaderResourceView shaderResourceView)
		{
			if (shaderResourceView != null)
			{
				var res = shaderResourceView.Resource;
				this.tex = res.QueryInterface<Texture2D>();
				(res as IUnknown).Release();
				(tex as IUnknown).Release();
			}
			Name = shaderResourceView.DebugName;
		}

		public TexturePreview(Texture2D tex)
		{
			this.tex = tex;
			Name = tex.DebugName;
		}
		
		public void PreviewDraw()
		{
			if (shaderResourceView == null) return;

			var game = Game.Instance;
			var dc = game.DeviceContext;

			dc.ClearRenderTargetView(game.DefaultRenderTargetView, Color.Maroon);
			dc.ClearDepthStencilView(game.DefaultDepthStencilView, DepthStencilClearFlags.Depth, 1, 0);

			pass.Apply();

			dc.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
			dc.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);

			dc.PixelShader.SetShaderResource(0, shaderResourceView);
			dc.Draw(6, 0);
			dc.PixelShader.SetShaderResource(0, null);
		}
		
		public void PreviewActive()
		{
			var game = Game.Instance;

			CreateTextureShaderResourceView(tex);
			if (shaderResourceView == null) return;

			var passDesc = new MaterialPassDesc();
			passDesc.ManualConstantBuffers = true;
			passDesc.ShaderFile = "TexturePreview.hlsl";
			passDesc.InputElements = new InputElement[]{
				new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0),
				new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 12, 0),
			};
			pass = new MaterialPass(Game.Instance.Device, passDesc, "Preview");

			BuildVertexBuffer(tex.Description);
		}

		void CreateTextureShaderResourceView(Texture2D tex)
		{
			var desc = new ShaderResourceViewDescription();
			desc.Format = tex.Description.Format;
			desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D;
			desc.Texture2D.MipLevels = 1;
			desc.Texture2D.MostDetailedMip = 0;
			shaderResourceView = new ShaderResourceView(Game.Instance.Device, tex, desc);
			shaderResourceView.DebugName = "Preview";
		}

		void BuildVertexBuffer(Texture2DDescription textureDesc)
		{
			var game = Game.Instance;
			var clientSize = game.RenderViewSize;
			var texWidth = textureDesc.Width;
			var texHeight = textureDesc.Height;

			var clientRatio = clientSize.Width / (float)clientSize.Height;
			var textureRatio = texWidth / (float)texHeight;

			float w = 1f;
			float h = 1f;

			if (clientRatio > textureRatio)
			{
				if (texHeight > clientSize.Height)
				{
					w = h * textureRatio / clientRatio;
				}
				else
				{
					h = texHeight / (float)clientSize.Height;
					w = texWidth / (float)clientSize.Width;
				}
			}
			else
			{
				if (texWidth > clientSize.Width)
				{
					h = w * clientRatio / textureRatio;
				}
				else
				{
					h = texHeight / (float)clientSize.Height;
					w = texWidth / (float)clientSize.Width;
				}
			}

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
			vertexBuffer.DebugName = "Preview(Texture2D)";

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
		
		public void PreviewDeactive()
		{
			Utilities.Dispose(ref pass);
			Utilities.Dispose(ref vertexBuffer);
			Utilities.Dispose(ref shaderResourceView);
			shaderResourceView = null;
		}

		public Controls.Control[] Controls
		{
			get { return new Controls.Control[0]; }
		}

		public string NameInObjectList
		{
			get { return Name; }
		}
	}
}
