using System;
using System.Collections.Immutable;
using System.Linq;
using AngouriMath;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Boolean;
using static AngouriMath.MathS;
using AngouriMath.Extensions;
using System.Threading;
using System.Collections.Generic;
using static AngouriMath.Entity.Number;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            Integer a = 3;
            if (a is Integer { EInteger: var ein })
                a = ein.ToInt32Checked();
            Entity expr1 = "1.0000000000000000000001";
            Entity expr2 = "1";
            Console.WriteLine(expr1 + expr2);
            
        }
    }
}