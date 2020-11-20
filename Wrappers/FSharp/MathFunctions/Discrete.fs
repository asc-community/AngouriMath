module Discrete

open Core
open AngouriMath

let conj a b = MathS.Conjunction(parse a, parse b)

let disj a b = MathS.Disjunction(parse a, parse b)

let impl assum concl = MathS.Implication(parse assum, parse concl)

let neg a = MathS.Negation(parse a)

let xor a b = MathS.ExclusiveDisjunction(parse a, parse b)


let union a b = MathS.Union(parse a, parse b)

let intersect a b = MathS.Intersection(parse a, parse b)

let set_subtraction a b = MathS.SetSubtraction(parse a, parse b)