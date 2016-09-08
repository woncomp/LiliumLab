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
	public class AnimationClip
	{
		public string Name;
		public double SecondsPerFrame;
		public double Duration;
		public Dictionary<string, AnimationNodeChannel> Channels = new Dictionary<string,AnimationNodeChannel>();
	}

	public class AnimationNodeChannel
	{
		public AnimationClip Clip;

		public Matrix[] Frames;

		public AnimationNodeChannel(long frameCount)
		{
			Frames = new Matrix[frameCount];
		}
	}

	public class AnimationClipCreateInfo
	{
		public string Name;
		public long Start, End;

		public AnimationClipCreateInfo(string name, long start, long end)
		{
			Name = name;
			Start = start;
			End = end;
		}
	}

	public class AnimationClipCreateInfoList
	{
		public List<AnimationClipCreateInfo> Items = new List<AnimationClipCreateInfo>();

		public void Add(string name, long start, long end)
		{
			var info = new AnimationClipCreateInfo(name, start, end);
			Items.Add(info);
		}

		public static implicit operator AnimationClipCreateInfo[](AnimationClipCreateInfoList l)
		{
			return l.Items.ToArray();
		}

        public static implicit operator PersistentAnimationCreateInfo[](AnimationClipCreateInfoList l)
        {
            return l.Items.Select(c =>
                new PersistentAnimationCreateInfo()
                {
                    AnimationClipName = c.Name,
                    Start = c.Start,
                    End = c.End
                }).ToArray();
        }
	}
}
