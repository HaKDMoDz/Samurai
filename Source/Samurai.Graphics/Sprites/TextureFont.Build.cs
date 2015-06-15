using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace Samurai.Graphics.Sprites
{
	public sealed partial class TextureFont
	{
		public static TextureFont Build(GraphicsContext graphics, string fontName, float fontSize, TextureFontParams parameters)
		{
			Font font = new Font(fontName, fontSize, TextureFontStyleToFontStyle(parameters.Style), GraphicsUnit.Pixel);

			System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(new Bitmap(1, 1, PixelFormat.Format32bppArgb));

			List<Bitmap> charBitmaps = new List<Bitmap>(parameters.MaxChar - parameters.MinChar);
			Dictionary<char, Rectangle> rectangles = new Dictionary<char, Rectangle>();

			int bitmapWidth = 0;
			int bitmapHeight = 0;
			int lineHeight = 0;
			int rows = 1;

			int count = 0;
			int x = 0;
			int y = 0;
			const int padding = 4;

			int spaceCharBitmapIndex = -1;

			Color4 color4 = parameters.Color;
			Color color = Color.FromArgb(color4.A, color4.R, color4.G, color4.B);

			Color4 backgroundColor4 = parameters.BackgroundColor;
			Color backgrounColor = Color.FromArgb(backgroundColor4.A, backgroundColor4.R, backgroundColor4.G, backgroundColor4.B);
			
			for (char ch = parameters.MinChar; ch < parameters.MaxChar; ch++)
			{
				Bitmap charBitmap = RenderChar(g, font, ch, color, backgrounColor, parameters.Antialias);

				charBitmaps.Add(charBitmap);

				x += charBitmap.Width + padding;

				if (ch != ' ')
				{
					lineHeight = Math.Max(lineHeight, charBitmap.Height);
				}
				else
				{
					spaceCharBitmapIndex = charBitmaps.Count - 1;
				}

				count++;
				if (count >= 16)
				{
					bitmapWidth = Math.Max(bitmapWidth, x);
					rows++;
					x = 0;
					count = 0;
				}
			}

			int top = lineHeight;
			int bottom = 0;

			for (int i = 0; i < charBitmaps.Count; i++)
			{
				if (i != spaceCharBitmapIndex)
				{
					Bitmap charBitmap = charBitmaps[i];

					int charTop = FindTopOfChar(charBitmap, backgrounColor);

					if (charTop < top)
						top = charTop;

					int charBottom = FindBottomOfChar(charBitmap, backgrounColor);

					if (charBottom > bottom)
						bottom = charBottom;
				}
			}

			lineHeight = bottom - top;

			for (int i = 0; i < charBitmaps.Count; i++)
			{
				charBitmaps[i] = CropCharHeight(charBitmaps[i], top, bottom);
			}

			bitmapHeight = (lineHeight * rows) + (padding * rows);

			using (Bitmap bitmap = new Bitmap(bitmapWidth, bitmapHeight, PixelFormat.Format32bppArgb))
			{
				using (System.Drawing.Graphics bitmapGraphics = System.Drawing.Graphics.FromImage(bitmap))
				{
					bitmapGraphics.Clear(backgrounColor);

					count = 0;
					x = 0;
					y = 0;

					char ch = parameters.MinChar;
					for (int i = 0; i < charBitmaps.Count; i++)
					{
						int offset = (lineHeight - charBitmaps[i].Height);
						bitmapGraphics.DrawImage(charBitmaps[i], x, y + offset);

						rectangles.Add(ch, new Rectangle(x, y, charBitmaps[i].Width, lineHeight));
						ch++;

						x += charBitmaps[i].Width + padding;
						charBitmaps[i].Dispose();

						count++;
						if (count >= 16)
						{
							x = 0;
							y += lineHeight + padding;
							count = 0;
						}
					}
				}

				byte[] bytes = BitmapHelper.GetBytes(bitmap);
				
				Texture texture = Texture.LoadFromBytes(graphics, bytes, bitmap.Width, bitmap.Height, new TextureParams()
					{
						ColorKey = parameters.ColorKey
					});
				
				return new TextureFont(texture, rectangles);
			}
		}

		private static FontStyle TextureFontStyleToFontStyle(TextureFontStyle style)
		{
			FontStyle fontStyle = FontStyle.Regular;

			if (style.HasFlag(TextureFontStyle.Bold))
				fontStyle = fontStyle & FontStyle.Bold;

			if (style.HasFlag(TextureFontStyle.Italic))
				fontStyle = fontStyle & FontStyle.Italic;

			if (style.HasFlag(TextureFontStyle.Strikeout))
				fontStyle = fontStyle & FontStyle.Strikeout;

			if (style.HasFlag(TextureFontStyle.Underline))
				fontStyle = fontStyle & FontStyle.Underline;

			return fontStyle;
		}

		private static Bitmap RenderChar(System.Drawing.Graphics graphics, Font font, char ch, Color color, Color backgroundColor, bool antialias)
		{
			string text = ch.ToString();
			SizeF size = graphics.MeasureString(text, font);

			int charWidth = (int)Math.Ceiling(size.Width);
			int charHeight = (int)Math.Ceiling(size.Height);

			Bitmap charBitmap = new Bitmap(charWidth, charHeight, PixelFormat.Format32bppArgb);

			using (System.Drawing.Graphics bitmapGraphics = System.Drawing.Graphics.FromImage(charBitmap))
			{
				if (antialias)
				{
					bitmapGraphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
				}
				else
				{
					bitmapGraphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
				}

				bitmapGraphics.Clear(backgroundColor);

				using (Brush brush = new SolidBrush(color))
				using (StringFormat format = new StringFormat())
				{
					format.Alignment = StringAlignment.Near;
					format.LineAlignment = StringAlignment.Near;

					bitmapGraphics.DrawString(text, font, brush, 0, 0, format);
				}

				bitmapGraphics.Flush();
			}

			if (ch != ' ')
				charBitmap = CropCharWidth(charBitmap, backgroundColor);

			return charBitmap;
		}

		private static bool AreColorsEqual(Color c1, Color c2)
		{
			return c1.A == c2.A &&
				   c1.R == c2.R &&
				   c1.G == c2.G &&
				   c1.B == c2.B;
		}

		private static int FindLeftOfChar(Bitmap charBitmap, Color backgrounColor)
		{
			for (int x = 0; x < charBitmap.Width; x++)
			{
				for (int y = 0; y < charBitmap.Height; y++)
				{
					if (!AreColorsEqual(charBitmap.GetPixel(x, y), backgrounColor))
						return x;
				}
			}

			return 0;
		}

		private static int FindRightOfChar(Bitmap charBitmap, Color backgrounColor)
		{
			for (int x = charBitmap.Width - 1; x >= 0; x--)
			{
				for (int y = 0; y < charBitmap.Height; y++)
				{
					if (!AreColorsEqual(charBitmap.GetPixel(x, y), backgrounColor))
						return x;
				}
			}

			return 0;
		}

		private static int FindTopOfChar(Bitmap charBitmap, Color backgrounColor)
		{
			for (int y = 0; y < charBitmap.Height; y++)
			{
				for (int x = 0; x < charBitmap.Width; x++)
				{
					if (!AreColorsEqual(charBitmap.GetPixel(x, y), backgrounColor))
						return y;
				}
			}

			return 0;
		}

		private static int FindBottomOfChar(Bitmap charBitmap, Color backgrounColor)
		{
			for (int y = charBitmap.Height - 1; y >= 0; y--)
			{
				for (int x = 0; x < charBitmap.Width; x++)
				{
					if (!AreColorsEqual(charBitmap.GetPixel(x, y), backgrounColor))
						return y;
				}
			}

			return charBitmap.Height;
		}

		/// <summary>
		/// Removes the left and right blank space of a character bitmap.
		/// </summary>
		/// <param name="charBitmap"></param>
		/// <returns></returns>
		private static Bitmap CropCharWidth(Bitmap charBitmap, Color backgrounColor)
		{
			int left = FindLeftOfChar(charBitmap, backgrounColor);
			int right = FindRightOfChar(charBitmap, backgrounColor);

			// We can't crop or don't need to crop
			if (left > right || (left == 0 && right == charBitmap.Width - 1))
				return charBitmap;

			Bitmap croppedBitmap = new Bitmap((right - left) + 1, charBitmap.Height, PixelFormat.Format32bppArgb);

			using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(croppedBitmap))
			{
				graphics.CompositingMode = CompositingMode.SourceCopy;

				RectangleF dest = new RectangleF(0, 0, (right - left) + 1, charBitmap.Height);
				RectangleF src = new RectangleF(left, 0, (right - left) + 1, charBitmap.Height);
				graphics.DrawImage(charBitmap, dest, src, GraphicsUnit.Pixel);
				graphics.Flush();
			}

			return croppedBitmap;
		}

		private static Bitmap CropCharHeight(Bitmap charBitmap, int top, int bottom)
		{
			Bitmap croppedBitmap = new Bitmap(charBitmap.Width, bottom - top + 1, PixelFormat.Format32bppArgb);

			using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(croppedBitmap))
			{
				graphics.CompositingMode = CompositingMode.SourceCopy;

				RectangleF dest = new RectangleF(0, 0, croppedBitmap.Width, croppedBitmap.Height);
				RectangleF src = new RectangleF(0, top, croppedBitmap.Width, croppedBitmap.Height);
				graphics.DrawImage(charBitmap, dest, src, GraphicsUnit.Pixel);
				graphics.Flush();
			}

			return croppedBitmap;
		}

		private static byte HexDigitToByte(char c)
		{
			switch (c)
			{
				case '0': return (byte)0;
				case '1': return (byte)1;
				case '2': return (byte)2;
				case '3': return (byte)3;
				case '4': return (byte)4;
				case '5': return (byte)5;
				case '6': return (byte)6;
				case '7': return (byte)7;
				case '8': return (byte)8;
				case '9': return (byte)9;
				case 'A': return (byte)10;
				case 'B': return (byte)11;
				case 'C': return (byte)12;
				case 'D': return (byte)13;
				case 'E': return (byte)14;
				case 'F': return (byte)15;
			}

			return (byte)0;
		}

		private static Color ColorFromHexString(string hex)
		{
			if (string.IsNullOrEmpty(hex) || hex.Length != 8)
				return Color.Black;

			hex = hex.ToUpper();

			int r = (HexDigitToByte(hex[0]) << 4) + HexDigitToByte(hex[1]);
			int g = (HexDigitToByte(hex[2]) << 4) + HexDigitToByte(hex[3]);
			int b = (HexDigitToByte(hex[4]) << 4) + HexDigitToByte(hex[5]);
			int a = (HexDigitToByte(hex[6]) << 4) + HexDigitToByte(hex[7]);

			return Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);
		}
	}
}
