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
	public class LineRenderer : IDisposable
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

		public MaterialPassDesc Desc { get; private set; }

		private Buffer vertexBuffer;
		private Buffer matrixBuffer;
		private MaterialPass pass;
		private VertexBufferBinding vertexBufferBinding;

		private int capacity;
		private int vertexCount = 0;

		private LineVertex[] vertices;
		private Game game;
		private string debugName;

		private bool isDirty;
		private bool is3DInvalid;

		public LineRenderer(Game game, int capacity = 128, string debugName = null)
		{
			this.game = game;
			this.capacity = capacity;
			this.debugName = debugName;

			vertices = new LineVertex[capacity * 2];
			isDirty = true;
			is3DInvalid = true;

			Desc = new MaterialPassDesc();
			Desc.ManualConstantBuffers = true;
			Desc.ShaderFile = InternalResources.SHADER_DEBUG_LINE;
			Desc.InputElements = new InputElement[]{
					new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0),
					new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 12, 0),
				};
			Desc.RasteriazerStates.CullMode = CullMode.None;
		}

		public void Create3D()
		{
			Dispose3D();
			{
				var desc = new BufferDescription();
				desc.BindFlags = BindFlags.VertexBuffer;
				desc.Usage = ResourceUsage.Dynamic;
				desc.CpuAccessFlags = CpuAccessFlags.Write;
				desc.OptionFlags = ResourceOptionFlags.None;
				desc.SizeInBytes = Utilities.SizeOf<LineVertex>() * vertices.Length;
				desc.StructureByteStride = 0;
				vertexBuffer = new Buffer(game.Device, desc);
				vertexBuffer.DebugName = debugName;
				vertexBufferBinding = new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<LineVertex>(), 0);
			}
			{
				pass = new MaterialPass(game.Device, Desc, debugName);
				matrixBuffer = Material.CreateBuffer<Matrix>();
				matrixBuffer.DebugName = debugName;
			}
		}

		public void Dispose3D()
		{
			Utilities.Dispose(ref pass);
			Utilities.Dispose(ref matrixBuffer);
			Utilities.Dispose(ref vertexBuffer);
		}

		public void Add(Vector3 from, Vector3 to, Color color)
		{
			isDirty = true;
			if (vertexCount + 2 >= vertices.Length)
			{
				int c = 16;
				while (c <= capacity)
					c <<= 1;
				capacity = c;
				vertices = new LineVertex[capacity * 2];
			}
			vertices[vertexCount + 0] = new LineVertex(from, color);
			vertices[vertexCount + 1] = new LineVertex(to, color);
			vertexCount += 2;
		}

		public void Clear()
		{
			vertexCount = 0;
		}

		public void Draw()
		{
			if (vertexCount == 0) return;
			var dc = game.DeviceContext;

			if(is3DInvalid)
			{
				Create3D();
				is3DInvalid = false;
			}
			if(isDirty)
			{

				var box = dc.MapSubresource(vertexBuffer, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None);
				Utilities.Write(box.DataPointer, vertices, 0, vertexCount);
				dc.UnmapSubresource(vertexBuffer, 0);
			}

			pass.Apply();

			var matViewProj = Camera.MainCamera.ViewMatrix * Camera.MainCamera.ProjectionMatrix;
			dc.UpdateSubresource(ref matViewProj, matrixBuffer);
			dc.VertexShader.SetConstantBuffer(0, matrixBuffer);

			dc.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
			dc.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);

			dc.Draw(vertexCount, 0);

			pass.Clear();
		}

		public void Dispose()
		{
			Dispose3D();
		}
	}
}
