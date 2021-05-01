using System;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;

namespace workspacer.Bar
{
    public partial class BarForm : Form
    {
        private IMonitor _monitor;
        private BarPluginConfig _config;
        private System.Timers.Timer _timer;

        private FlowLayoutPanel leftPanel;
        private FlowLayoutPanel rightPanel;
        private FlowLayoutPanel midPanel;

        private BarSection _left;
        private BarSection _right;
        private BarSection _mid;

        public BarForm(IMonitor monitor, BarPluginConfig config)
        {
            _monitor = monitor;
            _config = config;
            _timer = new System.Timers.Timer(50);
            _timer.Elapsed += Redraw;

            this.Text = config.BarTitle;
            this.ControlBox = false;
            this.FormBorderStyle = FormBorderStyle.None;

            this.BackColor = ColorToColor(config.DefaultWidgetBackground);

            this.Load += OnLoad;

            InitializeComponent();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // turn on WS_EX_TOOLWINDOW style bit
                cp.ExStyle |= 0x80;
                return cp;
            }
        }

        public void Initialize(IBarWidget[] left, IBarWidget[] mid, IBarWidget[] right, IConfigContext context)
        {
            _left = new BarSection(false, leftPanel, left, _monitor, context,
                _config.DefaultWidgetForeground, _config.DefaultWidgetBackground, _config.FontName, _config.FontSize);
            _right = new BarSection(true, rightPanel, right, _monitor, context,
                _config.DefaultWidgetForeground, _config.DefaultWidgetBackground, _config.FontName, _config.FontSize);
            _mid = new BarSection(false, midPanel, mid, _monitor, context,
                _config.DefaultWidgetForeground, _config.DefaultWidgetBackground, _config.FontName, _config.FontSize);
        }

        private System.Drawing.Color ColorToColor(Color color)
        {
            return System.Drawing.Color.FromArgb(color.R, color.G, color.B);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            this.Height = _config.BarHeight;
            // TODO: test changing height to see if we can move to bottom
            var titleBarHeight = this.ClientRectangle.Height - this.Height;
            // changes location successfully - how to move stuff around it though?
            // this.Location = new Point(_monitor.X, _monitor.Y + _monitor.Height - this.Height);
            this.Location = new Point(_monitor.X, _monitor.Y - titleBarHeight);
            _timer.Enabled = true;

            this.Width = _monitor.Width;
        }

        private void InitializeComponent()
        {
            this.leftPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.rightPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.midPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            //
            // leftPanel
            //
            this.leftPanel.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top |
                    System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)));
            this.leftPanel.AutoSize = true;
            this.leftPanel.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (0)))), ((int) (((byte) (0)))),
                ((int) (((byte) (0)))), ((int) (((byte) (0)))));
            this.leftPanel.Location = new System.Drawing.Point(0, 0);
            this.leftPanel.Margin = new System.Windows.Forms.Padding(0);
            this.leftPanel.Name = "leftPanel";
            this.leftPanel.Size = new System.Drawing.Size(50, 50);
            this.leftPanel.TabIndex = 0;
            this.leftPanel.WrapContents = false;


            //
            // rightPanel
            //
            this.rightPanel.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top |
                    System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.rightPanel.AutoSize = true;
            this.rightPanel.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (0)))), ((int) (((byte) (0)))),
                ((int) (((byte) (0)))), ((int) (((byte) (0)))));
            this.rightPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.rightPanel.Location = new System.Drawing.Point(Width - 50, 0);
            this.rightPanel.Margin = new System.Windows.Forms.Padding(0);
            this.rightPanel.Name = "rightPanel";
            this.rightPanel.Size = new System.Drawing.Size(50, 50);
            this.rightPanel.TabIndex = 2;
            this.rightPanel.WrapContents = false;


            //
            // midPanel
            //
            this.midPanel.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top |
                    System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)));
            this.midPanel.AutoSize = false;
            this.midPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.midPanel.Location = new System.Drawing.Point(50, 0);
            this.midPanel.Margin = new System.Windows.Forms.Padding(0);
            this.midPanel.Name = "midPanel";
            this.midPanel.Size = new System.Drawing.Size(50, 50);
            this.midPanel.TabIndex = 1;
            this.midPanel.WrapContents = false;

            //
            // BarForm
            //
            this.ClientSize = new System.Drawing.Size(Width, 50);
            this.Controls.Add(this.leftPanel);
            this.Controls.Add(this.midPanel);
            this.Controls.Add(this.rightPanel);
            this.Name = "BarForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void Redraw(object sender, ElapsedEventArgs args)
        {
            try
            {
                if (IsHandleCreated)
                {
                    Invoke((MethodInvoker)(() =>
                    {
                        _left.Draw();
                        _right.Draw();
                        // TODO: refactor into nice private methods
                        var rightWidth = _monitor.Width - rightPanel.Left;
                        var newWidth = _monitor.Width - leftPanel.Right - rightWidth;

                        SuspendLayout();
                        midPanel.Location = new Point(leftPanel.Right, 0);
                        midPanel.Width = newWidth;
                        ResumeLayout();
                        _mid.DrawWithSize(newWidth);
                    }));
                }
            }
            catch (ObjectDisposedException)
            {
                // Sometimes after waking from sleep, BarForm has been disposed of.
            }
        }
    }
}
