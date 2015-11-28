using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D11;

namespace Lilium.Controls
{
	public partial class PassTextureSlot : Control
	{
		MaterialPass pass;
		int textureIndex;

		public PassTextureSlot(MaterialPass pass, int textureIndex)
		{
			InitializeComponent();

			this.pass = pass;
			this.textureIndex = textureIndex;
			this.labelTitle.Text = "Texture " + textureIndex;
			this.label1.Text = pass.Desc.Textures[textureIndex].TextureFile;
			UpdateThumb();
		}

		private void btnSampler_Click(object sender, EventArgs e)
		{
			var desc = pass.Desc.Textures[textureIndex];
			var serializing = new Serializing.MaterialTexture();
			serializing.Import(desc);
			PropertiesForm.Show(serializing, () =>
			{
				serializing.Export(desc);
			});
		}

		private void pictureBox1_Click(object sender, EventArgs e)
		{
			var file = ResourceBrowser.ChooseTexture2D();
			if(file != null)
			{
				pass.Desc.Textures[textureIndex].TextureFile = file;
				pass.TextureList[textureIndex] = Game.Instance.ResourceManager.Tex2D.Load(file);
				this.label1.Text = pass.Desc.Textures[textureIndex].TextureFile;
				UpdateThumb();
			}
		}

		void UpdateThumb()
		{
			var game = Game.Instance;

			var filePath = game.ResourceManager.FindValidResourceFilePath(pass.Desc.Textures[textureIndex].TextureFile, game.ResourceManager.Tex2D.SubfolderName);
			if (System.IO.File.Exists(filePath))
			{
				if (filePath.EndsWith(".dds"))
				{
					pictureBox1.Image = null;
				}
				else
				{
					try
					{
						pictureBox1.Image = new Bitmap(filePath);
					}
					catch (Exception e)
					{
						Debug.Log(e.Message);
					}
				}
			}
			else
				pictureBox1.Image = null;

			//var tex2d = shaderResourceView.Resource.QueryInterface<Texture2D>();
			//var desc = tex2d.Description;
			//desc.Usage = ResourceUsage.Staging;
			//desc.CpuAccessFlags = CpuAccessFlags.Read;
			//var stagingTexture = new Texture2D(game.Device, desc);
			//game.DeviceContext.CopyResource(tex2d, stagingTexture);
			//var box = game.DeviceContext.MapSubresource(stagingTexture, 0, MapMode.Read, MapFlags.None);

			//(tex2d as IUnknown).Release();
		}
	}
}
