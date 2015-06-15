using System;
using System.Collections.Generic;
using Samurai;
using Samurai.GLFW;
using Samurai.Graphics;
using Samurai.Graphics.Sprites;

namespace Planes
{
	public class PlanesSample : Game
	{
		SpriteRenderer spriteRenderer;
		BasicSpriteShaderProgram shaderProgram;
		Texture planesTexture;
		SpriteSheet planeSpriteSheet;
		TextureFont font;
				
		List<Plane> planes;

		const int PlaneCount = 100;

		int fps = 0;
		int fpsCount;
		float fpsTimer;

		public PlanesSample()
            : base()
		{
			this.Window.Title = "Samurai Planes Sample";

			this.spriteRenderer = new SpriteRenderer(this.Graphics);
			this.shaderProgram = new BasicSpriteShaderProgram(this.Graphics);
						
			this.planesTexture = Texture.LoadFromFile(this.Graphics, "Planes.png", new TextureParams()
				{
				});

			this.planeSpriteSheet = SpriteSheet.Build(this.planesTexture, 64, 64);

			this.font = TextureFont.Build(this.Graphics, "Segoe UI", 72, new TextureFontParams()
				{
					Color = Color4.White,
					BackgroundColor = Color4.Transparent,
				});
						
			Random random = new Random();

			this.planes = new List<Plane>();

			for (int i = 0; i < PlaneCount; i++)
			{
				this.planes.Add(new Plane(
					random.Next(4) * 3,
					new Vector2(random.Next(this.Window.Width), random.Next(this.Window.Height)),
					(float)random.Next(360),
					new Size(this.Window.Width, this.Window.Height)
				));
			}
		}

		protected override void Update(TimeSpan elapsed)
		{
			foreach (Plane plane in this.planes)
			{
				plane.Update(elapsed);
			}
		}

		protected override void Draw(TimeSpan elapsed)
		{
			this.Graphics.Clear(Color4.Black);

			this.spriteRenderer.Begin(this.shaderProgram);

			string text = string.Format("FPS: {0}", fps);

			Size textSize = this.font.MeasureString(text);
			int halfWindowWidth = this.Window.Width / 2;
			int halfWindowHeight = this.Window.Height / 2;
			int halfTextWidth = textSize.Width / 2;
			int halfTextHeight = textSize.Height / 2;

			this.spriteRenderer.DrawString(
				this.font,
				text,
				new Rectangle(halfWindowWidth - halfTextWidth, halfWindowHeight - halfTextHeight, textSize.Width, textSize.Height),
				tint: Color4.CornflowerBlue,
				origin: new Vector2(halfTextWidth, halfTextHeight),
				rotation: MathHelper.ToRadians(45.0f),
				layerDepth: 0.5f);

			foreach (Plane plane in this.planes)
			{
				this.spriteRenderer.Draw(
					this.planeSpriteSheet,
					plane.StartFrame + plane.FrameOffset,
					plane.Position,
					origin: new Vector2(32, 32),
					rotation: MathHelper.ToRadians(plane.Rotation)
				);
			}

			this.spriteRenderer.End();

			this.Graphics.SwapBuffers();

			this.fpsCount++;
			this.fpsTimer += (float)elapsed.TotalSeconds;
			if (this.fpsTimer >= 1.0f)
			{
				this.fps = this.fpsCount;
				this.fpsTimer -= 1.0f;
				this.fpsCount = 0;
			}
		}

		private static void Main(string[] args)
		{
			using (PlanesSample game = new PlanesSample())
				game.Run();
		}
	}
}
