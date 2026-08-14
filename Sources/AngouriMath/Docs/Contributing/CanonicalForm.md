# What canonical form means here, and how it differs from simplest

[#746](https://github.com/asc-community/AngouriMath/issues/746) tier 1 asks for two things this
file is: a written specification of what canonical means for each node class, and **a stated
distinction between canonical and "simplest"**. Its item 65 says to take a position, write it down,
and let the engine be checked against it. The checking is `work/canoncheck`, and every number below
came out of it rather than out of an argument.

Read [SimplificationContract.md](SimplificationContract.md) first if you are changing a rewrite.
That file is about whether a rewrite is *sound*. This one is about what shape the expression it is
handed is in, and what a caller may conclude from two expressions having the same shape.

---

## 1. The position, in one paragraph

**Canonical is about identity. Simplest is about presentation. They are different jobs, they want
different machinery, and conflating them is how a library ends up with neither.**

A *canonical form* is a function `c` on expressions such that `c(a)` and `c(b)` are the **identical
tree** whenever `a` and `b` denote the same mathematical object. Its whole purpose is that equality
becomes a structural comparison: you get to decide `a = b` by canonicalising both and comparing
nodes. It says nothing about whether the result is nice to look at.

A *simplest form* is the best-rated member of an equivalence class under a cost metric. The metric
is a caller's choice — smallest tree, fewest radicals, numerically stablest, most readable to a
student — so "simplest" is only defined relative to one, and two different metrics give two
different answers that are both right.

The library has one of each, and they are `InnerSimplified` and `Simplify`. Neither is currently
canonical; §3 measures how far off each is, and the answer is not the one the names suggest.

## 2. A complete canonical form does not exist, and that is a theorem rather than a gap

For the class of expressions AngouriMath accepts — rationals, `pi`, the exponential, the
trigonometric functions, `abs`, and composition — **deciding whether an expression is zero is
undecidable.** That is Richardson's theorem (Daniel Richardson, *Some undecidable problems involving
elementary functions of a real variable*, J. Symbolic Logic 33 (1968), 514–520), and since a
canonical form would decide zero-equivalence by canonicalising and comparing against `0`, no
canonical form exists for the whole language.

So the specification cannot be "canonicalise everything", and any roadmap item that reads that way
is asking for something that is not there. What it can be, and what the rest of this file is:

1. **A canonical form on a decidable sublanguage**, stated exactly, with the boundary written down.
2. **A normalisation everywhere else** — idempotent, order-independent, and cheap — which is not
   canonical and must not be relied on as though it were.
3. **A search** for a presentable form, which is `Simplify`, and which is explicitly not required to
   be canonical.

This is the classical three-way split — Joel Moses, *Algebraic simplification: a guide for the
perplexed*, CACM 14 (1971), 527–537 — and taking the position costs nothing except writing it down,
which is what tier 1 asked for.

## 3. Where the library actually stands, measured

Three properties, each of which a canonical form must have and none of which needs an oracle:

- **idempotence** — applying the form twice is applying it once;
- **order independence** — the operands of a commutative operator may be written either way round;
- **agreement** — two writings of one expression reach the same form.

`work/canoncheck` measures all three against both candidates, comparing **entities and not printed
strings**. Measured on `master`:

| | `InnerSimplified` | `Simplify` |
|---|---|---|
| idempotence | 1 failed of 834 | **0 failed of 120** |
| order independence | 2024 failed of 2738 | 8 failed of 72 |
| listed agreements | 20 failed of 30 | 6 failed of 30 |

**Neither is canonical, and `Simplify` is much the closer of the two.** That is worth stating
plainly because the names imply the opposite: `InnerSimplified` sounds like the normal form and
`Simplify` like the pretty-printer, and it is the other way round. `InnerSimplified` does not order
the operands of a sum or a product at all, so it fails three quarters of the order checks by
construction; `Simplify` reorders as a side effect of searching and rating candidates, and agrees
far more often — but not always, because when two candidates rate equal the tie goes to whichever
was generated first, which depends on the input order. What ranks them is
`MathS.Settings.ComplexityCriteria` — `SimplifiedRate`, a weighted count — and not the node count
that `Complexity` returns; ties between differently-shaped forms are common.

### The finding that matters most for anyone writing a rule

`(x + y) + a` and `x + (y + a)` **both print as `x + y + a` and are different trees.** Associativity
is normalised in the printer, not in the expression. A rule, a test or a cache that compares printed
forms will call them equal; one that compares entities will not. Compare entities.

### Two defects it turned up, rather than decisions

- `cos(0 ^ y)` is not idempotent under `InnerSimplified`: the first pass leaves `-(-1)` at the head
  of the answer and the second folds it to `1`. `-(-1)` on its own folds immediately, so a rewrite
  is building it above already-normalised children and returning without re-normalising.
- `cos(-x)`, `sin(-x)`, `tan(-x)` and `abs(-x)` are left alone by `Simplify`, while `cos(-2 * x)`
  folds to `cos(2 * x)`. The parity identities are keyed on a shape that a bare negation does not
  have.

## 4. What canonical means per node class

This is the **target**, not a description of today. Where the library already meets a line it is
marked; where it does not, the line is what `canoncheck` should eventually assert.

### Leaves

| class | canonical form |
|---|---|
| `Integer`, `Rational` | lowest terms, denominator positive; a rational with denominator one is an `Integer` — **met** |
| `Real`, `Complex` | a complex with zero imaginary part is a `Real`; likewise down the tower — **met** |
| `Variable` | itself; the name is the identity |
| `Boolean` | `True` or `False` |

### Commutative operators

| class | canonical form |
|---|---|
| `Sumf` | flat over nesting, operands in a total order, numeric operands folded into one leading term, a zero term dropped — **not met**: neither flattened nor ordered in the tree |
| `Mulf` | flat, ordered, numeric factors folded into one leading factor, a one factor dropped, a zero factor collapsing the product — **not met**, as above |
| `Andf`, `Orf`, `Xorf` | flat, ordered, constants folded, duplicates dropped — **not met** |
| `Unionf`, `Intersectionf` | flat, ordered by the element order — **not met** |

A **total order on operands** is what makes ordering decidable, and it has to be specified rather
than left to whatever a sort happens to do: by node class first in a fixed class order, then by the
class's own key — a number by value, a variable by name, a function by its name and then
lexicographically by its already-ordered children. It has to be total and stable, and it has to be
independent of how the expression was written, which is exactly what the order checks test.

### Operators that are sugar for a commutative one

| class | canonical form |
|---|---|
| `Minusf` | absent — `a - b` is `a + (-1) * b` |
| `Divf` | absent — `a / b` is `a * b ^ (-1)` |

**This is a position and it is not free.** It is the one that makes the commutative laws reachable:
while a difference is its own node, `x - y` and `x + (-y)` are different trees denoting one thing —
measured, both listed pairs disagree — and no amount of ordering sums fixes it, because one of them
is not a sum. The cost is that the printer must put subtraction and division back, or every output
regresses into `a + (-1) * b`. The printer already does exactly this for a negative coefficient,
which is why `(-1) * x` prints `-x`; the change is to stop the *tree* from having two shapes, not to
change what is shown.

**This is the one line here that needs a maintainer's yes before anybody implements it.** Removing
two node classes touches pattern matching, every `switch` over the hierarchy, the parser's output,
the exporters and `AddingNode.cs`, and it is a breaking change to anything matching on `Divf`. It is
written down as the position because a specification that ducks the question is not one; it is not
written down as a decision already taken.

### Powers

| class | canonical form |
|---|---|
| `Powf` | a numeric exponent in lowest terms; `x ^ 1` is `x`; `x ^ 0` is `1` **only where the base is known non-zero**, and otherwise carries the condition; a power of a power multiplies its exponents only where that is sound over the complex plane |

`x ^ 0` is the standing example of a canonicalisation that a naive specification gets wrong:
measured, the library answers `1 provided not x = 0`, and that is right. `0 ^ 0` is not `1`. A
canonical form that maps `x ^ 0` to `1` unconditionally has made the library answer wrongly to buy
itself a tidier rule, which
[AGENTS.md](../../../../AGENTS.md) forbids in the first line.

Likewise `sqrt(x)` and `x ^ (1/2)` **must** reach the same form — they denote the same principal
branch — and `(x ^ 2) ^ 3` and `x ^ 6` must not be assumed to, since `(x ^ a) ^ b` is `x ^ (a * b)`
only under conditions the contract file states.

### Functions

| class | canonical form |
|---|---|
| `Absf`, `Cosf`, and the even functions | the argument's own canonical form, with a leading negation removed — **not met**, see §3 |
| `Sinf`, `Tanf`, and the odd functions | a leading negation lifted out of the argument to the front of the node — **not met** |
| `Logf` | the base and the argument each canonical; no rewriting between bases, since that is a rule's decision and not a form's |
| everything else | children canonical, node unchanged |

### Nodes that are not values

| class | canonical form |
|---|---|
| `Providedf` | the condition itself canonical, and a `Providedf` never nested inside another — `(a provided p) provided q` is `a provided p and q` |
| `Derivativef`, `Integralf`, `Limitf` | children canonical; the node is a request, and a form must not evaluate it |
| `Setf` and the set classes | a finite set ordered and deduplicated — **met**, measured |

## 5. Where a canonical form is actually available: the polynomial sublanguage

§2 rules out canonicalising everything. It does not rule out canonicalising the part of the language
where zero-equivalence *is* decidable, and that part is large and useful: **rational functions over
`Q` in finitely many variables.** For those, a canonical form exists, is classical, and — since
[#918](https://github.com/asc-community/AngouriMath/pull/918) and
[#923](https://github.com/asc-community/AngouriMath/pull/923) — is now buildable out of parts the
library has:

- a quotient of two polynomials, each expanded and with a specified monomial order;
- divided through by their multivariate GCD, which `PolynomialGcd` computes;
- with the denominator made monic in the order, the sign carried in the numerator;
- and the coefficients rationals in lowest terms.

Two rational functions are equal exactly when this form is identical, so the equality question is
decided rather than searched. This is the concrete deliverable behind tier 1's "a specified
canonicaliser", and it is the piece to build first, because it is the piece that is *possible*.

The boundary has to be stated at the API rather than hidden: a caller asking to canonicalise
`sin(x) + 1` must be told that no canonical form is claimed, not handed a normalisation that looks
like one. **A form that is canonical on part of the language and silently approximate on the rest is
worse than no form at all**, because its whole value was that structural equality meant something.

## 6. What `Simplify` is, and what it is not required to be

`Simplify` searches: it generates candidates by applying rewrites and returns the best by
`MathS.Settings.ComplexityCriteria`. It is therefore

- **metric-relative** — a different criterion is a different answer, and that is intended;
- **not required to be canonical** — measured, it fails 8 of 72 order checks, all of them ties
  broken by generation order;
- **not required to be idempotent**, though measured it is, at 120 of 120;
- **not a decision procedure for equality**. `a.Simplify() == b.Simplify()` failing proves nothing
  at all. Use the residual — `(a - b).Simplify()` against zero — and even that is a semi-decision,
  by §2.

The one thing it *is* required to be is sound, which is [SimplificationContract.md](SimplificationContract.md).

## 7. How this is checked

`work/canoncheck`. It builds expressions by growing a small grammar, then checks idempotence and
order independence generatively and a listed set of agreements by hand. Its three counts are the
thing to watch; the listed pairs are each a claim this file makes or disclaims, so a disagreement
there is a decision to take rather than necessarily a defect.

It compares entities. Nothing in it reads a printed form, which is deliberate: the associativity
finding in §3 is invisible to a string comparison and is the single most likely thing to be got
wrong by a test written in a hurry.

## 8. What is owed

1. A total order on operands, specified here and implemented once, rather than per node class.
2. Flattening of sums and products in the tree rather than in the printer.
3. The two defects in §3 — the non-idempotent `-(-1)`, and the parity identities that miss a bare
   negation.
4. The rational-function canonical form of §5, as an explicit operation with the boundary in its
   signature.
5. `canoncheck` in CI once the counts are meant to be zero, which they are not yet. Until then it is
   a measurement, and its numbers belong in a commit message rather than in a gate.
