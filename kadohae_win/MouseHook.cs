using System;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace 角から生えるやつ
{
    public class MouseHook
    {
        #region EventHandler
        public delegate void MouseMoveHandler(POINT point);
        static public event MouseMoveHandler MouseMove;
        static public event EventHandler Error;
        #endregion

        #region PInvoke
        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static int hHook = 0;
        private static int hLLHook = 0;

        private const int HC_ACTION = 0;
        private const int WH_MOUSE_LL = 14;

        static HookProc MouseHookLLProcedure;

        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MouseHookStruct
        {
            public POINT pt;
            public int hwnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MouseLLHookStruct
        {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);
        #endregion

        static public void StartMouseLowLevelHook()
        {
            if (hLLHook == 0)
            {
                MouseHookLLProcedure = new HookProc(MouseLLHookProc);
                hLLHook = SetWindowsHookEx(WH_MOUSE_LL,
                            MouseHookLLProcedure,
                            GetModuleHandle(null),
                            0);
                if (hLLHook == 0)
                {
                    //MessageBox.Show("SetWindowsHookEx Failed");
                    Error?.Invoke(null, EventArgs.Empty);
                    return;
                }
            }
        }

        static  public void FinishMouseLowLevelHook()
        {
            if (hLLHook != 0)
            {
                bool ret = UnhookWindowsHookEx(hLLHook);
                if (ret == false)
                {
                    //MessageBox.Show("UnhookWindowsHookEx Failed");
                    Error?.Invoke(null, EventArgs.Empty);
                    return;
                }
                hLLHook = 0;
            }
        }

        static private int MouseLLHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode == HC_ACTION)
            {
                var mouse = (MouseLLHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseLLHookStruct));
                MouseMove?.Invoke(mouse.pt);
            }
            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }

        // MouseHook.MouseMove(MouseHook.MouseMoveHandler<MouseHook.POINT>)イベントのObservable化する
        public static IObservable<MouseHook.POINT> MouseMoveAsObservable()
        {
            return Observable.FromEvent<MouseHook.MouseMoveHandler, MouseHook.POINT>(
                h => (point) => h(point),
                h => MouseHook.MouseMove += h,
                h => MouseHook.MouseMove -= h);
        }
    }
}
