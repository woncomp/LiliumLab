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

		public bool ManualConstantBuffers = false;

		public MaterialPassDesc()
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

			Passes = new MaterialPass[Desc.Passes.Count];
			for (int i = 0; i < Passes.Length; ++i)
			{
				Passes[i] = new MaterialPass(Device, Desc.Passes[i], debugName + " Pass " + i);
				if(!Passes[i].IsValid)
				{
					IsValid = false;
					sb.Append(Passes[i].ErrorMessage);
					sb.Append('\n');
				}
			}

			if (IsValid)
			{
				CreateControls();
			}
			else
			{
				ErrorMessage = sb.ToString();
			}
		}

		public override string ToString()
		{
			return debugName;
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
			var label = new Lilium.Controls.Label("Error", () => IsValid ? "No Error" : ErrorMessage);
			list.Add(label);
			var btn1 = new Lilium.Controls.Button("Edit", () =>
			{
				var editor = new MaterialEditor(this);
				editor.Show();
			});
			list.Add(btn1);
			var btn2 = new Lilium.Controls.Button("Reload", Reload);
			list.Add(btn2);
			var btn3 = new Lilium.Controls.Button("Save", () =>
			{
				Game.ResourceManager.Material.Save(this);
			});
			list.Add(btn3);
			for (int i = 0; i < Passes.Length; ++i)
			{
				list.Add(new Lilium.Controls.Label("Pass " + i, () => "--------------"));
				Passes[i].CreateAutoVariableControls(list);
			}
			controls = list.ToArray();
		}

		public Controls.Control[] Controls
		{
			get { return controls; }
		}
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

		public MaterialPass(Device device, MaterialPassDesc desc, string debugName = null)
		{
			this.Device = device;
			this.DeviceContext = device.ImmediateContext;
			this.Desc = desc;
			this.debugName = debugName ?? ("Pass Object " + Debug.NextObjectId);

			IsValid = false;
			try
			{
				autoConstantBuffers = new List<AutoConstantBuffer>();
				var filename = Game.Instance.ResourceManager.FindValidShaderFilePath(desc.ShaderFile);

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
			DeviceContext.OutputMerger.DepthStencilState = Game.Instance.DefaultDepthStencilState;

			for (int i = 0; i < TextureList.Length; ++i)
			{
				DeviceContext.PixelShader.SetSampler(i, null);
				DeviceContext.PixelShader.SetShaderResource(i, null);
			}
		}

		public void Dispose()
		{
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

		private List<AutoConstantBuffer> autoConstantBuffers = new List<AutoConstantBuffer>();

		class AutoConstantBuffer : IDisposable
		{
			public string Name;
			public List<AutoVariable> Variables = new List<AutoVariable>();
			public int BindPoint;
			public int Size;

			public Buffer buffer;

			private Device device;

			public void Init(Device device)
			{
				this.device = device;

				var desc = new BufferDescription();
				desc.BindFlags = BindFlags.ConstantBuffer;
				desc.CpuAccessFlags = CpuAccessFlags.Write;
				desc.OptionFlags = ResourceOptionFlags.None;
				desc.SizeInBytes = Size;
				desc.StructureByteStride = 0;
				desc.Usage = ResourceUsage.Dynamic;
				buffer = new Buffer(device, desc);
			}

			public void Update()
			{
				var dc = device.ImmediateContext;
				DataStream stream;
				dc.MapSubresource(buffer, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out stream);
				int offset = 0;
				for (int i = 0; i < Variables.Count; ++i)
				{
					var v = Variables[i];
					if (offset < v.StartOffset) stream.Seek(v.StartOffset - offset, SeekOrigin.Current);
					Variables[i].Write(stream);
					offset = v.StartOffset + v.Size;
				}
				dc.UnmapSubresource(buffer, 0);
				dc.VertexShader.SetConstantBuffer(BindPoint, buffer);
				dc.PixelShader.SetConstantBuffer(BindPoint, buffer);
				dc.HullShader.SetConstantBuffer(BindPoint, buffer);
				dc.DomainShader.SetConstantBuffer(BindPoint, buffer);
			}

			public void Dispose()
			{
				Utilities.Dispose(ref buffer);
			}
		}

		abstract class AutoVariable
		{
			public string Name;
			public int Size;
			public int StartOffset;

			public AutoVariable(ShaderReflectionVariable v)
			{
				Name = v.Description.Name;
				Size = v.Description.Size;
				StartOffset = v.Description.StartOffset;
			}

			public abstract void Write(DataStream stream);
			public abstract Lilium.Controls.Control CreateControl();
		}

		class AutoFloatVariable : AutoVariable
		{
			public float value = 0;
			public float maxValue = 1;
			public float minValue = 0;

			public AutoFloatVariable(ShaderReflectionVariable v) : base(v)
			{
				unsafe
				{
					float* p = (float*)v.Description.DefaultValue;
					if (p != null) value = *p;
				}
			}
			public override void Write(DataStream stream) { stream.Write(value); }
			public override Controls.Control CreateControl()
			{
				return new Lilium.Controls.Slider(Name, minValue, maxValue, () => value, val => value = val);
			}
		}

		class AutoFloat2Variable : AutoVariable
		{
			public Vector2 value = new Vector2();

			public AutoFloat2Variable(ShaderReflectionVariable v)
				: base(v)
			{
				unsafe
				{
					Vector2* p = (Vector2*)v.Description.DefaultValue;
					if (p != null) value = *p;
				}
			}
			public override void Write(DataStream stream) { stream.Write(value); }
			public override Controls.Control CreateControl()
			{
				Debug.Log("Auto control of float2 variable type not implemented.");
				return null;
			}
		}

		class AutoFloat3Variable : AutoVariable
		{
			public Vector3 value = new Vector3();

			public AutoFloat3Variable(ShaderReflectionVariable v)
				: base(v)
			{
				unsafe
				{
					Vector3* p = (Vector3*)v.Description.DefaultValue;
					if (p != null) value = *p;
				}
			}
			public override void Write(DataStream stream) { stream.Write(value); }
			public override Controls.Control CreateControl()
			{
				Debug.Log("Auto control of float3 variable type not implemented.");
				return null;
			}
		}

		class AutoFloat4Variable : AutoVariable
		{
			public Vector4 value = new Vector4();

			public AutoFloat4Variable(ShaderReflectionVariable v)
				: base(v)
			{
				unsafe
				{
					Vector4* p = (Vector4*)v.Description.DefaultValue;
					if (p != null) value = *p;
				}
			}
			public override void Write(DataStream stream) { stream.Write(value); }
			public override Controls.Control CreateControl()
			{
				Debug.Log("Auto control of float4 variable type not implemented.");
				return null;
			}
		}

		class AutoColorVariable : AutoVariable
		{
			public Vector4 value = Vector4.One;

			public AutoColorVariable(ShaderReflectionVariable v)
				: base(v)
			{
				unsafe
				{
					Vector4* p = (Vector4*)v.Description.DefaultValue;
					if (p != null) value = *p;
				}
			}
			public override void Write(DataStream stream) { stream.Write(value); }
			public override Controls.Control CreateControl()
			{
				return new Lilium.Controls.ColorPicker(Name, () => value, val => value = val);
			}
		}

		class AutoMatrixVariable : AutoVariable
		{
			public Matrix value = Matrix.Identity;

			public AutoMatrixVariable(ShaderReflectionVariable v) : base(v) { }
			public override void Write(DataStream stream) { stream.Write(value); }
			public override Controls.Control CreateControl()
			{
				Debug.Log("Auto control of matrix variable type not implemented.");
				return null;
			}
		}

		void ScanConstantBuffers(ShaderBytecode code)
		{
			if (Desc.ManualConstantBuffers) return;
			var r = new ShaderReflection(code);
			int count = r.Description.ConstantBuffers;
			for (int i = 0; i < count; ++i)
			{
				var constantBuffer = r.GetConstantBuffer(i);
				var name = constantBuffer.Description.Name;
				if (name == "LiliumPerFrame" || name == "LiliumPerObject") continue;
				if (autoConstantBuffers.Exists(b => b.Name == name)) continue;

				var acb = new AutoConstantBuffer();
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
					AutoVariable av = null;
					switch (variable.GetVariableType().Description.Class)
					{
						case ShaderVariableClass.Scalar:
							av = new AutoFloatVariable(variable);
							break;
						case ShaderVariableClass.Vector:
							switch (variable.Description.Size)
							{
								case 8:
									av = new AutoFloat2Variable(variable);
									break;
								case 12:
									av = new AutoFloat3Variable(variable);
									break;
								case 16:
									if (variable.Description.Name.EndsWith("Color"))
									{
										av = new AutoColorVariable(variable);
									}
									else
									{
										av = new AutoFloat4Variable(variable);
									}
									break;
								default:
									throw new Exception("ShaderVariableClass.Vector --> variable.Description.Size");
							}
							break;
						case ShaderVariableClass.MatrixRows:
							av = new AutoMatrixVariable(variable);
							break;
						case ShaderVariableClass.MatrixColumns:
							av = new AutoMatrixVariable(variable);
							break;
						//case ShaderVariableClass.InterfaceClass:
						//	break;
						//case ShaderVariableClass.InterfacePointer:
						//	break;
						//case ShaderVariableClass.Object:
						//	break;
						//case ShaderVariableClass.Struct:
						//	break;
						default:
							throw new Exception("ScanConstantBuffers() -> variable.GetVariableType().Description.Class");
					}
					acb.Variables.Add(av);
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
	}
}
