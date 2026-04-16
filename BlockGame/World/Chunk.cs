using BlockGame.Rendering.Models;
using OpenTK.Mathematics;

namespace BlockGame.World
{
    public class Chunk
    {
        public int chunkX;
        public int chunkY;
        public int chunkZ;

        private byte[] chunkData;

        public byte[] GetChunkData()
        {
            return chunkData;
        }


        public bool isOptimized = false;
        public bool isGenerated = false;
        public Chunk(int x, int y, int z)
        {
            chunkData = new byte[4096];

            chunkX = x;
            chunkY = y;
            chunkZ = z;

            waterMesh = new VoxelMesh(Level.TextureMap, this);
            waterMesh.GetMesh().position = new Vector3(chunkX * 16, chunkY * 16, chunkZ * 16);

            solidMesh = new VoxelMesh(Level.TextureMap, this);
            solidMesh.GetMesh().position = new Vector3(chunkX * 16, chunkY * 16, chunkZ * 16);
        }


        private VoxelMesh solidMesh;
        private VoxelMesh waterMesh;
        public Mesh GetSolidMesh()
        {
            return solidMesh.GetMesh();
        }

        public Mesh GetWaterMesh()
        {
            return waterMesh.GetMesh();
        }

        public void SetVoxelAt(int x, int y, int z, byte voxel)
        {
            int index = x + (y * 16) + (z * 256);

            chunkData[index] = voxel;
            RegenerateMeshes();
            UpdateNear();
        }

        public byte GetVoxelAt(int x, int y, int z)
        {
            int index = x + (y * 16) + (z * 256);
            return chunkData[index];
        }

        public string GetFilePath()
        {
            return $"Level/{Level.LevelName}/{chunkX}_{chunkY}_{chunkZ}.ch";
        }

        public async Task LoadFromFile()
        {
            isGenerated = false;
            byte[] data = await File.ReadAllBytesAsync(GetFilePath());

            for (int x = 0; x < 16; x++)
                for (int y = 0; y < 16; y++)
                    for (int z = 0; z < 16; z++)
                    {
                        int index = x + (y * 16) + (z * 256);
                        chunkData[index] = data[GetIndexOf(x, y, z)];
                    }

            RegenerateMeshes(false);
        }

        public bool isDirty;

        public void SaveToFile()
        {
            isDirty = true;
        }

        private int GetIndexOf(int x, int y, int z)
        {
            return x + (y * 16) + (z * 16 * 16);
        }

        private bool hasAllNearLoaded;
        public bool HasAllNearLoaded()
        {
            return hasAllNearLoaded;
        }

        public async void UpdateNear()
        {

            Vector3i[] directions = {
                    new(1, 0, 0), new(-1, 0, 0),
                    new(0, 1, 0), new(0, -1, 0),
                    new(0, 0, 1), new(0, 0, -1)
                };

            foreach (var dir in directions)
            {
                Chunk? neighbor = Level.GetChunk(chunkX + dir.X, chunkY + dir.Y, chunkZ + dir.Z);

                if (neighbor != null && neighbor.isGenerated)
                {
                    _ = neighbor.RegenerateMeshes(false);
                }
            }
        }


        public async Task Generate()
        {
            if (File.Exists(GetFilePath()))
            {
                await LoadFromFile();
                RegenerateMeshes(false);
                isGenerated = true;
                UpdateNear();
                return;
            }

            Random rng = new Random();

            for (int x = 0; x < 16; x++)
                for (int y = 0; y < 16; y++)
                    for (int z = 0; z < 16; z++)
                    {
                        Vector3i worldPos = GetWorldPos(x, y, z);

                        float frequency = 0.02f;
                        float hill = (float)Level.noise.GetValue(worldPos.X * frequency, worldPos.Z * frequency);
                        float biome = (float)Level.noise.GetValue(worldPos.X * 0.002f, (worldPos.Z * 0.002f) + 10000);


                        int waterLevel = -3;
                        int index = x + (y * 16) + (z * 256);
                        float surface = hill * (biome * 12);


                        if (worldPos.Y <= surface)
                        {
                            float depth = surface - worldPos.Y;

                            if (depth <= 3)
                            {
                                chunkData[index] = 0x01; // dirt
                            }
                            else
                            {
                                chunkData[index] = 0x03; // stone
                            }
                        }
                        else
                        {
                            // Above ground
                            if (worldPos.Y <= waterLevel)
                            {
                                chunkData[index] = 0x02; // water
                            }
                            else
                            {
                                chunkData[index] = 0x00; // air
                            }
                        }

                        if (worldPos.Y == (int)surface && chunkData[index] == 0x00)
                        {
                            if (rng.Next(0, 20) == 2)
                            {
                                chunkData[index] = 0x07; // plant
                            }
                        }

                        if (worldPos.Y + 10 < hill)
                        {
                            float cave = (float)Level.noise.GetValue(worldPos.X * frequency, worldPos.Y * frequency, worldPos.Z * frequency);
                            float cave2 = (float)Level.noise.GetValue(worldPos.X * frequency * 2f, worldPos.Y * frequency * 2f, worldPos.Z * frequency * 2f);


                            if (cave > -.2 && cave < .2 && cave2 > 0.4)
                                chunkData[index] = 0x00;
                        }


                    }

            await RegenerateMeshes(true);
            isGenerated = true;
            UpdateNear();
        }

        public async Task RegenerateMeshes(bool save = true)
        {
            if (save)
                SaveToFile();

            hasAllNearLoaded = true;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        Chunk? chunk = Level.GetChunk(chunkX + x, chunkY + y, chunkZ + z);
                        if (chunk == null)
                            hasAllNearLoaded = false;
                    }
                }
            }


            byte[] waterMask = new byte[16 * 16 * 16];
            byte[] solidMask = new byte[16 * 16 * 16];
            for (int i = 0; i < 16 * 16 * 16; i++)
            {
                if (chunkData[i] == 0x02)
                {
                    waterMask[i] = chunkData[i];
                }
                else
                {
                    solidMask[i] = chunkData[i];
                }
            }

            waterMesh.Regenerate(waterMask);
            solidMesh.Regenerate(solidMask);
        }

        public Vector3i GetWorldPos(int x, int y, int z)
        {
            return new Vector3i((chunkX * 16) + x, (chunkY * 16) + y, (chunkZ * 16) + z);
        }
    }
}
