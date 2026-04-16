using OpenTK.Mathematics;

namespace BlockGame.Entities.Goals
{
    public class WanderGoal : Goal
    {

        public override Vector3 GetGoalLocation(Entity entity)
        {
            Random rng = new Random();

            return entity.mesh.position + new Vector3(rng.Next(-5, 5), rng.Next(-5, 5), rng.Next(-5, 5));
        }
    }
}
