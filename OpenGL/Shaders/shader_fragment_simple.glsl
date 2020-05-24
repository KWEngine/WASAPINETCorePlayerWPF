#version 330

in float vHeight;

//uniform vec3 uBaseColor;

out vec4 color;
//out vec4 bloom;

void main()
{
	vec3 tmpColor = vec3(vHeight * 2.0, 1.0 - vHeight, 0.0);
	color = vec4(tmpColor, 1.0);
	//bloom = vec4(0.0);
}