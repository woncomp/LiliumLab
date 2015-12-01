using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Lilium
{
	public static class Debug
	{
		private static int nextObjectId = 0;
		public static int NextObjectId { get { return ++nextObjectId; } }

		private static LineRenderer debugLineRenderer;

		public static void Log(object str)
		{
			System.Diagnostics.Debug.WriteLine(str);
		}

		public static void Line(Vector3 from, Vector3 to, Color color)
		{
			debugLineRenderer.Add(from, to, color);
		}

		public static void Init(Game game)
		{
			debugLineRenderer = new LineRenderer(game, 256, "Debug Line");
			debugLineRenderer.Desc.DepthStencilStates.IsDepthEnabled = false;
			debugLineRenderer.Create3D();
		}

		public static void Shutdown()
		{
			Utilities.Dispose(ref debugLineRenderer);
		}

		public static LineRenderer EDITOR_GetDebugLineRenderer()
		{
			return debugLineRenderer;
		}
	}
}
