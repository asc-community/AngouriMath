# Contributing: how to

<a href="https://discord.gg/YWJEX7a"><img alt="Discord" src="https://img.shields.io/discord/642350046213439489?color=darkgreen&label=Join+our+chat!&logo=discord&style=flat&labelColor=474"></a>

We warmly welcome any contributors and contributions to our project. You should not hesitate to open pull requests and ask us about any issue you encounter while writing your code. Remember: if you are here, you already want to help the project, so feel free to ask anything. Not to break this cozy atmosphere, there are a few recommendations to follow.

## Developer guide

### Building and testing

If you are not on Windows, static analyzers and some samples might not be built. However, AngouriMath, AngouriMath.FSharp, AngouriMath.Interactive and AngouriMath.CPP are all buildable on Windows, Linux and MacOS, as well as the tests for them. There is no explicit build script, as everything is built in a normal way via both GUI and CLI. CLI:
```
cd Sources
dotnet build
```
This will build your solution successfully, if you are on Windows. Otherwise, you might need to build projects separately -- `dotnet build` takes a path, since `-p` is short for `--property` and not for a project:
```
cd Sources
dotnet build AngouriMath/AngouriMath.csproj
cd Wrappers
dotnet build AngouriMath.FSharp/AngouriMath.FSharp.fsproj
dotnet build AngouriMath.Interactive/AngouriMath.Interactive.fsproj
```

Running tests is no more complicated:
```
cd Sources/Tests
dotnet test UnitTests
dotnet test FSharpWrapperUnitTests
```

Every test in `UnitTests` carries an `Area` trait naming the directory it lives in, so while
you are working on one part of the library you can run just that part:

```
dotnet test UnitTests --filter "Area=Calculus"
```

The areas are `Algebra`, `Calculus`, `Common`, `Convenience`, `Core`, `Discrete` and
`PatternsTest`, and every test has exactly one -- the seven of them add up to the whole
suite, so nothing is missed by running them all separately.

**Run the whole suite before opening a pull request.** The traits are for the loop you run
while writing the change, not for what you check before proposing it: this library is full
of rules that interact, and a change to simplification has broken solving and integration
here more than once.

Use `Sources/Samples/Samples/Playground.csproj` as a sandbox project, where you can manually test anything you want.

### Tips for working with git

You should be familiar with `git` if you want to contribute to the project. As a good tip, however, we provide a sample set of commands for basic needs.

Adding upstream:
```
git remote add upstream https://github.com/asc-community/AngouriMath
```

Adding a branch based on AngouriMath/master to your fork:
```
git checkout upstream/master
git pull upstream master
git switch -c my-branch
git push --set-upstream origin my-branch
```

## Contribution guidelines

### Closing an issue

One of the most valuable ways to contribute to the project is to close tickets from [issues](https://github.com/asc-community/AngouriMath/issues). If you wish to work on a card, open a pull request on it -- a draft is fine -- saying `Part of #n`; that is the claim, and nothing else is needed.

Then, when you started working on it, we highly recommend opening a draft pull request as soon as possible. This will help everybody see your changes and potentially help you. Then, once PR is ready, open it and wait for a review.

### Adding your feature or fixing a bug

It is highly encouraged to open an issue first. Once opened, follow the approach described above.

Three things are expected of a pull request here that are not obvious from the code, and all three
are set out at length in [AGENTS.md](AGENTS.md), which applies to humans too:

- **If the same input now gives a different answer, it goes in
  [BREAKING-CHANGES.md](BREAKING-CHANGES.md)** -- including when the old answer was wrong -- with the
  old value, the new one and why, measured on a build of each version rather than read off the diff.
  A test you had to change is the usual sign that you owe an entry.
- **The corpus gate runs on every commit.** `Sources/Tests/UnitTests/Corpus` reports solved, wrong,
  error and timeout counts, and it fails both when the library gets worse *and* when it gets better
  without the record being updated to say so. The second is not a nuisance: an improvement nobody
  wrote down is an improvement nobody can tell from a fluke later.
- **Read your pull request's own thread before it merges.** Comments arrive after the checks go
  green, so a PR that was clear when it was opened need not still be, and the thread and the
  comments left on the diff are two separate places.

### Types of issues

An issue's *kind* is its GitHub issue type, not a label; the labels say what state it is in and
where it belongs.

- **No type** -- a goal: what you want to be true, and the easiest issue to write. A goal is a
  meta-issue that spawns work items -- the tracker's `Goal: ...` issues, the dockets, and anything
  a contributor states without knowing whether it is a bug or a feature. The bar is minimal on
  purpose: it is mathematics, and we do all of it; the triage sorts a goal into the Bugs and
  Features it needs, as sub-issues of it, so a blank issue and a goal are the same thing and
  nothing has to be forced onto an issue whose kind is not known yet.
- **Bug** -- the behaviour is not what the mathematics says, or the library crashes, hangs or
  answers something it should have declined. A bug whose impact is low (a simplification that is
  merely not as good as it could be) is still a Bug; say so in the body.
- **Feature** -- an idea, a request, a design: what used to carry the `Proposal` label. If the idea
  is a good one and is going to be implemented, it is marked `Accepted`; if it cannot be implemented
  any time soon, `Not now`. A Feature without `Accepted` is not agreed: comment on it, do not
  implement it.

Questions and requests for opinions are **Discussions**, not issues -- the Q&A and Ideas
categories -- and are answered there; an issue that turns out to be one is redirected and, once
answered, closed unless a work item came of it. `up-for-grabs` marks an issue reserved for a
newcomer.

Who is *working* an issue is whoever has an open pull request on it, draft or not, saying
`Part of #n`: the pull request is the claim, a week without a push or a comment on it makes the
claim stale, and an issue that is several pull requests' worth of work is split into sub-issues
that are claimed one at a time. The assignee field is a queue -- who means to take an issue
next -- and never a lock. The rules the agents follow for this are item 10 of the working
practice in [AGENTS.md](AGENTS.md).

`Area: *` - a number of labels for issues, which are only specific to one of the wrappers: AngouriMath.FSharp, AngouriMath.Interactive, AngouriMath.CPP.

### Contributing details

You might have some questions about the way you should write your code, or how you could call a function, etc. It is recommended to check the [documentation](./Sources/AngouriMath/Docs/Contributing).

## Architecture of the project

There are a few projects the repository contains:
```
AngouriMath --+-> AngouriMath.FSharp --> AngouriMath.Interactive --> AngouriMath.Terminal
              |
              +-> AngouriMath.CPP
```

Four of those are published to NuGet: `AngouriMath`, `AngouriMath.FSharp`,
`AngouriMath.Interactive` and `AngouriMath.Terminal`. The C++ wrapper builds and is tested but is
not packaged, and there is no `AngouriMath.Experimental` project -- `MathS.Experimental` is a class
inside the kernel.
