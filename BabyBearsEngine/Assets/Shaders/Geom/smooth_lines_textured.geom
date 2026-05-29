// Textured variant of smooth_lines.geom. Same UI-border mitering pipeline as the textureless
// version, but also computes per-vertex texture coordinates so the line can carry a texture
// (e.g. a dashed pattern). HorizontalTextures chooses whether the texture runs along the
// line's length or perpendicular to it; TextureRepeatLength controls the repeat frequency.
#version 150

layout(lines_adjacency) in;
layout(triangle_strip, max_vertices = 30) out;

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

uniform float MiterLimit = 0.75;	// 1.0: always miter, -1.0: never miter, 0.75: default
uniform float Thickness;
uniform mat3 MVMatrix;
uniform mat3 PMatrix;
uniform bool ThicknessInPixels = true;
uniform float TextureRepeatLength;
uniform bool HorizontalTextures;
uniform int ShiftMode = 0;	//linked to BorderPosition enum


void main()
{
	Output_Colour.Colour = Input_Colour[1].Colour;
	Output_TexCoord.TexCoord = Input_TexCoord[1].TexCoord;

	vec2 p0;
	vec2 p1;
	vec2 p2;
	vec2 p3;

	if (ThicknessInPixels)
	{
		p0 = (MVMatrix * vec3(gl_in[0].gl_Position.xy, 1)).xy;
		p1 = (MVMatrix * vec3(gl_in[1].gl_Position.xy, 1)).xy;
		p2 = (MVMatrix * vec3(gl_in[2].gl_Position.xy, 1)).xy;
		p3 = (MVMatrix * vec3(gl_in[3].gl_Position.xy, 1)).xy;
	}
	else
	{
		p0 = gl_in[0].gl_Position.xy;
		p1 = gl_in[1].gl_Position.xy;
		p2 = gl_in[2].gl_Position.xy;
		p3 = gl_in[3].gl_Position.xy;
	}

	vec2 v0 = normalize(p1 - p0);
	vec2 v1 = normalize(p2 - p1);
	vec2 v2 = normalize(p3 - p2);

	vec2 n0 = vec2(-v0.y, v0.x);
	vec2 n1 = vec2(-v1.y, v1.x);
	vec2 n2 = vec2(-v2.y, v2.x);

	vec2 miter_a = normalize(n0 + n1);
	vec2 miter_b = normalize(n1 + n2);

	float length_a = Thickness / (2.0 * dot(miter_a, n1));
	float length_b = Thickness / (2.0 * dot(miter_b, n1));

	if (ShiftMode == 1)
	{
		p1 = p1 + length_a * miter_a;
		p2 = p2 + length_b * miter_b;
	}
	else if (ShiftMode == 2)
	{
		p1 = p1 - length_a * miter_a;
		p2 = p2 - length_b * miter_b;
	}

	if (dot(v0, v1) < -MiterLimit)
	{
		miter_a = n1;
		length_a = Thickness * 0.5;

		if (dot(v0, n1) > 0)
		{
			if (ThicknessInPixels)
			{
				gl_Position = vec4((PMatrix * vec3(p1 + Thickness * n0 * 0.5, 1.0)).xy, 0, 1);
				EmitVertex();
				gl_Position = vec4((PMatrix * vec3(p1 + Thickness * n1 * 0.5, 1.0)).xy, 0, 1);
				EmitVertex();
				gl_Position = vec4((PMatrix * vec3(p1, 1.0)).xy, 0, 1);
				EmitVertex();
				EndPrimitive();
			}
			else
			{
				gl_Position = vec4((PMatrix * MVMatrix * vec3(p1 + Thickness * n0 * 0.5, 1.0)).xy, 0, 1);
				EmitVertex();
				gl_Position = vec4((PMatrix * MVMatrix * vec3(p1 + Thickness * n1 * 0.5, 1.0)).xy, 0, 1);
				EmitVertex();
				gl_Position = vec4((PMatrix * MVMatrix * vec3(p1, 1.0)).xy, 0, 1);
				EmitVertex();
				EndPrimitive();
			}
		}
		else
		{
			if (ThicknessInPixels)
			{
				gl_Position = vec4((PMatrix * vec3(p1 - Thickness * n1 * 0.5, 1.0)).xy, 0, 1);
				EmitVertex();
				gl_Position = vec4((PMatrix * vec3(p1 - Thickness * n0 * 0.5, 1.0)).xy, 0, 1);
				EmitVertex();
				gl_Position = vec4((PMatrix * vec3(p1, 1.0)).xy, 0, 1);
				EmitVertex();
				EndPrimitive();
			}
			else
			{
				gl_Position = vec4((PMatrix * MVMatrix * vec3(p1 - Thickness * n1 * 0.5, 1.0)).xy, 0, 1);
				EmitVertex();
				gl_Position = vec4((PMatrix * MVMatrix * vec3(p1 - Thickness * n0 * 0.5, 1.0)).xy, 0, 1);
				EmitVertex();
				gl_Position = vec4((PMatrix * MVMatrix * vec3(p1, 1.0)).xy, 0, 1);
				EmitVertex();
				EndPrimitive();
			}
		}
	}

	if (dot(v1, v2) < -MiterLimit)
	{
		miter_b = n1;
		length_b = Thickness * 0.5;
	}

	if (HorizontalTextures)
	{
		if (ThicknessInPixels)
		{
			gl_Position = vec4((PMatrix * vec3(p1 + length_a * miter_a, 1.0)).xy, 0, 1);
			Output_TexCoord.TexCoord = vec2(Input_TexCoord[1].TexCoord.x + dot(length_a * miter_a, v1) * TextureRepeatLength / Thickness, 0);
			EmitVertex();

			gl_Position = vec4((PMatrix * vec3(p1 - length_a * miter_a, 1.0)).xy, 0, 1);
			Output_TexCoord.TexCoord = vec2(Input_TexCoord[1].TexCoord.x - dot(length_a * miter_a, v1) * TextureRepeatLength / Thickness, 0);
			EmitVertex();

			gl_Position = vec4((PMatrix * vec3(p2 + length_b * miter_b, 1.0)).xy, 0, 1);
			Output_TexCoord.TexCoord = vec2(Input_TexCoord[1].TexCoord.x + (length(p2 - p1) + dot(length_b * miter_b, v1)) * TextureRepeatLength / Thickness, 0);
			EmitVertex();

			gl_Position = vec4((PMatrix * vec3(p2 - length_b * miter_b, 1.0)).xy, 0, 1);
			Output_TexCoord.TexCoord = vec2(Input_TexCoord[1].TexCoord.x + (length(p2 - p1) - dot(length_b * miter_b, v1)) * TextureRepeatLength / Thickness, 0);
			EmitVertex();
		}
		else
		{
			gl_Position = vec4((PMatrix * MVMatrix * vec3(p1 + length_a * miter_a, 1.0)).xy, 0, 1);
			Output_TexCoord.TexCoord = vec2(Input_TexCoord[1].TexCoord.x + dot(length_a * miter_a, v1) * TextureRepeatLength / Thickness, 0);
			EmitVertex();

			gl_Position = vec4((PMatrix * MVMatrix * vec3(p1 - length_a * miter_a, 1.0)).xy, 0, 1);
			Output_TexCoord.TexCoord = vec2(Input_TexCoord[1].TexCoord.x - dot(length_a * miter_a, v1) * TextureRepeatLength / Thickness, 0);
			EmitVertex();

			gl_Position = vec4((PMatrix * MVMatrix * vec3(p2 + length_b * miter_b, 1.0)).xy, 0, 1);
			Output_TexCoord.TexCoord = vec2(Input_TexCoord[1].TexCoord.x + (length(p2 - p1) + dot(length_b * miter_b, v1)) * TextureRepeatLength / Thickness, 0);
			EmitVertex();

			gl_Position = vec4((PMatrix * MVMatrix * vec3(p2 - length_b * miter_b, 1.0)).xy, 0, 1);
			Output_TexCoord.TexCoord = vec2(Input_TexCoord[1].TexCoord.x + (length(p2 - p1) - dot(length_b * miter_b, v1)) * TextureRepeatLength / Thickness, 0);
			EmitVertex();
		}
	}
	else
	{
		if (ThicknessInPixels)
		{
			gl_Position = vec4((PMatrix * vec3(p1 + length_a * miter_a, 1.0)).xy, 0, 1);
			Output_TexCoord.TexCoord = vec2(0, 0);
			EmitVertex();

			gl_Position = vec4((PMatrix * vec3(p1 - length_a * miter_a, 1.0)).xy, 0, 1);
			Output_TexCoord.TexCoord = vec2(TextureRepeatLength, 0);
			EmitVertex();

			gl_Position = vec4((PMatrix * vec3(p2 + length_b * miter_b, 1.0)).xy, 0, 1);
			Output_TexCoord.TexCoord = vec2(0, 0);
			EmitVertex();

			gl_Position = vec4((PMatrix * vec3(p2 - length_b * miter_b, 1.0)).xy, 0, 1);
			Output_TexCoord.TexCoord = vec2(TextureRepeatLength, 0);
			EmitVertex();
		}
		else
		{
			gl_Position = vec4((PMatrix * MVMatrix * vec3(p1 + length_a * miter_a, 1.0)).xy, 0, 1);
			Output_TexCoord.TexCoord = vec2(0, 0);
			EmitVertex();

			gl_Position = vec4((PMatrix * MVMatrix * vec3(p1 - length_a * miter_a, 1.0)).xy, 0, 1);
			Output_TexCoord.TexCoord = vec2(TextureRepeatLength, 0);
			EmitVertex();

			gl_Position = vec4((PMatrix * MVMatrix * vec3(p2 + length_b * miter_b, 1.0)).xy, 0, 1);
			Output_TexCoord.TexCoord = vec2(0, 0);
			EmitVertex();

			gl_Position = vec4((PMatrix * MVMatrix * vec3(p2 - length_b * miter_b, 1.0)).xy, 0, 1);
			Output_TexCoord.TexCoord = vec2(TextureRepeatLength, 0);
			EmitVertex();
		}
	}

	EndPrimitive();
}
