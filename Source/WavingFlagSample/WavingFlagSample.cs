using Samurai;
using Samurai.GLFW;
using Samurai.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WavingFlagSample
{
	public class WavingFlagSample : Game
	{
		[StructLayout(LayoutKind.Sequential)]
		struct Vertex
		{
			public Vector2 Position;

			public Vector2 UV;
		}

		ShaderProgram shader;
		Texture texture;
		StaticVertexBuffer<Vertex> vertexBuffer;
		StaticIndexBuffer<ushort> indexBuffer;

		float totalElapsedSeconds;

		public WavingFlagSample()
			: base()
		{
			this.Window.Title = "Samurai Waving Flag Sample";

			this.shader = new ShaderProgram(
				this.Graphics,
				VertexShader.Compile(this.Graphics, File.ReadAllText("WavingFlagSample.vert")),
				FragmentShader.Compile(this.Graphics, File.ReadAllText("WavingFlagSample.frag")));

			this.Graphics.ShaderProgram = this.shader;
			
			this.texture = Texture.LoadFromFile(this.Graphics, "Flag.png", TextureParams.Default);

			int totalChunks = 100;
			int chunkSize = this.texture.Width / totalChunks;
			Rectangle destination = new Rectangle(
				(this.Window.Width - this.texture.Width) / 2,
				(this.Window.Height - this.texture.Height) / 2,
				chunkSize,
				this.texture.Height);

			Rectangle source = new Rectangle(0, 0, chunkSize, this.texture.Height);

			Vertex[] vertexData = new Vertex[totalChunks * 4];

			for (int i = 0; i < totalChunks * 4; i += 4)
			{
				vertexData[i] = new Vertex() { Position = new Vector2(destination.X, destination.Y), UV = new Vector2(source.X / (float)texture.Width, 0) };
				vertexData[i + 1] = new Vertex() { Position = new Vector2(destination.X + chunkSize, destination.Y), UV = new Vector2((source.X + chunkSize) / (float)texture.Width, 0) };
				vertexData[i + 2] = new Vertex() { Position = new Vector2(destination.X + chunkSize, destination.Y + this.texture.Height), UV = new Vector2((source.X + chunkSize) / (float)texture.Width, 1.0f) };
				vertexData[i + 3] = new Vertex() { Position = new Vector2(destination.X, destination.Y + this.texture.Height), UV = new Vector2(source.X / (float)texture.Width, 1.0f) };

				destination.X += chunkSize;
				source.X += chunkSize;
			}

			this.vertexBuffer = new StaticVertexBuffer<Vertex>(this.Graphics, vertexData);

			ushort[] indexData = new ushort[vertexData.Length * 6];

			for (ushort i = 0, vertex = 0; i < indexData.Length; i += 6, vertex += 4)
			{
				indexData[i] = vertex;
				indexData[i + 1] = (ushort)(vertex + 1);
				indexData[i + 2] = (ushort)(vertex + 2);
				indexData[i + 3] = vertex;
				indexData[i + 4] = (ushort)(vertex + 2);
				indexData[i + 5] = (ushort)(vertex + 3);
			}

			this.indexBuffer = new StaticIndexBuffer<ushort>(this.Graphics, indexData);
		}
				
		protected override void Draw(TimeSpan elapsed)
		{
			this.Graphics.Clear(Color4.CornflowerBlue);

			Matrix4 projection = new Matrix4()
			{
				M11 = 2f / this.Window.Width,
				M22 = -2f / this.Window.Height,
				M33 = 1f,
				M44 = 1f,
				M41 = -1f,
				M42 = 1f
			};

			totalElapsedSeconds += (float)elapsed.TotalSeconds;

			this.shader.SetValue("projection", ref projection);
			this.shader.SetValue("startX", (float)((this.Window.Width - this.texture.Width) / 2));
			this.shader.SetValue("time", totalElapsedSeconds);
			this.shader.SetValue("texture0", this.texture);

			this.Graphics.Draw(PrimitiveType.Triangles, this.vertexBuffer, this.indexBuffer);

			this.Graphics.SwapBuffers();
		}

		public static void Main()
		{
			using (WavingFlagSample sample = new WavingFlagSample())
				sample.Run();
		}
	}
}
