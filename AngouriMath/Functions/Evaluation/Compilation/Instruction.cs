using AngouriMath.Core;
using System;
using System.Collections.Generic;
using System.Text;
using static AngouriMath.CompiledMathFunctions;

namespace AngouriMath
{
    internal class Instruction
    {
        internal enum InstructionType
        {
            PUSHVAR,
            PUSHCONST,
            CALL
        }
        internal InstructionType Type;
        internal int VarCount;
        internal int FuncNumber;
        internal int VarNumber;
        internal string FuncName;
        internal Number Value;
        internal Instruction(string funcName, int varCount)
        {
            FuncName = funcName;
            FuncNumber = CompiledMathFunctions.func2Num[funcName];
            VarCount = varCount;
            Type = InstructionType.CALL;
        }
        internal Instruction(int varNumber)
        {
            VarNumber = varNumber;
            Type = InstructionType.PUSHVAR;
        }
        internal Instruction(Number value)
        {
            Value = value;
            Type = InstructionType.PUSHCONST;
        }

        public override string ToString()
        {
            string b = Type.ToString() + " ";
            if (Type == InstructionType.CALL)
                return b + FuncName;
            else if (Type == InstructionType.PUSHCONST)
                return b + Value;
            else
                return b + VarNumber.ToString();
        }
    }
    internal class InstructionSet : List<Instruction>
    {
        internal void AddInstruction(string funcName, int varCount) 
            => Add(new Instruction(funcName, varCount));
        internal void AddInstruction(int varName)
            => Add(new Instruction(varName));
        internal void AddInstruction(Number value)
            => Add(new Instruction(value));
    }
}
