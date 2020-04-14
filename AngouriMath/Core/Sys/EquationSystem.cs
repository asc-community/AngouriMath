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
            if (equations.Count == 1)
                return equations[0].Latexise();
            if (equations.Count == 0)
                return string.Empty;
            var sb = new StringBuilder();
            sb.Append(@"\begin{cases}");
            foreach (var eq in equations)
                sb.Append(eq.Latexise()).Append(" = 0").Append(@"\\");
            sb.Append(@"\end{cases}");
            return sb.ToString();
        }
    }
}
