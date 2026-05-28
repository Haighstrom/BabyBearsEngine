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

	// fwidth gives the rate the distance changes per screen pixel, so the
	// antialiased band stays ~1px wide no matter how far the text is scaled.
	// This is the win over a fixed-resolution coverage atlas.
	float width = fwidth(distance);
	float alpha = smoothstep(0.5 - width, 0.5 + width, distance);

	// Input_Colour arrives premultiplied (the vertex shader does rgb *= a), so
	// scaling the whole colour by coverage keeps it premultiplied for blending.
	Colour = Input_Colour.Colour * alpha;

	// Alpha test
	if (Colour.a <= 0.0)
	{
		discard;
	}
}
