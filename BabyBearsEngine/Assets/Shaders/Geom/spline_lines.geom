// Cubic Catmull-Rom spline geometry shader. Expects a lines_adjacency primitive where the
// first and last are control points that won't be drawn - the curve passes through points 1
// and 2 only. Paired with splines.vert which forwards each control point's neighbours.
#version 150

layout(lines_adjacency) in;
layout(triangle_strip, max_vertices = 102) out;

uniform mat3 PMatrix;
uniform mat3 MVMatrix;
uniform float AnimationTextureIndex;
uniform float TextureRepeatLength;
uniform float LevelOfDetail;
uniform float MITER_LIMIT = 1.0;
uniform float THICKNESS;

in NeighbourPositionsData
{
	vec2 NextPos;
	vec2 PrevPos;
} Input_NeighbourPositions[];

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


vec2 CatmullRomSpline(vec2 v[4], float t, vec2 prevP, vec2 nextP)
{
	if (t > 1.0)
	{
		v[0] = v[1];
		v[1] = v[2];
		v[2] = v[3];
		v[3] = nextP;
		t = t - 1.0;
	}
	else if (t < 0.0)
	{
		v[3] = v[2];
		v[2] = v[1];
		v[1] = v[0];
		v[0] = prevP;
		t = t + 1.0;
	}

	float t2 = t * t;
	float t3 = t2 * t;

	return ((2 * v[1]) +
		(-v[0] + v[2]) * t +
		(2 * v[0] - 5 * v[1] + 4 * v[2] - v[3]) * t2 +
		(-v[0] + 3 * v[1] - 3 * v[2] + v[3]) * t3) * 0.5;
}

//Same code as used in smooth_lines.geom. p0 and p3 are control points; line is drawn in
//triangle strip segments between p1 and p2.
void DrawNeatLines(vec2 p0, vec2 p1, vec2 p2, vec2 p3)
{
	vec2 v0 = normalize(p1 - p0);
	vec2 v1 = normalize(p2 - p1);
	vec2 v2 = normalize(p3 - p2);

	vec2 n0 = vec2(-v0.y, v0.x);
	vec2 n1 = vec2(-v1.y, v1.x);
	vec2 n2 = vec2(-v2.y, v2.x);

	vec2 miter_a = normalize(n0 + n1);
	vec2 miter_b = normalize(n1 + n2);

	float length_a = THICKNESS / dot(miter_a, n1);
	float length_b = THICKNESS / dot(miter_b, n1);

	if (dot(v0, v1) < -MITER_LIMIT)
	{
		miter_a = n1;
		length_a = THICKNESS;

		if (dot(v0, n1) > 0)
		{
			gl_Position = vec4((PMatrix * MVMatrix * vec3(p1 + THICKNESS * n0, 1.0)).xy, 0, 1);
			EmitVertex();
			gl_Position = vec4((PMatrix * MVMatrix * vec3(p1 + THICKNESS * n1, 1.0)).xy, 0, 1);
			EmitVertex();
			gl_Position = vec4((PMatrix * MVMatrix * vec3(p1, 1.0)).xy, 0, 1);
			EmitVertex();
			EndPrimitive();
		}
		else
		{
			gl_Position = vec4((PMatrix * MVMatrix * vec3(p1 - THICKNESS * n1, 1.0)).xy, 0, 1);
			EmitVertex();
			gl_Position = vec4((PMatrix * MVMatrix * vec3(p1 - THICKNESS * n0, 1.0)).xy, 0, 1);
			EmitVertex();
			gl_Position = vec4((PMatrix * MVMatrix * vec3(p1, 1.0)).xy, 0, 1);
			EmitVertex();
			EndPrimitive();
		}
	}

	if (dot(v1, v2) < -MITER_LIMIT)
	{
		miter_b = n1;
		length_b = THICKNESS;
	}

	gl_Position = vec4((PMatrix * MVMatrix * vec3(p1 + length_a * miter_a, 1.0)).xy, 0, 1);
	EmitVertex();
	gl_Position = vec4((PMatrix * MVMatrix * vec3(p1 - length_a * miter_a, 1.0)).xy, 0, 1);
	EmitVertex();
	gl_Position = vec4((PMatrix * MVMatrix * vec3(p2 + length_b * miter_b, 1.0)).xy, 0, 1);
	EmitVertex();
	gl_Position = vec4((PMatrix * MVMatrix * vec3(p2 - length_b * miter_b, 1.0)).xy, 0, 1);
	EmitVertex();

	EndPrimitive();
}

void main()
{
	vec2 pos[4];
	pos[0] = gl_in[0].gl_Position.xy;  //Control Point 1
	pos[1] = gl_in[1].gl_Position.xy;  //Point 1
	pos[2] = gl_in[2].gl_Position.xy;  //Point 2
	pos[3] = gl_in[3].gl_Position.xy;  //Control Point 2

	vec2 nextPos = Input_NeighbourPositions[3].NextPos;
	vec2 prevPos = Input_NeighbourPositions[0].PrevPos;

	float OneOverDetail = 1.0 / float(LevelOfDetail);

	for (float i = 1; i <= LevelOfDetail; i++)
	{
		Output_Colour.Colour = Input_Colour[1].Colour;
		Output_TexCoord.TexCoord = vec2(AnimationTextureIndex * TextureRepeatLength, 0);

		DrawNeatLines(
			CatmullRomSpline(pos, (i - 2.0) * OneOverDetail, prevPos, nextPos),
			CatmullRomSpline(pos, (i - 1.0) * OneOverDetail, prevPos, nextPos),
			CatmullRomSpline(pos, (i + 0.0) * OneOverDetail, prevPos, nextPos),
			CatmullRomSpline(pos, (i + 1.0) * OneOverDetail, prevPos, nextPos)
		);
	}
}
