## Public methods and classes

When adding a class for purposes inside the library, you must not mark them as `public`.
When adding a method, make sure that it's either not `public` or the class you are adding the method to
is not `public`.

However, if you intentionally want to add a publicly-exposed functional, you will get the 
<a href="https://github.com/dotnet/roslyn-analyzers/blob/master/src/PublicApiAnalyzers/Microsoft.CodeAnalysis.PublicApiAnalyzers.md#rs0016-add-public-types-and-members-to-the-declared-api">RS0016 error</a>.

That means you're required to report about this public method or class. You can simply add it to the 
PublicApi.*.txt file or hit `ctrl + .` to do it.