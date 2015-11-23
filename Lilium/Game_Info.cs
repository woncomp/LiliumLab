using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Reflection;
using Lilium.Controls;
using Control = Lilium.Controls.Control;

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
			var preview = selectedObject as IPreviewable;
			if (preview != null) preview.PreviewDraw();
		}

		void Info_SetSelcectedObject(ISelectable obj)
		{
			var preview = selectedObject as IPreviewable;
			if (preview != null) preview.PreviewDeactive();
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
				{
					var str = string.Format("{0}({1})", selectedObject.NameInObjectList, selectedObject.GetType().Name);
					var label = new SelectionTargetHeader(str);
					AddControl(label);
					selectedObjectControls.Add(label);
				}
				var controls = selectedObject.Controls;
				if (controls != null)
				{
					for (int i = 0; i < controls.Length; ++i)
					{
						AddControl(controls[i]);
						selectedObjectControls.Add(controls[i]);
					}
				}
				else
				{
					var c = new Label("Warning", () => "Selected object don't have any control.(" + obj.ToString() + ")");
					AddControl(c);
					selectedObjectControls.Add(c);
				}
			}
			preview = selectedObject as IPreviewable;
			if (preview != null) preview.PreviewActive();
		}

		public void AddObject(ISelectable obj)
		{
			if (obj == null) return;
			MainForm.Instance.AddObject(obj);
		}

		public void AddResource(SharpDX.Direct3D11.ShaderResourceView texture)
		{
			if (texture == null) return;
			MainForm.Instance.AddObject(new TexturePreview(texture));
		}

		public void AddControl(Control control)
		{
			infoControls.Add(control);
			MainForm.Instance.InfoContainer.Controls.Add(control);
		}

		public void RemoveControl(Control control)
		{
			MainForm.Instance.InfoContainer.Controls.Remove(control);
			infoControls.Remove(control);
		}

		public void InsertControl(Control control, Control before, bool belongsToSelectedObject = false)
		{
			int index = MainForm.Instance.InfoContainer.Controls.IndexOf(before);
			MainForm.Instance.InfoContainer.Controls.Add(control);
			if (index >= 0) MainForm.Instance.InfoContainer.Controls.SetChildIndex(control, index);
			infoControls.Add(control);
			if (belongsToSelectedObject) selectedObjectControls.Add(control);
		}
	}

	public interface ISelectable
	{
		Control[] Controls { get; }
		string NameInObjectList { get; }
	}

	public interface IPreviewable
	{
		void PreviewDraw();
		void PreviewActive();
		void PreviewDeactive();
	}

	public class CustomSelectedTypeNameAttribute : Attribute
	{
		public string TypeName { get; private set; }
		public CustomSelectedTypeNameAttribute(string name) { TypeName = name; }
	}
}
