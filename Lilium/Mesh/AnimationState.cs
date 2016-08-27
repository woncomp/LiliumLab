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
    public class AnimationState
    {
        public AnimationClip Animation;

        public SkinnedMesh Mesh {get; set; }

        Dictionary<string, BlendNode> BlendNodes = new Dictionary<string,BlendNode>();

        public double AnimationTime = 0;

        private List<SkeletonNode> skeletonNodeList = new List<SkeletonNode>();
        private Dictionary<string, SkeletonNode> skeletonNodeDict = new Dictionary<string,SkeletonNode>();
        
        public AnimationState(SkinnedMesh m)
        {
            this.Mesh = m;

            foreach (var node in m.skeleton)
            {
                var name = node.Name;
                skeletonNodeDict[name] = node;
                skeletonNodeList.Add(node);
                BlendNodes[name] = new PoseBlendNode(node);
            }
        }

        public void PlayAnimation(AnimationClip anim, float transitionTime)
        {
            Animation = anim;
            foreach (var node in skeletonNodeList)
            {
                var name = node.Name;
                AnimationNodeChannel channel;
				anim.Channels.TryGetValue(name, out channel);
                BlendNode newBlendNode = null;
                if (channel == null)
                {
                    newBlendNode = new PoseBlendNode(node);
                }
                else
                {
                    newBlendNode = new AnimationBlendNode(channel);
                }
                newBlendNode.FadingOut = BlendNodes[name];
                newBlendNode.TotalTransitionTime = transitionTime;
                BlendNodes[name] = newBlendNode;
            }
        }

        public void Update(double dt)
        {
            if(Animation == null) return;
            foreach (var node in skeletonNodeList)
            {
                var blendNode = BlendNodes[node.Name];
                node.LocalTransform = blendNode.Blend(dt);
            }
        }

        class BlendNode
        {
            public Matrix Transform;

            public double CurrentTransitionTime;
            public double TotalTransitionTime;
            public BlendNode FadingOut;
            
            public Matrix Blend(double dt)
            {
                Tick(dt);
                if(FadingOut != null)
                {
                    CurrentTransitionTime += dt;
                    var FadingWeight = (float)(CurrentTransitionTime / TotalTransitionTime);
                    if(FadingWeight >= 1)
                    {
                        FadingOut = null;
                        return Transform;
                    }
                    else
                        return Transform * FadingWeight + FadingOut.Blend(dt) * (1 - FadingWeight);
                }
                else
                {
                    return Transform;
                }
            }

            public virtual void Tick(double dt)
            {

            }
        }

        class PoseBlendNode : BlendNode
        {
            public SkeletonNode Node;

            public PoseBlendNode(SkeletonNode node)
            {
                Node = node;
            }

            public override void Tick(double dt)
            {
                Transform = Node.PoseMatrix;
            }
        }

        class AnimationBlendNode : BlendNode
        {
			public AnimationNodeChannel Channel;

            private double playTime;

			public AnimationBlendNode(AnimationNodeChannel channel)
            {
                Channel = channel;
            }

			public override void Tick(double dt)
			{
				var animLength = Channel.Clip.Duration;
				var frameLength = Channel.Clip.SecondsPerFrame;

				playTime += dt;
				var t = playTime % animLength;
				if (t <= 0) t += animLength;
				var frameIndex = (int)Math.Floor(t / frameLength);
				if(frameIndex >= Channel.Frames.Length-1)
				{
					Transform = Channel.Frames[Channel.Frames.Length - 1];
					return;
				}

				var frameRemaining = t - frameIndex * frameLength;
				var lerpFactor = frameRemaining / frameLength;
				Transform = Matrix.Lerp(Channel.Frames[frameIndex], Channel.Frames[frameIndex + 1], (float)lerpFactor);
			}

			//public override void Tick(double dt)
			//{
			//	playTime += dt;
			//	var t = playTime % Length;
			//	if(t < 0) t += Length;
			//	t *= TicksPerSecond;
			//	Vector3 translation, scale; Quaternion rotation;
			//	Node.PoseMatrix.Decompose(out scale, out rotation, out translation);
			//	if(Channel.HasPositionKeys) translation = SkinnedMesh.AssimpConvertVector3(Sample(Channel.PositionKeys.ToArray(), t));
			//	if(Channel.HasRotationKeys) rotation = SkinnedMesh.AssimpConvertQuaternion(Sample(Channel.RotationKeys.ToArray(), t));
			//	if(Channel.HasScalingKeys) scale = SkinnedMesh.AssimpConvertVector3(Sample(Channel.ScalingKeys.ToArray(), t));
                
			//	var matTranslation = Matrix.Translation(translation);
			//	var matRotation = Matrix.RotationQuaternion(rotation);
			//	var matScale = Matrix.Scaling(scale);
			//	Transform = matScale * matRotation * matTranslation;
			//}
        }

		//static Assimp.Vector3D Sample(Assimp.VectorKey[] keys, double time)
		//{
		//	if(time < keys[0].Time) return keys[0].Value;
		//	for(int i=0;i<keys.Length;++i)
		//	{
		//		var key1 =keys[i]; 
		//		if(time < key1.Time)
		//		{
		//			var key0 = keys[i - 1];
		//			var t0 = time - key0.Time;
		//			var t1 = key1.Time - key0.Time;
		//			if(t1 == 0) return key1.Value;
		//			var t = (float)(t0 / t1);
		//			return key0.Value * t + key1.Value * (1 - t);
		//		}
		//	}
		//	return keys[keys.Length-1].Value;
		//}

		//static Assimp.Quaternion Sample(Assimp.QuaternionKey[] keys, double time)
		//{
		//	if(time < keys[0].Time) return keys[0].Value;
		//	for(int i=0;i<keys.Length;++i)
		//	{
		//		var key1 =keys[i]; 
		//		if(time < key1.Time)
		//		{
		//			var key0 = keys[i - 1];
		//			var t0 = time - key0.Time;
		//			var t1 = key1.Time - key0.Time;
		//			if(t1 == 0) return key1.Value;
		//			var t = (float)(t0 / t1);
		//			return Assimp.Quaternion.Slerp(key0.Value, key1.Value, t);
		//		}
		//	}
		//	return keys[keys.Length-1].Value;
		//}
    }
}
