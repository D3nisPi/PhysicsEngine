using OpenTK.Mathematics;
using OpenGL.Textures;
using OpenGL.Objects;
using OpenGL.Shaders;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace OpenGL.Models
{
    public enum DrawType
    {
        Color,
        Texture,
    }
    public class Model3D
    {
        private const int VERTEXLOCATION = 0; // Location of position vector in shader
        private const int COLORLOCATION = 1; // Location of color vector in shader
        private const int TEXTURELOCATION = 2; // Location of texture vector in shader
        private const int VERTEXCOUNT = 3; // Amount of floats to describe a vertex (xyz)
        private const int COLORCOUNT = 4; // Amount of floats to describe a color (rgba)
        private const int TEXTURECOUNT = 2; // Amount of floats to describe a texture coordinate (xy)

        public static readonly Vector4 DEFAULTCOLOR;

        private DrawType _drawType;
        public DrawType DrawType
        {
            get => _drawType;
            set
            {
                if (_texture == null)
                    throw new NullReferenceException("Required objects are not attached to this model");
                _drawType = value;
            }
        }

        private Shader _colorShader;
        private Shader? _textureShader;
        private Texture? _texture;
        //private Material _material // for light fx

        private VertexArrayObject _vaoColor;
        private VertexArrayObject? _vaoTexture;

        private VertexBufferObject _vboVertices;
        private VertexBufferObject _vboColors;
        private VertexBufferObject? _vboTextures;

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

        static Model3D()
        {
            DEFAULTCOLOR = new Vector4(1, 1, 1, 1);
        }
        //Constructor for model without texture
        public Model3D(float[] vertices, uint[] indices, float[] colors, Shader colorShader)
        {
            _position = Vector3.Zero;
            _size = Size.One;
            _rotation = RotationAngles.Zero;

            MovementPerSecond = Vector3.Zero;
            ScalingPerSecond = Size.One;
            RotationPerSecond = RotationAngles.Zero;

            _colorShader = colorShader;

            _drawType = DrawType.Color;

            // Creating VBOs for vertices and colors
          
            _vboVertices = new VertexBufferObject(vertices);
            _vboColors = new VertexBufferObject(colors);

            // Creating VAO

            _vaoColor = new VertexArrayObject();
            _vaoColor.Activate();

            _vboVertices.Activate(); // Bind the vbo that stores vertices
            _vaoColor.AttribPointer(VERTEXLOCATION, VERTEXCOUNT, VERTEXCOUNT, 0);

            _vboColors.Activate(); // Bind the vbo that stores colors
            _vaoColor.AttribPointer(COLORLOCATION, COLORCOUNT, COLORCOUNT, 0);

            // Creating EBO

            _ebo = new ElementBufferObject(indices);

            // Unbind all

            _vboVertices.Deactivate();
            _vaoColor.Deactivate();
            _ebo.Deactivate();
        }
        public Model3D(float[] vertices, uint[] indices, float[] colors, float[] texCoords, Shader colorShader, Shader textureShader, Texture texture)
        {
            _position = Vector3.Zero;
            _size = Size.One;
            _rotation = RotationAngles.Zero;

            MovementPerSecond = Vector3.Zero;
            ScalingPerSecond = Size.One;
            RotationPerSecond = RotationAngles.Zero;

            _colorShader = colorShader;
            _textureShader = textureShader;

            _texture = texture;

            _drawType = DrawType.Texture;

            // Creating VBOs for vertices, colors and texture coordinates

            _vboVertices = new VertexBufferObject(vertices);
            _vboColors = new VertexBufferObject(colors);
            _vboTextures = new VertexBufferObject(texCoords);

            // Creating VAOs

            _vaoColor = new VertexArrayObject();
            _vaoColor.Activate();

            _vboVertices.Activate(); // Bind the vbo that stores vertices
            _vaoColor.AttribPointer(VERTEXLOCATION, VERTEXCOUNT, VERTEXCOUNT, 0);

            _vboColors.Activate(); // Bind the vbo that stores colors
            _vaoColor.AttribPointer(COLORLOCATION, COLORCOUNT, COLORCOUNT, 0);


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

        public void SetColor(Vector4 color)
        {
            for (int i = 0; i < _vboColors.Vertices.Length; i = i + COLORCOUNT)
            {
                _vboColors.Vertices[i] = color.X;
                _vboColors.Vertices[i + 1] = color.Y;
                _vboColors.Vertices[i + 2] = color.Z;
                _vboColors.Vertices[i + 3] = color.W;
            }

            _vboColors.UpdateData();
        }
        public void Draw()
        {
            switch (DrawType)
            {
                case DrawType.Color:
                    _colorShader.ActivateProgram();
                    _vaoColor.DrawElements(_ebo);
                    break;
                case DrawType.Texture:              
                    _textureShader!.ActivateProgram();
                    _texture!.Activate();
                    _vaoTexture!.DrawElements(_ebo);
                    break;
            }
        }
        public void Update(float seconds)
        {
            if (MovementPerSecond != Vector3.Zero)
                Move(MovementPerSecond.X * seconds, MovementPerSecond.Y * seconds, MovementPerSecond.Z * seconds);
            if (ScalingPerSecond != Size.One)
                Scale(ScalingPerSecond.X * seconds, ScalingPerSecond.Y * seconds, ScalingPerSecond.Z * seconds);
            if (RotationPerSecond != RotationAngles.Zero)
                Rotate(RotationPerSecond.X * seconds, RotationPerSecond.Y * seconds, RotationPerSecond.Z * seconds);
        }
        public void Scale(float scaleX, float scaleY, float scaleZ)
        {
            for (int i = 0; i < _vboVertices.Vertices.Length; i = i + VERTEXCOUNT)
            {
                _vboVertices.Vertices[i] =  (_vboVertices.Vertices[i] - Position.X) * scaleX + Position.X;
                _vboVertices.Vertices[i + 1] = (_vboVertices.Vertices[i + 1] - Position.Y) * scaleY + Position.Y;
                _vboVertices.Vertices[i + 2] = (_vboVertices.Vertices[i + 2] - Position.Z) * scaleZ + Position.Z;
            }

            _vboVertices.UpdateData();

            _size.X *= scaleX;
            _size.Y *= scaleY;
            _size.Z *= scaleZ;
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

            _position.X += shiftX;
            _position.Y += shiftY;
            _position.Z += shiftZ;
        }
        public void RotateX(float angleRad)
        {
            for (int i = 0; i < _vboVertices.Vertices.Length; i = i + VERTEXCOUNT)
            {
                float x = _vboVertices.Vertices[i] - Position.X;
                float y = _vboVertices.Vertices[i + 1] - Position.Y;
                float z = _vboVertices.Vertices[i + 2] - Position.Z;

                _vboVertices.Vertices[i] = x + Position.X; 
                _vboVertices.Vertices[i + 1] = MathF.Cos(angleRad) * y - MathF.Sin(angleRad) * z + Position.Y;
                _vboVertices.Vertices[i + 2] = MathF.Sin(angleRad) * y + MathF.Cos(angleRad) * z + Position.Z;
            }
            
            _vboVertices.UpdateData();

            _rotation.X += angleRad;
            _rotation.X = _rotation.NormalizedX;
        }
        public void RotateY(float angleRad)
        {
            for (int i = 0; i < _vboVertices.Vertices.Length; i = i + VERTEXCOUNT)
            {
                float x = _vboVertices.Vertices[i] - Position.X;
                float y = _vboVertices.Vertices[i + 1] - Position.Y;
                float z = _vboVertices.Vertices[i + 2] - Position.Z;

                _vboVertices.Vertices[i] = MathF.Cos(angleRad) * x + MathF.Sin(angleRad) * z + Position.X;
                _vboVertices.Vertices[i + 1] = y + Position.Y;
                _vboVertices.Vertices[i + 2] = -MathF.Sin(angleRad) * x + MathF.Cos(angleRad) * z + Position.Z;
            }
            
            _vboVertices.UpdateData();

            _rotation.Y += angleRad;
            _rotation.Y = _rotation.NormalizedY;
        }
        public void RotateZ(float angleRad)
        {
            for (int i = 0; i < _vboVertices.Vertices.Length; i = i + VERTEXCOUNT)
            {
                float x = _vboVertices.Vertices[i] - Position.X;
                float y = _vboVertices.Vertices[i + 1] - Position.Y;
                float z = _vboVertices.Vertices[i + 2] - Position.Z;

                _vboVertices.Vertices[i] = MathF.Cos(angleRad) * x - MathF.Sin(angleRad) * y + Position.X;
                _vboVertices.Vertices[i + 1] = MathF.Sin(angleRad) * x + MathF.Cos(angleRad) * y + Position.Y;
                _vboVertices.Vertices[i + 2] = z + Position.Z;
            }
            
            _vboVertices.UpdateData();

            _rotation.Z += angleRad;
            _rotation.Z = _rotation.NormalizedZ;
        }
        public void Rotate(float angleRadX, float angleRadY, float angleRadZ)
        {
            RotateX(angleRadX);
            RotateY(angleRadY);
            RotateZ(angleRadZ);
        }

        public static Model3D ParseOBJ(string objPath, Shader colorShader)
        {
            return ParseOBJ(objPath, colorShader, DEFAULTCOLOR);
        }
        public static Model3D ParseOBJ(string objPath, Shader colorShader, Vector4 color)
        {
            //-----------------------
            // To do:
            // 1) Parse normals
            //-----------------------

            List<float> vertices = new List<float>();
            List<uint> indices = new List<uint>();

            using (var reader = new StreamReader(objPath))
            {
                string? line = reader.ReadLine();
                while (line != null)
                {
                    line = line.Replace('.', ',').Trim();
                    if (line.StartsWith("v "))
                    {
                        string[] tokens = line.Split(' ');

                        vertices.Add(float.Parse(tokens[1]));
                        vertices.Add(float.Parse(tokens[2]));
                        vertices.Add(float.Parse(tokens[3]));
                    }
                    else if (line.StartsWith("f "))
                    {
                        string[] tokens = line.Split(' ');
                        for (int i = 1; i < tokens.Length; i++)
                        {
                            string[] parts = tokens[i].Split('/');
                            indices.Add(uint.Parse(parts[0]) - 1);
                        }
                    }
                    line = reader.ReadLine();
                }
            }

            List<float> colors = new List<float>(vertices.Count / 3 * 4);
            for (int i = 0; i < vertices.Count / 3; i++)
            {
                colors.Add(color.X);
                colors.Add(color.Y);
                colors.Add(color.Z);
                colors.Add(color.W);
            }

            return new Model3D(vertices.ToArray(), indices.ToArray(), colors.ToArray(), colorShader);
        }
        public static Model3D ParseOBJ(string objPath, string texPath, Shader colorShader, Shader textureShader)
        {
            return ParseOBJ(objPath, texPath, colorShader, textureShader, DEFAULTCOLOR);
        }
        public static Model3D ParseOBJ(string objPath, string texPath, Shader colorShader, Shader textureShader, Vector4 color)
        {
            // One .obj file may contain multiple objects with multiple textures
            // To do:
            //      1) Parse normals info
            //      2) Parse multiple objects in 1 .obj file
            //      3) Get file paths from parsing, parse .mtl file info
            //      4) Add the ability to assign a color to each vertex

            List<float> v = new List<float>();
            List<float> vt = new List<float>();
            List<string> f = new List<string>();

            using (var reader = new StreamReader(objPath))
            {
                string? line = reader.ReadLine();
                while (line != null)
                {
                    line = line.Replace('.', ',').Trim();
                    if (line.StartsWith("v "))
                    {
                        string[] tokens = line.Split(' ');

                        v.Add(float.Parse(tokens[1]));
                        v.Add(float.Parse(tokens[2]));
                        v.Add(float.Parse(tokens[3]));
                    }
                    else if (line.StartsWith("vt "))
                    {
                        string[] tokens = line.Split(' ');
                        vt.Add(float.Parse(tokens[1]));
                        vt.Add(float.Parse(tokens[2]));
                    }
                    else if (line.StartsWith("f "))
                    {
                        string[] tokens = line.Split(' ');
                        f.Add(tokens[1]);
                        f.Add(tokens[2]);
                        f.Add(tokens[3]);
                    }
                    line = reader.ReadLine();
                }
            }

            List<float> vertices = new List<float>(v);
            List<float?> textureVertices = new List<float?>(new float?[vertices.Count / 3 * 2]);
            List<uint> indices = new List<uint>();
            Dictionary<int, List<int>> aliasIndices = new Dictionary<int, List<int>>();
            for (int i = 0; i < vertices.Count / 3; i++)
                aliasIndices[i] = new List<int>() { i };

            foreach (var vertexInfo in f)
            {
                string[] info = vertexInfo.Split("/");
                int vIndex = int.Parse(info[0]) - 1;
                int vtIndex = int.Parse(info[1]) - 1;
                int vnIndex = int.Parse(info[2]) - 1;

                if (textureVertices[vIndex * 2] == null && textureVertices[vIndex * 2 + 1] == null)
                {
                    textureVertices[vIndex * 2] = vt[vtIndex * 2];
                    textureVertices[vIndex * 2 + 1] = vt[vtIndex * 2 + 1];

                    indices.Add((uint)vIndex);
                }
                else
                {
                    bool found = false;
                    foreach(var aliasIndex in aliasIndices[vIndex])
                    {
                        if (textureVertices[aliasIndex * 2] == vt[vtIndex * 2] && textureVertices[aliasIndex * 2 + 1] == vt[vtIndex * 2 + 1])
                        {
                            found = true;
                            indices.Add((uint)aliasIndex);
                            break;
                        }
                    }
                    if (found) continue;

                    vertices.Add(v[vIndex * 3]);
                    vertices.Add(v[vIndex * 3 + 1]);
                    vertices.Add(v[vIndex * 3 + 2]);

                    aliasIndices[vIndex].Add(textureVertices.Count / 2);
                    indices.Add((uint)textureVertices.Count / 2);

                    textureVertices.Add(vt[vtIndex * 2]);
                    textureVertices.Add(vt[vtIndex * 2 + 1]);
                }
            }
            float[] textures = new float[textureVertices.Count];
            for (int i = 0; i < textureVertices.Count; i++)
                textures[i] = textureVertices[i].GetValueOrDefault();

            List<float> colors = new List<float>(v.Count / 3 * 4);
            for (int i = 0; i < vertices.Count / 3; i++)
            {
                colors.Add(color.X);
                colors.Add(color.Y);
                colors.Add(color.Z);
                colors.Add(color.W);
            }
            Texture texture = Texture.LoadFromFile(texPath);
            return new Model3D(vertices.ToArray(), indices.ToArray(), colors.ToArray(), textures.ToArray(), colorShader, textureShader, texture);
        }
    }

    
}