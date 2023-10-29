using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
namespace Models
{
    
    public struct RotationAngles // angles (rad) of rotation around axes between current and initial position of object 
    {
        private float _rotationX; // around X-axis
        private float _rotationY; // around Y-axis
        private float _rotationZ; // around Z-axis
        public float X 
        {
            get { return _rotationX; }
            set
            {
                value %= MathHelper.TwoPi;
                if (value < 0)
                    value = MathHelper.TwoPi + value;
                _rotationX = value;
            }
        }
        public float Y 
        {
            get { return _rotationY; }
            set
            {
                value %= MathHelper.TwoPi;
                if (value < 0)
                    value = MathHelper.TwoPi + value;
                _rotationY = value;
            }
        }
        public float Z 
        {
            get { return _rotationZ; }
            set
            {
                value %= MathHelper.TwoPi;
                if (value < 0)
                    value = MathHelper.TwoPi + value;
                _rotationZ = value;
            }
        }
        public static RotationAngles Zero;
        static RotationAngles()
        {
            Zero = new RotationAngles(0, 0, 0);
        }
        public RotationAngles()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }
        public RotationAngles(float angleX, float angleY, float angleZ)
        {
            X = angleX;
            Y = angleY;
            Z = angleZ;
        }
        public static bool operator ==(RotationAngles op1, RotationAngles op2)
        {
            return op1.X == op2.X && op1.Y == op2.Y && op1.Z == op2.Z;
        }
        public static bool operator !=(RotationAngles op1, RotationAngles op2)
        {
            return !(op1 == op2);
        }
    }
    public struct Size
    {
        public float X;
        public float Y;
        public float Z;
        public static Size One;
        static Size()
        {
            One = new Size(1, 1, 1);
        }
        public Size()
        {
            X = 1;
            Y = 1;
            Z = 1;
        }
        public Size(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static bool operator ==(Size op1, Size op2)
        {
            return op1.X == op2.X && op1.Y == op2.Y && op1.Z == op2.Z;
        }
        public static bool operator !=(Size op1, Size op2)
        {
            return !(op1 == op2);
        }
    }

    public class Model3D
    {
        private float[] _vertices;
        private uint[] _indices;

        //private Texture _texture;
        //private Material _material // for light fx

        private int _vao;
        private int _vbo;
        private int _ebo;

        public Vector3 Position { get; private set; }
        public Size Size { get; private set; }
        public RotationAngles Rotation { get; private set; }


        public Vector3 MovementPerSecond;
        public Size ScalingPerSecond;
        public RotationAngles RotationPerSecond;



        public Model3D(float[] vertices, float[] colors, uint[] indices)
        {
            Position = new Vector3();
            Size = new Size();
            Rotation = new RotationAngles();

            MovementPerSecond = Vector3.Zero;
            ScalingPerSecond = Size.One;
            RotationPerSecond = RotationAngles.Zero;

            _vertices = (float[])vertices.Clone();
            _indices = (uint[])indices.Clone();

            // Bind VBO
            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            // Bind VAO
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, true, 7 * sizeof(float), 0);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, true, 7 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            // Bind EBO
            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            //Unbind all
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

        }

        private void SetDataVBO()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
        public void Draw()
        {
            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);

            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
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
            for (int i = 0; i < _vertices.Length; i = i + 7)
            {
                _vertices[i] *=  (_vertices[i] - Position.X) * scaleX + Position.X;
                _vertices[i + 1] *= (_vertices[i] - Position.Y) * scaleY + Position.Y;
                _vertices[i + 2] *= (_vertices[i] - Position.Z) * scaleZ + Position.Z;
            }

            SetDataVBO();

            Size = new Size(Size.X * scaleX, Size.Y * scaleY, Size.Z * scaleZ);
        }

        public void Move(float shiftX, float shiftY, float shiftZ)
        {
            for (int i = 0; i < _vertices.Length; i = i + 7)
            {
                _vertices[i] += shiftX;
                _vertices[i + 1] += shiftY;
                _vertices[i + 2] += shiftZ;
            }

            SetDataVBO();

            Position = new Vector3(Position.X + shiftX, Position.Y + shiftY, Position.Z + shiftZ);
        }

        public void RotateX(float angleRad)
        {
            for (int i = 0; i < _vertices.Length; i = i + 7)
            {
                float x = _vertices[i] - Position.X;
                float y = _vertices[i + 1] - Position.Y;
                float z = _vertices[i + 2] - Position.Z;

                _vertices[i] = x + Position.X; 
                _vertices[i + 1] = MathF.Cos(angleRad) * y - MathF.Sin(angleRad) * z + Position.Y;
                _vertices[i + 2] = MathF.Sin(angleRad) * y + MathF.Cos(angleRad) * z + Position.Z;
            }

            SetDataVBO();

            Rotation = new RotationAngles(Rotation.X + angleRad, Rotation.Y, Rotation.Z);
        }
        public void RotateY(float angleRad)
        {
            for (int i = 0; i < _vertices.Length; i = i + 7)
            {
                float x = _vertices[i] - Position.X;
                float y = _vertices[i + 1] - Position.Y;
                float z = _vertices[i + 2] - Position.Z;

                _vertices[i] = MathF.Cos(angleRad) * x + MathF.Sin(angleRad) * z + Position.X;
                _vertices[i + 1] = y + Position.Y;
                _vertices[i + 2] = -MathF.Sin(angleRad) * x + MathF.Cos(angleRad) * z + Position.Z;
            }

            SetDataVBO();

            Rotation = new RotationAngles(Rotation.X, Rotation.Y + angleRad, Rotation.Z);
        }
        public void RotateZ(float angleRad)
        {
            for (int i = 0; i < _vertices.Length; i = i + 7)
            {
                float x = _vertices[i] - Position.X;
                float y = _vertices[i + 1] - Position.Y;
                float z = _vertices[i + 2] - Position.Z;

                _vertices[i] = MathF.Cos(angleRad) * x - MathF.Sin(angleRad) * y + Position.X;
                _vertices[i + 1] = MathF.Sin(angleRad) * x + MathF.Cos(angleRad) * y + Position.Y;
                _vertices[i + 2] = z + Position.Z;
            }

            SetDataVBO();

            Rotation = new RotationAngles(Rotation.X, Rotation.Y, Rotation.Z + angleRad);
        }
        public void Rotate(float angleRadX, float angleRadY, float angleRadZ)
        {
            RotateX(angleRadX);
            RotateY(angleRadY);
            RotateZ(angleRadZ);
        }

    }

    
}