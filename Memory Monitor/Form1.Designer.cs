namespace Memory_Monitor
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.lblProcessesTitle = new System.Windows.Forms.Label();
            this.listViewProcesses = new System.Windows.Forms.ListView();
            this.columnProcessName = new System.Windows.Forms.ColumnHeader();
            this.columnMemoryUsage = new System.Windows.Forms.ColumnHeader();
            this.columnMemoryMB = new System.Windows.Forms.ColumnHeader();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.darkModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.showDiskMonitorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showNetworkMonitorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.trayContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ramUsageGauge = new Memory_Monitor.EnhancedGaugeControl();
            this.cpuLoadGauge = new Memory_Monitor.EnhancedGaugeControl();
            this.diskUsageGauge = new Memory_Monitor.EnhancedGaugeControl();
            this.networkUsageGauge = new Memory_Monitor.EnhancedGaugeControl();
            this.gpuVramGauge = new Memory_Monitor.EnhancedGaugeControl();
            this.lblRamGauge = new System.Windows.Forms.Label();
            this.lblCpuGauge = new System.Windows.Forms.Label();
            this.lblDiskGauge = new System.Windows.Forms.Label();
            this.lblNetworkGauge = new System.Windows.Forms.Label();
            this.lblGpuGauge = new System.Windows.Forms.Label();
            this.menuStrip.SuspendLayout();
            this.trayContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(1000, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip1";
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.darkModeToolStripMenuItem,
            this.toolStripSeparator1,
            this.showDiskMonitorToolStripMenuItem,
            this.showNetworkMonitorToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // darkModeToolStripMenuItem
            // 
            this.darkModeToolStripMenuItem.CheckOnClick = true;
            this.darkModeToolStripMenuItem.Name = "darkModeToolStripMenuItem";
            this.darkModeToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.darkModeToolStripMenuItem.Text = "Dark Mode";
            this.darkModeToolStripMenuItem.Click += new System.EventHandler(this.DarkModeToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(197, 6);
            // 
            // showDiskMonitorToolStripMenuItem
            // 
            this.showDiskMonitorToolStripMenuItem.CheckOnClick = true;
            this.showDiskMonitorToolStripMenuItem.Checked = true;
            this.showDiskMonitorToolStripMenuItem.Name = "showDiskMonitorToolStripMenuItem";
            this.showDiskMonitorToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.showDiskMonitorToolStripMenuItem.Text = "Show Disk Monitor";
            this.showDiskMonitorToolStripMenuItem.Click += new System.EventHandler(this.ShowDiskMonitorToolStripMenuItem_Click);
            // 
            // showNetworkMonitorToolStripMenuItem
            // 
            this.showNetworkMonitorToolStripMenuItem.CheckOnClick = true;
            this.showNetworkMonitorToolStripMenuItem.Checked = true;
            this.showNetworkMonitorToolStripMenuItem.Name = "showNetworkMonitorToolStripMenuItem";
            this.showNetworkMonitorToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.showNetworkMonitorToolStripMenuItem.Text = "Show Network Monitor";
            this.showNetworkMonitorToolStripMenuItem.Click += new System.EventHandler(this.ShowNetworkMonitorToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // RAM Usage Gauge
            // 
            this.ramUsageGauge.BackColor = System.Drawing.Color.Transparent;
            this.ramUsageGauge.GaugeBackgroundColor = System.Drawing.Color.FromArgb(50, 55, 60);
            this.ramUsageGauge.GaugeColor = System.Drawing.Color.FromArgb(58, 150, 221);
            this.ramUsageGauge.NeedleColor = System.Drawing.Color.FromArgb(58, 150, 221);
            this.ramUsageGauge.Location = new System.Drawing.Point(30, 50);
            this.ramUsageGauge.MaxValue = 100F;
            this.ramUsageGauge.MinimumSize = new System.Drawing.Size(180, 180);
            this.ramUsageGauge.Name = "ramUsageGauge";
            this.ramUsageGauge.Size = new System.Drawing.Size(200, 200);
            this.ramUsageGauge.TabIndex = 1;
            this.ramUsageGauge.Unit = "%";
            // 
            // RAM Label
            // 
            this.lblRamGauge.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblRamGauge.Location = new System.Drawing.Point(30, 255);
            this.lblRamGauge.Name = "lblRamGauge";
            this.lblRamGauge.Size = new System.Drawing.Size(200, 25);
            this.lblRamGauge.TabIndex = 2;
            this.lblRamGauge.Text = "RAM USAGE";
            this.lblRamGauge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CPU Load Gauge
            // 
            this.cpuLoadGauge.BackColor = System.Drawing.Color.Transparent;
            this.cpuLoadGauge.GaugeBackgroundColor = System.Drawing.Color.FromArgb(50, 55, 60);
            this.cpuLoadGauge.GaugeColor = System.Drawing.Color.FromArgb(220, 50, 50);
            this.cpuLoadGauge.NeedleColor = System.Drawing.Color.FromArgb(220, 50, 50);
            this.cpuLoadGauge.Location = new System.Drawing.Point(270, 50);
            this.cpuLoadGauge.MaxValue = 100F;
            this.cpuLoadGauge.MinimumSize = new System.Drawing.Size(180, 180);
            this.cpuLoadGauge.Name = "cpuLoadGauge";
            this.cpuLoadGauge.Size = new System.Drawing.Size(200, 200);
            this.cpuLoadGauge.TabIndex = 3;
            this.cpuLoadGauge.Unit = "%";
            // 
            // CPU Label
            // 
            this.lblCpuGauge.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblCpuGauge.Location = new System.Drawing.Point(270, 255);
            this.lblCpuGauge.Name = "lblCpuGauge";
            this.lblCpuGauge.Size = new System.Drawing.Size(200, 25);
            this.lblCpuGauge.TabIndex = 4;
            this.lblCpuGauge.Text = "CPU LOAD";
            this.lblCpuGauge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Disk I/O Gauge
            // 
            this.diskUsageGauge.BackColor = System.Drawing.Color.Transparent;
            this.diskUsageGauge.GaugeBackgroundColor = System.Drawing.Color.FromArgb(50, 55, 60);
            this.diskUsageGauge.GaugeColor = System.Drawing.Color.FromArgb(50, 200, 80);
            this.diskUsageGauge.NeedleColor = System.Drawing.Color.FromArgb(50, 200, 80);
            this.diskUsageGauge.Location = new System.Drawing.Point(510, 50);
            this.diskUsageGauge.MaxValue = 500F;
            this.diskUsageGauge.MinimumSize = new System.Drawing.Size(180, 180);
            this.diskUsageGauge.Name = "diskUsageGauge";
            this.diskUsageGauge.Size = new System.Drawing.Size(200, 200);
            this.diskUsageGauge.TabIndex = 5;
            this.diskUsageGauge.Unit = "MB/s";
            // 
            // Disk Label
            // 
            this.lblDiskGauge.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblDiskGauge.Location = new System.Drawing.Point(510, 255);
            this.lblDiskGauge.Name = "lblDiskGauge";
            this.lblDiskGauge.Size = new System.Drawing.Size(200, 25);
            this.lblDiskGauge.TabIndex = 6;
            this.lblDiskGauge.Text = "DISK I/O";
            this.lblDiskGauge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Network/Ethernet Gauge
            // 
            this.networkUsageGauge.BackColor = System.Drawing.Color.Transparent;
            this.networkUsageGauge.GaugeBackgroundColor = System.Drawing.Color.FromArgb(50, 55, 60);
            this.networkUsageGauge.GaugeColor = System.Drawing.Color.FromArgb(255, 200, 50);
            this.networkUsageGauge.NeedleColor = System.Drawing.Color.FromArgb(255, 200, 50);
            this.networkUsageGauge.Location = new System.Drawing.Point(150, 310);
            this.networkUsageGauge.MaxValue = 1000F;
            this.networkUsageGauge.MinimumSize = new System.Drawing.Size(180, 180);
            this.networkUsageGauge.Name = "networkUsageGauge";
            this.networkUsageGauge.Size = new System.Drawing.Size(200, 200);
            this.networkUsageGauge.TabIndex = 7;
            this.networkUsageGauge.Unit = "Mbps";
            // 
            // Network Label
            // 
            this.lblNetworkGauge.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblNetworkGauge.Location = new System.Drawing.Point(150, 515);
            this.lblNetworkGauge.Name = "lblNetworkGauge";
            this.lblNetworkGauge.Size = new System.Drawing.Size(200, 25);
            this.lblNetworkGauge.TabIndex = 8;
            this.lblNetworkGauge.Text = "ETHERNET";
            this.lblNetworkGauge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // GPU VRAM Gauge
            // 
            this.gpuVramGauge.BackColor = System.Drawing.Color.Transparent;
            this.gpuVramGauge.GaugeBackgroundColor = System.Drawing.Color.FromArgb(50, 55, 60);
            this.gpuVramGauge.GaugeColor = System.Drawing.Color.FromArgb(160, 90, 240);
            this.gpuVramGauge.NeedleColor = System.Drawing.Color.FromArgb(160, 90, 240);
            this.gpuVramGauge.Location = new System.Drawing.Point(400, 310);
            this.gpuVramGauge.MaxValue = 24F;
            this.gpuVramGauge.MinimumSize = new System.Drawing.Size(180, 180);
            this.gpuVramGauge.Name = "gpuVramGauge";
            this.gpuVramGauge.Size = new System.Drawing.Size(200, 200);
            this.gpuVramGauge.TabIndex = 9;
            this.gpuVramGauge.Unit = "GB";
            // 
            // GPU Label
            // 
            this.lblGpuGauge.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblGpuGauge.Location = new System.Drawing.Point(400, 515);
            this.lblGpuGauge.Name = "lblGpuGauge";
            this.lblGpuGauge.Size = new System.Drawing.Size(200, 25);
            this.lblGpuGauge.TabIndex = 10;
            this.lblGpuGauge.Text = "GPU VRAM";
            this.lblGpuGauge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // updateTimer
            // 
            this.updateTimer.Interval = 1000;
            this.updateTimer.Tick += new System.EventHandler(this.UpdateTimer_Tick);
            // 
            // lblProcessesTitle
            // 
            this.lblProcessesTitle.AutoSize = true;
            this.lblProcessesTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblProcessesTitle.Location = new System.Drawing.Point(20, 560);
            this.lblProcessesTitle.Name = "lblProcessesTitle";
            this.lblProcessesTitle.Size = new System.Drawing.Size(234, 21);
            this.lblProcessesTitle.TabIndex = 11;
            this.lblProcessesTitle.Text = "Processes Using > 400 MB RAM";
            // 
            // listViewProcesses
            // 
            this.listViewProcesses.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnProcessName,
            this.columnMemoryUsage,
            this.columnMemoryMB});
            this.listViewProcesses.FullRowSelect = true;
            this.listViewProcesses.Location = new System.Drawing.Point(20, 590);
            this.listViewProcesses.Name = "listViewProcesses";
            this.listViewProcesses.OwnerDraw = true;
            this.listViewProcesses.Size = new System.Drawing.Size(760, 200);
            this.listViewProcesses.TabIndex = 12;
            this.listViewProcesses.UseCompatibleStateImageBehavior = false;
            this.listViewProcesses.View = System.Windows.Forms.View.Details;
            this.listViewProcesses.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.ListView_DrawColumnHeader);
            this.listViewProcesses.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.ListView_DrawItem);
            this.listViewProcesses.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.ListView_DrawSubItem);
            // 
            // columnProcessName
            // 
            this.columnProcessName.Text = "Process Name";
            this.columnProcessName.Width = 300;
            // 
            // columnMemoryUsage
            // 
            this.columnMemoryUsage.Text = "Memory";
            this.columnMemoryUsage.Width = 150;
            // 
            // columnMemoryMB
            // 
            this.columnMemoryMB.Text = "MB";
            this.columnMemoryMB.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnMemoryMB.Width = 140;
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
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.trayContextMenu.Name = "trayContextMenu";
            this.trayContextMenu.Size = new System.Drawing.Size(153, 76);
            // 
            // showToolStripMenuItem
            // 
            this.showToolStripMenuItem.Name = "showToolStripMenuItem";
            this.showToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.showToolStripMenuItem.Text = "Show";
            this.showToolStripMenuItem.Click += new System.EventHandler(this.ShowToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(149, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 850);
            this.Controls.Add(this.lblGpuGauge);
            this.Controls.Add(this.lblNetworkGauge);
            this.Controls.Add(this.lblDiskGauge);
            this.Controls.Add(this.lblCpuGauge);
            this.Controls.Add(this.lblRamGauge);
            this.Controls.Add(this.gpuVramGauge);
            this.Controls.Add(this.networkUsageGauge);
            this.Controls.Add(this.diskUsageGauge);
            this.Controls.Add(this.cpuLoadGauge);
            this.Controls.Add(this.ramUsageGauge);
            this.Controls.Add(this.listViewProcesses);
            this.Controls.Add(this.lblProcessesTitle);
            this.Controls.Add(this.menuStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MainMenuStrip = this.menuStrip;
            this.MaximizeBox = true;
            this.MinimumSize = new System.Drawing.Size(800, 850);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "System Monitor";
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.trayContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Timer updateTimer;
        private System.Windows.Forms.Label lblProcessesTitle;
        private System.Windows.Forms.ListView listViewProcesses;
        private System.Windows.Forms.ColumnHeader columnProcessName;
        private System.Windows.Forms.ColumnHeader columnMemoryUsage;
        private System.Windows.Forms.ColumnHeader columnMemoryMB;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem darkModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem showDiskMonitorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showNetworkMonitorToolStripMenuItem;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip trayContextMenu;
        private System.Windows.Forms.ToolStripMenuItem showToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private EnhancedGaugeControl ramUsageGauge;
        private EnhancedGaugeControl cpuLoadGauge;
        private EnhancedGaugeControl diskUsageGauge;
        private EnhancedGaugeControl networkUsageGauge;
        private EnhancedGaugeControl gpuVramGauge;
        private System.Windows.Forms.Label lblRamGauge;
        private System.Windows.Forms.Label lblCpuGauge;
        private System.Windows.Forms.Label lblDiskGauge;
        private System.Windows.Forms.Label lblNetworkGauge;
        private System.Windows.Forms.Label lblGpuGauge;
    }
}
