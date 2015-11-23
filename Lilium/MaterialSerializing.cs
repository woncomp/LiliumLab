using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using Newtonsoft.Json;

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using System.ComponentModel;

namespace Lilium.Serializing
{
	class Material
	{
		public static void Serialize(MaterialDesc desc, string filePath)
		{
			var materialSerializing = new Material();
			materialSerializing.Import(desc);
			var str = JsonConvert.SerializeObject(materialSerializing, Formatting.Indented);
			System.IO.File.WriteAllText(filePath, str);
		}

		public static void Deserialize(MaterialDesc desc, string filePath)
		{
			var str = System.IO.File.ReadAllText(filePath);
			var materialSerializing = JsonConvert.DeserializeObject<Material>(str);
			materialSerializing.Export(desc);
		}

		public MaterialPass[] Passes;
		
		public Material()
		{
			Passes = new MaterialPass[0];
		}

		public void Import(MaterialDesc desc)
		{
			Passes = new MaterialPass[desc.Passes.Count];
			for (int i = 0; i < Passes.Length; ++i)
			{
				Passes[i] = new MaterialPass();
				Passes[i].Import(desc.Passes[i]);
			}
		}

		public void Export(MaterialDesc desc)
		{
			desc.Passes = new List<MaterialPassDesc>();
			for (int i = 0; i < Passes.Length; ++i)
			{
				var pass = new MaterialPassDesc();
				Passes[i].Export(pass);
				desc.Passes.Add(pass);
			}
		}
	}

	class MaterialPass
	{
		public ShaderEntry ShaderEntry { get; set; }
		public RasterizerState RasterizerStates { get; set; }
		public BlendState BlendStates { get; set; }
		public DepthStencilState DepthStencilStates { get; set; }

		public InputElement[] InputElements { get; set; }
		public MaterialTexture[] Textures { get; set; }

		public Dictionary<string, string> VariableValues;

		public MaterialPass()
		{
			ShaderEntry = new ShaderEntry();
			RasterizerStates = new RasterizerState();
			BlendStates = new BlendState();
			DepthStencilStates = new DepthStencilState();
		}

		public void Import(MaterialPassDesc desc)
		{
			ShaderEntry.Import(ref desc);
			RasterizerStates.Import(ref desc.RasteriazerStates);
			BlendStates.Import(ref desc.BlendStates);
			DepthStencilStates.Import(ref desc.DepthStencilStates);

			// Other
			InputElements = InputElement.Import(desc.InputElements);
			Textures = MaterialTexture.Import(desc.Textures);
			VariableValues = desc.VariableValues;
		}

		public void Export(MaterialPassDesc desc)
		{
			ShaderEntry.Export(ref desc);
			RasterizerStates.Export(ref desc.RasteriazerStates);
			BlendStates.Export(ref desc.BlendStates);
			DepthStencilStates.Export(ref desc.DepthStencilStates);

			// Other
			desc.InputElements = InputElement.Export(InputElements);
			desc.Textures = MaterialTexture.Export(Textures);
			desc.VariableValues = VariableValues ?? new Dictionary<string, string>();
		}
	}

	[TypeConverter(typeof(ExpandableObjectConverter))]
	[JsonObject]
	class ShaderEntry
	{
		public string ShaderFile;

		public string VertexShaderFunction { get; set; }
		public string PixelShaderFunction { get; set; }
		public string GeometryShaderFunction { get; set; }
		public string HullShaderFunction { get; set; }
		public string DomainShaderFunction { get; set; }

		public void Import(ref MaterialPassDesc desc)
		{
			ShaderFile = desc.ShaderFile;
			VertexShaderFunction = desc.VertexShaderFunction;
			PixelShaderFunction = desc.PixelShaderFunction;
			GeometryShaderFunction = desc.GeometryShaderFunction;
			HullShaderFunction = desc.HullShaderFunction;
			DomainShaderFunction = desc.DomainShaderFunction;
		}

		public void Export(ref MaterialPassDesc desc)
		{
			desc.ShaderFile = ShaderFile;
			desc.VertexShaderFunction = VertexShaderFunction;
			desc.PixelShaderFunction = PixelShaderFunction;
			desc.GeometryShaderFunction = GeometryShaderFunction;
			desc.HullShaderFunction = HullShaderFunction;
			desc.DomainShaderFunction = DomainShaderFunction;
		}
	}

	[TypeConverter(typeof(ExpandableObjectConverter))]
	[JsonObject]
	class RasterizerState
	{
		public CullMode CullMode { get; set; }
		public int DepthBias { get; set; }
		public float DepthBiasClamp { get; set; }
		public FillMode FillMode { get; set; }
		public bool IsAntialiasedLineEnabled { get; set; }
		public bool IsDepthClipEnabled { get; set; }
		public bool IsFrontCounterClockwise { get; set; }
		public bool IsMultisampleEnabled { get; set; }
		public bool IsScissorEnabled { get; set; }
		public float SlopeScaledDepthBias { get; set; }

		public void Import(ref RasterizerStateDescription rs)
		{
			CullMode = rs.CullMode;
			DepthBias = rs.DepthBias;
			DepthBiasClamp = rs.DepthBiasClamp;
			FillMode = rs.FillMode;
			IsAntialiasedLineEnabled = rs.IsAntialiasedLineEnabled;
			IsDepthClipEnabled = rs.IsDepthClipEnabled;
			IsFrontCounterClockwise = rs.IsFrontCounterClockwise;
			IsMultisampleEnabled = rs.IsMultisampleEnabled;
			IsScissorEnabled = rs.IsScissorEnabled;
			SlopeScaledDepthBias = rs.SlopeScaledDepthBias;
		}

		public void Export(ref RasterizerStateDescription rs)
		{
			rs.CullMode = CullMode;
			rs.DepthBias = DepthBias;
			rs.DepthBiasClamp = DepthBiasClamp;
			rs.FillMode = FillMode;
			rs.IsAntialiasedLineEnabled = IsAntialiasedLineEnabled;
			rs.IsDepthClipEnabled = IsDepthClipEnabled;
			rs.IsFrontCounterClockwise = IsFrontCounterClockwise;
			rs.IsMultisampleEnabled = IsMultisampleEnabled;
			rs.IsScissorEnabled = IsScissorEnabled;
			rs.SlopeScaledDepthBias = SlopeScaledDepthBias;
		}
	}

	[TypeConverter(typeof(ExpandableObjectConverter))]
	[JsonObject]
	class BlendState
	{
		public bool AlphaToCoverageEnable { get; set; }
		public bool IndependentBlendEnable { get; set; }
		public RenderTargetBlend[] RenderTarget { get; set; }

		public void Import(ref BlendStateDescription bs)
		{
			AlphaToCoverageEnable = bs.AlphaToCoverageEnable;
			IndependentBlendEnable = bs.IndependentBlendEnable;
			RenderTarget = bs.RenderTarget.Select(src =>
			{
				var dest = new RenderTargetBlend();
				dest.AlphaBlendOperation = src.AlphaBlendOperation;
				dest.BlendOperation = src.BlendOperation;
				dest.DestinationAlphaBlend = src.DestinationAlphaBlend;
				dest.DestinationBlend = src.DestinationBlend;
				dest.IsBlendEnabled = src.IsBlendEnabled;
				dest.RenderTargetWriteMask = src.RenderTargetWriteMask;
				dest.SourceAlphaBlend = src.SourceAlphaBlend;
				dest.SourceBlend = src.SourceBlend;
				return dest;
			}).ToArray();
		}

		public void Export(ref BlendStateDescription bs)
		{
			bs.AlphaToCoverageEnable = AlphaToCoverageEnable;
			bs.IndependentBlendEnable = IndependentBlendEnable;
			for (int i = 0; i < bs.RenderTarget.Length; ++i)
			{
				var src = this.RenderTarget[i];
				bs.RenderTarget[i].AlphaBlendOperation = src.AlphaBlendOperation;
				bs.RenderTarget[i].BlendOperation = src.BlendOperation;
				bs.RenderTarget[i].DestinationAlphaBlend = src.DestinationAlphaBlend;
				bs.RenderTarget[i].DestinationBlend = src.DestinationBlend;
				bs.RenderTarget[i].IsBlendEnabled = src.IsBlendEnabled;
				bs.RenderTarget[i].RenderTargetWriteMask = src.RenderTargetWriteMask;
				bs.RenderTarget[i].SourceAlphaBlend = src.SourceAlphaBlend;
				bs.RenderTarget[i].SourceBlend = src.SourceBlend;
			}
		}
	}

	[TypeConverter(typeof(ExpandableObjectConverter))]
	[JsonObject]
	class DepthStencilState
	{
		public Comparison DepthComparison { get; set; }
		public DepthWriteMask DepthWriteMask { get; set; }
		public DepthStencilOperation BackFace { get; set; }
		public DepthStencilOperation FrontFace { get; set; }
		public bool IsDepthEnabled { get; set; }
		public bool IsStencilEnabled { get; set; }
		public byte StencilReadMask { get; set; }
		public byte StencilWriteMask { get; set; }

		public void Import(ref DepthStencilStateDescription dss)
		{
			DepthComparison = dss.DepthComparison;
			DepthWriteMask = dss.DepthWriteMask;
			BackFace = DepthStencilOperation.Import(ref dss.BackFace);
			FrontFace = DepthStencilOperation.Import(ref dss.FrontFace);
			IsDepthEnabled = dss.IsDepthEnabled;
			IsStencilEnabled = dss.IsStencilEnabled;
			StencilReadMask = dss.StencilReadMask;
			StencilWriteMask = dss.StencilWriteMask;
		}

		public void Export(ref DepthStencilStateDescription dss)
		{
			dss.DepthComparison = DepthComparison;
			dss.DepthWriteMask = DepthWriteMask;
			dss.BackFace = DepthStencilOperation.Export(BackFace);
			dss.FrontFace = DepthStencilOperation.Export(FrontFace);
			dss.IsDepthEnabled = IsDepthEnabled;
			dss.IsStencilEnabled = IsStencilEnabled;
			dss.StencilReadMask = StencilReadMask;
			dss.StencilWriteMask = StencilWriteMask;
		}
	}

	[TypeConverter(typeof(ExpandableObjectConverter))]
	[JsonObject]
	class DepthStencilOperation
	{
		public Comparison Comparison { get; set; }
		public StencilOperation DepthFailOperation { get; set; }
		public StencilOperation FailOperation { get; set; }
		public StencilOperation PassOperation { get; set; }

		public static DepthStencilOperation Import(ref DepthStencilOperationDescription src)
		{
			var dest = new DepthStencilOperation();
			dest.Comparison = src.Comparison;
			dest.DepthFailOperation = src.DepthFailOperation;
			dest.FailOperation = src.FailOperation;
			dest.PassOperation = src.PassOperation;
			return dest;
		}

		public static DepthStencilOperationDescription Export(DepthStencilOperation src)
		{
			var dest = new DepthStencilOperationDescription();
			dest.Comparison = src.Comparison;
			dest.DepthFailOperation = src.DepthFailOperation;
			dest.FailOperation = src.FailOperation;
			dest.PassOperation = src.PassOperation;
			return dest;
		}
	}

	[TypeConverter(typeof(ExpandableObjectConverter))]
	[JsonObject]
	class RenderTargetBlend
	{
		public BlendOperation AlphaBlendOperation { get; set; }
		public BlendOperation BlendOperation { get; set; }
		public BlendOption DestinationAlphaBlend { get; set; }
		public BlendOption DestinationBlend { get; set; }
		public bool IsBlendEnabled { get; set; }
		public ColorWriteMaskFlags RenderTargetWriteMask { get; set; }
		public BlendOption SourceAlphaBlend { get; set; }
		public BlendOption SourceBlend { get; set; }
	}

	[TypeConverter(typeof(ExpandableObjectConverter))]
	[JsonObject]
	class InputElement
	{
		public int AlignedByteOffset { get; set; }
		public InputClassification Classification { get; set; }
		public Format Format { get; set; }
		public int InstanceDataStepRate { get; set; }
		public int SemanticIndex { get; set; }
		public string SemanticName { get; set; }
		public int Slot { get; set; }

		public static InputElement[] Import(SharpDX.Direct3D11.InputElement[] src)
		{
			return src.Select(e =>
			{
				var dest = new InputElement();
				dest.AlignedByteOffset = e.AlignedByteOffset;
				dest.Classification = e.Classification;
				dest.Format = e.Format;
				dest.InstanceDataStepRate = e.InstanceDataStepRate;
				dest.SemanticIndex = e.SemanticIndex;
				dest.SemanticName = e.SemanticName;
				dest.Slot = e.Slot;
				return dest;
			}).ToArray();
		}

		public static SharpDX.Direct3D11.InputElement[] Export(InputElement[] src)
		{
			return src.Select(e =>
			{
				var dest = new SharpDX.Direct3D11.InputElement();
				dest.AlignedByteOffset = e.AlignedByteOffset;
				dest.Classification = e.Classification;
				dest.Format = e.Format;
				dest.InstanceDataStepRate = e.InstanceDataStepRate;
				dest.SemanticIndex = e.SemanticIndex;
				dest.SemanticName = e.SemanticName;
				dest.Slot = e.Slot;
				return dest;
			}).ToArray();
		}
	}

	[TypeConverter(typeof(ExpandableObjectConverter))]
	[JsonObject]
	public class MaterialTexture
	{
		public TextureAddressMode AddressU { get; set; }
		public TextureAddressMode AddressV { get; set; }
		public TextureAddressMode AddressW { get; set; }
		public Color4 BorderColor { get; set; }
		public Comparison ComparisonFunction { get; set; }
		public Filter Filter { get; set; }
		public int MaximumAnisotropy { get; set; }
		public float MaximumLod { get; set; }
		public float MinimumLod { get; set; }
		public float MipLodBias { get; set; }

		public string TextureFile;

		public void Import(MaterialTextureDesc src)
		{
			var dest = this;
			dest.AddressU = src.SamplerStates.AddressU;
			dest.AddressV = src.SamplerStates.AddressV;
			dest.AddressW = src.SamplerStates.AddressW;
			dest.BorderColor = src.SamplerStates.BorderColor;
			dest.ComparisonFunction = src.SamplerStates.ComparisonFunction;
			dest.Filter = src.SamplerStates.Filter;
			dest.MaximumAnisotropy = src.SamplerStates.MaximumAnisotropy;
			dest.MaximumLod = src.SamplerStates.MaximumLod;
			dest.MinimumLod = src.SamplerStates.MinimumLod;
			dest.MipLodBias = src.SamplerStates.MipLodBias;
			dest.TextureFile = src.TextureFile;
		}

		public void Export(MaterialTextureDesc dest)
		{
			var src = this;
			dest.SamplerStates.AddressU = src.AddressU;
			dest.SamplerStates.AddressV = src.AddressV;
			dest.SamplerStates.AddressW = src.AddressW;
			dest.SamplerStates.BorderColor = src.BorderColor;
			dest.SamplerStates.ComparisonFunction = src.ComparisonFunction;
			dest.SamplerStates.Filter = src.Filter;
			dest.SamplerStates.MaximumAnisotropy = src.MaximumAnisotropy;
			dest.SamplerStates.MaximumLod = src.MaximumLod;
			dest.SamplerStates.MinimumLod = src.MinimumLod;
			dest.SamplerStates.MipLodBias = src.MipLodBias;
			dest.TextureFile = src.TextureFile;
		}

		public static MaterialTexture[] Import(MaterialTextureDesc[] src)
		{
			return src.Select(e =>
			{
				var dest = new MaterialTexture();
				dest.Import(e);
				return dest;
			}).ToArray();
		}

		public static MaterialTextureDesc[] Export(MaterialTexture[] src)
		{
			return src.Select(e =>
			{
				var dest = new MaterialTextureDesc();
				e.Export(dest);
				return dest;
			}).ToArray();
		}
	}

}
