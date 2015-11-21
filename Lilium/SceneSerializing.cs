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
using Newtonsoft.Json;

namespace Lilium.Serializing
{
	class Scene
	{
		public SceneEntity[] Entities;

		public static void Serialize(Lilium.Scene scene, string filePath)
		{
			var sceneSerializing = new Scene();
			sceneSerializing.FromScene(scene);
			var str = JsonConvert.SerializeObject(sceneSerializing, Formatting.Indented);
			System.IO.File.WriteAllText(filePath, str);
		}

		public static void Deserialize(Lilium.Scene scene, string filePath)
		{
			var str = System.IO.File.ReadAllText(filePath);
			var sceneSerializing = JsonConvert.DeserializeObject<Scene>(str);
			sceneSerializing.LoadScene(scene);
		}

		void FromScene(Lilium.Scene scene)
		{
			Entities = scene.Entities.Select(src =>
			{
				var dest = new SceneEntity();
				dest.Mesh = src.Mesh.ResourceName;
				dest.Materials = src.SubmeshMaterials.Select(m =>
					{
						if (m == null) return null;
						else return m.Desc.ResourceName;
					}).ToArray();
				dest.Position = src.Position;
				dest.Rotation = src.Rotation;
				dest.Scale = src.Scale;
				return dest;
			}).ToArray();
		}

		void LoadScene(Lilium.Scene scene)
		{
			for (int i = 0; i < Entities.Length; ++i)
			{
				var entityS = Entities[i];

				var entity = new Lilium.Entity(entityS.Mesh);
				if(entity.Mesh != null)
				{
					entity.SubmeshMaterials = entityS.Materials.Select(name => Game.Instance.ResourceManager.Material.Load(name)).ToArray();
					entity.Position = entityS.Position;
					entity.Rotation = entityS.Rotation;
					entity.Scale = entityS.Scale;
					scene.Entities.Add(entity);
					Game.Instance.AddObject(entity);
				}
			}
		}
	}

	class SceneEntity
	{
		public string Mesh;
		public string[] Materials;

		public Vector3 Position;
		public Vector3 Rotation;
		public Vector3 Scale;
	}
}
