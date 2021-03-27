module Compilation

open Core

let compile<'TIn1, 'TOut> x1 expr =
    let parsed = parse expr
    let px1 = symbol x1
    let compiled = parsed.Compile<'TIn1, 'TOut>(px1)
    fun x -> compiled.Invoke(x)

