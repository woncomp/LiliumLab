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
	public abstract class MaterialVariable
	{
		public string Name;
		public int Size;
		public int StartOffset;

		public MaterialVariable(ShaderReflectionVariable v)
		{
			Name = v.Description.Name;
			Size = v.Description.Size;
			StartOffset = v.Description.StartOffset;
		}

		public abstract void Write(DataStream stream);
		public abstract Lilium.Controls.Control CreateControl();
		public abstract void Deserialize(string str);
		public abstract string Serialize();
	}

	public class MaterialFloatVariable : MaterialVariable
	{
		public float value = 0;
		public float maxValue = 1;
		public float minValue = 0;

		public MaterialFloatVariable(ShaderReflectionVariable v)
			: base(v)
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

		public override void Deserialize(string str)
		{
			var split = str.Split(new char[] { ';', '=' });
			value = float.Parse(split[0]);
			minValue = float.Parse(split[2]);
			maxValue = float.Parse(split[4]);
		}

		public override string Serialize()
		{
			return string.Format("{0};min={1};max={2}", value, minValue, maxValue);
		}
	}

	public class MaterialFloat2Variable : MaterialVariable
	{
		public Vector2 value = new Vector2();

		public MaterialFloat2Variable(ShaderReflectionVariable v)
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

		public override void Deserialize(string str)
		{
			var split = str.Split(',');
			value.X = float.Parse(split[0]);
			value.Y = float.Parse(split[1]);
		}

		public override string Serialize()
		{
			return string.Format("{0},{1}", value.X, value.Y);
		}
	}

	public class MaterialFloat3Variable : MaterialVariable
	{
		public Vector3 value = new Vector3();

		public MaterialFloat3Variable(ShaderReflectionVariable v)
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

		public override void Deserialize(string str)
		{
			var split = str.Split(',');
			value.X = float.Parse(split[0]);
			value.Y = float.Parse(split[1]);
			value.Z = float.Parse(split[2]);
		}

		public override string Serialize()
		{
			return string.Format("{0},{1},{2}", value.X, value.Y, value.Z);
		}
	}

	public class MaterialFloat4Variable : MaterialVariable
	{
		public Vector4 value = new Vector4();

		public MaterialFloat4Variable(ShaderReflectionVariable v)
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

		public override void Deserialize(string str)
		{
			var split = str.Split(',');
			value.X = float.Parse(split[0]);
			value.Y = float.Parse(split[1]);
			value.Z = float.Parse(split[2]);
			value.W = float.Parse(split[3]);
		}

		public override string Serialize()
		{
			return string.Format("{0},{1},{2},{3}", value.X, value.Y, value.Z, value.W);
		}
	}

	public class MaterialColorVariable : MaterialVariable
	{
		public Vector4 value = Vector4.One;

		public MaterialColorVariable(ShaderReflectionVariable v)
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

		public override void Deserialize(string str)
		{
			var split = str.Split(',');
			value.X = float.Parse(split[0]);
			value.Y = float.Parse(split[1]);
			value.Z = float.Parse(split[2]);
			value.W = float.Parse(split[3]);
		}

		public override string Serialize()
		{
			return string.Format("{0},{1},{2},{3}", value.X, value.Y, value.Z, value.W);
		}
	}

	public class MaterialConstantBuffer : IDisposable
	{
		public string Name;
		public List<MaterialVariable> Variables = new List<MaterialVariable>();
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
}
