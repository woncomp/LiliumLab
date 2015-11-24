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
	public class StencilShadowRenderer : IDisposable
	{
		struct ShaderData
		{
			public Matrix ShadowWorldTransform;
			public float ShadowIndensity;
			public Vector3 ___;
		};
		MaterialPass pass;
		Buffer buffer;

		Game game;

		public StencilShadowRenderer(Game game)
		{
			this.game = game;

			var desc = new MaterialPassDesc();
			desc.ManualConstantBuffers = true;
			desc.ShaderFile = "StencilShadow.hlsl";
			desc.BlendStates.RenderTarget[0].IsBlendEnabled = true;
			desc.BlendStates.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
			desc.BlendStates.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
			desc.BlendStates.RenderTarget[0].BlendOperation = BlendOperation.Add;
			desc.BlendStates.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
			desc.BlendStates.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
			desc.BlendStates.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
			desc.BlendStates.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
			desc.DepthStencilStates.IsDepthEnabled = true;
			desc.DepthStencilStates.DepthWriteMask = DepthWriteMask.Zero;
			desc.DepthStencilStates.DepthComparison = Comparison.LessEqual;
			desc.DepthStencilStates.IsStencilEnabled = true;
			desc.DepthStencilStates.StencilReadMask = 1;
			desc.DepthStencilStates.StencilWriteMask = 1;
			desc.DepthStencilStates.FrontFace.FailOperation = StencilOperation.Keep;
			desc.DepthStencilStates.FrontFace.DepthFailOperation = StencilOperation.Keep;
			desc.DepthStencilStates.FrontFace.PassOperation = StencilOperation.Replace;
			desc.DepthStencilStates.FrontFace.Comparison = Comparison.NotEqual;
			desc.StencilRef = 1;
			pass = new MaterialPass(game.Device, desc, "StencilShadow");
			buffer = Material.CreateBuffer<ShaderData>();
		}

		public void Begin(Matrix transform, Plane plane, float shadowIndensity)
		{
			pass.Apply();

			var data = new ShaderData();
			data.ShadowWorldTransform = transform * Matrix.Shadow(Light.MainLight.LightDir4, plane);
			data.ShadowIndensity = shadowIndensity;
			data.___ = Vector3.Zero;
			game.DeviceContext.UpdateSubresource(ref data, buffer);
			game.DeviceContext.VertexShader.SetConstantBuffer(0, buffer);
			game.DeviceContext.PixelShader.SetConstantBuffer(0, buffer);
		}

		public void Dispose()
		{
			buffer.Dispose();
			pass.Dispose();
		}
	}
}
