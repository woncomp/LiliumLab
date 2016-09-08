using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using LiliumFbx;
using System.IO;

namespace Lilium
{
	public partial class SkinnedMesh
	{
		public static SkinnedMesh CreateWithFbxsdk(Device device, string filePath, AnimationClipCreateInfo[] clips)
		{
			var scene = FBXScene.Load(filePath);
			if (scene == null) return null;

			var skinnedMesh = new SkinnedMesh(device);
			skinnedMesh.FbxsdkFilePath = filePath;

			var meshDict = new Dictionary<SkeletonNode, FBXMesh>();
			skinnedMesh.FbxsdkBuildHierachy(scene.GetRootNode(), null, meshDict);
			skinnedMesh.FbxsdkLoadSubmeshes(meshDict);
			skinnedMesh.FbxsdkLoadAnimations(scene, clips);

			skinnedMesh.skeleton.UpdateDebugDrawBone();
			skinnedMesh.CreateControls();

			return skinnedMesh;
		}

		string FbxsdkFilePath;
		
		void FbxsdkBuildHierachy(FBXNode fbxNode, SkeletonNode parentNode, Dictionary<SkeletonNode, FBXMesh> outputMeshDict)
		{
			var node = new SkeletonNode();
			node.Name = fbxNode.GetName();
			node.PoseMatrix = FbxsdkConvertMatrix(fbxNode.GetLocalTransform());
			node.Parent = parentNode;
			if (parentNode == null) skeleton = node;
			else parentNode.Children.Add(node);
			var mesh = fbxNode.GetMesh() ;
			if(mesh != null)
			{
				outputMeshDict[node] = mesh;
			}
			if (skeletonNodeDict.ContainsKey(node.Name))
			{
				Debug.Log("Found duplicated skeleton node name: " + node.Name);
			}
			else
			{
				skeletonNodeDict.Add(node.Name, node);
			}
			for (int i = 0; i < fbxNode.GetChildCount(); ++i)
			{
				FbxsdkBuildHierachy(fbxNode.GetChild(i), node, outputMeshDict);
			}
		}

		void FbxsdkLoadSubmeshes(Dictionary<SkeletonNode, FBXMesh> meshDict)
		{
			foreach (var pair in meshDict)
			{
				var meshNode = pair.Key;
				var fbxMesh = pair.Value;
				fbxMesh.ProcessVertices();

				var submesh = new SkinnedSubMesh(device);
				submeshes.Add(submesh);

				submesh.Node = meshNode;
				submesh.Show = true;
				
				// Vertex 
				var vertices = fbxMesh.Vertices.Select(FbxsdkConvertVertex).ToArray();
				var indices = fbxMesh.Indices.ToArray();
				
				// Bone
				var bones = new Bone[fbxMesh.Bones.Count];
				for (int i = 0; i < bones.Length; ++i)
				{
					var fbxBone = fbxMesh.Bones[i];
					var nodeName = fbxBone.NodeName;
					SkeletonNode node;
					skeletonNodeDict.TryGetValue(nodeName, out node);
					var bone = new Bone();
					bone.Node = node;
					bone.Index = i;
					bone.OffsetMatrix = FbxsdkConvertMatrix(fbxBone.OffsetMatrix);
					bones[i] = bone;
				}

				var bufferData = new BufferData_SkinnedMeshStandard();
				bufferData.AddRange(vertices);
				submesh.SetVertexData(bufferData);
				submesh.SetIndexData(indices);
				submesh.SetBoneData(bones);
			}
		}

		public void FbxsdkLoadAnimations(string fileName, AnimationClipCreateInfo[] clips)
		{
			var filePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(FbxsdkFilePath), fileName);

			var scene = FBXScene.Load(filePath);
			if (scene == null) return;

			FbxsdkLoadAnimations(scene, clips);
		}

        public void FbxsdkLoadAnimations(string yamlFileName)
        {
			var filePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(FbxsdkFilePath), yamlFileName);
            
			using (var sr = System.IO.File.OpenText(filePath))
            { 
                var data = new YamlDotNet.Serialization.Deserializer().Deserialize<PersistentAnimationData>(sr);

                for(int i=0;i<data.Files.Count;++i)
                {
                    var fileInfo = data.Files[i];
                    var sceneFilePath = Game.Instance.ResourceManager.FindValidResourceFilePath(fileInfo.FileName, "SkinnedMesh");
                    if(!File.Exists(sceneFilePath)) continue;
                    var scene = FBXScene.Load(sceneFilePath);
                    if(scene == null) continue;
                    FbxsdkLoadAnimations(scene, fileInfo.Clips.Select(c => new AnimationClipCreateInfo(c.AnimationClipName, c.Start, c.End)).ToArray());
                    scene.Dispose();
                }

                AnimationComponent = new AnimationComponent(this);
                AnimationComponent.Load(data);
            }
        }

		void FbxsdkLoadAnimations(FBXScene scene, AnimationClipCreateInfo[] clips)
		{
			const double FRAMES_PER_SECOND = 30;
			if (clips == null) return;
			List<FBXNode> fbxNodeList = new List<FBXNode>();
			FbxsdkFindAllAnimatedNodes(scene, scene.GetRootNode(), fbxNodeList);

			if (fbxNodeList.Count == 0)
			{
				Debug.Log("Don't have animation data");
				return;
			}
			foreach (var clipCreateInfo in clips)
			{
				var start = clipCreateInfo.Start;
				var frameCount = clipCreateInfo.End - start + 1;
				var clip = new AnimationClip();
				clip.Name = clipCreateInfo.Name;
				clip.SecondsPerFrame = 1 / FRAMES_PER_SECOND;
				clip.Duration = frameCount / FRAMES_PER_SECOND;
				foreach (var fbxNode in fbxNodeList)
				{
					var channel = new AnimationNodeChannel(frameCount);
					channel.Clip = clip;
					for (int i = 0; i < frameCount; ++i)
					{
						channel.Frames[i] = FbxsdkConvertMatrix(fbxNode.EvaluateLocalTransform(start + i));
					}
					clip.Channels[fbxNode.GetName()] = channel;
				}
				AnimationClips[clip.Name] = clip;
			}
		}

		void FbxsdkFindAllAnimatedNodes(FBXScene scene, FBXNode node, List<FBXNode> outputCollection)
		{
			if(scene.HasCurve(node))
				outputCollection.Add(node);
			for(int i=0;i<node.GetChildCount();++i)
			{
				FbxsdkFindAllAnimatedNodes(scene, node.GetChild(i), outputCollection);
			}
		}

		static Matrix FbxsdkCalcTransform(SkeletonNode node)
		{
			var t = Matrix.Identity;
			while (node != null)
			{
				t = t * node.PoseMatrix;
				node = node.Parent;
			}
			return t;
		}

		static Matrix FbxsdkConvertMatrix(FBXMatrix input)
		{
			var output = new Matrix();
			output.M11 = input.M11;
			output.M12 = input.M12;
			output.M13 = input.M13;
			output.M14 = input.M14;
			output.M21 = input.M21;
			output.M22 = input.M22;
			output.M23 = input.M23;
			output.M24 = input.M24;
			output.M31 = input.M31;
			output.M32 = input.M32;
			output.M33 = input.M33;
			output.M34 = input.M34;
			output.M41 = input.M41;
			output.M42 = input.M42;
			output.M43 = input.M43;
			output.M44 = input.M44;
			return output;
		}

		static Vector3 FbxsdkConvertVector3(FBXVector3 v)
		{
			return new Vector3(v.X, v.Y, v.Z);
		}

		static Vector2 FbxsdkConvertTexCoord(FBXVector3 v)
		{
			return new Vector2(v.X, v.Y);
		}

		static SkinnedMeshVertex FbxsdkConvertVertex(FBXVertex input)
		{
			var output = new SkinnedMeshVertex();
			output.Position = FbxsdkConvertVector3( input.Position);
			output.Normal = FbxsdkConvertVector3( input.Normal);
			output.Tangent = FbxsdkConvertVector3( input.Tangent);
			output.TexCoord = FbxsdkConvertTexCoord( input.TexCoord0);
			output.BoneIndex[0] = input.Weight0.Index;
			output.BoneWeight[0] = input.Weight0.Weight;
			output.BoneIndex[1] = input.Weight1.Index;
			output.BoneWeight[1] = input.Weight1.Weight;
			output.BoneIndex[2] = input.Weight2.Index;
			output.BoneWeight[2] = input.Weight2.Weight;
			output.BoneIndex[3] = input.Weight3.Index;
			output.BoneWeight[3] = input.Weight3.Weight;
			return output;
		}
	}
}
