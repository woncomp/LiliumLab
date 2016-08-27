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
	public class UIWidget
	{
		public Vector2 Position
		{
			get { return mPosition; }
			set
			{
				if(mPosition == value) return;
				
				mPosition = value;
				SetDirty();
			}
		}
		public float Depth
		{
			get { return mDepth; }
			set
			{
				if (mDepth == value) return;
				mDepth = value;
				SetDirty();
			}
		}
		public float Scale
		{
			get { return mScale; }
			set
			{
				if (mScale == value) return;
				mScale = value;
				SetDirty();
			}
		}

		public UISurface Surface;
		public UISurfaceBatch Batch;

		private Vector2 mPosition;
		private float mDepth;
		private float mScale = 1;

		public virtual void FillGeometry(List<UIVertex> vertices, List<uint> indices)
		{

		}
		
		protected void SetDirty()
		{
			if (Surface != null) Surface.IsDirty = true;
		}
	}
}
