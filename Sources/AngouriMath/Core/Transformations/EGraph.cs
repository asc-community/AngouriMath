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
        internal ENode(string op, int[] children)
        {
            Op = op ?? throw new ArgumentNullException(nameof(op));
            Children = children ?? throw new ArgumentNullException(nameof(children));
        }

        internal string Op { get; }
        internal int[] Children { get; }

        public bool Equals(ENode other)
        {
            if (Op != other.Op || Children.Length != other.Children.Length) return false;
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
            foreach (var child in Children) hash = unchecked(hash * 31 + child);
            return hash;
        }
    }

    /// <summary>
    /// An e-graph: e-classes over a union-find, e-nodes keyed by operator and child class,
    /// hash-consed.
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

        /// <summary>
        /// The <see cref="Entity.Codomain"/> the source <see cref="Entity"/> carried at the exact
        /// e-node a real entity was inserted as, where it differs from that node's own default --
        /// <c>Add(string, int[])</c> has no <see cref="Entity"/> to read it from, only
        /// <see cref="AddEntity"/> does. <c>Extract(int, Func{Entity, double})</c> re-applies it
        /// after rebuilding through <see cref="MatchPattern.ConstructNode"/>, which builds through
        /// a bare constructor and so restores nothing on its own -- unlike every other place this
        /// codebase reconstructs a node, which copies it forward through a <c>New(...)</c> helper.
        /// Caught in code review before this PR was merged.
        /// </summary>
        private readonly Dictionary<ENode, Domain> codomains = new();

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
            var canonical = new ENode(op, children.Select(Find).ToArray());
            if (codomain is { } existingDomain) codomains[canonical] = existingDomain;
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
        /// If <paramref name="node"/> is a neutral element applied to something, the class that
        /// already denotes its value -- <see langword="null"/> otherwise.
        /// </summary>
        /// <remarks>
        /// <c>Sumf</c> and <c>Mulf</c> are commutative, so the identity folds from either side:
        /// <c>x + 0</c> and <c>0 + x</c> both denote <c>x</c>. <c>Minusf</c> and <c>Divf</c> are
        /// not: <c>x - 0</c> and <c>x / 1</c> denote <c>x</c>, but <c>0 - x</c> and <c>1 / x</c>
        /// do not -- they negate or invert it, a different value from either operand, and not
        /// something this method may fold away. <c>1 ^ x</c> is likewise not <c>x</c> -- it is
        /// the constant 1 -- so <c>Powf</c> only checks the exponent.
        /// </remarks>
        private int? NeutralClass(ENode node)
        {
            if (node.Children.Length != 2) return null;
            return node.Op switch
            {
                "Sumf" => Holds(node.Children[1], "0") ? Find(node.Children[0])
                    : Holds(node.Children[0], "0") ? Find(node.Children[1])
                    : (int?)null,
                "Minusf" => Holds(node.Children[1], "0") ? Find(node.Children[0]) : (int?)null,
                "Mulf" => Holds(node.Children[1], "1") ? Find(node.Children[0])
                    : Holds(node.Children[0], "1") ? Find(node.Children[1])
                    : (int?)null,
                "Divf" => Holds(node.Children[1], "1") ? Find(node.Children[0]) : (int?)null,
                "Powf" => Holds(node.Children[1], "1") ? Find(node.Children[0]) : (int?)null,
                _ => null
            };
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
                        var canonical = new ENode(node.Op, node.Children.Select(Find).ToArray());
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
            => Extract(id, cost, new Dictionary<int, Entity>(), new HashSet<int>());

        private Entity? Extract(int id, Func<Entity, double> cost,
            Dictionary<int, Entity> memo, HashSet<int> visiting)
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
            Entity? best = null;
            var bestCost = double.MaxValue;
            // Ordered, not as the set enumerates: a tie on cost is settled by whichever candidate
            // is reached first, and set order is neither specified nor stable across processes.
            foreach (var node in NodesOf(id).OrderBy(node => node))
            {
                var parts = new Entity[node.Children.Length];
                var ok = true;
                for (var i = 0; i < parts.Length && ok; i++)
                {
                    var part = Extract(node.Children[i], cost, memo, visiting);
                    if (part is null) ok = false;
                    else parts[i] = part;
                }
                if (!ok) continue;
                Entity? built = parts.Length == 0
                    ? TryParseLeaf(node.Op)
                    : MatchPattern.ConstructNode(OperatorType(node.Op), parts);
                if (built is null) continue;
                if (codomains.TryGetValue(node, out var domain)) built = built.WithCodomain(domain);
                double here;
                try { here = cost(built); } catch { continue; }
                // A model that answers NaN has not ranked this candidate, which is what a model
                // that throws is already saying, so both decline it the same way. Without this,
                // the comparison below is false for NaN on either side (IEEE-754), so NaN becomes
                // the incumbent cheapest and every later candidate then beats it unconditionally
                // -- the answer stops being the cheapest and becomes whichever member came last.
                if (double.IsNaN(here)) continue;
                if (here >= bestCost) continue;
                best = built;
                bestCost = here;
            }
            visiting.Remove(id);
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
