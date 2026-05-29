// Billboard vertex shader. Outputs raw position + per-axis size for a geometry-shader stage
// (billboard_points_to_quads.geom) to expand into a camera-facing quad.
#version 330

layout (location = 0) in vec2 Position;
layout (location = 1) in vec4 Colour;
layout (location = 2) in vec2 Size;

out ColourData
{
	vec4 Colour;
} Output_Colour;

out SizeData
{
	vec2 Size;
} Output_Size;

void main()
{
	gl_Position = vec4(Position, 0, 1);

	Output_Colour.Colour = vec4(Colour.xyz * Colour.w, Colour.w);

	Output_Size.Size = Size;
}
