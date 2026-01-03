#version 430

uniform vec2 WindowSize;

layout (location = 0) in vec2 Position;
layout (location = 1) in vec4 Colour;

out ColourData
{
	vec4 Colour;
} Output_Colour;

void main()
{
	gl_Position = vec4(Position.x * 2 / WindowSize.x - 1, (Position.y * - 2 / WindowSize.y) + 1, 0.0, 1.0);

    Output_Colour.Colour = vec4(Colour.xyz * Colour.w, Colour.w);
}