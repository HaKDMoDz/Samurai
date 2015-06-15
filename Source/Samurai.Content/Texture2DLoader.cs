using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Samurai.Graphics;

namespace Samurai.Content
{
	public class Texture2DLoader : ContentLoader<Texture>
	{
		GraphicsContext graphics;

		public Texture2DLoader(GraphicsContext graphics)
		{
			if (graphics == null)
				throw new ArgumentNullException("graphics");

			this.graphics = graphics;
		}
				
		public override Texture Load(Stream stream)
		{
			using (BinaryReader br = new BinaryReader(stream))
			{
				int width = br.ReadInt32();
				int height = br.ReadInt32();
				bool isCompressed = br.ReadBoolean();

				int byteCount = br.ReadInt32();

				byte[] bytes = new byte[byteCount];
				
				int bytesRead = br.Read(bytes, 0, byteCount);

				if (bytesRead != byteCount)
					throw new ContentException("Unable to read image bytes.");

				if (isCompressed)
				{
					using (MemoryStream ms = new MemoryStream(bytes))
					using (DeflateStream deflate = new DeflateStream(ms, CompressionMode.Decompress))
					{
						byte[] bytes2 = new byte[width * height * 4];

						bytesRead = deflate.Read(bytes2, 0, bytes2.Length);

						bytes = bytes2;
					}
				}

				return Texture.LoadFromBytes(this.graphics, bytes, width, height, new TextureParams());
			}
		}
	}
}
