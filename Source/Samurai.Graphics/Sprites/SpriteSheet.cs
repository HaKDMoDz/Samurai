using System;
using System.Collections.Generic;

namespace Samurai.Graphics.Sprites
{
	/// <summary>
	/// Wraps a texture and keeps track of the frames within it.
	/// </summary>
	public sealed partial class SpriteSheet
	{
		IList<Rectangle> frames;

		/// <summary>
		/// The texture of the SpriteSheet.
		/// </summary>
		public Texture Texture
		{
			get;
			private set;
		}
				
		/// <summary>
		/// The number of frames in the SpriteSheet.
		/// </summary>
		public int FrameCount
		{
			get { return this.frames.Count; }
		}

		/// <summary>
		/// Retrieves a frame rectangle from the SpriteSheet.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public Rectangle this[int i]
		{
			get { return this.frames[i]; }
		}
				
		/// <summary>
		/// Initializes a new SpriteSheet.
		/// </summary>
		/// <param name="texture"></param>
		/// <param name="frames"></param>
		public SpriteSheet(Texture texture, IList<Rectangle> frames)
		{
			if (texture == null)
				throw new ArgumentNullException("texture");

			if (frames == null)
				throw new ArgumentNullException("rectangles");

			this.Texture = texture;
			this.frames = frames;

			for (int i = 0; i < this.frames.Count; i++)
			{
				if (this.frames[i].Left < 0 ||
					this.frames[i].Top < 0 ||
					this.frames[i].Right > texture.Width ||
					this.frames[i].Bottom > texture.Height)
				{
					throw new InvalidOperationException(string.Format("Frame {0} is outside of the bounds of the sprite texture.", this.frames[i]));
				}
			}
		}
	}
}
