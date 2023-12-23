namespace Models
{
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
}
