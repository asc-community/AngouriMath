//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core;
using AngouriMath.Core.Transformations;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// The e-graph builds a leaf back as the entity it was given, not by parsing its printed
    /// form: a variable the library names itself, <c>u_sub_1</c> or <c>u_exp_1</c>, prints as a
    /// name the parser does not read, and every class holding one could not be built at all. The
    /// parse threw, and a catch-all took that for a leaf that does not build -- 589 times over the
    /// unit tests and 1518 over a sample of Rubi's problems, nearly all for the integrator's
    /// variables.
    /// https://github.com/asc-community/AngouriMath/issues/1546
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class EGraphLeafTest
    {
        [Fact]
        public void AVariableTheLibraryNamedIsBuiltBack()
        {
            var u = Entity.Variable.CreateUnique("x".ToEntity(), "u_sub");
            var graph = new EGraph();
            var id = graph.AddEntity(u + 1);
            Assert.Equal(u + 1, graph.Extract(id, CostModel.Default.Cost));
        }

        [Fact]
        public void ItsTypeIsReadWithoutParsing()
        {
            var u = Entity.Variable.CreateUnique("x".ToEntity(), "u_exp");
            var graph = new EGraph();
            var id = graph.AddEntity(u);
            Assert.All(graph.NodesOf(id), node => Assert.Equal(typeof(Entity.Variable), graph.RuntimeType(node)));
        }
    }
}
