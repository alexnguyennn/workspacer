using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace workspacer.FocusIndicator
{
    public class FocusIndicatorPlugin : IPlugin
    {
        private IConfigContext _context;
        private FocusIndicatorPluginConfig _config;

        private FocusIndicatorForm _form;

        public FocusIndicatorPlugin() : this(new FocusIndicatorPluginConfig()) { }

        public FocusIndicatorPlugin(FocusIndicatorPluginConfig config)
        {
            _config = config;
            _form = new FocusIndicatorForm(config);
        }

        public void AfterConfig(IConfigContext context)
        {
            _context = context;

            _context.Windows.WindowFocused += WindowFocused;
        }

        private void WindowFocused(IWindow window)
        {
            // TODO: extract logic somewhere more convenient (can even hook from config context)
            // TODO: position is not correct in non-full screen layout engines, maybe doing it there  better
            // TODO: possibly do it on workspace change when there's no window too?
            var screen = Screen.AllScreens[_context.MonitorContainer.FocusedMonitor.Index].Bounds.Location;
            Cursor.Position =  new Point(screen.X + (window.Location.Width / 2), screen.Y + (window.Location.Height / 2));
            var location = window.Location;
            _form.ShowInLocation(location);
        }
    }
}
