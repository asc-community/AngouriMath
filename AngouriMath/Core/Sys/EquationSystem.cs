using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AngouriMath.Core.Sys
{
    public class EquationSystem
    {
        private readonly List<Entity> equations;
        public EquationSystem(params Entity[] equations)
        {
            this.equations = equations.ToList();
        }

        public Tensor Solve(params VariableEntity[] vars)
        {
            return MathS.Solve(equations, vars.ToList());
        }

        public string Latexise()
        {
            var sb = new StringBuilder();
            sb.Append(@"\begin{cases}");
            foreach (var eq in equations)
                sb.Append(eq).Append(" = 0").Append(@"\\");
            sb.Append(@"\end{cases}");
            return sb.ToString();
        }
    }
}
