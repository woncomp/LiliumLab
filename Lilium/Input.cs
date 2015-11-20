using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MouseButtons = System.Windows.Forms.MouseButtons;

namespace Lilium
{
	public class Input
	{
		const int KEYBOARD_INDEX_START = 0;
		const int KEYBOARD_INDEX_END = 500;
		const int MOUSE_INDEX_START = 500;
		const int MOUSE_INDEX_END = 512;
		const int MAX_INPUT = 512;

		List<int> inputDownEvents = new List<int>(10);
		List<int> inputUpEvents = new List<int>(10);
		bool[] inputState = new bool[MAX_INPUT];

		private float mouseWheelDelta = 0;
		private System.Drawing.Point mouseLoc;

		private Dictionary<MouseButtons, int> mouseDic;

		public bool Shift { get; private set; }
		public bool Control { get; private set; }
		public bool Alt { get; private set; }

		public System.Drawing.Point MouseLocation { get; private set; }
		public System.Drawing.Size MouseLocationDelta { get; private set; }

		public float MouseWheelValue { get; private set; }
		public float MouseWheelDelta { get; private set; }

		public Input()
		{
			mouseDic = new Dictionary<MouseButtons, int>();
			mouseDic[MouseButtons.None] = 0;
			mouseDic[MouseButtons.Left] = 1;
			mouseDic[MouseButtons.Right] = 2;
			mouseDic[MouseButtons.Middle] = 3;
			mouseDic[MouseButtons.XButton1] = 4;
			mouseDic[MouseButtons.XButton2] = 5;
		}

		public bool GetKey(System.Windows.Forms.Keys key)
		{
			return GetInput((int)key, KEYBOARD_INDEX_START, KEYBOARD_INDEX_END);
		}

		public bool GetKeyDown(System.Windows.Forms.Keys key)
		{
			return GetInputDown((int)key, KEYBOARD_INDEX_START, KEYBOARD_INDEX_END);
		}

		public bool GetKeyUp(System.Windows.Forms.Keys key)
		{
			return GetInputUp((int)key, KEYBOARD_INDEX_START, KEYBOARD_INDEX_END);
		}

		public bool GetMouseButton(MouseButtons b)
		{
			return GetInput(mouseDic[b], MOUSE_INDEX_START, MOUSE_INDEX_END);
		}

		public bool GetMouseButtonDown(MouseButtons b)
		{
			return GetInputDown(mouseDic[b], MOUSE_INDEX_START, MOUSE_INDEX_END);
		}

		public bool GetMouseButtonUp(MouseButtons b)
		{
			return GetInputUp(mouseDic[b], MOUSE_INDEX_START, MOUSE_INDEX_END);
		}

		public void Hook(System.Windows.Forms.Control control)
		{
			control.MouseDown += control_MouseDown;
			control.MouseUp += control_MouseUp;
			control.MouseMove += control_MouseMove;
			control.MouseWheel += control_MouseWheel;

			control.KeyDown += control_KeyDown;
			control.KeyUp += control_KeyUp;
		}

		public void Update()
		{
			inputDownEvents.Clear();
			inputUpEvents.Clear();

			MouseLocationDelta = new System.Drawing.Size(mouseLoc.X - MouseLocation.X, mouseLoc.Y - MouseLocation.Y);
			MouseLocation = mouseLoc;
			MouseWheelValue += mouseWheelDelta;
			MouseWheelDelta = mouseWheelDelta;
			mouseWheelDelta = 0;
		}

		void RecieveInputDown(int code, int start, int end)
		{
			int index = code + start;
			if(index >= start && index < end)
			{
				inputDownEvents.Add(index);
				inputState[index] = true;
			}
		}

		void RecieveInputUp(int code, int start, int end)
		{
			int index = code + start;
			if(index >= start && index < end)
			{
				inputDownEvents.Add(index);
				inputState[index] = false;
			}
		}

		bool GetInput(int code, int start, int end)
		{
			int index = code + start;
			if(index >= start && index < end)
			{
				return inputState[index];
			}
			return false;
		}

		bool GetInputDown(int code, int start, int end)
		{
			int index = code + start;
			if(index >= start && index < end)
			{
				return inputDownEvents.Contains(index);
			}
			return false;
		}

		bool GetInputUp(int code, int start, int end)
		{
			int index = code + start;
			if(index >= start && index < end)
			{
				return inputUpEvents.Contains(index);
			}
			return false;
		}

		void control_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			RecieveInputDown(mouseDic[e.Button], MOUSE_INDEX_START, MOUSE_INDEX_END);
		}

		void control_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			RecieveInputUp(mouseDic[e.Button], MOUSE_INDEX_START, MOUSE_INDEX_END);
		}

		void control_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			mouseWheelDelta += e.Delta;
		}

		void control_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			mouseLoc = e.Location;
		}

		void control_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			Shift = e.Shift;
			Control = e.Control;
			Alt = e.Alt;
			RecieveInputDown((int)e.KeyCode, KEYBOARD_INDEX_START, KEYBOARD_INDEX_END);
		}

		void control_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			Shift = e.Shift;
			Control = e.Control;
			Alt = e.Alt;
			RecieveInputUp((int)e.KeyCode, KEYBOARD_INDEX_START, KEYBOARD_INDEX_END);
		}
	}
}
