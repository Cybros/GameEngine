#version 420

in vec3 position;
in vec2 texCoord;
in vec3 normal;
in vec3 tangent;


out vec2 texCoord0;
out vec3 normal0;
out vec3 worldPos0;
out vec4 shadowMapCoords0;
out vec3 T;
out vec3 B;
out vec3 N;

uniform mat4 MVP;
uniform mat4 Model;
uniform mat4 lightMatrix;
uniform vec4 clipPlane;

void main()
{
	gl_Position = MVP*vec4(position, 1.0);
	shadowMapCoords0 =  lightMatrix*vec4(position, 1.0);
	texCoord0 = texCoord;
	normal0 = normal;
	worldPos0 =vec3(Model*vec4(position,1.0));
	gl_ClipDistance[0] = dot( vec4(worldPos0,1) , clipPlane);

	vec3 n = normalize((Model*vec4(normal ,0.0)).xyz);
	vec3 t = normalize((Model*vec4(tangent , 0.0)).xyz);

	t = normalize(t - dot(t,n)*n);

	vec3 bitangent = cross(t,n);
	T = t;
	B = bitangent;
	N = n;
}
