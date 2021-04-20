module AngouriMath.FSharp.Compilation

open Core

/// Compiles the given expression into a function
/// Use it for performance. Pass some built-in type,
/// be that a BigInteger, int, long, bool, float, double.
/// If a necessary type is missing, use the original function
/// Entity.Compile<> with a passed Compilation protocol
let compiled1In<'TIn1, 'TOut> x1 expr =
    let compiled = (parsed expr).Compile<'TIn1, 'TOut>(symbol x1)
    fun x1 -> compiled.Invoke(x1)

/// Compiles the given expression into a function
/// Use it for performance. Pass some built-in type,
/// be that a BigInteger, int, long, bool, float, double.
/// If a necessary type is missing, use the original function
/// Entity.Compile<> with a passed Compilation protocol
let compiled2In<'TIn1, 'TIn2, 'TOut> x1 x2 expr =
    let compiled = (parsed expr).Compile<'TIn1, 'TIn2, 'TOut>(symbol x1, symbol x2)
    fun x1 x2 -> compiled.Invoke(x1, x2)

/// Compiles the given expression into a function
/// Use it for performance. Pass some built-in type,
/// be that a BigInteger, int, long, bool, float, double.
/// If a necessary type is missing, use the original function
/// Entity.Compile<> with a passed Compilation protocol
let compiled3In<'TIn1, 'TIn2, 'TIn3, 'TOut> x1 x2 x3 expr =
    let compiled = (parsed expr).Compile<'TIn1, 'TIn2, 'TIn3, 'TOut>(symbol x1, symbol x2, symbol x3)
    fun x1 x2 x3 -> compiled.Invoke(x1, x2, x3)

/// Compiles the given expression into a function
/// Use it for performance. Pass some built-in type,
/// be that a BigInteger, int, long, bool, float, double.
/// If a necessary type is missing, use the original function
/// Entity.Compile<> with a passed Compilation protocol
let compiled4In<'TIn1, 'TIn2, 'TIn3, 'TIn4, 'TOut> x1 x2 x3 x4 expr =
    let compiled = (parsed expr).Compile<'TIn1, 'TIn2, 'TIn3, 'TIn4, 'TOut>(symbol x1, symbol x2, symbol x3, symbol x4)
    fun x1 x2 x3 x4 -> compiled.Invoke(x1, x2, x3, x4)