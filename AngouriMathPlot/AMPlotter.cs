using System;
using System.Collections.Generic;
using AngouriMath;
using AngouriMath.Core;
using ScottPlot;
using System.Windows.Forms;

namespace AngouriMathPlot
{
    public class AMPlotter
    {
        private readonly FormsPlot destination;
        private List<FastExpression> functions;
        private double[] dataX;
        private double[] dataY;
        private int pointCount;

        /// <summary>
        /// Creates instance of AMScatter attached to FormsPlot from ScottPlot
        /// </summary>
        /// <param name="dst"></param>
        public AMPlotter(FormsPlot dst)
        {
            destination = dst;
            SetPointCount(500);
        }

        /// <summary>
        /// How many points we have to draw?
        /// </summary>
        /// <param name="number">
        /// The higher - the more detailed image you get,
        /// The lower - the higher performance you get.
        /// </param>
        public void SetPointCount(int number)
        {
            dataX = new double[number];
            dataY = new double[number];
            pointCount = number;
        }

        /// <summary>
        /// Plots from an expression over variable x.
        /// It's better to run this function of already compiled function
        /// </summary>
        /// <param name="expr">
        /// Expression to build
        /// </param>
        /// <param name="from">
        /// Low bound
        /// </param>
        /// <param name="to">
        /// High bound
        /// </param>
        public void PlotScatter(Entity expr, VariableEntity x, Number from, Number to)
            => PlotScatter(expr.Compile(x), from, to);

        /// <summary>
        /// Plots from an expression over variable x
        /// </summary>
        /// <param name="expr">
        /// Expression to build
        /// </param>
        /// <param name="from">
        /// Low bound
        /// </param>
        /// <param name="to">
        /// High bound
        /// </param>
        public void PlotScatter(FastExpression func, Number from, Number to)
        {
            Func<int, double> inner = it => ((to - from) / (pointCount - 1) * it).Re;
            Clear();
            BuildData(inner,
                        it => func.Call(inner(it)).Re);
            destination.plt.PlotScatter(dataX, dataY);
            destination.Render();
        }

        /// <summary>
        /// Intepreting real values of func as X,
        /// imaginary as Y we iterate on range [from; to]
        /// </summary>
        /// <param name="func"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void PlotIterativeComplex(FastExpression func, Number from, Number to)
        {
            Func<int, double> X = it => func.Call((from + to) / (pointCount - 1) * it).Re;
            Func<int, double> Y = it => func.Call((from + to) / (pointCount - 1) * it).Im;
            BuildData(X, Y);
            destination.plt.PlotScatter(dataX, dataY);
        }

        /// <summary>
        /// Cleans the graph
        /// </summary>
        public void Clear()
        {
            destination.plt.Clear();
        }

        /// <summary>
        /// Renders onto the destination ScottPlot form
        /// </summary>
        public void Render()
        {
            destination.Render();
        }

        private void BuildData(Func<int, double> X, Func<int, double> Y)
        {
            for(int i = 0; i < pointCount; i++)
            {
                dataX[i] = X(i);
                dataY[i] = Y(i);
            }
        }
    }
}
