using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Cyotek.Drawing.BitmapFont;

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Lilium
{
	public class UILabel : UIWidget
	{
		public UIFont Font;

		public string Text
		{
			get { return mText; }
			set
			{
				if (mText == value) return;
				mText = value;
				SetDirty();
			}
		}

		private string mText;

		public UILabel()
		{
		}

		public void SetFont(string fontName)
		{
			Font = Game.Instance.ResourceManager.Font.Load(fontName);
		}

		public override void FillGeometry(List<UIVertex> vertices, List<uint> indices)
		{
			if (mText == null) return;
			var _font = Font.BMFont;
			var previousCharacter = ' ';
			var normalizedText = _font.NormalizeLineBreaks(mText);
			var size = _font.MeasureFont(normalizedText);

			if (size.Height != 0 && size.Width != 0)
			{
				float x = Position.X;
				float y = Position.Y;
				float scale = Scale;

				foreach (char character in normalizedText)
				{
					switch (character)
					{
						case '\n':
							x = 0;
							y += _font.LineHeight * scale;
							break;
						default:
							Character data;
							int kerning;

							data = _font[character];
							kerning = _font.GetKerning(previousCharacter, character);

							var pixelX0 = x + (data.Offset.X + kerning) * scale;
							var pixelX1 = pixelX0 + data.Bounds.Width * scale;
							var pixelY0 = y + data.Offset.Y * scale;
							var pixelY1 = pixelY0 + data.Bounds.Height * scale;
							
							float surfW = Surface.Width;
							float surfH = Surface.Height;
							var posX0 = pixelX0 * 2 / surfW - 1f;
							var posX1 = pixelX1 * 2 / surfW - 1f;
							var posY0 = 1f - pixelY0 * 2 / surfH;
							var posY1 = 1f - pixelY1 * 2 / surfH;

							float fontTexW = _font.TextureSize.Width;
							float fontTexH = _font.TextureSize.Height;
							var u0 = data.Bounds.Left / fontTexW;
							var u1 = data.Bounds.Right / fontTexW;
							var v0 = data.Bounds.Top / fontTexH;
							var v1 = data.Bounds.Bottom / fontTexH;

							uint vertexOffset = (uint)vertices.Count;
							indices.Add(vertexOffset + 0);
							indices.Add(vertexOffset + 2);
							indices.Add(vertexOffset + 1);
							indices.Add(vertexOffset + 1);
							indices.Add(vertexOffset + 2);
							indices.Add(vertexOffset + 3);

							FillVertex(vertices, posX0, posY0, u0, v0);
							FillVertex(vertices, posX0, posY1, u0, v1);
							FillVertex(vertices, posX1, posY0, u1, v0);
							FillVertex(vertices, posX1, posY1, u1, v1);

							x += (data.XAdvance + kerning) * scale;
							break;
					}

					previousCharacter = character;
				}
			}
		}

		void FillVertex(List<UIVertex> vertices, float posX, float posY, float u, float v)
		{
			vertices.Add(new UIVertex()
				{
					Position = new Vector3(posX, posY, 0),
					TexCoord = new Vector2(u, v),
				});
		}
	}
}
