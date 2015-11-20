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
	public partial class Slider : Control
	{
		private float _min;
		private float _max;
		private Func<float> _get;
		private Action<float> _set;

		private bool _ignoreChange = false;

		public Slider(string title, float min, float max, Func<float> get, Action<float> set)
		{
			InitializeComponent();

			this._min = min;
			this._max = max;
			this._get = get;
			this._set = set;

			this.labelTitle.Text = title;
			this.labelValue.Text = get().ToString();
			this.trackBar1.Minimum = 0;
			this.trackBar1.Maximum = 1000;
			this.trackBar1.ValueChanged += trackBar1_ValueChanged;
		}

		public override void UpdateData()
		{
			var v = _get();
			_ignoreChange = true;
			this.trackBar1.Value = (int)SharpDX.MathUtil.Lerp(this.trackBar1.Minimum, this.trackBar1.Maximum, (v - _min) / (_max - _min));
			_ignoreChange = false;

			this.labelValue.Text = v.ToString();
		}

		void trackBar1_ValueChanged(object sender, EventArgs e)
		{
			if (_ignoreChange) return;

			var v = SharpDX.MathUtil.Lerp(_min, _max, (trackBar1.Value - trackBar1.Minimum) / (float)(trackBar1.Maximum - trackBar1.Minimum));
			_set(v);
			this.labelValue.Text = v.ToString();
		}
	}
	
}

namespace Lilium
{
	public class SliderAttribute : ControlAttribute
	{
		public float MinValue { get; private set; }
		public float MaxValue { get; private set; }

		public SliderAttribute(float min, float max)
		{
			MinValue = min;
			MaxValue = max;
		}

		public override bool IsValid(MemberInfo memberInfo)
		{
			if( memberInfo is FieldInfo)
			{
				var fieldInfo = memberInfo as FieldInfo;
				return fieldInfo.FieldType == typeof(float);
			}
			return false;
		}

		public override Lilium.Controls.Control CreateControl(MemberInfo memberInfo, object obj)
		{
			var fieldInfo = memberInfo as FieldInfo;
			return new Lilium.Controls.Slider(fieldInfo.Name, MinValue, MaxValue, () => (float)fieldInfo.GetValue(obj), v => fieldInfo.SetValue(obj, v));
		}
	}
}
