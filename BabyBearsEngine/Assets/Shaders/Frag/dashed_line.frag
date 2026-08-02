// Solid-colour fill with an optional dash pattern along TexCoord.x (distance travelled along the
// line - see line_to_quad.geom / smooth_lines.geom). GapLength = 0 (the default) never discards,
// so this behaves identically to solid_colour.frag for a plain solid line.
#version 150

uniform float DashLength = 1.0;
uniform float GapLength = 0.0;

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
	Colour = Input_Colour.Colour;

	//Alpha test
	if (Colour.a <= 0)
		discard;

	float period = DashLength + GapLength;
	if (period > 0.0)
	{
		float phase = mod(Input_TexCoord.TexCoord.x, period);
		if (phase >= DashLength)
			discard;
	}
}
