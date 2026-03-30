using BlockGame.Input;
using BlockGame.Rendering;
using BlockGame.Rendering.Models;
using BlockGame.Rendering.Textures;
using BlockGame.Rendering.UI;
using BlockGame.Rendering.World;
using OpenTK.Mathematics;

namespace BlockGame.World
{
    public class Building
    {
        private static Mesh? selector;
        public void UpdateSelector()
        {
            if (selector != null)
                RenderCanvas.meshes.Remove(selector);

            selector = UserInterface.AddUIElement(new Vector2(0f, -0.8f), new Vector2(0.05f, 0.1f), ImageTexture.LoadFromPng("textures/block1.png"), block);
        }


        public Building()
        {
            RenderCanvas.Update += Update;
        }

        private byte[] inventory = { 1, 3, 4, 5, 6, 7, 8, 9 };
        private float selectedIndex = 0;


        public byte block = 0x05;
        public float reach = 10;
        public void Update(double time)
        {
            float scroll = Mouse.current.scroll.Y;

            if (scroll != 0)
            {
                if (scroll > 0) selectedIndex -= RenderCanvas.deltaTime * 10;
                else if (scroll < 0) selectedIndex += RenderCanvas.deltaTime * 10;

                if (selectedIndex < 0) selectedIndex = inventory.Length - 1;
                if (selectedIndex >= inventory.Length) selectedIndex = 0;

                block = inventory[(int)selectedIndex];
                UpdateSelector();
            }


            // Break a block
            if (Mouse.current.LeftPressedThisFrame())
            {
                RaycastHit? hit = Level.Raycast(Camera.position, Camera.forwards, reach);

                if (hit != null)
                {
                    Level.SetVoxelAt(hit.WorldBlockPos.X, hit.WorldBlockPos.Y, hit.WorldBlockPos.Z, 0);
                }
            }

            if (Mouse.current.RightPressedThisFrame())
            {
                RaycastHit? hit = Level.Raycast(Camera.position, Camera.forwards, reach);

                if (hit != null)
                {
                    Vector3i worldPos = hit.WorldBlockPos + hit.Normal;
                    Level.SetVoxelAt(worldPos.X, worldPos.Y, worldPos.Z, block);
                }
            }
        }
    }
}
