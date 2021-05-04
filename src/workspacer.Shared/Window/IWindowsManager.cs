
using System.Collections.Generic;

namespace workspacer
{
    public delegate void WindowFocusDelegate(IWindow window);

    public interface IWindowsManager
    {
        IWindowsDeferPosHandle DeferWindowsPos(int count, IEnumerable<IWindow> focusStealingWindows);
        void DumpWindowDebugOutput();
        void DumpWindowUnderCursorDebugOutput();

        event WindowFocusDelegate WindowFocused;

        void ToggleFocusedWindowTiling(); // mod-t
    }
}
