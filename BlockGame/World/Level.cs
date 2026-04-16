using BlockGame.Rendering.Textures;
using BlockGame.Rendering.World;
using LibNoise.Primitive;
using OpenTK.Mathematics;
using System.Collections.Concurrent;


namespace BlockGame.World
{
    public class Level
    {


        public static int viewDistance = 8;
        public static int verticalViewDistance = 3;


        public static string LevelName = "Main";

        public static void CreateLevelFolder()
        {
            Directory.CreateDirectory($"Level/{LevelName}");
        }

        public static TextureMap TextureMap;

        public static Dictionary<(int x, int y, int z), Chunk> Chunks = new Dictionary<(int x, int y, int z), Chunk>();

        public static void UnloadAll()
        {
            Chunks.Clear();
            generatedChunks.Clear();
            generationQueue.Clear();
            isProcessing = false;
        }

        public static Chunk? GetChunk(int x, int y, int z)
        {
            lock (Chunks)
            {
                if (!Chunks.TryGetValue((x, y, z), out var chunk))
                    return null;

                return chunk;
            }
        }

        public static SimplexPerlin noise = new SimplexPerlin();

        public static RaycastHit? Raycast(Vector3 position, Vector3 direction, float maxDistance = 5)
        {

            Vector3 pointer = position;
            Vector3i lastVoxel = (Vector3i)pointer.Floor();
            while (Vector3.Distance(pointer, position) < maxDistance)
            {

                int chunkX = (int)MathF.Floor(pointer.X / 16);
                int chunkY = (int)MathF.Floor(pointer.Y / 16);
                int chunkZ = (int)MathF.Floor(pointer.Z / 16);

                int blockX = (int)(pointer.X - (chunkX * 16));
                int blockY = (int)(pointer.Y - (chunkY * 16));
                int blockZ = (int)(pointer.Z - (chunkZ * 16));
                Vector3i currentVoxel = (Vector3i)pointer.Floor();
                if (!Chunks.ContainsKey((chunkX, chunkY, chunkZ)))
                    return null;

                byte block = Chunks[(chunkX, chunkY, chunkZ)].GetVoxelAt(blockX, blockY, blockZ);
                if (block != 0x00)
                {
                    Vector3i delta = currentVoxel - lastVoxel;

                    Vector3i normal = (Vector3i)new Vector3(-delta.X, -delta.Y, -delta.Z);

                    return new RaycastHit(Chunks[(chunkX, chunkY, chunkZ)], new Vector3i(blockX, blockY, blockZ), (Vector3i)pointer.Floor(), normal, block);
                }

                lastVoxel = currentVoxel;
                pointer += direction / 100;
            }

            return null;
        }

        public static void Generate()
        {
            Console.WriteLine("Generating Terrain...");
        }

        private static int cameraChunkX;
        private static int cameraChunkY;
        private static int cameraChunkZ;

        public static void SetVoxelAt(int x, int y, int z, byte voxel)
        {

            int chunkX = (int)MathF.Floor(x / 16f);
            int chunkY = (int)MathF.Floor(y / 16f);
            int chunkZ = (int)MathF.Floor(z / 16f);

            int blockX = x - (chunkX * 16);
            int blockY = y - (chunkY * 16);
            int blockZ = z - (chunkZ * 16);

            GetChunk(chunkX, chunkY, chunkZ)?.SetVoxelAt(blockX, blockY, blockZ, voxel);

        }


        public static byte? GetVoxelAt(int x, int y, int z)
        {
            int chunkX = (int)MathF.Floor(x / 16f);
            int chunkY = (int)MathF.Floor(y / 16f);
            int chunkZ = (int)MathF.Floor(z / 16f);

            int blockX = x - (chunkX * 16);
            int blockY = y - (chunkY * 16);
            int blockZ = z - (chunkZ * 16);

            Chunk? chunk = GetChunk(chunkX, chunkY, chunkZ);

            if (chunk == null)
                return null;

            return chunk!.GetVoxelAt(blockX, blockY, blockZ);
        }

        public static bool IsVoxelLoadedAt(int x, int y, int z)
        {
            int chunkX = (int)MathF.Round(Camera.position.X / 16f);
            int chunkY = (int)MathF.Round(Camera.position.Y / 16f);
            int chunkZ = (int)MathF.Round(Camera.position.Z / 16f);

            return Chunks.ContainsKey((chunkX, chunkY, chunkZ));
        }

        public static void OnCameraMotion()
        {
            cameraChunkX = (int)MathF.Round(Camera.position.X / 16f);
            cameraChunkY = (int)MathF.Round(Camera.position.Y / 16f);
            cameraChunkZ = (int)MathF.Round(Camera.position.Z / 16f);

            UpdateWorld();
        }
        private static HashSet<(int x, int y, int z)> generatedChunks = new();


        private static bool isProcessing = false;
        private static ConcurrentQueue<(int, int, int)> generationQueue = new();

        public static async void UpdateWorld()
        {
            var candidateChunks = new List<(int x, int y, int z)>();

            for (int x = cameraChunkX - viewDistance; x <= cameraChunkX + viewDistance; x++)
            {
                for (int y = cameraChunkY - verticalViewDistance; y <= cameraChunkY + verticalViewDistance; y++)
                {
                    for (int z = cameraChunkZ - viewDistance; z <= cameraChunkZ + viewDistance; z++)
                    {
                        var pos = (x, y, z);

                        if (!Level.Chunks.ContainsKey(pos) && !generationQueue.Contains(pos))
                        {
                            candidateChunks.Add(pos);
                        }
                    }
                }
            }

            candidateChunks.Sort((a, b) =>
            {
                int distA = ((a.x - cameraChunkX) * (a.x - cameraChunkX)) +
                            ((a.y - cameraChunkY) * (a.y - cameraChunkY)) +
                            ((a.z - cameraChunkZ) * (a.z - cameraChunkZ));

                int distB = ((b.x - cameraChunkX) * (b.x - cameraChunkX)) +
                            ((b.y - cameraChunkY) * (b.y - cameraChunkY)) +
                            ((b.z - cameraChunkZ) * (b.z - cameraChunkZ));

                return distA.CompareTo(distB);
            });

            foreach (var pos in candidateChunks)
            {
                generationQueue.Enqueue(pos);
            }


            if (isProcessing) return;
            isProcessing = true;

            int initCount = generationQueue.Count;
            while (generationQueue.TryDequeue(out var pos))
            {
                double distSq = Math.Pow(pos.Item1 - cameraChunkX, 2) +
                                Math.Pow(pos.Item2 - cameraChunkY, 2) +
                                Math.Pow(pos.Item3 - cameraChunkZ, 2);

                if (distSq > Math.Pow(viewDistance + 2, 2))
                    continue;

                await Task.Run(async () =>
                {
                    var chunk = new Chunk(pos.Item1, pos.Item2, pos.Item3);
                    lock (Level.Chunks)
                        Level.Chunks[pos] = chunk;
                    await chunk.Generate();
                });

                if (generationQueue.Count % 10 == 0)
                {
                    await Task.Delay(1);
                }
            }

            isProcessing = false;
        }

        public static void Save()
        {

            lock (Level.Chunks)
            {
                foreach (Chunk chunk in Level.Chunks.Values)
                {
                    if (chunk.isDirty)
                    {
                        chunk.isDirty = false;
                        File.WriteAllBytes(chunk.GetFilePath(), chunk.GetChunkData());
                    }
                }
            }
        }
    }
}
