﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace workspacer.ActionMenu
{
    public partial class ActionMenuForm : Form
    {
        private static Logger Logger = Logger.Create();
        private IConfigContext _context;
        private ActionMenuPluginConfig _config;

        private string _message;
        private bool _freeform;
        private ActionMenuItem[] _items;

        public ActionMenuForm(IConfigContext context, ActionMenuPluginConfig config)
        {
            _context = context;
            _config = config;

            InitializeComponent();

            this.Shown += OnLoad;
            this.textBox.KeyPress += OnKeyPress;
            this.KeyPress += OnKeyPress;
            this.textBox.KeyDown += OnKeyDown;
            this.KeyDown += OnKeyDown;
            this.textBox.TextChanged += OnTextChanged;
            this.listBox.GotFocus += OnGotFocus;
            this.textBox.LostFocus += OnLostFocus;

            this.TopMost = true;
            this.ControlBox = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = System.Drawing.Color.LimeGreen;
            this.TransparencyKey = System.Drawing.Color.LimeGreen;
            this.Text = config.MenuTitle;
            this.Width = config.MenuWidth;
            this.MinimumSize = new Size(config.MenuWidth, config.MenuHeight);
            // TODO: get rid of magic number
            this.MaximumSize = new Size(config.MenuWidth, 100000);
            // this.MaximumSize = new Size(_context.MonitorContainer.FocusedMonitor.Width, _context.MonitorContainer.FocusedMonitor.Height);
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            this.label.Text = "";
            this.label.BackColor = ColorToColor(config.Background);
            this.label.ForeColor = ColorToColor(config.Foreground);
            this.textBox.BackColor = ColorToColor(config.Background);
            this.textBox.ForeColor = ColorToColor(config.Foreground);
            this.listBox.BackColor = ColorToColor(config.Background);
            this.listBox.ForeColor = ColorToColor(config.Foreground);
            // TODO: get rid of magic number
            this.listBox.MaximumSize = new Size(config.MenuWidth, 100000);

            this.textBox.AutoSize = true;
            this.listBox.AutoSize = true;

            this.listBox.IntegralHeight = true;

            this.label.Font = new Font(config.FontName, config.FontSize, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.textBox.Font = new Font(config.FontName, config.FontSize, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.listBox.Font = new Font(config.FontName, config.FontSize, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));

            this.listBox.DisplayMember = "Text";
            this.listBox.ValueMember = "Text";
        }

        public void SetItems(string message, ActionMenuItem[] items)
        {
            this.textBox.Text = "";
            _message = message;
            _items = items;
            _freeform = false;
            ApplyFilter();
        }

        public void SetFreeForm(string message, Action<string> callback)
        {
            this.textBox.Text = "";
            _message = message;
            _items = new ActionMenuItem[] {
                new ActionMenuItem(message, () => callback(this.textBox.Text))
            };
            _freeform = true;
            ApplyFilter();
        }

        private System.Drawing.Color ColorToColor(Color color)
        {
            return System.Drawing.Color.FromArgb(color.R, color.G, color.B);
        }

        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)27)
            {
                Cleanup();
                e.Handled = true;
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Down)
            {
                SelectNext();
                e.SuppressKeyPress = true;
            } else if (e.KeyCode == System.Windows.Forms.Keys.Up)
            {
                SelectPrevious();
                e.SuppressKeyPress = true;
            } else if (e.KeyCode == System.Windows.Forms.Keys.Enter)
            {
                CommitSelection();
            }
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            var monitor = _context.MonitorContainer.FocusedMonitor;
            var width = this.ClientRectangle.Width;
            // this.Location = new Point(Scale(monitor.X) + Scale(monitor.Width / 2) - Scale(width / 2), 0);
            // this.Location = new Point(monitor.X, 0);
            this.Location = new Point(monitor.X + GetDiff(monitor.Width, width), 0);
            this.textBox.Text = "";

            this.TopMost = true;
            this.ActiveControl = this.textBox;
            this.textBox.Focus();

            ApplyFilter();
            this.listBox.SelectedIndex = 0;
        }

        private void OnLostFocus(object sender, EventArgs e)
        {
            Cleanup();
        }

        private void OnGotFocus(object sender, EventArgs e)
        {
            FixLayout();
        }

        private void SelectNext()
        {
            if (this.listBox.SelectedIndex < this.listBox.Items.Count - 1)
                this.listBox.SelectedIndex++;
            // Logger.Info($"*** current selected text is: {(this.listBox.Items[this.listBox.SelectedIndex] as ActionMenuItem)?.Text}");
        }

        private void SelectPrevious()
        {
            if (this.listBox.SelectedIndex > 0)
                this.listBox.SelectedIndex--;
        }

        private void CommitSelection()
        {
            ActionMenuItem item;
            if (_freeform)
            {
                item = _items[0];
            } else
            {
                item = this.listBox.SelectedItem as ActionMenuItem;
            }

            Cleanup();

            if (item != null)
            {
                item.Callback();
            }
        }

        private void ApplyFilter()
        {
            this.label.Text = _message;

            if (_items == null || _freeform)
            {
                this.listBox.Items.Clear();
                FixLayout();
                return;
            }

            var query = this.textBox.Text;
            var matchedItems = _items.Where(item => _config.Matcher.Match(query, item.Text) != null).ToList();

            int i;
            for (i = 0; i < matchedItems.Count; i++)
            {
                if (i < this.listBox.Items.Count)
                {
                    this.listBox.Items[i] = matchedItems[i];
                } else
                {
                    this.listBox.Items.Add(matchedItems[i]);
                }
            }

            var remaining = this.listBox.Items.Count - i;
            for (i = 0; i < remaining; i++)
            {
                this.listBox.Items.RemoveAt(this.listBox.Items.Count - 1);
            }

            if (this.listBox.SelectedIndex == -1 && this.listBox.Items.Count > 0)
            {
                this.listBox.SelectedIndex = 0;
            }
            FixLayout();
        }

        private void FixLayout()
        {
            var width = this.ClientRectangle.Width;
            this.label.Width = this.textBox.Width;
            this.label.Height = this.textBox.Height;
            this.label.Visible = this.label.Text != "";


            var labelHeight = this.label.Text != "" ? this.label.Height : 0;

            var monitor = _context.MonitorContainer.FocusedMonitor;
            this.listBox.Height = this.listBox.ItemHeight * this.listBox.Items.Count;

            this.textBox.Location = new Point(0, labelHeight);
            this.listBox.Location = new Point(0, this.textBox.Height + labelHeight);
            this.listBox.Visible = this.listBox.Items.Count > 0;
            // this.Location = new Point(monitor.X, monitor.Y);
            this.Location = new Point(monitor.X + GetDiff(monitor.Width, width), monitor.Y);
            this.listBox.Refresh();
            this.textBox.Refresh();
            this.label.Refresh();
        }

        private int GetDiff(int effectiveMonitorWidth, int effectiveFormWidth)
        {
            // Logger.Info($"~~~~ calc diff: half mon length: {effectiveMonitorWidth} half form length: {effectiveFormWidth} ~~~");
            var monitor = _context.MonitorContainer.FocusedMonitor;
            // Logger.Info($"~~~~ monitor stats: {monitor.Width}x{monitor.Height} | dpi: {monitor.Dpi.X} ref dpi: {monitor.MainDpi.X}");
            return effectiveMonitorWidth - effectiveFormWidth > 0
                ? (effectiveMonitorWidth - effectiveFormWidth) / 2
                : 0;
        }

        private void Cleanup()
        {
            ApplyFilter();
            Hide();
        }
    }
}
