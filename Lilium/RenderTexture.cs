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
		public RenderTargetView RenderTargetView { get { return renderTargetViews[0]; } }
		public ShaderResourceView ShaderResourceView { get { return shaderResourceViews[0]; } }
		public Color4 ClearColor { get { return clearColors[0]; } set { clearColors[0] = value; } }

		public DepthStencilView DepthStencilView { get { return depthStencilView; } }

		public Viewport Viewport { get { return vp; } }
		public string DebugName { get { return debugName; } }

		Texture2D[] textures;
		ShaderResourceView[] shaderResourceViews;
		RenderTargetView[] renderTargetViews;

		Color4[] clearColors;

		Texture2D depthStencilTexture;
		DepthStencilView depthStencilView;

		Viewport vp;

		string debugName;
		Game game;

		private TexturePreview texturePreview;
		private int previewIndex = 0;

		public RenderTexture(Game game, int mrtCount = 1, string debugName = null)
			: this(game, game.RenderControl.ClientRectangle.Width, game.RenderControl.ClientRectangle.Height, mrtCount, debugName)
		{ }

		public RenderTexture(Game game, int width, int height, int mrtCount = 1, string debugName = null)
		{
			this.game = game;

			if (mrtCount < 1) mrtCount = 1;
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

				textures = new Texture2D[mrtCount];
				for (int i = 0; i < mrtCount; ++i)
				{
					textures[i] = new Texture2D(game.Device, desc);
					textures[i].DebugName = DebugName + "[" + i + "]";
				}
			}
			{
				var desc = new RenderTargetViewDescription();
				desc.Format = format;
				desc.Dimension = RenderTargetViewDimension.Texture2D;
				desc.Texture2D.MipSlice = 0;

				renderTargetViews = new RenderTargetView[mrtCount];
				for (int i = 0; i < mrtCount; ++i)
				{
					renderTargetViews[i] = new RenderTargetView(game.Device, textures[i], desc);
					renderTargetViews[i].DebugName = DebugName + "[" + i + "]";
				}
			}
			{
				shaderResourceViews = new ShaderResourceView[mrtCount];
				for (int i = 0; i < mrtCount; ++i)
				{
					shaderResourceViews[i] = new ShaderResourceView(game.Device, textures[i]);
					shaderResourceViews[i].DebugName = DebugName + "[" + i + "]";
				}
			}
			{
				clearColors = new Color4[mrtCount];
				for (int i = 0; i < mrtCount; ++i)
				{
					clearColors[i] = Color4.Black;
				}
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
			vp = new SharpDX.Viewport(0, 0, width, height);

			game.AddObject(this);
		}

		public int GetRenderTargetCount()
		{
			return renderTargetViews.Length;
		}

		public RenderTargetView[] GetRenderTargetViews()
		{
			return renderTargetViews;
		}

		public ShaderResourceView[] GetShaderResourceViews()
		{
			return shaderResourceViews;
		}

		public Color4 GetClearColor(int renderTargetIndex)
		{
			return clearColors[renderTargetIndex];
		}

		public void SetClearColor(int renderTargetIndex, Color4 color)
		{
			clearColors[renderTargetIndex] = color;
		}

		public void Begin()
		{
			if (renderTargetViews.Length > 0)
			{
				game.DeviceContext.OutputMerger.SetTargets(depthStencilView, renderTargetViews);
			}
			else
			{
				game.DeviceContext.OutputMerger.SetRenderTargets(depthStencilView, renderTargetViews[0]);
			}
			game.DeviceContext.Rasterizer.SetViewport(vp);
			for (int i = 0; i < renderTargetViews.Length; ++i)
			{
				game.DeviceContext.ClearRenderTargetView(renderTargetViews[i], clearColors[i]);
			}
			game.DeviceContext.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
		}

		public void End()
		{
			game.DeviceContext.OutputMerger.SetRenderTargets(game.DefaultDepthStencilView, game.DefaultRenderTargetView);
			game.DeviceContext.Rasterizer.SetViewport(game.DefaultViewport);
		}

		public void Dispose()
		{
			PreviewDeactive();
			Utilities.Dispose(ref depthStencilView);
			Utilities.Dispose(ref depthStencilTexture);
			for (int i = 0; i < textures.Length; ++i)
			{
				Utilities.Dispose(ref shaderResourceViews[i]);
				Utilities.Dispose(ref renderTargetViews[i]);
				Utilities.Dispose(ref textures[i]);
			}
		}

		public Controls.Control[] Controls
		{
			get {
				if (GetRenderTargetCount() == 1) return new Lilium.Controls.Control[0];
				string[] items = new string[renderTargetViews.Length];
				for(int i=0;i<renderTargetViews.Length;++i)
				{
					items[i] = "RenderTarget " + i;
				}
				var combo = new Lilium.Controls.ComboBox("RenderTargetIndex", items, () => previewIndex, val =>
				{
					PreviewDeactive();
					previewIndex = val;
					PreviewActive();
				});
				return new Lilium.Controls.Control[] { combo };
			}
		}

		public string NameInObjectList
		{
			get { return debugName; }
		}

		public void PreviewDraw()
		{
			if(texturePreview != null) texturePreview.PreviewDraw();
		}

		public void PreviewActive()
		{
			if (texturePreview == null)
			{
				texturePreview = new TexturePreview(shaderResourceViews[previewIndex]);
				texturePreview.PreviewActive();
			}
		}

		public void PreviewDeactive()
		{
			if (texturePreview != null)
			{
				texturePreview.PreviewDeactive();
				texturePreview = null;
			}
		}
	}
}
