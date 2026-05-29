// Cubic Bezier curve geometry shader. Expects (control1, point1, point2, control2) as a
// lines_adjacency primitive; emits a line strip approximating the Bezier curve.
#version 150

layout(lines_adjacency) in;
layout(line_strip, max_vertices = 100) out;

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

vec3 evaluateBezierPosition(vec3 v[4], float t)
{
	float OneMinusT = 1.0 - t;
	float b0 = OneMinusT*OneMinusT*OneMinusT;
	float b1 = 3.0*t*OneMinusT*OneMinusT;
	float b2 = 3.0*t*t*OneMinusT;
	float b3 = t*t*t;
	return b0*v[0] + b1*v[1] + b2*v[2] + b3*v[3];
}

void main()
{
	vec3 pos[4];
	pos[0] = gl_in[1].gl_Position.xyz;  //Point 1
	pos[1] = gl_in[0].gl_Position.xyz;  //Control point 1
	pos[2] = gl_in[3].gl_Position.xyz;  //Control point 2
	pos[3] = gl_in[2].gl_Position.xyz;  //Point 2

	float g_Detail = 10.0;
	float OneOverDetail = 1.0 / float(g_Detail - 1.0);
	for (int i = 0; i < g_Detail; i++)
	{
		float t = i * OneOverDetail;
		vec3 p = evaluateBezierPosition(pos, t);
		Output_Colour.Colour = Input_Colour[1].Colour;
		Output_TexCoord.TexCoord = mix(Input_TexCoord[1].TexCoord, Input_TexCoord[2].TexCoord, t);
		gl_Position = vec4(p.xyz, 1.0);
		EmitVertex();
	}

	EndPrimitive();
}
