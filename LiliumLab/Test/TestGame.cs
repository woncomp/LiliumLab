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
		UILabel mFPSLabel;
		
		protected override void OnStart()
		{
			ResourceManager.SearchPaths.Add("../../Test");

			//var path = System.IO.Path.Combine(Game.Instance.ResourceManager.FirstSearchFolder, "MainScene.txt");
			//if (System.IO.File.Exists(path))
			//	MainScene.Load(path);

			mFPSLabel = new UILabel();
			mFPSLabel.SetFont("font.fnt");
			UI.AddWidget(mFPSLabel);

			var label = new UILabel();
			label.Position = new Vector2(0, 600);
			label.SetFont("font.fnt");
			label.Text = "Lorem ipsum dolor sit amet,\nconsectetuer adipiscing elit.\n9876543210";
			UI.AddWidget(label);
		}

		protected override void OnUpdate()
		{
			mFPSLabel.Text = string.Format("{0:00.00}ms",(FrameTime * 1000));
		}
	}
}
