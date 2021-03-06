﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Windows;

namespace Lilium
{
	public class Camera : ISelectable
	{
		public static Camera MainCamera;
		public static Camera ActiveCamera;

		public string Name;

		public Vector3 Position;
		public Vector3 FocusPoint;

		public float FovDegrees = 60;
		public float NearPlane = 1f;
		public float FarPlane = 1000f;

		public Matrix ViewMatrix { get; private set; }
		public Matrix ProjectionMatrix { get; private set; }

		public float CameraDistance = 30;
		public float CameraYaw = 200;
		public float CameraPitch = -30;

		//public bool ClearColorBuffer = true;
		//public Color ClearColor = new Color(0.95f);
		public Vector4 SkyBottomColor = new Vector4(0.89f, 0.89f, 0.91f, 1);
		public Vector4 SkyTopColor = new Vector4(1, 1, 1, 1);
		public DepthStencilClearFlags DepthStencilClearFlags = DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil;
		public float DepthClearValue = 1;
		public byte StencilClearValue = 0;
		
		public Camera()
		{
			Name = "Camera " + Debug.NextObjectId;
			CreateControls();
		}

		public void Begin()
		{
			var game = Game.Instance;

			ActiveCamera = this;

			if (this == MainCamera) HandleInput();

			float yawR = MathUtil.DegreesToRadians(CameraYaw);
			float pitchR = MathUtil.DegreesToRadians(CameraPitch);
			var mat = Matrix.RotationYawPitchRoll(yawR, pitchR, 0);
			Position = FocusPoint + Vector3.TransformCoordinate(Vector3.ForwardLH, mat) * CameraDistance;

			ViewMatrix = Matrix.LookAtLH(Position, FocusPoint, Vector3.Up);

			var renderViewSize = Game.Instance.RenderViewSize;
			var ratio = renderViewSize.Width / (float)renderViewSize.Height;
			ProjectionMatrix = Matrix.PerspectiveFovLH(MathUtil.DegreesToRadians(FovDegrees), ratio, NearPlane, FarPlane);

			//if(ClearColorBuffer)
			//	game.DeviceContext.ClearRenderTargetView(game.DefaultRenderTargetView, ClearColor);
			if(0 != (int)DepthStencilClearFlags)
				game.DeviceContext.ClearDepthStencilView(game.DefaultDepthStencilView, DepthStencilClearFlags, DepthClearValue, StencilClearValue);
		}

		void HandleInput()
		{
			var game = Game.Instance;
			if (game.Input.GetMouseButton(System.Windows.Forms.MouseButtons.Left))
			{
				var mouseLocDelta = game.Input.MouseLocationDelta;
				var dx = mouseLocDelta.Width;
				var dy = mouseLocDelta.Height;
				CameraYaw += dx * 0.21f;
				CameraPitch -= dy * 0.21f;
				if (CameraPitch > 89) CameraPitch = 89;
				if (CameraPitch < -89) CameraPitch = -89;
			}
			if(game.Input.GetMouseButton(System.Windows.Forms.MouseButtons.Middle))
			{
				var mouseLocDelta = game.Input.MouseLocationDelta;
				FocusPoint.Y += mouseLocDelta.Height * 0.05f;
			}
			CameraDistance -= game.Input.MouseWheelDelta * 0.03f;
			if (CameraDistance < 1) CameraDistance = 1;
		}

		#region Selectable
		private Controls.Control[] controls;

		void CreateControls()
		{
			var cameraInfo = new Lilium.Controls.Label("Camera Pos", () => Position.ToString("0.000"));
			var button = new Lilium.Controls.Button("Front", () =>
			{
				CameraYaw = 0;
				CameraPitch = 0;
			});
			var sliderx = new Lilium.Controls.Slider("Focus Point X", -10, 10, () => FocusPoint.X, val => FocusPoint.X = val);
			var slidery = new Lilium.Controls.Slider("Focus Point Y", -10, 10, () => FocusPoint.Y, val => FocusPoint.Y = val);
			var sliderz = new Lilium.Controls.Slider("Focus Point Z", -10, 10, () => FocusPoint.Z, val => FocusPoint.Z = val);

			//var clearColor = new Lilium.Controls.ColorPicker("Clear Color", () => ClearColor.ToVector4(), val => ClearColor = new Color(val));
			var sky1 = new Lilium.Controls.ColorPicker("Sky Bottom", () => SkyBottomColor, val => SkyBottomColor = val);
			var sky2 = new Lilium.Controls.ColorPicker("Sky Top", () => SkyTopColor, val => SkyTopColor = val);

			controls = new Controls.Control[] { cameraInfo, button, sliderx, slidery, sliderz, sky1, sky2 };
		}

		public Controls.Control[] Controls { get { return controls; } }
		public string NameInObjectList { get { return Name; } }

		#endregion
	}
}
