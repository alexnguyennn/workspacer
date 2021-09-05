using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace workspacer.ActionMenu
{
    public class ActionMenuPlugin : IPlugin
    {
        private IConfigContext _context;
        private ActionMenuForm _menu;
        private ActionMenuPluginConfig _config;

        public ActionMenuItemBuilder DefaultMenu { get; private set; }

        public ActionMenuPlugin() : this(new ActionMenuPluginConfig()) { }

        public ActionMenuPlugin(ActionMenuPluginConfig config)
        {
            _config = config;

            DefaultMenu = CreateDefault();
        }

        public void AfterConfig(IConfigContext context)
        {
            _context = context;
            _menu = new ActionMenuForm(context, _config);

            if (_config.RegisterKeybind)
            {
                _context.Keybinds.Subscribe(_config.KeybindMod, _config.KeybindKey, () => ShowDefault(), "open action menu");
            }

            if (!(_config.AfterConfigDelegate is null))
                _config.AfterConfigDelegate(this);
        }

        public void ShowMenu(string message, ActionMenuItemBuilder builder)
        {
            _menu.SetItems(message, builder.Get());
            _menu.Show();
            // _menu.Activate();
            // _menu.Handle
            // TODO: add toggle for which one to use
            Win32Helper.ForceForegroundWindow(_menu.Handle);
        }

        public void ShowMenu(ActionMenuItemBuilder builder)
        {
            _menu.SetItems("", builder.Get());
            _menu.Show();
            // _menu.Activate();
            Win32Helper.ForceForegroundWindow(_menu.Handle);
        }

        public void ShowFreeForm(string message, Action<string> callback)
        {
            _menu.SetFreeForm(message, callback);
            _menu.Show();
            _menu.Activate();
        }

        public ActionMenuItemBuilder Create()
        {
            return new ActionMenuItemBuilder(this);
        }

        public void ShowDefault()
        {
            ShowMenu(DefaultMenu);
        }

        private ActionMenuItemBuilder CreateDefault()
        {
            return new ActionMenuItemBuilder(this)
                .Add("restart workspacer", () => _context.Restart())
                .Add("quit workspacer", () => _context.Quit(false))
                .AddMenu("switch to window", () => CreateSwitchToWindowMenu(_context));
        }

        // consider making a similar constructor to ShowDefault and keep this private
        public ActionMenuItemBuilder CreateSwitchToWindowMenu(IConfigContext context)
        {
            var builder = Create();
            var workspaces = context.WorkspaceContainer.GetAllWorkspaces();
            foreach (var workspace in workspaces)
            {
                foreach (var window in workspace.ManagedWindows)
                {
                    // TODO: maybe add setting to configure this? maybe not?
                    var fullText = $"[{workspace.Name}] {window.Title}";
                    // var text = fullText.Substring(0, fullText.Length > 100 ? 100 : fullText.Length);
                    var text = fullText;
                    builder.Add(text, () => context.Workspaces.SwitchToWindow(window));
                }
            }
            return builder;
        }
    }
}
