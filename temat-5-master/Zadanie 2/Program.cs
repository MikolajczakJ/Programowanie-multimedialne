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
        }

        public static void InitSound()
        {
            device = ALC.OpenDevice(null);
            context = ALC.CreateContext(device, new ALContextAttributes());
            ALC.MakeContextCurrent(context);

            buf = AL.GenBuffer();

            int channels, bits, sampleFreq;
            double f = 800;
            double A = short.MaxValue;
            int fp = 44100;
            double op = 1.0 / fp;
            int lp = 10 * fp;
            short[] data = new short[lp];
            for (int x = 0; x < lp; x++)
            {
                data[x] = signal(op * x, f, A); //generuj kolejne próbki
            }
            AL.BufferData<short>(buf, ALFormat.Mono16, data, fp);

            source = AL.GenSource();

            AL.BindBufferToSource(source, buf);
            AL.Source(source, ALSourceb.Looping, true);
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
        public static short signal(double t, double f, double A)
        {
            
            return (short)(A * Math.Sin(2.0 * Math.PI * f * t));
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