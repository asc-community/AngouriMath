//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using AngouriMath.Core.Serialization;
using Xunit;

namespace AngouriMath.Tests.Common
{
    /// <summary>
    /// An <see cref="Entity"/> written out and read back is the entity that was written.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Asserted against entities and never against the JSON text, except in the one test that is
    /// about the text. What is serialized is the printed form, so a test that compared strings
    /// would be re-testing <c>Stringize</c> and would move whenever printing moved.
    /// </para>
    /// <para>
    /// The node types are enumerated rather than listed, the same way
    /// <see cref="EveryNodeSurvivesEveryPipelineTest"/> does it, so a node type added later is
    /// covered on the day it is added rather than on the day somebody remembers.
    /// </para>
    /// https://github.com/asc-community/AngouriMath/issues/323
    /// </remarks>
    [Trait("Area", "Common")]
    public sealed class EntitySerializationTest
    {
        private static readonly Entity.Variable X = MathS.Var("x");

        /// <summary>Every concrete node type there is; abstract is the only exclusion.</summary>
        private static IEnumerable<Type> ConcreteNodeTypes =>
            typeof(Entity).Assembly.GetTypes()
                .Where(t => !t.IsAbstract && typeof(Entity).IsAssignableFrom(t))
                .OrderBy(t => t.FullName, StringComparer.Ordinal);

        /// <summary>
        /// One sample per node type reflective construction cannot reach — the constructor takes
        /// something that is not an <see cref="Entity"/>, or there is no public one at all.
        /// </summary>
        private static readonly Entity[] HandBuilt =
        {
            MathS.Apply(X, X),                                  // Application
            Entity.Boolean.True,                                // Boolean
            MathS.Derivative(X, X, 2),                          // Derivativef, with an iteration count
            MathS.Integral(X, X),                               // Integralf, indefinite
            MathS.Integral(X, X, 0, 1),                         // Integralf, over a range
            MathS.Lambda(X, X),                                 // Lambda
            MathS.Limit(X, X, 0),                               // Limitf
            MathS.Vector(1, 2),                                 // Matrix, one row
            MathS.Matrix(new Entity[,] { { 1, 2 }, { 3, 4 } }), // Matrix, two by two
            3,                                                  // Integer
            Entity.Number.Rational.Create(1, 2),                // Rational
            MathS.pi.Evaled,                                    // Real
            Entity.Number.Complex.Create(1, 2),                 // Complex
            MathS.Piecewise((X, X > 0), ((Entity)1, X <= 0)),   // Piecewise
            MathS.Sets.Finite(1, 2),                            // FiniteSet
            MathS.Interval(0, 1),                               // Interval
            MathS.Sets.C,                                       // Complexes
            MathS.Sets.R,                                       // Reals
            MathS.Sets.Q,                                       // Rationals
            MathS.Sets.Z,                                       // Integers
            Entity.Set.SpecialSet.Create(Domain.Boolean),       // Booleans
            X,                                                  // Variable
            MathS.pi,                                           // Constant
        };

        public static IEnumerable<object[]> EveryNodeType()
        {
            var built = new List<Entity>();
            foreach (var type in ConcreteNodeTypes)
            {
                var constructor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                    .Where(c => c.GetParameters().Length > 0
                                && c.GetParameters().All(p => p.ParameterType == typeof(Entity)))
                    .OrderBy(c => c.GetParameters().Length)
                    .FirstOrDefault();
                if (constructor is null)
                    continue;
                try
                {
                    built.Add((Entity)constructor.Invoke(
                        constructor.GetParameters().Select(_ => (object)(Entity)X).ToArray()));
                }
                catch (Exception) { /* a node that will not take a bare variable is not the subject */ }
            }
            built.AddRange(HandBuilt);

            var covered = new HashSet<Type>(built.Select(n => n.GetType()));
            var uncovered = ConcreteNodeTypes.Where(t => !covered.Contains(t)).ToList();
            Assert.True(uncovered.Count is 0,
                "no sample is built for these node types, so serialization is not tested against "
                + $"them — add one to {nameof(HandBuilt)}:\n  "
                + string.Join("\n  ", uncovered.Select(t => t.FullName)));

            return built.Select(n => new object[] { n });
        }

        /// <summary>
        /// The round trips that do not hold, keyed by what the node prints as, with the reason.
        /// </summary>
        /// <remarks>
        /// Inherited from the printed form rather than owned here: the converter writes what
        /// <c>Stringize</c> prints, so it round trips exactly where printing does, and this list is
        /// the same one <see cref="EveryNodeSurvivesEveryPipelineTest"/> keeps. Held in both
        /// directions, so an entry that starts round tripping fails too.
        /// </remarks>
        private static readonly Dictionary<string, string> KnownRoundTripFailures = new(StringComparer.Ordinal)
        {
            ["1 + 2i"] = "a Complex prints as a sum, and a sum parses as Sumf(1, 2i)",
        };

        [Theory]
        [MemberData(nameof(EveryNodeType))]
        public void EveryNodeTypeSurvivesJson(Entity node)
        {
            var json = JsonSerializer.Serialize(node);
            var back = JsonSerializer.Deserialize<Entity>(json);
            var survives = back == node;

            if (KnownRoundTripFailures.TryGetValue(node.Stringize(), out var reason))
                Assert.False(survives,
                    $"{node.Stringize()} [{node.GetType().Name}] survives JSON now, so it is no "
                    + $"longer a known failure — drop it from {nameof(KnownRoundTripFailures)}, "
                    + $"where it reads \"{reason}\"");
            else
                Assert.True(survives,
                    $"{node.Stringize()} [{node.GetType().Name}] came back as "
                    + $"{back!.Stringize()} [{back.GetType().Name}] (JSON was {json})");
        }

        /// <summary>
        /// What is written is the printed form and nothing else — one JSON string, not an object
        /// with the tree spelled out. This is the one place the text itself is the subject: it is
        /// the claim that there is no second representation to keep in step with the first.
        /// </summary>
        [Fact]
        public void WhatIsWrittenIsThePrintedForm()
        {
            Entity expression = "(x + 1) ^ 2 / sin(y)";
            Assert.Equal(JsonSerializer.Serialize(expression.Stringize()),
                         JsonSerializer.Serialize(expression));
        }

        /// <summary>
        /// Every public node type carries the converter, not only <see cref="Entity"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="JsonSerializerOptions"/> looks a converter attribute up on the declared type
        /// with <c>inherit: false</c>, so one on <see cref="Entity"/> alone leaves a member
        /// declared as <see cref="Entity.Variable"/> to the reflecting object converter — which
        /// walks <see cref="Entity.Nodes"/>, a node's own enumeration of itself, and reports an
        /// object cycle. So the attribute is on every public node type, and this is what says so
        /// when a new one arrives without it.
        /// </remarks>
        [Fact]
        public void EveryPublicNodeTypeCarriesItsConverter()
        {
            var missing = typeof(Entity).Assembly.GetTypes()
                .Where(t => typeof(Entity).IsAssignableFrom(t) && PubliclyVisible(t))
                .Where(t => t.GetCustomAttribute<JsonConverterAttribute>(inherit: false) is null)
                .OrderBy(t => t.FullName, StringComparer.Ordinal)
                .ToList();

            Assert.True(missing.Count is 0,
                "these node types are not told about the converter, so a member declared as one of "
                + "them fails with an object cycle — add it to Core/Serialization/Entity.Serialization.cs:\n  "
                + string.Join("\n  ", missing.Select(t => t.FullName)));
        }

        private static bool PubliclyVisible(Type type)
        {
            while (type.IsNested)
            {
                if (!type.IsNestedPublic) return false;
                type = type.DeclaringType!;
            }
            return type.IsPublic;
        }

        private sealed class Problem
        {
            public string? Title { get; set; }
            public Entity? Body { get; set; }
            public Entity.Variable? Unknown { get; set; }
            public Entity.Matrix? Coefficients { get; set; }
            public List<Entity>? Steps { get; set; }
            public Dictionary<Entity, Entity>? Substitutions { get; set; }
        }

        [Fact]
        public void AnEntityIsAMemberOfASerializableType()
        {
            var problem = new Problem
            {
                Title = "quadratic",
                Body = "x ^ 2 - 3 * x + 2",
                Unknown = MathS.Var("x"),
                Coefficients = (Entity.Matrix)MathS.Vector(1, -3, 2),
                Steps = new List<Entity> { "(x - 1) * (x - 2)", MathS.pi, MathS.i },
                Substitutions = new Dictionary<Entity, Entity> { ["x"] = "y + 1" },
            };

            var back = JsonSerializer.Deserialize<Problem>(JsonSerializer.Serialize(problem))!;

            Assert.Equal("quadratic", back.Title);
            Assert.Equal(problem.Body, back.Body);
            Assert.Equal(problem.Unknown, back.Unknown);
            Assert.Equal(problem.Coefficients, back.Coefficients);
            Assert.Equal(problem.Steps, back.Steps);
            Assert.Equal(problem.Substitutions, back.Substitutions);
        }

        [Fact]
        public void AnAbsentExpressionIsNull()
        {
            var back = JsonSerializer.Deserialize<Problem>(JsonSerializer.Serialize(new Problem()))!;
            Assert.Null(back.Body);
            Assert.Null(back.Unknown);
        }

        [Theory]
        [InlineData("123")]
        [InlineData("true")]
        [InlineData("[1]")]
        [InlineData("{\"a\": 1}")]
        public void SomethingThatIsNotAStringIsRejected(string json)
            => Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Entity>(json));

        /// <summary>
        /// A string that is not an expression fails exactly as <see cref="MathS.FromString(string)"/>
        /// fails, message included. That is the same choice as everything else here: the contract is
        /// inherited from parsing rather than restated, and the parser's message names the position
        /// and what it expected.
        /// </summary>
        [Fact]
        public void SomethingThatIsNotAnExpressionFailsTheWayParsingFails()
            => Assert.Throws<UnhandledParseException>(() => JsonSerializer.Deserialize<Entity>("\"x +\""));

        [Fact]
        public void AnExpressionThatIsNotTheDeclaredNodeTypeIsRejected()
        {
            var thrown = Assert.Throws<JsonException>(
                () => JsonSerializer.Deserialize<Entity.Variable>("\"x + 1\""));
            Assert.Contains("Variable", thrown.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void TheConverterCanBeRegisteredExplicitly()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new EntityJsonConverter());
            Entity expression = "a / b";
            Assert.Equal(expression,
                JsonSerializer.Deserialize<Entity>(JsonSerializer.Serialize(expression, options), options));
        }

        /// <summary>
        /// The one thing the printed form does not carry, pinned so that it is a statement about
        /// where the two part company rather than a hole nobody measured. Written to fail when
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1022">#1022</a> is fixed,
        /// since the converter needs no change for it and this test would then be describing
        /// something that had stopped being true.
        /// </summary>
        [Fact]
        public void ACodomainDoesNotSurviveBecauseNothingPrintsIt()
        {
            Entity narrowed = MathS.FromString("domain(x, ZZ)");
            Assert.Equal(Domain.Integer, narrowed.Codomain);
            Assert.Equal("x", narrowed.Stringize());

            var back = JsonSerializer.Deserialize<Entity>(JsonSerializer.Serialize(narrowed))!;
            Assert.NotEqual(narrowed, back);
            Assert.Equal(Domain.Any, back.Codomain);
        }

        /// <summary>
        /// A binder's parameter is a name, and serializing does not rename it. The concern is
        /// specific: <see cref="Entity.DirectChildren"/> publishes a bound body with the parameter
        /// renamed, so anything that reads an expression through traversal can hand back an
        /// alpha-equivalent expression rather than the one it was given. Printing does not go
        /// through it, and this is what says so.
        /// </summary>
        [Theory]
        [InlineData("lambda(x, x + y)")]
        [InlineData("lambda(x, lambda(y, x + y))")]
        [InlineData("apply(lambda(x, x + 1), 5)")]
        [InlineData("{ x : x > 0 }")]
        [InlineData("sum(x ^ 2, x, 1, 10)")]
        [InlineData("product(x, x, 1, 10)")]
        [InlineData("integral(sin(x), x, 0, pi)")]
        [InlineData("derivative(x ^ 3, x, 2)")]
        [InlineData("limit(sin(x) / x, x, 0)")]
        [InlineData("limitleft(1 / x, x, 0)")]
        [InlineData("limitright(1 / x, x, 0)")]
        [InlineData("piecewise(x provided x > 0, 0 provided x <= 0)")]
        [InlineData("(x + 1) provided (x > 0)")]
        public void ABoundNameIsNotRenamed(string source)
        {
            Entity expression = MathS.FromString(source);
            var back = JsonSerializer.Deserialize<Entity>(JsonSerializer.Serialize(expression))!;
            Assert.Equal(expression, back);
            Assert.Equal(expression.Vars, back.Vars);
        }
    }
}
