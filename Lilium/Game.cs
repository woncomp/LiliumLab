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
		public Lilium.Controls.RenderControl RenderControl;

		private Grid grid;
		private List<IDisposable> _disposeList = new List<IDisposable>();
		
		public virtual string ResourceFolder { get { return ""; } }

		[Obsolete]
		public void LoadTexture(string path, out ShaderResourceView tex)
		{
			if (!path.StartsWith("..")) path = ResourceFolder + path;
			tex = ShaderResourceView.FromFile(Device, path);
			AutoDispose(tex);
		}

		[Obsolete]
		public void LoadMesh(string path, out Mesh mesh)
		{
			if (!path.StartsWith("..")) path = ResourceFolder + path;
			if (path.EndsWith("txt", StringComparison.OrdinalIgnoreCase))
				mesh = Mesh.CreateFromTXT(path);
			else
				mesh = Mesh.CreateFromFile(path);
			AutoDispose(mesh);
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
			Info_Init();
			Preview_Init();
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

			DebugLines.Instance = new DebugLines(Device);
			DebugLines.Instance.Init();
			AutoDispose(DebugLines.Instance);

			AutoLoad_Scan();
			OnStart();
			Info_Scan();
		}

		public void LoopUpdate()
		{
			Light.MainLight.Update();
			Time_Update(true);

			Camera.MainCamera.Begin();
			UpdatePerFrameBuffer();
			OnUpdate();//MTBTODO
			MainScene.Draw();

			grid.Draw();
			Preview_Render();
			Info_UpdateData();

			DebugLines.Instance.Update(previewSuppressDebugLines);
		}

		public void Dispose()
		{
			Utilities.Dispose(ref MainScene);
			Preview_Dispose();
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
			public Vector4 ambientColor;
			public Vector4 diffuseColor;
		}

		struct LiliumPerObjectData
		{
			public Matrix matWorld;
		}

		Buffer perFrameBuffer;
		Buffer perObjectBuffer;

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

			var data = new LiliumPerFrameData();
			data.matView = Camera.MainCamera.ViewMatrix;
			data.matProjection = Camera.MainCamera.ProjectionMatrix;
			data.lightDir = light.LightDir4;
			data.ambientColor = light.AmbientColor;
			data.diffuseColor = light.DiffuseColor;
			DeviceContext.UpdateSubresource(ref data, perFrameBuffer);
			DeviceContext.VertexShader.SetConstantBuffer(1, perFrameBuffer);
			DeviceContext.PixelShader.SetConstantBuffer(1, perFrameBuffer);
		}

		public void UpdatePerObjectBuffer(Matrix objectTransform)
		{
			var data = new LiliumPerObjectData();
			data.matWorld = objectTransform;
			DeviceContext.UpdateSubresource(ref data, perObjectBuffer);
			DeviceContext.VertexShader.SetConstantBuffer(2, perObjectBuffer);
			DeviceContext.PixelShader.SetConstantBuffer(2, perObjectBuffer);
		}
#endregion
	}
}
