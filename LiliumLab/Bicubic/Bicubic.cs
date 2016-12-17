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
	public class Bicubic : Game
	{
		Material material;
		Buffer vertexBuffer;
		VertexBufferBinding vertexBufferBinding;

		List<MeshVertex> vertexBufferBuilder;

		[Slider(0, 15)]
		float HighlightLine = 0;

		protected override void OnStart()
		{
			ResourceManager.SearchPaths.Add("../../Bicubic");

			material = ResourceManager.Material.Load("Bicubic.lm");
			
			vertexBufferBuilder = new List<MeshVertex>();
			AddVertex(-1,		0, 1,		0, 0);
			AddVertex(-0.34f,	0, 1,		1, 0);
			AddVertex(0.34f,	0, 1,		2, 0);
			AddVertex(1,		0, 1,		3, 0);

			AddVertex(-1,		0, 0.34f,	0, 1);
			AddVertex(-0.34f,	2, 0.34f,	1, 1);
			AddVertex(0.34f,	2, 0.34f,	2, 1);
			AddVertex(1,		0, 0.34f,	3, 1);

			AddVertex(-1,		0, -0.34f,	0, 2);
			AddVertex(-0.34f,	2, -0.34f,	1, 2);
			AddVertex(0.34f,	2, -0.34f,	2, 2);
			AddVertex(1,		0, -0.34f,	3, 2);

			AddVertex(-1,		0, -1,		0, 3);
			AddVertex(-0.34f,	0, -1,		1, 3);
			AddVertex(0.34f,	0, -1,		2, 3);
			AddVertex(1,		0, -1,		3, 3);
			var vertexArray = vertexBufferBuilder.ToArray();
			vertexBuffer = Buffer.Create<MeshVertex>(Device, BindFlags.VertexBuffer, vertexArray);

			vertexBufferBinding.Buffer = vertexBuffer;
			vertexBufferBinding.Stride = Utilities.SizeOf<MeshVertex>();
			vertexBufferBinding.Offset = 0;

			int z = 4;
		}

		protected override void OnUpdate()
		{
			DeviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.PatchListWith16ControlPoints;
			DeviceContext.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);

			Game.Instance.UpdatePerObjectBuffer(Matrix.Identity);

			material.Passes[0].Apply();
			material.Passes[0].UpdateConstantBuffers();
			DeviceContext.Draw(16, 0);
			material.Passes[0].Clear();

			int highlight = (int)HighlightLine;
			for (int i = 0; i < 16; ++i)
				Debug.Line(Vector3.Zero, vertexBufferBuilder[i].Position, (i == highlight) ? Color.Magenta : Color.Black);
			HighlightLine = highlight;
		}

		void AddVertex(float x, float y, float z, float u, float v)
		{
			MeshVertex vertex;
			vertex.Position = new Vector3(x, y, z);
			vertex.Normal = Vector3.Zero;
			vertex.Tangent = Vector3.Zero;
			vertex.TexCoord = new Vector2(u, v);
			vertexBufferBuilder.Add(vertex);
		}
	}
}
