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
using System.IO;

namespace Lilium
{
	public class MaterialDesc
	{
		public static MaterialDesc Load(string filePath)
		{
			var desc = new MaterialDesc();
			Serializing.Material.Deserialize(desc, filePath);
			return desc;
		}

		public void Save(string filePath)
		{
			Serializing.Material.Serialize(this, filePath);
		}

		public string ResourceName;
		public List<MaterialPassDesc> Passes;

		public MaterialDesc()
		{
			Passes = new List<MaterialPassDesc>(1);
			Passes.Add(new MaterialPassDesc());
		}
	}

	public class MaterialPassDesc
	{
		public string ShaderFile;
		public string VertexShaderFunction;
		public string PixelShaderFunction;
		public string GeometryShaderFunction;
		public string HullShaderFunction;
		public string DomainShaderFunction;

		public RasterizerStateDescription RasteriazerStates;
		public BlendStateDescription BlendStates;
		public DepthStencilStateDescription DepthStencilStates;
		public int StencilRef;

		public InputElement[] InputElements;

		public MaterialTextureDesc[] Textures;

		public Dictionary<string, string> VariableValues;

		public bool ManualConstantBuffers = false;

		public MaterialPassDesc()
		{
			VertexShaderFunction = "VS";
			PixelShaderFunction = "PS";

			RasteriazerStates = RasterizerStateDescription.Default();
			BlendStates = BlendStateDescription.Default();
			DepthStencilStates = DepthStencilStateDescription.Default();

			StencilRef = 0;

			InputElements = new InputElement[]{
				new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
				new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
				new InputElement("TANGENT", 0, Format.R32G32B32_Float, 24, 0),
				new InputElement("TEXCOORD", 0, Format.R32G32_Float, 36, 0),
			};

			Textures = new MaterialTextureDesc[0];
			VariableValues = new Dictionary<string, string>();
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
			TextureFile = "white.png";
		}
	}

	public class Material : IDisposable, ISelectable
	{
		public static Buffer CreateBuffer<T>() where T : struct
		{
			return new Buffer(Game.Instance.Device, Utilities.SizeOf<T>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
		}

		public MaterialDesc Desc;

		public bool IsValid { get; private set; }
		public string ErrorMessage { get; private set; }

		public string DebugName
		{
			get { return debugName; }
		}

		public MaterialPass[] Passes { get; private set; }

		public Device Device { get; private set; }
		public DeviceContext DeviceContext { get; private set; }
		public Game Game { get; private set; }

		private string debugName = "";

		public Material(Game game, MaterialDesc desc, string debugName = null)
		{
			this.Game = game;
			this.Device = game.Device;
			this.DeviceContext = game.DeviceContext;
			this.Desc = desc;
			this.debugName = debugName ?? "Material Object " + Debug.NextObjectId;

			Load();
		}

		public void Reload()
		{
			Dispose();
			Load();
		}

		void Load()
		{
			IsValid = true;
			var sb = new StringBuilder();
			sb.Append("Errors:");

			Passes = new MaterialPass[Desc.Passes.Count];
			for (int i = 0; i < Passes.Length; ++i)
			{
				Passes[i] = new MaterialPass(Device, Desc.Passes[i], debugName + " Pass " + i);
				if(!Passes[i].IsValid)
				{
					IsValid = false;
					sb.Append('\n');
					sb.Append("Pass " + i + ":\n");
					sb.Append(Passes[i].ErrorMessage);
				}
			}

			if (IsValid)
			{
				DeserializeVariables();
				ErrorMessage = "";
			}
			else
			{
				ErrorMessage = sb.ToString();
			}
			CreateControls();
		}

		public void SerializeVariables()
		{
			for (int i = 0; i < Passes.Length; ++i)
			{
				var pass = Passes[i];
				if(pass.IsValid)
				{
					var dic = new Dictionary<string, string>();
					pass.SerializeVariables(dic);
					Desc.Passes[i].VariableValues = dic;
				}
			}
		}

		public void DeserializeVariables()
		{
			for (int i = 0; i < Passes.Length; ++i)
			{
				var pass = Passes[i];
				if (pass.IsValid)
				{
					pass.DeserializeVariables(Desc.Passes[i].VariableValues);
				}
			}
		}

		public void Dispose()
		{
			for (int i = 0; i < Passes.Length; ++i)
			{
				Utilities.Dispose(ref Passes[i]);
			}
		}

		#region Selectable

		private Controls.Control[] controls;

		void CreateControls()
		{
			List<Lilium.Controls.Control> list = new List<Lilium.Controls.Control>();
			list.Add(new Lilium.Controls.MaterialHeader(this));
			if (!IsValid)
			{
				var textArea = new Lilium.Controls.TextArea();
				textArea.Text = ErrorMessage;
				list.Add(textArea);
			}
			for (int i = 0; i < Passes.Length; ++i)
			{
				var pass = Passes[i];
				list.Add(new Lilium.Controls.PassHeader("< Pass " + i + " >", pass.Desc));
				for (int j = 0; j < pass.Desc.Textures.Length; ++j)
				{
					list.Add(new Lilium.Controls.PassTextureSlot(pass, j));
				}
				Lilium.Controls.Control _insertAnchor = null;
				var btn = new Lilium.Controls.Button("Add Texture", () =>
				{
					int index = pass.Desc.Textures.Length;
					int size = index + 1;
					Array.Resize(ref pass.Desc.Textures, size);
					Array.Resize(ref pass.TextureList, size);
					Array.Resize(ref pass.SamplerStateList, size);

					pass.Desc.Textures[index] = new MaterialTextureDesc();
					pass.TextureList[index] = Game.ResourceManager.Tex2D.Load(pass.Desc.Textures[index].TextureFile);

					Game.InsertControl(new Lilium.Controls.PassTextureSlot(pass, index), _insertAnchor, true);
				});
				_insertAnchor = btn;
				list.Add(btn);

				if (pass.IsValid)
				{
					Passes[i].CreateAutoVariableControls(list);
				}
				else
				{
					list.Add(new Lilium.Controls.Label("XXXXXX", () => "Pass Error !!!"));
				}
			}
			list.Add(new Lilium.Controls.Button("Add Pass", () =>
			{
				Desc.Passes.Add(Desc.Passes[Desc.Passes.Count - 1]);
				Game.Instance.SelectedObject = null;
				Reload();
				Game.Instance.SelectedObject = this;
			}));
			controls = list.ToArray();
		}

		public Controls.Control[] Controls { get { return controls; } }
		public string NameInObjectList { get { return DebugName; } }

		#endregion
	}

	public class MaterialPass : IDisposable
	{
		public VertexShader VertexShader { get; private set; }
		public PixelShader PixelShader { get; private set; }
		public GeometryShader GeometryShader { get; private set; }
		public DomainShader DomainShader { get; private set; }
		public HullShader HullShader { get; private set; }
		public InputLayout Layout { get; private set; }

		public RasterizerState RasterizerState;
		public BlendState BlendState;
		public DepthStencilState DepthStencilState;

		public ShaderResourceView[] TextureList;
		public SamplerState[] SamplerStateList;

		public MaterialPassDesc Desc;

		public Device Device;
		public DeviceContext DeviceContext;

		public bool IsValid { get; private set; }
		public string ErrorMessage { get; private set; }

		private string debugName;
		private List<ShaderResourceBinding> shaderResourceBindings = new List<ShaderResourceBinding>();

		public MaterialPass(Device device, MaterialPassDesc desc, string debugName = null)
		{
			this.Device = device;
			this.DeviceContext = device.ImmediateContext;
			this.Desc = desc;
			this.debugName = debugName ?? ("Pass Object " + Debug.NextObjectId);

			IsValid = true;
			var filename = Game.Instance.ResourceManager.FindValidShaderFilePath(desc.ShaderFile);
			if(filename == null)
			{
				IsValid = false;
				ErrorMessage = "Cannot find a shader file with name " + desc.ShaderFile;
			}
			if(IsValid) try
			{
				autoConstantBuffers = new List<MaterialConstantBuffer>();

				if (string.IsNullOrEmpty(desc.VertexShaderFunction))
					throw new System.ArgumentException("Vertex shader function name is nessessary in a material.");

				var vertexShaderByteCode = CompileShader(filename, desc.VertexShaderFunction, "vs_5_0");
				VertexShader = new VertexShader(Device, vertexShaderByteCode);
				VertexShader.DebugName = debugName;
				ScanConstantBuffers(vertexShaderByteCode);

				if (!string.IsNullOrEmpty(desc.PixelShaderFunction))
				{
					var pixelShaderByteCode = CompileShader(filename, desc.PixelShaderFunction, "ps_5_0");
					PixelShader = new PixelShader(Device, pixelShaderByteCode);
					PixelShader.DebugName = debugName;
					ScanConstantBuffers(pixelShaderByteCode);
				}

				if (!string.IsNullOrEmpty(desc.DomainShaderFunction))
				{
					var domainShaderByteCode = CompileShader(filename, desc.DomainShaderFunction, "ds_5_0");
					DomainShader = new DomainShader(Device, domainShaderByteCode);
					DomainShader.DebugName = debugName;
					ScanConstantBuffers(domainShaderByteCode);
				}

				if (!string.IsNullOrEmpty(desc.HullShaderFunction))
				{
					var hullShaderByteCode = CompileShader(filename, desc.HullShaderFunction, "hs_5_0");
					HullShader = new HullShader(Device, hullShaderByteCode);
					HullShader.DebugName = debugName;
					ScanConstantBuffers(hullShaderByteCode);
				}

				var signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
				Layout = new InputLayout(Device, signature, desc.InputElements);
			}
			catch (SharpDX.CompilationException e)
			{
				IsValid = false;
				ErrorMessage = e.Message;
				Debug.Log(e.Message);
			}

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
			DeviceContext.OutputMerger.SetDepthStencilState(DepthStencilState, Desc.StencilRef);

			DeviceContext.PixelShader.SetSamplers(0, SamplerStateList);
			DeviceContext.PixelShader.SetShaderResources(0, TextureList);

			for(int i=0;i<shaderResourceBindings.Count;++i)
			{
				var b = shaderResourceBindings[i];
				DeviceContext.PixelShader.SetSampler(b.Slot, b.Sampler);
				if (b.CubemapEntity != null)
				{
					DeviceContext.PixelShader.SetShaderResource(b.Slot, b.CubemapEntity.Cubemap.ShaderResourceView);
				}
				else
				{
					DeviceContext.PixelShader.SetShaderResource(b.Slot, b.Res);
				}
			}
		}

		public void UpdateConstantBuffers()
		{
			for (int i = 0; i < autoConstantBuffers.Count; ++i)
			{
				autoConstantBuffers[i].Update();
			}
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
			DeviceContext.OutputMerger.SetDepthStencilState(null);

			for (int i = 0; i < TextureList.Length || i < shaderResourceBindings.Count; ++i)
			{
				//DeviceContext.PixelShader.SetSampler(i, null);
				DeviceContext.PixelShader.SetShaderResource(i, null);
			}
		}

		public void BindShaderResource(int slot, ShaderResourceView shaderResourceView)
		{
			BindShaderResource(slot, shaderResourceView, SamplerStateDescription.Default());
		}

		public void BindShaderResource(int slot, ShaderResourceView shaderResourceView, SamplerStateDescription desc)
		{
			BindShaderResource(slot, shaderResourceView, null, desc);
		}

		public void BindRealtimeCubemap(int slot, Entity entity)
		{
			BindShaderResource(slot, null, entity, SamplerStateDescription.Default());
		}

		private void BindShaderResource(int slot, ShaderResourceView shaderResourceView, Entity cubemapEntity, SamplerStateDescription desc)
		{
			while (shaderResourceBindings.Count <= slot) shaderResourceBindings.Add(null);
			if (shaderResourceBindings[slot] != null)
			{
				Utilities.Dispose(ref shaderResourceBindings[slot].Sampler);
			}
			shaderResourceBindings[slot] = new ShaderResourceBinding()
			{
				Slot = slot,
				Res = shaderResourceView,
				Sampler = new SamplerState(Device, desc),
				CubemapEntity = cubemapEntity,
			};
		}

		public void SerializeVariables(Dictionary<string, string> dic)
		{
			for (int i = 0; i < autoConstantBuffers.Count; ++i)
			{
				var acb = autoConstantBuffers[i];
				for (int j = 0; j < acb.Variables.Count; ++j)
				{
					var variable = acb.Variables[j];
					if (!variable.Name.StartsWith("__"))
					{
						dic[variable.Name] = variable.Serialize();
					}
				}
			}
		}

		public void DeserializeVariables(Dictionary<string, string> dic)
		{
			for (int i = 0; i < autoConstantBuffers.Count; ++i)
			{
				var acb = autoConstantBuffers[i];
				for (int j = 0; j < acb.Variables.Count; ++j)
				{
					var variable = acb.Variables[j];
					if(dic.ContainsKey(variable.Name))
					{
						variable.Deserialize(dic[variable.Name]);
					}
				}
			}
		}

		public MaterialVariable FindVariable(string name)
		{
			for (int i = 0; i < autoConstantBuffers.Count; ++i)
			{
				var acb = autoConstantBuffers[i];
				for (int j = 0; j < acb.Variables.Count; ++j)
				{
					var variable = acb.Variables[j];
					if (variable.Name == name) return variable;
				}
			}
			return null;
		}

		public void Dispose()
		{
			for (int i = 0; i < shaderResourceBindings.Count; ++i)
			{
				var b = shaderResourceBindings[i];
				if (b != null) b.Sampler.Dispose();
			}

			for (int i = 0; i < autoConstantBuffers.Count; ++i)
			{
				autoConstantBuffers[i].Dispose();
			}

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

		#region CompileShader
		class ShaderInclude : Include
		{
			IDisposable shadow;
			List<IDisposable> toDispose = new List<IDisposable>();

			public void Close(System.IO.Stream stream)
			{
				stream.Close();
				toDispose.Add(stream);
			}

			public System.IO.Stream Open(IncludeType type, string fileName, System.IO.Stream parentStream)
			{
				var filePath = fileName;
				if (type == IncludeType.Local)
				{
					filePath = Game.Instance.ResourceManager.FindValidShaderFilePath(fileName);
				}
				else
				{
					var SearchPaths = Game.Instance.ResourceManager.SearchPaths;
					for (int i = SearchPaths.Count - 1; i >= 0; --i)
					{
						var folder = Path.Combine(SearchPaths[i], ResourceManager.SUBFOLDER_SHADER);
						var filePath2 = Path.Combine(folder, fileName);
						if (File.Exists(filePath2))
						{
							filePath = filePath2;
							break;
						}
					}
				}
				return new FileStream(filePath, FileMode.Open);
			}

			public IDisposable Shadow
			{
				get { return shadow; }
				set { shadow = value; }
			}

			public void Dispose()
			{
				foreach (var d in toDispose)
				{
					if (d != null) d.Dispose();
				}
			}
		}

		CompilationResult CompileShader(string fileName, string entryPoint, string profile)
		{
			return ShaderBytecode.CompileFromFile(fileName, entryPoint, profile, ShaderFlags.Debug, EffectFlags.None,
				null, new ShaderInclude());
		}
		#endregion

		#region Auto Shader Varaiables

		private List<MaterialConstantBuffer> autoConstantBuffers = new List<MaterialConstantBuffer>();

		void ScanConstantBuffers(ShaderBytecode code)
		{
			if (Desc.ManualConstantBuffers) return;
			var r = new ShaderReflection(code);
			int count = r.Description.ConstantBuffers;
			for (int i = 0; i < count; ++i)
			{
				var constantBuffer = r.GetConstantBuffer(i);
				var name = constantBuffer.Description.Name;
				if (name == "LiliumPerFrame" || name == "LiliumPerObject" || name.StartsWith("_")) continue;
				if (autoConstantBuffers.Exists(b => b.Name == name)) continue;

				var acb = new MaterialConstantBuffer();
				acb.Name = name;
				acb.Size = constantBuffer.Description.Size;
					
				for(int j = 0;j < r.Description.BoundResources;++j)
				{
					var rbd = r.GetResourceBindingDescription(j);
					if(rbd.Name == name)
					{
						acb.BindPoint = rbd.BindPoint;
						break;
					}
				}

				for(int j=0;j<constantBuffer.Description.VariableCount;++j)
				{
					var variable = constantBuffer.GetVariable(j);
					MaterialVariable av = null;
					var cls = variable.GetVariableType().Description.Class;
					switch (cls)
					{
						case ShaderVariableClass.Scalar:
							av = new MaterialFloatVariable(variable);
							break;
						case ShaderVariableClass.Vector:
							switch (variable.Description.Size)
							{
								case 8:
									av = new MaterialFloat2Variable(variable);
									break;
								case 12:
									av = new MaterialFloat3Variable(variable);
									break;
								case 16:
									if (variable.Description.Name.EndsWith("Color"))
									{
										av = new MaterialColorVariable(variable);
									}
									else
									{
										av = new MaterialFloat4Variable(variable);
									}
									break;
								default:
									throw new Exception("ShaderVariableClass.Vector --> variable.Description.Size");
							}
							break;
						//case ShaderVariableClass.MatrixRows:
						//	break;
						//case ShaderVariableClass.MatrixColumns:
						//	break;
						//case ShaderVariableClass.InterfaceClass:
						//	break;
						//case ShaderVariableClass.InterfacePointer:
						//	break;
						//case ShaderVariableClass.Object:
						//	break;
						//case ShaderVariableClass.Struct:
						//	break;
						case ShaderVariableClass.MatrixColumns:
							break;
						default:
							throw new Exception("ScanConstantBuffers() -> variable.GetVariableType().Description.Class : " + cls);
					}
					if(av != null) acb.Variables.Add(av);
				}
				acb.Variables.Sort((a, b) => a.StartOffset - b.StartOffset);
				acb.Init(Device);
				autoConstantBuffers.Add(acb);
			}
		}

		public void CreateAutoVariableControls(List<Lilium.Controls.Control> list)
		{
			foreach (var acb in autoConstantBuffers)
			{
				foreach (var av in acb.Variables)
				{
					if (av.Name.StartsWith("__")) continue;
					var control = av.CreateControl();
					if (control != null) list.Add(control);
				}
			}
		}
		#endregion
	
		class ShaderResourceBinding
		{
			public int Slot;
			public ShaderResourceView Res;
			public SamplerState Sampler;
			public Entity CubemapEntity;
		}
	}
}
