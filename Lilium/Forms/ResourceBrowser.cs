using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lilium
{
	public partial class ResourceBrowser : Form
	{
		public static string ChooseTexture2D()
		{
			var w = new ResourceBrowser();
			w.Text = "Choose Texture 2D";
			var mgr = Game.Instance.ResourceManager;
			return w.ChooseResource(mgr.SearchPaths, mgr.Tex2D.SubfolderName);
		}

		public static string ChooseShader()
		{
			var w = new ResourceBrowser();
			w.Text = "Choose Shader";
			var mgr = Game.Instance.ResourceManager;
			return w.ChooseResource(mgr.SearchPaths, ResourceManager.SUBFOLDER_SHADER);
		}

		public static string ChooseMaterial()
		{
			var w = new ResourceBrowser();
			w.Text = "Choose Material";
			var mgr = Game.Instance.ResourceManager;
			return w.ChooseResource(mgr.SearchPaths, mgr.Material.SubfolderName);
		}

		public static string ChooseMesh()
		{
			var w = new ResourceBrowser();
			w.Text = "Choose Mesh";
			var mgr = Game.Instance.ResourceManager;
			return w.ChooseResource(mgr.SearchPaths, mgr.Mesh.SubfolderName);
		}

		private List<string> resourceNames = new List<string>();

		public ResourceBrowser()
		{
			InitializeComponent();
		}

		string ChooseResource(List<string> searchPaths, string subfolder)
		{
			for (int i = searchPaths.Count - 1; i >= 0; --i)
			{
				var folder = Path.Combine(searchPaths[i], subfolder);
				if (Directory.Exists(folder))
				{
					foreach (var file in Directory.GetFiles(folder))
					{
						var name = Path.GetFileName(file);
						if (!resourceNames.Contains(name))
						{
							resourceNames.Add(name);
							this.listBox1.Items.Add(name);
						}
					}
				}
			}
			var dr = ShowDialog();
			if (dr == System.Windows.Forms.DialogResult.OK)
			{
				return this.listBox1.SelectedItem.ToString();
			}
			else
			{
				return null;
			}
		}

		private void btnChoose_Click(object sender, EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.OK;
			Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.Cancel;
			Close();
		}
	}
}
