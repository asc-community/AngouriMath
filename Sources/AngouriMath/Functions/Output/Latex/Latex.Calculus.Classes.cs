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
                var varOverDeriv = Var is Variable { IsLatexUprightFormatted: false } ? Var.Latexise() : @"\left(" + Var.Latexise() + @"\right)";
                string ParenIfNeeded(string paren) => Expression.Priority < Priority.Pow ? paren : "";
                // NOTE: \mathrm{d} is used for upright 'd' following ISO 80000-2 standard.
                // The differential operator should be upright (roman) to distinguish it from variables, similar to sin, cos, log, etc.
                return $$"""\frac{\mathrm{d}{{powerIfNeeded}}}{\mathrm{d}{{varOverDeriv}}{{powerIfNeeded}}}{{ParenIfNeeded(@"\left[")}}{{Expression.Latexise()}}{{ParenIfNeeded(@"\right]")}}""";

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

                // NOTE: \mathrm{d} is used for upright 'd' following ISO 80000-2 standard.
                // The differential operator should be upright (roman) to distinguish it from variables.
                // Multiple integrals use repeated differentials (\mathrm{d}x \mathrm{d}x) rather than power notation (\mathrm{d}^2 x).
                // While derivatives use \mathrm{d}^n / \mathrm{d}x^n, power notation for integrals (\mathrm{d}^2 x) would be confusing
                // as the number of \mathrm{d} is usually expected to match the number of \int.
                // Thin spaces (\,) are added between differentials following standard practice.
                for (int i = 0; i < Iterations; i++)
                {
                    if (i > 0)
                        sb.Append(@"\,");  // Add thin space between repeated differentials
                    else
                        sb.Append(' ');    // Leading space before first differential
                    sb.Append(@"\mathrm{d}");
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
                    .Append(@"\to ");

                switch (ApproachFrom)
                {
                    case ApproachFrom.Left:
                        sb.Append(Destination.Latexise(Destination.Priority <= Priority.Pow)).Append("^-");
                        break;
                    case ApproachFrom.Right:
                        sb.Append(Destination.Latexise(Destination.Priority <= Priority.Pow)).Append("^+");
                        break;
                    case ApproachFrom.BothSides:
                        sb.Append(Destination.Latexise());
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
