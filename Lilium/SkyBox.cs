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
	public class SkyBox : IDisposable
	{
		struct Data
		{
			public Matrix matWorld2;
		}

		Mesh cube;
		MaterialPass pass;
		Buffer buffer;

		ShaderResourceView cubeMap;

		Game game;
	
		public SkyBox(Game game, ShaderResourceView cubeMap, string debugName = null)
		{
			this.game = game;
			this.cubeMap = cubeMap;
			cube = game.ResourceManager.Mesh.Load(InternalResources.MESH_CUBE);

			MaterialPassDesc desc = new MaterialPassDesc();
			desc.ManualConstantBuffers = true;
			desc.ShaderFile = "Skybox.hlsl";
			desc.RasteriazerStates.CullMode = CullMode.Front;
			desc.DepthStencilStates.DepthWriteMask = DepthWriteMask.Zero;

			pass = new MaterialPass(game.Device, desc, debugName ?? "Skybox" + Debug.NextObjectId);
			buffer = Material.CreateBuffer<Data>();
		}

		public void Draw()
		{
			pass.Apply();

			var data = new Data();
			data.matWorld2 = Matrix.Translation(Camera.ActiveCamera.Position);

			var dc = game.DeviceContext;
			dc.UpdateSubresource(ref data, buffer);
			dc.VertexShader.SetConstantBuffer(0, buffer);
			dc.PixelShader.SetConstantBuffer(0, buffer);
			dc.PixelShader.SetShaderResource(0, cubeMap);
			
			cube.DrawBegin();
			cube.DrawSubmesh(0);
		}

		public void Dispose()
		{
			cube.Dispose();
			pass.Dispose();
			buffer.Dispose();
		}
	}
}
