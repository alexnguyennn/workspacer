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
            var currentWorkspace = Context.WorkspaceContainer.GetWorkspaceForMonitor(Context.Monitor);
            var focusedWindow = currentWorkspace.FocusedWindow ??
                currentWorkspace.LastFocusedWindow ??
                currentWorkspace.ManagedWindows.FirstOrDefault();

            // TODO: feed callback that switches to that window from context
            // TODO: implement focus window given int
            // TODO: match on window title
            var managedWindows = currentWorkspace.ManagedWindows;
            return managedWindows
                .Select((window) => (window.Handle == focusedWindow?.Handle))
                .Zip(managedWindows,
                    (isFocused, window) => Part(
                        getTitleString(IsShortTitle
                            ? window.Title.Split("-").Last()
                            : window.Title),
                        isFocused ? color : Color.Gray,
                        maxWidth: GetTitleMaxWidth()))
                .ToArray();
        }

        private int GetTitleMaxWidth()
        {
            var focusedMonitor = Context.MonitorContainer.FocusedMonitor;
            var monitorWidth = focusedMonitor.Width;
            var nWindows = Context.WorkspaceContainer.GetWorkspaceForMonitor(focusedMonitor).ManagedWindows.Count();
            return (nWindows > 0 ? monitorWidth / nWindows : monitorWidth) - OtherWidgetOffset;
        }

        private string getTitleString(string windowTitle)
        {
            return $"|{windowTitle}";
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
