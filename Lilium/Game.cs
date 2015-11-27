using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Lilium
{
	public abstract partial class Game : IDisposable
	{
		public static Game Instance { get; private set; }

		public static void Run<GameT>() where GameT : Game, new()
		{
			if (!Device.IsSupportedFeatureLevel(FeatureLevel.Level_11_0))
			{
				System.Windows.Forms.MessageBox.Show("DirectX11 Not Supported");
				return;
			}

			Game.Instance = new GameT();
			var form = new MainForm(Game.Instance);

			System.Windows.Forms.Application.Run(form);
		}

		public System.Drawing.Size RenderViewSize
		{
			get { return RenderControl.ClientSize; }
		}
		public IntPtr ControlHandle
		{
			get { return RenderControl.Handle; }
		}

		public Device Device { get { return this.RenderControl.Device; } }
		public DeviceContext DeviceContext { get { return RenderControl.DeviceContext; } }

		public RenderTargetView DefaultRenderTargetView { get { return RenderControl.DefaultRenderTargetView; } }
		public DepthStencilView DefaultDepthStencilView { get { return RenderControl.DefaultDepthStencilView; } }

		public Input Input { get { return RenderControl.Input; } }
		public ResourceManager ResourceManager { get; private set; }

		public Scene MainScene;
		public StencilShadowRenderer StencilShadowRenderer;
		public Lilium.Controls.RenderControl RenderControl;

		private Grid grid;
		private Skydome skydome;
		private List<IDisposable> _disposeList = new List<IDisposable>();
		
		public void AutoDispose(IDisposable disposeable)
		{
			if (!_disposeList.Contains(disposeable))
				_disposeList.Add(disposeable);
		}

		protected abstract void OnStart();
		protected abstract void OnUpdate();

		public void Init()
		{
			Render_Init();
			this.ResourceManager = new ResourceManager(this);
			this.ResourceManager.SearchPaths.Add("../../../InternalAssets/");
			this.ResourceManager.SearchPaths.Add("./");
			this.ResourceManager.Init();
			Info_Init();
			Time_Init();
			InitShaderBuffers();

			MainScene = new Scene();
			MainScene.Name = "Main Scene";
			AddObject(MainScene);

			Camera.MainCamera = new Camera();
			Camera.MainCamera.Name = "Main Camera";
			AddObject(Camera.MainCamera);
			Light.MainLight = new Light();
			Light.MainLight.Name = "Main Light";
			AddObject(Light.MainLight);
			grid = new Grid(Device);
			grid.Init();
			AutoDispose(grid);
			skydome = new Skydome(Device);
			AutoDispose(skydome);

			StencilShadowRenderer = new Lilium.StencilShadowRenderer(this);
			AutoDispose(StencilShadowRenderer);

			DebugLines.Instance = new DebugLines(Device);
			DebugLines.Instance.Init();
			AutoDispose(DebugLines.Instance);

			OnStart();
			Info_Scan();
		}

		public void LoopUpdate()
		{
			Light.MainLight.Update();
			Time_Update(true);

			Camera.MainCamera.Begin();
			UpdatePerFrameBuffer();
			skydome.Draw();
			MainScene.Draw();
			OnUpdate();

			grid.Draw();
			Info_UpdateData();

			DebugLines.Instance.Update(Config.PreviewSuppressDebugLines);
		}

		public void Dispose()
		{
			SelectedObject = null;
			Utilities.Dispose(ref MainScene);
			for (int i = _disposeList.Count; i > 0; )
			{
				--i;
				_disposeList[i].Dispose();
			}
			ResourceManager.Dispose();
		}

#region UpdateShaderBuffer
		struct LiliumPerFrameData
		{
			public Matrix matView;
			public Matrix matProjection;
			public Vector4 lightDir;
			public Vector4 eyePos;
			public Vector4 ambientColor;
			public Vector4 diffuseColor;
			public float renderTargetWidth;
			public float renderTargetHeight;
			public float cameraNearPlane;
			public float cameraFarPlane;
		}

		struct LiliumPerObjectData
		{
			public Matrix matWorld;
			public Matrix matWorldInverseTranspose;
			public Matrix matWorldViewInverseTranspose;
		}

		Buffer perFrameBuffer;
		Buffer perObjectBuffer;

		LiliumPerFrameData perFrameData;
		LiliumPerObjectData perObjectData;

		void InitShaderBuffers()
		{
			perFrameBuffer = new Buffer(Device, Utilities.SizeOf<LiliumPerFrameData>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
			AutoDispose(perFrameBuffer);

			perObjectBuffer = new Buffer(Device, Utilities.SizeOf<LiliumPerObjectData>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
			AutoDispose(perObjectBuffer);
		}

		public void UpdatePerFrameBuffer()
		{
			var light = Light.MainLight;

			perFrameData.matView = Camera.ActiveCamera.ViewMatrix;
			perFrameData.matProjection = Camera.ActiveCamera.ProjectionMatrix;
			perFrameData.lightDir = light.LightDir4;
			perFrameData.eyePos = new Vector4(Camera.ActiveCamera.Position, 1);
			perFrameData.ambientColor = light.AmbientColor;
			perFrameData.diffuseColor = light.DiffuseColor;
			perFrameData.renderTargetWidth = DefaultViewport.Width;
			perFrameData.renderTargetHeight = DefaultViewport.Height;
			perFrameData.cameraNearPlane = Camera.ActiveCamera.NearPlane;
			perFrameData.cameraFarPlane = Camera.ActiveCamera.FarPlane;
			DeviceContext.UpdateSubresource(ref perFrameData, perFrameBuffer);
			DeviceContext.VertexShader.SetConstantBuffer(1, perFrameBuffer);
			DeviceContext.PixelShader.SetConstantBuffer(1, perFrameBuffer);
		}

		public void UpdatePerObjectBuffer(Matrix objectTransform)
		{
			perObjectData.matWorld = objectTransform;
			perObjectData.matWorldInverseTranspose = Matrix.Invert(Matrix.Transpose(objectTransform));
			perObjectData.matWorldViewInverseTranspose = Matrix.Invert(Matrix.Transpose(perObjectData.matWorld * perFrameData.matView));
			DeviceContext.UpdateSubresource(ref perObjectData, perObjectBuffer);
			DeviceContext.VertexShader.SetConstantBuffer(2, perObjectBuffer);
			DeviceContext.PixelShader.SetConstantBuffer(2, perObjectBuffer);
		}
#endregion
	}
}
