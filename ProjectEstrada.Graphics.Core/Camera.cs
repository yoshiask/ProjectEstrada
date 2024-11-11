using System;
using System.Numerics;

namespace ProjectEstrada.Graphics.Core
{
    public class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Front { get; set; }

        public Vector3 Up { get; private set; }
        public float AspectRatio { get; set; }

        public float Yaw { get; set; } = -90f;
        public float Pitch { get; set; }

        private float Zoom = 45f;

        public Camera(Vector3 position, Vector3 front, Vector3 up, float aspectRatio)
        {
            Position = position;
            AspectRatio = aspectRatio;
            Front = front;
            Up = up;
        }

        public void ModifyZoom(float zoomAmount)
        {
            // We don't want to be able to zoom in too close or too far away so clamp to these values
            Zoom = MathEx.Clamp(Zoom - zoomAmount, 1.0f, 45f);
        }

        public void ModifyDirection(float xOffset, float yOffset)
        {
            Yaw += xOffset;
            Pitch -= yOffset;

            // We don't want to be able to look behind us by going over our head or under our feet so make sure it stays within these bounds
            Pitch = MathEx.Clamp(Pitch, -89f, 89f);

            var cameraDirection = Vector3.Zero;
            cameraDirection.X = (float)(Math.Cos(MathEx.DegreesToRadians(Yaw)) * Math.Cos(MathEx.DegreesToRadians(Pitch)));
            cameraDirection.Y = (float)Math.Sin(MathEx.DegreesToRadians(Pitch));
            cameraDirection.Z = (float)(Math.Sin(MathEx.DegreesToRadians(Yaw)) * Math.Cos(MathEx.DegreesToRadians(Pitch)));

            Front = Vector3.Normalize(cameraDirection);
        }

        public Matrix4x4 GetViewMatrix()
        {
            return Matrix4x4.CreateLookAt(Position, Position + Front, Up);
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(MathEx.DegreesToRadians(Zoom), AspectRatio, 0.1f, 100.0f);
        }
    }
}