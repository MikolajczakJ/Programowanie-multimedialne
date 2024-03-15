using GLFW; //Przestrzeń nazw biblioteki GLFW.NET. Zawiera ona bindingi do biblioteki GLFW zapewniającej możliwość tworzenia aplikacji wykorzystujacych OpenGL
using GlmSharp; //Przestrzeń nazw biblioteki GlmSharp. GlmSharp to port biblioteki GLM - OpenGL Mathematics implementującej podstawowe operacje matematyczne wykorzystywane w grafice 3D.

using Shaders; //Przestrzeń nazw pomocniczej biblioteki do wczytywania programów cieniującch

using System;
using System.IO;

using OpenTK;
using OpenTK.Graphics.OpenGL4;

using System.Drawing;

using Models;

namespace PMLabs
{


    //Implementacja interfejsu dostosowującego metodę biblioteki Glfw służącą do pozyskiwania adresów funkcji i procedur OpenGL do współpracy z OpenTK.
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
        static float move_z; 

        static Sphere bubble = new Sphere(0.08f,12,12);
        static KeyCallback kc = KeyProcessor;

        //Obsługa klawiatury - zmiana prędkości obrotu wokół poszczególnych osi w zależności od wciśniętych klawiszy
        public static void KeyProcessor(System.IntPtr window, Keys key, int scanCode, InputState state, ModifierKeys mods) { 
            if (state==InputState.Press)
            {
                if (key == Keys.Left) speed_y = -3.14f;
                if (key == Keys.Right) speed_y =  3.14f;
                if (key == Keys.Up) speed_x = -3.14f;
                if (key == Keys.Down) speed_x = 3.14f;

                if (key == Keys.A) move_x += -3.14f;
                if (key == Keys.D) move_x += 3.14f;
                if (key == Keys.S) move_z += 3.14f;
                if (key == Keys.W) move_z += -3.14f;
            }
            if (state == InputState.Release)
            {
                if (key == Keys.Left) speed_y = 0;
                if (key == Keys.Right) speed_y = 0;
                if (key == Keys.Up) speed_x = 0;
                if (key == Keys.Down) speed_x = 0;

                //if (key == Keys.A) move_x = 0;
                //if (key == Keys.D) move_x = 0;
                //if (key == Keys.S) move_y = 0;
                //if (key == Keys.W) move_y = 0;
            }
        }

        //Metoda wykonywana po zainicjowaniu bibliotek, przed rozpoczęciem pętli głównej
        //Tutaj umieszczamy nasz kod inicjujący
        public static void InitOpenGLProgram(Window window)
        {
            GL.ClearColor(0, 0, 0, 1); //Wyczyść zawartość okna na czarno (r=0,g=0,b=0,a=1)
            GL.Enable(EnableCap.DepthTest);
            DemoShaders.InitShaders("Pomoce/");
            Glfw.SetKeyCallback(window, kc); //Zarejestruj metodę obsługi klawiatury
        }

        //Metoda wykonywana po zakończeniu pętli główej, przed zwolnieniem zasobów bibliotek
        //Tutaj zwalniamy wszystkie zasoby zaalokowane na począdku programu
        public static void FreeOpenGLProgram(Window window)
        {
            
        }

        //Metoda wykonywana najczęściej jak się da. Umieszczamy tutaj kod rysujący
        public static void DrawScene(Window window,float angle_x,float angle_y, float time, float loc_x, float loc_y)
        {
            // Wyczyść zawartość okna (buforów kolorów i głębokości)
            GL.Clear(ClearBufferMask.ColorBufferBit| ClearBufferMask.DepthBufferBit);

            mat4 P = mat4.Perspective(glm.Radians(50.0f), 1, 1, 50); //Wylicz macierz rzutowania
            mat4 V = mat4.LookAt(new vec3(0, 0, -5), new vec3(0, 0, 0), new vec3(0, 1, 0)); //Wylicz macierz widoku
            DemoShaders.spColored.Use();

            GL.UniformMatrix4(DemoShaders.spColored.U("P"), 1, false, P.Values1D); //Wyślij do zmiennej jednorodnej P programu cieniującego wartość zmiennej P zadeklarowanej powyżej
            GL.UniformMatrix4(DemoShaders.spColored.U("V"), 1, false, V.Values1D); //Wyślij do zmiennej jednorodnej V programu cieniującego wartość zmiennej V zadeklarowanej powyżej


            // BLAT stołu
            mat4 M = mat4.Rotate(angle_y, new vec3(0, 1, 0)) * mat4.Rotate(angle_x,new vec3 (1,0,0));
            M *=mat4.Translate(move_x, 0, move_z);
            M *= mat4.Translate(new vec3(0.0f, -0.5f, 0.0f));
            M *= mat4.Scale(new vec3(1.5f, 0.15f, 1.5f));
            GL.UniformMatrix4(DemoShaders.spConstant.U("M"), 1, false, M.Values1D);
            GL.UniformMatrix4(DemoShaders.spColored.U("M"), 1, false, M.Values1D); //Wyślij do zmiennej jednorodnej M programu cieniującego wartość zmiennej M zadeklarowanej powyżej

            GL.EnableVertexAttribArray(DemoShaders.spColored.A("vertex"));
            GL.EnableVertexAttribArray(DemoShaders.spColored.A("color"));
            GL.VertexAttribPointer(DemoShaders.spColored.A("vertex"), 4, VertexAttribPointerType.Float, false, 0, MyCube.vertices);
            GL.VertexAttribPointer(DemoShaders.spColored.A("color"), 4, VertexAttribPointerType.Float, false, 0, MyCube.colors);
            GL.DrawArrays(PrimitiveType.Triangles, 0, MyCube.vertexCount);
            GL.DisableVertexAttribArray(DemoShaders.spColored.A("vertex"));
            GL.DisableVertexAttribArray(DemoShaders.spColored.A("color"));


            //Noga 1
            mat4 legM1 = mat4.Rotate(angle_y, new vec3(0, 1, 0)) * mat4.Rotate(angle_x, new vec3(1, 0, 0)); //Macierz modelu to iloczyun macierzy obrotu wokół osi Y i X.
            legM1 *= mat4.Translate(move_x, 0, move_z);
            legM1 *= mat4.Translate(new vec3(1.25f, -1.75f, 1.25f));
            legM1 *= mat4.Scale(new vec3(0.15f, 1.25f, 0.15f));
            GL.UniformMatrix4(DemoShaders.spConstant.U("M"), 1, false, legM1.Values1D);
            GL.UniformMatrix4(DemoShaders.spColored.U("M"), 1, false, legM1.Values1D); //Wyślij do zmiennej jednorodnej M programu cieniującego wartość zmiennej M zadeklarowanej powyżej

            GL.EnableVertexAttribArray(DemoShaders.spColored.A("vertex"));
            GL.EnableVertexAttribArray(DemoShaders.spColored.A("color"));
            GL.VertexAttribPointer(DemoShaders.spColored.A("vertex"), 4, VertexAttribPointerType.Float, false, 0, MyCube.vertices);
            GL.VertexAttribPointer(DemoShaders.spColored.A("color"), 4, VertexAttribPointerType.Float, false, 0, MyCube.colors);
            GL.DrawArrays(PrimitiveType.Triangles, 0, MyCube.vertexCount);
            GL.DisableVertexAttribArray(DemoShaders.spColored.A("vertex"));
            GL.DisableVertexAttribArray(DemoShaders.spColored.A("color"));
            
            //Noga 2
            mat4 legM2 = mat4.Rotate(angle_y, new vec3(0, 1, 0)) * mat4.Rotate(angle_x, new vec3(1, 0, 0)); //Macierz modelu to iloczyun macierzy obrotu wokół osi Y i X.
            legM2 *= mat4.Translate(move_x, 0, move_z);
            legM2 *= mat4.Translate(new vec3(1.25f, -1.75f, -1.25f));
            legM2 *= mat4.Scale(new vec3(0.15f, 1.25f, 0.15f));
            GL.UniformMatrix4(DemoShaders.spConstant.U("M"), 1, false, legM2.Values1D);
            GL.UniformMatrix4(DemoShaders.spColored.U("M"), 1, false, legM2.Values1D); //Wyślij do zmiennej jednorodnej M programu cieniującego wartość zmiennej M zadeklarowanej powyżej

            GL.EnableVertexAttribArray(DemoShaders.spColored.A("vertex"));
            GL.EnableVertexAttribArray(DemoShaders.spColored.A("color"));
            GL.VertexAttribPointer(DemoShaders.spColored.A("vertex"), 4, VertexAttribPointerType.Float, false, 0, MyCube.vertices);
            GL.VertexAttribPointer(DemoShaders.spColored.A("color"), 4, VertexAttribPointerType.Float, false, 0, MyCube.colors);
            GL.DrawArrays(PrimitiveType.Triangles, 0, MyCube.vertexCount);
            GL.DisableVertexAttribArray(DemoShaders.spColored.A("vertex"));
            GL.DisableVertexAttribArray(DemoShaders.spColored.A("color"));


            //noga 3

            mat4 legM3 = mat4.Rotate(angle_y, new vec3(0, 1, 0)) * mat4.Rotate(angle_x, new vec3(1, 0, 0)); //Macierz modelu to iloczyun macierzy obrotu wokół osi Y i X.
            legM3 *= mat4.Translate(move_x, 0, move_z);
            legM3 *= mat4.Translate(new vec3(-1.25f, -1.75f, -1.25f));
            legM3 *= mat4.Scale(new vec3(0.15f, 1.25f, 0.15f));
            GL.UniformMatrix4(DemoShaders.spConstant.U("M"), 1, false, legM3.Values1D);
            GL.UniformMatrix4(DemoShaders.spColored.U("M"), 1, false, legM3.Values1D); //Wyślij do zmiennej jednorodnej M programu cieniującego wartość zmiennej M zadeklarowanej powyżej

            GL.EnableVertexAttribArray(DemoShaders.spColored.A("vertex"));
            GL.EnableVertexAttribArray(DemoShaders.spColored.A("color"));
            GL.VertexAttribPointer(DemoShaders.spColored.A("vertex"), 4, VertexAttribPointerType.Float, false, 0, MyCube.vertices);
            GL.VertexAttribPointer(DemoShaders.spColored.A("color"), 4, VertexAttribPointerType.Float, false, 0, MyCube.colors);
            GL.DrawArrays(PrimitiveType.Triangles, 0, MyCube.vertexCount);
            GL.DisableVertexAttribArray(DemoShaders.spColored.A("vertex"));
            GL.DisableVertexAttribArray(DemoShaders.spColored.A("color"));


            //noga 4

            mat4 legM4 = mat4.Rotate(angle_y, new vec3(0, 1, 0)) * mat4.Rotate(angle_x, new vec3(1, 0, 0)); //Macierz modelu to iloczyun macierzy obrotu wokół osi Y i X.
            legM4 *= mat4.Translate(move_x, 0, move_z);
            legM4 *= mat4.Translate(new vec3(-1.25f, -1.75f, 1.25f));
            legM4 *= mat4.Scale(new vec3(0.15f, 1.25f, 0.15f));
            GL.UniformMatrix4(DemoShaders.spConstant.U("M"), 1, false, legM4.Values1D);
            GL.UniformMatrix4(DemoShaders.spColored.U("M"), 1, false, legM4.Values1D); //Wyślij do zmiennej jednorodnej M programu cieniującego wartość zmiennej M zadeklarowanej powyżej

            GL.EnableVertexAttribArray(DemoShaders.spColored.A("vertex"));
            GL.EnableVertexAttribArray(DemoShaders.spColored.A("color"));
            GL.VertexAttribPointer(DemoShaders.spColored.A("vertex"), 4, VertexAttribPointerType.Float, false, 0, MyCube.vertices);
            GL.VertexAttribPointer(DemoShaders.spColored.A("color"), 4, VertexAttribPointerType.Float, false, 0, MyCube.colors);
            GL.DrawArrays(PrimitiveType.Triangles, 0, MyCube.vertexCount);
            GL.DisableVertexAttribArray(DemoShaders.spColored.A("vertex"));
            GL.DisableVertexAttribArray(DemoShaders.spColored.A("color"));

            // Czajnik
            mat4 teapotM = mat4.Rotate(angle_y, new vec3(0, 1, 0)) * mat4.Rotate(angle_x, new vec3(1, 0, 0));
            teapotM *= mat4.Translate(move_x, 0, move_z);
            teapotM *= mat4.Translate(new vec3(0.0f, 0.035f, 0.0f));
            GL.UniformMatrix4(DemoShaders.spConstant.U("M"), 1, false, teapotM.Values1D);
            GL.UniformMatrix4(DemoShaders.spColored.U("M"), 1, false, teapotM.Values1D);

            GL.EnableVertexAttribArray(DemoShaders.spColored.A("vertex"));
            GL.EnableVertexAttribArray(DemoShaders.spColored.A("color"));
            GL.VertexAttribPointer(DemoShaders.spColored.A("vertex"), 4, VertexAttribPointerType.Float, false, 0, MyTeapot.vertices);
            GL.VertexAttribPointer(DemoShaders.spColored.A("color"), 4, VertexAttribPointerType.Float, false, 0, MyTeapot.colors);
            GL.DrawArrays(PrimitiveType.Triangles, 0, MyTeapot.vertexCount);
            GL.DisableVertexAttribArray(DemoShaders.spColored.A("vertex"));
            GL.DisableVertexAttribArray(DemoShaders.spColored.A("color"));




            mat4 bubbleM = mat4.Rotate(angle_y, new vec3(0, 1, 0)) * mat4.Rotate(angle_x, new vec3(1, 0, 0));
            bubbleM *= mat4.Translate(new vec3(0.85f, FloatingBubble(time), 0.0f));
            GL.UniformMatrix4(DemoShaders.spConstant.U("M"), 1, false, bubbleM.Values1D);

            bubble.colors = MyTeapot.colors;
            bubble.drawSolid();

            //Skopiuj ukryty bufor do bufora widocznego            
            Glfw.SwapBuffers(window);
        }

        public static float FloatingBubble(float time)
        {
            float temp = time % 4;

            if (temp > 1) { return temp * 0.35f; }
            else /*if (temp == 0) */{ return -100f; }


        }


        //Metoda główna
        static void Main(string[] args)
        {
            Glfw.Init();//Zainicjuj bibliotekę GLFW

            Window window = Glfw.CreateWindow(1500, 1500, "OpenGL", GLFW.Monitor.None, Window.None); //Utwórz okno o wymiarach 500x500 i tytule "OpenGL"

            Glfw.MakeContextCurrent(window); //Ustaw okno jako aktualny kontekst OpenGL - tutaj będą realizowane polecenia OpenGL
            Glfw.SwapInterval(1); //Skopiowanie tylnego bufora na przedni ma się rozpocząć po zakończeniu aktualnego odświerzania ekranu

            GL.LoadBindings(new BC()); //Pozyskaj adresy implementacji poszczególnych procedur OpenGL

            InitOpenGLProgram(window); //Wykonaj metodę inicjującą Twoje zasoby 

            float angle_x = 0; //Aktualny kąt obrotu wokół osi X
            float angle_y = 0; //Aktualny kąt obrotu wokół osi Y

            Glfw.Time = 0; //Wyzeruj licznik czasu

            while (!Glfw.WindowShouldClose(window)) //Wykonuj tak długo, dopóki użytkownik nie zamknie okna
            {
                angle_x += speed_x * 40;// (float)Glfw.Time; //Aktualizuj kat obrotu wokół osi X zgodnie z prędkością obrotu
                angle_y += speed_y * 40;//(float)Glfw.Time; //Aktualizuj kat obrotu wokół osi Y zgodnie z prędkością obrotu

                //Glfw.Time = 0; //Wyzeruj licznik czasu
                DrawScene(window,angle_x,angle_y, (float)Glfw.Time,move_x, move_z); //Wykonaj metodę odświeżającą zawartość okna

                Glfw.PollEvents(); //Obsłuż zdarzenia użytkownika
            }


            FreeOpenGLProgram(window);//Zwolnij zaalokowane przez siebie zasoby

            Glfw.Terminate(); //Zwolnij zasoby biblioteki GLFW
        }
                    

    }
}