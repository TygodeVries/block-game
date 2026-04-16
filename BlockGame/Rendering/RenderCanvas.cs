using BlockGame.Entities;
using BlockGame.Input;
using BlockGame.Player;
using BlockGame.Rendering.Models;
using BlockGame.Rendering.Shaders;
using BlockGame.Rendering.Textures;
using BlockGame.Rendering.UI;
using BlockGame.Rendering.World;
using BlockGame.World;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.ComponentModel;

namespace BlockGame.Rendering
{
    public class RenderCanvas : GameWindow
    {
        public static ShaderProgram worldShader;
        public static ShaderProgram fluidShader;
        public static ShaderProgram uiShader;
        public static ShaderProgram entityShader;
        private ImageTexture[] worldTexture;
        private float time;

        private Movement movement;

        public RenderCanvas(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            if (Program.settings!.MovementMode == 0)
            {
                movement = new NormalMovement();
            }

            else if (Program.settings!.MovementMode == 1)
            {
                movement = new SpectatorMovement();
            }


            worldTexture = new ImageTexture[2];
            worldTexture[0] = ImageTexture.LoadFromPng("textures/block1.png");
            worldTexture[1] = ImageTexture.LoadFromPng("textures/block2.png");


            Level.TextureMap = new TextureMap(worldTexture[0]);
            worldShader = new ShaderProgram(File.ReadAllText("shaders/block.vert"), File.ReadAllText("shaders/block.frag"));
            fluidShader = new ShaderProgram(File.ReadAllText("shaders/fluid.vert"), File.ReadAllText("shaders/fluid.frag"));
            uiShader = new ShaderProgram(File.ReadAllText("shaders/ui.vert"), File.ReadAllText("shaders/ui.frag"));
            entityShader = new ShaderProgram(File.ReadAllText("shaders/entity.vert"), File.ReadAllText("shaders/entity.frag"));

            Random rng = new Random();
            for (int i = 0; i < Program.settings!.SheepCount; i++)
            {
                Entity entity = new Entity("sheep", new Vector3(rng.Next(-10, 10), 10, rng.Next(-10, 10)));

            }
        }

        public static Mesh loadingMesh;

        protected override void OnLoad()
        {
            info = File.ReadAllText("Resources/info.txt");
            SetDefaultFog();
            SetFog(goal_FogColor, 0, 0, 1000);
            UserInterface.AddUIElement(new Vector2(0, 0), new Vector2(0.02f, 0.03f), ImageTexture.LoadFromPng("textures/crosshair.png"));

            loadingMesh = UserInterface.AddUIElement(new Vector2(0, 0), new Vector2(1, 1), ImageTexture.LoadFromPng("textures/Loading.png"));

            Level.OnCameraMotion();

            GL.ClearColor(0.6f, 0.7f, 0.8f, 1.0f);

            var building = new Building();
            building.UpdateSelector();
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            //GL.Enable(EnableCap.CullFace);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            Camera.position = new Vector3(0, 50, 0);

            new Thread(() =>
            {
                Thread.Sleep(Program.settings!.SaveInterfall);

                Console.WriteLine("Saving Chunks Soon(!)...");
                Thread.Sleep(3000);
                MainThread.Run(() =>
                {
                    Console.Clear();
                    Console.WriteLine("Saving Chunks... (This might take some time)");
                    Level.Save();
                });
            }).Start();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            MainThread.Run(() =>
            {
                Console.Clear();
                Console.WriteLine("Saving Chunks... (This might take some time)");

                Level.Save();

                Console.WriteLine("Saving Complete! See you next time!");
                Environment.Exit(-1);
            });
            base.OnClosing(e);
        }

        public static Action<double>? Update;

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            Camera.aspectRatio = e.Width / (float)e.Height;

            GL.Viewport(0, 0, e.Width, e.Height);
        }


        private Vector3 lastGenPoint = Vector3.Zero;
        private string info;
        public static float deltaTime;
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            if (args.Time < .1)
                deltaTime = (float)args.Time;
            Update?.Invoke(args.Time);

            if (Vector3.Distance(lastGenPoint, Camera.position) > 1)
            {
                lastGenPoint = Camera.position;
                Level.OnCameraMotion();
            }

            if (Keyboard.current.IsPressedThisFrame(Key.F6))
            {
                Level.UnloadAll();
                Level.UpdateWorld();
            }

            if (Keyboard.current.IsPressedThisFrame(Key.Escape))
            {
                if (CursorState == CursorState.Grabbed)
                {
                    CursorState = CursorState.Normal;
                    WindowState = WindowState.Normal;
                }
                else
                {
                    CursorState = CursorState.Grabbed;
                    WindowState = WindowState.Fullscreen;
                }
            }

            movement.Update(args.Time);

            MainThread.Update();

            foreach (Entity entity in entities)
            {
                entity.Tick();
            }

            // Call update here!
            base.OnUpdateFrame(args);

            tickProgress += Program.settings!.TicksPerSecond * args.Time;

            int ticksToRun = (int)tickProgress;
            tickProgress -= ticksToRun;

            for (int i = 0; i < ticksToRun; i++)
            {
                DoTick();
            }

        }

        private double tickProgress = 0;

        private void DoTick()
        {
            Vector3 randomAroundPlayer = Camera.position;

            randomAroundPlayer += new Vector3(rng.Next(-50, 50), rng.Next(-50, 50), rng.Next(-50, 50));
            SmartVoxel.DoRandomTick((Vector3i)randomAroundPlayer);

        }



        private Random rng = new Random();

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            Keyboard.current.SetKeyState((Input.Key)e.Key, true);
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            Keyboard.current.SetKeyState((Input.Key)e.Key, false);
            base.OnKeyUp(e);
        }


        private int frameCounter = 0;
        private double fpsTimeCounter;
        private int currentFPS;

        private int animationFrame = 0;
        private double animationTimer = 0;

        private static Vector3 fogColor = new Vector3(0.6f, 0.7f, 0.8f);
        private static float fogNear;
        private static float fogFar;

        private static float goal_FogNear;
        private static float goal_FogFar;
        private static Vector3 goal_FogColor;
        private static float fogSpeed = 10;

        public static void SetFog(Vector3 color, float near, float far, float fogSpeed)
        {
            goal_FogColor = color;
            goal_FogFar = far;
            RenderCanvas.fogSpeed = fogSpeed;
            goal_FogNear = near;
        }

        public static void SetDefaultFog()
        {
            SetFog(new Vector3(0.5f, 0.6f, 0.7f), 100, 110, 1);
        }

        private static float Lerp(float a, float b, float t)
        {
            t = Math.Clamp(t, 0, 1);

            return a + ((b - a) * t);
        }

        private int frame = 0;
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            frame++;
            fogFar = Lerp(fogFar, goal_FogFar, (float)args.Time * fogSpeed);
            fogNear = Lerp(fogNear, goal_FogNear, (float)args.Time * fogSpeed);
            fogColor = Vector3.Lerp(fogColor, goal_FogColor, (float)args.Time * fogSpeed);

            if (frame % 400 == 1)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Clear();

                Console.WriteLine(info);
                Console.WriteLine($"Frame Rate: {currentFPS}");

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Press Escape for Fullscreen!");

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
                Console.WriteLine("[ --- Controls --- ]");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("W-A-S-D to Move");
                Console.WriteLine("Mouse to Look");
                Console.WriteLine("Space to Jump");
                Console.WriteLine();
                Console.WriteLine("Scroll to Select");
                Console.WriteLine("Left Mouse to Destroy");
                Console.WriteLine("Right Mouse to Create");

                if (currentFPS < 60)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("LowFPS detected! Concider lowering the TicksPerSecond in the settings.");
                }
            }

            frameCounter++;
            fpsTimeCounter += args.Time;

            if (fpsTimeCounter >= 1.0)
            {
                currentFPS = frameCounter;
                frameCounter = 0;
                fpsTimeCounter -= 1.0;

            }


            if (animationTimer > 1)
            {
                animationFrame++;
                if (animationFrame == worldTexture.Length)
                    animationFrame = 0;

                animationTimer = 0;
            }
            animationTimer += args.Time;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            worldShader.Use();

            // Set camera matrix
            worldShader.SetMatrix4("u_View", Camera.GetViewMatrix());
            worldShader.SetMatrix4("u_Projection", Camera.GetProjectionMatrix());
            worldShader.SetVector3("u_CameraPos", Camera.position);

            worldShader.SetFloat("u_FogNear", fogNear);
            worldShader.SetFloat("u_FogFar", fogFar);
            worldShader.SetVector3("u_FogColor", fogColor);

            // Set the texture
            worldTexture[animationFrame].Use(TextureUnit.Texture0);
            worldShader.SetTextureId("u_Color", 0);

            lock (Level.Chunks)
            {
                foreach (Chunk chunk in Level.Chunks.Values)
                {
                    if (!chunk.isGenerated)
                        continue;

                    Mesh mesh = chunk.GetSolidMesh();

                    if (!mesh.hasData)
                        continue;
                    // Set model
                    worldShader.SetMatrix4("u_Model", mesh.GetModelMatrix());


                    // Render everything here!
                    mesh.Render();
                }
            }

            fluidShader.Use();

            // Set camera matrix
            time += (float)args.Time;
            fluidShader.SetFloat("u_Time", time);
            fluidShader.SetMatrix4("u_View", Camera.GetViewMatrix());
            fluidShader.SetMatrix4("u_Projection", Camera.GetProjectionMatrix());
            fluidShader.SetVector3("u_CameraPos", Camera.position);
            // Set the texture
            worldTexture[animationFrame].Use(TextureUnit.Texture0);
            fluidShader.SetTextureId("u_Color", 0);

            lock (Level.Chunks)
            {
                foreach (Chunk chunk in Level.Chunks.Values)
                {
                    if (!chunk.isGenerated)
                        continue;

                    Mesh mesh = chunk.GetWaterMesh();

                    if (!mesh.hasData)
                        continue;
                    // Set model
                    fluidShader.SetMatrix4("u_Model", mesh.GetModelMatrix());


                    // Render everything here!
                    mesh.Render();
                }
            }


            foreach (Entity entity in entities)
            {
                entity.mesh.shader.Use();

                entity.mesh.texture?.Use(TextureUnit.Texture0);
                entity.mesh.shader.SetMatrix4("u_Model", entity.mesh.GetModelMatrix());

                entity.mesh.shader.SetMatrix4("u_View", Camera.GetViewMatrix());
                entity.mesh.shader.SetMatrix4("u_Projection", Camera.GetProjectionMatrix());
                entity.mesh.shader.SetVector3("u_CameraPos", Camera.position);

                entity.mesh.Render();
            }

            foreach (Mesh mesh in meshes)
            {
                mesh.shader.Use();

                mesh.texture?.Use(TextureUnit.Texture0);
                mesh.shader.SetMatrix4("u_Model", mesh.GetModelMatrix());

                mesh.shader.SetMatrix4("u_View", Camera.GetViewMatrix());
                mesh.shader.SetMatrix4("u_Projection", Camera.GetProjectionMatrix());
                mesh.shader.SetVector3("u_CameraPos", Camera.position);

                mesh.Render();
            }

            SwapBuffers();
            Keyboard.current.EndOfFrame();
            Mouse.current.EndOfFrame();
        }

        public static List<Entity> entities = new List<Entity>();


        public static List<Mesh> meshes = new List<Mesh>();
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.Button == OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left)
                Mouse.current.leftPressed = true;

            if (e.Button == OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Right)
                Mouse.current.rightPressed = true;

            if (e.Button == OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Middle)
                Mouse.current.middlePressed = true;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.Button == OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left)
                Mouse.current.leftPressed = false;

            if (e.Button == OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Right)
                Mouse.current.rightPressed = false;

            if (e.Button == OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Middle)
                Mouse.current.middlePressed = false;
        }


        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            Mouse.current.mouseDelta = e.Delta;
            Mouse.current.position = e.Position;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            Mouse.current.scroll.X = e.OffsetX;
            Mouse.current.scroll.Y = e.OffsetY;
        }
    }
}
