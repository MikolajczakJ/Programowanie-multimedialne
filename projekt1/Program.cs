using GLFW; //Przestrzeń nazw biblioteki GLFW.NET. Zawiera ona bindingi do biblioteki GLFW zapewniającej możliwość tworzenia aplikacji wykorzystujacych OpenGL
using GlmSharp; //Przestrzeń nazw biblioteki GlmSharp. GlmSharp to port biblioteki GLM - OpenGL Mathematics implementującej podstawowe operacje matematyczne wykorzystywane w grafice 3D.

using Shaders; //Przestrzeń nazw pomocniczej biblioteki do wczytywania programów cieniującch

using System;
using System.IO;

using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;

using Models;
using OpenTK.Mathematics;
using System.Text;
using System.Threading;

namespace PMLabs
{
    /*Do zrobienia:
     * https://multimedialne.placzek.tk/tryb-wyzwania/misja-1
     * Co już mamy:
     */

    public class BC: IBindingsContext
    {
        public IntPtr GetProcAddress(string procName)
        {
            return Glfw.GetProcAddress(procName);
        }
    }

    class Program
    {
        static float speed_y; //Prędkość obrotu wokół osi Y [rad/s]
        static float speed_x; //Prędkość obrotu wokół osi X [rad/s]
        static float move_x; 
        static float move_y;
        static float move_z;
        static vec3 legScaleVec = new vec3(0.15f, 1.25f, 0.15f);
        static float bubbleSpeed = 0.5f;
        static Sphere bubble = new Sphere(0.08f,12,12);
        static KeyCallback kc = KeyProcessor;
        //Obsługa klawiatury - zmiana prędkości obrotu wokół poszczególnych osi w zależności od wciśniętych klawiszy
        public static void KeyProcessor(System.IntPtr window, Keys key, int scanCode, InputState state, ModifierKeys mods) { 
            if (state==InputState.Press)
            {
                if (key == Keys.Left) speed_y = 3.14f;
                if (key == Keys.Right) speed_y = -3.14f;
                if (key == Keys.Up) speed_x = 3.14f;
                if (key == Keys.Down) speed_x = -3.14f;


                if (key == Keys.Alpha1) bubbleSpeed = 0.5f;
                if (key == Keys.Alpha2) bubbleSpeed = 0.75f;
                if (key == Keys.Alpha3) bubbleSpeed = 1.0f;
                if (key == Keys.Alpha4) bubbleSpeed = 1.25f;
                if (key == Keys.Alpha5) bubbleSpeed = 1.5f;
                if (key == Keys.Alpha6) bubbleSpeed = 1.75f;
                if (key == Keys.Alpha7) bubbleSpeed = 2.0f;
                if (key == Keys.Alpha8) bubbleSpeed = 2.25f;
                if (key == Keys.Alpha9) bubbleSpeed = 2.5f;


                if (key == Keys.Z) move_y += -3.14f;
                if (key == Keys.X) move_y += 3.14f;
                if (key == Keys.A) move_x += 3.14f;
                if (key == Keys.D) move_x += -3.14f;
                if (key == Keys.S) move_z += -3.14f;
                if (key == Keys.W) move_z += 3.14f;
            }
            if (state == InputState.Release)
            {
                if (key == Keys.Left) speed_y = 0;
                if (key == Keys.Right) speed_y = 0;
                if (key == Keys.Up) speed_x = 0;
                if (key == Keys.Down) speed_x = 0;

                //if (key == Keys.A) move_x = 0;
                //if (key == Keys.D) move_x = 0;
                //if (key == Keys.S) move_z = 0;
                //if (key == Keys.W) move_z = 0;
            }
        }
        public static void InitOpenGLProgram(Window window)
        {
            GL.ClearColor(0, 0, 0, 1); //Wyczyść zawartość okna na czarno (r=0,g=0,b=0,a=1)
            GL.Enable(EnableCap.DepthTest);
            DemoShaders.InitShaders("Pomoce/");
            Glfw.SetKeyCallback(window, kc); //Zarejestruj metodę obsługi klawiatury
        }
        public static void FreeOpenGLProgram(Window window)
        {
        }
        public static void DrawScene(Window window,float angle_x,float angle_y, float time, float bubbleSpeed)
        {
            // Wyczyść zawartość okna (buforów kolorów i głębokości)
            GL.Clear(ClearBufferMask.ColorBufferBit| ClearBufferMask.DepthBufferBit);

            mat4 P = mat4.Perspective(glm.Radians(50.0f), 1, 1, 50); //Wylicz macierz rzutowania

            //Tutaj trzeba będzie pozmieniać żeby przemieszczanie aktualizowało się po zmianie perspektywy
            mat4 V = mat4.LookAt(new vec3(0, 0, -5), new vec3(0,0,0), new vec3(0, 1, 0)); //Wylicz macierz widoku
            DemoShaders.spColored.Use();
            P *= mat4.Rotate(angle_y, new vec3(0, 1, 0)) * mat4.Rotate(angle_x, new vec3(1, 0, 0));
            P *= mat4.Translate(move_x, move_y, move_z);
            GL.UniformMatrix4(DemoShaders.spColored.U("P"), 1, false, P.Values1D);
            GL.UniformMatrix4(DemoShaders.spColored.U("V"), 1, false, V.Values1D);


            // BLAT stołu
            RenderTable(new vec3(0.0f, -0.5f, 0.0f), new vec3(1.5f, 0.15f, 1.5f));
            //Nogi
            RenderTable(new vec3(1.25f, -1.75f, 1.25f), legScaleVec);
            RenderTable(new vec3(1.25f, -1.75f, -1.25f), legScaleVec);
            RenderTable(new vec3(-1.25f, -1.75f, -1.25f), legScaleVec);
            RenderTable(new vec3(-1.25f, -1.75f, 1.25f), legScaleVec);

            RenderTeapot();
            RenderBubble(time, bubbleSpeed);

            Glfw.SwapBuffers(window);
        }
        private static void RenderBubble(float time, float bubbleSpeed)
        {
            float size, speed;
            (size, speed) = FloatingBubble(time,bubbleSpeed);
            mat4 bubbleM = mat4.Rotate(0, new vec3(0, 1, 0)) * mat4.Rotate(0, new vec3(1, 0, 0));
            //mat4 bubbleM = mat4.Rotate(angle_y, new vec3(0, 1, 0)) * mat4.Rotate(angle_x, new vec3(1, 0, 0));
            bubbleM *= mat4.Translate(new vec3(0.85f, speed, 0.0f));
            //bubbleM *= mat4.Translate(move_x, 0, move_z);
            bubbleM *= mat4.Translate(new vec3(0.0f, 0.035f, 0.0f));
            bubbleM *= mat4.Scale(size);
            GL.UniformMatrix4(DemoShaders.spConstant.U("M"), 1, false, bubbleM.Values1D);
            bubble.colors = MyTeapot.colors;
            bubble.drawSolid();
        }
        private static void RenderTeapot()
        {
            mat4 teapotM = mat4.Rotate(0, new vec3(0, 1, 0)) * mat4.Rotate(0, new vec3(1, 0, 0));
            //teapotM *= mat4.Translate(move_x, 0, move_z);
            teapotM *= mat4.Translate(new vec3(0.0f, 0.035f, 0.0f));
            GL.UniformMatrix4(DemoShaders.spColored.U("M"), 1, false, teapotM.Values1D);
            GL.UniformMatrix4(DemoShaders.spColored.U("M"), 1, false, teapotM.Values1D);

            GL.EnableVertexAttribArray(DemoShaders.spColored.A("vertex"));
            GL.EnableVertexAttribArray(DemoShaders.spColored.A("color"));
            GL.VertexAttribPointer(DemoShaders.spColored.A("vertex"), 4, VertexAttribPointerType.Float, false, 0, MyTeapot.vertices);
            GL.VertexAttribPointer(DemoShaders.spColored.A("color"), 4, VertexAttribPointerType.Float, false, 0, MyTeapot.colors);
            GL.DrawArrays(PrimitiveType.Triangles, 0, MyTeapot.vertexCount);
            GL.DisableVertexAttribArray(DemoShaders.spColored.A("vertex"));
            GL.DisableVertexAttribArray(DemoShaders.spColored.A("color"));
        }
        private static void RenderTable(vec3 translateVec,vec3 scaleVec)
        {
            mat4 legM3 = mat4.Rotate(0, new vec3(0, 1, 0)) * mat4.Rotate(0, new vec3(1, 0, 0)); //Macierz modelu to iloczyun macierzy obrotu wokół osi Y i X.
            //mat4 legM3 = mat4.Rotate(angle_y, new vec3(0, 1, 0)) * mat4.Rotate(angle_x, new vec3(1, 0, 0)); //Macierz modelu to iloczyun macierzy obrotu wokół osi Y i X.
            //legM3 *= mat4.Translate(move_x, 0, move_z);
            legM3 *= mat4.Translate(translateVec);
            legM3 *= mat4.Scale(scaleVec);
            GL.UniformMatrix4(DemoShaders.spConstant.U("M"), 1, false, legM3.Values1D);
            GL.UniformMatrix4(DemoShaders.spColored.U("M"), 1, false, legM3.Values1D); //Wyślij do zmiennej jednorodnej M programu cieniującego wartość zmiennej M zadeklarowanej powyżej

            GL.EnableVertexAttribArray(DemoShaders.spColored.A("vertex"));
            GL.EnableVertexAttribArray(DemoShaders.spColored.A("color"));
            GL.VertexAttribPointer(DemoShaders.spColored.A("vertex"), 4, VertexAttribPointerType.Float, false, 0, MyCube.vertices);
            GL.VertexAttribPointer(DemoShaders.spColored.A("color"), 4, VertexAttribPointerType.Float, false, 0, MyCube.colors);
            GL.DrawArrays(PrimitiveType.Triangles, 0, MyCube.vertexCount);
            GL.DisableVertexAttribArray(DemoShaders.spColored.A("vertex"));
            GL.DisableVertexAttribArray(DemoShaders.spColored.A("color"));
        }
        public static (float,float) FloatingBubble(float time, float bubbleSpeed)
        {
            float temp = (time) % 10;
            if (temp <= (2))
            {
                return (1.0f, -100f);
            }
            else if (temp > (2) && temp < (5))
            {

                return (1 + ((temp % 5) * 0.1f), 0.38f);
            }
            else
            {
                return (1.9f, (temp % 5 * 0.38f) + 0.38f);
            }
            //float temp = (time * bubbleSpeed) % 10;
            //if (temp <= (2 * bubbleSpeed))
            //{
            //    return (1.0f, -100f);
            //}
            //else if (temp > (2 * bubbleSpeed) && temp < (5 * bubbleSpeed))
            //{

            //    return (1 + ((temp % 5) * 0.1f), 0.38f);
            //}
            //else
            //{
            //    return (1.9f, ((temp%5) * 0.38f) + 0.38f);
            //}

        }
        static void Main(string[] args)
        {
            Glfw.Init();

            Window window = Glfw.CreateWindow(500, 500, "Projekt czajnik", GLFW.Monitor.None, Window.None);

            Glfw.MakeContextCurrent(window);
            Glfw.SwapInterval(1); 

            GL.LoadBindings(new BC());

            InitOpenGLProgram(window);

            float angle_x = 0; //Aktualny kąt obrotu wokół osi X
            float angle_y = 0; //Aktualny kąt obrotu wokół osi Y

            Glfw.Time = 0; //Wyzeruj licznik czasu

            while (!Glfw.WindowShouldClose(window)) //Wykonuj tak długo, dopóki użytkownik nie zamknie okna
            {
                angle_x += speed_x * 40;//Aktualizuj kat obrotu wokół osi X zgodnie z prędkością obrotu
                angle_y += speed_y * 40;//Aktualizuj kat obrotu wokół osi Y zgodnie z prędkością obrotu

                //Glfw.Time = 0; //Wyzeruj licznik czasu
                DrawScene(window,angle_x,angle_y, (float)Glfw.Time,bubbleSpeed);
                Glfw.PollEvents();
            }
            FreeOpenGLProgram(window);
            Glfw.Terminate();
        }
    }
}