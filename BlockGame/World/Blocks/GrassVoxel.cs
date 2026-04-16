using OpenTK.Mathematics;

namespace BlockGame.World.Blocks
{
    public class GrassVoxel : SmartVoxel
    {
        public override void OnRandomTick(Vector3i pos)
        {
            Random rng = new Random();
            int x = rng.Next(-1, 2);
            int y = rng.Next(-1, 2);
            int z = rng.Next(-1, 2);

            byte? current = Level.GetVoxelAt(pos.X + x, pos.Y + y, pos.Z + z);

            if (current == null)
                return;

            if (current.Value == 0x01)
            {
                Level.SetVoxelAt(pos.X + x, pos.Y + y, pos.Z + z, 4);

                if (rng.Next(0, 4) == 1)
                    if (Level.GetVoxelAt(pos.X + x, pos.Y + y + 1, pos.Z + z) == 0)
                    {
                        Level.SetVoxelAt(pos.X + x, pos.Y + y + 1, pos.Z + z, (byte)rng.Next(5, 7));
                    }

                if (Level.GetVoxelAt(pos.X + x, pos.Y + y + 1, pos.Z + z) == 7)
                {
                    Level.SetVoxelAt(pos.X + x, pos.Y + y + 1, pos.Z + z, 8);
                }
            }

            if (x == 0 && y == 1 && z == 0 && current.Value == 0 && rng.Next(0, 10) == 2)
            {
                Level.SetVoxelAt(pos.X + x, pos.Y + y, pos.Z + z, 5);
            }
        }
    }
}
