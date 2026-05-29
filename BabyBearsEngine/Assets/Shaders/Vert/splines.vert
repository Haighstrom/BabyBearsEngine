// Pass-through vertex shader for splines and line series. Does not apply the MV/projection
// matrix; carries previous/next neighbour positions for the geometry-shader stage to spline
// against.
#version 150

in vec2 Position;
in vec2 Next;
in vec2 Previous;
in vec4 Colour;

out NeighbourPositionsData
{
	vec2 NextPos;
	vec2 PrevPos;
} Output_NeighbourPositions;

out ColourData
{
	vec4 Colour;
} Output_Colour;

void main()
{
	gl_Position = vec4(Position, 0, 1);

	Output_NeighbourPositions.NextPos = Next;
	Output_NeighbourPositions.PrevPos = Previous;

	Output_Colour.Colour = vec4(Colour.xyz * Colour.w, Colour.w);
}
