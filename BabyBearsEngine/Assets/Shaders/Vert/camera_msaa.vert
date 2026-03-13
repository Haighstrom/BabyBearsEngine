#version 430

uniform mat3 MVMatrix;
uniform mat3 PMatrix;

layout (location = 0) in vec2 Position;
layout (location = 2) in vec2 TexCoord;

out TexCoordData
{
   vec2 TexCoord;
} Output_TexCoord;

void main()
{
   gl_Position = vec4(PMatrix * MVMatrix * vec3(Position, 1), 1);
   Output_TexCoord.TexCoord = TexCoord;
}