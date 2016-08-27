using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Cyotek.Drawing.BitmapFont;

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Lilium
{
	public class UIFont : IDisposable
	{
		public ShaderResourceView Texture;
		public BitmapFont BMFont;

		public void Load(Device device, string filePath)
		{
			BMFont = BitmapFontLoader.LoadFontFromFile(filePath);
			Texture = ShaderResourceView.FromFile(device, filePath.Replace(".fnt", ".png"));
		}

		public void Dispose()
		{
			Texture.Dispose();
		}
	}
}
