// Box-blur fragment shader. Samples a 9×9 neighbourhood around each texel and averages.
// BlurSize controls the kernel spread (in texture-space units, scaled by /400 so a value of
// ~1-10 reads naturally).
#version 150

uniform sampler2D Sampler;
uniform float BlurSize;

in ColourData
{
	vec4 Colour;
} Input_Colour;

in TexCoordData
{
	vec2 TexCoord;
} Input_TexCoord;

out vec4 Colour;

void main()
{
	float blurStep = BlurSize / 400.0;

	vec4 sum = vec4(0.0);
	for (int x = -4; x <= 4; x++)
		for (int y = -4; y <= 4; y++)
			sum += texture(Sampler, vec2(Input_TexCoord.TexCoord.x + x * blurStep, Input_TexCoord.TexCoord.y + y * blurStep)) / 81.0;
	Colour = sum * Input_Colour.Colour;

	//Alpha test
	if (Colour.a <= 0)
		discard;
}
