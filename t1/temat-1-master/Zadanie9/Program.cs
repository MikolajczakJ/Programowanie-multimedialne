using OpenTK;
using OpenTK.Graphics.OpenGL4;
using GLFW;
using GlmSharp;

using Shaders;
using Models;

namespace PMLabs
{
    //Implementacja interfejsu dostosowującego metodę biblioteki Glfw służącą do pozyskiwania adresów funkcji i procedur OpenGL do współpracy z OpenTK.
    public class BC : IBindingsContext
    {
        public IntPtr GetProcAddress(string procName)
        {
            return Glfw.GetProcAddress(procName);
        }
    }

    class Program
    {
        static Torus[] toruses = new Torus[] {
        new Torus(), new Torus(), new Torus(),
        new Torus(), new Torus(), new Torus()
        };
        static Cube[] cubes = new Cube[] {
            new Cube(), new Cube(), new Cube(),
            new Cube(), new Cube(), new Cube(),
            new Cube(), new Cube(), new Cube()
        };

        //Można tak?
        static Cube[][] cubeMatrix = new Cube[][]
        {
        cubes,cubes,cubes,cubes,cubes,cubes
        };

        //static Cube[][] cubeMatrix = new Cube[][]
        //{
        //    new Cube[]
        //    {
        //        new Cube(), new Cube(), new Cube(),
        //        new Cube(), new Cube(), new Cube(),
        //        new Cube(), new Cube(), new Cube()
        //    },
        //    new Cube[]
        //    {
        //        new Cube(), new Cube(), new Cube(),
        //        new Cube(), new Cube(), new Cube(),
        //        new Cube(), new Cube(), new Cube()
        //    },
        //    new Cube[]
        //    {
        //        new Cube(), new Cube(), new Cube(),
        //        new Cube(), new Cube(), new Cube(),
        //        new Cube(), new Cube(), new Cube()
        //    },
        //                new Cube[]
        //    {
        //        new Cube(), new Cube(), new Cube(),
        //        new Cube(), new Cube(), new Cube(),
        //        new Cube(), new Cube(), new Cube()
        //    },
        //    new Cube[]
        //    {
        //        new Cube(), new Cube(), new Cube(),
        //        new Cube(), new Cube(), new Cube(),
        //        new Cube(), new Cube(), new Cube()
        //    },
        //    new Cube[]
        //    {
        //        new Cube(), new Cube(), new Cube(),
        //        new Cube(), new Cube(), new Cube(),
        //        new Cube(), new Cube(), new Cube()
        //    }

        //};
        static vec3[] TorusPositions = new vec3[]
        {

            new vec3(1.0f, 0.0f, 0.0f), //Lewy środkowy
            new vec3(0.52f, 0.95f, 0.0f), //Lewy górny
            new vec3(-0.52f, 0.95f, 0.0f), //Prawy górny
            new vec3(-1.0f, 0.0f, 0.0f), //Prawy środkowy
            new vec3(-0.52f, -0.95f, 0.0f), //Prawy dolny
            new vec3(0.52f, -0.95f, 0.0f), //Lewy dolny
        };
        public static void InitOpenGLProgram(Window window)
        {
            // Czyszczenie okna na kolor czarny
            GL.ClearColor(0, 0, 0, 1);

            // Ładowanie programów cieniujących
            DemoShaders.InitShaders("Shaders\\");
        }

        public static void DrawScene(Window window, float time)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            mat4 V = mat4.LookAt(
                new vec3(0.0f, 0.0f, -5.0f),
                new vec3(0.0f, 0.0f, 0.0f),
                new vec3(0.0f, 1.0f, 0.0f));
            mat4 P = mat4.Perspective(glm.Radians(50.0f), 1.0f, 1.0f, 50.0f);

            DemoShaders.spConstant.Use();
            GL.UniformMatrix4(DemoShaders.spConstant.U("P"), 1, false, P.Values1D);
            GL.UniformMatrix4(DemoShaders.spConstant.U("V"), 1, false, V.Values1D);

            mat4 M = mat4.Identity;
            GL.UniformMatrix4(DemoShaders.spConstant.U("M"), 1, false, M.Values1D);

            // TU RYSUJEMY
            bool torusSpin = false;
            bool fixCogWheel = false;
            for (int i = 0; i < toruses.Length; i++)
            {
                mat4 torusM = mat4.Identity;
                torusM *= mat4.Translate(TorusPositions[i]);
                torusM *= mat4.Scale(0.5f);

                torusM *= torusSpin ?
                    mat4.Rotate(glm.Radians(30.0f * time), new vec3(0.0f, 0.0f, 1.0f)) :
                    mat4.Rotate(glm.Radians(30.0f * time), new vec3(0.0f, 0.0f, -1.0f));
                torusSpin = !torusSpin;

                GL.UniformMatrix4(DemoShaders.spConstant.U("M"), 1, false, torusM.Values1D);
                toruses[i].drawWire();
                for (int j = 0; j < cubeMatrix[i].Length; j++)
                {
                    mat4 cubeM = torusM;
                    cubeM *= fixCogWheel ?
                        mat4.Rotate((30 * j), new vec3(0.0f, 0.0f, 1.0f)) :
                        mat4.Rotate((30 * j) + 10, new vec3(0.0f, 0.0f, 1.0f));

                    cubeM *= mat4.Translate(new vec3(1.0f, 0.0f, 0.0f));
                    cubeM *= mat4.Scale(0.13f);
                    GL.UniformMatrix4(DemoShaders.spConstant.U("M"), 1, false, cubeM.Values1D);
                    cubeMatrix[i][j].drawWire();
                }
                fixCogWheel = !fixCogWheel;

            }

            Glfw.SwapBuffers(window);
        }

        public static void FreeOpenGLProgram(Window window)
        {
            // Możesz dodać odpowiednie czyszczenie zasobów tutaj, jeśli jest to konieczne
        }

        static void Main(string[] args)
        {
            Glfw.Init();

            Window window = Glfw.CreateWindow(1500, 1500, "Programowanie multimedialne", GLFW.Monitor.None, Window.None);

            Glfw.MakeContextCurrent(window);
            Glfw.SwapInterval(1);

            GL.LoadBindings(new BC()); //Pozyskaj adresy implementacji poszczególnych procedur OpenGL

            InitOpenGLProgram(window);

            Glfw.Time = 0;

            while (!Glfw.WindowShouldClose(window))
            {
                DrawScene(window, (float)Glfw.Time);
                Glfw.PollEvents();
            }


            FreeOpenGLProgram(window);

            Glfw.Terminate();
        }


    }
}