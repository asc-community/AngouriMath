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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        static FastExpression niceFunc;
        static double t = 120;
        private void Form1_Load(object sender, EventArgs e)
        {
            var A = MathS.Var("A");
            var B = MathS.Var("B");
            var expr = B * MathS.Sin(B) * MathS.Pow(MathS.e, MathS.i * B * MathS.Cos(A));
            niceFunc = expr.Compile(A, B);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var X = new List<double>();
            var Y = new List<double>();
            var A = t;
            for(double B = 0; B < A; B += 0.1)
            {
                var res = niceFunc.Call(A, B);
                X.Add(res.Re);
                Y.Add(res.Im);
            }
            formsPlot1.plt.Clear();
            formsPlot1.plt.PlotScatter(X.ToArray(), Y.ToArray());
            formsPlot1.Render();
            t += 0.0005;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            t += 1.0;
        }
    }
}
