namespace Memory_Monitor
{
    partial class MiniMonitorForm
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
            this.fpsDisplayModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fpsAutoDetectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fpsAlwaysShowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fpsAlwaysHideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ramGauge = new Memory_Monitor.CompactGaugeControl();
            this.cpuGauge = new Memory_Monitor.CompactGaugeControl();
            this.gpuUsageGauge = new Memory_Monitor.CompactGaugeControl();
            this.gpuVramGauge = new Memory_Monitor.CompactGaugeControl();
            this.diskGauge = new Memory_Monitor.CompactGaugeControl();
            this.networkGauge = new Memory_Monitor.CompactGaugeControl();
            this.fpsGauge = new Memory_Monitor.FpsGaugeControl();
            this.lblDate = new System.Windows.Forms.Label();
            this.lblTime = new System.Windows.Forms.Label();
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
            this.notifyIcon.Text = "System Monitor";
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
            this.fpsDisplayModeToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.trayContextMenu.Name = "trayContextMenu";
            this.trayContextMenu.Size = new System.Drawing.Size(180, 120);
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
            // ramGauge
            // 
            this.ramGauge.BackColor = System.Drawing.Color.Transparent;
            this.ramGauge.GaugeColor = System.Drawing.Color.FromArgb(58, 150, 221);
            this.ramGauge.Label = "RAM";
            this.ramGauge.Location = new System.Drawing.Point(20, 10);
            this.ramGauge.MaxValue = 100F;
            this.ramGauge.MinimumSize = new System.Drawing.Size(120, 120);
            this.ramGauge.Name = "ramGauge";
            this.ramGauge.Size = new System.Drawing.Size(300, 460);
            this.ramGauge.TabIndex = 0;
            this.ramGauge.Unit = "%";
            // 
            // cpuGauge
            // 
            this.cpuGauge.BackColor = System.Drawing.Color.Transparent;
            this.cpuGauge.GaugeColor = System.Drawing.Color.FromArgb(220, 50, 50);
            this.cpuGauge.Label = "CPU";
            this.cpuGauge.Location = new System.Drawing.Point(320, 10);
            this.cpuGauge.MaxValue = 100F;
            this.cpuGauge.MinimumSize = new System.Drawing.Size(120, 120);
            this.cpuGauge.Name = "cpuGauge";
            this.cpuGauge.Size = new System.Drawing.Size(300, 460);
            this.cpuGauge.TabIndex = 1;
            this.cpuGauge.Unit = "%";
            // 
            // gpuUsageGauge
            // 
            this.gpuUsageGauge.BackColor = System.Drawing.Color.Transparent;
            this.gpuUsageGauge.GaugeColor = System.Drawing.Color.FromArgb(255, 140, 0);
            this.gpuUsageGauge.Label = "GPU";
            this.gpuUsageGauge.Location = new System.Drawing.Point(620, 10);
            this.gpuUsageGauge.MaxValue = 100F;
            this.gpuUsageGauge.MinimumSize = new System.Drawing.Size(120, 120);
            this.gpuUsageGauge.Name = "gpuUsageGauge";
            this.gpuUsageGauge.Size = new System.Drawing.Size(300, 460);
            this.gpuUsageGauge.TabIndex = 2;
            this.gpuUsageGauge.Unit = "%";
            // 
            // gpuVramGauge
            // 
            this.gpuVramGauge.BackColor = System.Drawing.Color.Transparent;
            this.gpuVramGauge.GaugeColor = System.Drawing.Color.FromArgb(160, 90, 240);
            this.gpuVramGauge.Label = "VRAM";
            this.gpuVramGauge.Location = new System.Drawing.Point(920, 10);
            this.gpuVramGauge.MaxValue = 24F;
            this.gpuVramGauge.MinimumSize = new System.Drawing.Size(120, 120);
            this.gpuVramGauge.Name = "gpuVramGauge";
            this.gpuVramGauge.Size = new System.Drawing.Size(300, 460);
            this.gpuVramGauge.TabIndex = 3;
            this.gpuVramGauge.Unit = "GB";
            // 
            // diskGauge
            // 
            this.diskGauge.BackColor = System.Drawing.Color.Transparent;
            this.diskGauge.GaugeColor = System.Drawing.Color.FromArgb(50, 200, 80);
            this.diskGauge.Label = "DISK";
            this.diskGauge.Location = new System.Drawing.Point(1220, 10);
            this.diskGauge.MaxValue = 500F;
            this.diskGauge.MinimumSize = new System.Drawing.Size(120, 120);
            this.diskGauge.Name = "diskGauge";
            this.diskGauge.Size = new System.Drawing.Size(300, 460);
            this.diskGauge.TabIndex = 4;
            this.diskGauge.Unit = "MB/s";
            // 
            // networkGauge
            // 
            this.networkGauge.BackColor = System.Drawing.Color.Transparent;
            this.networkGauge.GaugeColor = System.Drawing.Color.FromArgb(255, 200, 50);
            this.networkGauge.Label = "NET";
            this.networkGauge.Location = new System.Drawing.Point(1520, 10);
            this.networkGauge.MaxValue = 1000F;
            this.networkGauge.MinimumSize = new System.Drawing.Size(120, 120);
            this.networkGauge.Name = "networkGauge";
            this.networkGauge.Size = new System.Drawing.Size(300, 460);
            this.networkGauge.TabIndex = 5;
            this.networkGauge.Unit = "Mbps";
            // 
            // fpsGauge
            // 
            this.fpsGauge.BackColor = System.Drawing.Color.Transparent;
            this.fpsGauge.GaugeColor = System.Drawing.Color.FromArgb(0, 200, 150);
            this.fpsGauge.Location = new System.Drawing.Point(820, 150);
            this.fpsGauge.MinimumSize = new System.Drawing.Size(80, 80);
            this.fpsGauge.Name = "fpsGauge";
            this.fpsGauge.Size = new System.Drawing.Size(180, 180);
            this.fpsGauge.TabIndex = 6;
            this.fpsGauge.Visible = false;
            // 
            // lblDate
            // 
            this.lblDate.BackColor = System.Drawing.Color.Transparent;
            this.lblDate.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.lblDate.ForeColor = System.Drawing.Color.White;
            this.lblDate.Location = new System.Drawing.Point(15, 15);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(280, 40);
            this.lblDate.TabIndex = 7;
            this.lblDate.Text = "";
            this.lblDate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTime
            // 
            this.lblTime.BackColor = System.Drawing.Color.Transparent;
            this.lblTime.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.lblTime.ForeColor = System.Drawing.Color.White;
            this.lblTime.Location = new System.Drawing.Point(1625, 15);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(280, 40);
            this.lblTime.TabIndex = 8;
            this.lblTime.Text = "";
            this.lblTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // MiniMonitorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(25, 28, 32);
            this.ClientSize = new System.Drawing.Size(1920, 480);
            this.Controls.Add(this.fpsGauge);
            this.Controls.Add(this.lblTime);
            this.Controls.Add(this.lblDate);
            this.Controls.Add(this.networkGauge);
            this.Controls.Add(this.diskGauge);
            this.Controls.Add(this.gpuVramGauge);
            this.Controls.Add(this.gpuUsageGauge);
            this.Controls.Add(this.cpuGauge);
            this.Controls.Add(this.ramGauge);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MiniMonitorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "System Monitor";
            this.TopMost = true;
            this.Resize += new System.EventHandler(this.MiniMonitorForm_Resize);
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
        private System.Windows.Forms.ToolStripMenuItem fpsDisplayModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fpsAutoDetectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fpsAlwaysShowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fpsAlwaysHideToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private CompactGaugeControl ramGauge;
        private CompactGaugeControl cpuGauge;
        private CompactGaugeControl gpuUsageGauge;
        private CompactGaugeControl gpuVramGauge;
        private CompactGaugeControl diskGauge;
        private CompactGaugeControl networkGauge;
        private FpsGaugeControl fpsGauge;
        private System.Windows.Forms.Label lblDate;
        private System.Windows.Forms.Label lblTime;
    }
}
