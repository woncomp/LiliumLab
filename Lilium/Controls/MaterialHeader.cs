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
	public partial class MaterialHeader : Control
	{
		private Material material;
		public MaterialHeader(Material material)
		{
			InitializeComponent();

			this.material = material;
		}

		private void btnReload_Click(object sender, EventArgs e)
		{
			material.Reload();
			Game.Instance.SelectedObject = material;
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			Game.Instance.ResourceManager.Material.Save(material);
		}
	}
}
