using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lilium.Controls
{
	public partial class PassHeader : Control
	{
		private MaterialPassDesc passDesc;

		public PassHeader(string title, MaterialPassDesc desc)
		{
			InitializeComponent();

			passDesc = desc;

			labelTitle.Text = title;
			labelShader.Text = desc.ShaderFile;
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			var shaderFile = ResourceBrowser.ChooseShader();
			if (shaderFile != null)
			{
				passDesc.ShaderFile = shaderFile;
				labelShader.Text = passDesc.ShaderFile;
			}
		}

		private void btnRasterizer_Click(object sender, EventArgs e)
		{
			var pass = passDesc;
			var serializing = new Serializing.RasterizerState();
			serializing.Import(ref pass.RasteriazerStates);
			PropertiesForm.Show(serializing, () =>
				{
					serializing.Export(ref pass.RasteriazerStates);
				});
		}

		private void btnBlend_Click(object sender, EventArgs e)
		{
			var pass = passDesc;
			var serializing = new Serializing.BlendState();
			serializing.Import(ref pass.BlendStates);
			PropertiesForm.Show(serializing, () =>
			{
				serializing.Export(ref pass.BlendStates);
			});
		}

		private void btnDepthStencil_Click(object sender, EventArgs e)
		{
			var pass = passDesc;
			var serializing = new Serializing.DepthStencilState();
			serializing.Import(ref pass.DepthStencilStates);
			PropertiesForm.Show(serializing, () =>
			{
				serializing.Export(ref pass.DepthStencilStates);
			});
		}

		private void btnEntries_Click(object sender, EventArgs e)
		{
			var pass = passDesc;
			var serializing = new Serializing.ShaderEntry();
			serializing.Import(ref pass);
			PropertiesForm.Show(serializing, () =>
			{
				serializing.Export(ref pass);
			});
		}
	}
}
