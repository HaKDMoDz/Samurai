using System;
using System.Collections.Generic;

namespace Samurai.Graphics.Sprites
{
	public sealed partial class SpriteSheet
	{
		public static SpriteSheet Build(Texture texture, int frameWidth, int frameHeight)
		{
			List<Rectangle> frames = new List<Rectangle>();

			for (int y = 0; y < texture.Height; y += frameHeight)
			{
				if (y + frameHeight > texture.Height)
					break;

				for (int x = 0; x < texture.Width; x += frameWidth)
				{
					if (x + frameWidth > texture.Width)
						break;

					frames.Add(new Rectangle(x, y, frameWidth, frameHeight));
				}
			}

			return new SpriteSheet(texture, frames);
		}
	}
}
