using BlockGame.Rendering;
using BlockGame.World;
using OpenTK.Windowing.Desktop;
using System.Diagnostics;
using System.Text.Json;

public class Program
{
    public static bool SavingEnabled = false;

    public static void Logo()
    {
        string[] lines = File.ReadAllLines("Resources/logo.txt");
        for (int i = 0; i < lines.Length; i++)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Thread.Sleep(100);
            Console.WriteLine(lines[i]);


            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    public static void Legal()
    {
        string line = File.ReadAllText("Resources/legal.txt");
        for (int i = 0; i < line.Length; i++)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Thread.Sleep(1);
            Console.Write(line[i]);


            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    public static void Main(string[] args)
    {
        ReloadSettings();

        if (settings.SkipIntro != "n")
        {
            Console.WriteLine("Fast Start Mode is Active!!!");
            Start();
            return;
        }

        Console.WriteLine("(c) Tygo de Vries -> April 17th - 2026");
        Console.WriteLine("> Loading...");
        Thread.Sleep(2000);
        Console.WriteLine("> Ready! Press 'ENTER' to begin!");
        Console.ReadLine();

        Console.Clear();

        Console.WriteLine("(c) Tygo de Vries -> April 17th - 2026");
        Console.WriteLine();
        Logo();

        Console.WriteLine("> Welcome to Life OS!");
        Thread.Sleep(1500);

        Console.WriteLine("> Please enter your username, and press enter:");
        string username = Console.ReadLine();
        Console.WriteLine();
        Console.WriteLine();

        Console.WriteLine("> You are about to wake the remote life recovery droid.");
        Thread.Sleep(1500);

        Console.WriteLine("> Are you sure? This action can not be undone. type 'Y' or 'N')");
        string response = Console.ReadLine().ToLower();

        if (response == "n")
        {
            Console.WriteLine("> Sarcasm detected, activating sarcasm translation layer.");
            Thread.Sleep(1500);

            Console.WriteLine("  [---  Press Enter to Continue  ---]  ");

            Console.ReadLine();
        }

        else if (response != "y")
        {

            Console.WriteLine("> Interpreting vague answer as: \"absolutely!\"");
            Thread.Sleep(1500);

            Console.WriteLine("  [---  Press Enter to Continue  ---]  ");

            Console.ReadLine();
        }

        bool hasReadMission = false;

        while (true)
        {
            Console.Clear();
            Logo();

            Console.WriteLine($"Welcome to LIFEOS, {username}!");
            Thread.Sleep(300);
            Console.WriteLine($"Its {DateTime.Now.DayOfWeek} {DateTime.Now.Day}/{DateTime.Now.Month} 2104.");

            Thread.Sleep(300);
            Console.WriteLine();
            Console.WriteLine("[ Remote Droid Interface (RDI) ]");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("1: Open Mission Statement");
            Console.WriteLine("2: Open Credits");

            if (!hasReadMission)
                Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("3: Start Mission (Must read mission statement first!)");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("4: Open Settings");


            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine();
            Console.WriteLine("> Please type the number of the action you would like to perform.");

            string option = Console.ReadLine();

            if (option == "1")
            {
                Console.Clear();
                Logo();

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("[Mission Statement] Earth, 2026");
                Console.WriteLine("When you are reading this, that means that everything is dead.");
                Console.WriteLine("If you find this, this also means you have the responsability of cleaning up our mess.");

                Thread.Sleep(1000);
                Console.WriteLine("A lifeform has been detected on Earth, for the first time in a long time.");
                Console.WriteLine("You are now in control of a state-of-the-art remote cleanup bot.");
                Console.WriteLine("Use the resources the bot has access to, to do the following:");

                Thread.Sleep(1000);

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("- Investigate the lifeform");
                Console.WriteLine("- Regrow enough nature to life comfortably.");
                Console.WriteLine("- Build a shelter");

                Thread.Sleep(1000);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
                Console.WriteLine("This lifeform must be one of the strongest in the multiverse, as nothing else has managed to stay alive.");
                Console.WriteLine("So please be kind to them.");
                Console.WriteLine();



                Thread.Sleep(3000);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Goodluck, whoever you may be.");

                Console.WriteLine();

                hasReadMission = true;
                Console.WriteLine("  [---  Press Enter to Continue  ---]  ");

                Console.ReadLine();
            }
            else if (option == "2")
            {
                Console.Clear();


                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(File.ReadAllText("Resources/me.txt"));


                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();


                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("- About -");
                Console.WriteLine("In this game you CHANGE the world, by bringing back life, and creating structures. You are the ORIGIN of nature and the creations of this world.");
                Console.WriteLine("The game was made in a 70 hour time frame.");
                Console.WriteLine("I had as a personal goal to get better at making games without a game engine, so here we are. I learned a lot about OpenGL and realtime mesh generation, as well as multi threading.");
                Console.WriteLine("I personally recommend placing down grass blocks, to see the system build that spreads the grass, I am really proud of that.");
                Console.WriteLine("The world is infinite, and caves generate underground, so take some time to explore.");


                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("- Credits -");
                Console.WriteLine("All assets, code and resources created by Tygo de Vries with OpenTK in C#.");
                Console.WriteLine("Thank you Sofie, for playtesting the game and giving the suggestion to add sheeps.");
                Console.WriteLine("Geert, for mentioning that the game needed a better UI, and Richard for your emotional support.");
                Console.WriteLine("Some external code has been used as inspiration, like from my game engine, Dart. As well as consulting some AI that tolt me what lines I turned the wrong way around.");

                Console.ReadLine();
            }

            else if (option == "3")
            {
                if (hasReadMission)
                    break;
                else
                {
                    Console.Clear();
                    Console.WriteLine($"> You chose not to read the mission statement. I am very disappointed in you.");
                    Thread.Sleep(2000);
                    break;
                }
            }
            else if (option == "4")
            {
                Process.Start("notepad.exe", "settings.json");
            }
        }

        Console.Clear();
        Thread.Sleep(500);

        Console.WriteLine(">>>>>  Starting   Mission  <<<<<");
        Thread.Sleep(1000);

        Console.WriteLine("> Establishing Deep Space Link...");
        Thread.Sleep(300);
        Console.WriteLine("> Signal re-routed through orbital relay.");
        Thread.Sleep(400);

        Console.WriteLine("> Activating Wheels.");
        Thread.Sleep(500);
        Console.WriteLine("> Readying Deployers.");
        Thread.Sleep(200);
        Console.WriteLine("> Starting Camera Feed.");

        Thread.Sleep(400);
        Console.Write("> Checking Camera Feed Status: ");
        Thread.Sleep(600);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Perfect (< 10ms)!");
        Console.ForegroundColor = ConsoleColor.White;

        Thread.Sleep(200);
        Console.WriteLine("> Calibrating planetary sensors...");
        Thread.Sleep(400);

        Console.Write("> Atmospheric analysis: ");
        Thread.Sleep(500);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Bad");
        Console.ForegroundColor = ConsoleColor.White;

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("> Mission Paused <");
        Console.WriteLine("> Further Analysis Activated.");

        Thread.Sleep(3000);
        Console.WriteLine("> Lowering minimum requirements for mission... (CMGT room level)");

        Thread.Sleep(3000);
        Console.Write("> Atmospheric re-analysis: ");
        Thread.Sleep(500);
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("Close Enough");
        Console.ForegroundColor = ConsoleColor.White;


        Thread.Sleep(500);
        Console.WriteLine("> Gravity compensation systems: ONLINE");
        Thread.Sleep(600);
        Console.WriteLine("> Terrain mapping: SCANNING...");

        Thread.Sleep(400);
        Console.WriteLine("> Satellite uplink: SYNCHRONIZED");
        Thread.Sleep(500);

        Console.WriteLine("> Remote pilot connection: ");
        Thread.Sleep(400);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("ESTABLISHED");
        Console.ForegroundColor = ConsoleColor.White;

        Legal();
        Thread.Sleep(300);
        Console.Clear();

        Console.WriteLine();
        Console.Write(">>>>> MISSION STATUS: ");

        Thread.Sleep(1500);
        Start();
    }

    public static Settings? settings = null;

    public static void CreateDefaultSettings()
    {
        settings = new Settings();
        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
        jsonSerializerOptions.WriteIndented = true;

        File.WriteAllText("settings.json", JsonSerializer.Serialize(settings, jsonSerializerOptions));
    }

    private static void ReloadSettings()
    {
        if (File.Exists("settings.json"))
        {
            try
            {
                settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText("settings.json"));
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to read settings file!!!!!!!!!");
                Console.WriteLine(e);
            }
        }
        if (settings == null)
        {
            // Create Defaults
            CreateDefaultSettings();
        }
    }

    private static void Start()
    {

        ReloadSettings();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("ACTIVE");


        SmartVoxel.Initialize();
        Level.CreateLevelFolder();
        NativeWindowSettings windowSettings = new NativeWindowSettings()
        {
            ClientSize = new OpenTK.Mathematics.Vector2i(500, 500),
            Title = "Life OS Streaming Window (Live)",
            StartVisible = true
        };

        DedicatedSwitch.Switch();


        RenderCanvas renderCanvas = new RenderCanvas(GameWindowSettings.Default, windowSettings);

        renderCanvas.Run();
    }
}