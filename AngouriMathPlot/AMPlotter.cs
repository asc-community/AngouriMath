using System;
using System.Collections.Generic;
using System.Numerics;
using AngouriMath;
using ScottPlot;
using AngouriMath.Core.Numerix;

namespace AngouriMathPlot
{
    public class AMPlotter
    {
        private readonly FormsPlot destination;
        private double[] dataX = Array.Empty<double>();
        private double[] dataY = Array.Empty<double>();
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
        public void PlotScatter(Entity expr, Entity.Variable x, ComplexNumber from, ComplexNumber to)
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
        public void PlotScatter(FastExpression func, ComplexNumber from, ComplexNumber to)
        {
            double inner(int it) => ((to - from) / (pointCount - 1) * it).Real.Decimal.ToDouble();
            Clear();
            BuildData(inner,
                        it => func.Call(new Complex((double)inner(it), 0)).Real);
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
        public void PlotIterativeComplex(FastExpression func, ComplexNumber from, ComplexNumber to)
        {
            double X(int it) => func.Call(((from + to) / (pointCount - 1) * it).AsComplex()).Real;
            double Y(int it) => func.Call(((from + to) / (pointCount - 1) * it).AsComplex()).Imaginary;
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
