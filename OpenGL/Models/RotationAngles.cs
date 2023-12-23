using OpenTK.Mathematics;
namespace OpenGL.Models
{
    /// <summary>
    /// Angles (rad) of rotation around axes between current and initial position of object
    /// </summary>
    public struct RotationAngles
    {
        public float X { get; set; } // Around X-axis
        public float Y { get; set; } // Around Y-axis
        public float Z { get; set; } // Around Z-axis
        public float NormalizedX // [0 ; TwoPi]
        {
            get
            {
                float value = X % MathHelper.TwoPi;
                if (value < 0)
                    value = MathHelper.TwoPi + value;
                return value;
            }
        }
        public float NormalizedY // [0 ; TwoPi]
        {
            get
            {
                float value = Y % MathHelper.TwoPi;
                if (value < 0)
                    value = MathHelper.TwoPi + value;
                return value;
            }
        }
        public float NormalizedZ // [0 ; TwoPi]
        {
            get
            {
                float value = Z % MathHelper.TwoPi;
                if (value < 0)
                    value = MathHelper.TwoPi + value;
                return value;
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
            const float epsilon = 1e-4f;
            return (MathF.Abs(op1.X - op2.X) < epsilon) && (MathF.Abs(op1.Y - op2.Y) < epsilon) && (MathF.Abs(op1.Z - op2.Z) < epsilon);
        }
        public static bool operator !=(RotationAngles op1, RotationAngles op2)
        {
            return !(op1 == op2);
        }
    }
}
