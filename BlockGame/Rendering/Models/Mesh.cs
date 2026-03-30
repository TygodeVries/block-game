using BlockGame.Rendering.Shaders;
using BlockGame.Rendering.Textures;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace BlockGame.Rendering.Models
{
    public class Mesh
    {
        private int vertexArrayObject;
        private int vertexBufferObject;
        private int elementBufferObject;
        private int uvBufferObject;
        private int normalBufferObject;

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

            return translationMatrix * rotationMatrix;
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
        public void Set(float[] vertices, int[] indices, float[] uvs, float[] normals)
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

            // Normal Buffer
            normalBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, normalBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, normals.Length * sizeof(float), normals, BufferUsage.StaticDraw);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(2);

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
    }
}
