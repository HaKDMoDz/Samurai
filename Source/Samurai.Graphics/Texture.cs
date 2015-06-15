using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace Samurai.Graphics
{
	public sealed class Texture : GraphicsObject
	{
		internal uint Index
		{
			get;
			private set;
		}

		internal uint Handle
		{
			get;
			private set;
		}

		public TextureFilter MinFilter
		{
			get;
			private set;
		}

		public TextureFilter MagFilter
		{
			get;
			private set;
		}

		public TextureWrap WrapS
		{
			get;
			private set;
		}

		public TextureWrap WrapT
		{
			get;
			private set;
		}
				
		public int Width
		{
			get;
			private set;
		}

		public int Height
		{
			get;
			private set;
		}

		private int PixelCount
		{
			get { return this.Width * this.Height; }
		}

		private Texture(GraphicsContext graphics)
			: base(graphics)
		{
			this.Index = this.Graphics.AllocateTextureIndex();
			this.Handle = this.Graphics.GL.GenTexture();
		}

		public Texture(
			GraphicsContext graphics,
			int width,
			int height)
			: base(graphics)
		{
			this.Index = this.Graphics.AllocateTextureIndex();
			this.Handle = this.Graphics.GL.GenTexture();

			graphics.GL.ActiveTexture(GLContext.Texture0 + this.Index);
			graphics.GL.BindTexture(GLContext.Texture2D, this.Handle);

			graphics.GL.TexImage2D(
				GLContext.Texture2D,
				0,
				(int)GLContext.Rgba8,
				width,
				height,
				0,
				GLContext.Rgba,
				(int)GLContext.UnsignedByte,
				null);

			this.Width = width;
			this.Height = height;
		}

		protected override void DisposeManagedResources()
		{
			if (!this.Graphics.IsDisposed)
				this.Graphics.DeallocateTextureIndex(this.Index);
		}

		protected override void DisposeUnmanagedResources()
		{
			this.Graphics.GL.DeleteTexture(this.Handle);
		}

		public byte[] GetBytes()
		{
			byte[] bytes = new byte[this.PixelCount * 4];
			this.Graphics.GL.GetTexImage(GLContext.Texture2D, 0, GLContext.Rgba, (int)GLContext.UnsignedByte, bytes);
			return bytes;
		}

		public Color4[] GetPixels()
		{
			byte[] bytes = this.GetBytes();
			Color4[] pixels = new Color4[this.PixelCount];

			GCHandle pixelsPtr = GCHandle.Alloc(pixels, GCHandleType.Pinned);

			try
			{
				Marshal.Copy(bytes, 0, pixelsPtr.AddrOfPinnedObject(), bytes.Length);
			}
			finally
			{
				pixelsPtr.Free();
			}

			return pixels;
		}

		public static Texture LoadFromFile(GraphicsContext graphics, string fileName, TextureParams parameters)
		{
			if (graphics == null)
				throw new ArgumentNullException("graphics");

			if (fileName == null)
				throw new ArgumentNullException("fileName");

			using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
				return LoadFromStream(graphics, file, parameters);
		}

		public static Texture LoadFromStream(GraphicsContext graphics, Stream stream, TextureParams parameters)
		{
			if (graphics == null)
				throw new ArgumentNullException("graphics");

			if (stream == null)
				throw new ArgumentNullException("stream");

			using (Bitmap bitmap = (Bitmap)Bitmap.FromStream(stream))
			{
				byte[] bytes = BitmapHelper.GetBytes(bitmap);
				return LoadFromBytes(graphics, bytes, bitmap.Width, bitmap.Height, parameters);
			}
		}

		public static Texture LoadFromBytes(GraphicsContext graphics, byte[] bytes, int width, int height, TextureParams parameters)
		{
			if (graphics == null)
				throw new ArgumentNullException("graphics");

			if (bytes == null)
				throw new ArgumentNullException("bytes");

			Texture texture = new Texture(graphics);

			graphics.GL.ActiveTexture(GLContext.Texture0 + texture.Index);
			graphics.GL.BindTexture(GLContext.Texture2D, texture.Handle);

			if (parameters.ColorKey != null)
			{
				for (int i = 0; i < bytes.Length; i += 4)
				{
					uint pixel = GLHelper.MakePixelRGBA(bytes[i], bytes[i + 1], bytes[i + 2], bytes[i + 3]);

					if (pixel == parameters.ColorKey.Value.ToRgba())
						GLHelper.DecomposePixelRGBA(parameters.TransparentPixel.ToRgba(), out bytes[i], out bytes[i + 1], out bytes[i + 2], out bytes[i + 3]);
				}
			}

			graphics.GL.TexParameteri(GLContext.Texture2D, GLContext.TextureMagFilter, (int)parameters.MagFilter);
			graphics.GL.TexParameteri(GLContext.Texture2D, GLContext.TextureMinFilter, (int)parameters.MinFilter);
			graphics.GL.TexParameteri(GLContext.Texture2D, GLContext.TextureWrapS, (int)parameters.WrapS);
			graphics.GL.TexParameteri(GLContext.Texture2D, GLContext.TextureWrapT, (int)parameters.WrapT);

			graphics.GL.TexImage2D(
				GLContext.Texture2D,
				0,
				(int)GLContext.Rgba8,
				width,
				height,
				0,
				GLContext.Rgba,
				(int)GLContext.UnsignedByte,
				bytes);

			texture.Width = width;
			texture.Height = height;

			return texture;
		}
	}
}
