using BlockGame.Rendering.Models;
using OpenTK.Mathematics;

namespace BlockGame.World
{
    public class Chunk
    {
        public int chunkX;
        public int chunkY;
        public int chunkZ;

        private byte[,,] chunkData;

        public bool isOptimized = false;
        public bool isGenerated = false;
        public Chunk(int x, int y, int z)
        {

            chunkData = new byte[16, 16, 16];

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
            chunkData[x, y, z] = voxel;
            RegenerateMeshes();
            UpdateNear();
        }

        public byte GetVoxelAt(int x, int y, int z)
        {
            return chunkData[x, y, z];
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
                        chunkData[x, y, z] = data[GetIndexOf(x, y, z)];
                    }

            RegenerateMeshes(false);
        }

        public void SaveToFile()
        {
            byte[] data = new byte[16 * 16 * 16];

            for (int x = 0; x < 16; x++)
                for (int y = 0; y < 16; y++)
                    for (int z = 0; z < 16; z++)
                    {
                        data[GetIndexOf(x, y, z)] = chunkData[x, y, z];
                    }

            File.WriteAllBytes(GetFilePath(), data);
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

            for (int x = 0; x < 16; x++)
                for (int y = 0; y < 16; y++)
                    for (int z = 0; z < 16; z++)
                    {
                        Vector3i worldPos = GetWorldPos(x, y, z);

                        float frequency = 0.02f;
                        float hill = (float)Level.noise.GetValue(worldPos.X * frequency, worldPos.Z * frequency);


                        int waterLevel = -3;

                        if (worldPos.Y + 3 < hill * 15)
                        {
                            chunkData[x, y, z] = 0x04;
                        }
                        else if (worldPos.Y < hill * 15)
                        {
                            // Underground
                            chunkData[x, y, z] = 0x01;

                            // Sand
                            if (worldPos.Y < waterLevel + 1)
                            {
                                chunkData[x, y, z] = 0x03;
                            }
                        }
                        else
                        {

                            // Above ground
                            if (worldPos.Y < waterLevel)
                            {
                                chunkData[x, y, z] = 0x02;
                            }
                            else
                            {
                                chunkData[x, y, z] = 0x00;
                            }
                        }


                        if (worldPos.Y + 10 < hill)
                        {
                            float cave = (float)Level.noise.GetValue(worldPos.X * frequency, worldPos.Y * frequency, worldPos.Z * frequency);
                            float cave2 = (float)Level.noise.GetValue(worldPos.X * frequency * 2f, worldPos.Y * frequency * 2f, worldPos.Z * frequency * 2f);


                            if (cave > -.2 && cave < .2 && cave2 > 0.4)
                                chunkData[x, y, z] = 0x00;
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


            for (int x = 0; x < 16; x++)
                for (int y = 0; y < 16; y++)
                    for (int z = 0; z < 16; z++)
                    {
                        byte voxel = GetVoxelAt(x, y, z);

                        if (voxel == 2)
                        {
                            waterMesh.SetVoxel(x, y, z, voxel);
                            solidMesh.SetVoxel(x, y, z, 0);
                        }
                        else
                        {
                            solidMesh.SetVoxel(x, y, z, voxel);
                            waterMesh.SetVoxel(x, y, z, 0);
                        }
                    }

            waterMesh.Regenerate();
            solidMesh.Regenerate();
        }

        public Vector3i GetWorldPos(int x, int y, int z)
        {
            return new Vector3i((chunkX * 16) + x, (chunkY * 16) + y, (chunkZ * 16) + z);
        }
    }
}
