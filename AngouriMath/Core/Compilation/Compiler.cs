/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System.Collections.Generic;
using AngouriMath.Core.Exceptions;
using static AngouriMath.Entity;
using static AngouriMath.Core.FastExpression;
using System;

namespace AngouriMath
{
    public partial record Entity
    {
        private protected virtual void CompileNode(Compiler compiler)
            => throw new NotSupportedException($"The node of type {GetType()} does not support compilation. Feel free to report it as an issue on our official repository.");
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
        }

        public partial record Tensor : Entity
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
    }
}

namespace AngouriMath.Core
{
    public partial class FastExpression
    {
        /// <summary>The <paramref name="Cache"/> stores the saved cache number if zero/positive,
        /// or the bitwise complement of the unsaved cache number if negative.</summary>
        internal record Compiler(List<Instruction> Instructions,
            IReadOnlyDictionary<Variable, int> VarNamespace, IDictionary<Entity, int> Cache)
        {
            /// <summary>Returns a compiled expression. Allows to boost substitution a lot</summary>
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