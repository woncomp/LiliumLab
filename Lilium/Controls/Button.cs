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
	public partial class Button : Control
	{
		private Action _action;

		public Button(string title, Action action)
		{
			InitializeComponent();

			_action = action;

			button1.Text = title;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			_action();
		}
	}
}
