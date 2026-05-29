// Billboard vertex shader. Outputs raw position + size for a geometry-shader stage
// (billboard_points_to_quads.geom) to expand into a camera-facing quad.
#version 150

in vec2 Position;
in vec4 Colour;
in float Size;

out ColourData
{
	vec4 Colour;
} Output_Colour;

out SizeData
{
	float Size;
} Output_Size;

void main()
{
	gl_Position = vec4(Position, 0, 1);

	Output_Colour.Colour = vec4(Colour.xyz * Colour.w, Colour.w);

	Output_Size.Size = Size;
}
