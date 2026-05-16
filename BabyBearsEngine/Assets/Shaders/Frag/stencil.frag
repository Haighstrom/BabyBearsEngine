#version 430

uniform sampler2D ImageSampler;
uniform sampler2D StencilSampler;
// Pixels with mask below this value are fully discarded. Set > 0 (e.g. 0.5) for hard-edged
// stencils from lossy sources (JPEG) where mipmap blending creates unwanted partial values.
uniform float Threshold;

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
	// Half-pixel offset avoids sub-pixel seam artefacts at texture boundaries.
	vec2 texSize = textureSize(ImageSampler, 0);
	vec2 offset = vec2(0.5 / texSize.x, 0.5 / texSize.y);
	vec2 uv = Input_TexCoord.TexCoord + offset;

	Colour = texture(ImageSampler, uv) * Input_Colour.Colour;

	if (Colour.a <= 0)
		discard;

	// Multiply alpha and red channels so both conventions work naturally:
	//   JPEG / opaque B&W image: alpha = 1.0, so mask = red (white shows, black discards).
	//   PNG with transparency:   red channel ignored where alpha = 0, so transparent = discard.
	//   Semi-transparent PNG:    both channels contribute.
	vec4 stencil = texture(StencilSampler, uv);
	float mask = stencil.a * stencil.r;

	if (mask <= Threshold)
		discard;

	Colour = vec4(Colour.rgb, mask * Colour.a);
}
