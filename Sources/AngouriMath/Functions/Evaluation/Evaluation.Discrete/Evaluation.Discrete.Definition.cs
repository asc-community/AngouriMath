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
        /// Use this to verify whether it is safe to call <see cref="EvalBoolean"/>
        /// </summary>
        public bool EvaluableBoolean => Evaled is Boolean;

        /// <summary>
        /// Evaluates a given expression to one boolean or throws exception
        /// </summary>
        /// <returns>
        /// <see cref="Boolean"/>
        /// </returns>
        /// <exception cref="CannotEvalException">
        /// Thrown when this entity cannot be represented as a simple boolean.
        /// <see cref="EvalBoolean"/> should be used to check beforehand.
        /// </exception>
        public Boolean EvalBoolean() =>
            Evaled is Boolean value ? value :
                throw new CannotEvalException
                    ($"Result cannot be represented as a simple boolean! Use {nameof(EvaluableBoolean)} to check beforehand.");
    }
}