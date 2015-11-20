using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Lilium
{
	public partial class Game
	{
		void AutoLoad_Scan()
		{
			var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			foreach (var field in this.GetType().GetFields(flags))
			{
				var attr = (field as MemberInfo).GetCustomAttribute(typeof(AutoLoadAttribute)) as AutoLoadAttribute;
				if(attr != null)
				{
					string path = attr.FilePath;
					if(field.FieldType == typeof(SharpDX.Direct3D11.ShaderResourceView))
					{
						SharpDX.Direct3D11.ShaderResourceView tex = null;
						LoadTexture(path, out tex);
						field.SetValue(this, tex);
					}
					else if(field.FieldType == typeof(Mesh))
					{
						Mesh mesh = null;
						LoadMesh(path, out mesh);
						field.SetValue(this, mesh);
					}
					else
					{
						System.Diagnostics.Debug.WriteLine("Failed to load '" + attr.FilePath + "' for " + field.Name + ", Unsupported var type.");
					}
				}
			}

		}
	}

	public class AutoLoadAttribute : System.Attribute
	{
		public string FilePath { get; private set; }
		public AutoLoadAttribute(string filePath)
		{
			FilePath = filePath;
		}
	}
}
