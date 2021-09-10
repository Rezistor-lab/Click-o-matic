using System;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Interop;

namespace ClickerMatic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("User32.dll")] private static extern bool RegisterHotKey([In] IntPtr hWnd, [In] int id, [In] uint fsModifiers, [In] uint vk);
        [DllImport("User32.dll")] private static extern bool UnregisterHotKey([In] IntPtr hWnd, [In] int id);
        [DllImport("User32.dll", SetLastError = true)] public static extern int SendInput(int nInputs, ref INPUT pInputs, int cbSize);

        private Timer _timer;

        //mouse event constants
        const int MOUSEEVENTF_LEFTDOWN = 2;
        const int MOUSEEVENTF_LEFTUP = 4;
        //input type constant
        const int INPUT_MOUSE = 0;

        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        public struct INPUT
        {
            public uint type;
            public MOUSEINPUT mi;
        };

        private HwndSource _source;
        private const int HOTKEY_ID = 9000;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            WindowInteropHelper helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);
            _timer = new Timer();
            _timer.Elapsed += _timer_Elapsed;

            const uint VK_F10 = 0x79;
            const uint MOD_CTRL = 0x0002;
            if (!RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_CTRL, VK_F10))
            {
                // handle error
            }
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //set up the INPUT struct and fill it for the mouse down
            INPUT i = new INPUT();
            i.type = INPUT_MOUSE;
            i.mi.dx = 0;
            i.mi.dy = 0;
            i.mi.dwFlags = MOUSEEVENTF_LEFTDOWN;
            i.mi.dwExtraInfo = IntPtr.Zero;
            i.mi.mouseData = 0;
            i.mi.time = 0;
            //send the input 
            _ = SendInput(1, ref i, Marshal.SizeOf(i));
            //set the INPUT for mouse up and send it
            i.mi.dwFlags = MOUSEEVENTF_LEFTUP;
            _ = SendInput(1, ref i, Marshal.SizeOf(i));
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            _source = null;
            WindowInteropHelper helper = new WindowInteropHelper(this);
            _ = UnregisterHotKey(helper.Handle, HOTKEY_ID);
            base.OnClosed(e);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            _timer.Interval = int.Parse(tbSpeed.Text);
                            _timer.Enabled = !_timer.Enabled;
                            handled = true;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }
    }
}
