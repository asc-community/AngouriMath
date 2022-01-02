//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;

namespace AngouriMath
{
    partial record Entity
    {
        // Variable, along with Set and Tensor is a unique element, that might be either Continuous or Discrete
        // and/or contain either Continuous or Discrete elements

        #region Variable
        /// <summary>
        /// Variable node. It only has a name.
        /// Construct a <see cref="Variable"/> with an implicit conversion from <see cref="string"/>.
        /// It has no type, so you can substitute any value under a given variable.
        /// </summary>
        public sealed partial record Variable : Entity
        {
            /// <summary>
            /// Deconstructs Variable as follows
            /// </summary>
            /// <param name="name">To where the result is put</param>
            public void Deconstruct(out string name)
                => name = Name;
            internal static Variable CreateVariableUnchecked(string name) => new(name);
            private Variable(string name) => Name = name;

            /// <summary>
            /// The name of the variable as a string
            /// </summary>
            public string Name { get; }
            internal override Priority Priority => Priority.Leaf;

            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(this);
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => Array.Empty<Entity>();

            [ConstantField] internal static readonly Variable pi = new Variable(nameof(pi));
            [ConstantField] internal static readonly Variable e = new Variable(nameof(e));
            [ConstantField] internal static readonly IReadOnlyDictionary<Variable, Number.Complex> ConstantList =
                new Dictionary<Variable, Number.Complex>
                {
                    { pi, MathS.DecimalConst.pi },
                    { e, MathS.DecimalConst.e }
                };

            /// <summary>
            /// Extracts this <see cref="Variable"/>'s name and index
            /// from its <see cref="Name"/> (e. g. "qua" or "phi_3" or "qu_q")
            /// </summary>
            /// <returns>
            /// If this contains _ and valid name and index, returns a pair of
            /// (<see cref="string"/> Prefix, <see cref="string"/> Index),
            /// <see langword="null"/> otherwise
            /// </returns>
            internal (string Prefix, string Index)? SplitIndex() =>
                Name.IndexOf('_') is var pos_ && pos_ == -1
                ? null
                : ((string Prefix, string Index)?)(Name.Substring(0, pos_), Name.Substring(pos_ + 1));
            /// <summary>
            /// Finds next var index name that is unused in <paramref name="expr"/> starting with 1, e. g.
            /// x + n_0 + n_a + n_3 + n_1
            /// will find n_2
            /// </summary>
            /// <remarks>
            /// This is intended for variables visible to the user.
            /// For non-visible variables, use <see cref="CreateTemp"/> instead.
            /// </remarks>
            internal static Variable CreateUnique(Entity expr, string prefix)
            {
                var indices = new HashSet<int>();
                foreach (var var in expr.Vars)
                    if (var.SplitIndex() is var (varPrefix, index)
                        && varPrefix == prefix
                        && int.TryParse(index, out var num))
                        indices.Add(num);
                var i = 1;
                while (indices.Contains(i))
                    i++;
                return new Variable(prefix + "_" + i);
            }

            [ConstantField]
            private static readonly Variable[] letterVars = 
                "xyzabcdefghijklmnopqrstuvw"
                .Select(c => new Variable(c.ToString()))
                .Where(c => c.IsConstant is false)
                .ToArray();

            /// <summary>
            /// First, tries to find a good single-character variable
            /// in the alphabet list. Then, if all used, returns
            /// a unique with incrementable prefix
            /// </summary>
            internal static Variable CreateUniqueAlphabetFirst(Entity expr, string prefix = "a")
            {
                // TODO: Vars to be a set
                foreach (var v in letterVars)
                    if (expr.Vars.Contains(v) is false)
                        return v;
                return CreateUnique(expr, prefix);
            }

            /// <summary>Creates a temporary variable like %1, %2 and %3 that is not in <paramref name="existingVars"/></summary>
            /// <remarks>
            /// This is intended for variables not visible to the user.
            /// For visible variables, use <see cref="CreateUnique"/> instead.
            /// </remarks>
            internal static Variable CreateTemp(IEnumerable<Variable> existingVars)
            {
                var indices = new HashSet<int>();
                foreach (var var in existingVars)
                    if (var.Name.StartsWith("%") && int.TryParse(var.Name.Substring(1), out var num))
                        indices.Add(num);
                var i = 1;
                while (indices.Contains(i))
                    i++;
                return new Variable("%" + i);
            }

            /// <summary>
            /// Used for cases, when the variable should be valid, yet should have no intersection with those existing in the expression
            /// </summary>
            internal static Variable CreateRandom(Entity expr)
            {
                static char IntToChar(uint c)
                    => (char)(c % (122 - 97 + 1) + 97);

                var hs = (uint)(expr.GetHashCode());
                var s = new string(new[]{ IntToChar(hs >> 24), IntToChar(hs << 8 >> 24), IntToChar(hs << 16 >> 24), IntToChar(hs << 24 >> 24)});
                return MathS.Var(s);
            }
        }
        #endregion
    }
}
