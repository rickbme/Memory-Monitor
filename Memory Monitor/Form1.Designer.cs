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
            this.lblSystemMemoryTitle = new System.Windows.Forms.Label();
            this.lblSystemMemoryValue = new System.Windows.Forms.Label();
            this.progressBarSystemMemory = new System.Windows.Forms.ProgressBar();
            this.lblGPUMemoryTitle = new System.Windows.Forms.Label();
            this.lblGPUMemoryValue = new System.Windows.Forms.Label();
            this.progressBarGPUMemory = new System.Windows.Forms.ProgressBar();
            this.lblSystemMemoryPercent = new System.Windows.Forms.Label();
            this.lblGPUMemoryPercent = new System.Windows.Forms.Label();
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.lblProcessesTitle = new System.Windows.Forms.Label();
            this.listViewProcesses = new System.Windows.Forms.ListView();
            this.columnProcessName = new System.Windows.Forms.ColumnHeader();
            this.columnMemoryUsage = new System.Windows.Forms.ColumnHeader();
            this.columnMemoryMB = new System.Windows.Forms.ColumnHeader();
            this.systemMemoryGraph = new Memory_Monitor.MemoryGraphControl();
            this.gpuMemoryGraph = new Memory_Monitor.MemoryGraphControl();
            this.lblCPUUsageTitle = new System.Windows.Forms.Label();
            this.lblCPUUsageValue = new System.Windows.Forms.Label();
            this.cpuUsageGraph = new Memory_Monitor.MemoryGraphControl();
            this.lblGPUUsageTitle = new System.Windows.Forms.Label();
            this.lblGPUUsageValue = new System.Windows.Forms.Label();
            this.gpuUsageGraph = new Memory_Monitor.MemoryGraphControl();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.darkModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.showDiskMonitorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showNetworkMonitorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblDiskUsageTitle = new System.Windows.Forms.Label();
            this.lblDiskUsageValue = new System.Windows.Forms.Label();
            this.diskUsageGauge = new Memory_Monitor.CircularGaugeControl();
            this.lblNetworkUsageTitle = new System.Windows.Forms.Label();
            this.lblNetworkUsageValue = new System.Windows.Forms.Label();
            this.networkUsageGauge = new Memory_Monitor.CircularGaugeControl();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.trayContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.menuStrip.Size = new System.Drawing.Size(760, 24);
            this.menuStrip.TabIndex = 18;
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
            this.showDiskMonitorToolStripMenuItem.Name = "showDiskMonitorToolStripMenuItem";
            this.showDiskMonitorToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.showDiskMonitorToolStripMenuItem.Text = "Show Disk Monitor";
            this.showDiskMonitorToolStripMenuItem.Click += new System.EventHandler(this.ShowDiskMonitorToolStripMenuItem_Click);
            // 
            // showNetworkMonitorToolStripMenuItem
            // 
            this.showNetworkMonitorToolStripMenuItem.CheckOnClick = true;
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
            // lblCPUUsageTitle
            // 
            this.lblCPUUsageTitle.AutoSize = true;
            this.lblCPUUsageTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblCPUUsageTitle.Location = new System.Drawing.Point(20, 35);
            this.lblCPUUsageTitle.Name = "lblCPUUsageTitle";
            this.lblCPUUsageTitle.Size = new System.Drawing.Size(95, 21);
            this.lblCPUUsageTitle.TabIndex = 0;
            this.lblCPUUsageTitle.Text = "CPU Usage";
            // 
            // lblCPUUsageValue
            // 
            this.lblCPUUsageValue.AutoSize = true;
            this.lblCPUUsageValue.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblCPUUsageValue.ForeColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.lblCPUUsageValue.Location = new System.Drawing.Point(20, 60);
            this.lblCPUUsageValue.Name = "lblCPUUsageValue";
            this.lblCPUUsageValue.Size = new System.Drawing.Size(52, 32);
            this.lblCPUUsageValue.TabIndex = 1;
            this.lblCPUUsageValue.Text = "0%";
            // 
            // cpuUsageGraph
            // 
            this.cpuUsageGraph.BackColor = System.Drawing.Color.FromArgb(20, 20, 20);
            this.cpuUsageGraph.LineColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.cpuUsageGraph.Location = new System.Drawing.Point(20, 100);
            this.cpuUsageGraph.MaxDataPoints = 60;
            this.cpuUsageGraph.Name = "cpuUsageGraph";
            this.cpuUsageGraph.Size = new System.Drawing.Size(300, 60);
            this.cpuUsageGraph.TabIndex = 2;
            // 
            // lblGPUUsageTitle
            // 
            this.lblGPUUsageTitle.AutoSize = true;
            this.lblGPUUsageTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblGPUUsageTitle.Location = new System.Drawing.Point(340, 35);
            this.lblGPUUsageTitle.Name = "lblGPUUsageTitle";
            this.lblGPUUsageTitle.Size = new System.Drawing.Size(95, 21);
            this.lblGPUUsageTitle.TabIndex = 3;
            this.lblGPUUsageTitle.Text = "GPU Usage";
            // 
            // lblGPUUsageValue
            // 
            this.lblGPUUsageValue.AutoSize = true;
            this.lblGPUUsageValue.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblGPUUsageValue.ForeColor = System.Drawing.Color.FromArgb(0, 200, 80);
            this.lblGPUUsageValue.Location = new System.Drawing.Point(340, 60);
            this.lblGPUUsageValue.Name = "lblGPUUsageValue";
            this.lblGPUUsageValue.Size = new System.Drawing.Size(52, 32);
            this.lblGPUUsageValue.TabIndex = 4;
            this.lblGPUUsageValue.Text = "0%";
            // 
            // gpuUsageGraph
            // 
            this.gpuUsageGraph.BackColor = System.Drawing.Color.FromArgb(20, 20, 20);
            this.gpuUsageGraph.LineColor = System.Drawing.Color.FromArgb(0, 200, 80);
            this.gpuUsageGraph.Location = new System.Drawing.Point(340, 100);
            this.gpuUsageGraph.MaxDataPoints = 60;
            this.gpuUsageGraph.Name = "gpuUsageGraph";
            this.gpuUsageGraph.Size = new System.Drawing.Size(300, 60);
            this.gpuUsageGraph.TabIndex = 5;
            // 
            // lblDiskUsageTitle
            // 
            this.lblDiskUsageTitle.AutoSize = true;
            this.lblDiskUsageTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblDiskUsageTitle.Location = new System.Drawing.Point(680, 35);
            this.lblDiskUsageTitle.Name = "lblDiskUsageTitle";
            this.lblDiskUsageTitle.Size = new System.Drawing.Size(40, 20);
            this.lblDiskUsageTitle.TabIndex = 24;
            this.lblDiskUsageTitle.Text = "Disk";
            this.lblDiskUsageTitle.Visible = false;
            // 
            // lblDiskUsageValue
            // 
            this.lblDiskUsageValue.AutoSize = true;
            this.lblDiskUsageValue.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblDiskUsageValue.ForeColor = System.Drawing.Color.FromArgb(255, 140, 0);
            this.lblDiskUsageValue.Location = new System.Drawing.Point(680, 58);
            this.lblDiskUsageValue.Name = "lblDiskUsageValue";
            this.lblDiskUsageValue.MaximumSize = new System.Drawing.Size(130, 0);
            this.lblDiskUsageValue.Size = new System.Drawing.Size(58, 13);
            this.lblDiskUsageValue.TabIndex = 25;
            this.lblDiskUsageValue.Text = "0 MB/s";
            this.lblDiskUsageValue.Visible = false;
            // 
            // diskUsageGauge
            // 
            this.diskUsageGauge.BackColor = System.Drawing.Color.Transparent;
            this.diskUsageGauge.GaugeBackgroundColor = System.Drawing.Color.FromArgb(20, 20, 20);
            this.diskUsageGauge.GaugeColor = System.Drawing.Color.FromArgb(255, 140, 0);
            this.diskUsageGauge.Location = new System.Drawing.Point(660, 80);
            this.diskUsageGauge.MaxValue = 500F;
            this.diskUsageGauge.MinimumSize = new System.Drawing.Size(120, 120);
            this.diskUsageGauge.Name = "diskUsageGauge";
            this.diskUsageGauge.NeedleColor = System.Drawing.Color.FromArgb(255, 140, 0);
            this.diskUsageGauge.Size = new System.Drawing.Size(150, 150);
            this.diskUsageGauge.TabIndex = 26;
            this.diskUsageGauge.Visible = false;
            // 
            // lblNetworkUsageTitle
            // 
            this.lblNetworkUsageTitle.AutoSize = true;
            this.lblNetworkUsageTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblNetworkUsageTitle.Location = new System.Drawing.Point(680, 245);
            this.lblNetworkUsageTitle.Name = "lblNetworkUsageTitle";
            this.lblNetworkUsageTitle.Size = new System.Drawing.Size(70, 20);
            this.lblNetworkUsageTitle.TabIndex = 27;
            this.lblNetworkUsageTitle.Text = "Network";
            this.lblNetworkUsageTitle.Visible = false;
            // 
            // lblNetworkUsageValue
            // 
            this.lblNetworkUsageValue.AutoSize = true;
            this.lblNetworkUsageValue.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblNetworkUsageValue.ForeColor = System.Drawing.Color.FromArgb(138, 43, 226);
            this.lblNetworkUsageValue.Location = new System.Drawing.Point(680, 268);
            this.lblNetworkUsageValue.Name = "lblNetworkUsageValue";
            this.lblNetworkUsageValue.MaximumSize = new System.Drawing.Size(130, 0);
            this.lblNetworkUsageValue.Size = new System.Drawing.Size(58, 13);
            this.lblNetworkUsageValue.TabIndex = 28;
            this.lblNetworkUsageValue.Text = "0 MB/s";
            this.lblNetworkUsageValue.Visible = false;
            // 
            // networkUsageGauge
            // 
            this.networkUsageGauge.BackColor = System.Drawing.Color.Transparent;
            this.networkUsageGauge.GaugeBackgroundColor = System.Drawing.Color.FromArgb(20, 20, 20);
            this.networkUsageGauge.GaugeColor = System.Drawing.Color.FromArgb(138, 43, 226);
            this.networkUsageGauge.Location = new System.Drawing.Point(660, 290);
            this.networkUsageGauge.MinimumSize = new System.Drawing.Size(120, 120);
            this.networkUsageGauge.Name = "networkUsageGauge";
            this.networkUsageGauge.Size = new System.Drawing.Size(150, 150);
            this.networkUsageGauge.TabIndex = 29;
            this.networkUsageGauge.Visible = false;
            // 
            // lblSystemMemoryTitle
            // 
            this.lblSystemMemoryTitle.AutoSize = true;
            this.lblSystemMemoryTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblSystemMemoryTitle.Location = new System.Drawing.Point(20, 180);
            this.lblSystemMemoryTitle.Name = "lblSystemMemoryTitle";
            this.lblSystemMemoryTitle.Size = new System.Drawing.Size(130, 21);
            this.lblSystemMemoryTitle.TabIndex = 6;
            this.lblSystemMemoryTitle.Text = "System Memory";
            // 
            // lblSystemMemoryValue
            // 
            this.lblSystemMemoryValue.AutoSize = true;
            this.lblSystemMemoryValue.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSystemMemoryValue.Location = new System.Drawing.Point(20, 205);
            this.lblSystemMemoryValue.Name = "lblSystemMemoryValue";
            this.lblSystemMemoryValue.Size = new System.Drawing.Size(106, 15);
            this.lblSystemMemoryValue.TabIndex = 7;
            this.lblSystemMemoryValue.Text = "0 GB / 0 GB Used";
            // 
            // progressBarSystemMemory
            // 
            this.progressBarSystemMemory.Location = new System.Drawing.Point(20, 225);
            this.progressBarSystemMemory.Name = "progressBarSystemMemory";
            this.progressBarSystemMemory.Size = new System.Drawing.Size(240, 20);
            this.progressBarSystemMemory.TabIndex = 8;
            // 
            // lblSystemMemoryPercent
            // 
            this.lblSystemMemoryPercent.AutoSize = true;
            this.lblSystemMemoryPercent.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSystemMemoryPercent.Location = new System.Drawing.Point(270, 227);
            this.lblSystemMemoryPercent.Name = "lblSystemMemoryPercent";
            this.lblSystemMemoryPercent.Size = new System.Drawing.Size(26, 15);
            this.lblSystemMemoryPercent.TabIndex = 9;
            this.lblSystemMemoryPercent.Text = "0%";
            // 
            // systemMemoryGraph
            // 
            this.systemMemoryGraph.BackColor = System.Drawing.Color.FromArgb(20, 20, 20);
            this.systemMemoryGraph.LineColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.systemMemoryGraph.Location = new System.Drawing.Point(20, 250);
            this.systemMemoryGraph.MaxDataPoints = 60;
            this.systemMemoryGraph.Name = "systemMemoryGraph";
            this.systemMemoryGraph.Size = new System.Drawing.Size(300, 60);
            this.systemMemoryGraph.TabIndex = 10;
            // 
            // lblGPUMemoryTitle
            // 
            this.lblGPUMemoryTitle.AutoSize = true;
            this.lblGPUMemoryTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblGPUMemoryTitle.Location = new System.Drawing.Point(340, 180);
            this.lblGPUMemoryTitle.Name = "lblGPUMemoryTitle";
            this.lblGPUMemoryTitle.Size = new System.Drawing.Size(112, 21);
            this.lblGPUMemoryTitle.TabIndex = 11;
            this.lblGPUMemoryTitle.Text = "GPU Memory";
            // 
            // lblGPUMemoryValue
            // 
            this.lblGPUMemoryValue.AutoSize = true;
            this.lblGPUMemoryValue.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblGPUMemoryValue.Location = new System.Drawing.Point(340, 205);
            this.lblGPUMemoryValue.Name = "lblGPUMemoryValue";
            this.lblGPUMemoryValue.Size = new System.Drawing.Size(106, 15);
            this.lblGPUMemoryValue.TabIndex = 12;
            this.lblGPUMemoryValue.Text = "0 GB / 0 GB Used";
            // 
            // progressBarGPUMemory
            // 
            this.progressBarGPUMemory.Location = new System.Drawing.Point(340, 225);
            this.progressBarGPUMemory.Name = "progressBarGPUMemory";
            this.progressBarGPUMemory.Size = new System.Drawing.Size(240, 20);
            this.progressBarGPUMemory.TabIndex = 13;
            // 
            // lblGPUMemoryPercent
            // 
            this.lblGPUMemoryPercent.AutoSize = true;
            this.lblGPUMemoryPercent.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblGPUMemoryPercent.Location = new System.Drawing.Point(590, 227);
            this.lblGPUMemoryPercent.Name = "lblGPUMemoryPercent";
            this.lblGPUMemoryPercent.Size = new System.Drawing.Size(26, 15);
            this.lblGPUMemoryPercent.TabIndex = 14;
            this.lblGPUMemoryPercent.Text = "0%";
            // 
            // gpuMemoryGraph
            // 
            this.gpuMemoryGraph.BackColor = System.Drawing.Color.FromArgb(20, 20, 20);
            this.gpuMemoryGraph.LineColor = System.Drawing.Color.FromArgb(0, 200, 80);
            this.gpuMemoryGraph.Location = new System.Drawing.Point(340, 250);
            this.gpuMemoryGraph.MaxDataPoints = 60;
            this.gpuMemoryGraph.Name = "gpuMemoryGraph";
            this.gpuMemoryGraph.Size = new System.Drawing.Size(300, 60);
            this.gpuMemoryGraph.TabIndex = 15;
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
            this.lblProcessesTitle.Location = new System.Drawing.Point(20, 330);
            this.lblProcessesTitle.Name = "lblProcessesTitle";
            this.lblProcessesTitle.Size = new System.Drawing.Size(234, 21);
            this.lblProcessesTitle.TabIndex = 16;
            this.lblProcessesTitle.Text = "Processes Using > 400 MB RAM";
            // 
            // listViewProcesses
            // 
            this.listViewProcesses.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnProcessName,
            this.columnMemoryUsage,
            this.columnMemoryMB});
            this.listViewProcesses.FullRowSelect = true;
            this.listViewProcesses.Location = new System.Drawing.Point(20, 360);
            this.listViewProcesses.Name = "listViewProcesses";
            this.listViewProcesses.OwnerDraw = true;
            this.listViewProcesses.Size = new System.Drawing.Size(620, 250);
            this.listViewProcesses.TabIndex = 17;
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
            this.ClientSize = new System.Drawing.Size(840, 630);
            this.Controls.Add(this.networkUsageGauge);
            this.Controls.Add(this.lblNetworkUsageValue);
            this.Controls.Add(this.lblNetworkUsageTitle);
            this.Controls.Add(this.diskUsageGauge);
            this.Controls.Add(this.lblDiskUsageValue);
            this.Controls.Add(this.lblDiskUsageTitle);
            this.Controls.Add(this.listViewProcesses);
            this.Controls.Add(this.lblProcessesTitle);
            this.Controls.Add(this.gpuMemoryGraph);
            this.Controls.Add(this.lblGPUMemoryPercent);
            this.Controls.Add(this.progressBarGPUMemory);
            this.Controls.Add(this.lblGPUMemoryValue);
            this.Controls.Add(this.lblGPUMemoryTitle);
            this.Controls.Add(this.systemMemoryGraph);
            this.Controls.Add(this.lblSystemMemoryPercent);
            this.Controls.Add(this.progressBarSystemMemory);
            this.Controls.Add(this.lblSystemMemoryValue);
            this.Controls.Add(this.lblSystemMemoryTitle);
            this.Controls.Add(this.gpuUsageGraph);
            this.Controls.Add(this.lblGPUUsageValue);
            this.Controls.Add(this.lblGPUUsageTitle);
            this.Controls.Add(this.cpuUsageGraph);
            this.Controls.Add(this.lblCPUUsageValue);
            this.Controls.Add(this.lblCPUUsageTitle);
            this.Controls.Add(this.menuStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MainMenuStrip = this.menuStrip;
            this.MaximizeBox = true;
            this.MinimumSize = new System.Drawing.Size(520, 700);
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

        private System.Windows.Forms.Label lblSystemMemoryTitle;
        private System.Windows.Forms.Label lblSystemMemoryValue;
        private System.Windows.Forms.ProgressBar progressBarSystemMemory;
        private System.Windows.Forms.Label lblSystemMemoryPercent;
        private System.Windows.Forms.Label lblGPUMemoryTitle;
        private System.Windows.Forms.Label lblGPUMemoryValue;
        private System.Windows.Forms.ProgressBar progressBarGPUMemory;
        private System.Windows.Forms.Label lblGPUMemoryPercent;
        private System.Windows.Forms.Timer updateTimer;
        private System.Windows.Forms.Label lblProcessesTitle;
        private System.Windows.Forms.ListView listViewProcesses;
        private System.Windows.Forms.ColumnHeader columnProcessName;
        private System.Windows.Forms.ColumnHeader columnMemoryUsage;
        private System.Windows.Forms.ColumnHeader columnMemoryMB;
        private MemoryGraphControl systemMemoryGraph;
        private MemoryGraphControl gpuMemoryGraph;
        private System.Windows.Forms.Label lblCPUUsageTitle;
        private System.Windows.Forms.Label lblCPUUsageValue;
        private MemoryGraphControl cpuUsageGraph;
        private System.Windows.Forms.Label lblGPUUsageTitle;
        private System.Windows.Forms.Label lblGPUUsageValue;
        private MemoryGraphControl gpuUsageGraph;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem darkModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem showDiskMonitorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showNetworkMonitorToolStripMenuItem;
        private System.Windows.Forms.Label lblDiskUsageTitle;
        private System.Windows.Forms.Label lblDiskUsageValue;
        private CircularGaugeControl diskUsageGauge;
        private System.Windows.Forms.Label lblNetworkUsageTitle;
        private System.Windows.Forms.Label lblNetworkUsageValue;
        private CircularGaugeControl networkUsageGauge;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip trayContextMenu;
        private System.Windows.Forms.ToolStripMenuItem showToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
    }
}
