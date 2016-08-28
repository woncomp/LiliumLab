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
using System.Windows.Forms;

namespace Lilium
{
	public abstract partial class Game : IDisposable
	{
		public static Game Instance { get; private set; }

		public Input Input { get; private set; }
		public ResourceManager ResourceManager { get; private set; }
		public UISurface UI { get { return mUISurface;  } }

		public Scene MainScene;
		public SkyBox SkyBox;
		public StencilShadowRenderer StencilShadowRenderer;

		private LineRenderer debugLine;
		private Grid grid;
		private Skydome skydome;
		private UISurface mUISurface;
		private List<IDisposable> _disposeList = new List<IDisposable>();

		public Game()
		{
			Instance = this;
		}
		
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
			mUISurface = new UISurface(Device);
			AutoDispose(mUISurface);

			StencilShadowRenderer = new Lilium.StencilShadowRenderer(this);
			AutoDispose(StencilShadowRenderer);

			Debug.Init(this);
			debugLine = Debug.EDITOR_GetDebugLineRenderer();

			OnStart();
			Info_Scan();
		}

		public void LoopUpdate()
		{
			Input.Update();

			Light.MainLight.Update();
			Time_Update(true);

			foreach (var entity in MainScene.Entities)
			{
				if (entity.Cubemap != null) entity.Cubemap.CaptureSceneAtPosition(entity.Position);
			}
			Camera.MainCamera.Begin();
			UpdatePerFrameBuffer();
			if (SkyBox != null) SkyBox.Draw();
			else skydome.Draw();
			MainScene.Draw();
			OnUpdate();

			grid.Draw();
			Info_UpdateData();

			if (!Config.PreviewSuppressDebugLines) debugLine.Draw();
			debugLine.Clear();
			mUISurface.Draw();
		}

		public void Dispose()
		{
			SelectedObject = null;
			Utilities.Dispose(ref SkyBox);
			Utilities.Dispose(ref MainScene);
			Debug.Shutdown();
			for (int i = _disposeList.Count; i > 0; )
			{
				--i;
				_disposeList[i].Dispose();
			}
			ResourceManager.Dispose();
			Dispose_Device();
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

		public RenderCubemap CurrentCubemap { get; set; }

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

			if (CurrentCubemap != null)
			{
				perFrameData.matView = CurrentCubemap.ViewMatrix;
				perFrameData.matProjection = RenderCubemap.PROJECTION_MATRIX;
				perFrameData.eyePos = new Vector4(CurrentCubemap.Position, 1);
				perFrameData.cameraNearPlane = RenderCubemap.NEAR_PLANE;
				perFrameData.cameraFarPlane = RenderCubemap.FAR_PLANE;

				perFrameData.renderTargetWidth = CurrentCubemap.Viewport.Width;
				perFrameData.renderTargetHeight = CurrentCubemap.Viewport.Height;
			}
			else
			{
				perFrameData.matView = Camera.ActiveCamera.ViewMatrix;
				perFrameData.matProjection = Camera.ActiveCamera.ProjectionMatrix;
				perFrameData.eyePos = new Vector4(Camera.ActiveCamera.Position, 1);
				perFrameData.cameraNearPlane = Camera.ActiveCamera.NearPlane;
				perFrameData.cameraFarPlane = Camera.ActiveCamera.FarPlane;

				perFrameData.renderTargetWidth = DefaultViewport.Width;
				perFrameData.renderTargetHeight = DefaultViewport.Height;
			}

			perFrameData.lightDir = light.LightDir4;
			perFrameData.ambientColor = light.AmbientColor;
			perFrameData.diffuseColor = light.DiffuseColor;

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
