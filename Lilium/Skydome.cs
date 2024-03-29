﻿using System;
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
	public class Skydome : IDisposable
	{
		struct Data
		{
			public Matrix matWorld2;
			public Vector4 bottomColor;
			public Vector4 topColor;
		}

		Mesh sphere;
		MaterialPass pass;
		Buffer buffer;
		Device device;

		public Skydome(Device device)
		{
			this.device = device;
			sphere = Mesh.CreateSphere();

			MaterialPassDesc desc = new MaterialPassDesc();
			desc.ManualConstantBuffers = true;
			desc.ShaderFile = "Skydome.hlsl";
			desc.RasteriazerStates.CullMode = CullMode.None;
			desc.DepthStencilStates.IsDepthEnabled = false;
			pass = new MaterialPass(device, desc, "Skydome");
			buffer = Material.CreateBuffer<Data>();

			//Game.Instance.AddControl(new Lilium.Controls.ColorPicker("Bottom", () => data.bottomColor, val => data.bottomColor = val));
			//Game.Instance.AddControl(new Lilium.Controls.ColorPicker("Top", () => data.topColor, val => data.topColor = val));
		}

		public void Draw()
		{
			pass.Apply();

			var data = new Data();
			data.matWorld2 = Matrix.Translation(Camera.ActiveCamera.Position);
			data.bottomColor = Camera.ActiveCamera.SkyBottomColor;
			data.topColor = Camera.ActiveCamera.SkyTopColor;
			device.ImmediateContext.UpdateSubresource(ref data, buffer);
			device.ImmediateContext.VertexShader.SetConstantBuffer(0, buffer);
			device.ImmediateContext.PixelShader.SetConstantBuffer(0, buffer);

			sphere.DrawBegin();
			sphere.DrawSubmesh(0);
		}

		public void Dispose()
		{
			sphere.Dispose();
			pass.Dispose();
			buffer.Dispose();
		}
	}
}
