namespace BlockGame.Rendering.World.BlockInfo
{
    public class RenderType
    {
        public static RenderTypes GetRenderType(byte block)
        {
            if (block == 0x05 || block == 0x06 || block == 0x07 | block == 0x08)
            {
                return RenderTypes.GRASS;
            }

            return RenderTypes.SOLID;
        }
    }

    public enum RenderTypes
    {
        SOLID,
        GRASS
    }
}
