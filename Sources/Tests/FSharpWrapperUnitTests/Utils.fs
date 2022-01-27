module AngouriMath.FSharp.Tests.Utils

open AngouriMath
open Xunit

let testEqual (a : Entity, b : Entity) =
    match a, b with
    | :? Entity.Matrix as m1, (:? Entity.Matrix as m2) ->
        if m1 <> m2 then
            Assert.False(true, $"First:\n{m1.ToString(true)}\n\nSecond:\n{m2.ToString(true)}")
    | otherA, otherB -> Assert.Equal(otherA, otherB)