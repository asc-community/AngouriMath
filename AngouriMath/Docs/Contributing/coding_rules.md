## Coding rules

It is highly recommended to follow them to avoid rewriting your code after submitting a PR.

### OOP

Each inheritable type is either abstract or sealed.

### Immutability

It should be guaranteed that the user cannot change fields of a record which is inherited from `Entity`. 