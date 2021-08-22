using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Win32;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.HiDpi;

namespace workspacer
{
    public class NativeMonitorContainer : IMonitorContainer
    {
        private Monitor[] _monitors;

        private Dictionary<IMonitor, int> _monitorMap;

        public NativeMonitorContainer()
        {
            var screens = Screen.AllScreens;
            _monitors = new Monitor[screens.Length];
            _monitorMap = new Dictionary<IMonitor, int>();

            var primaryMonitor = new Monitor(0, Screen.PrimaryScreen);
            _monitors[0] = primaryMonitor;
            _monitorMap[primaryMonitor] = 0;

            var index = 1;
            foreach (var screen in screens)
            {
                if (!screen.Primary)
                {
                    var monitor = new Monitor(index, screen);
                    _monitors[index] = monitor;
                    _monitorMap[monitor] = index;
                    index++;
                }
            }

            var handleToDpi = new Dictionary<Screen, (uint X, uint Y)>();
            // TODO: move to win32 library (maybe? would add win forms dependency)
            // pass in action to preserve scope? or have it just give back map of screen dimensions -> dpi
            unsafe
            {
                (uint X, uint Y) mainDpi = (96, 96);
                PInvoke.EnumDisplayMonitors(null, null, (hMonitor, _, _, _) =>
                {
                    var result = PInvoke.GetDpiForMonitor(
                        hMonitor,
                        MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI,
                        out var dpiX,
                        out var dpiY);

                    if (!result.Succeeded)
                    {
                        dpiX = 96; // default 100%
                        dpiY = 96;
                    }

                    var monitorInfo = new MONITORINFO
                    {
                       cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFO))
                    };

                    PInvoke.GetMonitorInfo(hMonitor, &monitorInfo);
                    var screen = Screen.FromRectangle(Rectangle.FromLTRB(
                        monitorInfo.rcWork.left,
                        monitorInfo.rcWork.top,
                        monitorInfo.rcWork.right,
                        monitorInfo.rcWork.bottom
                        ));

                    if (screen.Primary)
                    {
                        mainDpi = (dpiX, dpiY);
                    }

                    handleToDpi[screen] = (dpiX, dpiY);
                    return true;
                }, 0);

                foreach (var m in _monitors)
                {
                    m.Dpi = handleToDpi[m.Screen];
                    m.MainDpi = mainDpi;
                }

            }

            FocusedMonitor = _monitors[0];
        }

        public NativeMonitorContainer(Dictionary<string, int> screenToIndex)
        {
            // TODO: match screen with index
            // win32 get handles to all screens
            // set monitors map with screens
            // map monitor to dpi x/dpiy
        }

        public int NumMonitors => _monitors.Length;

        public IMonitor FocusedMonitor { get; set; }

        public IMonitor[] GetAllMonitors()
        {
            return _monitors.ToArray();
        }

        public IMonitor GetMonitorAtIndex(int index)
        {
            return _monitors[index % _monitors.Length];
        }

        public IMonitor GetMonitorAtPoint(int x, int y)
        {
            var screen = Screen.FromPoint(new Point(x, y));
            return _monitors.FirstOrDefault(m => m.Screen.DeviceName == screen.DeviceName) ?? _monitors[0];
        }

        public IMonitor GetMonitorAtRect(int x, int y, int width, int height)
        {
            var screen = Screen.FromRectangle(new Rectangle(x, y, width, height));
            return _monitors.FirstOrDefault(m => m.Screen.DeviceName == screen.DeviceName) ?? _monitors[0];
        }

        public IMonitor GetNextMonitor()
        {
            var index = _monitorMap[FocusedMonitor];
            if (index >= _monitors.Length - 1)
                index = 0;
            else
                index = index + 1;

            return _monitors[index];
        }

        public int ScaleCoordinates(int size, int numerator, int divisor)
        {
            return PInvoke.MulDiv(size, numerator, divisor);
        }

        public IMonitor GetPreviousMonitor()
        {
            var index = _monitorMap[FocusedMonitor];
            if (index == 0)
                index = _monitors.Length - 1;
            else
                index = index - 1;

            return _monitors[index];
        }
    }
}
