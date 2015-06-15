using System;

namespace Samurai.Graphics.Sprites
{
	public interface ISpriteShaderProgram
	{
		ShaderProgram ShaderProgram { get; }

		void SetTransform(ref Matrix4 transform);

		void SetSampler(Texture texture);
	}
}
