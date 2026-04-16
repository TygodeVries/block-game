
using OpenTK.Mathematics;

namespace BlockGame.Entities.Goals
{
    public abstract class Goal
    {
        public abstract Vector3 GetGoalLocation(Entity entity);
        public virtual void Finished(Entity entity) { }
    }
}
