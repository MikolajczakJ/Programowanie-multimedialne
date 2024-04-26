using GLFW;

using OpenTK;
using OpenTK.Audio.OpenAL;

using HelpersNS;

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


        static KeyCallback kc = KeyProcessor;

        static ALDevice device;
        static ALContext context;
        static int buf;
        static int source;


        public static void KeyProcessor(System.IntPtr window, Keys key, int scanCode, InputState state, ModifierKeys mods)
        {
            if (state == InputState.Press) 
            {
                if (key == Keys.P)
                {
                    int sourcestate;
                    AL.GetSource(source, ALGetSourcei.SourceState, out sourcestate);
                    if ((ALSourceState)sourcestate == ALSourceState.Playing)
                    {
                        AL.SourcePause(source);
                    }
                    if ((ALSourceState)sourcestate == ALSourceState.Paused)
                    {
                        AL.SourcePlay(source);
                    }
                    if ((ALSourceState)sourcestate == ALSourceState.Stopped)
                    {
                        AL.SourcePlay(source);
                    }
                }
            }
        }

        public static void InitSound()
        {
            device = ALC.OpenDevice(null);
            context = ALC.CreateContext(device, new ALContextAttributes());
            ALC.MakeContextCurrent(context);

            buf = AL.GenBuffer();

            int channels, bits, sampleFreq;
            byte[] data = Helpers.LoadWave("misc_sound.wav", out channels, out bits, out sampleFreq);
            AL.BufferData<byte>(buf, Helpers.GetFormat(channels, bits), data, sampleFreq);

            source = AL.GenSource();

            AL.BindBufferToSource(source, buf);
            AL.Source(source,ALSourceb.Looping,true);
            AL.SourcePlay(source);
        }

        public static void FreeSound()
        {
            AL.SourceStop(source);
            AL.DeleteSource(source);
            AL.DeleteBuffer(buf);

            if (context != ALContext.Null)
            {
                ALC.MakeContextCurrent(ALContext.Null);
                ALC.DestroyContext(context);
            }
            context = ALContext.Null;

            if (device != ALDevice.Null)
            {
                ALC.CloseDevice(device);
            }
            device = ALDevice.Null;
        }

        public static void SoundEvents()
        {

        }

        static void Main(string[] args)
        {
            Glfw.Init();

            Window window = Glfw.CreateWindow(500, 500, "OpenAL", GLFW.Monitor.None, Window.None);

            Glfw.MakeContextCurrent(window);
            Glfw.SetKeyCallback(window, kc);

            InitSound();


            while (!Glfw.WindowShouldClose(window))
            {
                SoundEvents();
                Glfw.PollEvents();
            }


            FreeSound();
            Glfw.Terminate();
        }


    }
}