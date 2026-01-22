namespace Memory_Monitor
{
    partial class BarGraphDisplayForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.trayContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.moveToMonitorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.topMostToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displayModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.circularGaugesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.barGraphToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fpsDisplayModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fpsAutoDetectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fpsAlwaysShowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fpsAlwaysHideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cpuPanel = new Memory_Monitor.BarGraphPanelControl();
            this.gpuPanel = new Memory_Monitor.BarGraphPanelControl();
            this.vramPanel = new Memory_Monitor.BarGraphPanelControl();
            this.drivePanel = new Memory_Monitor.BarGraphPanelControl();
            this.networkPanel = new Memory_Monitor.NetworkBarPanelControl();
            this.trayContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // updateTimer
            // 
            this.updateTimer.Interval = 1000;
            this.updateTimer.Tick += new System.EventHandler(this.UpdateTimer_Tick);
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.trayContextMenu;
            this.notifyIcon.Text = "Memory Monitor - Bar Graph";
            this.notifyIcon.Visible = true;
            this.notifyIcon.DoubleClick += new System.EventHandler(this.NotifyIcon_DoubleClick);
            // 
            // trayContextMenu
            // 
            this.trayContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showToolStripMenuItem,
            this.toolStripSeparator1,
            this.moveToMonitorToolStripMenuItem,
            this.topMostToolStripMenuItem,
            this.displayModeToolStripMenuItem,
            this.fpsDisplayModeToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.trayContextMenu.Name = "trayContextMenu";
            this.trayContextMenu.Size = new System.Drawing.Size(180, 148);
            // 
            // showToolStripMenuItem
            // 
            this.showToolStripMenuItem.Name = "showToolStripMenuItem";
            this.showToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.showToolStripMenuItem.Text = "Show";
            this.showToolStripMenuItem.Click += new System.EventHandler(this.ShowToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(176, 6);
            // 
            // moveToMonitorToolStripMenuItem
            // 
            this.moveToMonitorToolStripMenuItem.Name = "moveToMonitorToolStripMenuItem";
            this.moveToMonitorToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.moveToMonitorToolStripMenuItem.Text = "Move to Next Monitor";
            this.moveToMonitorToolStripMenuItem.Click += new System.EventHandler(this.MoveToMonitorToolStripMenuItem_Click);
            // 
            // topMostToolStripMenuItem
            // 
            this.topMostToolStripMenuItem.Name = "topMostToolStripMenuItem";
            this.topMostToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.topMostToolStripMenuItem.Text = "Always on Top";
            this.topMostToolStripMenuItem.Checked = true;
            this.topMostToolStripMenuItem.CheckOnClick = true;
            this.topMostToolStripMenuItem.Click += new System.EventHandler(this.TopMostToolStripMenuItem_Click);
            // 
            // displayModeToolStripMenuItem
            // 
            this.displayModeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.circularGaugesToolStripMenuItem,
            this.barGraphToolStripMenuItem});
            this.displayModeToolStripMenuItem.Name = "displayModeToolStripMenuItem";
            this.displayModeToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.displayModeToolStripMenuItem.Text = "Display Mode";
            // 
            // circularGaugesToolStripMenuItem
            // 
            this.circularGaugesToolStripMenuItem.Name = "circularGaugesToolStripMenuItem";
            this.circularGaugesToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.circularGaugesToolStripMenuItem.Text = "Circular Gauges";
            this.circularGaugesToolStripMenuItem.Click += new System.EventHandler(this.CircularGaugesToolStripMenuItem_Click);
            // 
            // barGraphToolStripMenuItem
            // 
            this.barGraphToolStripMenuItem.Name = "barGraphToolStripMenuItem";
            this.barGraphToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.barGraphToolStripMenuItem.Text = "Bar Graph";
            this.barGraphToolStripMenuItem.Checked = true;
            this.barGraphToolStripMenuItem.Click += new System.EventHandler(this.BarGraphToolStripMenuItem_Click);
            // 
            // fpsDisplayModeToolStripMenuItem
            // 
            this.fpsDisplayModeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fpsAutoDetectToolStripMenuItem,
            this.fpsAlwaysShowToolStripMenuItem,
            this.fpsAlwaysHideToolStripMenuItem});
            this.fpsDisplayModeToolStripMenuItem.Name = "fpsDisplayModeToolStripMenuItem";
            this.fpsDisplayModeToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.fpsDisplayModeToolStripMenuItem.Text = "FPS Display";
            // 
            // fpsAutoDetectToolStripMenuItem
            // 
            this.fpsAutoDetectToolStripMenuItem.Name = "fpsAutoDetectToolStripMenuItem";
            this.fpsAutoDetectToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.fpsAutoDetectToolStripMenuItem.Text = "Auto-detect";
            this.fpsAutoDetectToolStripMenuItem.Checked = true;
            this.fpsAutoDetectToolStripMenuItem.Click += new System.EventHandler(this.FpsAutoDetectToolStripMenuItem_Click);
            // 
            // fpsAlwaysShowToolStripMenuItem
            // 
            this.fpsAlwaysShowToolStripMenuItem.Name = "fpsAlwaysShowToolStripMenuItem";
            this.fpsAlwaysShowToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.fpsAlwaysShowToolStripMenuItem.Text = "Always Show";
            this.fpsAlwaysShowToolStripMenuItem.Click += new System.EventHandler(this.FpsAlwaysShowToolStripMenuItem_Click);
            // 
            // fpsAlwaysHideToolStripMenuItem
            // 
            this.fpsAlwaysHideToolStripMenuItem.Name = "fpsAlwaysHideToolStripMenuItem";
            this.fpsAlwaysHideToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.fpsAlwaysHideToolStripMenuItem.Text = "Always Hide";
            this.fpsAlwaysHideToolStripMenuItem.Click += new System.EventHandler(this.FpsAlwaysHideToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(176, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // cpuPanel
            // 
            this.cpuPanel.AccentColor = System.Drawing.Color.FromArgb(0, 180, 255);
            this.cpuPanel.BackColor = System.Drawing.Color.Transparent;
            this.cpuPanel.Location = new System.Drawing.Point(15, 10);
            this.cpuPanel.MinimumSize = new System.Drawing.Size(150, 150);
            this.cpuPanel.Name = "cpuPanel";
            this.cpuPanel.Size = new System.Drawing.Size(368, 460);
            this.cpuPanel.TabIndex = 0;
            this.cpuPanel.Title = "CPU USAGE";
            // 
            // gpuPanel
            // 
            this.gpuPanel.AccentColor = System.Drawing.Color.FromArgb(100, 200, 80);
            this.gpuPanel.BackColor = System.Drawing.Color.Transparent;
            this.gpuPanel.Location = new System.Drawing.Point(393, 10);
            this.gpuPanel.MinimumSize = new System.Drawing.Size(150, 150);
            this.gpuPanel.Name = "gpuPanel";
            this.gpuPanel.Size = new System.Drawing.Size(368, 460);
            this.gpuPanel.TabIndex = 1;
            this.gpuPanel.Title = "GPU USAGE";
            // 
            // vramPanel
            // 
            this.vramPanel.AccentColor = System.Drawing.Color.FromArgb(160, 90, 240);
            this.vramPanel.BackColor = System.Drawing.Color.Transparent;
            this.vramPanel.Location = new System.Drawing.Point(771, 10);
            this.vramPanel.MinimumSize = new System.Drawing.Size(150, 150);
            this.vramPanel.Name = "vramPanel";
            this.vramPanel.Size = new System.Drawing.Size(368, 460);
            this.vramPanel.TabIndex = 2;
            this.vramPanel.Title = "VRAM USAGE";
            // 
            // drivePanel
            // 
            this.drivePanel.AccentColor = System.Drawing.Color.FromArgb(255, 160, 50);
            this.drivePanel.BackColor = System.Drawing.Color.Transparent;
            this.drivePanel.Location = new System.Drawing.Point(1149, 10);
            this.drivePanel.MinimumSize = new System.Drawing.Size(150, 150);
            this.drivePanel.Name = "drivePanel";
            this.drivePanel.Size = new System.Drawing.Size(368, 460);
            this.drivePanel.TabIndex = 3;
            this.drivePanel.Title = "DRIVE USAGE";
            // 
            // networkPanel
            // 
            this.networkPanel.AccentColor = System.Drawing.Color.FromArgb(0, 200, 220);
            this.networkPanel.BackColor = System.Drawing.Color.Transparent;
            this.networkPanel.Location = new System.Drawing.Point(1527, 10);
            this.networkPanel.MinimumSize = new System.Drawing.Size(150, 150);
            this.networkPanel.Name = "networkPanel";
            this.networkPanel.Size = new System.Drawing.Size(378, 460);
            this.networkPanel.TabIndex = 4;
            this.networkPanel.Title = "NETWORK";
            // 
            // BarGraphDisplayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(15, 18, 22);
            this.ClientSize = new System.Drawing.Size(1920, 480);
            this.Controls.Add(this.networkPanel);
            this.Controls.Add(this.drivePanel);
            this.Controls.Add(this.vramPanel);
            this.Controls.Add(this.gpuPanel);
            this.Controls.Add(this.cpuPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BarGraphDisplayForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "System Monitor - Bar Graph";
            this.TopMost = true;
            this.Resize += new System.EventHandler(this.BarGraphDisplayForm_Resize);
            this.trayContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer updateTimer;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip trayContextMenu;
        private System.Windows.Forms.ToolStripMenuItem showToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem moveToMonitorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem topMostToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem displayModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem circularGaugesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem barGraphToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fpsDisplayModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fpsAutoDetectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fpsAlwaysShowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fpsAlwaysHideToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private BarGraphPanelControl cpuPanel;
        private BarGraphPanelControl gpuPanel;
        private BarGraphPanelControl vramPanel;
        private BarGraphPanelControl drivePanel;
        private NetworkBarPanelControl networkPanel;
    }
}
