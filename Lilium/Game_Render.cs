using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Windows;
using SharpDX.DXGI;

namespace Lilium
{
    public partial class Game
    {
		public SamplerState DefaultSamplerState { get { return _samplerStateDefault; } }
		public SamplerState WrapSamplerState { get { return _samplerStateWrap; } }
		public BlendState DefaultBlendState { get { return _blendStateDefault; } }
		public BlendState AlphaBlendState { get { return _blendStateAlphaBlend; } }
		public RasterizerState DefaultRasterizerState { get { return _rasterizerStateDefault; } }
		public DepthStencilState DefaultDepthStencilState { get { return _depthStencilStateDefault; } }

		private SamplerState _samplerStateDefault;
		private SamplerState _samplerStateWrap;
		private BlendState _blendStateDefault;
		private BlendState _blendStateAlphaBlend;
		private RasterizerState _rasterizerStateDefault;
		private DepthStencilState _depthStencilStateDefault;

		void Render_Init()
		{
			CreateSamplerStates();
			CreateBlendStates();
			CreateRasterizerStates();
			CreateDepthStencilStates();
		}
		
		void CreateSamplerStates()
		{
			SamplerStateDescription description = SamplerStateDescription.Default();
			description.Filter = Filter.MinMagMipLinear;
			description.AddressU = TextureAddressMode.Clamp;
			description.AddressV = TextureAddressMode.Clamp;
			_samplerStateDefault = new SamplerState(Device, description);
			AutoDispose(_samplerStateDefault);

			description.Filter = Filter.MinMagLinearMipPoint;
			description.AddressU = TextureAddressMode.Wrap;
			description.AddressV = TextureAddressMode.Wrap;
			_samplerStateWrap = new SamplerState(Device, description);
			AutoDispose(_samplerStateWrap);

			DeviceContext.PixelShader.SetSampler(0, DefaultSamplerState);
		}

		void CreateBlendStates()
		{
			var desc = BlendStateDescription.Default();
			_blendStateDefault = new BlendState(Device, desc);
			AutoDispose(_blendStateDefault);

			desc = BlendStateDescription.Default();
			desc.RenderTarget[0].IsBlendEnabled = true;
			desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
			desc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
			desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
			desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
			desc.RenderTarget[0].SourceBlend = BlendOption.One;
			desc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
			desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
			_blendStateAlphaBlend = new BlendState(Device, desc);
			AutoDispose(_blendStateAlphaBlend);
		}

		void CreateRasterizerStates()
		{
			var desc = RasterizerStateDescription.Default();
			_rasterizerStateDefault = new RasterizerState(Device, desc);
			AutoDispose(_rasterizerStateDefault);
		}

		void CreateDepthStencilStates()
		{
			var desc = DepthStencilStateDescription.Default();
			_depthStencilStateDefault = new DepthStencilState(Device, desc);
			AutoDispose(_depthStencilStateDefault);
		}

		public void Clear(Color color)
		{
			DeviceContext.ClearRenderTargetView(DefaultRenderTargetView, color);
			DeviceContext.ClearDepthStencilView(DefaultDepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
		}
	}
}
