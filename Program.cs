using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Text;

namespace proton {
    public static class Settings {
        public static Size DefaultSize = new Size( 1080, 720 );
        public static string AppName = "Proton";
        public static string AppVerString = "v0.1.0";
        public static int AppVerNum = 1;

        public static class S {
            public static uint ScrollWheelDecelerator = 1;
            public static bool RememberSession = false;
            public static bool RememberSize = false;
            public static Encoding DefaultEncoding = Encoding.UTF8;
        }
    }

    static class Program {
        [DllImport( "kernel32.dll" )]
        static extern bool AttachConsole( int dwProcessId );

        [STAThread]
        static void Main() {
            AttachConsole( -1 );

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            Style.Init();

            Application.Run(new Main());
        }
    }
}