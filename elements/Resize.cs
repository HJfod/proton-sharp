using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Reflection;

namespace proton {
    public delegate void MouseMovedEvent(Point Location, bool MouseDown);

    public class GlobalMouseHandler {
        /// Massive thank you to where ever the fuck I stole this from

        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_LBUTTONUP = 0x202;
        private const int MK_LBUTTON = 0x0001;
        private const int WH_MOUSE_LL = 14;

        private delegate int HookProc(int nCode, int wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern int SetWindowsHookEx(
            int idHook,
            HookProc lpfn,
            IntPtr hMod,
            int dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern int CallNextHookEx(
            int idHook,
            int nCode,
            int wParam,
            IntPtr lParam);
        
        [StructLayout(LayoutKind.Sequential)]
        private struct MouseLLHookStruct {
            public Point Point;
            public int MouseData;
            public int Flags;
            public int Time;
            public int ExtraInfo;
        }

        private static void SuperMouseEventHandler(Point Location, bool MouseLeft) {}
        public static event Action<Point, bool> SuperMouseMove;

        private static void SuperMouseClickEventHandler(Point Location, bool Down) {}
        public static event Action<Point, bool> SuperMouseClick;

        private static int m_OldX;
        private static int m_OldY;
        private static bool m_isDown = false;

        private static HookProc s_MouseDelegate;

        public static void Initialize() {
            SuperMouseMove += new Action<Point, bool>(SuperMouseEventHandler);
            SuperMouseClick += new Action<Point, bool>(SuperMouseClickEventHandler);

            s_MouseDelegate = MouseHookProc;

            SetWindowsHookEx(
            WH_MOUSE_LL,
            s_MouseDelegate,
            Marshal.GetHINSTANCE(
                Assembly.GetExecutingAssembly().GetModules()[0]),
            0);
        }

        private static int MouseHookProc(int nCode, int wParam, IntPtr lParam) {
            if (nCode >= 0) {
                MouseLLHookStruct mouseHookStruct = (MouseLLHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseLLHookStruct));
                
                Point mPos = new Point(mouseHookStruct.Point.X, mouseHookStruct.Point.Y);

                switch (wParam) {
                    case WM_LBUTTONDOWN:
                        m_isDown = true;
                        SuperMouseClick(mPos, true);
                        break;
                    case WM_LBUTTONUP:
                        m_isDown = false;
                        SuperMouseClick(mPos, false);
                        break;
                }

                if ((SuperMouseMove != null) && (m_OldX != mouseHookStruct.Point.X || m_OldY != mouseHookStruct.Point.Y)) {
                    m_OldX = mouseHookStruct.Point.X;
                    m_OldY = mouseHookStruct.Point.Y;

                    SuperMouseMove(mPos, m_isDown);
                }
            }

            return 0;
        }
    }

    public static class WindowResizer {
        private const byte RZ_NONE      = 0;
        private const byte RZ_LEFT      = 3;
        private const byte RZ_UP        = 1;
        private const byte RZ_RIGHT     = 4;
        private const byte RZ_DOWN      = 6;
        private const byte RZ_UPLEFT    = 4;
        private const byte RZ_UPRIGHT   = 5;
        private const byte RZ_DOWNRIGHT = 10;
        private const byte RZ_DOWNLEFT  = 9;

        public static void ApplyWindowResizer(this Form _form, int _grip = 10) {
            byte resizing = RZ_NONE;

            byte CheckLocation (Point p) {
                byte res = RZ_NONE;
                if (_form.Left < p.X && p.X < _form.Left + _grip)                               res += RZ_LEFT;
                if (_form.Left + _form.Width > p.X && p.X > _form.Left + _form.Width - _grip)   res += RZ_RIGHT;
                if (_form.Top < p.X && p.X < _form.Top + _grip)                                 res += RZ_UP;
                if (_form.Top + _form.Height > p.Y && p.Y > _form.Top + _form.Height - _grip)   res += RZ_DOWN;
                return res;
            }

            GlobalMouseHandler.SuperMouseClick += (p, d) => {
                byte l = CheckLocation(p);
                if (d && resizing == RZ_NONE && l != RZ_NONE) resizing = l;
            };

            GlobalMouseHandler.SuperMouseMove += (p, l) => {
                if (l && resizing != RZ_NONE) {
                    switch (resizing) {
                        case RZ_LEFT:
                            _form.Width += _form.Left - p.X;
                            _form.Left = p.X;
                            break;
                        case RZ_RIGHT:
                            _form.Width -= _form.Left + _form.Width - p.X;
                            break;
                    }
                } else {
                    resizing = RZ_NONE;
                }
            };
            
            GlobalMouseHandler.Initialize();
        }
    }
}