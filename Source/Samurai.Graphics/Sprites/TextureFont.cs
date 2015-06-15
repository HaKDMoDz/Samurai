using System;
using System.Collections.Generic;
using System.Linq;

namespace Samurai.Graphics.Sprites
{
	public sealed partial class TextureFont : DisposableObject
	{
		public const string DefaultFontName = "Unknown";

		public const int DefaultFontSize = 12;

		public const int DefaultCharacterSpacing = 2;

		public const int DefaultLineSpacing = 0;

		private static readonly char[] CharsToExclude = new char[] { '\r' };

		IDictionary<char, Rectangle> rectangles;

		public Texture Texture
		{
			get;
			private set;
		}

		public int LineHeight
		{
			get;
			private set;
		}

		public string FontName
		{
			get;
			private set;
		}

		public int FontSize
		{
			get;
			private set;
		}

		/// <summary>
		/// The amount of space to use between each character when rendering a string.
		/// </summary>
		public int CharacterSpacing
		{
			get;
			set;
		}

		/// <summary>
		/// The amount of space to use between each line when rendering a string.
		/// </summary>
		public int LineSpacing
		{
			get;
			set;
		}

		public Rectangle this[char ch]
		{
			get { return this.rectangles[ch]; }
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="texture">The texture used by the font.</param>
		/// <param name="rectangles">Dictionary of characters to rectangles.</param>
		public TextureFont(Texture texture, IDictionary<char, Rectangle> rectangles)
		{
			if (texture == null)
				throw new ArgumentNullException("texture");

			if (rectangles == null)
				throw new ArgumentNullException("rectangles");

			this.Texture = texture;
			this.rectangles = rectangles;
			this.CharacterSpacing = DefaultCharacterSpacing;
			this.LineSpacing = DefaultLineSpacing;

			foreach (Rectangle rectangle in this.rectangles.Values)
				if (rectangle.Height > this.LineHeight)
					this.LineHeight = rectangle.Height;
		}

		protected override void DisposeManagedResources()
		{
			this.Texture.Dispose();
		}

		/// <summary>
		/// Returns true if the TextureFont can render the given character.
		/// </summary>
		/// <param name="ch"></param>
		/// <returns></returns>
		public bool ContainsCharacter(char ch)
		{
			return this.rectangles.ContainsKey(ch);
		}

		/// <summary>
		/// Measures the given string.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public Size MeasureString(string s)
		{
			return this.MeasureString(s, 0, s.Length);
		}

		/// <summary>
		/// Measures the size of the string.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="start">The index of the string at which to start measuring.</param>
		/// <param name="length">How many characters to measure from the start.</param>
		/// <returns></returns>
		public Size MeasureString(string s, int start, int length)
		{
			if (start < 0 || start > s.Length)
				throw new ArgumentOutOfRangeException("start", "Start is not an index within the string.");

			if (length < 0)
				throw new ArgumentOutOfRangeException("length", "Length must me >= 0.");

			if (start + length > s.Length)
				throw new ArgumentOutOfRangeException("length", "Start + length is greater than the string's length.");

			Size size = Size.Zero;

			size.Height = this.LineHeight;

			int lineWidth = 0;
			for (int i = start; i < length; i++)
			{
				if (s[i] == '\n')
				{
					if (lineWidth > size.Width)
						size.Width = lineWidth;

					lineWidth = 0;

					size.Height += this.LineHeight + this.LineSpacing;
				}
				else if (!CharsToExclude.Contains(s[i]))
				{
					lineWidth += this.rectangles[s[i]].Width + this.CharacterSpacing;
				}
			}

			if (lineWidth > size.Width)
				size.Width = lineWidth;

			return size;
		}
	}
}
