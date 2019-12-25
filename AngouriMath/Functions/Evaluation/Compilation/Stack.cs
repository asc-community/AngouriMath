using AngouriMath.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath
{
    internal class Stack
    {
        private Number[] inner;
        private int pointer;
        internal int Depth { get => pointer + 1; }
        public Stack(int maxLen)
        {
            inner = new Number[maxLen];
            pointer = -1;
        }
        internal void Pop(int len, Number[] dst)
        {
            if (len > pointer + 1)
                throw new Exception("Out of stack");
            for (int i = 0; i < len; i++)
                dst[i] = inner[pointer - i];
            pointer -= len;
        }
        internal void Push(Number number)
        {
            pointer++;
            inner[pointer] = number;
        }
        internal void Clear() => pointer = -1;
        internal Number Last { get => inner[pointer]; }
    }
}
