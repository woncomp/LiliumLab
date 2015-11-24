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
	public partial class EntityMaterialSlot : Control
	{
		Entity entity;
		int materialIndex;

		public EntityMaterialSlot(Entity entity, int materialIndex)
		{
			InitializeComponent();

			this.entity = entity;
			this.materialIndex = materialIndex;

			this.labelTitle.Text = "Material " + materialIndex;

			this.Paint += EntityMaterialSlot_Paint;
		}

		private void btnMaterialName_Click(object sender, EventArgs e)
		{
			var m = entity.SubmeshMaterials[materialIndex];
			if (m != null) Game.Instance.SelectedObject = m;
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			var resName = ResourceBrowser.ChooseMaterial();
			if (!string.IsNullOrEmpty(resName))
			{
				entity.SubmeshMaterials[materialIndex] = Game.Instance.ResourceManager.Material.Load(resName);
			}
		}

		public override void UpdateData()
		{
			string name = "Empty";
			var m = entity.SubmeshMaterials[materialIndex];
			if (m != null) name = m.DebugName;
			this.btnMaterialName.Text = name;
		}

		void EntityMaterialSlot_Paint(object sender, PaintEventArgs e)
		{
			Brush brush = Brushes.Yellow;
			var m = entity.SubmeshMaterials[materialIndex];
			if(m != null) brush = m.IsValid ? Brushes.Green : Brushes.Red;
			e.Graphics.FillEllipse(brush, 274, 0, 18, 18);
		}
	}
}
