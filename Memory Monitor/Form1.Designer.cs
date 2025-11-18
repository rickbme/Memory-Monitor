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
            this.SuspendLayout();
            // 
            // lblCPUUsageTitle
            // 
            this.lblCPUUsageTitle.AutoSize = true;
            this.lblCPUUsageTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblCPUUsageTitle.Location = new System.Drawing.Point(20, 20);
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
            this.lblCPUUsageValue.Location = new System.Drawing.Point(20, 45);
            this.lblCPUUsageValue.Name = "lblCPUUsageValue";
            this.lblCPUUsageValue.Size = new System.Drawing.Size(52, 32);
            this.lblCPUUsageValue.TabIndex = 1;
            this.lblCPUUsageValue.Text = "0%";
            // 
            // cpuUsageGraph
            // 
            this.cpuUsageGraph.BackColor = System.Drawing.Color.FromArgb(20, 20, 20);
            this.cpuUsageGraph.LineColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.cpuUsageGraph.Location = new System.Drawing.Point(20, 85);
            this.cpuUsageGraph.MaxDataPoints = 60;
            this.cpuUsageGraph.Name = "cpuUsageGraph";
            this.cpuUsageGraph.Size = new System.Drawing.Size(300, 60);
            this.cpuUsageGraph.TabIndex = 2;
            // 
            // lblGPUUsageTitle
            // 
            this.lblGPUUsageTitle.AutoSize = true;
            this.lblGPUUsageTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblGPUUsageTitle.Location = new System.Drawing.Point(340, 20);
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
            this.lblGPUUsageValue.Location = new System.Drawing.Point(340, 45);
            this.lblGPUUsageValue.Name = "lblGPUUsageValue";
            this.lblGPUUsageValue.Size = new System.Drawing.Size(52, 32);
            this.lblGPUUsageValue.TabIndex = 4;
            this.lblGPUUsageValue.Text = "0%";
            // 
            // gpuUsageGraph
            // 
            this.gpuUsageGraph.BackColor = System.Drawing.Color.FromArgb(20, 20, 20);
            this.gpuUsageGraph.LineColor = System.Drawing.Color.FromArgb(0, 200, 80);
            this.gpuUsageGraph.Location = new System.Drawing.Point(340, 85);
            this.gpuUsageGraph.MaxDataPoints = 60;
            this.gpuUsageGraph.Name = "gpuUsageGraph";
            this.gpuUsageGraph.Size = new System.Drawing.Size(300, 60);
            this.gpuUsageGraph.TabIndex = 5;
            // 
            // lblSystemMemoryTitle
            // 
            this.lblSystemMemoryTitle.AutoSize = true;
            this.lblSystemMemoryTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblSystemMemoryTitle.Location = new System.Drawing.Point(20, 165);
            this.lblSystemMemoryTitle.Name = "lblSystemMemoryTitle";
            this.lblSystemMemoryTitle.Size = new System.Drawing.Size(130, 21);
            this.lblSystemMemoryTitle.TabIndex = 6;
            this.lblSystemMemoryTitle.Text = "System Memory";
            // 
            // lblSystemMemoryValue
            // 
            this.lblSystemMemoryValue.AutoSize = true;
            this.lblSystemMemoryValue.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSystemMemoryValue.Location = new System.Drawing.Point(20, 190);
            this.lblSystemMemoryValue.Name = "lblSystemMemoryValue";
            this.lblSystemMemoryValue.Size = new System.Drawing.Size(106, 15);
            this.lblSystemMemoryValue.TabIndex = 7;
            this.lblSystemMemoryValue.Text = "0 GB / 0 GB Used";
            // 
            // progressBarSystemMemory
            // 
            this.progressBarSystemMemory.Location = new System.Drawing.Point(20, 210);
            this.progressBarSystemMemory.Name = "progressBarSystemMemory";
            this.progressBarSystemMemory.Size = new System.Drawing.Size(240, 20);
            this.progressBarSystemMemory.TabIndex = 8;
            // 
            // lblSystemMemoryPercent
            // 
            this.lblSystemMemoryPercent.AutoSize = true;
            this.lblSystemMemoryPercent.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSystemMemoryPercent.Location = new System.Drawing.Point(270, 212);
            this.lblSystemMemoryPercent.Name = "lblSystemMemoryPercent";
            this.lblSystemMemoryPercent.Size = new System.Drawing.Size(26, 15);
            this.lblSystemMemoryPercent.TabIndex = 9;
            this.lblSystemMemoryPercent.Text = "0%";
            // 
            // systemMemoryGraph
            // 
            this.systemMemoryGraph.BackColor = System.Drawing.Color.FromArgb(20, 20, 20);
            this.systemMemoryGraph.LineColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.systemMemoryGraph.Location = new System.Drawing.Point(20, 235);
            this.systemMemoryGraph.MaxDataPoints = 60;
            this.systemMemoryGraph.Name = "systemMemoryGraph";
            this.systemMemoryGraph.Size = new System.Drawing.Size(300, 60);
            this.systemMemoryGraph.TabIndex = 10;
            // 
            // lblGPUMemoryTitle
            // 
            this.lblGPUMemoryTitle.AutoSize = true;
            this.lblGPUMemoryTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblGPUMemoryTitle.Location = new System.Drawing.Point(340, 165);
            this.lblGPUMemoryTitle.Name = "lblGPUMemoryTitle";
            this.lblGPUMemoryTitle.Size = new System.Drawing.Size(112, 21);
            this.lblGPUMemoryTitle.TabIndex = 11;
            this.lblGPUMemoryTitle.Text = "GPU Memory";
            // 
            // lblGPUMemoryValue
            // 
            this.lblGPUMemoryValue.AutoSize = true;
            this.lblGPUMemoryValue.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblGPUMemoryValue.Location = new System.Drawing.Point(340, 190);
            this.lblGPUMemoryValue.Name = "lblGPUMemoryValue";
            this.lblGPUMemoryValue.Size = new System.Drawing.Size(106, 15);
            this.lblGPUMemoryValue.TabIndex = 12;
            this.lblGPUMemoryValue.Text = "0 GB / 0 GB Used";
            // 
            // progressBarGPUMemory
            // 
            this.progressBarGPUMemory.Location = new System.Drawing.Point(340, 210);
            this.progressBarGPUMemory.Name = "progressBarGPUMemory";
            this.progressBarGPUMemory.Size = new System.Drawing.Size(240, 20);
            this.progressBarGPUMemory.TabIndex = 13;
            // 
            // lblGPUMemoryPercent
            // 
            this.lblGPUMemoryPercent.AutoSize = true;
            this.lblGPUMemoryPercent.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblGPUMemoryPercent.Location = new System.Drawing.Point(590, 212);
            this.lblGPUMemoryPercent.Name = "lblGPUMemoryPercent";
            this.lblGPUMemoryPercent.Size = new System.Drawing.Size(26, 15);
            this.lblGPUMemoryPercent.TabIndex = 14;
            this.lblGPUMemoryPercent.Text = "0%";
            // 
            // gpuMemoryGraph
            // 
            this.gpuMemoryGraph.BackColor = System.Drawing.Color.FromArgb(20, 20, 20);
            this.gpuMemoryGraph.LineColor = System.Drawing.Color.FromArgb(0, 200, 80);
            this.gpuMemoryGraph.Location = new System.Drawing.Point(340, 235);
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
            this.lblProcessesTitle.Location = new System.Drawing.Point(20, 315);
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
            this.listViewProcesses.GridLines = true;
            this.listViewProcesses.Location = new System.Drawing.Point(20, 345);
            this.listViewProcesses.Name = "listViewProcesses";
            this.listViewProcesses.Size = new System.Drawing.Size(620, 265);
            this.listViewProcesses.TabIndex = 17;
            this.listViewProcesses.UseCompatibleStateImageBehavior = false;
            this.listViewProcesses.View = System.Windows.Forms.View.Details;
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
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(660, 630);
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
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "System Monitor";
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
    }
}
