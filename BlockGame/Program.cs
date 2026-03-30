using BlockGame.Rendering;
using BlockGame.World;
using OpenTK.Windowing.Desktop;

public class Program
{
    public static void Main(string[] args)
    {
        SmartVoxel.Initialize();
        Console.WriteLine("Starting Block Game...");
        Level.CreateLevelFolder();
        NativeWindowSettings windowSettings = new NativeWindowSettings()
        {
            ClientSize = new OpenTK.Mathematics.Vector2i(500, 500),
            Title = "Block Game",
            StartVisible = true
        };
        DedicatedSwitch.Switch();


        RenderCanvas renderCanvas = new RenderCanvas(GameWindowSettings.Default, windowSettings);

        Console.WriteLine("Opening Window...");
        renderCanvas.Run();
        Console.WriteLine("Window Exited!");
    }
}