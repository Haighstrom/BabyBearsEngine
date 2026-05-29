// Default fragment shader for 1D textures (e.g. gradient ramps, LUTs). Multiplies the
// sampled colour by the vertex colour and discards zero-alpha pixels.
#version 150

uniform sampler1D Sampler;

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
	Colour = texture(Sampler, Input_TexCoord.TexCoord.x) * Input_Colour.Colour;

	//Alpha test
	if (Colour.a <= 0)
		discard;
}
