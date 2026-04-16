using BlockGame.Entities.Goals;
using BlockGame.Rendering;
using BlockGame.Rendering.Models;
using BlockGame.Rendering.Textures;
using BlockGame.Rendering.World;
using BlockGame.World;
using OpenTK.Mathematics;


namespace BlockGame.Entities
{
    public class Entity
    {

        private Goal goal;

        public Mesh mesh;

        public Entity(string model, Vector3 spawn)
        {
            mesh = Mesh.FromFileObj($"models/{model}.obj", RenderCanvas.entityShader);
            mesh.texture = ImageTexture.LoadFromPng("textures/sheep.png");
            RenderCanvas.entities.Add(this);

            Teleport(Camera.position);
            mesh.position = spawn;
            goalPosition = mesh.position;
        }

        private void Teleport(Vector3 pos)
        {
            mesh.position = pos;
        }


        private Vector3 goalPosition;

        public Vector3 velocity;
        public bool jump;

        private float goalCooldown;
        public float hunger = 10;

        public void Tick()
        {
            hunger -= RenderCanvas.deltaTime;
            Vector3 toGoal = goalPosition - mesh.position;
            toGoal.Y = 0;

            if (toGoal.LengthSquared > 0.0001f)
            {
                toGoal = toGoal.Normalized();
                mesh.SetForwards(-toGoal);

                velocity.X = toGoal.X * 1f;
                velocity.Z = toGoal.Z * 1f;
                goalCooldown = 5;
            }
            else
            {
                velocity.X = 0;
                velocity.Z = 0;

                if (goal != null)
                {
                    goal.Finished(this);
                    goal = null;
                }

                if (goalCooldown < 0)
                {
                    if (hunger < 0)
                    {
                        goal = new HungerGoal();
                    }
                    else
                    {
                        goal = new WanderGoal();
                    }

                    goalPosition = goal.GetGoalLocation(this);
                }

                goalCooldown -= RenderCanvas.deltaTime;
            }

            velocity.Y -= RenderCanvas.deltaTime * 20f;

            Motion();
        }


        private float speed = 2;
        public void Motion()
        {
            Vector3 position = mesh.position;

            bool isGrounded = Collides(position + new Vector3(0, -0.05f, 0), false);
            if (isGrounded && velocity.Y < 0)
                velocity.Y = 0;

            isGrounded = Collides(position + new Vector3(0, -0.05f, 0), true);

            // Jumping
            if (jump && isGrounded)
            {
                velocity.Y = 5f;
                isGrounded = false;
                jump = false;
            }


            Vector3 step = velocity * RenderCanvas.deltaTime * speed;
            // Y
            Vector3 newPosition = position;
            newPosition.Y += step.Y;

            if (Collides(newPosition, false))
            {
                velocity.Y = 0;
            }

            // X
            newPosition = mesh.position;
            newPosition.X += step.X;

            if (Collides(newPosition, false))
            {
                if (!Collides(newPosition + new Vector3(0, 1, 0), false))
                {
                    jump = true;
                }
                velocity.X = 0;
            }
            // Z
            newPosition = mesh.position;
            newPosition.Z += step.Z;

            if (Collides(newPosition, false))
            {
                if (!Collides(newPosition + new Vector3(0, 1, 0), false))
                {
                    jump = true;
                }
                velocity.Z = 0;
            }

            // Set final new position


            Teleport(mesh.position + (velocity * RenderCanvas.deltaTime * speed));

        }

        private bool IsSolid(int x, int y, int z, bool isGround)
        {
            byte? block = Level.GetVoxelAt(x, y, z);
            if (block == null)
                return true;

            return block != 0 && (block != 2 || isGround) && block != 5 && block != 6 && block != 7 && block != 8;
        }

        private bool Collides(Vector3 p, bool isGround)
        {

            float playerWidth = 1;
            float playerHeight = 1;


            int minX = (int)MathF.Floor(p.X - (playerWidth / 2));
            int maxX = (int)MathF.Floor(p.X + (playerWidth / 2));

            int minY = (int)MathF.Floor(p.Y);
            int maxY = (int)MathF.Floor(p.Y + playerHeight);

            int minZ = (int)MathF.Floor(p.Z - (playerWidth / 2));
            int maxZ = (int)MathF.Floor(p.Z + (playerWidth / 2));

            for (int x = minX; x <= maxX; x++)
                for (int y = minY; y <= maxY; y++)
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        if (IsSolid(x, y, z, isGround))
                            return true;
                    }

            return false;
        }
    }
}
