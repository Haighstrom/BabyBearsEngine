// Simple in-out geometry shader that does not affect the vertices at all. Points in, points
// out - the points-equivalent of default.geom (which handles triangles).
#version 150

layout(points) in;
layout(points, max_vertices = 1) out;

in ColourData
{
	vec4 Colour;
} Input_Colour[];

in TexCoordData
{
	vec2 TexCoord;
} Input_TexCoord[];

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
	for (int i = 0; i < gl_in.length(); i++)
	{
		Output_Colour.Colour = Input_Colour[i].Colour;
		Output_TexCoord.TexCoord = Input_TexCoord[i].TexCoord;
		gl_Position = gl_in[i].gl_Position;
		EmitVertex();
	}
	EndPrimitive();
}
