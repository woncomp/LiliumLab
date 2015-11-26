﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Lilium
{
	public class Entity : IDisposable, ISelectable
	{
		public Mesh Mesh { get; private set; }
		public Material[] SubmeshMaterials;

		public Vector3 Position = Vector3.Zero;
		public Vector3 Rotation = Vector3.Zero;
		public Vector3 Scale = Vector3.One;

		public string Name;

		public float StencilShadowIndensity = 0;

		public Matrix TransformMatrix
		{
			get {
				var matTranslation = Matrix.Translation(Position);
				var matRotation = Matrix.RotationYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
				var matScale = Matrix.Scaling(Scale);
				return matScale * matRotation * matTranslation;
			}
		}

		public Entity(Mesh mesh)
		{
			this.Mesh = mesh;
			this.Name = "Entity" + Debug.NextObjectId;
			SubmeshMaterials = new Material[mesh.SubmeshCount];
			CreateControls();
		}

		public Entity(string meshName)
		{
			this.Mesh = Game.Instance.ResourceManager.Mesh.Load(meshName);
			this.Name = "Entity" + Debug.NextObjectId + " " + System.IO.Path.GetFileNameWithoutExtension(meshName);
			SubmeshMaterials = new Material[Mesh.SubmeshCount];
			CreateControls();
		}

		public void SetMaterial(int index, MaterialDesc materialDesc)
		{
			if(SubmeshMaterials.Length <= index)
			{
				var array = new Material[index + 1];
				Array.Copy(SubmeshMaterials, array, SubmeshMaterials.Length);
				SubmeshMaterials = array;
			}
			SubmeshMaterials[index] = new Material(Game.Instance, materialDesc, materialDesc.ResourceName);
			Game.Instance.AutoDispose(SubmeshMaterials[index]);
		}

		public void SetMaterial(int index, string materialName)
		{
			if (SubmeshMaterials.Length <= index)
			{
				var array = new Material[index + 1];
				Array.Copy(SubmeshMaterials, array, SubmeshMaterials.Length);
				SubmeshMaterials = array;
			}
			SubmeshMaterials[index] = Game.Instance.ResourceManager.Material.Load(materialName);
		}

		public void Draw()
		{
			if (Mesh == null) return;

			if (Game.Instance.SelectedObject == this)
			{
				var input = Game.Instance.Input;
				float moveSpeed = 2 * (input.Control ? 10 : 1);
				float moveDelta = moveSpeed * (float)Game.DeltaTime;
				if (input.GetKey(System.Windows.Forms.Keys.J)) Position.X -= moveDelta;
				if (input.GetKey(System.Windows.Forms.Keys.L)) Position.X += moveDelta;
				if (input.GetKey(System.Windows.Forms.Keys.K)) Position.Z -= moveDelta;
				if (input.GetKey(System.Windows.Forms.Keys.I)) Position.Z += moveDelta;
				if (input.GetKey(System.Windows.Forms.Keys.U)) Position.Y -= moveDelta;
				if (input.GetKey(System.Windows.Forms.Keys.O)) Position.Y += moveDelta;

				float rotateDelta = 2 * (float)Game.DeltaTime;
				if (input.GetKey(System.Windows.Forms.Keys.OemOpenBrackets)) Rotation.Y -= rotateDelta;
				if (input.GetKey(System.Windows.Forms.Keys.OemCloseBrackets)) Rotation.Y += rotateDelta;

				float scaleDelta = 1 * (float)Game.DeltaTime;
				if (input.GetKey(System.Windows.Forms.Keys.OemMinus))
				{
					Scale.X -= scaleDelta;
					Scale.Y -= scaleDelta;
					Scale.Z -= scaleDelta;
				}
				if (input.GetKey(System.Windows.Forms.Keys.Oemplus))
				{
					Scale.X += scaleDelta;
					Scale.Y += scaleDelta;
					Scale.Z += scaleDelta;
				}
			}
			Game.Instance.UpdatePerObjectBuffer(TransformMatrix);

			Mesh.DrawBegin();

			for (int i = 0; i < Mesh.SubmeshCount; ++i)
			{
				if (i >= SubmeshMaterials.Length) continue;
				var material = SubmeshMaterials[i];
				if (material == null) material = Game.Instance.ResourceManager.Material.DefaultDiffuse;
				if (!material.IsValid) continue;

				for (int j = 0; j < material.Passes.Length; ++j)
				{
					material.Passes[j].Apply();
					material.Passes[j].UpdateConstantBuffers();
					Mesh.DrawSubmesh(i);
				}
			}

			if (StencilShadowIndensity > 0) DrawStencilShadow();

			if (Config.DrawGizmo && Game.Instance.SelectedObject == this)
			{
				var pV = Vector3.TransformCoordinate(Vector3.Zero, Camera.MainCamera.ViewMatrix);
				var fov2 = MathUtil.DegreesToRadians(Camera.MainCamera.FovDegrees / 2);
				var scale = (float)(pV.Z / Math.Cos(fov2) * Math.Sin(fov2) * 0.2);

				var rot = Matrix.RotationYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);

				Debug.Line(Position, Position + Vector3.TransformNormal(Vector3.UnitX, rot) * scale, Color.Red);
				Debug.Line(Position, Position + Vector3.TransformNormal(Vector3.UnitY, rot) * scale, Color.Green);
				Debug.Line(Position, Position + Vector3.TransformNormal(Vector3.UnitZ, rot) * scale, Color.Blue);
			}
		}

		void DrawStencilShadow()
		{
			Game.Instance.StencilShadowRenderer.Begin(this.TransformMatrix, new Plane(0, 1, 0, -0.001f), StencilShadowIndensity);
			Mesh.DrawBegin();
			for (int j = 0; j < Mesh.SubmeshCount; ++j)
			{
				Mesh.DrawSubmesh(j);
			}
		}

		public void Dispose()
		{
		}

		#region Selectable

		private Controls.Control[] controls;

		void CreateControls()
		{
			List<Lilium.Controls.Control> list = new List<Lilium.Controls.Control>();
			var label1 = new Lilium.Controls.Label("Transform", () => "Move: X:J/L Y:U/O Z:K/I");
			list.Add(label1);
			var label2 = new Lilium.Controls.Label("", () => "Press Control to move faster.");
			list.Add(label2);
			var label3 = new Lilium.Controls.Label("", () => "Rotate:[/] Scale:-/+");
			list.Add(label3);
			var label4 = new Lilium.Controls.Label("Position", () =>Position.ToString("0.000"));
			list.Add(label4);
			var label5 = new Lilium.Controls.Label("Rotation", () => Rotation.ToString("0.000"));
			list.Add(label5);
			var label6 = new Lilium.Controls.Label("Scale", () => Scale.ToString("0.000"));
			list.Add(label6);
			var toggle = new Lilium.Controls.Toggle("Draw Gizmo", () => Config.DrawGizmo, val => Config.DrawGizmo = val);
			list.Add(toggle);
			var slider = new Lilium.Controls.Slider("Stencil Shadow", 0, 1, () => StencilShadowIndensity, val => StencilShadowIndensity = val);
			list.Add(slider);
			for (int i = 0; i < Mesh.SubmeshCount; ++i)
			{
				list.Add(new Lilium.Controls.EntityMaterialSlot(this, i));
			}
			controls = list.ToArray();
		}

		public Controls.Control[] Controls { get { return controls; } }
		public string NameInObjectList { get { return Name; } }

		#endregion
	}
}
