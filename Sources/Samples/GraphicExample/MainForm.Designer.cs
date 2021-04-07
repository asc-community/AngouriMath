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
            this.InputText = new System.Windows.Forms.TextBox();
            this.LabelTitle = new System.Windows.Forms.Label();
            this.LabelState = new System.Windows.Forms.Label();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.ButtonSolve = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Chart
            // 
            this.Chart.Location = new System.Drawing.Point(0, 0);
            this.Chart.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.Chart.Name = "Chart";
            this.Chart.Size = new System.Drawing.Size(573, 565);
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
            this.button1.Location = new System.Drawing.Point(14, 571);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(119, 27);
            this.button1.TabIndex = 1;
            this.button1.Text = "Jump";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.JumpClick);
            // 
            // InputText
            // 
            this.InputText.Location = new System.Drawing.Point(639, 154);
            this.InputText.Name = "InputText";
            this.InputText.Size = new System.Drawing.Size(476, 23);
            this.InputText.TabIndex = 2;
            this.InputText.Text = "a e sin(x ^ 14 + 3)3 + a b c d sin(x ^ 14 + 2)4 - k d sin(x ^ 14 + 3)2 + sin(x ^ " +
    "14 + 3) + e = 0";
            // 
            // LabelTitle
            // 
            this.LabelTitle.AutoSize = true;
            this.LabelTitle.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.LabelTitle.Location = new System.Drawing.Point(708, 99);
            this.LabelTitle.Name = "LabelTitle";
            this.LabelTitle.Size = new System.Drawing.Size(331, 28);
            this.LabelTitle.TabIndex = 3;
            this.LabelTitle.Text = "Multithreading (cancellation) sample";
            // 
            // LabelState
            // 
            this.LabelState.AutoSize = true;
            this.LabelState.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.LabelState.Location = new System.Drawing.Point(662, 310);
            this.LabelState.Name = "LabelState";
            this.LabelState.Size = new System.Drawing.Size(0, 30);
            this.LabelState.TabIndex = 4;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Location = new System.Drawing.Point(745, 206);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(119, 58);
            this.ButtonCancel.TabIndex = 5;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // ButtonSolve
            // 
            this.ButtonSolve.Location = new System.Drawing.Point(891, 206);
            this.ButtonSolve.Name = "ButtonSolve";
            this.ButtonSolve.Size = new System.Drawing.Size(119, 58);
            this.ButtonSolve.TabIndex = 6;
            this.ButtonSolve.Text = "Solve";
            this.ButtonSolve.UseVisualStyleBackColor = true;
            this.ButtonSolve.Click += new System.EventHandler(this.ButtonSolve_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1187, 638);
            this.Controls.Add(this.ButtonSolve);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.LabelState);
            this.Controls.Add(this.LabelTitle);
            this.Controls.Add(this.InputText);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Chart);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "MainForm";
            this.Text = "Example";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ScottPlot.FormsPlot Chart;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label LabelTitle;
        private System.Windows.Forms.Label LabelState;
        private System.Windows.Forms.TextBox InputText;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Button ButtonSolve;
    }
}

