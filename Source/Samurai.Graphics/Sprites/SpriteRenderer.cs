using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Samurai.Graphics.Sprites
{
	public sealed class SpriteRenderer : DisposableObject
	{		
		[StructLayout(LayoutKind.Sequential)]
		struct Vertex
		{
			public Vector3 Position;
			public Color4 Color;
			public Vector2 UV;
		}

		GraphicsContext graphics;

		Vertex[] vertices;
		int vertexCount;
		Texture texture;

		DynamicVertexBuffer<Vertex> vertexBuffer;
		StaticIndexBuffer<ushort> indexBuffer;

		ISpriteShaderProgram shader;
		bool drawInProgress;

		Matrix4 transform;
														
		public SpriteRenderer(GraphicsContext graphics)
            : this(graphics, 1024)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="maxSprites">The maximum number of sprites which can be batched.</param>
		public SpriteRenderer(GraphicsContext graphics, int maxSprites)
		{
			if (graphics == null)
				throw new ArgumentNullException("graphics");

			if (maxSprites <= 0)
				throw new ArgumentOutOfRangeException("maxSprites", "MaxSprites must be >= 1.");

			this.graphics = graphics;

			this.vertices = new Vertex[maxSprites * 4];

			this.vertexBuffer = new DynamicVertexBuffer<Vertex>(this.graphics);

			ushort[] indices = new ushort[1024 * 6];
			for (ushort i = 0, vertex = 0; i < indices.Length; i += 6, vertex += 4)
			{
				indices[i] = vertex;
				indices[i + 1] = (ushort)(vertex + 1);
				indices[i + 2] = (ushort)(vertex + 3);
				indices[i + 3] = (ushort)(vertex + 1);
				indices[i + 4] = (ushort)(vertex + 2);
				indices[i + 5] = (ushort)(vertex + 3);
			}

			this.indexBuffer = new StaticIndexBuffer<ushort>(this.graphics, indices);

			this.transform = new Matrix4()
			{
				M33 = 1f,
				M44 = 1f,
				M41 = -1f,
				M42 = 1f
			};
		}

		protected override void DisposeManagedResources()
		{
			this.vertexBuffer.Dispose();
			this.vertexBuffer = null;

			this.indexBuffer.Dispose();
			this.indexBuffer = null;
		}
		
		private void EnsureDrawInProgress()
		{
			if (!this.drawInProgress)
				throw new InvalidOperationException("Draw not currently in progress.");
		}

		public void Begin(ISpriteShaderProgram shader)
		{
			if (shader == null)
				throw new ArgumentNullException("shader");

			if (this.drawInProgress)
				throw new InvalidOperationException("Draw already in progress.");

			this.shader = shader;
						
			this.graphics.BlendEnabled = true;
			this.graphics.SourceBlendFactor = SourceBlendFactor.SourceAlpha;
			this.graphics.DestinationBlendFactor = DestinationBlendFactor.OneMinusSourceAlpha;

			this.graphics.DepthBufferEnabled = true;
			this.graphics.DepthFunction = DepthFunction.LessThanOrEqual;

			this.drawInProgress = true;
		}

		public void End()
		{
			this.EnsureDrawInProgress();

			this.Flush();		
			
			this.shader = null;
			this.drawInProgress = false;
		}

		private Vector2 CalculateUV(float x, float y)
		{
			Vector2 uv = Vector2.Zero;

			if (this.texture.Width != 1 || this.texture.Height != 1)
			{
				uv = new Vector2(x / (float)this.texture.Width, y / (float)this.texture.Height);
			}

			return uv;
		}

		private void AddQuad(
			ref Vector2 topLeft,
			ref Vector2 topRight,
			ref Vector2 bottomRight,
			ref Vector2 bottomLeft,
			Rectangle source,
			Color4 color,
			float layerDepth)
		{
			if (this.vertexCount == this.vertices.Length)
				this.Flush();

			this.vertices[this.vertexCount].Position = new Vector3(topLeft, layerDepth);
			this.vertices[this.vertexCount + 1].Position = new Vector3(topRight, layerDepth);
			this.vertices[this.vertexCount + 2].Position = new Vector3(bottomRight, layerDepth);
			this.vertices[this.vertexCount + 3].Position = new Vector3(bottomLeft, layerDepth);

			this.vertices[this.vertexCount].UV = this.CalculateUV(source.Left, source.Top);
			this.vertices[this.vertexCount + 1].UV = this.CalculateUV(source.Right, source.Top);
			this.vertices[this.vertexCount + 2].UV = this.CalculateUV(source.Right, source.Bottom);
			this.vertices[this.vertexCount + 3].UV = this.CalculateUV(source.Left, source.Bottom);

			this.vertices[this.vertexCount].Color = color;
			this.vertices[this.vertexCount + 1].Color = color;
			this.vertices[this.vertexCount + 2].Color = color;
			this.vertices[this.vertexCount + 3].Color = color;

			this.vertexCount += 4;
		}

		public void Draw(
			Texture texture,
			Vector2 destination,
			Rectangle? source = null,
			Color4? tint = null,
			Vector2? origin = null,
			Vector2? scale = null,
			float rotation = 0.0f,
			float layerDepth = 0.0f)
		{
			if (texture == null)
				throw new ArgumentNullException("texture");
		
			this.DrawInternal(
				texture,
				destination,
				source != null ? source.Value.Width : texture.Width,
				source != null ? source.Value.Height : texture.Height,
				source,
				tint,
				origin,
				scale,
				rotation,
				layerDepth);
		}
			
		public void Draw(
			Texture texture,
			Rectangle destination,
			Rectangle? source = null,
			Color4? tint = null,
			Vector2? origin = null,
			Vector2? scale = null,
			float rotation = 0.0f,
			float layerDepth = 0.0f)
		{
			this.DrawInternal(
				texture,
				new Vector2(destination.X, destination.Y),
				destination.Width,
				destination.Height,
				source,
				tint,
				origin,
				scale,
				rotation,
				layerDepth);
		}

		public void Draw(
			SpriteSheet spriteSheet,
			int frame,
			Vector2 position,
			Color4? tint = null,
			Vector2? origin = null,
			Vector2? scale = null,
			float rotation = 0.0f,
			float layerDepth = 0.0f)
		{
			if (spriteSheet == null)
				throw new ArgumentNullException("spriteSheet");

			Rectangle frameRect = spriteSheet[frame];
			
			this.DrawInternal(
				spriteSheet.Texture,
				position,
				frameRect.Width,
				frameRect.Height,
				frameRect,
				tint,
				origin,
				scale,
				rotation,
				layerDepth);
		}

		private void DrawInternal(
			Texture texture,
			Vector2 destination,
			int width,
			int height,
			Rectangle? source,
			Color4? tint,
			Vector2? origin,
			Vector2? scale,
			float rotation,
			float layerDepth)
		{
			this.EnsureDrawInProgress();

			if (texture == null)
				throw new ArgumentNullException("texture");

			if (texture != this.texture)
				this.Flush();

			this.texture = texture;

			if (source == null)
				source = new Rectangle(0, 0, texture.Width, texture.Height);

			if (tint == null)
				tint = Color4.White;
						
			if (origin == null)
				origin = Vector2.Zero;

			if (scale == null)
				scale = Vector2.One;

			Vector2 topLeft = new Vector2(-origin.Value.X, -origin.Value.Y);
			Vector2 topRight = new Vector2(width - origin.Value.X, -origin.Value.Y);
			Vector2 bottomRight = new Vector2(width - origin.Value.X, height - origin.Value.Y);
			Vector2 bottomLeft = new Vector2(-origin.Value.X, height - origin.Value.Y);

			Matrix4 rotationMatrix;
			Matrix4.CreateRotationZ(rotation, out rotationMatrix);

			Matrix4 scaleMatrix;
			Matrix4.CreateScale(scale.Value.X, scale.Value.Y, 1.0f, out scaleMatrix);

			Matrix4 translationMatrix;
			Matrix4.CreateTranslation(destination.X, destination.Y, out translationMatrix);

			Matrix4 identity = Matrix4.Identity;

			Matrix4 transform1;
			Matrix4.Multiply(ref identity, ref scaleMatrix, out transform1);

			Matrix4 transform2;
			Matrix4.Multiply(ref transform1, ref rotationMatrix, out transform2);

			Matrix4 transform3;
			Matrix4.Multiply(ref transform2, ref translationMatrix, out transform3);

			topLeft = topLeft.Transform(ref transform3);
			topRight = topRight.Transform(ref transform3);
			bottomRight = bottomRight.Transform(ref transform3);
			bottomLeft = bottomLeft.Transform(ref transform3);

			this.AddQuad(
				ref topLeft,
                ref topRight,
                ref bottomRight,
                ref bottomLeft,
				source.Value,
				tint.Value,
				layerDepth);
		}
		
		public Vector2 DrawString(
			TextureFont font,
			string text,
			Vector2 position,
			Color4? tint = null,
			Vector2? origin = null,
			Vector2? scale = null,
			float rotation = 0.0f,
			float layerDepth = 0.0f)
		{
			if (font == null)
				throw new ArgumentNullException("font");

			if (text == null)
				throw new ArgumentNullException("text");

			if (text.Length == 0)
				return position;

			Size textSize = font.MeasureString(text);

			return this.DrawString(font, text, new Rectangle((int)position.X, (int)position.Y, textSize.Width, textSize.Height), tint, origin, scale, rotation, layerDepth);
		}
				
		public Vector2 DrawString(
			TextureFont font,
			string text,
			Rectangle destination,
			Color4? tint = null,
			Vector2? origin = null,
			Vector2? scale = null,
			float rotation = 0.0f,
			float layerDepth = 0.0f)
		{
			if (font == null)
				throw new ArgumentNullException("font");

			if (text == null)
				throw new ArgumentNullException("text");

			if (text.Length == 0)
				return new Vector2(destination.X, destination.Y);

			if (tint == null)
				tint = Color4.White;

			if (origin == null)
				origin = Vector2.Zero;

			if (scale == null)
				scale = Vector2.One;

			float heightOfSingleLine = font.LineHeight * scale.Value.Y;

			if (heightOfSingleLine > destination.Height) // We can't draw anything
				return new Vector2(destination.X, destination.Y);

			Vector2 cursor = new Vector2(destination.X, destination.Y);

			for (int i = 0; i < text.Length; i++)
			{
				// Skip characters we can't render.
				if (text[i] == '\r')
					continue;

				float widthOfChar = 0;

				if (text[i] == '\n' || cursor.X + (widthOfChar = font[text[i]].Width * scale.Value.X) > destination.Right)
				{
					cursor.X = destination.X;
					cursor.Y += heightOfSingleLine + font.LineSpacing;

					// If the next line extends past the destination, quit.
					if (cursor.Y + heightOfSingleLine > destination.Bottom)
						return cursor;

					// We can't render a new line.
					if (text[i] == '\n')
						continue;
				}

				Vector2 characterOrigin = origin.Value;
				characterOrigin.X -= cursor.X - destination.X;
				characterOrigin.Y -= cursor.Y - destination.Y;

				Rectangle letterSource = font[text[i]];
				Rectangle letterDestination = new Rectangle((int)cursor.X + (int)characterOrigin.X, (int)cursor.Y + (int)characterOrigin.Y, (int)widthOfChar, (int)heightOfSingleLine);
								
				this.Draw(
					font.Texture,
					letterDestination,
					letterSource,
					tint,
					characterOrigin,
					scale,
					rotation,
					layerDepth);

				cursor.X += widthOfChar + font.CharacterSpacing;
			}

			return cursor;
		}

		private void Flush()
		{
			if (this.vertexCount > 0)
			{
				this.vertexBuffer.SetData(this.vertices, 0, this.vertexCount);

				this.graphics.ShaderProgram = this.shader.ShaderProgram;

				Rectangle viewport = this.graphics.Viewport;
				this.transform.M11 = 2f / viewport.Width;
				this.transform.M22 = -2f / viewport.Height;
								
				this.shader.SetTransform(ref this.transform);
				this.shader.SetSampler(this.texture);

				this.graphics.Draw(PrimitiveType.Triangles, this.vertexBuffer, this.indexBuffer, 0, (this.vertexCount / 4) * 6);

				this.vertexCount = 0;
			}
		}
	}

}
