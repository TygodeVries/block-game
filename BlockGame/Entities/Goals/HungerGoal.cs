using BlockGame.World;
using OpenTK.Mathematics;

namespace BlockGame.Entities.Goals
{
    public class HungerGoal : Goal
    {
        public bool foundGrass;
        public override void Finished(Entity entity)
        {
            if (foundGrass)
            {
                new Thread(() =>
                {
                    Thread.Sleep(1000);

                    MainThread.Run(() =>
                    {
                        Level.SetVoxelAt((int)block.X, (int)block.Y, (int)block.Z, 0);

                        entity.hunger = 10;
                    });
                }).Start();
            }
        }

        private Vector3 block;

        public override Vector3 GetGoalLocation(Entity entity)
        {
            int nx = (int)entity.mesh.position.X;
            int ny = (int)entity.mesh.position.Y;
            int nz = (int)entity.mesh.position.Z;

            Vector3 best = Vector3.Zero;
            float bestDist = float.MaxValue;

            for (int y = -2; y < 2; y++)
                for (int x = -5; x < 5; x++)
                    for (int z = -5; z < 5; z++)
                    {
                        int gx = nx + x;
                        int gy = ny + y;
                        int gz = nz + z;

                        if (Level.GetVoxelAt(gx, gy, gz) == 5)
                        {
                            float d = (x * x) + (y * y) + (z * z);
                            if (d < bestDist)
                            {
                                bestDist = d;
                                best = new Vector3(gx, gy, gz);
                            }
                        }
                    }

            if (bestDist < float.MaxValue)
            {
                foundGrass = true;
                block = best;
                return best + new Vector3(0.5f, 0.5f, 0.5f);
            }

            Random rng = new Random();

            return entity.mesh.position + new Vector3(rng.Next(-5, 5), rng.Next(-5, 5), rng.Next(-5, 5));
        }
    }
}
