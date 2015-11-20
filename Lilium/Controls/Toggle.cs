using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace Lilium.Controls
{
	public partial class Toggle : Control
	{
		private Func<bool> _get;
		private Action<bool> _set;

		private bool _ignoreChange = false;

		public Toggle(string title, Func<bool> get, Action<bool> set)
		{
			InitializeComponent();

			_get = get;
			_set = set;

			labelTitle.Text = title;
			checkBox1.CheckedChanged += checkBox1_CheckedChanged;
		}

		public override void UpdateData()
		{
			_ignoreChange = true;
			checkBox1.Checked = _get();
			_ignoreChange = false;
		}

		void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			if (_ignoreChange) return;

			_set(checkBox1.Checked);
		}
	}
}

namespace Lilium
{
	public class ToggleAttribute : ControlAttribute
	{
		public override bool IsValid(MemberInfo memberInfo)
		{
			if (memberInfo is FieldInfo)
			{
				var fieldInfo = memberInfo as FieldInfo;
				return fieldInfo.FieldType == typeof(bool);
			}
			return false;
		}

		public override Lilium.Controls.Control CreateControl(MemberInfo memberInfo, object obj)
		{
			var fieldInfo = memberInfo as FieldInfo;
			return new Lilium.Controls.Toggle(fieldInfo.Name, () => (bool)fieldInfo.GetValue(obj), v => fieldInfo.SetValue(obj, v));
		}
	}
}
