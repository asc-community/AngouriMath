//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Linq;
using AngouriMath;
using AngouriMath.Core;
using Xunit;

namespace AngouriMath.Tests.Common
{
    /// <summary>
    /// A node's <see cref="Entity.Codomain"/> survives being printed and read back.
    /// </summary>
    /// <remarks>
    /// <para>
    /// It did not until
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1022">#1022</a>: nothing
    /// printed it, so <c>domain(x, ZZ)</c> printed as <c>x</c> and came back a different node.
    /// The codomain is not decoration — <c>sqrt(-1)</c> is <c>i</c> and the same expression over
    /// the reals is <see cref="MathS.NaN"/> — so the two printed the same string and evaluated
    /// differently.
    /// </para>
    /// <para>
    /// Everything here compares entities. A printed form is what the assertion is about only
    /// where the assertion is that the printed form did <em>not</em> change.
    /// </para>
    /// </remarks>
    [Trait("Area", "Common")]
    public sealed class CodomainSurvivesPrintingTest
    {
        /// <summary>The domains a <c>domain(...)</c> can name, which is every one but <see cref="Domain.Any"/>.</summary>
        private static readonly Domain[] Writable =
        {
            Domain.Boolean, Domain.Integer, Domain.Rational, Domain.Real, Domain.Complex
        };

        public static IEnumerable<object[]> EveryNodeAndEveryWritableDomain() =>
            from node in EveryNodeSurvivesEveryPipelineTest.EveryNodeType().Select(row => (Entity)row[0])
            from domain in Writable
            select new object[] { node, domain };

        /// <summary>
        /// The annotated forms that still do not read back, keyed by what they print as, with
        /// the reason. Held strictly in both directions, so an entry that starts round tripping
        /// fails the test rather than sitting here describing something that stopped being true.
        /// </summary>
        /// <remarks>
        /// The one entry is a <em>parser</em> gap, not a printing one. No input string at all
        /// yields a <see cref="Entity.Number.Rational"/> whose codomain is
        /// <see cref="Domain.Complex"/>: the pass that reads a quotient of two integer literals
        /// as the rational it denotes
        /// (<a href="https://github.com/asc-community/AngouriMath/issues/873">#873</a>) uses
        /// <see cref="Domain.Complex"/> as its "nobody annotated this" sentinel, because that is
        /// what an un-annotated <see cref="Entity.Divf"/> carries, and drops it. So
        /// <c>domain(1/2, CC)</c> parses to the same node <c>1/2</c> does, and there is nothing
        /// the printer can emit instead.
        /// </remarks>
        private static readonly Dictionary<string, string> StillUnparseable = new(StringComparer.Ordinal)
        {
            ["domain(1/2, CC)"] =
                "the parser's integer-quotient-to-rational pass reads Complex as `not annotated`, "
                + "so no input produces a Rational whose codomain is Complex",
        };

        /// <summary>
        /// Every node type there is, narrowed to every domain that can be written down, prints
        /// and reads back as itself.
        /// </summary>
        /// <remarks>
        /// The sample list is <see cref="EveryNodeSurvivesEveryPipelineTest.EveryNodeType"/>'s,
        /// which fails the day a node type is added without one — so this cannot silently stop
        /// covering a node either.
        /// </remarks>
        [Theory]
        [MemberData(nameof(EveryNodeAndEveryWritableDomain))]
        public void ANarrowedNodeReadsBackAsItself(Entity node, Domain domain)
        {
            // A node whose bare form does not round trip cannot have its annotated form round
            // trip either, and the reason would be the older defect rather than this one.
            // EveryNodeSurvivesEveryPipelineTest.StringizeRoundTripsToTheSameNode owns those.
            if (MathS.FromString(node.Stringize()) != node)
                return;

            var narrowed = node.WithCodomain(domain);
            var printed = narrowed.Stringize();
            var back = MathS.FromString(printed);

            if (StillUnparseable.TryGetValue(printed, out var reason))
            {
                Assert.True(narrowed != back,
                    $"{printed} reads back as itself now — drop it from "
                    + $"{nameof(StillUnparseable)}, where it reads \"{reason}\"");
                return;
            }

            Assert.Equal(narrowed, back);
            Assert.Equal(domain, back.Codomain);
        }

        /// <summary>
        /// A codomain on a subnode is carried too: the annotation belongs to whichever node has
        /// it, not to the expression as a whole.
        /// </summary>
        [Fact]
        public void ACodomainDeepInTheTreeIsPrintedWhereItSits()
        {
            var x = MathS.Var("x");
            var y = MathS.Var("y");
            Entity expression = MathS.Sin(x.WithCodomain(Domain.Integer)) + y.WithCodomain(Domain.Real);

            Assert.Equal("sin(domain(x, ZZ)) + domain(y, RR)", expression.Stringize());
            Assert.Equal(expression, MathS.FromString(expression.Stringize()));
        }

        /// <summary>
        /// The examples on the issue, measured.
        /// </summary>
        [Theory]
        [InlineData("domain(x, ZZ)")]
        [InlineData("domain(x + 1, RR)")]
        [InlineData("domain(sqrt(-1), RR)")]
        [InlineData("domain([1, 2], RR)")]
        public void TheReportedExpressionsReadBackAsThemselves(string source)
        {
            var parsed = MathS.FromString(source);
            Assert.Equal(parsed, MathS.FromString(parsed.Stringize()));
        }

        /// <summary>
        /// Nothing is added to the ordinary case. A node carrying the codomain its own type
        /// carries by default is what the bare text already means, so wrapping it would put a
        /// <c>domain(...)</c> around every expression the library prints.
        /// </summary>
        [Theory]
        [MemberData(nameof(EveryNodeSurvivesEveryPipelineTest.EveryNodeType), MemberType = typeof(EveryNodeSurvivesEveryPipelineTest))]
        public void AFreshNodeCarriesItsDefaultCodomainAndPrintsNoAnnotation(Entity node)
        {
            foreach (var subnode in node.Nodes)
            {
                Assert.False(subnode.PrintsItsCodomain,
                    $"{subnode.GetType().Name} was built without anything narrowing it and still "
                    + $"prints an annotation: Codomain is {subnode.Codomain} where DefaultCodomain "
                    + $"is {subnode.DefaultCodomain}. The two are declared side by side in "
                    + "Domains.Classes.cs and have to name the same domain.");
                Assert.Equal(subnode.DefaultCodomain, subnode.Codomain);
            }
        }

        /// <summary>
        /// A narrowing that changes nothing prints nothing: <see cref="Entity.WithCodomain"/> to
        /// the default is the identity, and an expression that has been through it is
        /// indistinguishable from one that has not.
        /// </summary>
        [Theory]
        [InlineData("x + 1", Domain.Complex)]
        [InlineData("x", Domain.Any)]
        [InlineData("abs(x)", Domain.Real)]
        [InlineData("a and b", Domain.Boolean)]
        [InlineData("[1, 2]", Domain.Any)]
        public void NarrowingToTheDefaultLeavesTheOutputAlone(string source, Domain @default)
        {
            var expression = MathS.FromString(source);
            Assert.Equal(@default, expression.Codomain);
            Assert.Equal(source, expression.Stringize());
            Assert.Equal(source, expression.WithCodomain(@default).Stringize());
        }

        /// <summary>
        /// <see cref="Domain.Any"/> is written too, so the printed form no longer loses a
        /// widening. This test used to pin the opposite and is the record of it changing.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1048">#1048</a>
        /// </summary>
        /// <remarks>
        /// <para>
        /// There is still no node for "no restriction" — <see cref="Entity.Set.SpecialSet.Create(Domain)"/>
        /// throws for it — so <c>Any</c> is not a set literal. It is read in the second argument
        /// of <c>domain(...)</c> and nowhere else, which commits to a spelling without deciding
        /// whether there is a universal <em>set</em>.
        /// </para>
        /// <para>
        /// Read rather than lexed, deliberately: a literal in a parser rule becomes a global
        /// lexer token, and making <c>Any</c> a keyword stopped <c>Any + 1</c> parsing at all.
        /// <see cref="AVariableNamedAnyIsStillAVariable"/> is what keeps that from coming back.
        /// </para>
        /// </remarks>
        [Theory]
        [InlineData("x + 1")]
        [InlineData("abs(x)")]
        [InlineData("phi(x)")]
        public void WideningToAnyIsWrittenOut(string source)
        {
            var widened = MathS.FromString(source).WithCodomain(Domain.Any);
            Assert.Equal(Domain.Any, widened.Codomain);
            var printed = widened.Stringize();
            var read = MathS.FromString(printed);
            Assert.Equal(Domain.Any, read.Codomain);
            Assert.Equal(widened, read);
        }

        /// <summary>
        /// And <c>Any</c> is not reserved: it is read as the unrestricted codomain in the one
        /// position where a codomain is expected, and is an ordinary variable everywhere else.
        /// </summary>
        [Theory]
        [InlineData("Any + 1")]
        [InlineData("Any")]
        [InlineData("sin(Any)")]
        public void AVariableNamedAnyIsStillAVariable(string source)
        {
            var parsed = MathS.FromString(source);
            Assert.Contains((Entity.Variable)"Any", parsed.Vars);
            Assert.Equal(source, parsed.Stringize());
        }

        /// <summary>
        /// A sum no longer collects a node and the same node narrowed as one term. `Simplify`'s
        /// polynomial collection keys a monomial by its base's printed form, so while the printed
        /// form did not distinguish the two it added them up — and <c>0</c> is the answer only
        /// where <c>x</c> is an integer, which is exactly what the annotation says it might not
        /// be.
        /// </summary>
        [Theory]
        [InlineData("x - domain(x, ZZ)")]
        [InlineData("domain(x, ZZ) + x")]
        public void ANarrowedTermIsNotTheSameTermAsTheBareOne(string source)
        {
            var simplified = MathS.FromString(source).Simplify();
            Assert.Contains("domain(x, ZZ)", simplified.Stringize(), StringComparison.Ordinal);
        }

        /// <summary>
        /// LaTeX carries it as a subscript. There is no LaTeX parser here, so this is a
        /// statement about the rendering and not a round trip — the round trip for LaTeX is
        /// CSharpMath's, and <a href="https://github.com/asc-community/AngouriMath/issues/822">#822</a>
        /// is where it is tracked.
        /// </summary>
        [Fact]
        public void LatexSubscriptsTheSet()
        {
            Assert.Equal(@"{\left(x\right)}_{\mathbb{Z}}",
                         MathS.FromString("domain(x, ZZ)").Latexize());
            Assert.Equal(@"\sin\left({\left(x\right)}_{\mathbb{Z}}\right)",
                         MathS.FromString("sin(domain(x, ZZ))").Latexize());
            // The default is not rendered, exactly as it is not printed.
            Assert.Equal(@"x+1", MathS.FromString("x + 1").Latexize());
        }

        /// <summary>
        /// The parentheses in the LaTeX form are not decoration: a variable renders its own index
        /// as a subscript, so without them an annotated <c>x</c> and a variable called
        /// <c>x_Z</c> would render the same.
        /// </summary>
        [Fact]
        public void TheLatexParenthesesSeparateTheSetFromAVariableIndex()
        {
            Assert.Equal(@"x_{1}", MathS.Var("x_1").Latexize());
            Assert.NotEqual(MathS.Var("x_1").Latexize(),
                            MathS.FromString("domain(x, ZZ)").Latexize());
        }
    }
}
