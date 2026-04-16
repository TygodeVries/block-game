using OpenTK.Mathematics;

namespace BlockGame.World.Blocks
{
    public class SaplingVoxel : SmartVoxel
    {
        public override void OnRandomTick(Vector3i pos)
        {
            Random rng = new Random();

            if (rng.Next(0, 10) == 1)
            {

                Level.SetVoxelAt(pos.X, pos.Y, pos.Z, 0);

                new Thread(() =>
                {
                    int treeSize = rng.Next(6, 9);
                    for (int i = 0; i < treeSize; i++)
                    {
                        MainThread.Run(() =>
                        {
                            if (Level.GetVoxelAt(pos.X, pos.Y + i, pos.Z) == 0)
                                Level.SetVoxelAt(pos.X, pos.Y + i, pos.Z, 0x09);
                        });

                        Thread.Sleep(300);
                    }

                    for (int y = -1; y <= 2; y++)
                        for (int x = -1; x <= 1; x++)
                            for (int z = -1; z <= 1; z++)
                            {
                                MainThread.Run(() =>
                                {
                                    if (Level.GetVoxelAt(pos.X + x, pos.Y + treeSize - 2 + y, pos.Z + z) == 0)
                                        Level.SetVoxelAt(pos.X + x, pos.Y + treeSize - 2 + y, pos.Z + z, 0x0A);
                                });

                                Thread.Sleep(100);
                            }
                }).Start();
            }
        }
    }
}
