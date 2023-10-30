using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL
{
    /// <summary>
    /// A camera that is located on the surface of a hemisphere and looks at a specified target point
    /// </summary>
    public class Camera
    {
        //----------------------------------------------------------------
        // To do:
        // Add the abiliti to change target point (move the camera sphere)
        //----------------------------------------------------------------


        private const float DEPTHNEAR = 0.01f; // Distance to the near plane of the camera 
        private const float FOV = MathHelper.PiOver2; // Field of view
        private const float MINSENSITIVITY = 0.001f; // Minimum mouse sensitivity


        private float _sensitivity; // Mouse sensitivity
        private float _renderDistance; // Distance to the far plane of the camera
        private float _minRadius; // Minimum radius of a sphere
        private float _maxRadius; // Maximum radius of a sphere
        private float _r; // Current radius of a shere
        private float _theta; // Polar angle (radians)
        private float _phi; // Azimuthal angle (radians)


        public float Sensitivity
        {
            get { return _sensitivity; }
            set
            {
                _sensitivity = Math.Clamp(value, MINSENSITIVITY, R);
            }
        }
        public float RenderDistance
        {
            get { return _renderDistance; }
            set
            {
                _renderDistance = Math.Max(value, DEPTHNEAR);
            }
        }
        public float MinRadius
        {
            get { return _minRadius; }
            set
            {
                _minRadius = MathHelper.Clamp(value, DEPTHNEAR, MaxRadius);
            }
        }
        public float MaxRadius
        {
            get { return _maxRadius; }
            set
            {
                _maxRadius = Math.Max(value, MinRadius);
            }
        }
        public float R
        {
            get { return _r; }
            set
            {
                _r = MathHelper.Clamp(value, MinRadius, MaxRadius);
                UpdateVectors();
            }
        }
        public float Theta
        {
            get
            { return _theta; }
            set
            {
                _theta = MathHelper.Clamp(value, 0, MathHelper.PiOver2);
                UpdateVectors();
            }
        }
        public float Phi
        {
            get { return _phi; }
            set
            {
                value %= MathHelper.TwoPi;
                if (value < 0)
                    value = MathHelper.TwoPi + value;
                _phi = value;
                UpdateVectors();
            }
        }
        public Vector3 Position { get; private set; } // Current position of the camera in Cartesian coordinates
        public Vector3 Direction { get; private set; } // Vector of direction of the camera
        public Vector3 Target { get; private set; } // Target point 
        public Vector3 Up { get; private set; } // Up-vector
        public Vector3 Right { get; private set; } // Right-vector
        public float AspectRatio { private get; set; }



        public Camera(Vector3 position, Vector3 target, float aspectRatio, float renderDistance, float sensitivity)
        {
            // Abs(y) because camera lies on a semisphere, Y >= 0 and 0 <= Theta <= 90 degree
            Position = new Vector3(position.X, MathF.Abs(position.Y), position.Z); 
            AspectRatio = aspectRatio;
            RenderDistance = renderDistance;
            Target = target;

            R = MathF.Sqrt(position.X * position.X + position.Y * position.Y + position.Z * position.Z);

            Sensitivity = sensitivity;

            // Thus it would take 10 steps to reach max/min radius
            MaxRadius = R + 10 * Sensitivity; 
            MinRadius = R - 10 * Sensitivity;

            Theta = MathF.Acos(position.Z / R);
            Phi = MathF.Atan2(position.Z, position.X);

            Direction = Vector3.Normalize(Position - Target);
            Right = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, Direction));
            Up = Vector3.Normalize(Vector3.Cross(Direction, Right));
        }

        

        // Get the view matrix
        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Target, Up);
        }
        // Get the projection matrix
        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(FOV, AspectRatio, 0.01f, RenderDistance);
        }

        // This function is going to update the direction vertices
        private void UpdateVectors()
        {
            float X = R * MathF.Sin(Theta) * MathF.Cos(Phi) + Target.X;
            float Y = R * MathF.Cos(Theta) + Target.Y;
            float Z = R * MathF.Sin(Theta) * MathF.Sin(Phi) + Target.Z;

            Position = new Vector3(X, Y, Z);
            Direction = Vector3.Normalize(Position - Target);
            Right = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, Direction));
            Up = Vector3.Normalize(Vector3.Cross(Direction, Right));
        }
    }
}
