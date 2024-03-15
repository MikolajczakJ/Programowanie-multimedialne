﻿using GLFW;
using GlmSharp;

using Shaders;

using OpenTK;
using OpenTK.Graphics.OpenGL4;

using Models;
using System.Drawing;

namespace PMLabs
{


    public class BC : IBindingsContext
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
        static int tex;

        static KeyCallback kc = KeyProcessor;

        public static int ReadTexture(string filename)
        {
            var tex = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, tex);
            Bitmap bitmap = new Bitmap(filename);
            System.Drawing.Imaging.BitmapData data = bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width,
                data.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bitmap.UnlockBits(data);
            bitmap.Dispose();
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            return tex;
        }



        //Obsługa klawiatury - zmiana prędkości obrotu wokół poszczególnych osi w zależności od wciśniętych klawiszy
        public static void KeyProcessor(System.IntPtr window, Keys key, int scanCode, InputState state, ModifierKeys mods)
        {
            if (state == InputState.Press)
            {
                if (key == Keys.Left) speed_y = -3.14f;
                if (key == Keys.Right) speed_y = 3.14f;
                if (key == Keys.Up) speed_x = -3.14f;
                if (key == Keys.Down) speed_x = 3.14f;
            }
            if (state == InputState.Release)
            {
                if (key == Keys.Left) speed_y = 0;
                if (key == Keys.Right) speed_y = 0;
                if (key == Keys.Up) speed_x = 0;
                if (key == Keys.Down) speed_x = 0;
            }
        }

        public static void InitOpenGLProgram(Window window)
        {
            GL.ClearColor(0, 0, 0, 1);
            GL.Enable(EnableCap.DepthTest);
            tex = ReadTexture("bricks.png");
            DemoShaders.InitShaders("Shaders/");
            Glfw.SetKeyCallback(window, kc); //Zarejestruj metodę obsługi klawiatury
        }

        public static void FreeOpenGLProgram(Window window)
        {
            GL.DeleteTexture(tex);
        }

        public static void DrawScene(Window window, float angle_x, float angle_y)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            mat4 P = mat4.Perspective(glm.Radians(50.0f), 1, 1, 50);
            mat4 V = mat4.LookAt(new vec3(0, 0, -5), new vec3(0, 0, 0), new vec3(0, 1, 0));

            DemoShaders.spLambertTextured.Use();

            GL.UniformMatrix4(DemoShaders.spLambertTextured.U("P"), 1, false, P.Values1D);
            GL.UniformMatrix4(DemoShaders.spLambertTextured.U("V"), 1, false, V.Values1D);

            mat4 M = mat4.Rotate(angle_y, new vec3(0, 1, 0)) * mat4.Rotate(angle_x, new vec3(1, 0, 0));
            GL.UniformMatrix4(DemoShaders.spLambertTextured.U("M"), 1, false, M.Values1D);

            GL.Uniform1(DemoShaders.spLambertTextured.U("tex"), 0);

            GL.EnableVertexAttribArray(DemoShaders.spLambertTextured.A("vertex"));
            GL.EnableVertexAttribArray(DemoShaders.spLambertTextured.A("normal"));

            GL.EnableVertexAttribArray(DemoShaders.spLambertTextured.A("texCoord"));

            GL.VertexAttribPointer(DemoShaders.spLambertTextured.A("vertex"), 4, VertexAttribPointerType.Float, false, 0, MyTeapot.vertices);
            GL.VertexAttribPointer(DemoShaders.spLambertTextured.A("normal"), 4, VertexAttribPointerType.Float, false, 0, MyTeapot.normals);
            GL.VertexAttribPointer(DemoShaders.spLambertTextured.A("texCoord"), 2, VertexAttribPointerType.Float, false, 0, MyTeapot.texCoords);
            //GL.VertexAttribPointer(DemoShaders.spColored.A("color"), 4, VertexAttribPointerType.Float, false, 0, MyCube.colors);
            GL.DrawArrays(PrimitiveType.Triangles, 0, MyTeapot.vertexCount);
            GL.DisableVertexAttribArray(DemoShaders.spLambertTextured.A("vertex"));
            GL.DisableVertexAttribArray(DemoShaders.spLambertTextured.A("normal"));
            GL.DisableVertexAttribArray(DemoShaders.spLambertTextured.A("texCoord"));
            //GL.DisableVertexAttribArray(DemoShaders.spColored.A("color"));




            //torus.drawSolid();

            Glfw.SwapBuffers(window);
        }



        //Metoda główna
        static void Main(string[] args)
        {
            Glfw.Init();

            Window window = Glfw.CreateWindow(500, 500, "OpenGL", GLFW.Monitor.None, Window.None);

            Glfw.MakeContextCurrent(window);
            Glfw.SwapInterval(1);

            GL.LoadBindings(new BC());

            InitOpenGLProgram(window);
            Glfw.Time = 0;
            float angle_x = 0;
            float angle_y = 0;

            while (!Glfw.WindowShouldClose(window))
            {
                angle_x += speed_x * (float)Glfw.Time; //Aktualizuj kat obrotu wokół osi X zgodnie z prędkością obrotu
                angle_y += speed_y * (float)Glfw.Time; //Aktualizuj kat obrotu wokół osi Y zgodnie z prędkością obrotu
                Glfw.Time = 0; //Wyzeruj licznik czasu
                DrawScene(window, angle_x, angle_y);

                Glfw.PollEvents();
            }


            FreeOpenGLProgram(window);

            Glfw.Terminate();
        }


    }
}