using AngouriMath.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath
{
    public abstract partial class Entity
    {
        /// <summary>
        /// Returns a value of a definite integral of a function. Only works for one-variable functions
        /// </summary>
        /// <param name="x">
        /// Variable to integrate over
        /// </param>
        /// <param name="from">
        /// The down bound for integrating
        /// </param>
        /// <param name="to">
        /// The up bound for integrating
        /// </param>
        /// <returns></returns>
        public Number DefiniteIntegral(VariableEntity x, Number from, Number to)
        {
            return Integration.Integrate(this, x, from, to, 100);
        }

        /// <summary>
        /// Returns a value of a definite integral of a function. Only works for one-variable functions
        /// </summary>
        /// <param name="x">
        /// Variable to integrate over
        /// </param>
        /// <param name="from">
        /// The down bound for integrating
        /// </param>
        /// <param name="to">
        /// The up bound for integrating
        /// </param>
        /// <param name="stepCount">
        /// Accuracy (initially, amount of iterations)
        /// </param>
        /// <returns></returns>
        public Number DefiniteIntegral(VariableEntity x, Number from, Number to, int stepCount)
        {
            return Integration.Integrate(this, x, from, to, stepCount);
        }
    }
    public static class Integration
    {
        public static Number Integrate(Entity func, VariableEntity x, Number from, Number to, int stepCount)
        {
            double ReFrom = from.Re;
            double ImFrom = from.Im;
            double ReTo = to.Re;
            double ImTo = to.Im;
            var res = new Number(0, 0);
            var cfunc = func.Compile(x.Name);
            for(int i = 0; i <= stepCount; i++)
            {
                var share = ((double)i) / stepCount;
                var tmp = new Number(ReFrom * share + ReTo * (1 - share), ImFrom * share + ImTo * (1 - share));
                res += cfunc.Eval(tmp);
            }
            return res / (stepCount + 1) * (to - from);
        }
    }
}
