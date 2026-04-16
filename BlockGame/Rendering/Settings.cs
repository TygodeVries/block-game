namespace BlockGame.Rendering
{
    public class Settings
    {
        public int SheepCount { get; set; } = 10;
        public int SaveInterfall { get; set; } = 30_000;
        public int TicksPerSecond { get; set; } = 200_000;
        public string SkipIntro { get; set; } = "n";
        public int MovementMode { get; set; } = 0;
    }
}
