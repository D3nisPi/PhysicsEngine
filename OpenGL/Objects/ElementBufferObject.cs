using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace OpenGL.Objects
{
    /// <summary>
    /// Wrapper class over element buffer object (EBO)
    /// </summary>
    public class ElementBufferObject
    {
        public int Id { get; private set; }

        public ElementBufferObject(uint[] indices)
        {
            Id = GL.GenBuffer();
            SetData(indices);
        }
        public void Activate()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Id);
        }
        public void Deactivate()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }
        public void SetData(uint[] indices)
        {
            Activate();
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.DynamicDraw);
            Deactivate();
        }
    }
}
