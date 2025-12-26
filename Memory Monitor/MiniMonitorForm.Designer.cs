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
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ramGauge = new Memory_Monitor.CompactGaugeControl();
            this.cpuGauge = new Memory_Monitor.CompactGaugeControl();
            this.gpuUsageGauge = new Memory_Monitor.CompactGaugeControl();
            this.gpuVramGauge = new Memory_Monitor.CompactGaugeControl();
            this.diskGauge = new Memory_Monitor.CompactGaugeControl();
            this.networkGauge = new Memory_Monitor.CompactGaugeControl();
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
            this.exitToolStripMenuItem});
            this.trayContextMenu.Name = "trayContextMenu";
            this.trayContextMenu.Size = new System.Drawing.Size(104, 54);
            // 
            // showToolStripMenuItem
            // 
            this.showToolStripMenuItem.Name = "showToolStripMenuItem";
            this.showToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.showToolStripMenuItem.Text = "Show";
            this.showToolStripMenuItem.Click += new System.EventHandler(this.ShowToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(100, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
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
            // MiniMonitorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(25, 28, 32);
            this.ClientSize = new System.Drawing.Size(1920, 480);
            this.Controls.Add(this.networkGauge);
            this.Controls.Add(this.diskGauge);
            this.Controls.Add(this.gpuVramGauge);
            this.Controls.Add(this.gpuUsageGauge);
            this.Controls.Add(this.cpuGauge);
            this.Controls.Add(this.ramGauge);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MiniMonitorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "System Monitor";
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
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private CompactGaugeControl ramGauge;
        private CompactGaugeControl cpuGauge;
        private CompactGaugeControl gpuUsageGauge;
        private CompactGaugeControl gpuVramGauge;
        private CompactGaugeControl diskGauge;
        private CompactGaugeControl networkGauge;
    }
}
