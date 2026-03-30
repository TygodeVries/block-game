using OpenTK.Mathematics;

namespace BlockGame.World.Blocks
{
    public class WaterVoxel : SmartVoxel
    {
        public override byte GetBlockId()
        {
            return 2;
        }

        public override void OnBlockUpdate(Vector3i pos)
        {
            for (int x = -1; x <= 1; x++)
                for (int z = -1; z <= 1; z++)
                {
                    MainThread.Run(() =>
                    {
                        if (Level.GetVoxelAt(pos.X + x, pos.Y - 1, pos.Z + z) == 0x00)
                        {
                            Level.SetVoxelAt(pos.X + x, pos.Y - 1, pos.Z + z, 0x02);
                            Level.SetVoxelAt(pos.X, pos.Y, pos.Z, 0x00);
                        }
                    });
                }
        }
    }
}
