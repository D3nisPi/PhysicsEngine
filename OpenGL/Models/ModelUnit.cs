using OpenTK.Mathematics;
using OpenGL.Textures;
using OpenGL.Objects;

namespace OpenGL.Models
{
    public class ModelUnit
    {
        private const int VERTEXLOCATION = 0; // Location of position vector in shader
        private const int TEXTURELOCATION = 1; // Location of texture vector in shader
        private const int VERTEXCOUNT = 3; // Amount of floats to describe a vertex (xyz)
        private const int TEXTURECOUNT = 2; // Amount of floats to describe a texture coordinate (xy)

        private Model _model;

        private Texture? _texture;
        //private Material _material // for light fx

        private VertexArrayObject _vaoColor;
        private VertexArrayObject? _vaoTexture;

        private VertexBufferObject _vboVertices;
        private VertexBufferObject? _vboTextures;

        private ElementBufferObject _ebo;

        //Constructor for model without texture
        public ModelUnit(float[] vertices, uint[] indices)
        {
            // Creating VBOs for vertices and colors

            _vboVertices = new VertexBufferObject(vertices);

            // Creating VAO

            _vaoColor = new VertexArrayObject();
            _vaoColor.Activate();

            _vboVertices.Activate(); // Bind the vbo that stores vertices
            _vaoColor.AttribPointer(VERTEXLOCATION, VERTEXCOUNT, VERTEXCOUNT, 0);

            // Creating EBO

            _ebo = new ElementBufferObject(indices);

            // Unbind all

            VertexBufferObject.DeactivateObjects();
            VertexArrayObject.DeactivateObjects();
            ElementBufferObject.DeactivateObjects();
        }
        public ModelUnit(float[] vertices, uint[] indices, float[] texCoords, Texture texture)
        {
            _texture = texture;

            // Creating VBOs for vertices, colors and texture coordinates

            _vboVertices = new VertexBufferObject(vertices);
            _vboTextures = new VertexBufferObject(texCoords);

            // Creating VAOs

            _vaoColor = new VertexArrayObject();
            _vaoColor.Activate();

            _vboVertices.Activate(); // Bind the vbo that stores vertices
            _vaoColor.AttribPointer(VERTEXLOCATION, VERTEXCOUNT, VERTEXCOUNT, 0);


            _vaoTexture = new VertexArrayObject();
            _vaoTexture.Activate();

            _vboVertices.Activate(); // Bind the vbo that stores vertices
            _vaoColor.AttribPointer(VERTEXLOCATION, VERTEXCOUNT, VERTEXCOUNT, 0);

            _vboTextures.Activate();
            _vaoTexture.AttribPointer(TEXTURELOCATION, TEXTURECOUNT, TEXTURECOUNT, 0);

            // Creating EBO

            _ebo = new ElementBufferObject(indices);

            // Unbind all

            VertexBufferObject.DeactivateObjects();
            VertexArrayObject.DeactivateObjects();
            ElementBufferObject.DeactivateObjects();
        }
        public void SetModel(Model model)
        {
            _model = model;
        }
        public void Draw()
        {
            switch (_model.DrawMode)
            {
                case DrawMode.Color:
                    _vaoColor.DrawElements(_ebo);
                    break;
                case DrawMode.Texture:
                    _texture!.Activate();
                    _vaoTexture!.DrawElements(_ebo);
                    break;
            }
        }
        public void Scale(float scaleX, float scaleY, float scaleZ)
        {
            for (int i = 0; i < _vboVertices.Vertices.Length; i = i + VERTEXCOUNT)
            {
                _vboVertices.Vertices[i] = (_vboVertices.Vertices[i] - _model.Position.X) * scaleX + _model.Position.X;
                _vboVertices.Vertices[i + 1] = (_vboVertices.Vertices[i + 1] - _model.Position.Y) * scaleY + _model.Position.Y;
                _vboVertices.Vertices[i + 2] = (_vboVertices.Vertices[i + 2] - _model.Position.Z) * scaleZ + _model.Position.Z;
            }

            _vboVertices.UpdateData();
        }
        public void Move(float shiftX, float shiftY, float shiftZ)
        {
            for (int i = 0; i < _vboVertices.Vertices.Length; i = i + VERTEXCOUNT)
            {
                _vboVertices.Vertices[i] += shiftX;
                _vboVertices.Vertices[i + 1] += shiftY;
                _vboVertices.Vertices[i + 2] += shiftZ;
            }

            _vboVertices.UpdateData();
        }
        public void RotateX(float angleRad)
        {
            for (int i = 0; i < _vboVertices.Vertices.Length; i = i + VERTEXCOUNT)
            {
                float x = _vboVertices.Vertices[i] - _model.Position.X;
                float y = _vboVertices.Vertices[i + 1] - _model.Position.Y;
                float z = _vboVertices.Vertices[i + 2] - _model.Position.Z;

                _vboVertices.Vertices[i] = x + _model.Position.X;
                _vboVertices.Vertices[i + 1] = MathF.Cos(angleRad) * y - MathF.Sin(angleRad) * z + _model.Position.Y;
                _vboVertices.Vertices[i + 2] = MathF.Sin(angleRad) * y + MathF.Cos(angleRad) * z + _model.Position.Z;
            }

            _vboVertices.UpdateData();
        }
        public void RotateY(float angleRad)
        {
            for (int i = 0; i < _vboVertices.Vertices.Length; i = i + VERTEXCOUNT)
            {
                float x = _vboVertices.Vertices[i] - _model.Position.X;
                float y = _vboVertices.Vertices[i + 1] - _model.Position.Y;
                float z = _vboVertices.Vertices[i + 2] - _model.Position.Z;

                _vboVertices.Vertices[i] = MathF.Cos(angleRad) * x + MathF.Sin(angleRad) * z + _model.Position.X;
                _vboVertices.Vertices[i + 1] = y + _model.Position.Y;
                _vboVertices.Vertices[i + 2] = -MathF.Sin(angleRad) * x + MathF.Cos(angleRad) * z + _model.Position.Z;
            }

            _vboVertices.UpdateData();
        }
        public void RotateZ(float angleRad)
        {
            for (int i = 0; i < _vboVertices.Vertices.Length; i = i + VERTEXCOUNT)
            {
                float x = _vboVertices.Vertices[i] - _model.Position.X;
                float y = _vboVertices.Vertices[i + 1] - _model.Position.Y;
                float z = _vboVertices.Vertices[i + 2] - _model.Position.Z;

                _vboVertices.Vertices[i] = MathF.Cos(angleRad) * x - MathF.Sin(angleRad) * y + _model.Position.X;
                _vboVertices.Vertices[i + 1] = MathF.Sin(angleRad) * x + MathF.Cos(angleRad) * y + _model.Position.Y;
                _vboVertices.Vertices[i + 2] = z + _model.Position.Z;
            }

            _vboVertices.UpdateData();
        }
        public void Rotate(float angleRadX, float angleRadY, float angleRadZ)
        {
            RotateX(angleRadX);
            RotateY(angleRadY);
            RotateZ(angleRadZ);
        }
    }
}