// http://nuclear.mutantstargoat.com/articles/sdr_fract/

in vec2 fragPosition;
in vec2 fragUV;

uniform sampler2D palette;
uniform float centerX;
uniform float centerY;
uniform float scale;
uniform float iterations;

out vec4 outColor;

void main() {
    vec2 z, c;

    c.x = 1.3333 * (fragPosition.x - 0.5) * scale - centerX;
    c.y = (fragPosition.y - 0.5) * scale - centerY;

    int i;
    z = c;
    for (i = 0; i < int(iterations); i++)
	{
        float x = (z.x * z.x - z.y * z.y) + c.x;
        float y = (z.y * z.x + z.x * z.y) + c.y;

        if ((x * x + y * y) > 4.0)
			break;

        z.x = x;
        z.y = y;
    }

	float x = (i == int(iterations) ? 0.0 : float(i)) / 100.0f;
    outColor = texture(palette, vec2(x, 0));
}
