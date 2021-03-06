﻿#version 330

in		vec3 aPosition;

out		float vHeight;

uniform mat4 uVP;
uniform int uBinId;
uniform float uHeight[510];
uniform float uWindowHeight;
uniform float uBinWidth;
uniform float uBinOffset;
uniform float uStep;

mat4 createModelMatrix()
{
	return mat4(
		uBinWidth, 0.0, 0.0, 0.0,
		0.0, uHeight[gl_InstanceID] / 2.0, 0.0, 0.0,
		0.0, 0.0, 1.0, 0.0,
		uBinOffset + uStep * gl_InstanceID, uWindowHeight, 0.0, 1.0
		);
	
}

void main()
{
	vHeight = uHeight[gl_InstanceID] / 100.0;
	gl_Position = uVP * createModelMatrix() * vec4(aPosition, 1.0);
}