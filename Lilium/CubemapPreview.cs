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
			Face_PosX,
			Face_NegX,
			Face_PosY,
			Face_NegY,
			Face_PosZ,
			Face_NegZ,
		}

		public string Name = "";

		Texture2D tex = null;
		ShaderResourceView shaderResourceView;
		ShaderResourceView[] mFaces;
		int mode;

		Mesh cube;
		Mesh sphere;
		MaterialPass pass;
		SkyBox skybox;
		TexturePreviewQuad mQuad;

		private Game mGame;
		public CubemapPreview(Game game, ShaderResourceView view)
		{
			this.mGame = game;

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
			mGame.Clear(Color.Maroon);

			var dc = mGame.DeviceContext;
			switch ((Mode)mode)
			{
				case Mode.Cube:
					pass.Apply();
					mGame.UpdatePerObjectBuffer(Matrix.Identity);
					dc.PixelShader.SetShaderResource(0, shaderResourceView);
					cube.DrawBegin();
					cube.DrawSubmesh(0);
					break;
				case Mode.Skybox:
					skybox.Draw();
					break;
				case Mode.Sphere:
					pass.Apply();
					mGame.UpdatePerObjectBuffer(Matrix.Identity);
					dc.PixelShader.SetShaderResource(0, shaderResourceView);
					sphere.DrawBegin();
					sphere.DrawSubmesh(0);
					break;
				case Mode.Face_PosX:
					mQuad.Draw(mFaces[0]);
					break;
				case Mode.Face_NegX:
					mQuad.Draw(mFaces[1]);
					break;
				case Mode.Face_PosY:
					mQuad.Draw(mFaces[2]);
					break;
				case Mode.Face_NegY:
					mQuad.Draw(mFaces[3]);
					break;
				case Mode.Face_PosZ:
					mQuad.Draw(mFaces[4]);
					break;
				case Mode.Face_NegZ:
					mQuad.Draw(mFaces[5]);
					break;
				default:
					break;
			}
			dc.PixelShader.SetShaderResource(0, null);
		}

		public void PreviewActive()
		{
			var desc = new ShaderResourceViewDescription();
			desc.Format = tex.Description.Format;
			desc.Dimension = ShaderResourceViewDimension.TextureCube;
			desc.Texture2D.MipLevels = tex.Description.MipLevels;
			desc.Texture2D.MostDetailedMip = 0;
			shaderResourceView = new ShaderResourceView(mGame.Device, tex, desc);
			shaderResourceView.DebugName = "Preview";

			if (shaderResourceView == null) return;

			desc.Dimension = ShaderResourceViewDimension.Texture2DArray;
			desc.Texture2DArray.ArraySize = 1;
			desc.Texture2DArray.MipLevels = tex.Description.MipLevels;
			desc.Texture2DArray.MostDetailedMip = 0;
			mFaces = new ShaderResourceView[6];
			for (int i = 0; i < 6; ++i)
			{
				desc.Texture2DArray.FirstArraySlice = i;
				mFaces[i] = new ShaderResourceView(mGame.Device, tex, desc);
			}

			cube = mGame.ResourceManager.Mesh.Load(InternalResources.MESH_CUBE);
			sphere = mGame.ResourceManager.Mesh.Load(InternalResources.MESH_SPHERE);

			var passDesc = new MaterialPassDesc();
			passDesc.ManualConstantBuffers = true;
			passDesc.ShaderFile = "CubemapPreview.hlsl";
			pass = new MaterialPass(Game.Instance.Device, passDesc, "Preview");

			skybox = new SkyBox(mGame, shaderResourceView, "Preview");

			mQuad = new TexturePreviewQuad(mGame, tex.Description);
			Light.IsPreviewingTexture = true;
		}

		public void PreviewDeactive()
		{
			Light.IsPreviewingTexture = false;
			Utilities.Dispose(ref mQuad);
			Utilities.Dispose(ref pass);
			Utilities.Dispose(ref skybox);
			for (int i = 0; i < 6; ++i)
			{
				Utilities.Dispose(ref mFaces[i]);
			}
			Utilities.Dispose(ref shaderResourceView);
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
