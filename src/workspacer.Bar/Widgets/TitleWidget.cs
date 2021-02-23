using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace workspacer.Bar.Widgets
{
    public class TitleWidget : BarWidgetBase
    {
        public Color MonitorHasFocusColor { get; set; } = Color.Yellow;
        public bool IsShortTitle { get; set; } = false;
        public string NoWindowMessage { get; set; } = "No Windows";


        /// <summary>
        /// Offset to limit space taken up by titles
        /// increase offset to make space for other widgets on left/right sections
        /// TODO: check if can source widths of other widgets in section
        /// </summary>
        public int OtherWidgetOffset { get; set; } = 0;

        public override IBarWidgetPart[] GetParts()
        {
            var window = GetWindow();
            var isFocusedMonitor = Context.MonitorContainer.FocusedMonitor == Context.Monitor;
            var multipleMonitors = Context.MonitorContainer.NumMonitors > 1;
            var color = isFocusedMonitor && multipleMonitors ? MonitorHasFocusColor : null;

            return (window != null)
                ? Parts(GetWindowTitles(color))
                : Parts(Part(NoWindowMessage, color, fontname: FontName));
        }

        public override void Initialize()
        {
            Context.Workspaces.WindowAdded += RefreshAddRemove;
            Context.Workspaces.WindowRemoved += RefreshAddRemove;
            Context.Workspaces.WindowUpdated += RefreshUpdated;
            Context.Workspaces.FocusedMonitorUpdated += RefreshFocusedMonitor;
        }

        private IWindow GetWindow()
        {
            var currentWorkspace = Context.WorkspaceContainer.GetWorkspaceForMonitor(Context.Monitor);
            return currentWorkspace.FocusedWindow ??
                currentWorkspace.LastFocusedWindow ??
                currentWorkspace.ManagedWindows.FirstOrDefault();
        }

        private IBarWidgetPart[] GetWindowTitles(Color color)
        {
            var focusedMonitor = Context.Monitor;
            var currentWorkspace = Context.WorkspaceContainer.GetWorkspaceForMonitor(focusedMonitor);
            var focusedWindow = currentWorkspace.FocusedWindow ??
                currentWorkspace.LastFocusedWindow ??
                currentWorkspace.ManagedWindows.FirstOrDefault();

            // TODO: feed callback that switches to that window from context
            // TODO: implement focus window given int
            // TODO: match on window title
            var managedWindows = currentWorkspace.ManagedWindows;
            var nWindows = managedWindows.Count();
            return managedWindows
                .Select((window) => (window.Handle == focusedWindow?.Handle))
                .Zip(managedWindows,
                    (isFocused, window) => Part(
                        GetTitleString(
                                IsShortTitle
                                    ? window.Title.Split("-").Last()
                                    : window.Title.Trim(),
                                ShouldTruncateAt(
                                    window.Title.Trim().Length,
                                    GetTitleMaxWidth(nWindows))),
                        isFocused ? color : Color.Gray,
                        isFocused ? Color.Teal : Color.Black, // TODO: make elegant, centre text
                        maxWidth: GetTitleMaxWidth(nWindows)))
                .ToArray();
        }

        private int GetTitleMaxWidth(int nWindows)
        {
            var barMonitor = Context.Monitor; // width always respects monitor space it is on, not focused one
            var monitorWidth = barMonitor.Width - OtherWidgetOffset;
            return nWindows > 0 ? monitorWidth / nWindows : monitorWidth;
        }

        private int? ShouldTruncateAt(int titleLength, int maxWidth)
        {
            return titleLength > maxWidth ? maxWidth : null;
        }

        private string GetTitleString(string windowTitle, int? truncateCharactersAt = null)
        {
            return truncateCharactersAt is null ? windowTitle : windowTitle.Substring(0, truncateCharactersAt.Value);
        }

        private void RefreshAddRemove(IWindow window, IWorkspace workspace)
        {
            var currentWorkspace = Context.WorkspaceContainer.GetWorkspaceForMonitor(Context.Monitor);
            if (workspace == currentWorkspace)
            {
                Context.MarkDirty();
            }
        }

        private void RefreshUpdated(IWindow window, IWorkspace workspace)
        {
            var currentWorkspace = Context.WorkspaceContainer.GetWorkspaceForMonitor(Context.Monitor);
            if (workspace == currentWorkspace && window == GetWindow())
            {
                Context.MarkDirty();
            }
        }

        private void RefreshFocusedMonitor()
        {
            Context.MarkDirty();
        }

        public static string GetShortTitle(string title)
        {
            var parts = title.Split(new char[] { '-', '—', '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return title.Trim();
            }
            return parts.Last().Trim();
        }
    }
}
