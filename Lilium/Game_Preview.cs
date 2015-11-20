using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Lilium
{
	public class PreviewAttribute : System.Attribute
	{
		public string OverrideName = null;
		public PreviewAttribute(string overrideName = null)
		{
			OverrideName = overrideName;
		}
	}

	public partial class Game
	{
		bool previewSuppressDebugLines { get { return activePreview.SuppressDebugLines; } }

		List<Preview> previewList = new List<Preview>();
		Preview activePreview;

		void Preview_Init()
		{
			previewList.Add(new NoPreview());

			var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
			foreach (var field in GetType().GetFields(flags))
			{
				var attr = field.GetCustomAttribute(typeof(PreviewAttribute), false) as PreviewAttribute;
				if (attr != null)
				{
					Texture2D tex = null;
					Mesh mesh = null;
					if (TexturePreview.GetTexture2D(field, this, ref tex))
					{
						previewList.Add(new TexturePreview(field, attr));
					}
					else if(MeshPreview.GetMesh(field, ref mesh))
					{
						previewList.Add(new MeshPreview(field, attr));
					}
					else
					{
						throw new InvalidOperationException();
					}
				}
			}

			var control = new Lilium.Controls.ComboBox("Preview",
				previewList.Select(info => info.Name).ToArray(),
				null, Preview_IndexChanged);
			control.SelectedIndex = 0;
			AddControl(control);
		}

		void Preview_Render()
		{
			if(activePreview != null)
			{
				activePreview.Draw();
			}
		}

		void Preview_Dispose()
		{
			previewList.ForEach(info => info.Dispose());
		}

		void Preview_IndexChanged(int index)
		{
			if (activePreview != null) activePreview.Deactive();
			activePreview = previewList[index];
			if (activePreview != null) activePreview.Active();
		}
	}
	
	abstract class Preview : IDisposable
	{
		public string Name;
		public bool SuppressDebugLines = false;

		public abstract void Draw();
		public abstract void Active();
		public abstract void Deactive();
		public void Dispose()
		{
			Deactive();
		}
	}

	class NoPreview : Preview
	{
		public NoPreview()
		{
			Name = "No Preview";
		}

		public override void Draw() { }
		public override void Active() { }
		public override void Deactive() { }
	}

	class TexturePreview : Preview
	{
		public static bool GetTexture2D(FieldInfo field, Game game, ref Texture2D tex)
		{
			var type = field.FieldType;
			if (type == typeof(ShaderResourceView))
			{
				var shaderResourceView = field.GetValue(game) as ShaderResourceView;
				if (shaderResourceView != null)
				{
					var res = shaderResourceView.Resource;
					tex = res.QueryInterface<Texture2D>();
					(res as IUnknown).Release();
					(tex as IUnknown).Release();
				}
				return true;
			}
			else if (type == typeof(Texture2D))
			{
				tex = field.GetValue(game) as Texture2D;
				return true;
			}
			else return false;
		}

		const int VERTEX_FLOAT_COUNT = 5;
		const int VERTEX_COUNT = 6;

		System.Reflection.FieldInfo field;

		ShaderResourceView shaderResourceView;

		float[] vertices = new float[VERTEX_FLOAT_COUNT * VERTEX_COUNT];
		Material material;
		Buffer vertexBuffer;
		VertexBufferBinding vertexBufferBinding;

		public TexturePreview(FieldInfo field, PreviewAttribute attr)
		{
			this.field = field;
			this.Name = "Texture: " + (attr.OverrideName ?? field.Name);
			this.SuppressDebugLines = true;
		}

		public override void Draw()
		{
			if (shaderResourceView == null) return;

			var game = Game.Instance;
			var dc = game.DeviceContext;

			dc.ClearRenderTargetView(game.DefaultRenderTargetView, Color.Maroon);
			dc.ClearDepthStencilView(game.DefaultDepthStencilView, DepthStencilClearFlags.Depth, 1, 0);

			material.Apply();

			dc.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
			dc.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);

			dc.PixelShader.SetShaderResource(0, shaderResourceView);
			dc.Draw(6, 0);
			dc.PixelShader.SetShaderResource(0, null);
		}

		public override void Active()
		{
			var game = Game.Instance;
			Texture2D tex = null;
			GetTexture2D(field, game, ref tex);

			CreateTextureShaderResourceView(tex);
			if (shaderResourceView == null) return;

			MaterialDesc materialDesc = new MaterialDesc();
			materialDesc.ShaderFile = "../../../Models/sys/TexturePreview.hlsl";
			materialDesc.DebugName = "Preview";
			materialDesc.InputElements = new InputElement[]{
				new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0),
				new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 12, 0),
			};
			material = new Material(materialDesc);

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

		public override void Deactive()
		{
			Utilities.Dispose(ref material);
			Utilities.Dispose(ref vertexBuffer);
			Utilities.Dispose(ref shaderResourceView);
			shaderResourceView = null;
		}
	}

	class MeshPreview : Preview
	{
		struct ShaderData
		{
			public Matrix matWorld;
			public Matrix matView;
			public Matrix matProjection;
			public Matrix matWorldViewProj;
			public Vector4 lightDir;
			public Vector4 eyePos;
		}

		public static bool GetMesh(FieldInfo field, ref Mesh mesh)
		{
			if (field.FieldType == typeof(Mesh))
			{
				mesh = field.GetValue(Game.Instance) as Mesh;
				return true;
			}
			return false;
		}

		System.Reflection.FieldInfo field;

		Mesh mesh;

		Material materialFill;
		Material materialWireframe;
		Buffer shaderBuffer;

		public MeshPreview(FieldInfo field, PreviewAttribute attr)
		{
			this.field = field;
			this.Name = "Mesh: " + (attr.OverrideName ?? field.Name);
		}

		public override void Draw()
		{
			if (mesh == null) return;

			var game = Game.Instance;
			var dc = game.DeviceContext;

			dc.ClearRenderTargetView(game.DefaultRenderTargetView, Color.Maroon);
			dc.ClearDepthStencilView(game.DefaultDepthStencilView, DepthStencilClearFlags.Depth, 1, 0);

			mesh.DrawTangentSpaceBasis();
			
			var material = Config.DrawWireframe ? materialWireframe : materialFill;

			material.Apply();

			ShaderData data = new ShaderData();
			data.matWorld = Matrix.Identity;
			data.matView = Camera.MainCamera.ViewMatrix;
			data.matProjection = Camera.MainCamera.ProjectionMatrix;
			data.matWorldViewProj = data.matWorld * data.matView * data.matProjection;
			data.lightDir = Light.MainLight.LightDir4;
			data.eyePos = new Vector4(Camera.MainCamera.Position, 1);

			dc.UpdateSubresource(ref data, shaderBuffer);

			dc.VertexShader.SetConstantBuffer(0, shaderBuffer);
			dc.PixelShader.SetConstantBuffer(0, shaderBuffer);

			mesh.DrawBegin();
			for (int i = 0; i < mesh.SubmeshCount; ++i)
			{
				mesh.DrawSubmesh(i);
			}

			material.Clear();
		}

		public override void Active()
		{
			var game = Game.Instance;

			GetMesh(field, ref mesh);

			{
				var desc = new MaterialDesc();
				desc.DebugName = "Preview(Fill)";
				desc.ShaderFile = "../../../Models/sys/MeshPreview.hlsl";
				
				materialFill = new Material(desc);
			}
			{
				var desc = new MaterialDesc();
				desc.DebugName = "Preview(Wireframe)";
				desc.ShaderFile = "../../../Models/sys/MeshPreview.hlsl";
				desc.RasteriazerStates.FillMode = FillMode.Wireframe;

				materialWireframe = new Material(desc);
			}
			shaderBuffer = Material.CreateBuffer<ShaderData>();
			shaderBuffer.DebugName = "Preview";
		}

		public override void Deactive()
		{
			Utilities.Dispose(ref materialFill);
			Utilities.Dispose(ref materialWireframe);
			Utilities.Dispose(ref shaderBuffer);
		}
	}
}
