using AngouriMath.Core;
using System;
using System.Collections.Generic;
using System.Text;

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
        internal Number Value;
        internal Instruction(string funcName, int varCount)
        {
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
