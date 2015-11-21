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
	public partial class TextArea : Control
	{
		public new string Text
		{
			get { return textBox1.Text; }
			set { textBox1.Text = value.Replace("\r\n", "\n").Replace("\n", "\r\n"); }
		}

		public TextArea()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			var str = textBox1.Text;
			Debug.Log(str);
		}
	}
}
