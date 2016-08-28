using SharpDX.Windows;
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
	public partial class MainForm : Form
	{
		public static MainForm Instance;

		public RenderControl RenderControl;
		
		private Game game;

		public MainForm(Game game)
		{
			Instance = this;
			InitializeComponent();

			this.game = game;
			this.Text = game.GetType().Name;
		}

		public void AddObject(ISelectable obj)
		{
			lbObjects.Items.Add(new ListBoxItem( obj ));
		}

		protected override void OnCreateControl()
		{
			base.OnCreateControl();

			this.RenderControl = new RenderControl();

			if (game != null)
			{
				game.BindWithWindow(RenderControl);
			}
			this.RenderControl.Dock = DockStyle.Fill;
			this.splitContainer1.Panel1.Controls.Add(RenderControl);
		}

		private void lbObjects_SelectedIndexChanged(object sender, EventArgs e)
		{
			var item = lbObjects.SelectedItem as ListBoxItem;
			ISelectable obj = null;
			if (item != null) obj = item.target;
			game.Info_SetSelcectedObject(obj);
		}

		class ListBoxItem
		{
			public ISelectable target;
			public string Name;

			public ListBoxItem(ISelectable target)
			{
				this.target = target;
				var attr = target.GetType().GetCustomAttributes(typeof(CustomSelectedTypeNameAttribute), false);
				if(attr.Length > 0)
				{
					Name = (attr[0] as CustomSelectedTypeNameAttribute).TypeName;
				}
				else
				{
					Name = target.GetType().Name;
				}
				Name = string.Format("{0,-20} {1}", Name, target.NameInObjectList);
			}

			public override string ToString() { return Name; }
		}
	}
}
