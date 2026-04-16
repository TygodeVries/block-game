using BlockGame.Input;
using BlockGame.Rendering;
using BlockGame.Rendering.World;
using OpenTK.Mathematics;

namespace BlockGame.Player
{
    internal class SpectatorMovement : Movement
    {
        private Vector3 moveDelta = Vector3.Zero;
        private float flightSpeed = 4.0f;

        public override void Update(double delta)
        {
            RenderCanvas.meshes.Remove(RenderCanvas.loadingMesh);
            RenderCanvas.SetDefaultFog();
            Vector3 goalDelta = Vector3.Zero;
            flightSpeed += Mouse.current.scroll.Y * 1f;
            if (flightSpeed < 0)
                flightSpeed = 0;

            if (Keyboard.current.IsPressed(Key.A))
                goalDelta.X = -1;

            if (Keyboard.current.IsPressed(Key.D))
                goalDelta.X = 1;

            if (Keyboard.current.IsPressed(Key.Q))
                goalDelta.Y = -1;

            if (Keyboard.current.IsPressed(Key.E))
                goalDelta.Y = 1;

            if (Keyboard.current.IsPressed(Key.W))
                goalDelta.Z = -1;

            if (Keyboard.current.IsPressed(Key.S))
                goalDelta.Z = 1;

            moveDelta = Vector3.Lerp(moveDelta, goalDelta, RenderCanvas.deltaTime * 6f);

            Camera.position += Camera.forwards * -moveDelta.Z * RenderCanvas.deltaTime * flightSpeed;
            Camera.position += Camera.GetRight() * moveDelta.X * RenderCanvas.deltaTime * flightSpeed;
            Camera.position += Camera.GetUp() * moveDelta.Y * RenderCanvas.deltaTime * flightSpeed;

            angle += Mouse.current.mouseDelta.X * (float)delta * 0.01f;
            yaw -= Mouse.current.mouseDelta.Y * (float)delta * 0.01f;

            yaw = Math.Clamp(yaw, -0.49f, 0.49f);

            float cosPitch = MathF.Cos(yaw * MathF.PI);

            float x = MathF.Cos(angle * MathF.PI) * cosPitch;
            float y = MathF.Sin(yaw * MathF.PI);
            float z = MathF.Sin(angle * MathF.PI) * cosPitch;

            Camera.forwards = new Vector3(x, y, z).Normalized();
        }
        private float yaw = 0;
        private float angle = 0;

    }
}
