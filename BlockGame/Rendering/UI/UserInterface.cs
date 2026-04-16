using BlockGame.Rendering.Models;
using BlockGame.Rendering.Shaders;
using BlockGame.Rendering.Textures;
using BlockGame.World;
using OpenTK.Mathematics;


namespace BlockGame.Rendering.UI
{
    public class UserInterface
    {
        /// <summary>
        /// Add an image UI element, returns the mesh used to display the element.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="size"></param>
        /// <param name="imageTexture"></param>
        /// <param name="isBlockSelector"></param>
        /// <returns></returns>
        public static Mesh AddUIElement(Vector2 center, Vector2 size, ImageTexture imageTexture, int isBlockSelector = -1)
        {
            ShaderProgram shaderProgram = RenderCanvas.uiShader.Copy();
            Mesh mesh = new Mesh(shaderProgram);

            shaderProgram.Use();
            // shaderProgram.SetTextureId("u_Color", 0);
            mesh.texture = imageTexture;

            float[] uv = new float[] {
                0, 1,
                1, 1,
                0, 0,
                1, 0
            };

            if (isBlockSelector != -1)
            {
                Vector2[] uvV = Level.TextureMap.GetUV(isBlockSelector);

                // BL
                uv[0] = uvV[0].X; uv[1] = uvV[2].Y;

                // BR
                uv[2] = uvV[1].X; uv[3] = uvV[3].Y;

                uv[4] = uvV[3].X; uv[5] = uvV[1].Y;

                // TL
                uv[6] = uvV[2].X; uv[7] = uvV[0].Y;
            }

            mesh.Set(new float[]
            {
                center.X - size.X, center.Y - size.Y, 0,
                center.X + size.X, center.Y - size.Y, 0,
                center.X - size.X, center.Y + size.Y, 0,
                center.X + size.X, center.Y + size.Y, 0
            }, new int[] {
                0, 1, 2,
                3, 2, 1
            },
            uv);

            RenderCanvas.meshes.Add(mesh);
            return mesh;

        }
    }
}
