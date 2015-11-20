using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Lilium
{
	public class MaterialDesc
	{
		public static MaterialDesc Load(string filePath)
		{
			var desc = new MaterialDesc();
			desc.FilePath = filePath;
			Serializing.Material.Deserialize(desc);
			return desc;
		}

		public void Save(string filePath)
		{
			this.FilePath = filePath;
			Save();
		}

		public void Save()
		{
			Serializing.Material.Serialize(this);
		}

		public string FilePath;
		public string DebugName;

		public RasterizerStateDescription RasteriazerStates;
		public BlendStateDescription BlendStates;
		public DepthStencilStateDescription DepthStencilStates;

		public string ShaderFile;
		public string VertexShaderFunction;
		public string PixelShaderFunction;
		public string GeometryShaderFunction;
		public string HullShaderFunction;
		public string DomainShaderFunction;

		public InputElement[] InputElements;

		public MaterialTextureDesc[] Textures;

		public MaterialDesc()
		{
			RasteriazerStates = RasterizerStateDescription.Default();
			BlendStates = BlendStateDescription.Default();
			DepthStencilStates = DepthStencilStateDescription.Default();

			VertexShaderFunction = "VS";
			PixelShaderFunction = "PS";

			InputElements = new InputElement[]{
				new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
				new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
				new InputElement("TANGENT", 0, Format.R32G32B32_Float, 24, 0),
				new InputElement("TEXCOORD", 0, Format.R32G32_Float, 36, 0),
			};

			Textures = new MaterialTextureDesc[0];
		}
	}

	public class MaterialTextureDesc
	{
		public SamplerStateDescription SamplerStates;
		public string TextureFile;

		public MaterialTextureDesc()
		{
			SamplerStates = SamplerStateDescription.Default();
			SamplerStates.MaximumLod = 0;
			SamplerStates.MinimumLod = 0;
		}
	}

	public class Material : IDisposable, ISelectable
	{
		public static Buffer CreateBuffer<T>() where T : struct
		{
			return new Buffer(Game.Instance.Device, Utilities.SizeOf<T>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
		}

		public string ResourceName;
		public MaterialDesc Desc;

		public bool IsValid { get; private set; }
		public string ErrorMessage { get; private set; }

		public VertexShader VertexShader { get; private set; }
		public PixelShader PixelShader { get; private set; }
		public GeometryShader GeometryShader { get; private set; }
		public DomainShader DomainShader { get; private set; }
		public HullShader HullShader { get; private set; }
		public InputLayout Layout { get; private set; }

		public string DebugName
		{
			get { return debugName; }
		}

		public RasterizerState RasterizerState;
		public BlendState BlendState;
		public DepthStencilState DepthStencilState;

		public ShaderResourceView[] TextureList;
		public SamplerState[] SamplerStateList;

		private Device Device;
		private DeviceContext DeviceContext;

		private string debugName = "";

		public Material(MaterialDesc desc)
		{
			this.Desc = desc;
			this.Device = Game.Instance.Device;
			this.DeviceContext = Game.Instance.DeviceContext;
			this.debugName = string.IsNullOrEmpty(desc.DebugName) ?
				"Material" + Debug.NextObjectId + " " + System.IO.Path.GetFileNameWithoutExtension(desc.FilePath) :
				desc.DebugName;

			Load();
			CreateControls();
		}

		public void Reload()
		{
			Dispose();
			Load();
		}

		void Load()
		{
			var desc = this.Desc;
			IsValid = false;
			try
			{
				var filename = Game.Instance.ResourceManager.FindValidShaderFilePath(desc.ShaderFile);

				if (string.IsNullOrEmpty(desc.VertexShaderFunction))
					throw new System.ArgumentException("Vertex shader function name is nessessary in a material.");

				var vertexShaderByteCode = ShaderBytecode.CompileFromFile(filename, desc.VertexShaderFunction, "vs_5_0", ShaderFlags.Debug);
				VertexShader = new VertexShader(Device, vertexShaderByteCode);

				if (!string.IsNullOrEmpty(desc.PixelShaderFunction))
				{
					var pixelShaderByteCode = ShaderBytecode.CompileFromFile(filename, desc.PixelShaderFunction, "ps_5_0", ShaderFlags.Debug);
					PixelShader = new PixelShader(Device, pixelShaderByteCode);
				}

				if (!string.IsNullOrEmpty(desc.DomainShaderFunction))
				{
					var domainShaderByteCode = ShaderBytecode.CompileFromFile(filename, desc.DomainShaderFunction, "ds_5_0", ShaderFlags.Debug);
					DomainShader = new DomainShader(Device, domainShaderByteCode);
				}

				if (!string.IsNullOrEmpty(desc.HullShaderFunction))
				{
					var hullShaderByteCode = ShaderBytecode.CompileFromFile(filename, desc.HullShaderFunction, "hs_5_0", ShaderFlags.Debug);
					HullShader = new HullShader(Device, hullShaderByteCode);
				}

				var signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
				Layout = new InputLayout(Device, signature, desc.InputElements);

				RasterizerState = new RasterizerState(Device, desc.RasteriazerStates);
				BlendState = new BlendState(Device, desc.BlendStates);
				DepthStencilState = new DepthStencilState(Device, desc.DepthStencilStates);

				TextureList = new ShaderResourceView[desc.Textures.Length];
				SamplerStateList = new SamplerState[desc.Textures.Length];
				for (int i = 0; i < desc.Textures.Length; ++i)
				{
					var t = desc.Textures[i];
					if (string.IsNullOrEmpty(t.TextureFile))
					{
						TextureList[i] = null;
						SamplerStateList[i] = null;
					}
					else
					{
						TextureList[i] = Game.Instance.ResourceManager.Tex2D.Load(t.TextureFile);
						SamplerStateList[i] = new SamplerState(Device, t.SamplerStates);
					}
				}
				IsValid = true;
			}
			catch (Exception e)
			{
				ErrorMessage = e.Message;
				Debug.Log(ErrorMessage);
			}
			SetDebugName(debugName);
		}

		public void Apply()
		{
			if (!IsValid) return;
			DeviceContext.InputAssembler.InputLayout = Layout;
			DeviceContext.VertexShader.Set(VertexShader);
			DeviceContext.PixelShader.Set(PixelShader);
			DeviceContext.GeometryShader.Set(GeometryShader);
			DeviceContext.DomainShader.Set(DomainShader);
			DeviceContext.HullShader.Set(HullShader);

			DeviceContext.Rasterizer.State = RasterizerState;
			DeviceContext.OutputMerger.BlendState = BlendState;
			DeviceContext.OutputMerger.DepthStencilState = DepthStencilState;

			DeviceContext.PixelShader.SetSamplers(0, SamplerStateList);
			DeviceContext.PixelShader.SetShaderResources(0, TextureList);
		}

		public void Clear()
		{
			DeviceContext.VertexShader.Set(null);
			DeviceContext.PixelShader.Set(null);
			DeviceContext.GeometryShader.Set(null);
			DeviceContext.DomainShader.Set(null);
			DeviceContext.HullShader.Set(null);

			DeviceContext.Rasterizer.State = Game.Instance.DefaultRasterizerState;
			DeviceContext.OutputMerger.BlendState = Game.Instance.DefaultBlendState;
			DeviceContext.OutputMerger.DepthStencilState = Game.Instance.DefaultDepthStencilState;

			for (int i = 0; i < TextureList.Length; ++i)
			{
				DeviceContext.PixelShader.SetSampler(i, null);
				DeviceContext.PixelShader.SetShaderResource(i, null);
			}
		}

		void SetDebugName(string name)
		{
			if (VertexShader != null) VertexShader.DebugName = name;
			if (PixelShader != null) PixelShader.DebugName = name;
			if (GeometryShader != null) GeometryShader.DebugName = name;
			if (DomainShader != null) DomainShader.DebugName = name;
			if (HullShader != null) HullShader.DebugName = name;
		}

		public void Dispose()
		{
			Utilities.Dispose(ref RasterizerState);
			Utilities.Dispose(ref BlendState);
			Utilities.Dispose(ref DepthStencilState);

			if (TextureList != null)
			{
				for (int i = 0; i < TextureList.Length; ++i)
				{
					Utilities.Dispose(ref SamplerStateList[i]);
				}
			}

			if (VertexShader != null)
				VertexShader.Dispose();

			if (PixelShader != null)
				PixelShader.Dispose();

			if (GeometryShader != null)
				GeometryShader.Dispose();

			if (DomainShader != null)
				DomainShader.Dispose();

			if (HullShader != null)
				HullShader.Dispose();

			if (Layout != null)
				Layout.Dispose();
		}

		public override string ToString()
		{
			return debugName;
		}

		#region Selectable

		private Controls.Control[] controls;
		
		void CreateControls()
		{
			var label = new Lilium.Controls.Label("Error", () =>IsValid ? "No Error" : ErrorMessage);
			var btn1 = new Lilium.Controls.Button("Edit", () =>
			{
				var editor = new MaterialEditor(this);
				editor.Show();
			});
			var btn2 = new Lilium.Controls.Button("Reload", Reload);
			var btn3 = new Lilium.Controls.Button("Save", () =>
			{
				Game.Instance.ResourceManager.Material.Save(this);
			});
			controls = new Controls.Control[] { label, btn1, btn2, btn3 };
		}

		public Controls.Control[] Controls
		{
			get { return controls; }
		}
		#endregion
	}
}
