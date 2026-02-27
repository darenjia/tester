using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace FileEditor
{
    public static class WinAPI
    {
        /// <summary>
        ///     表示 Hook 回调函数。
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        ///     鼠标事件标识。
        /// </summary>
        public enum MouseEventFlags
        {
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            Wheel = 0x0800,
            Absolute = 0x8000
        }

        /// <summary>
        ///     表示进程间传递的数据结构
        /// </summary>
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            [MarshalAs(UnmanagedType.LPStr)] public string lpData;
        }

        /// <summary>
        ///     用于通过 API 获取位置信息的结构。
        /// </summary>
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        /// <summary>
        ///     与非托管通信的鼠标位置结构。
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        #region 常量声明

        public const int GWL_WNDPROC = -4; //得到窗口回调函数的地址，或者句柄。得到后必须使用CallWindowProc函数来调用
        public const int GWL_HINSTANCE = -6; //得到应用程序运行实例的句柄
        public const int GWL_HWNDPARENT = -8; //得到父窗口的句柄
        public const int GWL_STYLE = -16; //得到窗口风格
        public const int GWL_EXSTYLE = -20; //得到扩展的窗口风格
        public const int GWL_USERDATA = -21; //得到和窗口相关联的32位的值（每一个窗口都有一个有意留给创建窗口的应用程序是用的32位的值）
        public const int GWL_ID = -12; //得到窗口的标识符

        public const int DWL_MSGRESULT = 0;
        public const int DWL_DLGPROC = 4;
        public const int DWL_USER = 8;

        public const int HWND_BOTTOM = 1;
        public const int HWND_TOP = 0;
        public const int HWND_TOPMOST = -1;
        public const int HWND_NOTOPMOST = -2;

        public const int SWP_DRAW = 0x20;
        public const int SWP_HIDEWINDOW = 0x80;
        public const int SWP_NOACTIVATE = 0x10;
        public const int SWP_NOMOVE = 0x2;
        public const int SWP_NOREDRAW = 0x8;
        public const int SWP_NOSIZE = 0x1;
        public const int SWP_NOZORDER = 0x4;
        public const int SWP_SHOWWINDOW = 0x40;

        public const int WS_OVERLAPPED = 0;
        public const int WS_BORDER = 0x800000;
        public const int WS_CAPTION = 0xC00000;
        public const int WS_CHILD = 0x40000000;
        public const int WS_DLGFRAME = 0x400000;
        public const int WS_SIZEBOX = 0x40000;
        public const int WS_MAXIMIZEBOX = 0x10000;
        public const int WS_MINIMIZEBOX = 0x20000;
        public const int WS_SYSMENU = 0x80000;
        public const int WS_HSCROLL = 0x100000;
        public const int WS_VSCROLL = 0x200000;

        public const int WA_INACTIVE = 0;
        public const int WA_ACTIVE = 1;
        public const int WA_CLICKACTIVE = 2;
        public const int WM_NOTIFY = 0x004E;

        public const int WM_ACTIVATE = 0x0006;
        public const int WM_NULL = 0x0000;
        public const int WM_CREATE = 0x0001;
        public const int WM_DESTROY = 0x0002;
        public const int WM_MOVE = 0x0003;
        public const int WM_SIZE = 0x0005;
        public const int WM_SETFOCUS = 0x0007;
        public const int WM_MOUSEACTIVATE = 0x0021;
        public const int WM_CLOSE = 0x0010;
        public const int WM_QUIT = 0x0012;

        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_CHAR = 0x0102;
        public const int WM_DEADCHAR = 0x0103;
        public const int WM_SYSKEYDOWN = 0x0104;
        public const int WM_SYSKEYUP = 0x0105;
        public const int WM_SYSCHAR = 0x0106;
        public const int WM_SYSDEADCHAR = 0x0107;
        public const int WM_UNICHAR = 0x0109;
        public const int WM_KEYLAST = 0x0109;
        public const int UNICODE_NOCHAR = 0xFFFF;

        public const int MK_LBUTTON = 0x0001;
        public const int MK_RBUTTON = 0x0002;
        public const int MK_SHIFT = 0x0004;
        public const int MK_CONTROL = 0x0008;
        public const int MK_MBUTTON = 0x0010;

        public const int WM_MOUSEFIRST = 0x0200;
        public const int WM_MOUSEMOVE = 0x0200;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_LBUTTONDBLCLK = 0x0203;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_RBUTTONUP = 0x0205;
        public const int WM_RBUTTONDBLCLK = 0x0206;
        public const int WM_MBUTTONDOWN = 0x0207;
        public const int WM_MBUTTONUP = 0x0208;
        public const int WM_MBUTTONDBLCLK = 0x0209;
        public const int WM_MOUSEWHEEL = 0x020A;
        public const int WM_MDICREATE = 0x0220;

        public const int WM_ERASEBKGND = 0x14;
        public const int WM_PAINT = 0xF;
        public const int WM_NC_HITTEST = 0x84;
        public const int WM_NC_PAINT = 0x85;
        public const int WM_PRINTCLIENT = 0x318;
        public const int WM_SETCURSOR = 0x20;

        public const int BM_CLICK = 0x00F5;
        public const int BM_GETIMAGE = 0x00F6;
        public const int BM_SETIMAGE = 0x00F7;

        public const int VK_BACK = 0x08;
        public const int VK_TAB = 0x09;
        public const int VK_CLEAR = 0x0C;
        public const int VK_RETURN = 0x0D;
        public const int VK_SHIFT = 0x10;
        public const int VK_CONTROL = 0x11;
        public const int VK_MENU = 0x12;
        public const int VK_PAUSE = 0x13;
        public const int VK_CAPITAL = 0x14;
        public const int VK_KANA = 0x15;
        public const int VK_HANGEUL = 0x15;
        public const int VK_HANGUL = 0x15;
        public const int VK_JUNJA = 0x17;
        public const int VK_FINAL = 0x18;
        public const int VK_HANJA = 0x19;
        public const int VK_KANJI = 0x19;
        public const int VK_ESCAPE = 0x1B;
        public const int VK_CONVERT = 0x1C;
        public const int VK_NONCONVERT = 0x1D;
        public const int VK_ACCEPT = 0x1E;
        public const int VK_MODECHANGE = 0x1F;
        public const int VK_SPACE = 0x20;
        public const int VK_PRIOR = 0x21;
        public const int VK_NEXT = 0x22;
        public const int VK_END = 0x23;
        public const int VK_HOME = 0x24;
        public const int VK_LEFT = 0x25;
        public const int VK_UP = 0x26;
        public const int VK_RIGHT = 0x27;
        public const int VK_DOWN = 0x28;
        public const int VK_SELECT = 0x29;
        public const int VK_PRINT = 0x2A;
        public const int VK_EXECUTE = 0x2B;
        public const int VK_SNAPSHOT = 0x2C;
        public const int VK_INSERT = 0x2D;
        public const int VK_DELETE = 0x2E;
        public const int VK_HELP = 0x2F;

        public const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        public const int KEYEVENTF_KEYUP = 0x0002;
        public const int KEYEVENTF_UNICODE = 0x0004;
        public const int KEYEVENTF_SCANCODE = 0x0008;

        public const int HC_ACTION = 0x0;
        public const int WH_MOUSELL = 0xE;

        public const int STATUS_SUCCESS = 0x0;

        #endregion

        #region API函数声明

        [DllImport("user32.dll", EntryPoint = "SetParent")]
        public static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", EntryPoint = "GetParent")]
        public static extern int GetParent(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int GetWindowRect(IntPtr hwnd, out Rect lpRect);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int lNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy,
            int uFlags);

        [DllImport("user32.dll", EntryPoint = "UpdateWindow")]
        public static extern bool UpdateWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        [DllImport("user32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "keybd_event")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, long dwExtraInfo);

        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern void SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll", EntryPoint = "DrawMenuBar")]
        public static extern int DrawMenuBar(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "mouse_event")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, IntPtr dwExtraInfo);

        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        public static extern void SetCursorPos(int x, int y);

        [DllImport("user32.dll", EntryPoint = "GetCursorPos")]
        public static extern bool GetCursorPos(out POINT p);

        [DllImport("user32.dll", EntryPoint = "SetWindowsHookEx")]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll", EntryPoint = "UnhookWindowsHookEx")]
        public static extern int UnhookWindowsHookEx(int idHook);

        [DllImport("user32.dll", EntryPoint = "CallNextHookEx")]
        public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "GetWindowDC")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string name);

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(POINT Point);

        [DllImport("user32.dll", EntryPoint = "GetDoubleClickTime")]
        public static extern int GetDoubleClickTime();

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass,
            string lpszWindow);

        #endregion
    }

    /// <summary>
    ///     DevExpress 控件毒手伴侣（专门用来检测关闭它的注册弹框）。
    /// </summary>
    public class AboutDevCompanion
    {
        private readonly int Interval;
        private Thread m_Thread;
        private readonly bool ResidentMode;
        private bool StopCompanion;

        /// <summary>
        ///     按指定的模式创建 <see cref="Wunion.Budget.PowerBasicFramework.AboutDevCompanion" /> 对象实例。
        ///     <param name="interval">检测Dev注册弹框的时间间隔（以毫秒为单位）。</param>
        ///     <param name="resident">检测程序是否以常驻模式运行（默认值 true）。</param>
        /// </summary>
        public AboutDevCompanion(int interval, bool resident = true)
        {
            Interval = interval;
            ResidentMode = resident;
            StopCompanion = true;
        }

        /// <summary>
        ///     关闭 DevExpress 控件的注册弹框（如果调用时找到并关闭了DevExpress注册弹框则返回true，否则返回false）。
        /// </summary>
        /// <returns></returns>
        private bool CloseAboutDev()
        {
            var devHwnd = WinAPI.FindWindow(null, "About DevExpress");
            if (devHwnd != IntPtr.Zero)
            {
                WinAPI.SendMessage(devHwnd, WinAPI.WM_CLOSE, 0, 0);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     运行 Dev 伴侣，对其注册弹框下毒手。
        /// </summary>
        public void Run()
        {
            if (!StopCompanion)
                return; // 防止多次运行导致可能的线程错误。
            StopCompanion = false;
            m_Thread = new Thread(() =>
            {
                while (!StopCompanion)
                {
                    if (ResidentMode)
                        CloseAboutDev();
                    else // 如果非常驻模式，则在检测并关闭Dev注册弹框后应结束程序。
                        StopCompanion = CloseAboutDev();
                    Thread.Sleep(Interval);
                }
                m_Thread = null;
            });
            m_Thread.Start();
        }

        /// <summary>
        ///     关闭毒手程序的运行。
        /// </summary>
        public void Stop()
        {
            StopCompanion = true;
        }
    }
}