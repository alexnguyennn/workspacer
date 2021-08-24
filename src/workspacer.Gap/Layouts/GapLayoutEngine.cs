using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace workspacer.Gap
{
    public class GapLayoutEngine : ILayoutEngine
    {
        private int _innerGap;
        private int _outerGap;
        private int _delta;
        private ILayoutEngine _inner;
        public string Name => _inner.Name;

        public GapLayoutEngine(ILayoutEngine inner, int innerGap = 0, int outerGap = 0, int delta = 20)
        {
            _inner = inner;
            _innerGap = innerGap;
            _outerGap = outerGap;
            _delta = delta;
        }

        public IEnumerable<IWindowLocation> CalcLayout(
            IEnumerable<IWindow> windows, int spaceWidth, int spaceHeight, IMonitor monitor)
        {
            // TODO: fix properly
            // TODO: pass in dpi with expand/shrink methods
            var doubleOuter = ScaleSize(_outerGap * 2, monitor);
            var halfInner = ScaleSize(_innerGap / 2, monitor);
            return _inner
                .CalcLayout(windows, spaceWidth - doubleOuter, spaceHeight - doubleOuter, monitor)
                .Select(l => new WindowLocation(
                    l.X + ScaleSize(_outerGap, monitor) + halfInner,
                    l.Y + ScaleSize(_outerGap, monitor) + halfInner,
                    l.Width - ScaleSize(_innerGap, monitor),
                    l.Height - ScaleSize(_innerGap, monitor),
                    l.State)
            );
        }

        private int ScaleSize(int size, IMonitor monitor)
        {
            return ScaleSize(size, monitor.Dpi.X, monitor.MainDpi.X);
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

        public void IncrementInnerGap()
        {
            _innerGap += _delta;
        }

        public void DecrementInnerGap()
        {
            _innerGap -= _delta;
            if (_innerGap < 0)
                _innerGap = 0;
        }

        public void IncrementOuterGap()
        {
            _outerGap += _delta;
        }

        public void DecrementOuterGap()
        {
            _outerGap -= _delta;
            if (_outerGap < 0)
                _outerGap = 0;
        }

        public void ClearGaps()
        {
            _innerGap = 0;
            _outerGap = 0;
        }
    }
}
