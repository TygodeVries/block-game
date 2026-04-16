using BlockGame.Input;
using BlockGame.Rendering;
using BlockGame.Rendering.World;
using BlockGame.World;
using OpenTK.Mathematics;

namespace BlockGame.Player
{

    internal class NormalMovement : Movement
    {
        private Vector3 walkVelocity = Vector3.Zero;
        private float walkSpeed = 4.0f;
        private float yVelocity = 0f;

        private float angle = 0f;
        private float yaw = 0f;

        private const float playerWidth = 0.6f;
        private float playerHeight = 1.8f;
        private double bobbing = 0;

        private float Lerp(float a, float b, float t)
        {
            return a + ((b - a) * t);
        }


        private void UpdateFog()
        {
            Vector3i position = (Vector3i)Camera.position.Round();

            // Check if we are in a water block
            if (Level.GetVoxelAt(position.X, position.Y, position.Z) == 0x02)
            {
                RenderCanvas.SetFog(new Vector3(0.1f, 0.25f, 0.5f), 0, 20, 10);
            }
            else
            {
                RenderCanvas.SetDefaultFog();
            }
        }

        private Vector3 GetMovementInput()
        {
            Vector3 movementInput = Vector3.Zero;

            if (Keyboard.current.IsPressed(Key.A)) movementInput.X = -1;
            if (Keyboard.current.IsPressed(Key.D)) movementInput.X = 1;
            if (Keyboard.current.IsPressed(Key.W)) movementInput.Z = -1;
            if (Keyboard.current.IsPressed(Key.S)) movementInput.Z = 1;

            return movementInput;
        }

        public override void Update(double time)
        {

            Vector3 feetPosition = Camera.position - new Vector3(0, playerHeight, 0);
            if (!Level.IsVoxelLoadedAt((int)feetPosition.X, (int)feetPosition.Y, (int)feetPosition.Z))
                return;
            else
            {
                RenderCanvas.meshes.Remove(RenderCanvas.loadingMesh);
            }

            UpdateFog();

            // Smooth movement
            walkVelocity = Vector3.Lerp(walkVelocity, GetMovementInput(), RenderCanvas.deltaTime * 6f);

            // Gravity
            yVelocity -= (float)time * 20f;

            // Check if we are on the ground (exluding water, as we want to fall into water)
            bool isGrounded = Collides(feetPosition + new Vector3(0, -0.05f, 0), false);
            if (isGrounded && yVelocity < 0)
                yVelocity = 0;

            // Check if we are on the ground (including water, we want to be able to jump.)
            isGrounded = Collides(feetPosition + new Vector3(0, -0.05f, 0), true);

            // Jumping
            if (Keyboard.current.IsPressed(Key.Space) && isGrounded)
            {
                yVelocity = 8f;
                isGrounded = false;
            }


            Vector3 motion = Vector3.Zero;

            Vector3 forward = Camera.forwards;
            Vector3 right = Camera.GetRight();

            right.Y = 0;
            forward.Y = 0;

            right.Normalize();
            forward.Normalize();

            motion += forward * -walkVelocity.Z;
            motion += right * walkVelocity.X;
            motion = motion * RenderCanvas.deltaTime * walkSpeed;

            motion.Y = yVelocity * RenderCanvas.deltaTime;


            // Y
            Vector3 newPos = feetPosition;
            newPos.Y += motion.Y;

            if (!Collides(newPos, false))
            {
                feetPosition.Y = newPos.Y;
            }
            else
            {
                yVelocity = 0;
            }

            // X
            newPos = feetPosition;
            newPos.X += motion.X;

            if (!Collides(newPos, false))
            {
                feetPosition.X = newPos.X;
            }

            // Z
            newPos = feetPosition;
            newPos.Z += motion.Z;

            if (!Collides(newPos, false))
            {
                feetPosition.Z = newPos.Z;
            }

            // View bobbing (making it look like we are setting steps
            if (GetMovementInput().Length > 0.1)
            {
                bobbing += time * 15;
            }


            playerHeight = Lerp(1.85f, 1.9f, (float)Math.Cos(bobbing));

            // Set final new position
            Camera.position = feetPosition + new Vector3(0, playerHeight, 0);



            // Mouse Rotation
            angle += Mouse.current.mouseDelta.X * 0.0005f;
            yaw -= Mouse.current.mouseDelta.Y * 0.0005f;

            yaw = Math.Clamp(yaw, -0.49f, 0.49f);

            float cosPitch = MathF.Cos(yaw * MathF.PI);

            float x = MathF.Cos(angle * MathF.PI) * cosPitch;
            float y = MathF.Sin(yaw * MathF.PI);
            float z = MathF.Sin(angle * MathF.PI) * cosPitch;

            Camera.forwards = new Vector3(x, y, z).Normalized();
        }

        private bool IsSolid(int x, int y, int z, bool isGround)
        {
            byte? block = Level.GetVoxelAt(x, y, z);
            if (block == null)
                return true;

            return block != 0 && (block != 2 || isGround) && block != 5 && block != 6 && block != 7 && block != 8;
        }

        private bool Collides(Vector3 p, bool isGround)
        {
            int minX = (int)MathF.Floor(p.X - (playerWidth / 2));
            int maxX = (int)MathF.Floor(p.X + (playerWidth / 2));

            int minY = (int)MathF.Floor(p.Y);
            int maxY = (int)MathF.Floor(p.Y + playerHeight);

            int minZ = (int)MathF.Floor(p.Z - (playerWidth / 2));
            int maxZ = (int)MathF.Floor(p.Z + (playerWidth / 2));

            for (int x = minX; x <= maxX; x++)
                for (int y = minY; y <= maxY; y++)
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        if (IsSolid(x, y, z, isGround))
                            return true;
                    }

            return false;
        }
    }

}