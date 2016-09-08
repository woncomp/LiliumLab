using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;
using YamlDotNet.Serialization;

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Lilium
{
    public class AnimationComponent
    {
        public SkinnedMesh Mesh {get; set; }

        Layer[] mLayers;
        Dictionary<string, int> mLayerIndexDict;
        public Dictionary<string, PersistentAnimationState> mStates;

        public Dictionary<string, FloatParameter> mParameters = new Dictionary<string,FloatParameter>();
        
        private List<SkeletonNode> mSkeletonNodeList = new List<SkeletonNode>();
 
        FloatParameter this[string name]
        {
            get
            {
				EnsureParameter(name);
                return mParameters[name];
            }
        }
        
        public AnimationComponent(SkinnedMesh m)
        {
            this.Mesh = m;

            foreach (var node in m.skeleton)
            {
                var name = node.Name;
                mSkeletonNodeList.Add(node);
            }
            mLayers = new[] { new Layer(mSkeletonNodeList, null) };
        }

        public void Load(string fileName)
        {
            var filePath = Game.Instance.ResourceManager.FindValidResourceFilePath(fileName, "SkinnedMesh");

            using(var sr = new System.IO.StringReader(fileName))
            { 
                var data = new Deserializer().Deserialize<PersistentAnimationData>(sr);
                Load(data);
            }
        }

        public void Load(PersistentAnimationData data)
        {
			mLayerIndexDict = new Dictionary<string, int>();
            var list = new List<Layer>();
            list.Add(mLayers[0]);
                
            foreach (var layer in data.Layers)
	        {
                mLayerIndexDict[layer.LayerName] = list.Count;
		        list.Add(new Layer(mSkeletonNodeList, layer));
	        }
            mLayers = list.ToArray();
 
            mStates = data.States;
			foreach (var pair in mStates)
			{
				var p = pair.Value.ParamNameX;
				if (!string.IsNullOrEmpty(p)) EnsureParameter(p);
				p = pair.Value.ParamNameY;
				if (!string.IsNullOrEmpty(p)) EnsureParameter(p);
			}
        }

        public void SetParameter(string name, float value)
        {
            this[name].Value = value;
        }

        public void PlayAnimation(string stateName, float transitionTime)
        {
			if (!mStates.ContainsKey(stateName)) return;
            var stateData = mStates[stateName];
            
            var layerIndex = 0;
			if(stateData.Layer != null)
				mLayerIndexDict.TryGetValue(stateData.Layer, out layerIndex);
            var layer = mLayers[layerIndex];

            BlendNode blendNode;
            switch (stateData.BlendSpaceType)
            {
                case AnimationBlendSpaceType.BS_1D:
                    {
                        Motion[] array = new Motion[stateData.Clips.Count];
                        for(int i=0;i<array.Length;++i)
                        {
                            var clip = stateData.Clips[i];
                            array[i] = new Motion(mSkeletonNodeList, Mesh.AnimationClips[clip.AnimationClipName], clip.LocationX, clip.LocationY);
                        }
                        blendNode = new BlendSpace1D(this[stateData.ParamNameX], array);
                    }
                    break;
                case AnimationBlendSpaceType.BS_2D:
                    {
                        Motion[] array = new Motion[stateData.Clips.Count];
                        for(int i=0;i<array.Length;++i)
                        {
                            var clip = stateData.Clips[i];
                            array[i] = new Motion(mSkeletonNodeList, Mesh.AnimationClips[clip.AnimationClipName], clip.LocationX, clip.LocationY);
                        }
                        blendNode = new BlendSpace2D(this[stateData.ParamNameX], this[stateData.ParamNameY], array);
                    }
                    break;
                default:
                    {
                        var clip = stateData.Clips[0];
                        var motion = new Motion(mSkeletonNodeList, Mesh.AnimationClips[clip.AnimationClipName]);
                        blendNode = new AnimationBlendNode(motion);
                    }
                    break;
            }
            blendNode.FadingOut = layer.BlendNode;
            blendNode.TotalTransitionTime = transitionTime;
            layer.BlendNode = blendNode;
        }
        
        public void Update(double dt)
        {
            for (int i = 0; i < mLayers.Length; ++i)
            {
                mLayers[i].Blend(dt);
            }
        }

		private void EnsureParameter(string name)
		{
			if (!mParameters.ContainsKey(name)) mParameters[name] = new FloatParameter();
		}

        class Motion
        {
            public float LocationX;
            public float LocationY;

            public double AngularDistance;

            public AnimationNodeChannel[] Channels;
            public List<SkeletonNode> Nodes;

            private Matrix[] PoseMatrices;

            public Motion(List<SkeletonNode> nodes, AnimationClip clip = null, float x = 0, float y = 0)
            {
                Nodes = nodes;
                if(clip != null)
                {
                    LocationX = x;
                    LocationY = y;
                    Channels = new AnimationNodeChannel[nodes.Count];
                    for (int i = 0; i < nodes.Count; ++i)
                    {
                        AnimationNodeChannel channel;
				        clip.Channels.TryGetValue(nodes[i].Name, out channel);
                        Channels[i] = channel;
                    }
                }
                else
                {
                    Channels = null;
                    PoseMatrices = new Matrix[Nodes.Count];
                    for(int i=0;i<Nodes.Count;++i)
                    {
                        PoseMatrices[i] = Nodes[i].PoseMatrix;
                    }
                }
            }

            public Matrix[] Sample(double playTime)
            {
                if(Channels != null)
                {
                    var array = new Matrix[Channels.Length];
                    for(int i=0;i<Channels.Length;++i)
                    {
                        var c = Channels[i];
						if (c == null)
							array[i] = Matrix.Zero;
						else
                            array[i] = _Sample(c, playTime);
                    }
                    return array;
                }
                else return PoseMatrices;
            }

            static Matrix _Sample(AnimationNodeChannel Channel, double playTime)
            {
                var animLength = Channel.Clip.Duration;
                var frameLength = Channel.Clip.SecondsPerFrame;

                var t = playTime % animLength;
                if (t <= 0) t += animLength;
                var frameIndex = (int)Math.Floor(t / frameLength);
                if (frameIndex >= Channel.Frames.Length - 1)
                {
                    return Channel.Frames[Channel.Frames.Length - 1];
                }

                var frameRemaining = t - frameIndex * frameLength;
                var lerpFactor = frameRemaining / frameLength;
                return Matrix.Lerp(Channel.Frames[frameIndex], Channel.Frames[frameIndex + 1], (float)lerpFactor);
            }
        }

        public abstract class BlendNode
        {
            public double CurrentTransitionTime;
            public double TotalTransitionTime;
            public BlendNode FadingOut;
                                    
            public Matrix[] Blend(double dt)
            {
                var matrix = Tick(dt);
                if(FadingOut != null)
                {
                    CurrentTransitionTime += dt;
                    var FadingWeight = (float)(CurrentTransitionTime / TotalTransitionTime);
                    if(FadingWeight >= 1)
                    {
                        FadingOut = null;
                        return matrix;
                    }
                    else
                    {
                        Matrix[][] toBlend = new Matrix[2][];
                        float[] weights = new float[2];
                        toBlend[0] = matrix;
                        toBlend[1] = FadingOut.Blend(dt);
                        weights[0] = FadingWeight;
                        weights[1] = 1 - FadingWeight;
                        return _BlendN(toBlend, weights);
                    }
                }
                else
                {
                    return matrix;
                }
            }

            public abstract Matrix[] Tick(double dt);

            public static Matrix[] _BlendN(Matrix[][] toBlend, float[] weights)
            {
                var length = toBlend[0].Length;
                Matrix[] result = new Matrix[length];
                for (int i = 0; i < length; ++i)
                    result[i] = Matrix.Zero;

                for (int i = 0; i < weights.Length; ++i)
                {
                    var inputMatrices = toBlend[i];
                    var weight = weights[i];

                    for (int j = 0; j < length; ++j)
                    {
                        result[j] += inputMatrices[j] * weight;
                    }
                }
                return result;
            }
        }
        
        class AnimationBlendNode : BlendNode
        {
            Motion mMotion;
            private double playTime;

            public AnimationBlendNode(Motion m)
            {
                mMotion = m;
            }

            public override Matrix[] Tick(double dt)
            {
                playTime += dt;
                return mMotion.Sample(playTime);
            }
        }

        class BlendSpace1D : BlendNode
        {
            private double playTime;
            private FloatParameter mParameter;
            private Motion[] mMotions;

            public BlendSpace1D(FloatParameter p, params Motion[] motions)
            {
                mParameter = p;
                mMotions = motions;
            }

            public override Matrix[] Tick(double dt)
            {
                playTime += dt;
                var location = mParameter.Value;
                for (int i = 0; i < mMotions.Length; ++i)
                {
                    var m = mMotions[i];
                    if(m.LocationX >= location)
                    {
                        if(i == 0)
                            return m.Sample(playTime);

                        var m1 = mMotions[i - 1];
                        Matrix[][] toBlend = new Matrix[2][];
                        float[] weights = new float[2];
                        toBlend[0] = m1.Sample(playTime);
                        toBlend[1] = m.Sample(playTime);
                        weights[1] = (location - m1.LocationX) / (m.LocationX - m1.LocationX);
                        weights[0] = 1 - weights[1];
                        return _BlendN(toBlend, weights);
                    }
                }
                return mMotions[mMotions.Length - 1].Sample(playTime);
            }
        }

        class BlendSpace2D : BlendNode
        {
            private double playTime;

            private FloatParameter mParameterX;
            private FloatParameter mParameterY;
            private Motion[] mMotions;

            public BlendSpace2D(FloatParameter paramX, FloatParameter paramY, params Motion[] motions)
            {
                mParameterX = paramX;
                mParameterY = paramY;
                mMotions = motions;
            }
            
            public override Matrix[] Tick(double dt)
            {
                playTime += dt;
                Matrix[][] toBlend = new Matrix[mMotions.Length][];
                float[] weights = new float[mMotions.Length];
                
                var inputPos = new Vector2(mParameterX.Value, mParameterY.Value);

                var nodeList = new List<Motion>(mMotions);

                Motion centerNode = null;

                // 找出用于组成扇面三角形的节点
                var inputPhi = Math.Atan2(inputPos.Y, inputPos.X);
                for (int i = nodeList.Count - 1; i >= 0; --i)
                {
                    var node = nodeList[i];
                    var nodePos = new Vector2(node.LocationX, node.LocationY);

                    if(nodePos == Vector2.Zero)
                    { 
                        centerNode = node;
                        nodeList.RemoveAt(i);
                        continue;
                    }

                    var phi = Math.Atan2(nodePos.Y, nodePos.X) - inputPhi;
                    node.AngularDistance = phi % (Math.PI*2);
                }
                // 排序并取出节点。 这里31830(=100000/PI)是一个魔法数字，因为Compare要求返回一个整数
                nodeList.Sort((a, b) => (int)((a.AngularDistance - b.AngularDistance)*31830));
                var node0 = nodeList.First();
                var node1 = nodeList.Last();

                // 
                var pos0 = new Vector2(node0.LocationX, node0.LocationY);
                var pos1 = new Vector2(node1.LocationX, node1.LocationY);

                // 把节点坐标视为向量的话，一定有inputPos = t0 * pos0 + t1 * pos1，求解(t0, t1)
                Matrix mat = Matrix.Identity;
                mat.Column1 = new Vector4(pos0, 0, 0);
                mat.Column2 = new Vector4(pos1, 0, 0);
                mat.Invert();
                var t = Vector2.Transform(inputPos, mat);
        
                // 我们预期input点会在 (0,0), pos0, pos1 组成的三角形内。也就是说t0和t1都不会是负值。
                // 如果其中一项为负值，则说明在早期的步骤中并没能成功匹配到一个三角形扇区。
                // 对于这种情况，我们认为pos0和pos1各自享有50%的影响值
                var nodeInfluence0 = 0.5f;
                var nodeInfluence1 = 0.5f;
                if(t.X >= 0 && t.Y >= 0)
                {
                    var sum = t.X + t.Y;
                    nodeInfluence0 = t.X / sum;
                    nodeInfluence1 = t.Y / sum;
                }

                // 节点影响值。根据input点在原点与pos0, pos1的连线之间的倾向值来决定。
                // 如果input点更接近与原点，则中心影响值占据主导地位，如果input点更接近pos0,pos1的连线,则节点影响值占据主导地位。
                // 这个数字可以根据t来计算。我们知道在pos0, pos1的连线上=>{t0+t1=1}，而在原点>={t0+t1=0}，所以很容易得到答案。
                var nodeInfluence = MathUtil.Clamp(t.X+t.Y, 0, 1);
                node0.AngularDistance = nodeInfluence * nodeInfluence0;
                node1.AngularDistance = nodeInfluence * nodeInfluence1;

                // 中心影响值。如果存在一个中心节点centerNode，则所有中心影响值分配给该节点，否则将中心影响值平均分配给所有节点。
                if(nodeInfluence < 1)
                {
                    var centerInfluence = 1 - nodeInfluence;
                    if(centerNode != null)
                    {
                        centerNode.AngularDistance = centerInfluence;
                    }
                    else
                    {
                        centerInfluence /= mMotions.Length;
                        for (int i = 0; i < mMotions.Length; ++i) mMotions[i].AngularDistance += centerInfluence;
                    }
                }

                for (int i = 0; i < mMotions.Length; ++i)
                {
                    var m = mMotions[i];
                    toBlend[i] = m.Sample(playTime);
                    weights[i] = (float)m.AngularDistance;
                }

                return _BlendN(toBlend, weights);
            }
        }

        public class Layer
        {
            public string LayerName;
            public float Weight = 1;
            public BlendNode BlendNode;

            public SkeletonNode[] Nodes;

            public Layer(List<SkeletonNode> list, PersistentAnimationLayer layer)
            {
                Nodes = list.ToArray();
                if (layer != null)
                {
                    var set = new HashSet<string>(layer.AffectedNodes);

                    for (int i = 0; i < Nodes.Length; ++i)
                    {
                        if(!set.Contains(Nodes[i].Name))
                            Nodes[i] = null;
                    }
					if (layer.Weight > 0) Weight = layer.Weight;
                }
                var motion = new Motion(list);
                BlendNode = new AnimationBlendNode(motion);
            }

            public void Blend(double dt)
            {
                var matrix = BlendNode.Blend(dt);
                for (int i = 0; i < Nodes.Length; ++i)
                {
                    var skeletonNode = Nodes[i];
                    if(skeletonNode == null) continue;
					if (matrix[i].M44 == 0) continue;

                    skeletonNode.LocalTransform = skeletonNode.LocalTransform * (1 - Weight) + matrix[i] * Weight;
                }
            }
        }

        public class FloatParameter
        {
            public float Value;
        }
    }
}
