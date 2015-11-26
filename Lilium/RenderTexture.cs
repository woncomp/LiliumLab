using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Lilium
{
	public class RenderTexture :IDisposable, ISelectable, IPreviewable
	{
		Texture2D tex;
		ShaderResourceView shaderResourceView;
		RenderTargetView renderTargetView;

		Texture2D depthStencilTexture;
		DepthStencilView depthStencilView;

		Viewport vp;

		string debugName;
		Game game;

		public RenderTargetView RenderTargetView { get { return renderTargetView; } }
		public ShaderResourceView ShaderResourceView { get { return shaderResourceView; } }
		public DepthStencilView DepthStencilView { get { return depthStencilView; } }
		public Viewport Viewport { get { return vp; } }
		public string DebugName { get { return debugName; } }

		public Color4 ClearColor = Color.Black;

		private TexturePreview texturePreview;

		public RenderTexture(Game game, string debugName = null)
			: this(game, game.RenderControl.ClientRectangle.Width, game.RenderControl.ClientRectangle.Height, debugName)
		{ }

		public RenderTexture(Game game, int width, int height, string debugName = null)
		{
			this.game = game;

			this.debugName = debugName ?? "RenderTexture " + Debug.NextObjectId;
			var format = Format.R32G32B32A32_Float;
			{
				var desc = new Texture2DDescription();
				desc.Format = format;
				desc.Width = width;
				desc.Height = height;
				desc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;
				desc.Usage = ResourceUsage.Default;
				desc.ArraySize = 1;
				desc.CpuAccessFlags = CpuAccessFlags.None;
				desc.MipLevels = 1;
				desc.SampleDescription.Count = 1;
				desc.SampleDescription.Quality = 0;
				desc.OptionFlags = ResourceOptionFlags.None;

				tex = new Texture2D(game.Device, desc);
				tex.DebugName = DebugName;
			}
			{
				var desc = new RenderTargetViewDescription();
				desc.Format = format;
				desc.Dimension = RenderTargetViewDimension.Texture2D;
				desc.Texture2D.MipSlice = 0;
				renderTargetView = new RenderTargetView(game.Device, tex, desc);
				renderTargetView.DebugName = DebugName;
			}
			{
				shaderResourceView = new ShaderResourceView(game.Device, tex);
				shaderResourceView.DebugName = DebugName;
			}
			{
				var desc = new Texture2DDescription();
				desc.Format = Format.D24_UNorm_S8_UInt;
				desc.ArraySize = 1;
				desc.MipLevels = 1;
				desc.Width = width;
				desc.Height = height;
				desc.SampleDescription.Count = 1;
				desc.SampleDescription.Quality = 0;
				desc.Usage = ResourceUsage.Default;
				desc.BindFlags = BindFlags.DepthStencil;
				desc.CpuAccessFlags = CpuAccessFlags.None;
				desc.OptionFlags = ResourceOptionFlags.None;
				depthStencilTexture = new Texture2D(game.Device, desc);
				depthStencilTexture.DebugName = DebugName + "(DS)";
			}
			{
				depthStencilView = new DepthStencilView(game.Device, depthStencilTexture);
				depthStencilView.DebugName = DebugName;
			}
			texturePreview = new TexturePreview(tex);
			vp = new SharpDX.Viewport(0, 0, width, height);

			game.AddObject(this);
		}

		public void Begin()
		{
			game.DeviceContext.OutputMerger.SetRenderTargets(depthStencilView, renderTargetView);
			game.DeviceContext.Rasterizer.SetViewport(vp);
			game.DeviceContext.ClearRenderTargetView(renderTargetView, ClearColor);
			game.DeviceContext.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
		}

		public void End()
		{
			game.DeviceContext.OutputMerger.SetRenderTargets(game.DefaultDepthStencilView, game.DefaultRenderTargetView);
			game.DeviceContext.Rasterizer.SetViewport(game.DefaultViewport);
		}

		public void Dispose()
		{
			texturePreview.PreviewDeactive();
			Utilities.Dispose(ref depthStencilView);
			Utilities.Dispose(ref depthStencilTexture);
			Utilities.Dispose(ref shaderResourceView);
			Utilities.Dispose(ref renderTargetView);
			Utilities.Dispose(ref tex);
		}

		public Controls.Control[] Controls
		{
			get { return texturePreview.Controls; }
		}

		public string NameInObjectList
		{
			get { return debugName; }
		}

		public void PreviewDraw()
		{
			texturePreview.PreviewDraw();
		}

		public void PreviewActive()
		{
			texturePreview.PreviewActive();
		}

		public void PreviewDeactive()
		{
			texturePreview.PreviewDeactive();
		}
	}
}
