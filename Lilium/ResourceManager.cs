using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX.D3DCompiler;

namespace Lilium
{
	public class ResourceManager : IDisposable
	{
		public const string SUBFOLDER_SHADER = "Shader";

		public List<string> SearchPaths = new List<string>();

		public string FirstSearchFolder { get { return SearchPaths[SearchPaths.Count - 1]; } }

		public Game Game { get; private set; }

		public Loader<ShaderResourceView> Tex2D { get { return tex2D; } }
		public Loader<Mesh> Mesh { get { return mesh; } }
		public MaterialLoader Material { get { return material; } }

		Texture2DLoader tex2D;
		MaterialLoader material;
		MeshLoader mesh;

		public ResourceManager(Game game)
		{
			this.Game = game;

			this.tex2D = Texture2DLoader.Create<Texture2DLoader>(this);
			this.material = MaterialLoader.Create<MaterialLoader>(this);
			this.mesh = MeshLoader.Create<MeshLoader>(this);
		}

		public string FindValidShaderFilePath(string shaderName)
		{
			return FindValidResourceFilePath(shaderName, SUBFOLDER_SHADER);
		}

		string FindValidResourceFilePath(string resName, string subfolder)
		{
			for (int i = SearchPaths.Count - 1; i >= 0; --i)
			{
				var folder = Path.Combine(SearchPaths[i], subfolder);
				var filePath = Path.Combine(folder, resName);
				if (File.Exists(filePath))
				{
					return filePath;
				}
			}
			return null;
		}

		public void Dispose()
		{
			mesh.Dispose();
			material.Dispose();
			tex2D.Dispose();
		}
	}

	public abstract class Loader<ResT> :IDisposable where ResT : class, IDisposable
	{
		public static LoaderT Create<LoaderT>(ResourceManager mgr) where LoaderT : Loader<ResT>, new()
		{
			var loader = new LoaderT();
			loader.mgr = mgr;
			return loader;
		}

		public ResourceManager mgr;

		public abstract string SubfolderName { get; }
		protected abstract ResT LoadFunc(Device device, string filePath);

		protected string LoadingResourceName;

		Dictionary<string, ResT> dic = new Dictionary<string, ResT>();

		public ResT Load(string resName, bool forceReload = false)
		{
			LoadingResourceName = resName;
			bool contains = dic.ContainsKey(resName);
			if (forceReload && contains)
			{
				var obj = dic[resName];
				Utilities.Dispose(ref obj);
			}
			if (forceReload || !contains)
			{
				ResT obj = null;
				for (int i = mgr.SearchPaths.Count - 1; i >= 0; --i)
				{
					var folder = Path.Combine(mgr.SearchPaths[i], SubfolderName);
					var filePath = Path.Combine(folder, resName);
					if (File.Exists(filePath))
					{
						try
						{
							obj = LoadFunc(mgr.Game.Device, filePath);
							dic[resName] = obj;
							break;
						}
						catch (Exception e)
						{
							Debug.Log(e.Message);
						}
					}
				}
				if(obj == null)
				{
					try
					{
						obj = LoadFunc(mgr.Game.Device, resName);
						dic[resName] = obj;
					}
					catch (Exception e)
					{
						Debug.Log(e.Message);
					}
				}
				if (obj == null)
				{
					Debug.Log("Load resource failed - " + resName);
				}
				return obj;
			}
			else
			{
				return dic[resName];
			}
		}

		public void Dispose()
		{
			foreach (var pair in dic)
			{
				var obj = pair.Value;
				Utilities.Dispose(ref obj);
			}
		}
	}

	class Texture2DLoader : Loader<ShaderResourceView>
	{
		public override string SubfolderName { get { return "Texture2D"; } }

		protected override ShaderResourceView LoadFunc(Device device, string filePath)
		{
			var tex = ShaderResourceView.FromFile(device, filePath);
			tex.DebugName = Path.GetFileNameWithoutExtension(filePath);
			return tex;
		}
	}

	public class MaterialLoader : Loader<Material>
	{
		public override string SubfolderName { get { return "Material"; } }

		public void Save(Material material)
		{
			var desc = material.Desc;
			var oldFilePath = desc.FilePath;
			desc.FilePath = Path.Combine(mgr.FirstSearchFolder, SubfolderName, Path.GetFileName(oldFilePath));
			desc.Save();
			desc.FilePath = oldFilePath;
		}

		protected override Material LoadFunc(Device device, string filePath)
		{
			var desc = MaterialDesc.Load(filePath);
			var material = new Material(desc);
			material.ResourceName = LoadingResourceName;
			return material;
		}
	}

	class MeshLoader : Loader<Mesh>
	{
		public override string SubfolderName { get { return "Mesh"; } }

		protected override Mesh LoadFunc(Device device, string filePath)
		{
			Mesh mesh = null;
			if(filePath.EndsWith( InternalResources.MESH_QUAD))
			{
				mesh = Mesh.CreateQuad();
			}
			else if(filePath.EndsWith( InternalResources.MESH_CUBE))
			{
				mesh = Mesh.CreateCube();
			}
			else if (filePath.EndsWith(  InternalResources.MESH_PLANE))
			{
				mesh = Mesh.CreatePlane();
			}
			else if (filePath.EndsWith(  InternalResources.MESH_SPHERE))
			{
				mesh = Mesh.CreateSphere();
			}
			else if (filePath.EndsWith(  InternalResources.MESH_TEAPOT))
			{
				mesh = Mesh.CreateTeapot();
			}
			else if (filePath.EndsWith(".txt", StringComparison.CurrentCultureIgnoreCase))
			{
				mesh = Mesh.CreateFromTXT(filePath);
			}
			else
			{
				mesh = Mesh.CreateFromFile(filePath);
			}
			mesh.ResourceName = LoadingResourceName;
			return mesh;
		}
	}
}