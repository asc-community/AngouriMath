//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Numerics;
using AngouriMath;
using AngouriMath.Core;
using ScottPlot;

namespace AngouriMathPlot
{
    public sealed class AMPlotter
    {
        private readonly FormsPlot destination;
        private double[] dataX = Array.Empty<double>();
        private double[] dataY = Array.Empty<double>();
        private int pointCount;

        /// <summary>Creates instance of <see cref="AMPlotter"/> attached to <see cref="FormsPlot"/> from ScottPlot</summary>
        public AMPlotter(FormsPlot dst)
        {
            destination = dst;
            SetPointCount(500);
        }

        /// <summary>How many points we have to draw?</summary>
        /// <param name="number">The higher - the more detailed image you get,
        /// The lower - the higher performance you get.</param>
        public void SetPointCount(int number)
        {
            dataX = new double[number];
            dataY = new double[number];
            pointCount = number;
        }

        /// <summary>Plots from an expression over variable x.
        /// It's better to run this function of already compiled function</summary>
        /// <param name="expr">Expression to build</param>
        /// <param name="from">Low bound</param>
        /// <param name="to">High bound</param>
        public void PlotScatter(Entity expr, Entity.Variable x, Entity.Number.Complex from, Entity.Number.Complex to)
            => PlotScatter(expr.Compile(x), from, to);

        /// <summary>Plots from an expression over variable x</summary>
        /// <param name="expr">Expression to build</param>
        /// <param name="from">Low bound</param>
        /// <param name="to">High bound</param>
        public void PlotScatter(FastExpression func, Entity.Number.Complex from, Entity.Number.Complex to)
        {
            double inner(int it) => ((to - from) / (pointCount - 1) * it).RealPart.EDecimal.ToDouble();
            Clear();
            BuildData(inner,
                        it => func.Call(new Complex((double)inner(it), 0)).Real);
            destination.plt.PlotScatter(dataX, dataY);
            destination.Render();
        }

        /// <summary>Intepreting real values of func as X, imaginary as Y we iterate on range [from; to]</summary>
        public void PlotIterativeComplex(FastExpression func, Entity.Number.Complex from, Entity.Number.Complex to)
        {
            double X(int it) => func.Call(((from + to) / (pointCount - 1) * it).ToNumerics()).Real;
            double Y(int it) => func.Call(((from + to) / (pointCount - 1) * it).ToNumerics()).Imaginary;
            BuildData(X, Y);
            destination.plt.PlotScatter(dataX, dataY);
        }

        /// <summary>Cleans the graph</summary>
        public void Clear() => destination.plt.Clear();

        /// <summary>Renders onto the destination ScottPlot form</summary>
        public void Render() =>destination.Render();
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
