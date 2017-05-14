#version 330 core
in vec4 FragPos;

uniform vec3 lightPos;
uniform float farPlane;
out vec4 color;

void main()
{
    // get distance between fragment and light source
    float lightDistance = length(FragPos.xyz - lightPos);
    
    // map to [0;1] range by dividing by far_plane
    lightDistance = lightDistance / farPlane;
    
    // Write this as modified depth
    gl_FragDepth = (lightDistance);
     //color = vec4(lightDistance,lightDistance,lightDistance,1); why this is not working??
}  
