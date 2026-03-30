using OpenTK.Mathematics;

namespace BlockGame.Rendering.Textures
{

    /// <summary>
    /// Mapping a map to UV coordinates
    /// </summary>
    public class TextureMap
    {
        private int width = 0;
        private int height = 0;

        private int blockWidth = 16;
        private int blockHeight = 16;

        private int row;
        private int col;

        public TextureMap(ImageTexture texture)
        {
            width = texture.width;
            height = texture.height;

            row = width / blockWidth;
            col = height / blockHeight;
        }

        public TextureMap(int width, int height)
        {
            this.width = width;
            this.height = height;

            row = width / blockWidth;
            col = height / blockHeight;
        }

        public Vector2[] GetUV(int textureId)
        {
            int texX = textureId % row;
            int texY = textureId / row;

            float uvXmin = texX / (float)row;
            float uvYmin = texY / (float)col;

            float uvXmax = uvXmin + (blockWidth / (float)width);
            float uvYmax = uvYmin + (blockHeight / (float)height);

            return new Vector2[4]
            {
                new Vector2(uvXmin, uvYmin),
                new Vector2(uvXmax, uvYmin),
                new Vector2(uvXmax, uvYmax),
                new Vector2(uvXmin, uvYmax)
            };
        }
    }
}