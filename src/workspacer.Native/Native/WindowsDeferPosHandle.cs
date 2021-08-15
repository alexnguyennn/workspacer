using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace workspacer
{
    public class WindowsDeferPosHandle : IWindowsDeferPosHandle
    {
        private static Logger Logger = Logger.Create();
        private IntPtr _info;
        private readonly IEnumerable<IWindow> _focusStealingWindows;

        private List<IWindow> _toMinimize;
        private List<IWindow> _toMaximize;
        private List<IWindow> _toNormal;

        public WindowsDeferPosHandle(IntPtr info, IEnumerable<IWindow> focusStealingWindows)
        {
            _info = info;
            _focusStealingWindows = focusStealingWindows;
            _toMinimize = new List<IWindow>();
            _toMaximize = new List<IWindow>();
            _toNormal = new List<IWindow>();
        }

        public void Dispose()
        {
            foreach (var w in _toMinimize)
            {
                // Logger.Info($"_toMinimize: window prop:{JsonSerializer.Serialize(w)}");
                if (!w.IsMinimized)
                {
                    Win32.ShowWindow(w.Handle, Win32.SW.SW_MINIMIZE);
                }
            }
            foreach (var w in _toMaximize)
            {
                // Logger.Info($"_toMaximise: window prop:{JsonSerializer.Serialize(w)}");
                if (!w.IsMaximized)
                {
                    Win32.ShowWindow(w.Handle, Win32.SW.SW_SHOWMAXIMIZED);
                }
            }
            foreach (var w in _toNormal)
            {
                // Logger.Info($"_toNormal: window prop:{JsonSerializer.Serialize(w)}");
                if (!w.IsFocused
                    && _toNormal.Count > 1
                    && (_focusStealingWindows.Contains(w)
                    ))
                {
                    // Logger.Info($"_toNormal: focus window stealer; window prop:{JsonSerializer.Serialize(w)}");
                    Win32.ShowWindow(w.Handle, Win32.SW.SW_SHOWNOACTIVATE);
                }
                else
                {
                    // Logger.Info($"_toNormal: normal window; window prop:{JsonSerializer.Serialize(w)}");
                    Win32.ShowWindow(w.Handle, Win32.SW.SW_SHOWNORMAL);
                }
            }

            Win32.EndDeferWindowPos(_info);
        }

        public void DeferWindowPos(IWindow window, IWindowLocation location)
        {
            // TODO: cleanup ignore focus stealing draft
            // 1. Move workspace can still be a little buggy (focus wrong one on move, no resize?)
            // 2. matching filter is O(windows * filters) - optimise?
            // 3. consider setting movement branch in this method instead of on dispose
            // var flags = (window.ProcessName == "vcxsrv" || window.ProcessName == "rider64")
            var flags = Win32.SWP.SWP_FRAMECHANGED | Win32.SWP.SWP_NOACTIVATE | Win32.SWP.SWP_NOCOPYBITS |
                        Win32.SWP.SWP_NOZORDER | Win32.SWP.SWP_NOOWNERZORDER;

            // if (!window.IsFocused && (window.ProcessName == "vcxsrv" || window.ProcessName == "rider64"))
            // if (window.ProcessName == "vcxsrv" || window.ProcessName == "rider64")
            // {
            //     // _toMinimize.Add(window);
            //     flags = flags | Win32.SWP.SWP_NOMOVE | Win32.SWP.SWP_NOSIZE;
            // }
            //
            // else
            if (location.State == WindowState.Maximized)
            {
                _toMaximize.Add(window);
                flags = flags | Win32.SWP.SWP_NOMOVE | Win32.SWP.SWP_NOSIZE;
            }
            else if (location.State == WindowState.Minimized)
            {
                _toMinimize.Add(window);
                flags = flags | Win32.SWP.SWP_NOMOVE | Win32.SWP.SWP_NOSIZE;
            }
            else
            {
                // if (window.ProcessName == "vcxsrv" || window.ProcessName == "rider64")
                // {
                //     flags = flags | Win32.SWP.SWP_NOMOVE | Win32.SWP.SWP_NOSIZE;
                // }

                _toNormal.Add(window);
            }

            // Calculate final position for window
            var offset = window.Offset;
            int X = location.X + offset.X;
            int Y = location.Y + offset.Y;
            int Width = location.Width + offset.Width;
            int Height = location.Height + offset.Height;

            Win32.DeferWindowPos(_info, window.Handle, IntPtr.Zero, X, Y, Width, Height, flags);
        }
    }
}
