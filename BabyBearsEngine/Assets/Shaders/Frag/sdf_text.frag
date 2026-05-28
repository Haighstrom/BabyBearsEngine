#version 430

uniform sampler2D Sampler;

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
	// The R channel stores a signed distance field: 0.5 is the glyph outline,
	// > 0.5 is inside the glyph, < 0.5 is outside.
	float distance = texture(Sampler, Input_TexCoord.TexCoord).r;

	// fwidth gives the rate the distance changes per screen pixel. Dividing the
	// signed distance-to-edge by it yields a coverage ramp exactly one pixel wide,
	// so the edge stays crisp at any scale. (A full-fwidth smoothstep spans ~2px and
	// looks soft; this is the standard tight SDF antialiasing.)
	float aaWidth = max(fwidth(distance), 1e-5);
	float alpha = clamp((distance - 0.5) / aaWidth + 0.5, 0.0, 1.0);

	// Input_Colour arrives premultiplied (the vertex shader does rgb *= a), so
	// scaling the whole colour by coverage keeps it premultiplied for blending.
	Colour = Input_Colour.Colour * alpha;

	// Alpha test
	if (Colour.a <= 0.0)
	{
		discard;
	}
}
