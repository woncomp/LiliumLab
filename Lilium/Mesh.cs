using Assimp;
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
	public struct MeshVertex
	{
		public Vector3 Position;
		public Vector3 Normal;
		public Vector3 Tangent;
		public Vector2 TexCoord;
	}

	public class Mesh : MeshConstructor, IDisposable
	{
		internal List<Submesh> submeshes = new List<Submesh>();

		Device device;

		internal Buffer vertexBuffer;
		internal Buffer indexBuffer;

		internal VertexBufferBinding vertexBufferBinding;

		internal TangentSpaceBasisRenderer tangentRenderer;

		public string ResourceName;
				
		private string debugName;
		public string DebugName
		{
			get { return debugName; }
			set
			{
				debugName = value;
				vertexBuffer.DebugName = debugName + " VB";
				indexBuffer.DebugName = debugName + " IB";
			}
		}

		public int SubmeshCount { get { return submeshes.Count; } }

		public Mesh(SharpDX.Direct3D11.Device device)
		{
			this.device = device;
		}

		public void DrawBegin()
		{
			var dc = this.device.ImmediateContext;
			dc.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
			dc.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);
			dc.InputAssembler.SetIndexBuffer(indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);
		}

		public void DrawSubmesh(int submeshIndex)
		{
			var dc = this.device.ImmediateContext;
			var submesh = this.submeshes[submeshIndex];

			dc.DrawIndexed(submesh.Count, submesh.Start, 0);
		}

		public void DrawTangentSpaceBasis()
		{
			if (Config.DrawTBN)
			{
				tangentRenderer.OnRender();
			}
		}

		public Submesh GetSubmesh(int index)
		{
			return this.submeshes[index];
		}

		public void Dispose()
		{
			tangentRenderer.Dispose();
			if(vertexBuffer != null)
			{
				vertexBuffer.Dispose();
				indexBuffer.Dispose();
			}
		}
	}

	public class Submesh
	{
		public int Start;
		public int Count;
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
		private Material material;

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

						if(hasTangent)
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
				var materialDesc = new MaterialDesc();
				var passDesc = materialDesc.Passes[0];
				passDesc.ManualConstantBuffers = true;
				passDesc.ShaderFile = InternalResources.SHADER_DEBUG_LINE;
				passDesc.InputElements = new InputElement[]{
					new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0),
					new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 12, 0),
				};
				material = new Material(Game.Instance, materialDesc, "TangentSpaceBasisRenderer");
				matrixBuffer = Material.CreateBuffer<Matrix>();
			}
		}

		public void OnRender()
		{
			var dc = Game.Instance.DeviceContext;

			material.Passes[0].Apply();

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
			material.Dispose();
		}
	}

	public class MeshConstructor
	{
		public static Mesh CreateQuad(float width = 20, float height = 0)
		{
			var hw = width / 2;
			var hh = height > 0 ? height / 2 : hw;
			var meshBuilder = new MeshBuilder();

			meshBuilder.BeginSubmesh();

			var vertices = new MeshVertex[4];
			vertices[0] = new MeshVertex();
			vertices[0].Position = new Vector3(-hw, -hh, 0);
			vertices[0].Normal = new Vector3(0, 0, 1);
			//vertices[0].Tangent = new Vector3(0, 1, 0);
			vertices[0].TexCoord = new Vector2(1, 1);

			vertices[1] = new MeshVertex();
			vertices[1].Position = new Vector3(hw, -hh, 0);
			vertices[1].Normal = new Vector3(0, 0, 1);
			//vertices[1].Tangent = new Vector3(1, 0, 0);
			vertices[1].TexCoord = new Vector2(0, 1);

			vertices[2] = new MeshVertex();
			vertices[2].Position = new Vector3(-hw, hh, 0);
			vertices[2].Normal = new Vector3(0, 0, 1);
			//vertices[2].Tangent = new Vector3(-1, 0, 0);
			vertices[2].TexCoord = new Vector2(1, 0);

			vertices[3] = new MeshVertex();
			vertices[3].Position = new Vector3(hw, hh, 0);
			vertices[3].Normal = new Vector3(0, 0, 1);
			//vertices[3].Tangent = new Vector3(0, -1, 0);
			vertices[3].TexCoord = new Vector2(0, 0);

			vertices.ToList().ForEach(meshBuilder.Vertex);

			meshBuilder.Index(0);
			meshBuilder.Index(1);
			meshBuilder.Index(2);
			meshBuilder.Index(2);
			meshBuilder.Index(1);
			meshBuilder.Index(3);
			
			meshBuilder.EndSubmesh();

			return meshBuilder.Complete();
		}

		public static Mesh CreatePlane(float scale = 5)
		{
			return CreateFromFile(InternalResources.MESH_PLANE, true, false, scale);
		}

		public static Mesh CreateCube(float scale = 5)
		{
			return CreateFromFile(InternalResources.MESH_CUBE, true, false, scale);
		}

		public static Mesh CreateSphere(float scale = 10)
		{
			return CreateFromFile(InternalResources.MESH_SPHERE, true, false, scale);
		}

		public static Mesh CreateTeapot(float scale = 5)
		{
			return CreateFromFile(InternalResources.MESH_TEAPOT, true, false, scale);
		}

		public static Mesh CreateFromFile(string filePath, bool rotateYZ = false, bool convertToLeftHanded = false, float scale = 1)
		{
			Func<Assimp.Vector3D, Vector3> _MakeVector3 = v => new Vector3(v.X, v.Y, v.Z);
			Func<Assimp.Vector3D, Vector2> _MakeTexCoord = v =>
				{
					var x = v.X; while (x > 1) x -= 1; while (x < 0) x += 1;
					var y = v.Y; while (y > 1) y -= 1; while (y < 0) y += 1;
					return new Vector2(x, y);
				};

			using (var importer = new AssimpImporter())
			{
				if (Config.LoadMeshPrintLog)
				{
					LogStream logStream = new LogStream((msg, data) =>
					{
						Console.WriteLine(string.Format("Assimp: {0}", msg));
					});
					importer.AttachLogStream(logStream);
				}

				PostProcessSteps options = PostProcessSteps.None;
				if(Config.LoadMeshComputeTangent)
					options |= PostProcessSteps.CalculateTangentSpace;
				if(convertToLeftHanded)
					options |= PostProcessSteps.MakeLeftHanded;

				var scene = importer.ImportFile(filePath, options);

				if (!scene.HasMeshes) return null;

				var builder = new MeshBuilder();

				Matrix mat = Matrix.Identity;
				if(rotateYZ)
					mat = Matrix.RotationX(-MathUtil.PiOverTwo);

				foreach (var aiMesh in scene.Meshes)
				{
					builder.BeginSubmesh();

					for (int i = 0; i < aiMesh.VertexCount; ++i)
					{
						var v = new MeshVertex();
						v.Position = _MakeVector3(aiMesh.Vertices[i]) * scale;
						v.Position = Vector3.TransformCoordinate(v.Position, mat);
						if (aiMesh.HasNormals)
						{
							v.Normal = _MakeVector3(aiMesh.Normals[i]);
							v.Normal = Vector3.TransformNormal(v.Normal, mat);
						}
						if (aiMesh.HasTangentBasis)
						{
							v.Tangent = _MakeVector3(aiMesh.Tangents[i]);
							v.Tangent = Vector3.TransformNormal(v.Tangent, mat);
						}

						if (aiMesh.HasTextureCoords(0)) v.TexCoord = _MakeTexCoord(aiMesh.GetTextureCoords(0)[i]);

						builder.Vertex(v);
					}

					aiMesh.GetIntIndices().ToList().ForEach(builder.Index);

					if (scene.HasMaterials)
					{
						var folder = System.IO.Path.GetDirectoryName(filePath);
						var materialIndex = aiMesh.MaterialIndex;
						if (materialIndex >= 0 && materialIndex < scene.Materials.Length)
						{
							var material = scene.Materials[materialIndex];
							var textures = material.GetTextures(TextureType.Diffuse);
							if (textures != null)
							{
								builder.Texture(0, System.IO.Path.Combine(folder, textures[0].FilePath));
							}
							textures = material.GetTextures(TextureType.Ambient);
							if (textures != null)
								builder.Texture(1, System.IO.Path.Combine(folder, textures[0].FilePath));
							textures = material.GetTextures(TextureType.Specular);
							if (textures != null)
								builder.Texture(2, System.IO.Path.Combine(folder, textures[0].FilePath));
						}
					}

					builder.EndSubmesh();
				}

				return builder.Complete();
			}
		}

		public static Mesh CreateFromTXT(string filePath, float scale = 1.0f)
		{
			using(System.IO.StreamReader sr = new System.IO.StreamReader(filePath))
			{
				string line = sr.ReadLine();
				while (!line.Contains("Data:")) line = sr.ReadLine();

				var builder = new MeshBuilder();
				builder.BeginSubmesh();

				int index = 0;

				while(!sr.EndOfStream)
				{
					line = sr.ReadLine();
					if(line.Length > 0)
					{
						var v = new MeshVertex();
						var n = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
						v.Position = new Vector3(float.Parse(n[0]), float.Parse(n[1]), float.Parse(n[2])) * scale;
						v.TexCoord = new Vector2(float.Parse(n[3]), float.Parse(n[4]));
						v.Normal = new Vector3(float.Parse(n[5]), float.Parse(n[6]), float.Parse(n[7]));
						builder.Vertex(v);
						builder.Index(index++);
					}
				}

				builder.EndSubmesh();
				return builder.Complete(true);
			}
		}
	}

	class MeshBuilder
	{
		List<SubmeshInfo> submeshes = new List<SubmeshInfo>();
		SubmeshInfo currentSubmesh;
		int vertexCount = 0;
		int indexCount = 0;

		public MeshBuilder()
		{
		}

		public void BeginSubmesh()
		{
			if(currentSubmesh == null)
				currentSubmesh = new SubmeshInfo();
		}

		public void EndSubmesh()
		{
			if (currentSubmesh != null)
			{
				submeshes.Add(currentSubmesh);
				currentSubmesh = null;
			}
		}

		public void Vertex(MeshVertex vertex)
		{
			BeginSubmesh();
			currentSubmesh.Vertices.Add(vertex);
			vertexCount++;
		}

		public void Index(int index)
		{
			BeginSubmesh();
			currentSubmesh.Indices.Add(index);
			indexCount++;
		}

		public void Texture(int index, string texture)
		{
			BeginSubmesh();
			while (currentSubmesh.Textures.Count <= index) currentSubmesh.Textures.Add(null);
			currentSubmesh.Textures[index] = texture;
		}

		class AccumNormalTangent
		{
			public Vector3 AccumNormal = new Vector3();
			public Vector3 AccumTangent = new Vector3();
		}

		private void ComputeTangent(List<MeshVertex> vertices, List<int> indices)
		{
			Vector3[] tangents = vertices.Select(mv => new Vector3()).ToArray();
			Vector3[] binormals = vertices.Select(mv => new Vector3()).ToArray();
			for (int i = 0, c = indices.Count / 3; i < c; ++i)
			{
				int i0 = 3*i;
				int i1 = 3*i+1;
				int i2 = 3*i+2;
				
				var vert0 = vertices[indices[i0]];
				var vert1 = vertices[indices[i1]];
				var vert2 = vertices[indices[i2]];
				var p0 = vert0.Position;
				var p1 = vert1.Position;
				var p2 = vert2.Position;

				var v01 = p1 - p0;
				var v02 = p2 - p0;

				v01.Normalize();
				v02.Normalize();

				var tex01 = vert1.TexCoord - vert0.TexCoord;
				var tex02 = vert2.TexCoord - vert0.TexCoord;

				var r = 1 / (tex01.X * tex02.Y - tex02.X * tex01.Y);

				var t = (tex02.Y * v01 - tex01.Y * v02) * r;
				var b = (-tex02.X * v01 + tex01.X * v02) * r;

				for (int j = 0; j < 3; ++j)
				{
					int idx = indices[i0 + j];
					tangents[idx] += t;
					binormals[idx] += b;
				}
			}
			for (int i = 0; i < vertices.Count; ++i)
			{
				var mv = vertices[i];

				var tan = tangents[i];
				tan.Normalize();
				var binormal = Vector3.Cross(mv.Normal, tan);
				mv.Tangent = Vector3.Cross(binormal, mv.Normal);

				vertices[i] = mv;
			}
		}

		public Mesh Complete(bool computeTangent = false)
		{
			EndSubmesh();
			Device device = Game.Instance.Device;

			var mesh = new Mesh(device);

			List<MeshVertex> vertices = new List<MeshVertex>();
			List<int> indices = new List<int>();

			foreach(var smInfo in submeshes)
			{
				var submesh = new Submesh();
				submesh.Start = indices.Count;
				submesh.Count = smInfo.Indices.Count;

				mesh.submeshes.Add(submesh);

				indices.AddRange(smInfo.Indices.Select(i => vertices.Count + i));
				vertices.AddRange(smInfo.Vertices);
			}

			if(computeTangent) ComputeTangent(vertices, indices);

			mesh.tangentRenderer = new TangentSpaceBasisRenderer(vertices);

			mesh.vertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, vertices.ToArray());
			mesh.vertexBufferBinding = new VertexBufferBinding(mesh.vertexBuffer, Utilities.SizeOf<MeshVertex>(), 0);

			//mesh.indexBuffer = new Buffer(device, Utilities.SizeOf<int>() * indexCount,
			mesh.indexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, indices.ToArray());

			return mesh;
		}

		class SubmeshInfo
		{
			public List<MeshVertex> Vertices = new List<MeshVertex>();
			public List<int> Indices = new List<int>();

			public List<string> Textures = new List<string>();
		}
	}
}
