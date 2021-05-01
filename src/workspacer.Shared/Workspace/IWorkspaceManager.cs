using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace workspacer
{
    public delegate void WorkspaceUpdatedDelegate();
    public delegate void FocusedMonitorUpdatedDelegate();
    public delegate void WindowAddedDelegate(IWindow window, IWorkspace workspace);
    public delegate void WindowUpdatedDelegate(IWindow window, IWorkspace workspace);
    public delegate void WindowRemovedDelegate(IWindow window, IWorkspace workspace);
    public delegate void WindowMovedDelegate(IWindow window, IWorkspace oldWorkspace, IWorkspace newWorkspace);

    public interface IWorkspaceManager
    {
        IWorkspace FocusedWorkspace { get; }

        void SwitchToWindow(IWindow window);
        void SwitchToWorkspace(int index);
        void SwitchToWorkspace(IWorkspace workspace);
        void SwitchToLastFocusedWorkspace();
        void SwitchMonitorToWorkspace(int monitorIndex, int workspaceIndex);
        void SwitchToNextWorkspace();
        void SwitchToPreviousWorkspace();
        void SwitchFocusedMonitor(int index);
        void SwitchFocusToNextMonitor();
        void SwitchFocusToPreviousMonitor();
        void SwitchFocusedMonitorToMouseLocation();
        void MoveFocusedWindowToWorkspace(int index);
        void MoveFocusedWindowAndSwitchToNextWorkspace();
        void MoveFocusedWindowAndSwitchToPreviousWorkspace();
        void MoveFocusedWindowToMonitor(int index);
        void MoveAllWindows(IWorkspace source, IWorkspace dest);
        void MoveFocusedWindowToNextMonitor();
        void MoveFocusedWindowToPreviousMonitor();

        /// <summary>
        /// on focused workspace
        /// swap the focus and primary windows and sends updated event
        /// </summary>
        void SwapFocusAndPrimaryWindow(); // mod-return

        /// <summary>
        /// on focused workspace
        /// swap the focus and next windows and sends updated event
        /// </summary>
        void SwapFocusAndNextWindow(); // mod-shift-j

        /// <summary>
        /// on focused workspace
        /// swap the focus and previous windows and sends updated event
        /// </summary>
        void SwapFocusAndPreviousWindow(); // mod-shift-k

        /// <summary>
        /// on focused workspace
        /// swap the specified window to a (x,y) point
        /// in the workspace and sends updated event
        /// </summary>
        /// <param name="window">window to swap</param>
        /// <param name="x">x coordinate of the point</param>
        /// <param name="y">y coordinate of the point</param>
        void SwapWindowToPoint(IWindow window, int x, int y);


        void ForceWorkspaceUpdate();

        event WorkspaceUpdatedDelegate WorkspaceUpdated;
        event WindowAddedDelegate WindowAdded;
        event WindowUpdatedDelegate WindowUpdated;
        event WindowRemovedDelegate WindowRemoved;
        event WindowMovedDelegate WindowMoved;
        event FocusedMonitorUpdatedDelegate FocusedMonitorUpdated;
    }
}
