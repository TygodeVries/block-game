using BlockGame.World.Blocks;
using OpenTK.Mathematics;


namespace BlockGame.World
{
    public abstract class SmartVoxel
    {
        private static Dictionary<byte, SmartVoxel> smartVoxels = new Dictionary<byte, SmartVoxel>();

        public static void Initialize()
        {
            smartVoxels.Add(0x04, new GrassVoxel());
            smartVoxels.Add(0x08, new SaplingVoxel());
        }

        public static void DoRandomTick(Vector3i pos)
        {
            byte? block = Level.GetVoxelAt(pos.X, pos.Y, pos.Z);

            if (block == null)
                return;

            if (!smartVoxels.ContainsKey(block.Value))
                return;

            SmartVoxel sm = smartVoxels[block.Value];
            sm.OnRandomTick(pos);
        }

        public virtual void OnRandomTick(Vector3i pos) { }

        public Vector3 BlockPos;
    }
}
