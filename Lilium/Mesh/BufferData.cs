using Assimp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Lilium
{
    public enum BufferDataFormat
    {
        P_UV_N_T,
        P_UV_N_T_BONE,
    }

    public abstract class BufferData
    {
        public BufferDescription Desc;

        public abstract BufferDataFormat Format { get; }

        public abstract int Count { get; }

        public abstract Buffer CreateBuffer(Device device);

        public abstract int Stride { get; }
    }

    public abstract class BufferDataT<T> : BufferData where T : struct
    {
        public override BufferDataFormat Format { get { return format; } }
        public override int Count { get { return dataList.Count; } }
        public override int Stride { get { return Utilities.SizeOf<T>(); } }

        private BufferDataFormat format;
        public List<T> dataList = new List<T>();

        public BufferDataT(BufferDataFormat format)
        {
            this.format = format;
            Desc.BindFlags = BindFlags.VertexBuffer;
        }

        public void Add(T data)
        {
            dataList.Add(data);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            dataList.AddRange(collection);
        }

        public override Buffer CreateBuffer(Device device)
        {
            return Buffer.Create(device, dataList.ToArray(), Desc);
        }
    }

    public class BufferData_SkinnedMeshStandard : BufferDataT<SkinnedMeshVertex>
    {
        public BufferData_SkinnedMeshStandard()
            :base(SkinnedMeshVertex.Format)
        {
        }
    }
}
