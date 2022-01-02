//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Text;

namespace AngouriMath
{
    partial record Entity
    {
        public partial record Derivativef
        {
            /// <inheritdoc/>
            public override string Latexise()
            {
                var powerIfNeeded = Iterations == 1 ? "" : "^{" + Iterations + "}";

                var varOverDeriv =
                    Var is Variable { Name: { Length: 1 } name }
                    ? name
                    : @"\left[" + Var.Latexise(false) + @"\right]";

                // TODO: Should we display the d upright using \mathrm?
                // Differentiation is an operation, just like sin, cos, etc.
                return @"\frac{d" + powerIfNeeded +
                @"\left[" + Expression.Latexise(false) + @"\right]}{d" + varOverDeriv + powerIfNeeded + "}";
            }
        }

        public partial record Integralf
        {
            /// <inheritdoc/>
            public override string Latexise()
            {
                // Unlike derivatives, integrals do not have "power" that would be equal
                // to sequential applying integration to a function

                if (Iterations < 0)
                    return "Error";

                if (Iterations == 0)
                    return Expression.Latexise(false);

                var sb = new StringBuilder();
                for (int i = 0; i < Iterations; i++)
                    sb.Append(@"\int ");
                sb.Append(@"\left[");
                sb.Append(Expression.Latexise(false));
                sb.Append(@"\right]");

                // TODO: can we write d^2 x or (dx)^2 instead of dx dx?
                // I don't think I have ever seen the same variable being integrated more than one time. -- Happypig375
                for (int i = 0; i < Iterations; i++)
                {
                    sb.Append(" d");
                    if (Var is Variable { Name: { Length: 1 } name })
                        sb.Append(name);
                    else
                    {
                        sb.Append(@"\left[");
                        sb.Append(Var.Latexise(false));
                        sb.Append(@"\right]");
                    }
                }
                return sb.ToString();
            }
        }

        public partial record Limitf
        {
            /// <inheritdoc/>
            public override string Latexise()
            {
                var sb = new StringBuilder();
                sb.Append(@"\lim_{").Append(Var.Latexise())
                    .Append(@"\to ").Append(Destination.Latexise());

                switch (ApproachFrom)
                {
                    case ApproachFrom.Left:
                        sb.Append("^-");
                        break;
                    case ApproachFrom.Right:
                        sb.Append("^+");
                        break;
                }

                sb.Append("} ");
                if (Expression.Priority < Priority.Pow)
                    sb.Append(@"\left[");
                sb.Append(Expression.Latexise());
                if (Expression.Priority < Priority.Pow)
                    sb.Append(@"\right]");

                return sb.ToString();
            }
        }
    }
}
