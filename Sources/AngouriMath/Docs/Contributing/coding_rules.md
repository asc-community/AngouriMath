## Coding rules

It is highly recommended to follow them to avoid rewriting your code after submitting a PR.

### OOP

Each inheritable type is either abstract or sealed.

### Immutability

It should be guaranteed that the user cannot change fields of a record which is inherited from `Entity`.

### Visibility

Anything added for the library's own purposes is not `public`. A type used internally stays
`internal`; a method added to a `public` type stays `internal` or `private` unless it is meant for
callers. `public` is a promise that
[BREAKING-CHANGES.md](../../../../BREAKING-CHANGES.md) then has to keep, so it is worth making
deliberately rather than by default.

This used to be enforced by the `PublicApiAnalyzers` package, which required every public member to
be listed in a `PublicApi.*.txt`. Neither the package nor those files are in the tree any more, so
nothing checks it — which makes it a rule to follow rather than one to be caught by. 