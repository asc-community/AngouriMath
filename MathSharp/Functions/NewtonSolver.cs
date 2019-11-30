using MathSharp.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathSharp
{
    public abstract partial class Entity
    {
        public Number GetValue()
        {
            if (this is NumberEntity)
                return (this as NumberEntity).Value;
            else
                throw new MathSException("Cannot get number from expression");
        }
        private Number NewtonIter(VariableEntity x, Number value, int precision)
        {
            Number prev = value;
            Func<NumberEntity, Number> f = value => this.Substitute(x, value).Eval().GetValue();
            Func<NumberEntity, Number> df = value => Derivative.Substitute(x, value).Eval().GetValue();
            int minCheckIters = (int)Math.Sqrt(precision);
            for (int i = 0; i < precision; i++)
            {
                if(i == precision - 1)
                    prev = value.Copy();
                try
                {
                    value = value - f(value) / df(value);
                }
                catch(MathSException e)
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
        private Entity Derivative { set; get; }
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
            Derivative = this.Derive(v).Simplify();
            for (int x = 0; x < xStep; x++)
                for (int y = 0; y < yStep; y++)
                {
                    double xShare = ((double)x) / xStep;
                    double yShare = ((double)y) / yStep;
                    var value = new Number(from.Re * xShare + to.Re * (1 - xShare),
                                           from.Im * yShare + to.Im * (1 - yShare));
                    var root = NewtonIter(v, value, precision);
                    if (root != null)     // TODO
                        res.Include(root);
                        
                }
            return res;
        }
    }
}
