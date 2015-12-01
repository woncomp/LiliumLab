using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Lilium
{
	public class FrustumRenderer : IDisposable
	{
		public BoundingFrustum Frustum;

		private bool isDirty;
		private LineRenderer line;

		private Game game;
		private string debugName;

		public FrustumRenderer(Game game, BoundingFrustum frustum, string debugName = null)
		{
			this.game = game;
			this.debugName = debugName ?? "FrustumRenderer " + Debug.NextObjectId;

			line = new LineRenderer(game, 12, this.debugName);
			UpdateFrustum(frustum);
		}

		public void UpdateFrustum(BoundingFrustum frustum)
		{
			this.Frustum = frustum;
			this.isDirty = true;
		}

		public void Create3D()
		{
			line.Create3D();
		}

		public void Draw()
		{
			if(isDirty)
			{
				//     Returns the 8 corners of the frustum, element0 is Near1 (near right down
				//     corner) , element1 is Near2 (near right top corner) , element2 is Near3 (near
				//     Left top corner) , element3 is Near4 (near Left down corner) , element4 is
				//     Far1 (far right down corner) , element5 is Far2 (far right top corner) ,
				//     element6 is Far3 (far left top corner) , element7 is Far4 (far left down
				//     corner)
				line.Clear();
				var corners = Frustum.GetCorners();
				Add3(corners, 0, 1);
				Add3(corners, 1, 2);
				Add3(corners, 2, 3);
				Add3(corners, 3, 0);
				isDirty = false;
			}
			line.Draw();
		}

		private void Add3(Vector3[] corners, int i1, int i2)
		{
			line.Add(corners[i1], corners[i2], Color.Black);
			line.Add(corners[i1 + 4], corners[i2 + 4], Color.Black);
			line.Add(corners[i1], corners[i1 + 4], Color.Black);
		}

		public void Dispose()
		{
			Utilities.Dispose(ref line);
		}
	}
}
