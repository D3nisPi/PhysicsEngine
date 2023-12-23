#version 330

layout(location = 0) in vec3 aPosition;

layout(location = 2) in vec2 aTexCoord;

out vec2 texCoord;

// Transformation matrices
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main(void)
{
    texCoord = aTexCoord;

    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
}