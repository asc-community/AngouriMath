//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using static AngouriMath.Entity.Set;
using static AngouriMath.Entity;

namespace AngouriMath.Extensions
{
    public static partial class AngouriMathExtensions
    {
        ///<summary>Solves a given set of arbitrary equations</summary>
        ///<returns>A tensor whose width is 2 columns long or null if no solutions were found</returns>
        public static Matrix? SolveSystem(this (string eq1, string eq2) eqs, string var1, string var2)
            => MathS.Equations(eqs.eq1, eqs.eq2).Solve(var1, var2);

        ///<summary>Solves a given set of arbitrary equations</summary>
        ///<returns>A tensor whose width is 3 columns long or null if no solutions were found</returns>
        public static Matrix? SolveSystem(this (string eq1, string eq2, string eq3) eqs, string var1, string var2, string var3)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3).Solve(var1, var2, var3);

        ///<summary>Solves a given set of arbitrary equations</summary>
        ///<returns>A tensor whose width is 4 columns long or null if no solutions were found</returns>
        public static Matrix? SolveSystem(this (string eq1, string eq2, string eq3, string eq4) eqs, string var1, string var2, string var3, string var4)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3, eqs.eq4).Solve(var1, var2, var3, var4);

        ///<summary>Solves a given set of arbitrary equations</summary>
        ///<returns>A tensor whose width is 5 columns long or null if no solutions were found</returns>
        public static Matrix? SolveSystem(this (string eq1, string eq2, string eq3, string eq4, string eq5) eqs, string var1, string var2, string var3, string var4, string var5)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3, eqs.eq4, eqs.eq5).Solve(var1, var2, var3, var4, var5);

        ///<summary>Solves a given set of arbitrary equations</summary>
        ///<returns>A tensor whose width is 6 columns long or null if no solutions were found</returns>
        public static Matrix? SolveSystem(this (string eq1, string eq2, string eq3, string eq4, string eq5, string eq6) eqs, string var1, string var2, string var3, string var4, string var5, string var6)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3, eqs.eq4, eqs.eq5, eqs.eq6).Solve(var1, var2, var3, var4, var5, var6);

        ///<summary>Solves a given set of arbitrary equations</summary>
        ///<returns>A tensor whose width is 7 columns long or null if no solutions were found</returns>
        public static Matrix? SolveSystem(this (string eq1, string eq2, string eq3, string eq4, string eq5, string eq6, string eq7) eqs, string var1, string var2, string var3, string var4, string var5, string var6, string var7)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3, eqs.eq4, eqs.eq5, eqs.eq6, eqs.eq7).Solve(var1, var2, var3, var4, var5, var6, var7);

        ///<summary>Solves a given set of arbitrary equations</summary>
        ///<returns>A tensor whose width is 8 columns long or null if no solutions were found</returns>
        public static Matrix? SolveSystem(this (string eq1, string eq2, string eq3, string eq4, string eq5, string eq6, string eq7, string eq8) eqs, string var1, string var2, string var3, string var4, string var5, string var6, string var7, string var8)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3, eqs.eq4, eqs.eq5, eqs.eq6, eqs.eq7, eqs.eq8).Solve(var1, var2, var3, var4, var5, var6, var7, var8);

        ///<summary>Solves a given set of arbitrary equations</summary>
        ///<returns>A tensor whose width is 9 columns long or null if no solutions were found</returns>
        public static Matrix? SolveSystem(this (string eq1, string eq2, string eq3, string eq4, string eq5, string eq6, string eq7, string eq8, string eq9) eqs, string var1, string var2, string var3, string var4, string var5, string var6, string var7, string var8, string var9)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3, eqs.eq4, eqs.eq5, eqs.eq6, eqs.eq7, eqs.eq8, eqs.eq9).Solve(var1, var2, var3, var4, var5, var6, var7, var8, var9);

        /// <summary>
        /// Takes a <see cref="int"/> and <see cref="int"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (int left, int right) arg) 
            => MathS.Sets.Interval(arg.left, arg.right);

        /// <summary>
        /// Takes a <see cref="int"/> and <see cref="int"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (int left, bool leftClosed, int right, bool rightClosed) arg) 
            => new Interval(arg.left, arg.leftClosed, arg.right, arg.rightClosed);
        /// <summary>
        /// Takes a <see cref="int"/> and <see cref="double"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (int left, double right) arg) 
            => MathS.Sets.Interval(arg.left, arg.right);

        /// <summary>
        /// Takes a <see cref="int"/> and <see cref="double"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (int left, bool leftClosed, double right, bool rightClosed) arg) 
            => new Interval(arg.left, arg.leftClosed, arg.right, arg.rightClosed);
        /// <summary>
        /// Takes a <see cref="int"/> and <see cref="float"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (int left, float right) arg) 
            => MathS.Sets.Interval(arg.left, arg.right);

        /// <summary>
        /// Takes a <see cref="int"/> and <see cref="float"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (int left, bool leftClosed, float right, bool rightClosed) arg) 
            => new Interval(arg.left, arg.leftClosed, arg.right, arg.rightClosed);
        /// <summary>
        /// Takes a <see cref="int"/> and <see cref="string"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (int left, string right) arg) 
            => MathS.Sets.Interval(arg.left, arg.right);

        /// <summary>
        /// Takes a <see cref="int"/> and <see cref="string"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (int left, bool leftClosed, string right, bool rightClosed) arg) 
            => new Interval(arg.left, arg.leftClosed, arg.right, arg.rightClosed);
        /// <summary>
        /// Takes a <see cref="double"/> and <see cref="int"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (double left, int right) arg) 
            => MathS.Sets.Interval(arg.left, arg.right);

        /// <summary>
        /// Takes a <see cref="double"/> and <see cref="int"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (double left, bool leftClosed, int right, bool rightClosed) arg) 
            => new Interval(arg.left, arg.leftClosed, arg.right, arg.rightClosed);
        /// <summary>
        /// Takes a <see cref="double"/> and <see cref="double"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (double left, double right) arg) 
            => MathS.Sets.Interval(arg.left, arg.right);

        /// <summary>
        /// Takes a <see cref="double"/> and <see cref="double"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (double left, bool leftClosed, double right, bool rightClosed) arg) 
            => new Interval(arg.left, arg.leftClosed, arg.right, arg.rightClosed);
        /// <summary>
        /// Takes a <see cref="double"/> and <see cref="float"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (double left, float right) arg) 
            => MathS.Sets.Interval(arg.left, arg.right);

        /// <summary>
        /// Takes a <see cref="double"/> and <see cref="float"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (double left, bool leftClosed, float right, bool rightClosed) arg) 
            => new Interval(arg.left, arg.leftClosed, arg.right, arg.rightClosed);
        /// <summary>
        /// Takes a <see cref="double"/> and <see cref="string"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (double left, string right) arg) 
            => MathS.Sets.Interval(arg.left, arg.right);

        /// <summary>
        /// Takes a <see cref="double"/> and <see cref="string"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (double left, bool leftClosed, string right, bool rightClosed) arg) 
            => new Interval(arg.left, arg.leftClosed, arg.right, arg.rightClosed);
        /// <summary>
        /// Takes a <see cref="float"/> and <see cref="int"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (float left, int right) arg) 
            => MathS.Sets.Interval(arg.left, arg.right);

        /// <summary>
        /// Takes a <see cref="float"/> and <see cref="int"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (float left, bool leftClosed, int right, bool rightClosed) arg) 
            => new Interval(arg.left, arg.leftClosed, arg.right, arg.rightClosed);
        /// <summary>
        /// Takes a <see cref="float"/> and <see cref="double"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (float left, double right) arg) 
            => MathS.Sets.Interval(arg.left, arg.right);

        /// <summary>
        /// Takes a <see cref="float"/> and <see cref="double"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (float left, bool leftClosed, double right, bool rightClosed) arg) 
            => new Interval(arg.left, arg.leftClosed, arg.right, arg.rightClosed);
        /// <summary>
        /// Takes a <see cref="float"/> and <see cref="float"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (float left, float right) arg) 
            => MathS.Sets.Interval(arg.left, arg.right);

        /// <summary>
        /// Takes a <see cref="float"/> and <see cref="float"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (float left, bool leftClosed, float right, bool rightClosed) arg) 
            => new Interval(arg.left, arg.leftClosed, arg.right, arg.rightClosed);
        /// <summary>
        /// Takes a <see cref="float"/> and <see cref="string"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (float left, string right) arg) 
            => MathS.Sets.Interval(arg.left, arg.right);

        /// <summary>
        /// Takes a <see cref="float"/> and <see cref="string"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (float left, bool leftClosed, string right, bool rightClosed) arg) 
            => new Interval(arg.left, arg.leftClosed, arg.right, arg.rightClosed);
        /// <summary>
        /// Takes a <see cref="string"/> and <see cref="int"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (string left, int right) arg) 
            => MathS.Sets.Interval(arg.left, arg.right);

        /// <summary>
        /// Takes a <see cref="string"/> and <see cref="int"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (string left, bool leftClosed, int right, bool rightClosed) arg) 
            => new Interval(arg.left, arg.leftClosed, arg.right, arg.rightClosed);
        /// <summary>
        /// Takes a <see cref="string"/> and <see cref="double"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (string left, double right) arg) 
            => MathS.Sets.Interval(arg.left, arg.right);

        /// <summary>
        /// Takes a <see cref="string"/> and <see cref="double"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (string left, bool leftClosed, double right, bool rightClosed) arg) 
            => new Interval(arg.left, arg.leftClosed, arg.right, arg.rightClosed);
        /// <summary>
        /// Takes a <see cref="string"/> and <see cref="float"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (string left, float right) arg) 
            => MathS.Sets.Interval(arg.left, arg.right);

        /// <summary>
        /// Takes a <see cref="string"/> and <see cref="float"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (string left, bool leftClosed, float right, bool rightClosed) arg) 
            => new Interval(arg.left, arg.leftClosed, arg.right, arg.rightClosed);
        /// <summary>
        /// Takes a <see cref="string"/> and <see cref="string"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (string left, string right) arg) 
            => MathS.Sets.Interval(arg.left, arg.right);

        /// <summary>
        /// Takes a <see cref="string"/> and <see cref="string"/> and returns
        /// a closed interval (so that left and right ends are included)
        /// </summary>
        /// <returns>Interval</returns>
        public static Interval ToInterval(this (string left, bool leftClosed, string right, bool rightClosed) arg) 
            => new Interval(arg.left, arg.leftClosed, arg.right, arg.rightClosed);

    }
}