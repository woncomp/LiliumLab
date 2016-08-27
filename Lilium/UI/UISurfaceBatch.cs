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
	public class UISurfaceBatch : IDisposable
	{
		public List<UIWidget> Widgets = new List<UIWidget>();
		public Material Material;

		private Device mDevice;

		private Buffer mVertexBuffer;
		private Buffer mIndexBuffer;
		private List<UIVertex> mVertexList = new List<UIVertex>();
		private List<uint> mIndexList = new List<uint>();

		public UISurfaceBatch(Device device)
		{
			mDevice = device;
		}

		public void BuildBatch()
		{
			mVertexList.Clear();
			mIndexList.Clear();
			foreach (var w in Widgets)
			{
				w.FillGeometry(mVertexList, mIndexList);
			}
			if (mVertexBuffer != null) mVertexBuffer.Dispose();
			if (mIndexBuffer != null) mIndexBuffer.Dispose();
			mVertexBuffer = Buffer.Create(mDevice, BindFlags.VertexBuffer, mVertexList.ToArray());
			mIndexBuffer = Buffer.Create(mDevice, BindFlags.IndexBuffer, mIndexList.ToArray());
		}

		public void Draw()
		{
			var dc = mDevice.ImmediateContext;
			foreach (var pass in Material.Passes)
			{
				pass.Apply();
				dc.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
				dc.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(mVertexBuffer, Utilities.SizeOf<UIVertex>(), 0));
				dc.InputAssembler.SetIndexBuffer(mIndexBuffer, Format.R32_UInt, 0);
				dc.DrawIndexed(mIndexList.Count, 0, 0);
				pass.Clear();
			}
		}

		public void Dispose()
		{
			mVertexBuffer.Dispose();
			mIndexBuffer.Dispose();
		}
	}

	public class UISurfaceBatchPool : IDisposable
	{
		private Stack<UISurfaceBatch> mStack = new Stack<UISurfaceBatch>();

		private Device mDevice;

		public UISurfaceBatchPool(Device device)
		{
			mDevice = device;
		}

		public UISurfaceBatch Alloc()
		{
			if (mStack.Count > 0) return mStack.Pop();
			else return new UISurfaceBatch(mDevice);
		}

		public void Free(UISurfaceBatch obj)
		{
			mStack.Push(obj);
		}

		public void Dispose()
		{
			while(mStack.Count() > 0)
			{
				var b = mStack.Pop();
				b.Dispose();
			}
		}
	}
}
