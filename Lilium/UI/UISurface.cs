using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Cyotek.Drawing.BitmapFont;

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Lilium
{
	public class UISurface : IDisposable
	{
		public List<UIWidget> Widgets = new List<UIWidget>();
		public bool IsDirty;

		private Device mDevice;

		private List<UISurfaceBatch> mBatches = new List<UISurfaceBatch>();

		private UISurfaceBatchPool mBatchPool;
		private UISurfaceFontMaterialManager mFontMaterialMgr;

		private int mHeight;
		private int mWidth;
		private float mAspectRatio;

		public int Width { get { return mWidth; } }
		public int Height { get { return mHeight; } }

		public UISurface(Device device)
		{
			this.mDevice = device;
			mBatchPool = new UISurfaceBatchPool(device);
			mFontMaterialMgr = new UISurfaceFontMaterialManager(device);
			SetDesignHeight(Game.Instance.RenderViewSize.Height);
		}

		public void SetDesignHeight(int height)
		{
			var sz = Game.Instance.RenderViewSize;
			mAspectRatio = sz.Width / (float)sz.Height;
			mHeight = height;
			mWidth = (int)(height * mAspectRatio);
			IsDirty = true;
		}

		public void AddWidget(UIWidget w)
		{
			if (!Widgets.Contains(w))
			{
				w.Surface = this;
				Widgets.Add(w);
				IsDirty = true;
			}
		}

		public void RemoveWidget(UIWidget w)
		{
			if (Widgets.Contains(w))
			{
				Widgets.Remove(w);
				w.Batch = null;
				w.Surface = null;
				IsDirty = true;
			}
		}

		public void Draw()
		{
			UpdateBatch();
			foreach (var batch in mBatches)
			{
				batch.Draw();
			}
		}

		void UpdateBatch()
		{
			if (!IsDirty) return;
			
			foreach (var batch in mBatches)
			{
				batch.Widgets.Clear();
				mBatchPool.Free(batch);
			}
			mBatches.Clear();

			Widgets.Sort((a, b) => (a.Depth > b.Depth) ? 1 : -1);
			UISurfaceBatch currBatch = null;
			foreach (var w in Widgets)
			{
				var material = GetMaterial(w);
				bool newBatch = currBatch == null || currBatch.Material != material;
				if(newBatch)
				{
					currBatch = mBatchPool.Alloc();
					currBatch.Material = material;
					mBatches.Add(currBatch);
				}
				currBatch.Widgets.Add(w);
				w.Batch = currBatch;
			}
			foreach (var batch in mBatches)
			{
				batch.BuildBatch();
			}
			IsDirty = false;
		}

		Material GetMaterial(UIWidget w)
		{
			if (w is UILabel)
			{
				var label = w as UILabel;
				if (label.Font == null) return null;
				else return mFontMaterialMgr.GetMaterial(label.Font);
			}
			else return null;
		}

		public void Dispose()
		{
			Widgets.Clear();
			mBatches.Clear();
			mBatchPool.Dispose();
		}
	}

	public class UISurfaceFontMaterialManager
	{
		Dictionary<UIFont, Material> mMaterials = new Dictionary<UIFont, Material>();

		private Device mDevice;

		public UISurfaceFontMaterialManager(Device device)
		{
			mDevice = device;
		}

		public Material GetMaterial(UIFont font)
		{
			if(!mMaterials.ContainsKey(font))
			{
				var desc = new MaterialDesc();
				desc.Passes[0].InputElements = new InputElement[]{
					new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
					new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0),
				};
				desc.Passes[0].BlendStates = CreateAlphaBlendState();
				desc.Passes[0].ShaderFile = "UIFont.hlsl";
				desc.Passes[0].Textures = new MaterialTextureDesc[1];
				desc.Passes[0].Textures[0] = new MaterialTextureDesc();
				var m = new Material(Game.Instance, desc, font.Texture.DebugName);
				m.Passes[0].TextureList[0] = font.Texture;
				mMaterials[font] = m;
				Game.Instance.AddObject(m);
			}
			return mMaterials[font];
		}

		BlendStateDescription CreateAlphaBlendState()
		{
			var desc = BlendStateDescription.Default();
			desc.RenderTarget[0].IsBlendEnabled = true;
			desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
			desc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
			desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
			desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
			desc.RenderTarget[0].SourceBlend = BlendOption.One;
			desc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
			desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
			return desc;
		}
	}
}
