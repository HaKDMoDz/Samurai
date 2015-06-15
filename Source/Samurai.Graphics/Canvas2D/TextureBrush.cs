using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samurai.Graphics.Canvas2D
{
    public class TextureBrush : CanvasBrush
    {
        ShaderProgram shader;
        Color4 tint;

        public Texture Texture
        {
            get;
            set;
        }

        public Color4 Tint
        {
            get { return this.tint; }
            set { this.tint = value; }
        }

        public Rectangle? Source
        {
            get;
            set;
        }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="graphics">Handle to the GraphicsContext.</param>
        public TextureBrush(GraphicsContext graphics)
            : base (graphics)
		{
            this.shader = typeof(SolidColorBrush).Assembly.LoadShaderProgram(
                graphics,
                "Samurai.Graphics.Canvas2D.BasicCanvasShader.vert",
                "Samurai.Graphics.Canvas2D.TextureBrush.frag");

            this.Tint = Color4.White;
		}

		protected override void DisposeManagedResources()
		{
            this.shader.Dispose();
            this.shader = null;
		}
		
        public override void Apply(ref Matrix4 transform)
        {
            if (this.Texture == null)
                throw new InvalidOperationException("TextureBrush.Texture is null.");

            Rectangle? source = this.Source;

            this.Grahpics.ShaderProgram = this.shader;
            this.shader.SetValue("vertTransform", ref transform);
            this.shader.SetValue("fragTexture", this.Texture);
            this.shader.SetValue("fragTextureWidth", this.Texture.Width);
            this.shader.SetValue("fragTextureHeight", this.Texture.Height);

            if (source != null)
            {
                this.shader.SetValue("fragTextureSourceX", source.Value.X);
                this.shader.SetValue("fragTextureSourceY", source.Value.Y);
                this.shader.SetValue("fragTextureSourceWidth", source.Value.Width);
                this.shader.SetValue("fragTextureSourceHeight", source.Value.Height);
            }
            else
            {
                this.shader.SetValue("fragTextureSourceX", 0);
                this.shader.SetValue("fragTextureSourceY", 0);
                this.shader.SetValue("fragTextureSourceWidth", this.Texture.Width);
                this.shader.SetValue("fragTextureSourceHeight", this.Texture.Height);
            }

            this.shader.SetValue("fragTint", ref this.tint);
        }
    }
}
