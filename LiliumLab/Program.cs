using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lilium;

namespace LiliumLab
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			if (!SharpDX.Direct3D11.Device.IsSupportedFeatureLevel(SharpDX.Direct3D.FeatureLevel.Level_11_0))
			{
				System.Windows.Forms.MessageBox.Show("DirectX11 Not Supported");
				return;
			}
			Lilium.Config.DebugMode = true;
			var game = new TestGame();

			// Editor
			var form = new MainForm(game);

			// Game
			//var form = new SharpDX.Windows.RenderForm(game.GetType().Name);
			//form.Size = new System.Drawing.Size(1600, 900);
			//game.BindWithWindow(form);

			System.Windows.Forms.Application.Run(form);
		}
	}
}
