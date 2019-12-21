using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath
{
    using PatType = Entity.PatType;
    internal class Patterns
    {
        static Pattern a = new Pattern(0, PatType.COMMON);
        static Pattern b = new Pattern(1, PatType.COMMON);
        static Pattern c = new Pattern(2, PatType.COMMON);
        private static int InternNumber = 10000;
        static Pattern Num(double a) => new Pattern(++InternNumber, PatType.NUMBER, a.ToString());
        static VariableEntity x = new VariableEntity("4");
        internal static Dictionary<Pattern, Entity> patterns = new Dictionary<Pattern, Entity> {
            { a / (b / c), a * c / b },
            { Powf.PHang(Sinf.PHang(a), Num(2)) + Powf.PHang(Cosf.PHang(a), Num(2)), 1 },
            { Powf.PHang(Cosf.PHang(a), Num(2)) + Powf.PHang(Sinf.PHang(a), Num(2)), 1 },
        };
    }
}
