## Documentation for contributors and developers

This folder contains documentation required for proper contributing to the project. Some files are in `.cs` so that
it is easier to go through links.

If you aren't sure about what to add, you may want to check the current projects
[here](https://github.com/asc-community/AngouriMath/projects).

#### Table of content
1. <a href="./General.md">General information</a>
2. <a href="./AddingNode.cs">Adding a new node (function, operator)</a>
3. <a href="./ImproveParser.md">Improve parser</a>
4. <a href="./Transformations.md">Transformations</a> — the layer the 1.x entry points sit on, and
   how to add the next rule set or transformation
5. <a href="./SimplificationContract.md">What a simplification rule may assume</a> — read before
   adding or changing a rewrite. What has to be true for it to be sound, the four things that are
   not the same, and the branch-cut conventions this library commits to
6. <a href="./CanonicalForm.md">What canonical form means here</a> — and how it differs from
   "simplest". Why no canonical form exists for the whole language, what one means per node class,
   and where the library measurably stands against it
7. <a href="./coding_rules.md">Coding rules</a> — sealed-or-abstract, immutability, and what may be
   made `public`

See also <a href="../../../../BREAKING-CHANGES.md">BREAKING-CHANGES.md</a>, where a change that makes
the same input give a different answer is recorded, and
<a href="../../../../AGENTS.md">AGENTS.md</a>, which says why correctness is allowed to break
compatibility in the first place.