// Converts a line into a thick quad (triangle strip). Simpler than smooth_lines.geom - no
// mitering, just an extruded rectangle of width LineThickness. ThicknessInPixels switches
// between screen-space (post-MV) and world-space (pre-MV) thickness. Output_TexCoord carries
// distance travelled along the line (in TexCoord.x) rather than a texture coordinate, for
// dashed_line.frag's dash pattern - paired with solid-fill fragment shaders it's simply unread.
#version 150

uniform mat3 MVMatrix;
uniform mat3 PMatrix;
uniform float LineThickness;
uniform bool ThicknessInPixels = true;

layout(lines) in;
layout(triangle_strip, max_vertices = 4) out;

in ColourData
{
	vec4 Colour;
} Input_Colour[];

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
	vec2 p0;
	vec2 p1;

	if (ThicknessInPixels)
	{
		p0 = (MVMatrix * vec3(gl_in[0].gl_Position.xy, 1)).xy;
		p1 = (MVMatrix * vec3(gl_in[1].gl_Position.xy, 1)).xy;
	}
	else
	{
		p0 = gl_in[0].gl_Position.xy;
		p1 = gl_in[1].gl_Position.xy;
	}

	vec2 v0 = normalize(p1 - p0);
	vec2 n0 = vec2(-v0.y, v0.x) * 0.5;
	float lineLength = length(p1 - p0);

	Output_Colour.Colour = Input_Colour[0].Colour;

	if (ThicknessInPixels)
	{
		gl_Position = vec4(PMatrix * vec3(p0 + n0 * LineThickness, 1), 1);
		Output_TexCoord.TexCoord = vec2(0, 0);
		EmitVertex();

		gl_Position = vec4(PMatrix * vec3(p1 + n0 * LineThickness, 1), 1);
		Output_TexCoord.TexCoord = vec2(lineLength, 0);
		EmitVertex();

		gl_Position = vec4(PMatrix * vec3(p0 - n0 * LineThickness, 1), 1);
		Output_TexCoord.TexCoord = vec2(0, 0);
		EmitVertex();

		gl_Position = vec4(PMatrix * vec3(p1 - n0 * LineThickness, 1), 1);
		Output_TexCoord.TexCoord = vec2(lineLength, 0);
		EmitVertex();
	}
	else
	{
		gl_Position = vec4(PMatrix * MVMatrix * vec3(p0 + n0 * LineThickness, 1), 1);
		Output_TexCoord.TexCoord = vec2(0, 0);
		EmitVertex();

		gl_Position = vec4(PMatrix * MVMatrix * vec3(p1 + n0 * LineThickness, 1), 1);
		Output_TexCoord.TexCoord = vec2(lineLength, 0);
		EmitVertex();

		gl_Position = vec4(PMatrix * MVMatrix * vec3(p0 - n0 * LineThickness, 1), 1);
		Output_TexCoord.TexCoord = vec2(0, 0);
		EmitVertex();

		gl_Position = vec4(PMatrix * MVMatrix * vec3(p1 - n0 * LineThickness, 1), 1);
		Output_TexCoord.TexCoord = vec2(lineLength, 0);
		EmitVertex();
	}

	EndPrimitive();
}
