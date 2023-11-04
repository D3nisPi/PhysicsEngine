using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OpenGL.Shaders
{
    public class Shader
    {
        private int _vertShader; // Vertex shader id
        private int _fragShader; // Fragment shader id
        private int _program; // Shader program
        public Shader(string vertPath, string fragPath)
        {
            // Create shaders
            _vertShader = CreateShader(ShaderType.VertexShader, vertPath);
            _fragShader = CreateShader(ShaderType.FragmentShader, fragPath);

            // Create program
            _program = GL.CreateProgram();

            // Link shaders
            GL.AttachShader(_program, _vertShader);
            GL.AttachShader(_program, _fragShader);
            GL.LinkProgram(_program);


            // Check for errors
            GL.GetProgram(_program, GetProgramParameterName.LinkStatus, out var status);
            if (status != (int)All.True)
            {
                throw new Exception($"Error in compiling shader program #{_program}\nError: {GL.GetProgramInfoLog(_program)}");
            }

            // Delete shaders
            DeleteShader(_vertShader);
            DeleteShader(_fragShader);

        }
        private int CreateShader(ShaderType type, string path)
        {
            string shaderCode = File.ReadAllText(path);
            int shaderID = GL.CreateShader(type);

            // Compile shader
            GL.ShaderSource(shaderID, shaderCode);
            GL.CompileShader(shaderID);

            // Check for errors
            GL.GetShader(shaderID, ShaderParameter.CompileStatus, out var status);
            if (status != (int)All.True)
            {
                throw new Exception($"Error in compiling shader #{shaderID}\nОшибка: {GL.GetShaderInfoLog(shaderID)}");
            }

            return shaderID;
        }
        public void ActivateProgram()
        {
            GL.UseProgram(_program);
        }
        public void DeactivateProgram()
        {
            GL.UseProgram(0);
        }
        public void DeleteProgram()
        {
            GL.DeleteProgram(_program);
        }
        private void DeleteShader(int shaderID)
        {
            GL.DetachShader(_program, shaderID);
            GL.DeleteShader(shaderID);
        }
        public void SetMatrixUniform(string name, ref Matrix4 data)
        {
            int location = GL.GetUniformLocation(_program, name);
            GL.UniformMatrix4(location, true, ref data);
        }
    }
}
