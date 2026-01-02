//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;

namespace AngouriMath
{
    partial record Entity
    {
        /// <summary>
        /// Use this to verify whether it is safe to call <see cref="EvalNumerical"/>
        /// </summary>
        public bool EvaluableNumerical => Evaled is Complex;

        /// <summary>
        /// Evaluates a given expression to one number or throws exception
        /// </summary>
        /// <returns>
        /// <see cref="Complex"/> since new version
        /// </returns>
        /// <exception cref="CannotEvalException">
        /// Thrown when this entity cannot be represented as a simple number.
        /// <see cref="EvaluableNumerical"/> should be used to check beforehand.
        /// </exception>
        public Complex EvalNumerical() =>
            Evaled is Complex value ? value :
                throw new CannotEvalException
                    ($"Result cannot be represented as a simple number! Use {nameof(EvaluableNumerical)} to check beforehand.");
    }
}