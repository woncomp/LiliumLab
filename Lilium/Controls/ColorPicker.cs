using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using System.Threading;
using OpenPainter.ColorPicker;
using Color = System.Drawing.Color;

namespace Lilium.Controls
{
	public partial class ColorPicker : Control
	{
		Vector4 colorValue;
		Vector4 oldColorValue;
		int alpha;

		Func<Vector4> _get;
		Action<Vector4> _set;

		frmColorPicker colorPicker;
		System.Drawing.Point colorPickerLocation;
		Color colorPickerColor;

		public ColorPicker(string title, Func<Vector4> get, Action<Vector4> set)
		{
			InitializeComponent();
			labelTitle.Text = title;
			_get = get;
			_set = set;
			colorValue.X = 999;
		}
		
		public override void UpdateData()
		{
			if (_get == null) return;
			if (colorPicker != null && !colorPicker.Visible)
			{
				if (_set != null)
				{
					if (colorPicker.DialogResult == DialogResult.OK)
					{
						colorValue = ToVector4(colorPicker.PrimaryColor);
					}
					else
					{
						colorValue = oldColorValue;
					}
					pictureBox1.BackColor = ToColor(colorValue);
					_set(colorValue);
				}
				colorPicker.Dispose();
				colorPicker = null;
			}
			else
			{
				var newColor = _get();
				if (newColor != colorValue)
				{
					colorValue = newColor;
					alpha = MathUtil.Clamp((int)(newColor.W * 255), 0, 255);
					this.trackBar1.Value = alpha;
					this.pictureBox1.BackColor = ToColor(newColor);
				}
				if(alpha != this.trackBar1.Value)
				{
					alpha = this.trackBar1.Value;
					colorValue.W = (float)(alpha / 255.0);
				}
				if(colorPicker != null)
				{
					if(colorPickerColor != colorPicker.PrimaryColor)
					{
						colorPickerColor = colorPicker.PrimaryColor;
						pictureBox1.BackColor = colorPicker.PrimaryColor;
						colorValue = ToVector4(colorPickerColor);
					}
					colorPickerLocation = colorPicker.DesktopLocation;
				}
				if (_set != null) _set(colorValue);
			}
		}

		private void pictureBox1_Click(object sender, EventArgs e)
		{
			if (colorPicker == null)
			{
				if(colorPickerLocation.IsEmpty)
				{
					colorPickerLocation = pictureBox1.PointToScreen(new System.Drawing.Point(-200, -200));
				}
				oldColorValue = colorValue;
				colorPicker = new frmColorPicker(pictureBox1.BackColor);
				colorPicker.Show();
				colorPicker.DesktopLocation = colorPickerLocation;
			}
			else
			{
				colorPicker.BringToFront();
			}
		}

		Vector4 ToVector4(Color color)
		{
			return new SharpDX.Color(color.R, color.G, color.B, (byte)alpha).ToVector4();
		}

		Color ToColor(Vector4 v)
		{
			var c = new SharpDX.Color(v);
			return System.Drawing.Color.FromArgb(255, c.R, c.G, c.B);
		}
	}
}
