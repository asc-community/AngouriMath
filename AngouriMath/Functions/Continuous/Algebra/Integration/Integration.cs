namespace AngouriMath.Functions.Algebra
{
    public static partial class Integration
    {
        public static Entity ComputeIndefiniteIntegral(Entity expr, Entity.Variable x)
        {
            if (!expr.Contains(x)) return expr * x; // base case, handle here

            Entity? answer = null;

            answer = IntegralPatterns.TryStandardIntegrals(expr, x);
            if (answer is { }) return answer;

            answer = IndefiniteIntegralSolver.SolveBySplittingSum(expr, x);
            if (answer is { }) return answer;

            answer = IndefiniteIntegralSolver.SolveAsPolynomialTerm(expr, x);
            if (answer is { }) return answer;

            answer = IndefiniteIntegralSolver.SolveIntegratingByParts(expr, x);
            if (answer is { }) return answer;

            answer = IndefiniteIntegralSolver.SolveLogarithmic(expr, x);
            if (answer is { }) return answer;

            return new Entity.Integralf(expr, x, 1); // return as integral if nothing can be done with expression
        }
    }
}
