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
        /// <param name="returnType">
        /// The type to which the resulting type will be casted
        /// </param>
        /// <param name="typesAndNames">
        /// An <see cref="IEnumerable"/> of pairs, where the first element is the type of your argument,
        /// and the second one is the corresponding variable from the expression
        /// </param>
        /// <returns>
        /// Returnes a natively compiled expression of type <typeparamref name="TDelegate"/>
        /// </returns>
        public TDelegate Compile<TDelegate>(CompilationProtocol protocol, Type returnType, IEnumerable<(Type type, Variable variable)> typesAndNames) where TDelegate : Delegate
            => IntoLinqCompiler.Compile<TDelegate>(this, returnType, protocol, typesAndNames);

        /// <summary>
        /// Compiles a given expression into a native lambda. We use the default protocol.
        /// If you plan using non-standard types, consider passing a compilation protocol
        /// </summary>
        /// <typeparam name="TIn1">
        /// The type of the passed argument
        /// </typeparam>
        /// <typeparam name="TOut">
        /// The return type
        /// </typeparam>
        /// <param name="var1">
        /// The variable corresponding to the function's argument
        /// </param>
        /// <returns>
        /// Returns a natively-compiled delegate
        /// </returns>
        public Func<TIn1, TOut> Compile<TIn1, TOut>(Variable var1)
            => IntoLinqCompiler.Compile<Func<TIn1, TOut>>(this, typeof(TOut), CompilationProtocol.Assume<TIn1, TOut>(), new[] { (typeof(TIn1), var1) });

        /// <summary>
        /// Compiles a given expression into a native lambda. We use the default protocol.
        /// If you plan using non-standard types, consider passing a compilation protocol
        /// </summary>
        /// <typeparam name="TIn1">
        /// The type of the passed argument
        /// </typeparam>
        /// <typeparam name="TIn2">
        /// The type of the passed argument
        /// </typeparam>
        /// <typeparam name="TOut">
        /// The return type
        /// </typeparam>
        /// <param name="var1">
        /// The variable corresponding to the first function's argument
        /// </param>
        /// <param name="var2">
        /// The variable corresponding to the second function's argument
        /// </param>
        /// <returns>
        /// Returns a natively-compiled delegate
        /// </returns>
        public Func<TIn1, TIn2, TOut> Compile<TIn1, TIn2, TOut>(Variable var1, Variable var2)
            => IntoLinqCompiler.Compile<Func<TIn1, TIn2, TOut>>(this, typeof(TOut), CompilationProtocol.Assume<TIn1, TIn2, TOut>(), new[] { (typeof(TIn1), var1), (typeof(TIn2), var2) });

        /// <summary>
        /// Compiles a given expression into a native lambda. We use the default protocol.
        /// If you plan using non-standard types, consider passing a compilation protocol
        /// </summary>
        /// <typeparam name="TIn1">
        /// The type of the passed argument
        /// </typeparam>
        /// <typeparam name="TIn2">
        /// The type of the passed argument
        /// </typeparam>
        /// <typeparam name="TIn3">
        /// The type of the passed argument
        /// </typeparam>
        /// <typeparam name="TOut">
        /// The return type
        /// </typeparam>
        /// <param name="var1">
        /// The variable corresponding to the first function's argument
        /// </param>
        /// <param name="var2">
        /// The variable corresponding to the second function's argument
        /// </param>
        /// <param name="var3">
        /// The variable corresponding to the third function's argument
        /// </param>
        /// <returns>
        /// Returns a natively-compiled delegate
        /// </returns>
        public Func<TIn1, TIn2, TIn3, TOut> Compile<TIn1, TIn2, TIn3, TOut>(Variable var1, Variable var2, Variable var3)
            => IntoLinqCompiler.Compile<Func<TIn1, TIn2, TIn3, TOut>>(this, typeof(TOut), CompilationProtocol.Assume<TIn1, TIn2, TIn3, TOut>(), new[] { (typeof(TIn1), var1), (typeof(TIn2), var2), (typeof(TIn3), var3) });
    }
}