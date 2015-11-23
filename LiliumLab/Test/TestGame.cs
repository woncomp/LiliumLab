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
	public class TestGame : Game
	{
		
		protected override void OnStart()
		{
			ResourceManager.SearchPaths.Add("../../Test");

			var path = System.IO.Path.Combine(Game.Instance.ResourceManager.FirstSearchFolder, "MainScene.txt");
			if (System.IO.File.Exists(path))
				MainScene.Load(path);
		}

		protected override void OnUpdate()
		{
			
		}
	}
}
