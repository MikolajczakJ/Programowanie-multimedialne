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
        static Torus t1 = new Torus();
        static Torus t2 = new Torus();
        static Cube[] cubes = new Cube[]
        {
           //new Cube(),new Cube(),new Cube(),new Cube(),new Cube(),
           //new Cube(),new Cube(),new Cube(),new Cube(),
           new Cube(),new Cube(),new Cube(),new Cube(),new Cube(),
           new Cube(),new Cube(),new Cube(),new Cube()
        };
        static Cube[] cubes2 = new Cube[]
{
           new Cube(),new Cube(),new Cube(),new Cube(),new Cube(),
           new Cube(),new Cube(),new Cube(),new Cube()
            
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
            mat4 torusM1 = mat4.Identity;
            torusM1 *= mat4.Translate(new vec3(-1.05f, 0.0f, 0.0f));
            torusM1 *= mat4.Rotate(glm.Radians(30.0f * time), new vec3(0.0f, 0.0f, 1.0f));
            GL.UniformMatrix4(DemoShaders.spConstant.U("M"), 1, false, torusM1.Values1D);
            t1.drawWire();
            for (int i = 0; i < cubes.Length; i++)
            {
                mat4 cubeM = torusM1;
                cubeM *= mat4.Rotate(30*i,new vec3(0.0f,0.0f,1.0f));
                cubeM *= mat4.Translate(new vec3(1.0f, 0.0f, 0.0f));
                cubeM *= mat4.Scale(0.13f);
                GL.UniformMatrix4(DemoShaders.spConstant.U("M"), 1, false, cubeM.Values1D);
                cubes[i].drawWire();
            }


            mat4 torusM2 = mat4.Identity;
            torusM2 *= mat4.Translate(new vec3(1.07f, 0.0f, 0.0f));
            torusM2 *= mat4.Rotate(glm.Radians(-30.0f * time), new vec3(0.0f, 0.0f, 1.0f));
            GL.UniformMatrix4(DemoShaders.spConstant.U("M"), 1, false, torusM2.Values1D);
            t2.drawWire();
            for (int i = 0; i < cubes2.Length; i++)
            {
                mat4 cubeM = torusM2;
                cubeM *= mat4.Rotate((30 * i)+5, new vec3(0.0f, 0.0f, 1.0f));
                cubeM *= mat4.Translate(new vec3(1.0f, 0.0f, 0.0f));
                cubeM *= mat4.Scale(0.13f);
                GL.UniformMatrix4(DemoShaders.spConstant.U("M"), 1, false, cubeM.Values1D);
                cubes2[i].drawWire();
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