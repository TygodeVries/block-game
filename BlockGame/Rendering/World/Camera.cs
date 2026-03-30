using OpenTK.Mathematics;

namespace BlockGame.Rendering.World
{
    public class Camera
    {
        /// <summary>
        /// The position of the camera
        /// </summary>
        public static Vector3 position = Vector3.Zero;

        /// <summary>
        /// The direction of the camera
        /// </summary>
        public static Vector3 forwards = new Vector3(0, 0, 1);
        public static float aspectRatio;

        public static Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(
                OpenTK.Mathematics.MathHelper.DegreesToRadians(60),
                aspectRatio,
                0.1f, 4000.0f
            );
        }

        public static Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(position, position + forwards, Vector3.UnitY);
        }

        public static Vector3 GetRight()
        {

            Vector3 up = Vector3.UnitY;
            Vector3 right = Vector3.Cross(forwards, up);
            return right.Normalized();
        }

        public static Vector3 GetUp()
        {
            Vector3 forward = forwards;
            Vector3 right = GetRight();

            Vector3 up = Vector3.Cross(right, forward);
            return up.Normalized();
        }
    }
}
