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
	public class Postprocess : IDisposable, ISelectable
	{
		struct LiliumPostprocessData
		{
			public float renderTargetWidth;
			public float renderTargetHeight;
			public Vector2 ___;
		}

		const int VERTEX_FLOAT_COUNT = 5;
		const int VERTEX_COUNT = 6;

		readonly SamplerStateDescription defaultSamplerStateDescription;

		public string Name { get; private set; }

		Game game;

		RenderTexture renderTexture;

		private string debugName;
		private string shaderFile;

		float[] vertices = new float[VERTEX_FLOAT_COUNT * VERTEX_COUNT];
		MaterialPass pass;
		Buffer vertexBuffer;
		VertexBufferBinding vertexBufferBinding;

		Buffer ppBuffer;
		LiliumPostprocessData ppData;

		SamplerState[] samplerStates = new SamplerState[0];
		ShaderResourceView[] shaderResourceViews = new ShaderResourceView[0];
		ShaderResourceView[] shaderResourceViewsClear;

		public Postprocess(Game game, string shaderFile, RenderTexture rt = null, string name = null)
		{
			this.game = game;
			this.renderTexture = rt;
			if(name == null)
			{
				this.Name = System.IO.Path.GetFileNameWithoutExtension(shaderFile);
				this.debugName = "Postprocess " + this.Name;
			}
			else
			{
				this.debugName = this.Name = name;
			}
			this.shaderFile = shaderFile;

			var desc = SamplerStateDescription.Default();
			desc.Filter = Filter.MinMagMipPoint;
			defaultSamplerStateDescription = desc;

			LoadShader();
			BuildVertexBuffer();
			ppBuffer = Material.CreateBuffer<LiliumPostprocessData>();

			game.AddObject(this);
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

		public void SetSamplerState(int index, SamplerStateDescription desc)
		{
			if (index >= 0 && index < samplerStates.Length)
			{
				Utilities.Dispose(ref samplerStates[index]);
				samplerStates[index] = new SamplerState(game.Device, desc);
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
			if(renderTexture != null)
			{
				ppData.renderTargetWidth = renderTexture.Viewport.Width;
				ppData.renderTargetHeight = renderTexture.Viewport.Height;
			}
			else
			{
				ppData.renderTargetWidth = game.DefaultViewport.Width;
				ppData.renderTargetHeight = game.DefaultViewport.Height;
			}
			ppData.___ = Vector2.Zero;
			dc.UpdateSubresource(ref ppData, ppBuffer);
			dc.PixelShader.SetConstantBuffer(7, ppBuffer);

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

		void LoadShader()
		{
			var passDesc = new MaterialPassDesc();
			passDesc.ManualConstantBuffers = true;
			passDesc.ShaderFile = shaderFile;
			passDesc.InputElements = new InputElement[]{
				new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0),
				new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 12, 0),
			};
			pass = new MaterialPass(Game.Instance.Device, passDesc, debugName);
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
			vertexBuffer.DebugName = debugName;

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
			Utilities.Dispose(ref ppBuffer);
		}

		#region Selectable
		public Controls.Control[] Controls
		{
			get
			{
				var list = new List<Lilium.Controls.Control>();
				var btnReload = new Lilium.Controls.Button("Reload", () =>
				{
					game.SelectedObject = null;
					pass.Dispose();
					LoadShader();
					game.SelectedObject = this;
				});
				list.Add(btnReload);
				if (! pass.IsValid)
				{
					var textArea = new Lilium.Controls.TextArea();
					textArea.Text = pass.ErrorMessage;
					list.Add(textArea);
				}
				if(renderTexture != null)
				{
					var btn = new Lilium.Controls.Button("Select RenderTexture", () =>
					{
						game.SelectedObject = renderTexture;
					});
					list.Add(btn);
				}
				return list.ToArray();
			}
		}

		public string NameInObjectList
		{
			get { return Name; }
		}
		#endregion
	}
}
