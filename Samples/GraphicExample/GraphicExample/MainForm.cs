using AngouriMath;
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
        FastExpression niceFunc1;
        FastExpression niceFunc2;
        double t = 120;
        private void MainFormLoad(object sender, EventArgs e)
        {
            var A = MathS.Var("A");
            var B = MathS.Var("B");
            var expr1 = MathS.Cos(B) * MathS.Sin(B) * MathS.Pow(MathS.e, MathS.i * B * MathS.Cos(A));
            var expr2 = B * MathS.Sin(A + B) * MathS.Pow(MathS.e, MathS.i * B * MathS.Cos(A));
            niceFunc1 = expr1.Compile(A, B);
            niceFunc2 = expr2.Compile(A, B);
        }
        readonly List<double> X1 = new List<double>();
        readonly List<double> Y1 = new List<double>();
        readonly List<double> X2 = new List<double>();
        readonly List<double> Y2 = new List<double>();
        private void EveryFrame(object sender, EventArgs e)
        {
            X1.Clear(); Y1.Clear();
            X2.Clear(); Y2.Clear();
            var A = t;
            for(double B = 0; B < A; B += 0.1)
            {
                var res = niceFunc1.Call(A, B);
                X1.Add(res.Re * 150);
                Y1.Add(res.Im * 150);

                res = niceFunc2.Call(A, B);
                X2.Add(res.Re + 160);
                Y2.Add(res.Im);
            }
            Chart.plt.Clear();
            Chart.plt.PlotScatter(X1.ToArray(), Y1.ToArray());
            Chart.plt.PlotScatter(X2.ToArray(), Y2.ToArray());
            Chart.Render();
            t += 0.0005;
        }

        private void JumpClick(object sender, EventArgs e)
        {
            t += 1.0;
        }
    }
}
