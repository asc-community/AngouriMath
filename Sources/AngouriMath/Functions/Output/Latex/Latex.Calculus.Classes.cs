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
            internal static string LatexiseDerivative(Entity expression, Entity var, int iterations)
            {
                var powerIfNeeded = iterations == 1 ? "" : "^{" + iterations + "}";
                var varOverDeriv = var is Variable { IsLatexUprightFormatted: false } ? var.Latexise() : @"\left(" + var.Latexise() + @"\right)";
                string ParenIfNeeded(string paren) => expression.Priority < Priority.Pow ? paren : "";
                // NOTE: \mathrm{d} is used for upright 'd' following ISO 80000-2 standard.
                // The differential operator should be upright (roman) to distinguish it from variables, similar to sin, cos, log, etc.
                return $$"""\frac{\mathrm{d}{{powerIfNeeded}}}{\mathrm{d}{{varOverDeriv}}{{powerIfNeeded}}}{{ParenIfNeeded(@"\left[")}}{{expression.Latexise()}}{{ParenIfNeeded(@"\right]")}}""";
            }
            /// <inheritdoc/>
            public override string Latexise() => LatexiseDerivative(Expression, Var, Iterations);
        }

        public partial record Integralf
        {
            /// <inheritdoc/>
            public override string Latexise()
            {
                // Unlike derivatives, integrals do not have "power" that would be equal to sequentially applying integration to a function.
                // So for non-positive iterations, we just latexise the derivative. Since we treat integrals as functions for parenthesization,
                // we still need to latexize the 0th derivative explicitly and not just return the inner expression's latex form.
                if (Iterations <= 0)
                    return Derivativef.LatexiseDerivative(Expression, Var, -Iterations);

                var sb = new StringBuilder();
                for (int i = 0; i < Iterations; i++)
                    sb.Append(@"\int");
                sb.Append(@" \left[");
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
                    sb.Append(@"\,");// Leading space before first differential and between differentials
                    sb.Append(@"\mathrm{d}");
                    if (Var is Variable { IsLatexUprightFormatted: false })
                        sb.Append(Var.Latexise());
                    else
                    {
                        sb.Append(@"\left(");
                        sb.Append(Var.Latexise());
                        sb.Append(@"\right)");
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
