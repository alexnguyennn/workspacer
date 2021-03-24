using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace workspacer.Bar.Widgets
{
    public class FocusedMonitorWidget : BarWidgetBase
    {
       

        public string FocusedText { get; set; } = "**********";
        public string UnfocusedText { get; set; } = "";

        public override IBarWidgetPart[] GetParts()
        {
            if (Context.MonitorContainer.FocusedMonitor == Context.Monitor)
            {
                return Parts(Part(FocusedText, fontname: FontName));
            } else
            {
                return Parts(Part(UnfocusedText, fontname: FontName));
            }
        }

        public override void Initialize()
        {
            Context.Workspaces.FocusedMonitorUpdated += () =>
            {
                Context.MarkDirty();
                // TODO: extract logic into own plugin
                // TODO: position is not correct in non-full screen layout engines, implement mouse move on window focus too (but for specific layouts)
                // TODO: use activelayout widget as inspiration -> config layout; focus logic
                var monitor = Context.MonitorContainer.FocusedMonitor;
                Cursor.Position =  new Point(monitor.X + (monitor.Width / 2), monitor.Y + (monitor.Height / 2));
            };
        }
    }
}
