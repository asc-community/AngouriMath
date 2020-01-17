using AngouriMath;
using AngouriMathPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GraphicExample
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        AMPlotter plotter;
        double t = 120;
        private void MainFormLoad(object sender, EventArgs e)
        {
            plotter = new AMPlotter(Chart);
        }
        
        private void EveryFrame(object sender, EventArgs e)
        {
            var B = MathS.Var("B");
            var expr2 = B * MathS.Sin(t + B) * MathS.Pow(MathS.e, MathS.i * B * MathS.Cos(t));
            var niceFunc2 = expr2.Compile(B);
            plotter.Clear();
            plotter.PlotIterativeComplex(niceFunc2, 0, t);
            plotter.Render();
            t += 0.0005;
        }

        private void JumpClick(object sender, EventArgs e)
        {
            t += 1.0;
        }
    }
}
