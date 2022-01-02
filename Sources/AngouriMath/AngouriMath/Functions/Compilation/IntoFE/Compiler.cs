//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using static AngouriMath.Entity;
using static AngouriMath.Core.FastExpression;

namespace AngouriMath
{
    partial record Entity
    {
        private protected virtual void CompileNode(Compiler compiler)
            => throw new UncompilableNodeException($"The node of type {GetType()} does not support compilation. Feel free to report it as an issue on our official repository.");
        /// <summary>
        /// Recursive compilation that pushes intructions to the stack (<see cref="Compiler.Instructions"/>)
        /// </summary>
        internal void InnerCompile(Compiler compiler)
        {
            if (compiler.Cache.TryGetValue(this, out var cacheLine) && cacheLine >= 0)
                compiler.Instructions.Add(new(InstructionType.LOAD_CACHE, cacheLine));
            else
            {
                CompileNode(compiler);
                if (cacheLine < 0) // If cache doesn't store this entity, cacheLine will be uninitialized = 0
                {
                    cacheLine = ~cacheLine;
                    compiler.Cache[this] = cacheLine;
                    compiler.Instructions.Add(new(InstructionType.SAVE_CACHE, cacheLine));
                }
            }
        }

        public partial record Number
        {
            private protected override void CompileNode(Compiler compiler) =>
                compiler.Instructions.Add(new(InstructionType.PUSH_CONST, Value: ((Complex)this).ToNumerics()));
        }

        public partial record Variable
        {
            private protected override void CompileNode(Compiler compiler) =>
                compiler.Instructions.Add(new(InstructionType.PUSH_VAR, compiler.VarNamespace[this]));

            /// <inheritdoc/>
            public override int GetHashCode()
                => Name.GetHashCode();
        }

        partial record Matrix
        {
            private protected override void CompileNode(Compiler compiler) =>
                throw new UncompilableNodeException($"Tensors cannot be compiled");
        }

        // Each function and operator processing
        // Note: We pop values when executing instructions, so we add instructions in reverse child order
        public partial record Sumf
        {
            private protected override void CompileNode(Compiler compiler)
            {
                Addend.InnerCompile(compiler);
                Augend.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_SUM));
            }
        }

        public partial record Minusf
        {
            private protected override void CompileNode(Compiler compiler)
            {
                Minuend.InnerCompile(compiler);
                Subtrahend.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_MINUS));
            }
        }

        public partial record Mulf
        {
            private protected override void CompileNode(Compiler compiler)
            {
                Multiplicand.InnerCompile(compiler);
                Multiplier.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_MUL));
            }
        }

        public partial record Divf
        {
            private protected override void CompileNode(Compiler compiler)
            {
                Divisor.InnerCompile(compiler);
                Dividend.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_DIV));
            }
        }

        public partial record Powf
        {
            private protected override void CompileNode(Compiler compiler)
            {
                Exponent.InnerCompile(compiler);
                Base.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_POW));
            }
        }

        public partial record Sinf
        {
            private protected override void CompileNode(Compiler compiler)
            {
                Argument.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_SIN));
            }
        }

        public partial record Cosf
        {
            private protected override void CompileNode(Compiler compiler)
            {
                Argument.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_COS));
            }
        }

        public partial record Secantf
        {
            private protected override void CompileNode(Compiler compiler)
            {
                Argument.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_SECANT));
            }
        }

        public partial record Cosecantf
        {
            private protected override void CompileNode(Compiler compiler)
            {
                Argument.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_COSECANT));
            }
        }

        public partial record Tanf
        {
            private protected override void CompileNode(Compiler compiler)
            {
                Argument.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_TAN));
            }
        }

        public partial record Cotanf
        {
            private protected override void CompileNode(Compiler compiler)
            {
                Argument.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_COTAN));
            }
        }

        public partial record Logf
        {
            private protected override void CompileNode(Compiler compiler)
            {
                // Unlike AngouriMath which accepts Base as the first parameter,
                // Complex.Log accepts it as the second parameter
                // So we reverse reverse the child order -> same as child order
                Base.InnerCompile(compiler);
                Antilogarithm.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_LOG));
            }
        }

        public partial record Arcsinf
        {
            private protected override void CompileNode(Compiler compiler)
            {
                Argument.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_ARCSIN));
            }
        }

        public partial record Arccosf
        {
            private protected override void CompileNode(Compiler compiler)
            {
                Argument.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_ARCCOS));
            }
        }

        public partial record Arctanf
        {
            private protected override void CompileNode(Compiler compiler)
            {
                Argument.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_ARCTAN));
            }
        }

        public partial record Arccotanf
        {
            private protected override void CompileNode(Compiler compiler)
            {
                Argument.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_ARCCOTAN));
            }
        }

        public partial record Arcsecantf
        {
            private protected override void CompileNode(Compiler compiler)
            {
                Argument.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_ARCSECANT));
            }
        }

        public partial record Arccosecantf
        {
            private protected override void CompileNode(Compiler compiler)
            {
                Argument.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_ARCCOSECANT));
            }
        }

        public partial record Factorialf
        {
            private protected override void CompileNode(Compiler compiler)
            {
                Argument.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_FACTORIAL));
            }
        }

        public partial record Derivativef
        {
            private protected override void CompileNode(Compiler compiler) =>
                throw new UncompilableNodeException($"Derivatives cannot be compiled");
        }

        public partial record Integralf
        {
            private protected override void CompileNode(Compiler compiler) =>
                throw new UncompilableNodeException($"Integrals cannot be compiled");
        }

        public partial record Limitf
        {
            private protected override void CompileNode(Compiler compiler) =>
                throw new UncompilableNodeException($"Limits cannot be compiled");
        }

        public partial record Signumf
        {
            private protected override void CompileNode(Compiler compiler)
            {
                Argument.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_SIGNUM));
            }
        }

        public partial record Absf
        {
            private protected override void CompileNode(Compiler compiler)
            {
                Argument.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_ABS));
            }
        }

        public partial record Phif
        {
            private protected override void CompileNode(Compiler compiler)
            {
                Argument.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_PHI));
            }
        }
    }
}

namespace AngouriMath.Core
{
    /// <summary>
    /// Compiled function (not to a delegate, but to AM's VM readable format)
    /// </summary>
    public partial class FastExpression
    {
        /// <summary>The <ref name="Cache"/> stores the saved cache number if zero/positive,
        /// or the bitwise complement of the unsaved cache number if negative.</summary>
        internal sealed record Compiler(List<Instruction> Instructions,
            IReadOnlyDictionary<Variable, int> VarNamespace, IDictionary<Entity, int> Cache)
        {
            /// <summary>Returns a compiled expression. Allows to boost substitution a lot</summary>
            /// <param name="func">The function to be compiled</param>
            /// <param name="variables">Must be equal to func's variables (ignoring constants)</param>
            internal static FastExpression Compile(Entity func, IEnumerable<Variable> variables)
            {
                var varNamespace = new Dictionary<Variable, int>();
                int id = 0;
                foreach (var varName in variables)
                    if (!varName.IsConstant)
                        varNamespace[varName] = id++;
                func = func.Substitute(Variable.ConstantList);
                var visited = new HashSet<Entity>();
                var cache = new Dictionary<Entity, int>();
                foreach (var node in func.Nodes)
                    if (node is Number or Variable)
                        continue; // Don't store simple nodes in cache
                    else if (visited.Contains(node))
                    {
                        if (!cache.ContainsKey(node))
                            cache.Add(node, ~cache.Count); // Unsaved by default
                    }
                    else visited.Add(node);
                var compiler = new Compiler(new(), varNamespace, cache);
                func.InnerCompile(compiler);
                return new(id, compiler.Instructions, compiler.Cache.Count);
            }
        }
    }
}