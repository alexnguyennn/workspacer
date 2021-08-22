using System;
using System.Collections.Generic;
using System.Linq;

namespace workspacer.Bar
{
    public class MenuBarLayoutEngine : ILayoutEngine
    {
        private IMonitorContainer _monitors;
        private string _title;
        private int _offset;
        private readonly IConfigContext _context;
        private ILayoutEngine _inner;
        public string Name => _inner.Name;

        public MenuBarLayoutEngine(ILayoutEngine inner, string title, int offset, IConfigContext context)
        {
            _inner = inner;
            _title = title;
            _offset = offset;
            _context = context;
            _monitors = context.MonitorContainer;
        }

        public IEnumerable<IWindowLocation> CalcLayout(IEnumerable<IWindow> windows, int spaceWidth, int spaceHeight, IMonitor monitor)
        {
            var newWindows = windows.Where(w => !w.Title.Contains(_title));
            var mc = _context.MonitorContainer;
            // var offset = mc.ScaleCoordinates(_offset, (int)monitor.Dpi.Y, (int)monitor.MainDpi.Y);
            var offset = ScaleSize(_offset, monitor.Dpi.Y, monitor.MainDpi.Y);

            // return _inner.CalcLayout(newWindows, spaceWidth, spaceHeight - _offset)
            return _inner.CalcLayout(newWindows, spaceWidth, spaceHeight - offset, monitor)
                .Select(l => new WindowLocation(l.X, l.Y + offset, l.Width, l.Height, l.State));
        }

        private int ScaleSize(int size, uint currentDpi, uint primaryDpi)
        {
            // reimplement muldiv to reduce pinvokes required? or move to scale method to win32 lib and call it
            return (int)Math.Floor((double)size * currentDpi / primaryDpi);
        }

        public void ShrinkPrimaryArea()
        {
            _inner.ShrinkPrimaryArea();
        }

        public void ExpandPrimaryArea()
        {
            _inner.ExpandPrimaryArea();
        }

        public void ResetPrimaryArea()
        {
            _inner.ResetPrimaryArea();
        }

        public void IncrementNumInPrimary()
        {
            _inner.IncrementNumInPrimary();
        }

        public void DecrementNumInPrimary()
        {
            _inner.DecrementNumInPrimary();
        }
    }
}
