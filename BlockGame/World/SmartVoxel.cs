using BlockGame.World.Blocks;
using OpenTK.Mathematics;


namespace BlockGame.World
{
    public abstract class SmartVoxel
    {
        private static Dictionary<byte, SmartVoxel> smartVoxels = new Dictionary<byte, SmartVoxel>();

        public static void Initialize()
        {
            smartVoxels.Add(0x03, new SandVoxel());
            //  smartVoxels.Add(0x02, new WaterVoxel());
        }

        public static void BlockUpdateAround(Vector3i pos)
        {
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                    for (int z = -1; z <= 1; z++)
                        BlockUpdate(pos + new Vector3i(x, y, z));
        }

        public static void BlockUpdate(Vector3i pos)
        {
            byte? type = Level.GetVoxelAt(pos.X, pos.Y, pos.Z);
            if (type == null)
                return;

            if (smartVoxels.ContainsKey(type.Value))
            {
                smartVoxels[type.Value].OnBlockUpdate(pos);
            }
        }

        public abstract byte GetBlockId();

        public virtual void OnBlockUpdate(Vector3i pos)
        {

        }


        /// <summary>
        /// #TODO
        /// </summary>
        public virtual void OnRandomTick()
        {

        }

        public Vector3 BlockPos;
    }
}
