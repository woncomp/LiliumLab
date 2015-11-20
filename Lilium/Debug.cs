using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Lilium
{
	public static class Debug
	{
		private static int nextObjectId = 0;
		public static int NextObjectId { get { return ++nextObjectId; } }

		public static void Log(string str)
		{
			System.Diagnostics.Debug.WriteLine(str);
		}

		public static void Line(Vector3 from, Vector3 to, Color color)
		{
			DebugLines.Instance.Add(from, to, color);
		}
	}

	public class DebugLines : IDisposable
	{
		public static DebugLines Instance;

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

		const int MAX_LINE_COUNT = 512;

		private Buffer vertexBuffer;
		private Buffer matrixBuffer;
		private Material material;

		private LineVertex[] vertices = new LineVertex[MAX_LINE_COUNT * 2];
		private int vertexCount = 0;

		public Device Device { get; private set; }

		public DebugLines(Device device)
		{
			this.Device = device;
		}

		public void Init()
		{
			{
				var desc = new BufferDescription();
				desc.BindFlags = BindFlags.VertexBuffer;
				desc.Usage = ResourceUsage.Dynamic;
				desc.CpuAccessFlags = CpuAccessFlags.Write;
				desc.OptionFlags = ResourceOptionFlags.None;
				desc.SizeInBytes = Utilities.SizeOf<LineVertex>() * vertices.Length;
				desc.StructureByteStride = 0;
				vertexBuffer = new Buffer(Device, desc);
			}
			{
				var materialDesc = new MaterialDesc();
				materialDesc.ShaderFile = InternalResources.SHADER_DEBUG_LINE;
				materialDesc.InputElements = new InputElement[]{
					new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0),
					new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 12, 0),
				};
				materialDesc.RasteriazerStates.CullMode = CullMode.None;
				materialDesc.DepthStencilStates.IsDepthEnabled = false;

				material = new Material(materialDesc);
				matrixBuffer = Material.CreateBuffer<Matrix>();
			}
		}

		public void Add(Vector3 from, Vector3 to, Color color)
		{
			vertices[vertexCount + 0] = new LineVertex(from, color);
			vertices[vertexCount + 1] = new LineVertex(to, color);
			vertexCount += 2;
		}

		public void Update(bool suppressDraw)
		{
			if (!suppressDraw)
			{
				var dc = Device.ImmediateContext;
				
				material.Apply();

				var matViewProj = Camera.MainCamera.ViewMatrix * Camera.MainCamera.ProjectionMatrix;
				dc.UpdateSubresource(ref matViewProj, matrixBuffer);
				dc.VertexShader.SetConstantBuffer(0, matrixBuffer);

				var box = dc.MapSubresource(vertexBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
				Utilities.Write(box.DataPointer, vertices, 0, vertexCount);
				dc.UnmapSubresource(vertexBuffer, 0);

				dc.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
				var bd = new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<LineVertex>(), 0);
				dc.InputAssembler.SetVertexBuffers(0, bd);

				dc.Draw(vertexCount, 0);

				material.Clear();
			}

			vertexCount = 0;
		}

		public void Dispose()
		{
			material.Dispose();
			matrixBuffer.Dispose();
			vertexBuffer.Dispose();
		}
	}
}
