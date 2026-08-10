# Property check

Each transformation checked against a property it must satisfy, evaluated
numerically rather than compared as text: Simplify, Expand and Factorize must
preserve value; Differentiate must agree with a central difference quotient;
Integrate must differentiate back to the integrand. Points where either side
is undefined or leaves the reals are skipped rather than counted.

- Corpus: **151** expressions
- Checks that ran: **1340**
- Skipped, nothing comparable: **0**
- **Failures: 0**

