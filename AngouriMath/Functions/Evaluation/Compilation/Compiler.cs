
/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Collections.Generic;
using static AngouriMath.Entity;
using static AngouriMath.Instruction;

namespace AngouriMath
{
    public partial record Entity
    {
        private protected abstract void InnerCompile_(Compiler compiler);
        /// <summary>
        /// Recursive compilation that pushes intructions to the stack (<see cref="Compiler.Instructions"/>)
        /// </summary>
        internal void InnerCompile(Compiler compiler)
        {
            // Check whether it's better to pull from cache or not
            if (compiler.Cache.TryGetValue(this, out var cacheLine) && cacheLine > -1)
            {
                compiler.Instructions.Add(new(InstructionType.PULLCACHE, cacheLine));
                return;
            }
            InnerCompile_(compiler);
            if (cacheLine == -1)
            {
                cacheLine = compiler.Cache.Count;
                compiler.Cache[this] = cacheLine;
                compiler.Instructions.Add(new(InstructionType.TOCACHE, cacheLine));
            }
        }
        public partial record Number : Entity
        {
            private protected override void InnerCompile_(Compiler compiler) =>
                compiler.Instructions.Add(new(InstructionType.PUSHCONST, Value: AsComplex()));
        }
        public partial record Variable : Entity
        {
            private protected override void InnerCompile_(Compiler compiler) =>
                compiler.Instructions.Add(new(InstructionType.PUSHVAR, compiler.VarNamespace[Name]));
        }
        public partial record Tensor : Entity
        {
            private protected override void InnerCompile_(Compiler compiler) =>
                throw new UncompilableNodeException($"Tensors cannot be compiled");
        }
        // Each function and operator processing
        public partial record Sumf
        {
            private protected override void InnerCompile_(Compiler compiler)
            {
                Augend.InnerCompile(compiler);
                Addend.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_SUM));
            }
        }
        public partial record Minusf
        {
            private protected override void InnerCompile_(Compiler compiler)
            {
                Subtrahend.InnerCompile(compiler);
                Minuend.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_MINUS));
            }
        }
        public partial record Mulf
        {
            private protected override void InnerCompile_(Compiler compiler)
            {
                Multiplier.InnerCompile(compiler);
                Multiplicand.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_MUL));
            }
        }
        public partial record Divf
        {
            private protected override void InnerCompile_(Compiler compiler)
            {
                Dividend.InnerCompile(compiler);
                Divisor.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_DIV));
            }
        }
        public partial record Powf
        {
            private protected override void InnerCompile_(Compiler compiler)
            {
                Base.InnerCompile(compiler);
                Exponent.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_POW));
            }
        }
        public partial record Sinf
        {
            private protected override void InnerCompile_(Compiler compiler)
            {
                Argument.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_SIN));
            }
        }
        public partial record Cosf
        {
            private protected override void InnerCompile_(Compiler compiler)
            {
                Argument.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_COS));
            }
        }
        public partial record Tanf
        {
            private protected override void InnerCompile_(Compiler compiler)
            {
                Argument.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_TAN));
            }
        }
        public partial record Cotanf
        {
            private protected override void InnerCompile_(Compiler compiler)
            {
                Argument.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_COTAN));
            }
        }
        public partial record Logf
        {
            private protected override void InnerCompile_(Compiler compiler)
            {
                // Unlike AngouriMath which accepts Base as the first parameter,
                // Complex.Log accepts it as the second parameter
                Antilogarithm.InnerCompile(compiler);
                Base.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_LOG));
            }
        }

        public partial record Arcsinf
        {
            private protected override void InnerCompile_(Compiler compiler)
            {
                Argument.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_ARCSIN));
            }
        }
        public partial record Arccosf
        {
            private protected override void InnerCompile_(Compiler compiler)
            {
                Argument.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_ARCCOS));
            }
        }
        public partial record Arctanf
        {
            private protected override void InnerCompile_(Compiler compiler)
            {
                Argument.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_ARCTAN));
            }
        }
        public partial record Arccotanf
        {
            private protected override void InnerCompile_(Compiler compiler)
            {
                Argument.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_ARCCOTAN));
            }
        }
        public partial record Factorialf
        {
            private protected override void InnerCompile_(Compiler compiler)
            {
                Argument.InnerCompile(compiler);
                compiler.Instructions.Add(new(InstructionType.CALL_FACTORIAL));
            }
        }
        public partial record Derivativef
        {
            private protected override void InnerCompile_(Compiler compiler) =>
                throw new UncompilableNodeException($"Derivatives cannot be compiled");
        }
        public partial record Integralf
        {
            private protected override void InnerCompile_(Compiler compiler) =>
                throw new UncompilableNodeException($"Integrals cannot be compiled");
        }
        public partial record Limitf
        {
            private protected override void InnerCompile_(Compiler compiler) =>
                throw new UncompilableNodeException($"Limits cannot be compiled");
        }
    }
    internal partial record Instruction
    {
        internal record Compiler(List<Instruction> Instructions, Dictionary<Variable, int> VarNamespace, Dictionary<Entity, int> Cache)
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
                foreach (var node in func)
                {
                    if (node is Number or Variable)
                        continue; // Don't store simple nodes in cache
                    if (visited.Contains(node))
                        cache[node] = -1;
                    else visited.Add(node);
                }
                var compiler = new Compiler(new(), varNamespace, cache);
                func.InnerCompile(compiler);
                return new(id, compiler.Instructions, compiler.Cache.Count);
            }
        }
    }
}