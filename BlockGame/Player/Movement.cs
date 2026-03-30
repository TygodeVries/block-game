namespace BlockGame.Player
{
    public abstract class Movement
    {
        /// <summary>
        /// Handle movement for the camera object
        /// </summary>
        /// <param name="time">The time since the last frame</param>
        public abstract void Update(double time);
    }
}
