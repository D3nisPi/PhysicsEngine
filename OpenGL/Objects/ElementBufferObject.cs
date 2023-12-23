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
        public uint[] Indices;
        public int Id { get; private set; }

        public ElementBufferObject(uint[] indices)
        {
            Id = GL.GenBuffer();
            Indices = (uint[])indices.Clone();
            UpdateData();
        }
        public void Activate()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Id);
        }
        public void Deactivate()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }
        public static void DeactivateObjects()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }
        public void UpdateData()
        {
            Activate();
            GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Length * sizeof(uint), Indices, BufferUsageHint.DynamicDraw);
            Deactivate();
        }
    }
}
