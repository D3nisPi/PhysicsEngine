using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using GLObjects;

namespace Models
{
    public class Model3D
    {
        private const int VERTEXLOCATION = 0; // Location of position vector in shader
        private const int COLORLOCATION = 1; // Location of color vector in shader
        private const int VERTEXCOUNT = 3; // Amount of floats to describe a vertex (xyz)
        private const int COLORCOUNT = 4; // Amount of floats to describe a color (rgba)

        private float[] _vertices;
        private float[] _colors;
        private uint[] _indices;

        //private Shader _shader
        //private Texture _texture;
        //private Material _material // for light fx

        private VertexArrayObject _vao;
        private VertexBufferObject _vboVertices;
        private VertexBufferObject _vboColors;
        private ElementBufferObject _ebo;

        private Vector3 _position;
        private Size _size;
        private RotationAngles _rotation;
        public Vector3 Position { get => _position; }
        public Size Size { get => _size;  }
        public RotationAngles Rotation { get => _rotation;  }

        public Vector3 MovementPerSecond;
        public Size ScalingPerSecond;
        public RotationAngles RotationPerSecond;

        public Model3D(float[] vertices, float[] colors, uint[] indices)
        {
            _position = new Vector3();
            _size = new Size();
            _rotation = new RotationAngles();

            MovementPerSecond = Vector3.Zero;
            ScalingPerSecond = Size.One;
            RotationPerSecond = RotationAngles.Zero;

            _vertices = (float[])vertices.Clone();
            _colors = (float[])colors.Clone();
            _indices = (uint[])indices.Clone();

            // Creating VBOs for vertices and colors
          
            _vboVertices = new VertexBufferObject(_vertices);
            _vboColors = new VertexBufferObject(_colors);

            // Creating VAO

            _vao = new VertexArrayObject();
            _vao.Activate();

            _vboVertices.Activate(); // Bind the vbo that stores vertices
            _vao.AttribPointer(VERTEXLOCATION, VERTEXCOUNT, VERTEXCOUNT, 0);

            _vboColors.Activate(); // Bind the vbo that stores colors
            _vao.AttribPointer(COLORLOCATION, COLORCOUNT, COLORCOUNT, 0);

            // Creating EBO

            _ebo = new ElementBufferObject(_indices);

            // Unbind all

            _vboVertices.Deactivate();
            _vao.Deactivate();
            _ebo.Deactivate();
        }
        public void SetColor(Vector4 color)
        {
            for (int i = 0; i < _colors.Length; i = i + COLORCOUNT)
            {
                _colors[i] = color.X;
                _colors[i + 1] = color.Y;
                _colors[i + 2] = color.Z;
                _colors[i + 3] = color.W;
            }

            _vboColors.SetData(_colors);
        }
        public void Draw()
        {
            _vao.DrawElements(_indices, _vboVertices, _ebo); 
        }
        public void Update(float milliseconds)
        {
            if (MovementPerSecond != Vector3.Zero)
                Move(MovementPerSecond.X * milliseconds, MovementPerSecond.Y * milliseconds, MovementPerSecond.Z * milliseconds);
            if (ScalingPerSecond != Size.One)
                Scale(ScalingPerSecond.X * milliseconds, ScalingPerSecond.Y * milliseconds, ScalingPerSecond.Z * milliseconds);
            if (RotationPerSecond != RotationAngles.Zero)
                Rotate(RotationPerSecond.X * milliseconds, RotationPerSecond.Y * milliseconds, RotationPerSecond.Z * milliseconds);
        }
        public void Scale(float scaleX, float scaleY, float scaleZ)
        {
            for (int i = 0; i < _vertices.Length; i = i + VERTEXCOUNT)
            {
                _vertices[i] =  (_vertices[i] - Position.X) * scaleX + Position.X;
                _vertices[i + 1] = (_vertices[i + 1] - Position.Y) * scaleY + Position.Y;
                _vertices[i + 2] = (_vertices[i + 2] - Position.Z) * scaleZ + Position.Z;
            }

            _vboVertices.SetData(_vertices);

            _size.X *= scaleX;
            _size.Y *= scaleY;
            _size.Z *= scaleZ;
        }

        public void Move(float shiftX, float shiftY, float shiftZ)
        {
            for (int i = 0; i < _vertices.Length; i = i + VERTEXCOUNT)
            {
                _vertices[i] += shiftX;
                _vertices[i + 1] += shiftY;
                _vertices[i + 2] += shiftZ;
            }
            
            _vboVertices.SetData(_vertices);

            _position.X += shiftX;
            _position.Y += shiftY;
            _position.Z += shiftZ;
        }

        public void RotateX(float angleRad)
        {
            for (int i = 0; i < _vertices.Length; i = i + VERTEXCOUNT)
            {
                float x = _vertices[i] - Position.X;
                float y = _vertices[i + 1] - Position.Y;
                float z = _vertices[i + 2] - Position.Z;

                _vertices[i] = x + Position.X; 
                _vertices[i + 1] = MathF.Cos(angleRad) * y - MathF.Sin(angleRad) * z + Position.Y;
                _vertices[i + 2] = MathF.Sin(angleRad) * y + MathF.Cos(angleRad) * z + Position.Z;
            }
            
            _vboVertices.SetData(_vertices);

            _rotation.X += angleRad;
            _rotation.X = _rotation.NormalizedX;
        }
        public void RotateY(float angleRad)
        {
            for (int i = 0; i < _vertices.Length; i = i + VERTEXCOUNT)
            {
                float x = _vertices[i] - Position.X;
                float y = _vertices[i + 1] - Position.Y;
                float z = _vertices[i + 2] - Position.Z;

                _vertices[i] = MathF.Cos(angleRad) * x + MathF.Sin(angleRad) * z + Position.X;
                _vertices[i + 1] = y + Position.Y;
                _vertices[i + 2] = -MathF.Sin(angleRad) * x + MathF.Cos(angleRad) * z + Position.Z;
            }
            
            _vboVertices.SetData(_vertices);

            _rotation.Y += angleRad;
            _rotation.Y = _rotation.NormalizedY;
        }
        public void RotateZ(float angleRad)
        {
            for (int i = 0; i < _vertices.Length; i = i + VERTEXCOUNT)
            {
                float x = _vertices[i] - Position.X;
                float y = _vertices[i + 1] - Position.Y;
                float z = _vertices[i + 2] - Position.Z;

                _vertices[i] = MathF.Cos(angleRad) * x - MathF.Sin(angleRad) * y + Position.X;
                _vertices[i + 1] = MathF.Sin(angleRad) * x + MathF.Cos(angleRad) * y + Position.Y;
                _vertices[i + 2] = z + Position.Z;
            }
            
            _vboVertices.SetData(_vertices);

            _rotation.Z += angleRad;
            _rotation.Z = _rotation.NormalizedZ;
        }
        public void Rotate(float angleRadX, float angleRadY, float angleRadZ)
        {
            RotateX(angleRadX);
            RotateY(angleRadY);
            RotateZ(angleRadZ);
        }

        public static Model3D ParseOBJ(string filePath)
        {
            return ParseOBJ(filePath, new Vector4(1, 1, 1, 1));
        }
        public static Model3D ParseOBJ(string filePath, Vector4 defaultColor)
        {
            //-----------------------
            // To do:
            // 1) Parse textures
            // 2) Parse normals
            //-----------------------

            List<float> vertices = new List<float>();
            List<uint> indices = new List<uint>();
            List<float> colors = new List<float>();
            StreamReader? reader = null;
            try
            {
                reader = new StreamReader(filePath);
                string? line = reader.ReadLine();
                while (line != null)
                {
                    line = line.Replace('.', ',');
                    if (line.StartsWith("v "))
                    {
                        string[] tokens = line.Split(' ');

                        vertices.Add(float.Parse(tokens[1]));
                        vertices.Add(float.Parse(tokens[2]));
                        vertices.Add(float.Parse(tokens[3]));

                        colors.Add(defaultColor.X);
                        colors.Add(defaultColor.Y);
                        colors.Add(defaultColor.Z);
                        colors.Add(defaultColor.W);
                    }
                    else if (line.StartsWith("f "))
                    {
                        string[] tokens = line.Split(' ');
                        for (int i = 1; i < tokens.Length; i++)
                        {
                            string[] parts = tokens[i].Split('/');
                            indices.Add(Convert.ToUInt32(parts[0]) - 1);
                        }
                    }
                    line = reader.ReadLine();
                }
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred during parsing\nError: {e.Message}");
            }
            finally
            {
                reader?.Close();
            }
            return new Model3D(vertices.ToArray(), colors.ToArray(), indices.ToArray());
        }
    }

    
}