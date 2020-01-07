using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace AngouriMath
{
    public abstract partial class Entity
    {
        /// <summary>
        /// Compile function so you can evaluate numerical value 15x faster,
        /// than subsitution
        /// </summary>
        /// <param name="variables">
        /// List string names of variables in the same order
        /// as you will list them when evaluating
        /// </param>
        /// <returns></returns>
        public FastExpression Compile(params VariableEntity[] variables)
        {
            var strings = new List<string>();
            foreach (var varEnt in variables)
                strings.Add(varEnt.Name);
            return Compile(strings.ToArray());
        }

        /// <summary>
        /// Compile function so you can evaluate numerical value 15x faster,
        /// than subsitution
        /// </summary>
        /// <param name="variables">
        /// List string names of variables in the same order
        /// as you will list them when evaluating
        /// </param>
        /// <returns></returns>
        public FastExpression Compile(params string[] variables)
        {
            var varNamespace = new Dictionary<string, int>();
            int id = 0;
            foreach (var varName in variables)
            {
                varNamespace[varName] = id;
                id++;
            }
            var res = new FastExpression(variables.Length, this);
            InnerCompile(res, variables, varNamespace);
            res.Seal(); // Creates stack
            return res;
        }
    } 
}

namespace AngouriMath
{
    public abstract partial class Entity
    {
        /// <summary>
        /// Returns number of nodes in tree
        /// </summary>
        /// <returns></returns>
        private int Complexity()
        {
            int res = 0;
            foreach (var child in Children)
                res += child.Complexity();
            return res + 1;
        }

        /// <summary>
        /// Obviouosly, returns number of subtrees having exact same hash
        /// </summary>
        /// <returns></returns>
        internal int CountOccurances(string hash)
        {
            int res = this.ToString() == hash ? 1 : 0;
            foreach (var child in Children)
                res += child.CountOccurances(hash);
            return res;
        }

        /// <summary>
        /// Fills fast expression instance with instructions and cache
        /// </summary>
        /// <param name="fe"></param>
        /// <param name="variables"></param>
        /// <param name="varNamespace"></param>
        private void InnerCompile(FastExpression fe, string[] variables, Dictionary<string, int> varNamespace)
        {
            // Check whether it's better to pull from cache or not
            string hash = ToString();
            if (fe.HashToNum.ContainsKey(hash))
            {
                fe.instructions.AddPullCacheInstruction(fe.HashToNum[hash]);
                return;
            }

            for (int i = Children.Count - 1; i >= 0; i--)
                Children[i].InnerCompile(fe, variables, varNamespace);
            if (this is OperatorEntity || this is FunctionEntity)
                fe.instructions.AddCallInstruction(Name, Children.Count);
            else if (this is NumberEntity)
                fe.instructions.AddPushNumInstruction(GetValue().value);
            else if (this is VariableEntity)
                fe.instructions.AddPushVarInstruction(varNamespace[Name]);
            else
                throw new SysException("Unknown entity");

            // If the function is used more than once AND complex enough, we put it in cache
            if (fe.RawExpr.CountOccurances(hash) > 1 /*Raw expr is basically the root entity that we're compiling*/
                && Complexity() > 1 /* we don't check if it is already in cache as in this case it will pull from cache*/)
            {
                fe.HashToNum[hash] = fe.HashToNum.Count;
                fe.instructions.AddPushCacheInstruction(fe.HashToNum[hash]);
            }
        }
    }
    public class FastExpression
    {
        private Stack<Complex> stack;
        internal readonly InstructionSet instructions;
        private readonly int varCount;

        internal readonly Dictionary<string, int> HashToNum = new Dictionary<string, int>();
        private Complex[] Cache;

        internal Entity RawExpr { get; }

        internal FastExpression(int varCount, Entity rawExpr)
        {
            this.varCount = varCount;
            this.instructions = new InstructionSet();
            RawExpr = rawExpr;
        }

        internal void Seal()
        {
            stack = new Stack<Complex>(instructions.Count);
            Cache = new Complex[HashToNum.Count];
        }

        /// <summary>
        /// Calls the compiled function (synonim to Substitute)
        /// </summary>
        /// <param name="variables">
        /// List arguments in the same order in which you compiled the function
        /// </param>
        /// <returns></returns>
        public Number Call(params Number[] variables)
            => Substitute(variables);

        /// <summary>
        /// Calls the compiled function (synonim to Call)
        /// </summary>
        /// <param name="variables">
        /// List arguments in the same order in which you compiled the function
        /// </param>
        /// <returns></returns>
        public Number Substitute(params Number[] variables)
        {
            if (variables.Length != varCount)
                throw new SysException("Wrong amount of parameters");
            Instruction instruction;
            for (int i = 0; i < instructions.Count; i++)
            {
                instruction = instructions[i];
                switch (instruction.Type)
                {
                    case Instruction.InstructionType.PUSHVAR:
                        stack.Push(variables[instruction.VarNumber]);
                        break;
                    case Instruction.InstructionType.PUSHCONST:
                        stack.Push(instruction.Value);
                        break;
                    case Instruction.InstructionType.CALL:
                        switch(instruction.FuncNumber)
                        {
                            case 0:
                                stack.Push(stack.Pop() + stack.Pop());
                                break;
                            case 1:
                                stack.Push(stack.Pop() - stack.Pop());
                                break;
                            case 2:
                                stack.Push(stack.Pop() * stack.Pop());
                                break;
                            case 3:
                                stack.Push(stack.Pop() / stack.Pop());
                                break;
                            case 4:
                                stack.Push(Complex.Pow(stack.Pop(), stack.Pop()));
                                break;
                            case 5:
                                stack.Push(Complex.Sin(stack.Pop()));
                                break;
                            case 6:
                                stack.Push(Complex.Cos(stack.Pop()));
                                break;
                            case 7:
                                stack.Push(Complex.Tan(stack.Pop()));
                                break;
                            case 8:
                                stack.Push(1 / Complex.Tan(stack.Pop()));
                                break;
                            case 9:
                                stack.Push(Complex.Log(stack.Pop(), stack.Pop().Real));
                                break;
                            case 10:
                                stack.Push(Complex.Asin(stack.Pop()));
                                break;
                            case 11:
                                stack.Push(Complex.Acos(stack.Pop()));
                                break;
                            case 12:
                                stack.Push(Complex.Atan(stack.Pop()));
                                break;
                            case 13:
                                stack.Push(Complex.Atan(1 / stack.Pop()));
                                break;
                        }
                        break;
                    case Instruction.InstructionType.PULLCACHE:
                        stack.Push(Cache[instruction.CacheNumber]);
                        break;
                    default:
                        Cache[instruction.CacheNumber] = stack.Peek();
                        break;
                }
            }
            return stack.Pop();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var instruction in instructions)
                sb.Append(instruction.ToString()).Append("\n");
            return sb.ToString();
        }
    }
}
