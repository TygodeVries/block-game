using OpenTK.Mathematics;

namespace BlockGame.World.Blocks
{
    public class SandVoxel : SmartVoxel
    {
        public override byte GetBlockId()
        {
            return 3;
        }

        public override void OnBlockUpdate(Vector3i pos)
        {
            if (Level.GetVoxelAt(pos.X, pos.Y - 1, pos.Z) == 0x00)
            {
                Level.SetVoxelAt(pos.X, pos.Y - 1, pos.Z, 0x03);
                Level.SetVoxelAt(pos.X, pos.Y, pos.Z, 0x00);
            }
        }
    }
}
