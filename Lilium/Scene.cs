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
	public class Scene : IDisposable, ISelectable
	{
		public List<Entity> Entities;

		public string Name = "";

		public Scene()
		{
			Entities = new List<Entity>();
			CreateControls();
		}

		public void Save(string filePath)
		{
			Lilium.Serializing.Scene.Serialize(this, filePath);
		}

		public void Load(string filePath)
		{
			Lilium.Serializing.Scene.Deserialize(this, filePath);
		}

		public void Draw()
		{
			foreach (var entity in Entities)
			{
				entity.Draw();
			}
		}

		public void Dispose()
		{
			for (int i = 0; i < Entities.Count; ++i)
			{
				var entity = Entities[i];
				Utilities.Dispose(ref entity);
			}
		}

		#region Selectable

		private Controls.Control[] controls;

		void CreateControls()
		{
			var btn1 = new Lilium.Controls.Button("Add Entity", () =>
			{
				var meshName = ResourceBrowser.ChooseMesh();
				if(!string.IsNullOrEmpty(meshName))
				{
					var entity = new Entity(meshName);
					Entities.Add(entity);
					Game.Instance.AddObject(entity);
				}
			});
			var btn2 = new Lilium.Controls.Button("Save", () =>
			{
				var path = System.IO.Path.Combine(Game.Instance.ResourceManager.FirstSearchFolder, "MainScene.txt");
				Save(path);
			});
			var btn3 = new Lilium.Controls.Button("Load", () =>
			{
				var path = System.IO.Path.Combine(Game.Instance.ResourceManager.FirstSearchFolder, "MainScene.txt");
				if (System.IO.File.Exists(path))
					Load(path);
			});
			controls = new Controls.Control[] { btn1, btn2, btn3 };
		}

		public Controls.Control[] Controls { get { return controls; } }
		public string NameInObjectList { get { return Name; } }

		#endregion
	}
}
