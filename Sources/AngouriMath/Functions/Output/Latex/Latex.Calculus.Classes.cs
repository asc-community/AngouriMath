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
        partial record CalculusOperator
        {
            /// <inheritdoc/>
            internal override Priority LatexPriority => Priority.LatexCalculusOperation;
        }
        public partial record Derivativef
        {
            /// <inheritdoc/>
            public override string Latexise()
            {
                if (Iterations < 0)
                {
                    var sb = new StringBuilder();
                    for (int i = 0; i < -Iterations; i++)
                        sb.Append(@"\int");
                    sb.Append(' ').Append(Expression.Latexise(Expression.LatexPriority < Priority.LatexCalculusOperation));

                    for (int i = 0; i < -Iterations; i++)
                    {
                        sb.Append(@"\,");
                        sb.Append(@"\mathrm{d}");
                        sb.Append(Var.Latexise(Var is not Variable { IsLatexUprightFormatted: false }));
                    }
                    return sb.ToString();
                }
                var powerIfNeeded = Iterations == 1 ? "" : "^{" + Iterations + "}";
                // NOTE: \mathrm{d} is used for upright 'd' following ISO 80000-2 standard.
                // The differential operator should be upright (roman) to distinguish it from variables, similar to sin, cos, log, etc.
                return $$"""\frac{\mathrm{d}{{powerIfNeeded}}}{\mathrm{d}{{Var.Latexise(Var is not Variable { IsLatexUprightFormatted: false })
                    }}{{powerIfNeeded}}}{{Expression.Latexise(Expression.LatexPriority < Priority.LatexCalculusOperation)}}""";
            }
        }

        public partial record Integralf
        {
            /// <inheritdoc/>
            public override string Latexise()
            {
                var sb = new StringBuilder(@"\int");
                if (Range is var (from, to)) sb.Append('_').Append('{').Append(from.Latexise()).Append('}').Append('^').Append('{').Append(to.Latexise()).Append('}');
                sb.Append(' ').Append(Expression.Latexise(Expression.LatexPriority < Priority.LatexCalculusOperation));

                // NOTE: \mathrm{d} is used for upright 'd' following ISO 80000-2 standard.
                // The differential operator should be upright (roman) to distinguish it from variables.
                // Multiple integrals use repeated differentials (\mathrm{d}x \mathrm{d}x) rather than power notation (\mathrm{d}^2 x).
                // While derivatives use \mathrm{d}^n / \mathrm{d}x^n, power notation for integrals (\mathrm{d}^2 x) would be confusing
                // as the number of \mathrm{d} is usually expected to match the number of \int.
                // Thin spaces (\,) are added between differentials following standard practice.
                sb.Append(@"\,"); // Leading space before first differential and between differentials
                sb.Append(@"\mathrm{d}");
                sb.Append(Var.Latexise(Var is not Variable { IsLatexUprightFormatted: false }));
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
                        sb.Append(Destination.Latexise(Destination.LatexPriority <= Priority.Pow)).Append("^-");
                        break;
                    case ApproachFrom.Right:
                        sb.Append(Destination.Latexise(Destination.LatexPriority <= Priority.Pow)).Append("^+");
                        break;
                    case ApproachFrom.BothSides:
                        sb.Append(Destination.Latexise());
                        break;
                }

                sb.Append("} ");
                sb.Append(Expression.Latexise(Expression.LatexPriority < Priority.LatexCalculusOperation));
                return sb.ToString();
            }
        }
    }
}
