using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lilium
{
    public class PersistentAnimationData
    {
        public List<PersistentAnimationFileInfo> Files { get; set;}
        public List<PersistentAnimationLayer> Layers{ get; set;}
        public Dictionary<string, PersistentAnimationState> States{ get; set;}

        public void Save(string filePath)
        {
            var sb = new StringBuilder();
            using(var sw = new System.IO.StringWriter(sb))
            {
                new YamlDotNet.Serialization.Serializer().Serialize(sw, this);
            }
            System.IO.File.WriteAllText(filePath, sb.ToString());
        }
    }

    public class PersistentAnimationFileInfo
    {
        public string FileName { get; set;}
        public PersistentAnimationCreateInfo[] Clips { get; set;}
    }

    public class PersistentAnimationCreateInfo
    {
        public string AnimationClipName { get; set;}
        public long Start { get; set;}
        public long End { get; set;}
    }

    public class PersistentAnimationLayer
    {
        public string LayerName { get; set;}
		public float Weight { get; set; }
        public string[] AffectedNodes { get; set;}
    }

    public class PersistentAnimationState
    {
        public string Layer { get; set;}
        public AnimationBlendSpaceType BlendSpaceType { get; set;}
        public string ParamNameX { get; set;}
        public string ParamNameY { get; set;}
        public List<PersistentAnimationBlendClip> Clips { get; set;}
    }

    public class PersistentAnimationBlendClip
    {
        public string AnimationClipName { get; set;}
        public float LocationX { get; set;}
        public float LocationY { get; set;}
    }

    public enum AnimationBlendSpaceType
    {
        BS_None,
        BS_1D,
        BS_2D,
    }
}
