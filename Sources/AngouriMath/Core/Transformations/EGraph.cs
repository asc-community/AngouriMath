//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Linq;
using AngouriMath.Core.Transformations.Matching;
using AngouriMath.Extensions;

namespace AngouriMath.Core.Transformations
{
    /// <summary>
    /// One e-node: an operator, and the e-classes of its children.
    /// </summary>
    internal readonly struct ENode : IEquatable<ENode>, IComparable<ENode>
    {
        internal ENode(string op, int[] children, Domain? codomain = null)
        {
            Op = op ?? throw new ArgumentNullException(nameof(op));
            Children = children ?? throw new ArgumentNullException(nameof(children));
            Codomain = codomain;
        }

        internal string Op { get; }
        internal int[] Children { get; }

        /// <summary>
        /// The <see cref="Entity.Codomain"/> the node carries where it differs from its type's
        /// default, and <see langword="null"/> where it does not. <b>Part of the node's identity,
        /// deliberately.</b> An e-class is the graph's assertion that its members are equal, and
        /// <c>abs(x)</c> is not equal to <c>domain(abs(x), Any)</c> — they evaluate differently —
        /// so two nodes differing only here must hash apart, or the graph unions values that are
        /// not the same value. This used to live in a side table keyed on shape alone; both then
        /// landed in one class and extraction returned whichever was inserted last, and
        /// <c>Rebuild</c> dropped it altogether. <c>ENodeIdentityTest</c> holds all three.
        /// </summary>
        internal Domain? Codomain { get; }

        public bool Equals(ENode other)
        {
            if (Op != other.Op || Codomain != other.Codomain || Children.Length != other.Children.Length)
                return false;
            for (var i = 0; i < Children.Length; i++)
                if (Children[i] != other.Children[i]) return false;
            return true;
        }

        public override bool Equals(object? obj) => obj is ENode node && Equals(node);

        /// <summary>
        /// A total order on e-nodes, so that the members of one e-class can be visited in a
        /// defined sequence. <see cref="HashSet{T}"/> enumeration order is an unspecified
        /// implementation detail and a string's hash code is randomised per process, so a cost
        /// tie between two members would otherwise be settled differently from one run to the
        /// next -- against the premise that a bounded computation is reproducible given a defined
        /// algorithm order. Ordinal on <see cref="Op"/>, so it does not move with the culture
        /// either.
        /// </summary>
        public int CompareTo(ENode other)
        {
            var byOp = string.CompareOrdinal(Op, other.Op);
            if (byOp != 0) return byOp;
            var byCodomain = Nullable.Compare(Codomain, other.Codomain);
            if (byCodomain != 0) return byCodomain;
            if (Children.Length != other.Children.Length)
                return Children.Length.CompareTo(other.Children.Length);
            for (var i = 0; i < Children.Length; i++)
                if (Children[i] != other.Children[i])
                    return Children[i].CompareTo(other.Children[i]);
            return 0;
        }

        public override int GetHashCode()
        {
            var hash = Op.GetHashCode();
            hash = unchecked(hash * 31 + (Codomain is { } domain ? (int)domain + 1 : 0));
            foreach (var child in Children) hash = unchecked(hash * 31 + child);
            return hash;
        }
    }

    /// <summary>
    /// An e-graph: e-classes over a union-find, e-nodes keyed by operator, child class and
    /// non-default codomain, hash-consed. The codomain is in the key because an e-class is an
    /// equality claim and a codomain is something two unequal entities can differ in.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> tier 2's e-graph,
    /// moved from the <c>work/egraph</c> measurement harness into the library once the harness had
    /// answered what it was built to answer — see that harness's own report for the measurement
    /// this design rests on.
    /// </remarks>
    internal sealed class EGraph
    {
        /// <summary>
        /// How deep <see cref="Extract(int, Func{Entity, double})"/> will chain through child
        /// classes before declining to build. A crash guard rather than a quality knob: textbook
        /// input nests nowhere near this far, so the cap is only reached where the alternative is
        /// an uncatchable stack overflow.
        /// </summary>
        private const int MaxExtractionDepth = 256;

        private readonly List<int> parent = new();
        private readonly Dictionary<ENode, int> hashcons = new();
        private readonly Dictionary<int, HashSet<ENode>> classes = new();

        /// <summary>How many e-classes this graph currently has.</summary>
        internal int ClassCount => classes.Count;

        /// <summary>How many distinct e-nodes this graph has ever created.</summary>
        internal int NodeCount => hashcons.Count;

        /// <summary>The representative of the e-class <paramref name="id"/> currently belongs to.</summary>
        internal int Find(int id)
        {
            while (parent[id] != id) { parent[id] = parent[parent[id]]; id = parent[id]; }
            return id;
        }

        private int NewClass()
        {
            var id = parent.Count;
            parent.Add(id);
            classes[id] = new HashSet<ENode>();
            return id;
        }

        /// <summary>
        /// Adds a leaf or an operator over already-added children, and returns the e-class it
        /// belongs to -- an existing one if an equal e-node is already hash-consed, otherwise a
        /// fresh one.
        /// </summary>
        /// <param name="op">
        /// The operator identity: a leaf's printed self, so that two different variables or
        /// numbers never share a class, or a node type's name otherwise.
        /// </param>
        /// <param name="children">The e-classes of this e-node's children, already added.</param>
        internal int Add(string op, params int[] children) => Add(op, children, null);

        private int Add(string op, int[] children, Domain? codomain)
        {
            var canonical = new ENode(op, children.Select(Find).ToArray(), codomain);
            if (hashcons.TryGetValue(canonical, out var existing))
                return Find(existing);
            // Folding on insertion: a neutral-element application denotes exactly the value its
            // other operand already has, so it is unioned into that operand's class instead of
            // being given a fresh e-node. Proven against 16 corpus expressions in the #746 tier 2
            // measurement harness before moving here: it closed eight of the nine blow-ups that
            // harness found in equality saturation over the full, undirected rule set. Only
            // catches the shape at the moment it is added -- see Rebuild for what a union
            // afterwards does not reach back and fold.
            if (NeutralClass(canonical) is { } folded)
                return folded;
            var id = NewClass();
            hashcons[canonical] = id;
            classes[id].Add(canonical);
            return id;
        }

        private bool Holds(int id, string leaf)
            => classes.TryGetValue(Find(id), out var set)
               && set.Any(n => n.Children.Length == 0 && n.Op == leaf);

        /// <summary>
        /// The leaves worth trying as an identity. The additive and multiplicative identities,
        /// which is what the arithmetic operators have; a fold that needed some other constant
        /// would be a fact about that operator rather than about neutrality.
        /// </summary>
        [ConstantField]
        private static readonly string[] NeutralLeaves = { "0", "1" };

        /// <summary>
        /// Which operator folds away which leaf on which side, asked of
        /// <see cref="Entity.InnerSimplified"/> rather than written out a second time.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This used to be a hand-written table over <c>Sumf</c>, <c>Minusf</c>, <c>Mulf</c>,
        /// <c>Divf</c> and <c>Powf</c>, restating identities each of those types already
        /// implements and tests in its own <c>InnerSimplify</c>. Nothing kept the two in step, and
        /// the divergence would have been silent in the worst direction: the e-graph would go on
        /// asserting an equivalence the rest of the library had stopped believing, merging two
        /// classes that are no longer equal.
        /// </para>
        /// <para>
        /// Asking instead of restating also settles the cases that make a hand-written table
        /// fragile, without anybody having to remember them. <c>0 - x</c> is a negation and
        /// <c>1 / x</c> a reciprocal, so neither folds to its other operand; <c>1 ^ x</c> is the
        /// constant 1 rather than <c>x</c>. And if an arm ever answers with a condition attached
        /// — <c>Powf</c>'s <c>1 ^ x</c> already carries a <c>Providedf</c> domain condition — the
        /// answer is not the bare operand, so no fold is claimed for it. A hand-written table has
        /// no way to learn any of that.
        /// </para>
        /// </remarks>
        [ConstantField]
        private static readonly HashSet<(string Op, string Leaf, int LeafSide)> neutralFolds
            = BuildNeutralFolds();

        private static HashSet<(string Op, string Leaf, int LeafSide)> BuildNeutralFolds()
        {
            var folds = new HashSet<(string, string, int)>();
            Entity operand = MathS.Var("x");
            foreach (var type in MatchPattern.BuildableNodeTypes)
                foreach (var leafText in NeutralLeaves)
                    for (var side = 0; side < 2; side++)
                    {
                        var leaf = leafText.ToEntity();
                        var children = side == 0
                            ? new[] { leaf, operand }
                            : new[] { operand, leaf };
                        // A node this cannot build, or an arm that throws on a shape it did not
                        // expect, simply contributes no fold -- the table is what was observed.
                        try
                        {
                            if (MatchPattern.ConstructNode(type, children) is not { } built) continue;
                            if (built.InnerSimplified.Equals(operand))
                                folds.Add((type.Name, leafText, side));
                        }
                        catch { /* not a fold, and not this table's business why */ }
                    }
            return folds;
        }

        /// <summary>
        /// The folds this graph performs on insertion, as <c>Op(first, second)</c> — for the test
        /// that names them, so that a change in <c>InnerSimplify</c> shows up as a changed list
        /// rather than as e-graph folding quietly gaining or losing a case.
        /// </summary>
        internal static IEnumerable<string> NeutralFolds
            => neutralFolds.Select(fold => fold.LeafSide == 0
                ? $"{fold.Op}({fold.Leaf}, x)"
                : $"{fold.Op}(x, {fold.Leaf})");

        /// <summary>
        /// If <paramref name="node"/> is a neutral element applied to something, the class that
        /// already denotes its value -- <see langword="null"/> otherwise.
        /// </summary>
        private int? NeutralClass(ENode node)
        {
            if (node.Children.Length != 2) return null;
            // A node carrying a codomain of its own is not the value of its other operand: folding
            // `domain(x * 1, Any)` into x's class would union it with a plain `x`, which is the
            // same conflation the identity above exists to prevent.
            if (node.Codomain is not null) return null;
            foreach (var leaf in NeutralLeaves)
            {
                if (neutralFolds.Contains((node.Op, leaf, 1)) && Holds(node.Children[1], leaf))
                    return Find(node.Children[0]);
                if (neutralFolds.Contains((node.Op, leaf, 0)) && Holds(node.Children[0], leaf))
                    return Find(node.Children[1]);
            }
            return null;
        }

        /// <summary>Every e-class currently in the graph.</summary>
        internal IEnumerable<int> Classes => classes.Keys.ToList();

        /// <summary>The e-nodes belonging to the e-class <paramref name="id"/> is a member of.</summary>
        internal IReadOnlyCollection<ENode> NodesOf(int id) => classes[Find(id)];

        /// <summary>
        /// Merges the e-classes of <paramref name="left"/> and <paramref name="right"/>, and
        /// answers whether they were different classes before this call. Does not itself restore
        /// congruence between e-nodes that now share a child class -- see <see cref="Rebuild"/>.
        /// </summary>
        internal bool Union(int left, int right)
        {
            left = Find(left);
            right = Find(right);
            if (left == right) return false;
            parent[right] = left;
            foreach (var node in classes[right]) classes[left].Add(node);
            classes.Remove(right);
            return true;
        }

        /// <summary>
        /// Restores congruence: after a union, e-nodes whose children are now in the same class
        /// as each other are congruent and are merged too, repeated until nothing more merges.
        /// This is the step that makes an e-graph an e-graph rather than a set of terms related
        /// only by the unions asked for directly.
        /// </summary>
        internal void Rebuild()
        {
            bool changed;
            do
            {
                changed = false;
                var seen = new Dictionary<ENode, int>();
                foreach (var pair in classes.ToList())
                    foreach (var node in pair.Value.ToList())
                    {
                        var canonical = new ENode(node.Op, node.Children.Select(Find).ToArray(), node.Codomain);
                        if (seen.TryGetValue(canonical, out var other))
                        {
                            if (Union(other, pair.Key)) changed = true;
                        }
                        else seen[canonical] = pair.Key;
                    }
            } while (changed);
        }

        /// <summary>
        /// A key naming <see cref="Entity.Constant.EulerIntrinsic"/> specifically, distinct from
        /// any string a real leaf can print as (a leaf's printed form never starts with a NUL) --
        /// so it shares nothing with the ordinary named constant <c>e</c>, which prints the same
        /// text <see cref="Entity.Constant.EulerIntrinsic"/> does. Both denote the same number and
        /// both are kept out of this key's collision space on purpose:
        /// <see cref="Entity.Constant.EulerIntrinsic"/> is a distinguished reference a binder over
        /// the name <c>e</c> must not capture, and merging the two into one e-class -- which
        /// <see cref="Key"/> keying on <c>Entity.Stringize()</c> alone would do -- silently
        /// discards that distinction on the next extraction. Caught in code review before this PR
        /// was merged.
        /// </summary>
        private const string EulerIntrinsicKey = "\0EulerIntrinsic";

        /// <summary>
        /// The operator identity of a node: a leaf's printed self, so that two different
        /// variables or numbers never share a class, or the node's runtime type otherwise, so
        /// that <c>x + y</c> and <c>a + b</c> are the same operator over different children --
        /// except <see cref="Entity.Constant.EulerIntrinsic"/>, which prints as the ordinary named
        /// constant <c>e</c> does and is kept out of its class regardless.
        /// </summary>
        private static string Key(Entity expr)
            => ReferenceEquals(expr, Entity.Constant.EulerIntrinsic) ? EulerIntrinsicKey
             : expr.DirectChildren.Count == 0 ? expr.Stringize() : expr.GetType().Name;

        /// <summary>Adds <paramref name="expr"/> and every one of its subexpressions, bottom-up.</summary>
        internal int AddEntity(Entity expr)
        {
            var children = expr.DirectChildren.Select(AddEntity).ToArray();
            var codomain = expr.Codomain == expr.DefaultCodomain ? (Domain?)null : expr.Codomain;
            return Add(Key(expr), children, codomain);
        }

        /// <summary>
        /// The cheapest entity the e-class <paramref name="id"/> can be built as under
        /// <paramref name="cost"/>, or <see langword="null"/> where nothing in it can be built --
        /// every member refers, directly or through a cycle only unions can create, to a node
        /// type <see cref="MatchPattern.ConstructNode"/> does not build.
        /// </summary>
        internal Entity? Extract(int id, Func<Entity, double> cost)
            => Extract(id, new Cheapest(cost), new Dictionary<int, Entity>(), new HashSet<int>());

        /// <summary>
        /// The least entity the e-class <paramref name="id"/> can be built as under
        /// <paramref name="order"/>, or <see langword="null"/> where nothing in it can be built.
        /// </summary>
        /// <remarks>
        /// The same walk as <see cref="Extract(int, Func{Entity, double})"/>, choosing by an order
        /// rather than by a number. That is not a stylistic difference: a cost model ties, and a
        /// tie is settled by whichever member was reached first, so "the cheapest member" is not a
        /// well-defined expression where "the least member" is. See <see cref="EntityOrder"/>.
        /// </remarks>
        internal Entity? ExtractLeast(int id, IComparer<Entity> order)
            => Extract(id, new Least(order), new Dictionary<int, Entity>(), new HashSet<int>());

        /// <summary>
        /// How <c>Extract</c> chooses between the members of one e-class. A
        /// <see langword="struct"/> under a generic constraint rather than an interface reference,
        /// so the choice is compiled in rather than dispatched, and no selection is allocated for
        /// the classes an extraction walks.
        /// </summary>
        private interface ISelection
        {
            /// <summary>Forgets the previous class's incumbent.</summary>
            void Begin();

            /// <summary>Offers a candidate; the incumbent afterwards is the better of the two.</summary>
            void Offer(Entity candidate);

            /// <summary>The incumbent, or <see langword="null"/> where nothing was admissible.</summary>
            Entity? Best { get; }
        }

        private struct Cheapest : ISelection
        {
            private readonly Func<Entity, double> cost;
            private Entity? best;
            private double bestCost;

            internal Cheapest(Func<Entity, double> cost)
                => (this.cost, best, bestCost) = (cost, null, double.MaxValue);

            public void Begin() => (best, bestCost) = (null, double.MaxValue);

            public void Offer(Entity candidate)
            {
                double here;
                try { here = cost(candidate); } catch { return; }
                // A model that answers NaN has not ranked this candidate, which is what a model
                // that throws is already saying, so both decline it the same way. Without this,
                // the comparison below is false for NaN on either side (IEEE-754), so NaN becomes
                // the incumbent cheapest and every later candidate then beats it unconditionally
                // -- the answer stops being the cheapest and becomes whichever member came last.
                if (double.IsNaN(here)) return;
                if (here >= bestCost) return;
                (best, bestCost) = (candidate, here);
            }

            public readonly Entity? Best => best;
        }

        private struct Least : ISelection
        {
            private readonly IComparer<Entity> order;
            private Entity? best;

            internal Least(IComparer<Entity> order) => (this.order, best) = (order, null);

            public void Begin() => best = null;

            // No admissibility test to make: an order ranks every pair, where a cost model can
            // decline one. Nothing here can throw that Compare does not.
            public void Offer(Entity candidate)
            {
                if (best is null || order.Compare(candidate, best) < 0) best = candidate;
            }

            public readonly Entity? Best => best;
        }

        private Entity? Extract<TSelection>(int id, TSelection seed,
            Dictionary<int, Entity> memo, HashSet<int> visiting)
            where TSelection : struct, ISelection
        {
            id = Find(id);
            if (memo.TryGetValue(id, out var done)) return done;
            // visiting is the chain currently being expanded, so its size is this call's depth.
            // The cycle guard below bounds that chain only by the number of distinct classes,
            // which unions grow past the input expression's own syntactic depth -- and a
            // StackOverflowException cannot be caught, so exhausting the stack takes the process
            // down rather than failing one call. Declining to build is the answer the cycle case
            // already gives, and the same shape as Gruntz's own MaxDepth.
            if (visiting.Count >= MaxExtractionDepth) return null;
            if (!visiting.Add(id)) return null;              // a cycle; the other node will do
            var selection = seed;
            selection.Begin();
            // Ordered, not as the set enumerates: where the selection ties, the answer is
            // whichever candidate was reached first, and set order is neither specified nor
            // stable across processes.
            foreach (var node in NodesOf(id).OrderBy(node => node))
            {
                var parts = new Entity[node.Children.Length];
                var ok = true;
                for (var i = 0; i < parts.Length && ok; i++)
                {
                    var part = Extract(node.Children[i], seed, memo, visiting);
                    if (part is null) ok = false;
                    else parts[i] = part;
                }
                if (!ok) continue;
                Entity? built = parts.Length == 0
                    ? TryParseLeaf(node.Op)
                    : MatchPattern.ConstructNode(OperatorType(node.Op), parts);
                if (built is null) continue;
                if (node.Codomain is { } domain) built = built.WithCodomain(domain);
                selection.Offer(built);
            }
            visiting.Remove(id);
            var best = selection.Best;
            if (best is not null) memo[id] = best;
            return best;
        }

        private static Entity? TryParseLeaf(string printed)
        {
            if (printed == EulerIntrinsicKey) return Entity.Constant.EulerIntrinsic;
            try { return printed.ToEntity(); } catch { return null; }
        }

        /// <summary>
        /// <see cref="MatchPattern.ConstructNode"/> takes the runtime <see cref="Type"/> a
        /// <see cref="Key"/> string names, and <see cref="MatchPattern.BuildableNodeTypes"/> is
        /// where those types are named -- once, beside the <c>Construct</c> that builds them.
        /// This used to hold a second copy of that list, which nothing kept in step: a type added
        /// to one and not the other silently stopped being reachable from here, with no compiler
        /// error to say so.
        /// </summary>
        private static Type OperatorType(string op)
            => MatchPattern.NodeTypeNamed(op) ?? typeof(void);

        /// <summary>
        /// Whether the e-class <paramref name="id"/> already contains a leaf equal to
        /// <paramref name="leaf"/> -- the same check <see cref="NeutralClass"/> uses for a
        /// neutral element, offered for <see cref="Matching.MatchPattern"/>'s
        /// <c>ExactPattern.EMatch</c> to use for a literal.
        /// </summary>
        internal bool ContainsLeaf(int id, Entity leaf) => Holds(id, Key(leaf));

        /// <summary>
        /// The runtime <see cref="Type"/> an e-node builds as: a leaf's, by re-parsing its
        /// printed form, or a non-leaf's operator type, by the same lookup <c>Extract</c>
        /// uses to reconstruct one. <c>typeof(void)</c> where neither succeeds.
        /// </summary>
        internal static Type RuntimeType(ENode node)
            => node.Children.Length == 0
                ? TryParseLeaf(node.Op)?.GetType() ?? typeof(void)
                : OperatorType(node.Op);
    }
}
