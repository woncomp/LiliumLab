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
	public class RenderCubemap : IDisposable, ISelectable, IPreviewable
	{
		public static readonly float NEAR_PLANE = 0.1f;
		public static readonly float FAR_PLANE = 1000;
		public static readonly Matrix PROJECTION_MATRIX = Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1, NEAR_PLANE, FAR_PLANE);
		static readonly Vector3[] VIEW_DIRS = new[] {
				new Vector3(1, 0, 0),
				new Vector3(-1, 0, 0),
				new Vector3(0, 1, 0),
				new Vector3(0,-1, 0),
				new Vector3(0, 0, 1),
				new Vector3(0, 0, -1),
			};
		static readonly Vector3[] UP_DIRS = new[] {
				new Vector3(0, 1, 0),
				new Vector3(0, 1, 0),
				new Vector3(0, 0, -1),
				new Vector3(0, 0, 1),
				new Vector3(0, 1, 0),
				new Vector3(0, 1, 0),
			};

		public Vector3 Position;
		public Matrix ViewMatrix;

		public ShaderResourceView ShaderResourceView { get { return mSRV; } }
		public Viewport Viewport { get { return mViewport; } }

		public string Name;

		private int mSize;

		private Texture2D mTex;
		private ShaderResourceView mSRV;
		private RenderTargetView[] mRTVs;
		private DepthStencilView mDSV;
		private Viewport mViewport;

		private Game mGame;
		private CubemapPreview mPreview;

		public RenderCubemap(Game game, int size, string debugName = null)
		{
			this.mGame = game;
			this.mSize = size;
			this.Name = debugName ?? ("RenderCubemap" + Debug.NextObjectId);
			Create3D();
		}

		void Create3D()
		{
			var texDesc = new Texture2DDescription();
			texDesc.ArraySize = 6;
			texDesc.BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget;
			texDesc.CpuAccessFlags = CpuAccessFlags.None;
			texDesc.Format = Format.R8G8B8A8_UNorm;
			texDesc.Height = mSize;
			texDesc.Width = mSize;
			texDesc.MipLevels = 0;
			texDesc.SampleDescription.Count = 1;
			texDesc.SampleDescription.Quality = 0;
			texDesc.Usage = ResourceUsage.Default;
			texDesc.OptionFlags = ResourceOptionFlags.TextureCube | ResourceOptionFlags.GenerateMipMaps;

			mTex = new Texture2D(mGame.Device, texDesc);

			var srvDesc = new ShaderResourceViewDescription();
			srvDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.TextureCube;
			srvDesc.Format = texDesc.Format;
			srvDesc.TextureCube.MipLevels = -1;
			srvDesc.TextureCube.MostDetailedMip = 0;

			mSRV = new ShaderResourceView(mGame.Device, mTex, srvDesc);

			var rtvDesc = new RenderTargetViewDescription();
			rtvDesc.Dimension = RenderTargetViewDimension.Texture2DArray;
			rtvDesc.Format = texDesc.Format;
			rtvDesc.Texture2DArray.ArraySize = 1;
			rtvDesc.Texture2DArray.MipSlice = 0;

			mRTVs = new RenderTargetView[6];
			for (int i = 0; i < 6; ++i)
			{
				rtvDesc.Texture2DArray.FirstArraySlice = i;
				mRTVs[i] = new RenderTargetView(mGame.Device, mTex, rtvDesc);
			}

			var depthTexDesc = new Texture2DDescription()
			{
				Width = mSize,
				Height = mSize,
				MipLevels = 1,
				ArraySize = 1,
				SampleDescription = new SampleDescription(1, 0),
				Format = Format.D32_Float,
				Usage = ResourceUsage.Default,
				BindFlags = BindFlags.DepthStencil,
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None
			};
			var depthTex = new Texture2D(mGame.Device, depthTexDesc);
			var dsvDesc = new DepthStencilViewDescription();
			dsvDesc.Format = depthTexDesc.Format;
			dsvDesc.Dimension = DepthStencilViewDimension.Texture2D;
			dsvDesc.Texture2D.MipSlice = 0;
			dsvDesc.Flags = DepthStencilViewFlags.None;
			mDSV = new DepthStencilView(mGame.Device, depthTex, dsvDesc);

			Utilities.Dispose(ref depthTex);

			mViewport = new Viewport(0, 0, mSize, mSize);
		}

		public void Dispose()
		{
			Utilities.Dispose(ref mTex);
			Utilities.Dispose(ref mSRV);
			Utilities.Dispose(ref mDSV);
			for (int i = 0; i < 6; ++i)
			{
				Utilities.Dispose(ref mRTVs[i]);
			}
		}

		public Controls.Control[] Controls
		{
			get
			{
				PreviewActive();
				return mPreview.Controls;
			}
		}

		public string NameInObjectList
		{
			get { return Name; }
		}

		public void CaptureSceneAtPosition(Vector3 position)
		{
			mGame.CurrentCubemap = this;
			var dc = mGame.DeviceContext;
			Position = position;
			for (int idxFace = 0; idxFace < 6; ++idxFace)
			{
				ViewMatrix = Matrix.LookAtLH(Position, Position + VIEW_DIRS[idxFace], UP_DIRS[idxFace]);
				dc.Rasterizer.SetViewport(mViewport);
				dc.OutputMerger.SetTargets(mDSV, mRTVs[idxFace]);
				dc.ClearDepthStencilView(mDSV, DepthStencilClearFlags.Depth, 1, 0);
				mGame.UpdatePerFrameBuffer();
				if (mGame.SkyBox != null) mGame.SkyBox.Draw();
				for (int i = 0; i < mGame.MainScene.Entities.Count; ++i)
				{
					var e = mGame.MainScene.Entities[i];
					if (e.Cubemap != this) e.Draw();
				}
			}
			dc.GenerateMips(mSRV);
			mGame.CurrentCubemap = null;
			mGame.DeviceContext.OutputMerger.SetRenderTargets(mGame.DefaultDepthStencilView, mGame.DefaultRenderTargetView);
			mGame.DeviceContext.Rasterizer.SetViewport(mGame.DefaultViewport);
		}

		public void PreviewDraw()
		{
			if (mPreview != null) mPreview.PreviewDraw();
		}

		public void PreviewActive()
		{
			if (mPreview == null)
			{
				mPreview = new CubemapPreview(mGame, mSRV);
				mPreview.PreviewActive();
			}
		}

		public void PreviewDeactive()
		{
			if (mPreview != null)
			{
				mPreview.PreviewDeactive();
				mPreview = null;
			}
		}
	}
}
