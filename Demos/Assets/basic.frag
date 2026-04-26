#version 430

in vec3 Colour;

out vec4 FragColour;

void main()
{
    FragColour = vec4(Colour, 1.0);
}