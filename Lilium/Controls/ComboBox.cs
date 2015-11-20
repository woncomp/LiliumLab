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
	public partial class ComboBox : Control
	{
		private Func<int> _get;
		private Action<int> _set;

		private bool _ignoreChange = false;

		public int SelectedIndex
		{
			get { return comboBox1.SelectedIndex; }
			set { comboBox1.SelectedIndex = value; }
		}

		public ComboBox()
			: this("ComboBox", new string[]{"Item1", "Item2"}, null, null)
		{
			comboBox1.SelectedIndex = 0;
		}

		public ComboBox(string title, string[] items, Func<int> get, Action<int> set)
		{
			InitializeComponent();

			labelTitle.Text = title;
			_get = get;
			_set = set;

			comboBox1.Items.AddRange(items);
			UpdateData();
			comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
		}

		public void Add(object item)
		{
			comboBox1.Items.Add(item);
		}

		public override void UpdateData()
		{
			if (_get == null) return;
			var index = _get();
			if (index != comboBox1.SelectedIndex)
			{
				_ignoreChange = true;
				comboBox1.SelectedIndex = index;
				_ignoreChange = false;
			}
		}

		void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_ignoreChange || _set == null) return;

			_set(comboBox1.SelectedIndex);
		}
	}
}
