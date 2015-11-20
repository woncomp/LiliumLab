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
	public class Grid : IDisposable
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

		const int ROW = 21;
		const int COL = 21;
		const int MAX_LINE_COUNT = ROW + COL + 2;
		const float SPACE = 5;

		public Color GridColor = Color.Gray;

		private Buffer vertexBuffer;
		private Buffer matrixBuffer;
		private Material material;

		private LineVertex[] vertices = new LineVertex[MAX_LINE_COUNT * 2];
		private int vertexCount = 0;

		public Device Device { get; private set; }

		public Grid(Device device)
		{
			this.Device = device;
		}

		public void Init()
		{
			{
				BuildVertices();
				var desc = new BufferDescription();
				desc.BindFlags = BindFlags.VertexBuffer;
				desc.Usage = ResourceUsage.Default;
				desc.CpuAccessFlags = CpuAccessFlags.None;
				desc.OptionFlags = ResourceOptionFlags.None;
				desc.SizeInBytes = Utilities.SizeOf<LineVertex>() * vertices.Length;
				desc.StructureByteStride = 0;
				vertexBuffer = Buffer.Create(Device, vertices, desc);//new Buffer(Device, desc);
			}
			{
				var materialDesc = new MaterialDesc();
				materialDesc.ShaderFile = InternalResources.SHADER_DEBUG_LINE;
				materialDesc.InputElements = new InputElement[]{
					new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0),
					new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 12, 0),
				};
				materialDesc.RasteriazerStates.CullMode = CullMode.None;
				//materialDesc.DepthStencilStates.IsDepthEnabled = false;
				materialDesc.DepthStencilStates.DepthWriteMask = DepthWriteMask.Zero;

				material = new Material(materialDesc);
				matrixBuffer = Material.CreateBuffer<Matrix>();
			}
		}

		void BuildVertices()
		{
			float xstart = -SPACE * (COL-1) * 0.5f;
			float xend = -xstart;
			float zstart = -SPACE * (ROW-1) * 0.5f;
			float zend = -zstart;

			for (int i = 0; i < COL; ++i)
			{
				float x= xstart + SPACE*i;
				Add(new Vector3(x, 0, zstart), new Vector3(x, 0, zend), GridColor);
			}
			for (int i = 0; i < ROW; ++i)
			{
				float z = zstart + SPACE * i;
				Add(new Vector3(xstart, 0, z), new Vector3(xend, 0, z), GridColor);
			}
			Add(new Vector3(0, 0, zstart), new Vector3(0, 0, zend), Color.Blue);
			Add(new Vector3(xstart, 0, 0), new Vector3(xend, 0, 0), Color.Red);
		}

		void Add(Vector3 from, Vector3 to, Color color)
		{
			vertices[vertexCount + 0] = new LineVertex(from, color);
			vertices[vertexCount + 1] = new LineVertex(to, color);
			vertexCount += 2;
		}

		public void Draw()
		{
			var dc = Device.ImmediateContext;

			material.Apply();

			var matViewProj = Camera.ActiveCamera.ViewMatrix * Camera.ActiveCamera.ProjectionMatrix;
			dc.UpdateSubresource(ref matViewProj, matrixBuffer);
			dc.VertexShader.SetConstantBuffer(0, matrixBuffer);

			dc.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
			var bd = new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<LineVertex>(), 0);
			dc.InputAssembler.SetVertexBuffers(0, bd);

			dc.Draw(vertexCount, 0);

			material.Clear();
		}

		public void Dispose()
		{
			material.Dispose();
			matrixBuffer.Dispose();
			vertexBuffer.Dispose();
		}
	}
}
