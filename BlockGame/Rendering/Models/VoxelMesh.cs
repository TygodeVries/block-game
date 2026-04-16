using BlockGame.Rendering.Textures;
using BlockGame.Rendering.World.BlockInfo;
using BlockGame.World;
using OpenTK.Mathematics;

namespace BlockGame.Rendering.Models
{
    public class VoxelMesh
    {
        private Mesh mesh;
        private TextureMap textureMap;
        private Chunk chunk;
        public VoxelMesh(TextureMap textureMap, Chunk chunk)
        {
            this.chunk = chunk;
            this.textureMap = textureMap;

            mesh = new Mesh(RenderCanvas.worldShader);
        }

        public Mesh GetMesh()
        {
            return mesh;
        }



        private List<float>? verts = new List<float>();
        private List<int>? ind = new List<int>();
        private List<float>? uvs = new List<float>();
        private List<float>? normals = new List<float>();
        private List<float>? ao = new List<float>();
        public void Regenerate(byte[] voxels)
        {

            verts = new List<float>();
            ind = new List<int>();
            uvs = new List<float>();
            normals = new List<float>();
            ao = new List<float>();

            Chunk?[] neighbors = new Chunk?[] {
                Level.GetChunk(chunk.chunkX + 1, chunk.chunkY,     chunk.chunkZ),
                Level.GetChunk(chunk.chunkX - 1, chunk.chunkY,     chunk.chunkZ),
                Level.GetChunk(chunk.chunkX,     chunk.chunkY + 1, chunk.chunkZ),
                Level.GetChunk(chunk.chunkX,     chunk.chunkY - 1, chunk.chunkZ),
                Level.GetChunk(chunk.chunkX,     chunk.chunkY,     chunk.chunkZ + 1),
                Level.GetChunk(chunk.chunkX,     chunk.chunkY,     chunk.chunkZ - 1)
            };


            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        int index = x + (y * 16) + (z * 256);
                        byte voxel = voxels[index];
                        if (voxel == 0x00) continue;

                        RenderTypes type = RenderType.GetRenderType(voxel);

                        if (type == RenderTypes.SOLID)
                        {

                            AddSide(x, y, z, Vector3i.UnitX, voxel, neighbors, voxels);
                            AddSide(x, y, z, -Vector3i.UnitX, voxel, neighbors, voxels);
                            AddSide(x, y, z, Vector3i.UnitY, voxel, neighbors, voxels);
                            AddSide(x, y, z, -Vector3i.UnitY, voxel, neighbors, voxels);
                            AddSide(x, y, z, Vector3i.UnitZ, voxel, neighbors, voxels);
                            AddSide(x, y, z, -Vector3i.UnitZ, voxel, neighbors, voxels);
                        }

                        if (type == RenderTypes.GRASS)
                        {
                            AddFan(x, y, z, voxel);
                        }
                    }
                }
            }

            if (verts.Count == 0) return;

            var vertsArray = verts.ToArray();
            var indArray = ind.ToArray();
            var uvsArray = uvs.ToArray();
            var normalsArray = normals.ToArray();
            var aoArray = ao.ToArray();

            // Make sure GC actually does something :roll_eyes:
            verts = null;
            ind = null;
            uvs = null;
            ao = null;
            normals = null;

            MainThread.Run(() =>
            {
                mesh.Set(vertsArray, indArray, uvsArray, normalsArray, aoArray);
            });
        }

        private void AddFan(int x, int y, int z, byte block)
        {
            Vector3 basePos = new Vector3(x, y, z);

            Vector3 offset = new Vector3(0.1f, 0, 0.1f);

            Vector3[] quad1 = new Vector3[]
            {
                basePos + new Vector3(0, 1, 0) + offset,
                basePos + new Vector3(1, 1, 1) + offset,
                basePos + new Vector3(1, 0, 1) + offset,
                basePos + new Vector3(0, 0, 0) + offset
            };

            Vector3[] quad2 = new Vector3[]
            {
                basePos + new Vector3(1, 1, 0) + offset,
                basePos + new Vector3(0, 1, 1) + offset,
                basePos + new Vector3(0, 0, 1) + offset,
                basePos + new Vector3(1, 0, 0) + offset
            };

            Vector2[] uv = textureMap.GetUV(block);

            Vector3 normal = new Vector3(0, 1, 0);

            AddTris(quad1[0], quad1[1], quad1[2], uv[0], uv[1], uv[2], normal, 3, 3, 3);
            AddTris(quad1[2], quad1[3], quad1[0], uv[2], uv[3], uv[0], normal, 3, 3, 3);

            AddTris(quad2[0], quad2[1], quad2[2], uv[0], uv[1], uv[2], normal, 3, 3, 3);
            AddTris(quad2[2], quad2[3], quad2[0], uv[2], uv[3], uv[0], normal, 3, 3, 3);
        }
        private void AddSide(int x, int y, int z, Vector3i dir, byte voxel, Chunk?[] neighbors, byte[] voxels)
        {
            int nx = x + dir.X;
            int ny = y + dir.Y;
            int nz = z + dir.Z;

            byte neighborVoxel;

            if (nx >= 0 && nx < 16 && ny >= 0 && ny < 16 && nz >= 0 && nz < 16)
            {
                int index = nx + (ny * 16) + (nz * 256);
                neighborVoxel = voxels[index];
            }
            else
            {
                int neighborIdx = GetNeighborIndex(dir);
                Chunk? neighborChunk = neighbors[neighborIdx];

                if (neighborChunk == null)
                {
                    AddFace(x, y, z, dir, voxel);
                    return;
                }

                int localNx = (nx + 16) & 15;
                int localNy = (ny + 16) & 15;
                int localNz = (nz + 16) & 15;

                neighborVoxel = neighborChunk.GetVoxelAt(localNx, localNy, localNz);
            }

            if (Transparent.IsTransparentBlock(neighborVoxel) && neighborVoxel != voxel)
            {
                AddFace(x, y, z, dir, voxel);
            }
        }
        private int GetNeighborIndex(Vector3i dir)
        {
            if (dir.X == 1) return 0; if (dir.X == -1) return 1;
            if (dir.Y == 1) return 2; if (dir.Y == -1) return 3;
            if (dir.Z == 1) return 4; return 5;
        }

        private void AddFace(int x, int y, int z, Vector3i normal, byte voxel)
        {
            Vector3 basePos = new Vector3(x, y, z);

            Vector3[] corners = new Vector3[4];

            if (normal.X != 0)
            {
                float nx = normal.X > 0 ? 1f : 0f;
                corners[0] = basePos + new Vector3(nx, 0, 0);
                corners[1] = basePos + new Vector3(nx, 1, 0);
                corners[2] = basePos + new Vector3(nx, 1, 1);
                corners[3] = basePos + new Vector3(nx, 0, 1);
            }
            else if (normal.Y != 0)
            {
                float ny = normal.Y > 0 ? 1f : 0f;
                corners[0] = basePos + new Vector3(0, ny, 0);
                corners[1] = basePos + new Vector3(1, ny, 0);
                corners[2] = basePos + new Vector3(1, ny, 1);
                corners[3] = basePos + new Vector3(0, ny, 1);
            }
            else if (normal.Z != 0)
            {
                float nz = normal.Z > 0 ? 1f : 0f;
                corners[0] = basePos + new Vector3(0, 0, nz);
                corners[1] = basePos + new Vector3(1, 0, nz);
                corners[2] = basePos + new Vector3(1, 1, nz);
                corners[3] = basePos + new Vector3(0, 1, nz);
            }

            Vector2[] uv = textureMap.GetUV(voxel);
            Vector3 faceNormal = new Vector3(normal.X, normal.Y, normal.Z);


            AddTris(corners[0], corners[1], corners[2], uv[0], uv[1], uv[2], faceNormal, 3, 3, 3);
            AddTris(corners[2], corners[3], corners[0], uv[2], uv[3], uv[0], faceNormal, 3, 3, 3);
        }

        private void AddTris(Vector3 a, Vector3 b, Vector3 c, Vector2 uvA, Vector2 uvB, Vector2 uvC, Vector3 normal, float aoA, float aoB, float aoC)
        {

            verts.Add(a.X);
            verts.Add(a.Y);
            verts.Add(a.Z);

            verts.Add(b.X);
            verts.Add(b.Y);
            verts.Add(b.Z);


            verts.Add(c.X);
            verts.Add(c.Y);
            verts.Add(c.Z);

            ind.Add(ind.Count); ind.Add(ind.Count); ind.Add(ind.Count);

            uvs.Add(uvA.X); uvs.Add(uvA.Y);
            uvs.Add(uvB.X); uvs.Add(uvB.Y);
            uvs.Add(uvC.X); uvs.Add(uvC.Y);

            ao.Add(aoA);
            ao.Add(aoB);
            ao.Add(aoC);

            for (int i = 0; i < 3; i++)
            {
                normals.Add(normal.X);
                normals.Add(normal.Y);
                normals.Add(normal.Z);
            }
        }
    }
}
