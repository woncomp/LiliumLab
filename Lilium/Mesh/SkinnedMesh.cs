using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using System.Runtime.InteropServices;

namespace Lilium
{
    public partial class SkinnedMesh : IDisposable, ISelectable
    {
		Device device;

		public string ResourceName;

		internal List<SkinnedSubMesh> submeshes = new List<SkinnedSubMesh>();
        public SkeletonNode skeleton;
		Dictionary<string, SkeletonNode> skeletonNodeDict = new Dictionary<string, SkeletonNode>();

		List<SkeletonNode> submeshNodes = new List<SkeletonNode>();
        
		public SkinnedMesh(Device device)
		{
			this.device = device;
			CreateControls();
		}

		public void Draw(int submeshIndex = -1)
		{
            if(submeshIndex >= 0 && submeshIndex < submeshes.Count)
            {
                submeshes[submeshIndex].Draw();
            }
            else
            {
                for (int i = 0; i < submeshes.Count; ++i) submeshes[i].Draw();
            }
		}

		public SkinnedSubMesh GetSubmesh(int submeshIndex)
		{
            if(submeshIndex >= 0 && submeshIndex < submeshes.Count)
            {
			    return this.submeshes[submeshIndex];
            }
            return null;
		}

        public void Dispose()
        {
            foreach (var submesh in submeshes)
            {
                submesh.Dispose();
            }
            submeshes.Clear();
        }

#region SkinnedAnimation
		public Dictionary<string, AnimationClip> AnimationClips = new Dictionary<string, AnimationClip>();
        public AnimationState AnimationState;

        public void PlayAnimation(string animName)
        {
            if (!AnimationClips.ContainsKey(animName)) return;
            if(AnimationState == null) AnimationState = new Lilium.AnimationState(this);
			AnimationState.PlayAnimation(AnimationClips[animName], 1);
        }
        

        public void UpdateSkinning()
		{
			foreach (var node in skeletonNodeDict.Values) node.LocalTransform = node.PoseMatrix;
            if(AnimationState != null) AnimationState.Update(Game.DeltaTime);

            foreach (var node in skeleton)
            {
                node.GlobalTransform = node.LocalTransform;
                if(node.Parent != null)
                {
					node.GlobalTransform = node.GlobalTransform * node.Parent.GlobalTransform;
                }
            }
            
            //var input = Game.Instance.Input;
            //float moveDelta = 10.1f * (float)Game.DeltaTime;
            //if (input.GetKey(System.Windows.Forms.Keys.A)) bone2Rotation -= moveDelta;
            //if (input.GetKey(System.Windows.Forms.Keys.D)) bone2Rotation += moveDelta;

            //var n2 = skeletonNodeDict["Bone002"];
            //Vector3 scale, pos;
            //Quaternion rotation;
            //n2.LocalTransform.Decompose(out scale, out rotation, out pos);

            //    var matTranslation = Matrix.Translation(pos);
            //    var matRotation = Matrix.RotationX(bone2Rotation);
            //    var matScale = Matrix.Scaling(scale);
            //    n2.LocalTransform = matScale * matRotation * matTranslation;
            //n2.GlobalTransform = n2.LocalTransform * n2.Parent.GlobalTransform;
        }
#endregion

		#region Selectable

		private Lilium.Controls.Control[] controls;

		void CreateControls()
		{
			List<Lilium.Controls.Control> list = new List<Controls.Control>();
            if (true)
            {
                var toggle = new Lilium.Controls.Toggle("Draw TBN", () => Config.DrawTBN, val => Config.DrawTBN = val);
                list.Add(toggle);
                var slider = new Lilium.Controls.Slider("Draw TBN Offset", 0, 2, () => Config.TBNOffset, val => Config.TBNOffset = val);
                list.Add(slider);
            }
			if (true)
			{
				var toggle = new Lilium.Controls.Toggle("Draw Wireframe", () => Config.DrawWireframe, val => Config.DrawWireframe = val);
				list.Add(toggle);
			}
			for (int i = 0; i < submeshes.Count;++i )
			{
				var submesh = submeshes[i];
				var toggle = new Lilium.Controls.Toggle(i + " " + submesh.Node.Name, () => submesh.Show, val => submesh.Show = val);
				list.Add(toggle);
			}
			controls = list.ToArray();
		}

		public Controls.Control[] Controls
		{
			get { return controls; }
		}

		public string NameInObjectList
		{
			get { return System.IO.Path.GetFileNameWithoutExtension(ResourceName); }
		}
		#endregion
    }

    public class SkinnedSubMesh : IDisposable
	{
		public const int MaxBones = 48;
		public bool SoftwareSkinning = true;

        public SkeletonNode Node;
		public bool Show = false;

        Device device;
        internal Buffer vertexBuffer;
        internal Buffer indexBuffer;
        
        BufferDescription Desc;
        private BufferData vertexBufferData;
        private MeshVertex[] vertexBufferData2;
        private uint[] indexBufferData;

		private Bone[] boneList;

		Matrix[] matBonePalette;
		float[] skinDataArray;
		Buffer skinBuffer;

		public SkinnedSubMesh(Device device)
        {
			this.device = device;
			var sz = MaxBones * Utilities.SizeOf<Matrix>();//Utilities.SizeOf<LiliumSkinData>()
			skinBuffer = new Buffer(device, sz, ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
			matBonePalette = new Matrix[MaxBones];
			skinDataArray = new float[MaxBones * 16];
        }

        public void SetVertexData(BufferData data)
        {
            vertexBufferData = data;
			if (SoftwareSkinning)
			{
				Desc = new BufferDescription();
				Desc.BindFlags = BindFlags.VertexBuffer;
				Desc.Usage = ResourceUsage.Dynamic;
				Desc.CpuAccessFlags = CpuAccessFlags.Write;
				Desc.StructureByteStride = Utilities.SizeOf<MeshVertex>();
				Desc.SizeInBytes = vertexBufferData.Count * Desc.StructureByteStride;

				vertexBuffer = new Buffer(device, Desc);
			}
			else
				vertexBuffer = vertexBufferData.CreateBuffer(device);
        }

        public void SetIndexData(uint[] data)
        {
            indexBufferData = data;
            indexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, indexBufferData);
        }

		public void SetBoneData(Bone[] data)
		{
			boneList = data;
		}

        public void Draw()
		{
			if (skinBuffer == null) return;
			var dc = Game.Instance.DeviceContext;

			// update matrix palette
			for (int i = 0; i < boneList.Length; ++i)
			{
				var bone = boneList[i];
				var node = bone.Node;
				matBonePalette[i] = bone.OffsetMatrix * node.GlobalTransform;
			}

            //Software skinning
			if (SoftwareSkinning)
			{
				if (vertexBufferData2 == null) vertexBufferData2 = new MeshVertex[vertexBufferData.Count];
				var originalVertices = (vertexBufferData as BufferData_SkinnedMeshStandard).dataList;
				for (int i = 0; i < vertexBufferData2.Length; ++i)
				{
					var v0 = originalVertices[i];
					var matrix = Matrix.Zero;
					for (int j = 0; j < 4; ++j)
					{
						matrix += matBonePalette[v0.BoneIndex[j]] * v0.BoneWeight[j];
					}
					var pos = Vector3.TransformCoordinate(v0.Position, matrix);
					var newVertex = new MeshVertex()
					{
						Position = pos,
						Normal = v0.Normal,
						Tangent = v0.Tangent,
						TexCoord = v0.TexCoord,
					};
					var oldVertex = vertexBufferData2[i];
					vertexBufferData2[i] = newVertex;
				}
				DataStream stream;
				dc.MapSubresource(vertexBuffer, MapMode.WriteDiscard, MapFlags.None, out stream);
				stream.WriteRange(vertexBufferData2);
				dc.UnmapSubresource(vertexBuffer, 0);
				stream.Dispose();
				dc.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Desc.StructureByteStride, 0));
			}
			else
			{
				dc.UpdateSubresource(GetBonePaletteFloatArray(), skinBuffer);
				dc.VertexShader.SetConstantBuffer(3, skinBuffer);
				dc.PixelShader.SetConstantBuffer(3, skinBuffer);
				dc.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, vertexBufferData.Stride, 0));
			}

			dc.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
			dc.InputAssembler.SetIndexBuffer(indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);
            
			dc.DrawIndexed(indexBufferData.Length, 0, 0);
		}

		public float[] GetBonePaletteFloatArray()
		{
			for(int i=0;i<boneList.Length;++i)
			{
				var mat = matBonePalette[i];
				for (int j = 0; j < 16; ++j) skinDataArray[i * 16 + j] = mat[j];
			}
			return skinDataArray;
		}

        public void Dispose()
		{
			skinBuffer.Dispose();
			skinBuffer = null;
            if (vertexBuffer != null) { vertexBuffer.Dispose(); vertexBuffer = null; }
            if (indexBuffer != null) { indexBuffer.Dispose(); indexBuffer = null; }
        }
    }

	public class SkeletonNode : IEnumerable<SkeletonNode>
	{
		public string Name;
		public SkeletonNode Parent;
		public List<SkeletonNode> Children = new List<SkeletonNode>();
		public SkinnedSubMesh Submesh;
		public Matrix PoseMatrix;
		public Matrix LocalTransform;
		public Matrix GlobalTransform;

		public bool DbgDrawBone;

		public void UpdateDebugDrawBone()
		{
			DbgDrawBone = Submesh == null;
			foreach (var child in Children)
			{
				child.UpdateDebugDrawBone();
				DbgDrawBone |= child.DbgDrawBone;
			}
		}

        public IEnumerator<SkeletonNode> GetEnumerator()
        {
            return new SkeletonIterator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new SkeletonIterator(this);
        }

        public class SkeletonIterator : IEnumerator<SkeletonNode>
        {
            Queue<SkeletonNode> mQueue;
            SkeletonNode mRoot;
            SkeletonNode mCurrent = null;

            public SkeletonIterator(SkeletonNode node)
            {
                mRoot = node;
                Reset();
            }

            public SkeletonNode Current
            {
                get { return mCurrent; }
            }

            object System.Collections.IEnumerator.Current
            {
                get { return mCurrent; }
            }

            public bool MoveNext()
            {
                if(mQueue.Count == 0) return false;
                mCurrent = mQueue.Dequeue();
                foreach (var child in mCurrent.Children)
                {
                    mQueue.Enqueue(child);
                }
                return true;
            }

            public void Reset()
            {
                mQueue = new Queue<SkeletonNode>();
                mQueue.Enqueue(mRoot);
                mCurrent = null;
            }

            public void Dispose()
            {
            }
        }
    }

    public class Bone
    {
        public SkeletonNode Node;
        public int Index;
        public Matrix OffsetMatrix;
    }

	[StructLayout(LayoutKind.Sequential)]
    public struct SkinnedMeshVertex
    {
        public const BufferDataFormat Format = BufferDataFormat.P_UV_N_T_BONE;

		public Vector3 Position;
		public Vector3 Normal;
		public Vector3 Tangent;
		public Vector2 TexCoord;
		public Vector4 BoneWeight;
        public Int4 BoneIndex;
    }
}
