using BlockGame.Rendering.Shaders;
using BlockGame.Rendering.Textures;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Globalization;

namespace BlockGame.Rendering.Models
{
    public class Mesh
    {
        private int vertexArrayObject;
        private int vertexBufferObject;
        private int elementBufferObject;
        private int uvBufferObject;
        private int normalBufferObject;
        private int aoBufferObject;

        // The shader that the mesh uses
        public ShaderProgram shader;

        // The texture of the mesh
        public Texture? texture;

        public Mesh(ShaderProgram shader)
        {
            this.shader = shader;
        }

        /// <summary>
        /// Render the mesh
        /// </summary>
        public void Render()
        {
            if (indSize >= 0)
            {
                GL.BindVertexArray(vertexArrayObject);
                GL.DrawElements(PrimitiveType.Triangles, indSize, DrawElementsType.UnsignedInt, 0);
            }
        }

        /// <summary>
        /// The rotation of the mesh
        /// </summary>
        public Vector3 rotation;

        /// <summary>
        /// The position of the mesh
        /// </summary>
        public Vector3 position;

        public void SetForwards(Vector3 forwards)
        {
            forwards.Y = 0;
            forwards = Vector3.Normalize(forwards);

            rotation.Y = MathHelper.RadiansToDegrees(
                MathF.Atan2(forwards.X, forwards.Z)
            );
        }

        public Vector3 GetForwards()
        {
            Vector3 radians = new Vector3(
                OpenTK.Mathematics.MathHelper.DegreesToRadians(rotation.X),
                OpenTK.Mathematics.MathHelper.DegreesToRadians(rotation.Y),
                OpenTK.Mathematics.MathHelper.DegreesToRadians(rotation.Z));

            // Yaw (Y), Pitch (X)
            float yaw = radians.Y;
            float pitch = radians.X;

            float x = MathF.Cos(pitch) * MathF.Sin(yaw);
            float y = MathF.Sin(pitch);
            float z = MathF.Cos(pitch) * MathF.Cos(yaw);

            return new Vector3(x, y, z).Normalized();
        }

        public Matrix4 GetModelMatrix()
        {
            Vector3 radians = new Vector3(
                OpenTK.Mathematics.MathHelper.DegreesToRadians(rotation.X),
                OpenTK.Mathematics.MathHelper.DegreesToRadians(rotation.Y),
                OpenTK.Mathematics.MathHelper.DegreesToRadians(rotation.Z));

            Matrix4 rotationMatrix =
                Matrix4.CreateRotationY(radians.Y) *
                Matrix4.CreateRotationX(radians.X) *
                Matrix4.CreateRotationZ(radians.Z);

            Matrix4 translationMatrix = Matrix4.CreateTranslation(position);
            return rotationMatrix * translationMatrix;
        }

        private int indSize = -1;
        /// <summary>
        /// Set the contents of the mesh, and upload it to OpenGL
        /// (MAIN THREAD ONLY!)
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        /// <param name="uvs"></param>
        /// <param name="normals"></param>
        public void Set(float[] vertices, int[] indices, float[] uvs, float[]? normals = null, float[]? ao = null)
        {
            indSize = -1;
            // Delete any old stuff
            if (vertexArrayObject != 0) GL.DeleteVertexArray(vertexArrayObject);
            if (vertexBufferObject != 0) GL.DeleteBuffer(vertexBufferObject);
            if (elementBufferObject != 0) GL.DeleteBuffer(elementBufferObject);
            if (uvBufferObject != 0) GL.DeleteBuffer(uvBufferObject);
            if (normalBufferObject != 0) GL.DeleteBuffer(normalBufferObject);

            vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);

            // Position Buffer
            vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsage.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0); // Stride 0 is often safer for tightly packed
            GL.EnableVertexAttribArray(0);

            // UV Buffer
            uvBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, uvBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, uvs.Length * sizeof(float), uvs, BufferUsage.StaticDraw);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(1);

            if (normals != null)
            {
                // Normal Buffer
                normalBufferObject = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, normalBufferObject);
                GL.BufferData(BufferTarget.ArrayBuffer, normals.Length * sizeof(float), normals, BufferUsage.StaticDraw);
                GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(2);
            }

            if (ao != null)
            {
                // AO buffer
                aoBufferObject = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, aoBufferObject);
                GL.BufferData(BufferTarget.ArrayBuffer, ao.Length * sizeof(float), ao, BufferUsage.StaticDraw);
                GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(3);
            }

            // Index Buffer
            elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsage.StaticDraw);


            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            indSize = indices.Length;
            hasData = true;
        }

        public bool hasData = false;

        public static Mesh? FromFileObj(string asset, ShaderProgram shader)
        {
            string file = asset;
            List<Vector3> positions = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> texcoords = new List<Vector2>();

            List<float> finalVertices = new List<float>();
            List<float> finalNormals = new List<float>();
            List<float> finalUVs = new List<float>();
            List<int> finalIndices = new List<int>();

            Dictionary<string, int> vertexMap = new Dictionary<string, int>();

            string[] lines = File.ReadAllLines(file);

            foreach (string line in lines)
            {
                if (line.StartsWith("v "))
                {
                    string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    float x = float.Parse(parts[1], CultureInfo.InvariantCulture);
                    float y = float.Parse(parts[2], CultureInfo.InvariantCulture);
                    float z = float.Parse(parts[3], CultureInfo.InvariantCulture);
                    positions.Add(new Vector3(x, y, z));
                }
                else if (line.StartsWith("vt "))
                {
                    string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    float u = float.Parse(parts[1], CultureInfo.InvariantCulture);
                    float v = float.Parse(parts[2], CultureInfo.InvariantCulture);
                    texcoords.Add(new Vector2(u, v));
                }
                else if (line.StartsWith("vn "))
                {
                    string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    float x = float.Parse(parts[1], CultureInfo.InvariantCulture);
                    float y = float.Parse(parts[2], CultureInfo.InvariantCulture);
                    float z = float.Parse(parts[3], CultureInfo.InvariantCulture);
                    normals.Add(new Vector3(x, y, z));
                }
                else if (line.StartsWith("f "))
                {
                    string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    List<int> faceIndices = new();

                    for (int i = 1; i < parts.Length; i++)
                    {
                        string[] tokens = parts[i].Split('/');

                        int posIndex = int.Parse(tokens[0]) - 1;
                        int uvIndex = (tokens.Length > 1 && !string.IsNullOrEmpty(tokens[1])) ? int.Parse(tokens[1]) - 1 : -1;
                        int normIndex = (tokens.Length > 2 && !string.IsNullOrEmpty(tokens[2])) ? int.Parse(tokens[2]) - 1 : -1;

                        string key = $"{posIndex}/{uvIndex}/{normIndex}";
                        if (!vertexMap.TryGetValue(key, out int index))
                        {
                            Vector3 pos = positions[posIndex];
                            Vector2 uv = (uvIndex >= 0 && uvIndex < texcoords.Count) ? texcoords[uvIndex] : Vector2.Zero;
                            Vector3 norm = (normIndex >= 0 && normIndex < normals.Count) ? normals[normIndex] : Vector3.Zero;

                            finalVertices.Add(pos.X);
                            finalVertices.Add(pos.Y);
                            finalVertices.Add(pos.Z);

                            finalUVs.Add(uv.X);
                            finalUVs.Add(1 - uv.Y);

                            finalNormals.Add(norm.X);
                            finalNormals.Add(norm.Y);
                            finalNormals.Add(norm.Z);

                            index = (finalVertices.Count / 3) - 1;
                            vertexMap[key] = index;
                        }

                        faceIndices.Add(index);
                    }

                    for (int i = 1; i < faceIndices.Count - 1; i++)
                    {
                        finalIndices.Add(faceIndices[0]);
                        finalIndices.Add(faceIndices[i]);
                        finalIndices.Add(faceIndices[i + 1]);
                    }
                }
            }

            Mesh mesh = new Mesh(shader);

            mesh.Set(finalVertices.ToArray(), finalIndices.ToArray(), finalUVs.ToArray(), finalNormals.ToArray());

            return mesh;
        }
    }
}
