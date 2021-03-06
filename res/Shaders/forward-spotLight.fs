#version 420

in vec2 texCoord0;
in vec3 worldPos0;
in vec3 normal0;
in vec4 shadowMapCoords0;
in vec3 T;
in vec3 B;
in vec3 N;
out vec4 color;

struct BaseLight
{
	vec3 color;
	float intensity;
};
struct Attenuation
{
	float constant;
	float linear;
	float exponent;
};
struct SpotLight
{
	BaseLight base;
	vec3 position;
	vec3 direction;
	Attenuation atten;
	float range;
	float cut_off;
};

uniform sampler2D diffuse;
uniform sampler2D normalMap;
uniform sampler2D dispMap;
uniform sampler2D shadowMap;
uniform float dispMapScale;
uniform float dispMapBias;
uniform float shadowBias;
uniform vec3 eyePos;
uniform vec3 shadowTexelSize; 
uniform float specularPower;
uniform float specularIntensity;
uniform SpotLight spotLight;
uniform vec4 fogColor;


vec4 calcLightDiffuse(BaseLight base , vec3 direction , vec3 normal)
{
	float diffuseFactor = dot(normalize(normal) , -normalize(direction));
	vec4 diffuseColor = vec4(0,0,0,0);
	
	if(diffuseFactor >= 0)
	{
		diffuseColor = vec4(base.color,1)*base.intensity*diffuseFactor;
	}
	return diffuseColor;
}
vec4 calcLightSpec(BaseLight base , vec3 direction , vec3 normal)
{
	vec3 directionToEye = normalize(eyePos-worldPos0);
	vec3 reflectDirection = normalize(reflect(direction , normal));
	vec4 specularColor = vec4(0,0,0,0);

	float specularFactor = dot(directionToEye , reflectDirection);
	if(specularFactor > 0 )
	{
		specularFactor = pow(specularFactor , specularPower);
		specularColor = vec4(base.color , 1.0)*specularFactor*specularIntensity;
	}
	return specularColor;
}
vec2 calcParallaxTexCoords(sampler2D dMap , mat3 matrix , vec3 directionToEye , vec2 texCoords , float scale,
float bias)
{
	vec2 offset = (directionToEye * matrix).xy * (texture2D(dMap, texCoords.xy).r * scale + bias);
	vec2 texNew = texCoords.xy;
	texNew.x += offset.x;
	texNew.y -= offset.y;
	return texNew;
}
float sampleShadowMap(sampler2D map , vec2 coords , float compare)
{
	return step(compare , texture2D(map , coords.xy).r);
}
float sampleShadowMapLinear(sampler2D map , vec2 coords , float compare , vec2 texelSize)
{
	vec2 pixelPos = coords/texelSize + vec2(0.5);
	vec2 fracPart = fract(pixelPos);
	vec2 startTexel = (pixelPos - fracPart)*texelSize;
	
	float blTexel = sampleShadowMap(map , startTexel , compare);
	float brTexel = sampleShadowMap(map , startTexel + vec2(texelSize.x ,0.0) , compare);
	float tlTexel = sampleShadowMap(map , startTexel + vec2(0.0 , texelSize.y) , compare);
	float trTexel = sampleShadowMap(map , startTexel + texelSize , compare);

	float mixA = mix(blTexel , tlTexel , fracPart.y);
	float mixB = mix(brTexel , trTexel , fracPart.y);
	
	return mix(mixA , mixB , fracPart.x);
}
float sampleShadowMapPCF(sampler2D map , vec2 coords , float compare , vec2 texelSize)
{
	float result = 0.0f;
	for(float y = -1.0f ; y<= 1.0f ; y += 1.0f)
	{
		for(float x = -1.0f ; x<= 1.0f ; x += 1.0f)
		{
			vec2 coordsOffset = vec2(x,y)*texelSize;
			result += sampleShadowMapLinear(map , coords + coordsOffset , compare , texelSize);
		}
	}
	return result/9.0f;
}
float calcShadowMapEffect(sampler2D map , vec4 coords )
{
	vec3 finalCoords = (coords.xyz/coords.w)*vec3(0.5) + vec3(0.5);
	return sampleShadowMapPCF(map , finalCoords.xy , finalCoords.z - shadowBias , shadowTexelSize.xy);	
}
void main()
{

	vec4 tspec = vec4(0.0,0.0,0.0,1.0);
	vec4 tdiff = vec4(0.0,0.0,0.0,1.0);
	
	vec3 distanceVector = worldPos0 - spotLight.position;
	float distanceToPoint = length(distanceVector);
	distanceVector = normalize(distanceVector);
	mat3 matrix = mat3(T , B , N);

	vec3 directionToEye = normalize(eyePos - worldPos0);
	vec2 texcoords = calcParallaxTexCoords(dispMap,matrix,directionToEye,texCoord0,dispMapScale,dispMapBias);

	vec3 normal = normalize(matrix*(255.0/128.0*texture2D(normalMap , texcoords.xy).xyz - 1));
	vec4 dcolor = calcLightDiffuse(spotLight.base , distanceVector , normal);
	float attenu = spotLight.atten.constant + spotLight.atten.linear*distanceToPoint
		       + spotLight.atten.exponent*distanceToPoint*distanceToPoint + 0.01;
	float res = dot(distanceVector , normalize(spotLight.direction));
	float sinPhi =  length(cross(distanceVector , normalize(spotLight.direction)));
	
	if(dcolor.w > 0 && spotLight.range > distanceToPoint && res >spotLight.cut_off){
		float sinTheta = sqrt(1- spotLight.cut_off*spotLight.cut_off);
		float limit = spotLight.cut_off + sinTheta*0.035;
		float falloff = 1.0f;
		if(res < limit)
			falloff = (sinTheta*res - spotLight.cut_off*sinPhi)*28;
		tspec += calcLightSpec(spotLight.base , distanceVector , normal)*falloff/attenu;
		tdiff += dcolor*falloff/attenu;
		}
	float shadow = 	calcShadowMapEffect(shadowMap , shadowMapCoords0);
	vec3 finalCoords = (shadowMapCoords0.xyz/shadowMapCoords0.w)*vec3(0.5) + vec3(0.5);
	
	color = texture2D(diffuse, texcoords.xy)*(tdiff+ tspec)*shadow ;
	
		
}
