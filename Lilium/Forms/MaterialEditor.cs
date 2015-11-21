using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lilium
{
	public partial class MaterialEditor : Form
	{
		private Material material;

		private Serializing.Material serializing;

		public MaterialEditor(Material material)
		{
			InitializeComponent();

			this.material = material;
			this.serializing = new Serializing.Material();
			this.serializing.Import(this.material.Desc);

			this.propertyGrid1.SelectedObject = this.serializing;
			//this.tbShaderFile.Text = this.serializing.ShaderFile;
		}

		private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
		{
			this.serializing.Export(this.material.Desc);
		}

		private void btnBrowseShader_Click(object sender, EventArgs e)
		{
			var shaderName = ResourceBrowser.ChooseShader();
			if(shaderName != null)
			{
				//this.tbShaderFile.Text = this.serializing.ShaderFile = shaderName;
				this.serializing.Export(this.material.Desc);
			}
		}
	}
}
