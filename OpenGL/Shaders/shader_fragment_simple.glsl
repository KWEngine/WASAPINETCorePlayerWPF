#version 330

uniform vec3 uBaseColor;

out vec4 color;
//out vec4 bloom;

void main()
{
	color = vec4(uBaseColor, 1.0);
	//bloom = vec4(0.0);
}