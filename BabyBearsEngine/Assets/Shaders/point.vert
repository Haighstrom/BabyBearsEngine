#version 430

uniform mat3 PMatrix;
uniform float PointSize; // Allow dynamic point size control

layout (location = 0) in vec2 Position;
layout (location = 1) in vec4 Colour;

out ColourData
{
	vec4 Colour;
} Output_Colour;

void main()
{
    gl_Position = vec4(PMatrix * vec3(Position, 1), 1);
    gl_PointSize = PointSize;

    Output_Colour.Colour = vec4(Colour.xyz * Colour.w, Colour.w);
}