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
	public partial class PropertiesForm : Form
	{
		public static void Show(object target, Action onChanged)
		{
			if (instance == null) instance = new PropertiesForm();
			instance.onChanged = null;
			instance.propertyGrid1.SelectedObject = target;
			instance.onChanged = onChanged;
			instance.Show();
		}
		private static PropertiesForm instance;

		Action onChanged;

		public PropertiesForm()
		{
			InitializeComponent();
		}

		private void propertyGrid1_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e)
		{
			if (onChanged != null) onChanged();
		}

		private void PropertiesForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			instance = null;
		}
	}
}
