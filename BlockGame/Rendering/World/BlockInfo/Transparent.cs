namespace BlockGame.Rendering.World.BlockInfo
{
    internal class Transparent
    {

        /// <summary>
        /// If the spesific blockID is a transparent block
        /// </summary>
        /// <param name="blockId"></param>
        /// <returns></returns>
        public static bool IsTransparentBlock(byte blockId)
        {
            return blockId == 0 || blockId == 5 || blockId == 2 || blockId == 6;
        }
    }
}
