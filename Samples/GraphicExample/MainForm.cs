using AngouriMath;
using AngouriMathPlot;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GraphicExample
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            plotter = new AMPlotter(Chart);
        }
        readonly AMPlotter plotter;
        decimal t = 120;
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Chart.Size = new Size(Width, Height - 104);
            button1.Location = new Point(0, Height - 98);
        }

        private void EveryFrame(object sender, EventArgs e)
        {
            var B = MathS.Var("B");
            var expr2 = B * MathS.Sin(t + B) * MathS.Pow(MathS.e, MathS.i * B * MathS.Cos(t));
            var niceFunc2 = expr2.Compile(B);
            plotter.Clear();
            plotter.PlotIterativeComplex(niceFunc2, 0, t);
            plotter.Render();
            t += 0.0005m;
        }

        private void JumpClick(object sender, EventArgs e)
        {
            t += 1.0m;
        }
    }
}
