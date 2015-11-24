using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Lilium
{
	class MeshPreview : IDisposable
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

		Mesh mesh;

		MaterialPass passFill;
		MaterialPass passWireframe;
		Buffer shaderBuffer;

		internal TangentSpaceBasisRenderer tangentRenderer;

		public MeshPreview(Mesh mesh)
		{
			this.mesh = mesh;

			var game = Game.Instance;

			{
				var desc = new MaterialPassDesc();
				desc.ManualConstantBuffers = true;
				desc.ShaderFile = "MeshPreview.hlsl";

				passFill = new MaterialPass(game.Device, desc, "Preview(Fill)");
			}
			{
				var desc = new MaterialPassDesc();
				desc.ManualConstantBuffers = true;
				desc.ShaderFile = "MeshPreview.hlsl";
				desc.RasteriazerStates.FillMode = FillMode.Wireframe;

				passWireframe = new MaterialPass(game.Device, desc, "Preview(Wireframe)");
			}
			shaderBuffer = Material.CreateBuffer<ShaderData>();
			shaderBuffer.DebugName = "Preview";

			tangentRenderer = new TangentSpaceBasisRenderer(mesh.vertices);
		}

		public void Draw()
		{
			var game = Game.Instance;
			var dc = game.DeviceContext;

			dc.ClearRenderTargetView(game.DefaultRenderTargetView, Color.Maroon);
			dc.ClearDepthStencilView(game.DefaultDepthStencilView, DepthStencilClearFlags.Depth, 1, 0);

			if(Config.DrawTBN) tangentRenderer.Draw();

			var pass = Config.DrawWireframe ? passWireframe : passFill;

			pass.Apply();

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

			pass.Clear();
		}

		public void Dispose()
		{
			Utilities.Dispose(ref tangentRenderer);
			Utilities.Dispose(ref passFill);
			Utilities.Dispose(ref passWireframe);
			Utilities.Dispose(ref shaderBuffer);
		}
	}

	public class TangentSpaceBasisRenderer : IDisposable
	{
		struct LineVertex
		{
			public Vector3 Position;
			public Vector4 Color;

			public LineVertex(Vector3 pos, Color color)
			{
				Position = pos;
				Color = color.ToVector4();
			}
		}

		private Buffer vertexBuffer;
		private Buffer matrixBuffer;
		private MaterialPass pass;

		private bool hasTangent;

		private LineVertex[] lineVertices;
		private Action updateLineVertices;

		public TangentSpaceBasisRenderer(List<MeshVertex> vertices)
		{
			hasTangent = vertices[0].Tangent.LengthSquared() > 0;
			{
				var lineVerticesList = new List<LineVertex>();
				Action<Color> _addLine = (c) =>
				{
					lineVerticesList.Add(new LineVertex(Vector3.Zero, c));
					lineVerticesList.Add(new LineVertex(Vector3.Zero, c));
				};
				for (int i = 0; i < vertices.Count; ++i)
				{
					var v = vertices[i];
					if (hasTangent)
					{
						_addLine(Color.Red);
						_addLine(Color.Green);
					}
					_addLine(Color.Blue);
				}
				lineVertices = lineVerticesList.ToArray();

				updateLineVertices = delegate
				{
					int i = 0;
					int j = 0;
					for (i = 0; i < vertices.Count; ++i)
					{
						var v = vertices[i];
						var origin = v.Position + Config.TBNOffset * v.Normal;

						if (hasTangent)
						{
							lineVertices[j++].Position = origin;
							lineVertices[j++].Position = origin + v.Tangent;
							lineVertices[j++].Position = origin;
							lineVertices[j++].Position = origin + Vector3.Cross(v.Normal, v.Tangent);
						}
						lineVertices[j++].Position = origin;
						lineVertices[j++].Position = origin + v.Normal;
					}
				};
			}
			{
				var desc = new BufferDescription();
				desc.BindFlags = BindFlags.VertexBuffer;
				desc.Usage = ResourceUsage.Dynamic;
				desc.CpuAccessFlags = CpuAccessFlags.Write;
				desc.OptionFlags = ResourceOptionFlags.None;
				desc.SizeInBytes = Utilities.SizeOf<LineVertex>() * lineVertices.Length;
				desc.StructureByteStride = 0;
				vertexBuffer = new Buffer(Game.Instance.Device, desc);
			}
			{
				var passDesc = new MaterialPassDesc();
				passDesc.ManualConstantBuffers = true;
				passDesc.ShaderFile = InternalResources.SHADER_DEBUG_LINE;
				passDesc.InputElements = new InputElement[]{
					new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0),
					new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 12, 0),
				};
				pass = new MaterialPass(Game.Instance.Device, passDesc, "TangentSpaceBasisRenderer");
				matrixBuffer = Material.CreateBuffer<Matrix>();
			}
		}

		public void Draw()
		{
			var dc = Game.Instance.DeviceContext;

			pass.Apply();

			var matViewProj = Camera.MainCamera.ViewMatrix * Camera.MainCamera.ProjectionMatrix;
			dc.UpdateSubresource(ref matViewProj, matrixBuffer);
			dc.VertexShader.SetConstantBuffer(0, matrixBuffer);

			updateLineVertices();
			var box = dc.MapSubresource(vertexBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
			Utilities.Write(box.DataPointer, lineVertices, 0, lineVertices.Length);
			dc.UnmapSubresource(vertexBuffer, 0);

			dc.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
			var bd = new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<LineVertex>(), 0);
			dc.InputAssembler.SetVertexBuffers(0, bd);

			dc.Draw(lineVertices.Length, 0);
		}

		public void Dispose()
		{
			vertexBuffer.Dispose();
			matrixBuffer.Dispose();
			pass.Dispose();
		}
	}
}
