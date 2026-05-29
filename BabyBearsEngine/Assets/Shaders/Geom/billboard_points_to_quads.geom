// Expands each input point into a camera-facing quad centred on it. The per-axis Size carried
// through from billboard.vert (Size.x = quad width, Size.y = quad height) lets a single shader
// drive both square sprites (equal components) and stretched billboards like rain streaks
// (unequal components). Used for particle and sprite-from-point rendering.
#version 150

uniform mat3 MVMatrix;
uniform mat3 PMatrix;
const vec2 corners[4] = vec2[4](vec2(0.0, 1.0), vec2(0.0, 0.0), vec2(1.0, 1.0), vec2(1.0, 0.0));

layout(points) in;
layout(triangle_strip, max_vertices = 4) out;

in ColourData
{
	vec4 Colour;
} Input_Colour[];

in SizeData
{
	vec2 Size;
} Input_SizeData[];

out ColourData
{
	vec4 Colour;
} Output_Colour;

out TexCoordData
{
	vec2 TexCoord;
} Output_TexCoord;

void main()
{
	for (int i = 0; i < 4; ++i)
	{
		vec2 eyePos = gl_in[0].gl_Position.xy;
		eyePos += Input_SizeData[0].Size * (corners[i] - vec2(0.5));
		gl_Position = vec4(PMatrix * MVMatrix * vec3(eyePos.xy, 1), 1);
		Output_TexCoord.TexCoord = corners[i];
		Output_Colour.Colour = Input_Colour[0].Colour;
		EmitVertex();
	}

	EndPrimitive();
}
