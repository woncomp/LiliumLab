using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Lilium.Controls;
using System.Reflection;

namespace Lilium
{
	public partial class Game
	{
		public ISelectable SelectedObject
		{
			get { return selectedObject; }
			set { Info_SetSelcectedObject(value); }
		}

		private List<Control> infoControls = new List<Control>();
		private List<Control> selectedObjectControls = new List<Control>();

		private ISelectable selectedObject;

		void Info_Init()
		{
			MainForm.Instance.SelectedObjectChanged += Info_SetSelcectedObject;

			if(true)
			{
				var toggle = new Toggle("Draw TBN", () => Config.DrawTBN, val => Config.DrawTBN = val);
				AddControl(toggle);
				var slider = new Slider("Draw TBN Offset", 0, 2, () => Config.TBNOffset, val => Config.TBNOffset = val);
				AddControl(slider);
			}
			if(true)
			{
				var toggle = new Toggle("Draw Wireframe", () => Config.DrawWireframe, val => Config.DrawWireframe = val);
				AddControl(toggle);
			}
		}

		void Info_Scan()
		{
			var obj = this;
			var gameType = obj.GetType();
			var members = gameType.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (var member in members)
			{
				var attr = member.GetCustomAttribute(typeof(ControlAttribute));
				var controlAttribute = attr as ControlAttribute;
				if (controlAttribute != null && controlAttribute.IsValid(member))
				{
					var control = controlAttribute.CreateControl(member, obj);
					AddControl(control);
				}
			}
		}

		void Info_UpdateData()
		{
			foreach (var info in infoControls)
			{
				info.UpdateData();
			}
		}

		void Info_SetSelcectedObject(ISelectable obj)
		{
			if (selectedObjectControls.Count > 0)
			{
				for (int i = selectedObjectControls.Count - 1; i >= 0; --i) 
				{
					RemoveControl(selectedObjectControls[i]);
				}
				selectedObjectControls.Clear();
			}
			selectedObject = obj;
			if (selectedObject != null)
			{
				var controls = selectedObject.Controls;
				for (int i = 0; i < controls.Length; ++i)
				{
					AddControl(controls[i]);
					selectedObjectControls.Add(controls[i]);
				}
			}
		}

		public void AddObject(ISelectable obj)
		{
			if (obj == null) return;
			MainForm.Instance.AddObject(obj);
		}

		public void AddControl(Lilium.Controls.Control control)
		{
			infoControls.Add(control);
			MainForm.Instance.InfoContainer.Controls.Add(control);
		}

		public void RemoveControl(Lilium.Controls.Control control)
		{
			MainForm.Instance.InfoContainer.Controls.Remove(control);
			infoControls.Remove(control);
		}
	}

	public interface ISelectable
	{
		Control[] Controls { get; }
	}
}
