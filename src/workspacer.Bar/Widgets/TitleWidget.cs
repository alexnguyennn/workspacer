using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace workspacer.Bar.Widgets
{
    public class TitleWidget : BarWidgetBase
    {
        private static Logger Logger = Logger.Create();
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
                : Parts(Part(
                    text: NoWindowMessage,
                    fore: color,
                    back: isFocusedMonitor ? Color.Teal : Color.Black, // TODO: move to config
                    fontname: FontName));
        }

        public override void Initialize()
        {
            Context.Workspaces.WindowAdded += RefreshAddRemove;
            Context.Workspaces.WindowRemoved += RefreshAddRemove;
            Context.Workspaces.WindowUpdated += RefreshUpdated;
            Context.Workspaces.FocusedMonitorUpdated += RefreshFocusedMonitor;
            Context.Workspaces.WorkspaceUpdated += RefreshFocusedMonitor;
            Context.Workspaces.WindowMoved += RefreshWindowMoved;
        }

        private IWindow GetWindow()
        {
            var currentWorkspace = Context.WorkspaceContainer.GetWorkspaceForMonitor(Context.Monitor);
            if (!currentWorkspace.ManagedWindows.Any()) return null;

            return currentWorkspace.FocusedWindow ??
                currentWorkspace.LastFocusedWindow ??
                currentWorkspace.ManagedWindows.FirstOrDefault();
        }

        private IBarWidgetPart[] GetWindowTitles(Color color)
        {
            var currentMonitor = Context.Monitor;
            var currentWorkspace = Context.WorkspaceContainer.GetWorkspaceForMonitor(currentMonitor);
            var focusedWindow = currentWorkspace.FocusedWindow ??
                currentWorkspace.LastFocusedWindow ??
                currentWorkspace.ManagedWindows.FirstOrDefault();
            var managedWindows = currentWorkspace.ManagedWindows;
            var nWindows = managedWindows.Count();
            var maxWidth = GetTitleMaxWidth(currentWorkspace.ManagedWindows.Count());
            /*
            // TODO: seems to be dpi - values calculated are seem when it works vs when it doesn't, only difference is which monitor is focused
            Logger.Info($"Updating title widget on {currentMonitor.Index}, workspace {currentWorkspace.Name} and focused: {focusedWindow.Title}");
            Logger.Info($"n windows: {nWindows} Calculated max width: {maxWidth}, monitor width: {Context.Monitor.Width}, offset: {OtherWidgetOffset}");
            Logger.Info($"total width: {maxWidth*nWindows}, effective bar width: {Context.Monitor.Width - OtherWidgetOffset}, %: {((maxWidth*nWindows)/(Context.Monitor.Width-OtherWidgetOffset)) * 100}");
            Logger.Info($"Windows are: {currentWorkspace.ManagedWindows.Aggregate("",(accum, window) => { return $"{accum},{Environment.NewLine}|{window.Title}|"; })}");
            */
            // TODO: focus on non-main screens happens, but action isn't right
            // TODO: on non main screen, don't highlight background at all (even on focused one)
            // TODO: on non main screen, make sure width scales correctly

            // TODO: feed callback that switches to that window from context
            // TODO: implement focus window given int
            // TODO: match on window title
            return managedWindows
                .Select((window) => (
                    window.Handle == focusedWindow?.Handle
                    && Context.MonitorContainer.FocusedMonitor == Context.Monitor
                    ))
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
                        window.Focus,
                        maxWidth: GetTitleMaxWidth(nWindows)))
                .ToArray();
        }

        private int GetTitleMaxWidth(int nWindows)
        {
            // TODO: base logic on whether i'm focused monitor or not? hack, refine
            // maybe resolutions/landscape -- make this configurable? focused offset, non-focused offset
            // if i'm focused monitor, scale back even more (double offset)?
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
            // onClick switch callbacks mean that the updated window can be on any workspace now
            // need to update title of the src bar as well so can't filter out
            Context.MarkDirty();
            // TODO: move this into better location
            Cursor.Position =  new Point(window.Location.X + (window.Location.Width / 2), window.Location.Y + (window.Location.Height / 2));
        }

        private void RefreshWindowMoved(IWindow window, IWorkspace oldworkspace, IWorkspace newworkspace)
        {
            Context.MarkDirty();
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
