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

		public Lilium.Controls.RenderControl RenderControl;

		public Action<ISelectable> SelectedObjectChanged;

		private Game game;

		public MainForm(Game game)
		{
			Instance = this;
			InitializeComponent();

			this.game = game;
		}

		public void AddObject(ISelectable obj)
		{
			lbObjects.Items.Add(new ListBoxItem( obj ));
		}

		protected override void OnCreateControl()
		{
			base.OnCreateControl();

			this.RenderControl = new Lilium.Controls.RenderControl();
			this.RenderControl.Dock = DockStyle.Fill;
			this.splitContainer1.Panel1.Controls.Add(RenderControl);

			if (game != null)
			{
				this.game.RenderControl = this.RenderControl;

				var timer = new Timer();
				timer.Interval = 100;
				timer.Tick += timer_Tick;
				timer.Start();
			}
		}

		void timer_Tick(object sender, EventArgs e)
		{
			var timer = sender as Timer;
			timer.Stop();
			timer.Dispose();

			RenderControl.Start += game.Init;
			RenderControl.Update += game.LoopUpdate;
			RenderControl.WillDispose += game.Dispose;

			RenderControl.Startup();
		}

		private void lbObjects_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (SelectedObjectChanged != null)
			{
				var item = lbObjects.SelectedItem as ListBoxItem;
				ISelectable obj =  null;
				if(item != null)obj = item.target;
				SelectedObjectChanged(obj);
			}
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
