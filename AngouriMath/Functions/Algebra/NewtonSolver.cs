using AngouriMath.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath
{
    public abstract partial class Entity
    {
        /// <summary>
        /// To get Number from NumberEntity (in case of need a concrete number)
        /// </summary>
        /// <returns></returns>
        public Number GetValue()
        {
            if (this is NumberEntity)
                return (this as NumberEntity).Value;
            else
                throw new MathSException("Cannot get number from expression");
        }
        private Number NewtonIter(FastExpression f, FastExpression df, VariableEntity x, Number value, int precision)
        {
            Number prev = value;
            int minCheckIters = (int)Math.Sqrt(precision);
            for (int i = 0; i < precision; i++)
            {
                if(i == precision - 1)
                    prev = value.Copy();
                try
                {
                    value = value - f.Substitute(value) / df.Substitute(value);
                }
                catch(MathSException)
                {
                    throw new MathSException("Two or more variables in SolveNt is forbidden");
                }
                if (i > minCheckIters && prev == value)
                    return value;
            }
            if (Number.Abs(prev - value) > 0.01)
                return null;
            else
                return value;
        }

        /// <summary>
        /// Searches for numerical solutions via Newton's method https://en.wikipedia.org/wiki/Newton%27s_method
        /// </summary>
        /// <param name="v">
        /// Variable to solve over
        /// </param>
        /// <param name="from">
        /// Re(from) - down bound of search in real numbers, Im(from) - that in imaginary. 
        /// For example, from: new Number(-10, -10)
        /// </param>
        /// <param name="to">
        /// Re(to) - up bound of search in real numbers, Im(to) - that in imaginary.
        /// For exmaple, to: new Number(10, 10)
        /// </param>
        /// <param name="stepCount">
        /// Re(stepCount) - number of steps over real numbers, Im(stepCount) - number of steps over imaginary numbers.
        /// For example, stepCount: new Number(10, 10)
        /// </param>
        /// <param name="precision">
        /// If you get very similar roots that you think are equal, increase precision (but it will slower the algorithm)
        /// </param>
        /// <returns></returns>
        public NumberSet SolveNt(VariableEntity v, Number from = null, Number to = null, Number stepCount = null, int precision = 30)
        {
            var res = new NumberSet();
            if (from == null)
                from = new Number(-10, -10);
            if (to == null)
                to = new Number(10, 10);
            int xStep;
            int yStep;
            if (stepCount == null)
            {
                xStep = 5;
                yStep = 3;
            }
            else
            {
                xStep = (int)stepCount.Re;
                yStep = (int)stepCount.Im;
            }
            var df = this.Derive(v).Simplify().Compile(v);
            var f = this.Simplify().Compile(v);
            for (int x = 0; x < xStep; x++)
                for (int y = 0; y < yStep; y++)
                {
                    double xShare = ((double)x) / xStep;
                    double yShare = ((double)y) / yStep;
                    var value = new Number(from.Re * xShare + to.Re * (1 - xShare),
                                           from.Im * yShare + to.Im * (1 - yShare));
                    var root = NewtonIter(f, df, v, value, precision);
                    if (root != null)
                        res.Include(root);
                }
            return res;
        }
    }
}
