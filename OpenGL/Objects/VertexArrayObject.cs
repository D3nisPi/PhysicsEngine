using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace OpenGL.Objects
{
    /// <summary>
    /// Wrapper class over vertex array object (VAO)
    /// </summary>
    public class VertexArrayObject
    {
        public int Id { get; private set; }
        public VertexArrayObject()
        {
            Id = GL.GenVertexArray();
        }
        public void Activate()
        {
            GL.BindVertexArray(Id);
        }
        public void Deactivate()
        {
            GL.BindVertexArray(0);
        }
        public void AttribPointer(int index, int size, int stride, int offset)
        {
            GL.VertexAttribPointer(index, size, VertexAttribPointerType.Float, true, stride * sizeof(float), offset * sizeof(float));
            GL.EnableVertexAttribArray(index);
        }
        public void DrawElements(uint[] indices, VertexBufferObject vbo, ElementBufferObject ebo)
        {
            // Binding objects
            Activate();
            vbo.Activate();
            ebo.Activate();       
            // Drawing
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
            // Unbinding objects
            Deactivate();
            ebo.Deactivate();
            vbo.Deactivate();
        }
    }
}
