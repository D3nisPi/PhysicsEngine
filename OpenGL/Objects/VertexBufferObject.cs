using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;


namespace OpenGL.Objects
{
    /// <summary>
    /// Wrapper class over vertex buffer object (VBO)
    /// </summary>
    public class VertexBufferObject
    {
        public int Id { get; private set; }

        public VertexBufferObject(float[] vertices)
        {
            Id = GL.GenBuffer();
            SetData(vertices);
        }
        public void Activate()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, Id);
        }
        public void Deactivate()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
        public void SetData(float[] vertices)
        {
            Activate();
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            Deactivate();
        }
    }
}
