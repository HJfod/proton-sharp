using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;

namespace BorderlessResizer {
    public class GlobalMouseHandler {
        /// Massive thank you to where ever the fuck I stole this from

        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_LBUTTONUP = 0x202;
        private const int MK_LBUTTON = 0x0001;
        private const int WH_MOUSE_LL = 14;

        private delegate int HookProc(int nCode, int wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern int SetWindowsHookEx(
            int idHook,
            HookProc lpfn,
            IntPtr hMod,
            int dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int CallNextHookEx(
            int idHook,
            int nCode,
            int wParam,
            IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall )]
        public static extern IntPtr SetCursor(IntPtr handle);

        public const int   MOUSEEVENTF_LEFTDOWN    = 0x02;
        public const int   MOUSEEVENTF_LEFTUP      = 0x04;
        public const int   MOUSEEVENTF_RIGHTDOWN   = 0x08;
        public const int   MOUSEEVENTF_RIGHTUP     = 0x10;
        
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
        private const byte RZ_NONE      = 0b0000;
        private const byte RZ_LEFT      = 0b0001;
        private const byte RZ_UP        = 0b0010;
        private const byte RZ_RIGHT     = 0b0100;
        private const byte RZ_DOWN      = 0b1000;

        public static void ApplyWindowResizer(this Form _form, int _grip = 10) {
            byte resizing = RZ_NONE;
            bool resetCursor = false;

            byte CheckLocation (Point p) {
                byte res = RZ_NONE;
                if (_form.Left < p.X && p.X < _form.Left + _grip)                               res += RZ_LEFT;
                if (_form.Left + _form.Width > p.X && p.X > _form.Left + _form.Width - _grip)   res += RZ_RIGHT;
                if (_form.Top < p.X && p.X < _form.Top + _grip)                                 res += RZ_UP;
                if (_form.Top + _form.Height > p.Y && p.Y > _form.Top + _form.Height - _grip)   res += RZ_DOWN;
                return res;
            }
            
            void SetCursor(byte b) {
                Cursor c = Cursors.Default;

                if (b != RZ_NONE) {
                    if ((RZ_LEFT | RZ_RIGHT) == (b | 0b0101))
                        c = Cursors.SizeWE;
                    else if ((RZ_UP | RZ_DOWN) == (b | 0b1010))
                        c = Cursors.SizeNS;
                    else if ((RZ_UP | RZ_LEFT) == b || (RZ_DOWN | RZ_RIGHT) == b)
                        c = Cursors.SizeNWSE;
                    else if ((RZ_UP | RZ_RIGHT) == b || (RZ_DOWN | RZ_LEFT) == b)
                        c = Cursors.SizeNESW;
                    resetCursor = false;
                } else {
                    if (!resetCursor) {
                        resetCursor = true;
                        _form.Cursor = null;
                    }
                }
                
                //GlobalMouseHandler.SetCursor(c.Handle);
                if (!resetCursor) _form.Cursor = c;
            }

            GlobalMouseHandler.SuperMouseClick += (p, d) => {
                byte l = CheckLocation(p);
                if (d && resizing == RZ_NONE && l != RZ_NONE) resizing = l;
            };

            GlobalMouseHandler.SuperMouseMove += (p, l) => {
                SetCursor(CheckLocation(p));

                if (l && resizing != RZ_NONE) {
                    if ((RZ_LEFT | resizing) == resizing) {
                        _form.Width += _form.Left - p.X;
                        _form.Left = p.X;
                    }
                    if ((RZ_RIGHT | resizing) == resizing)
                        _form.Width -= _form.Left + _form.Width - p.X;
                    if ((RZ_UP | resizing) == resizing) {
                        _form.Height += _form.Top - p.Y;
                        _form.Top = p.Y;
                    }
                    if ((RZ_DOWN | resizing) == resizing)
                        _form.Height -= _form.Top + _form.Height - p.Y;
                } else {
                    resizing = RZ_NONE;
                }
            };
            
            GlobalMouseHandler.Initialize();
        }
    }
}