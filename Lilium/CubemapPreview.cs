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
using SharpDX.Direct3D;

namespace Lilium
{
	[CustomSelectedTypeName("Cubemap")]
	public class CubemapPreview : IPreviewable, ISelectable
	{
		enum Mode
		{
			Cube,
			Skybox,
			Sphere,
		}

		public string Name = "";

		Texture2D tex = null;
		ShaderResourceView shaderResourceView;
		int mode;

		Mesh cube;
		Mesh sphere;
		MaterialPass pass;
		SkyBox skybox;

		private Game game;
		public CubemapPreview(Game game, ShaderResourceView view)
		{
			this.game = game;

			if (view != null && view.Description.Dimension == SharpDX.Direct3D.ShaderResourceViewDimension.TextureCube)
			{
				var res = view.Resource;
				tex = res.QueryInterface<Texture2D>();
				(res as IUnknown).Release();
				(tex as IUnknown).Release();

				Name = view.DebugName;
			}
		}
	
		public void PreviewDraw()
		{
			if (shaderResourceView == null) return;
			game.Clear(Color.Black);

			var dc = game.DeviceContext;
			switch ((Mode)mode)
			{
				case Mode.Cube:
					pass.Apply();
					game.UpdatePerObjectBuffer(Matrix.Identity);
					dc.PixelShader.SetShaderResource(0, shaderResourceView);
					cube.DrawBegin();
					cube.DrawSubmesh(0);
					break;
				case Mode.Skybox:
					skybox.Draw();
					break;
				case Mode.Sphere:
					pass.Apply();
					game.UpdatePerObjectBuffer(Matrix.Identity);
					dc.PixelShader.SetShaderResource(0, shaderResourceView);
					sphere.DrawBegin();
					sphere.DrawSubmesh(0);
					break;
				default:
					break;
			}
		}

		public void PreviewActive()
		{
			CreateTextureShaderResourceView();
			if (shaderResourceView == null) return;
			cube = game.ResourceManager.Mesh.Load(InternalResources.MESH_CUBE);
			sphere = game.ResourceManager.Mesh.Load(InternalResources.MESH_SPHERE);

			var passDesc = new MaterialPassDesc();
			passDesc.ManualConstantBuffers = true;
			passDesc.ShaderFile = "CubemapPreview.hlsl";
			pass = new MaterialPass(Game.Instance.Device, passDesc, "Preview");

			skybox = new SkyBox(game, shaderResourceView, "Preview");
			Light.IsPreviewingTexture = true;
		}

		public void PreviewDeactive()
		{
			Light.IsPreviewingTexture = false;
			Utilities.Dispose(ref pass);
			Utilities.Dispose(ref skybox);
			Utilities.Dispose(ref shaderResourceView);
		}

		void CreateTextureShaderResourceView()
		{
			var desc = new ShaderResourceViewDescription();
			desc.Format = tex.Description.Format;
			desc.Dimension = ShaderResourceViewDimension.TextureCube;
			desc.Texture2D.MipLevels = tex.Description.MipLevels;
			desc.Texture2D.MostDetailedMip = 0;
			shaderResourceView = new ShaderResourceView(Game.Instance.Device, tex, desc);
			shaderResourceView.DebugName = "Preview";
		}

		public Controls.Control[] Controls
		{
			get
			{
				var list = new List<Controls.Control>();
				list.Add(new Lilium.Controls.ComboBox("Mode", Enum.GetNames(typeof(Mode)), () => mode, val => mode = val));
				return list.ToArray();
			}
		}

		public string NameInObjectList
		{
			get { return Name; }
		}
	}
}
