namespace GraphicExample
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.Chart = new ScottPlot.FormsPlot();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Chart
            // 
            this.Chart.Dock = System.Windows.Forms.DockStyle.Top;
            this.Chart.Location = new System.Drawing.Point(0, 0);
            this.Chart.Name = "Chart";
            this.Chart.Size = new System.Drawing.Size(861, 489);
            this.Chart.TabIndex = 0;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 10;
            this.timer1.Tick += new System.EventHandler(this.EveryFrame);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 495);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(102, 46);
            this.button1.TabIndex = 1;
            this.button1.Text = "Jump";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.JumpClick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(861, 553);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Chart);
            this.Name = "MainForm";
            this.Text = "Example";
            this.Load += new System.EventHandler(this.MainFormLoad);
            this.ResumeLayout(false);

        }

        #endregion

        private ScottPlot.FormsPlot Chart;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button button1;
    }
}

