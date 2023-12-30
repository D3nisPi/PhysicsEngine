using OpenGL.Shaders;
using OpenGL.Textures;
using OpenTK.Mathematics;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace OpenGL.Models
{
    public enum DrawMode
    {
        Color,
        Texture,
    }
    // Apply only one color to all vertices -> change shaders, remove vboColors, store Vec4 Color
    public partial class Model
    {
        public static readonly Vector4 DEFAULTCOLOR;

        private List<ModelUnit> _units;

        private bool _drawModeChangeable;
        private DrawMode _drawMode;
        
        private Shader _colorShader;
        private Shader? _textureShader;

        private Vector3 _position;
        private Size _size;
        private RotationAngles _rotation;

        public DrawMode DrawMode
        {
            get => _drawMode;
            set
            {
                if (!_drawModeChangeable)
                    throw new NullReferenceException("Required objects are not attached to this model");
                _drawMode = value;
            }
        }
        
        public Vector3 Position { get => _position; }
        public Size Size { get => _size; }
        public RotationAngles Rotation { get => _rotation; }


        public Vector4 Color;
        public Vector3 MovementPerSecond;
        public Size ScalingPerSecond;
        public RotationAngles RotationPerSecond;

        static Model()
        {
            DEFAULTCOLOR = new Vector4(0, 0, 0, 1);
        }
        public Model(IEnumerable<ModelUnit> units, Shader colorShader, Shader? textureShader) 
            : this(units, colorShader, textureShader, DEFAULTCOLOR) { }
        public Model(IEnumerable<ModelUnit> units, Shader colorShader, Shader? textureShader, Vector4 color)
        {
            _units = new List<ModelUnit>(units);
            foreach(var unit in _units)
                unit.SetModel(this);

            _colorShader = colorShader;
            _textureShader = textureShader;

            _drawModeChangeable = textureShader != null ? true : false;
            _drawMode = textureShader != null ? DrawMode.Texture : DrawMode.Color;

            _position = Vector3.Zero;
            _size = Size.One;
            _rotation = RotationAngles.Zero;

            MovementPerSecond = Vector3.Zero;
            ScalingPerSecond = Size.One;
            RotationPerSecond = RotationAngles.Zero;

            Color = color;
        }
        
        public void Draw()
        {
            switch (DrawMode)
            {
                case DrawMode.Color:
                    _colorShader.ActivateProgram();
                    _colorShader.SetVec4Uniform("aColor", ref Color);
                    break;
                case DrawMode.Texture:
                    _textureShader!.ActivateProgram();
                    break;
            }
            foreach (var unit in _units)
            {
                unit.Draw();
            }
        }
        // To do: create a separate thread for updating
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
            foreach (var unit in _units)
            {
                unit.Scale(scaleX, scaleY, scaleZ);
            }

            _size.X *= scaleX;
            _size.Y *= scaleY;
            _size.Z *= scaleZ;
        }
        public void Move(float shiftX, float shiftY, float shiftZ)
        {
            foreach (var unit in _units)
            {
                unit.Move(shiftX, shiftY, shiftZ);
            }

            _position.X += shiftX;
            _position.Y += shiftY;
            _position.Z += shiftZ;
        }
        public void RotateX(float angleRad)
        {
            foreach (var unit in _units)
            {
                unit.RotateX(angleRad);
            }

            _rotation.X += angleRad;
            _rotation.X = _rotation.NormalizedX;
        }
        public void RotateY(float angleRad)
        {
            foreach (var unit in _units)
            {
                unit.RotateY(angleRad);
            }

            _rotation.Y += angleRad;
            _rotation.Y = _rotation.NormalizedY;
        }
        public void RotateZ(float angleRad)
        {
            foreach (var unit in _units)
            {
                unit.RotateZ(angleRad);
            }

            _rotation.Z += angleRad;
            _rotation.Z = _rotation.NormalizedZ;
        }
        public void Rotate(float angleRadX, float angleRadY, float angleRadZ)
        {
            RotateX(angleRadX);
            RotateY(angleRadY);
            RotateZ(angleRadZ);
        }

        private struct ObjData
        {
            public string Usemtl;
            public float[] Vertices;
            public float[] Textures;
            public float[] Normals;
            public string[] Faces;
            public int VertexIndexShift;
            public int NormalIndexShift;
            public int TextureIndexShift;

            public ObjData(string usemtl, IEnumerable<float> vertices, IEnumerable<float> normals, IEnumerable<float> textures, IEnumerable<string> faces,
                int vertexIndexShift, int normalIndexShift, int textureIndexShift)
            {
                Usemtl = usemtl;
                Vertices = vertices.ToArray();
                Textures = textures.ToArray();
                Normals = normals.ToArray();
                Faces = faces.ToArray();
                VertexIndexShift = vertexIndexShift;
                NormalIndexShift = normalIndexShift;
                TextureIndexShift = textureIndexShift;             
            }
        }
        private struct MtlData
        {
            public string Name;
            public string TextureFile;

            public MtlData(string name, string textureFile)
            {
                Name = name;
                TextureFile = textureFile;
            }
        }

        private static (ObjData[], string) ReadObj(string path)
        {
            List<ObjData> data = new List<ObjData>();

            List<float> v = new List<float>();
            List<float> vt = new List<float>();
            List<float> vn = new List<float>();
            List<string> f = new List<string>();
            string usemtl = "";
            string mtllib = "";

            bool blockEnd = false;

            int vShift = 0;
            int vnShift = 0;
            int vtShift = 0;

            Regex regex = new Regex(@"\s+");

            using (var reader = new StreamReader(path))
            {
                string? line = reader.ReadLine();
                while (line != null)
                {
                    line = line.Trim();
                    line = regex.Replace(line, " ");
                    if (line.StartsWith("mtllib"))
                    {
                        string[] tokens = line.Split(' ');
                        
                        mtllib = @$"{Path.GetDirectoryName(path)}\{tokens[1]}";
                    }
                    if (line.StartsWith("v "))
                    {
                        if (blockEnd)
                        {
                            blockEnd = false;
                            ObjData obj = new ObjData(usemtl, v, vn, vt, f, vShift, vnShift, vtShift);
                            data.Add(obj);
                            usemtl = "";
                            vShift += v.Count / 3;
                            vnShift += vn.Count / 3;
                            vtShift += vt.Count / 2;
                            v.Clear(); vn.Clear(); vt.Clear(); f.Clear();
                        }

                        line = line.Replace('.', ',');
                        string[] tokens = line.Split(' ');

                        v.Add(float.Parse(tokens[1]));
                        v.Add(float.Parse(tokens[2]));
                        v.Add(float.Parse(tokens[3]));
                    }
                    else if (line.StartsWith("vn "))
                    {
                        line = line.Replace('.', ',');
                        string[] tokens = line.Split(' ');

                        vn.Add(float.Parse(tokens[1]));
                        vn.Add(float.Parse(tokens[2]));
                        vn.Add(float.Parse(tokens[3]));
                    }
                    else if (line.StartsWith("vt "))
                    {
                        line = line.Replace('.', ',');
                        string[] tokens = line.Split(' ');

                        vt.Add(float.Parse(tokens[1]));
                        vt.Add(float.Parse(tokens[2]));
                    }
                    else if (line.StartsWith("usemtl"))
                    {
                        string[] tokens = line.Split(' ');
                        usemtl = tokens[1];
                    }
                    else if (line.StartsWith("f "))
                    {
                        if (!blockEnd) blockEnd = true;

                        line = line.Replace('.', ',');
                        string[] tokens = line.Split(' ');

                        f.Add(tokens[1]);
                        f.Add(tokens[2]);
                        f.Add(tokens[3]);
                    }
                    line = reader.ReadLine();
                }
            }
            if (blockEnd)
            {
                ObjData obj = new ObjData(usemtl, v, vn, vt, f, vShift, vnShift, vtShift);
                data.Add(obj);
            }

            return (data.ToArray(), mtllib);
        }
        private static Dictionary<string, MtlData> ReadMtl(string path)
        {
            Dictionary<string, MtlData> mtlData = new Dictionary<string, MtlData>();
            string newmtl = "";
            string texturePath = "";
            bool blockEnd = false;

            using (var reader = new StreamReader(path))
            {
                string? line = reader.ReadLine();
                while (line != null)
                {
                    line = line.Trim().Trim('\t');
                    if (line.StartsWith("newmtl"))
                    {
                        if (blockEnd)
                        {
                            blockEnd = false;
                            mtlData[newmtl] = new MtlData(newmtl, texturePath);
                        }

                        string[] tokens = line.Split(' ');
                        newmtl = tokens[1];
                    }
                    else if (line.StartsWith("map_Kd"))
                    {
                        if (!blockEnd) blockEnd = true;

                        string[] tokens = line.Split(' ');
                        texturePath = @$"{Path.GetDirectoryName(path)}\{tokens[1]}"; ;
                    }
                    line = reader.ReadLine();
                }
            }
            if (blockEnd)
            {
                mtlData[newmtl] = new MtlData(newmtl, texturePath);
            }
            return mtlData;
        }

        private static ModelUnit CreateModelUnit(ObjData objData)
        {
            List<uint> indices = new List<uint>();
            foreach(var face in objData.Faces)
            {
                string[] parts = face.Split('/');
                indices.Add(uint.Parse(parts[0]) - 1 - (uint)objData.VertexIndexShift);
            }
            return new ModelUnit(objData.Vertices, indices.ToArray());
        }
        private static ModelUnit CreateModelUnit(ObjData objData, MtlData mtlData)
        {
            List<float> vertices = new List<float>(objData.Vertices);
            List<float?> textureVertices = new List<float?>(new float?[vertices.Count / 3 * 2]);
            List<uint> indices = new List<uint>();

            Dictionary<int, List<int>> aliasIndices = new Dictionary<int, List<int>>();
            for (int i = 0; i < vertices.Count / 3; i++)
                aliasIndices[i] = new List<int>() { i };

            foreach (var vertexInfo in objData.Faces)
            {
                string[] info = vertexInfo.Split("/");
                int vIndex = int.Parse(info[0]) - 1 - objData.VertexIndexShift;
                int vtIndex = int.Parse(info[1]) - 1 - objData.TextureIndexShift;
                int vnIndex = int.Parse(info[2]) - 1 - objData.NormalIndexShift;

                if (textureVertices[vIndex * 2] == null && textureVertices[vIndex * 2 + 1] == null)
                {
                    textureVertices[vIndex * 2] = objData.Textures[vtIndex * 2];
                    textureVertices[vIndex * 2 + 1] = objData.Textures[vtIndex * 2 + 1];

                    indices.Add((uint)vIndex);
                }
                else
                {
                    bool found = false;
                    foreach (var aliasIndex in aliasIndices[vIndex])
                    {
                        if (textureVertices[aliasIndex * 2] == objData.Textures[vtIndex * 2] 
                            && textureVertices[aliasIndex * 2 + 1] == objData.Textures[vtIndex * 2 + 1])
                        {
                            found = true;
                            indices.Add((uint)aliasIndex);
                            break;
                        }
                    }
                    if (found) continue;

                    vertices.Add(objData.Vertices[vIndex * 3]);
                    vertices.Add(objData.Vertices[vIndex * 3 + 1]);
                    vertices.Add(objData.Vertices[vIndex * 3 + 2]);

                    aliasIndices[vIndex].Add(textureVertices.Count / 2);
                    indices.Add((uint)textureVertices.Count / 2);

                    textureVertices.Add(objData.Textures[vtIndex * 2]);
                    textureVertices.Add(objData.Textures[vtIndex * 2 + 1]);
                }
            }
            float[] textures = new float[textureVertices.Count];
            for (int i = 0; i < textureVertices.Count; i++)
                textures[i] = textureVertices[i].GetValueOrDefault();


            Texture texture = Texture.LoadFromFile(mtlData.TextureFile);

            return new ModelUnit(vertices.ToArray(), indices.ToArray(), textures.ToArray(), texture);
        }

        public static Model ParseOBJ(string path, Shader colorShader)
        {
            return ParseOBJ(path, colorShader, DEFAULTCOLOR);
        }
        public static Model ParseOBJ(string path, Shader colorShader, Vector4 color)
        {
            (ObjData[] objData, string mtlPath) = ReadObj(path);

            List<ModelUnit> units = new List<ModelUnit>();
            foreach (ObjData obj in objData)
            {
                ModelUnit unit = CreateModelUnit(obj);
                units.Add(unit);
            }
            return new Model(units, colorShader, null, color);
        }
        public static Model ParseOBJ(string path, Shader colorShader, Shader textureShader)
        {
            return ParseOBJ(path, colorShader, textureShader, DEFAULTCOLOR);
        }
        public static Model ParseOBJ(string path, Shader colorShader, Shader textureShader, Vector4 color)
        {
            // To do:
            //      1) Parse normals info
            //      2) Parse .mtl file info


            (ObjData[] objData, string mtlPath) = ReadObj(path);

            Dictionary<string, MtlData> mtlData = ReadMtl(mtlPath);

            List<ModelUnit> units = new List<ModelUnit>();
            foreach(ObjData obj in objData)
            {
                MtlData mtl = mtlData[obj.Usemtl];
                ModelUnit unit = CreateModelUnit(obj, mtl);
                units.Add(unit);
            }
            return new Model(units, colorShader, textureShader, color);
        }
    }
}
