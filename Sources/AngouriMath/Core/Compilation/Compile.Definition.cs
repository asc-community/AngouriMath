/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AngouriMath.Core;
using AngouriMath.Core.Compilation.IntoLinq;
using static AngouriMath.Core.FastExpression;

namespace AngouriMath
{
    partial record Entity
    {
        /// <summary>
        /// Compile function so you can evaluate numerical value 15x faster,
        /// than subsitution
        /// </summary>
        /// <param name="variables">
        /// List string names of variables in the same order as you will list them when evaluating.
        /// Constants, i.e. <see cref="MathS.pi"/> and <see cref="MathS.e"/> will be ignored.
        /// </param>
        /// <returns></returns>
        public FastExpression Compile(params Variable[] variables) => Compiler.Compile(this, variables);

        /// <summary>
        /// Compile function so you can evaluate numerical value 15x faster,
        /// than subsitution
        /// </summary>
        /// <param name="variables">
        /// List string names of variables in the same order as you will list them when evaluating.
        /// Constants, i.e. <see cref="MathS.pi"/> and <see cref="MathS.e"/> will be ignored.
        /// </param>
        /// <returns></returns>
        public FastExpression Compile(params string[] variables) =>
            Compiler.Compile(this, variables.Select(x => (Variable)x));

        /// <summary>
        /// Compiles a given expression into a native lambda
        /// </summary>
        /// <typeparam name="TDelegate">
        /// The type of your delegate to convert to
        /// </typeparam>
        /// <param name="protocol">
        /// This is a protocol, according to which all nodes get compiled. Use this
        /// if you want to use the compilation for types different from those standard
        /// </param>
        /// <param name="typesAndNames">
        /// An <see cref="IEnumerable"/> of pairs, where the first element is the type of your argument,
        /// and the second one is the corresponding variable from the expression
        /// </param>
        /// <returns>
        /// Returnes a natively compiled expression of type <typeparamref name="TDelegate"/>
        /// </returns>
        public TDelegate Compile<TDelegate>(CompilationProtocol protocol, IEnumerable<(Type type, Variable variable)> typesAndNames) where TDelegate : Delegate
            => IntoLinqCompiler.Compile<TDelegate>(this, protocol, typesAndNames);

        /// <summary>
        /// Compiles a given expression into a native lambda. We use the default protocol.
        /// If you plan using non-standard types, consider passing a compilation protocol
        /// </summary>
        /// <typeparam name="TDelegate">
        /// The type of your delegate to convert to
        /// </typeparam>
        /// <param name="typesAndNames">
        /// An <see cref="IEnumerable"/> of pairs, where the first element is the type of your argument,
        /// and the second one is the corresponding variable from the expression
        /// </param>
        /// <returns>
        /// Returnes a natively compiled expression of type <typeparamref name="TDelegate"/>
        /// </returns>
        public TDelegate Compile<TDelegate>(IEnumerable<(Type type, Variable variable)> typesAndNames) where TDelegate : Delegate
            => IntoLinqCompiler.Compile<TDelegate>(this, new(), typesAndNames);

        /// <summary>
        /// Compiles a given expression into a native lambda. We use the default protocol.
        /// If you plan using non-standard types, consider passing a compilation protocol
        /// </summary>
        /// <typeparam name="TDelegate">
        /// The type of your delegate to convert to
        /// </typeparam>
        /// <param name="typesAndNames">
        /// An array of pairs, where the first element is the type of your argument,
        /// and the second one is the corresponding variable from the expression
        /// </param>
        /// <returns>
        /// Returnes a natively compiled expression of type <typeparamref name="TDelegate"/>
        /// </returns>
        public TDelegate Compile<TDelegate>(params (Type type, Variable variable)[] typesAndNames) where TDelegate : Delegate
            => IntoLinqCompiler.Compile<TDelegate>(this, new(), typesAndNames);
    }
}