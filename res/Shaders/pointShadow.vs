#version 420

in vec3 position;
in vec2 texCoord;
in vec3 normal;
in vec3 tangent;

uniform mat4 MVP;
uniform mat4 Model;
out VS_OUT
{
	vec2 TexCoords;
} vs_out;

void main()
{
	vs_out.TexCoords = texCoord;
	gl_Position = Model*vec4(position, 1.0);
}
