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
This will build your solution successfully, if you are on Windows. Otherwise, you might need to build projects separately:
```
cd Sources
dotnet build -p AngouriMath
cd Wrappers
dotnet build -p AngouriMath.FSharp
dotnet build -p AngouriMath.Interactive
```

Running tests is no more complicated:
```
cd Sources/Tests
dotnet test UnitTests
dotnet test FSharpWrapperUnitTests
```

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

One of the most valuable ways to contribute to the project is to close tickets from from [projects](https://github.com/asc-community/AngouriMath/projects) or [issues](https://github.com/asc-community/AngouriMath/issues). If you wish to work on a card, ping one of the maintaintainers, for example, @WhiteBlackGoose, and ask for assigning the issue to you.

Then, when you started working on it, we highly recommend opening a draft pull request as soon as possible. This will help everybody see your changes and potentially help you. Then, once PR is ready, open it and wait for a review.

### Adding your feature or fixing a bug

It is highly encouraged to open an issue first. Once opened, follow the approach described above.

### Types of issues

Issues marked with `Proposal` are those suggesting ideas. If the idea is a good one and is going to be implemented, it is marked as `Accepted`. If an idea cannot be implemented any time soon, it is marked as `Not now`.

`Minor bug` and `Bug` are applied to an issue after it's clear, that the behaviour is not desired. `Minor bug` is for cases, when despite that the behaviour is undesired, the impact is low (for example, in case if a simplificator doesn't simplify well enough). `Bug` reflects serious issues.

`Opinions wanted` - anybody is welcomed to share their opinion on a subject.

`Area: *` - a number of labels for issues, which are only specific to one of the wrappers: AngouriMath.FSharp, AngouriMath.Interactive, AngouriMath.CPP.

### Contributing details

You might have some questions about the way you should write your code, or how you could call a function, etc. It is recommended to check the [documentation](./Sources/AngouriMath/Docs/Contributing).

## Architecture of the project

There are a few projects the repository contains:
```
              +-> AngouriMath.Experimental
              |
AngouriMath --+-> AngouriMath.FSharp --> AngouriMath.Interactive --> AngouriMath.Terminal
              |
              +-> AngouriMath.CPP
```
