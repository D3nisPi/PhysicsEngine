#version 330

layout (location = 0) in vec3 aPosition; // Vertex coordinates
uniform vec4 aColor; // Vertex color

out vec4 vColor;

// Transformation matrices
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
	vColor = aColor;
	gl_Position = vec4(aPosition, 1.0) * model * view * projection;
}