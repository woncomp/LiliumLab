using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lilium.Controls
{
	public class Control : UserControl
	{
		public virtual void UpdateData() { }
	}
	
}

namespace Lilium
{
	public abstract class ControlAttribute : System.Attribute
	{
		public abstract bool IsValid(MemberInfo memberInfo);
		public abstract Lilium.Controls.Control CreateControl(MemberInfo memberInfo, object obj);
	}
}
