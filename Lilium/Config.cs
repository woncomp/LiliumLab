using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lilium
{
	public class Config
	{
		public static bool DebugMode = false;

		public static bool DrawTBN = false;
		public static float TBNOffset = 0;

		public static bool DrawGizmo = true;

		public static bool DrawWireframe = false;

		public static bool LoadMeshComputeTangent = true;
		public static bool LoadMeshPrintLog = true;
	}
}
