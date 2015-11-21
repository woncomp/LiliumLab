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
	public partial class Label : Control
	{
		private Func<string> _get;

		public Label(string title, Func<string> get)
		{
			InitializeComponent();

			_get = get;

			labelTitle.Text = title;
		}
		
		public override void UpdateData()
		{
			if (_get != null)
			{
				labelValue.Text = _get();
			}
		}
	}
}

namespace Lilium
{
	public class LabelAttribute : ControlAttribute
	{

		public override bool IsValid(MemberInfo memberInfo)
		{
			return memberInfo is FieldInfo;
		}
		public override Lilium.Controls.Control CreateControl(MemberInfo memberInfo, object obj)
		{
			var fieldInfo = memberInfo as FieldInfo;
			Func<string> func;
			if(fieldInfo.FieldType == typeof(SharpDX.Vector3))
			{
				func = () =>
					{
						var vec = (SharpDX.Vector3)fieldInfo.GetValue(obj);
						return vec.ToString("0.###");
					};
			}
			else if(fieldInfo.FieldType == typeof(SharpDX.Vector4))
			{
				func = () =>
					{
						var vec = (SharpDX.Vector4)fieldInfo.GetValue(obj);
						return vec.ToString("0.###");
					};
			}
			else
			{
				func = ()=> fieldInfo.GetValue(obj).ToString();
			}
			return new Lilium.Controls.Label(memberInfo.Name, func);
		}
	}
}
