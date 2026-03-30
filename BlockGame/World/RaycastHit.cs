using OpenTK.Mathematics;

namespace BlockGame.World
{
    public class RaycastHit
    {
        public Chunk Chunk;
        public Vector3i ChunkBlockPos;
        public Vector3i WorldBlockPos;
        public Vector3i Normal;
        public byte Block;

        public RaycastHit(Chunk chunk, Vector3i chunkBlockPos, Vector3i worldBlockPos, Vector3i normal, byte block)
        {
            Normal = normal;
            Chunk = chunk;
            ChunkBlockPos = chunkBlockPos;
            WorldBlockPos = worldBlockPos;
            Block = block;
        }
    }
}
