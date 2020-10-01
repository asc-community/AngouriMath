
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

using AngouriMath;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Extensions
{
	public static class AngouriMathExtensions
	{
		/// <summary>
		/// Takes a <see cref="int"/> and <see cref="int"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (int left, int right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="int"/> and <see cref="int"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (int left, bool leftClosed, int right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="int"/> and <see cref="uint"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (int left, uint right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="int"/> and <see cref="uint"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (int left, bool leftClosed, uint right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="int"/> and <see cref="short"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (int left, short right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="int"/> and <see cref="short"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (int left, bool leftClosed, short right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="int"/> and <see cref="ushort"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (int left, ushort right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="int"/> and <see cref="ushort"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (int left, bool leftClosed, ushort right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="int"/> and <see cref="byte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (int left, byte right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="int"/> and <see cref="byte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (int left, bool leftClosed, byte right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="int"/> and <see cref="sbyte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (int left, sbyte right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="int"/> and <see cref="sbyte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (int left, bool leftClosed, sbyte right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="int"/> and <see cref="long"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (int left, long right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="int"/> and <see cref="long"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (int left, bool leftClosed, long right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="int"/> and <see cref="ulong"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (int left, ulong right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="int"/> and <see cref="ulong"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (int left, bool leftClosed, ulong right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="int"/> and <see cref="string"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (int left, string right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="int"/> and <see cref="string"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (int left, bool leftClosed, string right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="uint"/> and <see cref="int"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (uint left, int right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="uint"/> and <see cref="int"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (uint left, bool leftClosed, int right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="uint"/> and <see cref="uint"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (uint left, uint right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="uint"/> and <see cref="uint"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (uint left, bool leftClosed, uint right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="uint"/> and <see cref="short"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (uint left, short right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="uint"/> and <see cref="short"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (uint left, bool leftClosed, short right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="uint"/> and <see cref="ushort"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (uint left, ushort right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="uint"/> and <see cref="ushort"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (uint left, bool leftClosed, ushort right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="uint"/> and <see cref="byte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (uint left, byte right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="uint"/> and <see cref="byte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (uint left, bool leftClosed, byte right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="uint"/> and <see cref="sbyte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (uint left, sbyte right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="uint"/> and <see cref="sbyte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (uint left, bool leftClosed, sbyte right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="uint"/> and <see cref="long"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (uint left, long right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="uint"/> and <see cref="long"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (uint left, bool leftClosed, long right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="uint"/> and <see cref="ulong"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (uint left, ulong right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="uint"/> and <see cref="ulong"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (uint left, bool leftClosed, ulong right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="uint"/> and <see cref="string"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (uint left, string right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="uint"/> and <see cref="string"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (uint left, bool leftClosed, string right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="short"/> and <see cref="int"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (short left, int right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="short"/> and <see cref="int"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (short left, bool leftClosed, int right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="short"/> and <see cref="uint"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (short left, uint right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="short"/> and <see cref="uint"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (short left, bool leftClosed, uint right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="short"/> and <see cref="short"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (short left, short right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="short"/> and <see cref="short"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (short left, bool leftClosed, short right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="short"/> and <see cref="ushort"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (short left, ushort right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="short"/> and <see cref="ushort"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (short left, bool leftClosed, ushort right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="short"/> and <see cref="byte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (short left, byte right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="short"/> and <see cref="byte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (short left, bool leftClosed, byte right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="short"/> and <see cref="sbyte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (short left, sbyte right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="short"/> and <see cref="sbyte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (short left, bool leftClosed, sbyte right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="short"/> and <see cref="long"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (short left, long right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="short"/> and <see cref="long"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (short left, bool leftClosed, long right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="short"/> and <see cref="ulong"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (short left, ulong right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="short"/> and <see cref="ulong"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (short left, bool leftClosed, ulong right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="short"/> and <see cref="string"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (short left, string right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="short"/> and <see cref="string"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (short left, bool leftClosed, string right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="ushort"/> and <see cref="int"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ushort left, int right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="ushort"/> and <see cref="int"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ushort left, bool leftClosed, int right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="ushort"/> and <see cref="uint"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ushort left, uint right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="ushort"/> and <see cref="uint"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ushort left, bool leftClosed, uint right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="ushort"/> and <see cref="short"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ushort left, short right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="ushort"/> and <see cref="short"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ushort left, bool leftClosed, short right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="ushort"/> and <see cref="ushort"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ushort left, ushort right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="ushort"/> and <see cref="ushort"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ushort left, bool leftClosed, ushort right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="ushort"/> and <see cref="byte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ushort left, byte right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="ushort"/> and <see cref="byte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ushort left, bool leftClosed, byte right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="ushort"/> and <see cref="sbyte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ushort left, sbyte right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="ushort"/> and <see cref="sbyte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ushort left, bool leftClosed, sbyte right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="ushort"/> and <see cref="long"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ushort left, long right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="ushort"/> and <see cref="long"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ushort left, bool leftClosed, long right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="ushort"/> and <see cref="ulong"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ushort left, ulong right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="ushort"/> and <see cref="ulong"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ushort left, bool leftClosed, ulong right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="ushort"/> and <see cref="string"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ushort left, string right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="ushort"/> and <see cref="string"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ushort left, bool leftClosed, string right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="byte"/> and <see cref="int"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (byte left, int right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="byte"/> and <see cref="int"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (byte left, bool leftClosed, int right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="byte"/> and <see cref="uint"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (byte left, uint right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="byte"/> and <see cref="uint"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (byte left, bool leftClosed, uint right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="byte"/> and <see cref="short"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (byte left, short right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="byte"/> and <see cref="short"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (byte left, bool leftClosed, short right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="byte"/> and <see cref="ushort"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (byte left, ushort right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="byte"/> and <see cref="ushort"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (byte left, bool leftClosed, ushort right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="byte"/> and <see cref="byte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (byte left, byte right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="byte"/> and <see cref="byte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (byte left, bool leftClosed, byte right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="byte"/> and <see cref="sbyte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (byte left, sbyte right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="byte"/> and <see cref="sbyte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (byte left, bool leftClosed, sbyte right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="byte"/> and <see cref="long"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (byte left, long right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="byte"/> and <see cref="long"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (byte left, bool leftClosed, long right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="byte"/> and <see cref="ulong"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (byte left, ulong right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="byte"/> and <see cref="ulong"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (byte left, bool leftClosed, ulong right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="byte"/> and <see cref="string"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (byte left, string right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="byte"/> and <see cref="string"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (byte left, bool leftClosed, string right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="sbyte"/> and <see cref="int"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (sbyte left, int right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="sbyte"/> and <see cref="int"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (sbyte left, bool leftClosed, int right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="sbyte"/> and <see cref="uint"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (sbyte left, uint right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="sbyte"/> and <see cref="uint"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (sbyte left, bool leftClosed, uint right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="sbyte"/> and <see cref="short"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (sbyte left, short right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="sbyte"/> and <see cref="short"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (sbyte left, bool leftClosed, short right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="sbyte"/> and <see cref="ushort"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (sbyte left, ushort right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="sbyte"/> and <see cref="ushort"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (sbyte left, bool leftClosed, ushort right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="sbyte"/> and <see cref="byte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (sbyte left, byte right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="sbyte"/> and <see cref="byte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (sbyte left, bool leftClosed, byte right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="sbyte"/> and <see cref="sbyte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (sbyte left, sbyte right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="sbyte"/> and <see cref="sbyte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (sbyte left, bool leftClosed, sbyte right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="sbyte"/> and <see cref="long"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (sbyte left, long right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="sbyte"/> and <see cref="long"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (sbyte left, bool leftClosed, long right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="sbyte"/> and <see cref="ulong"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (sbyte left, ulong right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="sbyte"/> and <see cref="ulong"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (sbyte left, bool leftClosed, ulong right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="sbyte"/> and <see cref="string"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (sbyte left, string right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="sbyte"/> and <see cref="string"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (sbyte left, bool leftClosed, string right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="long"/> and <see cref="int"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (long left, int right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="long"/> and <see cref="int"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (long left, bool leftClosed, int right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="long"/> and <see cref="uint"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (long left, uint right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="long"/> and <see cref="uint"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (long left, bool leftClosed, uint right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="long"/> and <see cref="short"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (long left, short right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="long"/> and <see cref="short"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (long left, bool leftClosed, short right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="long"/> and <see cref="ushort"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (long left, ushort right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="long"/> and <see cref="ushort"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (long left, bool leftClosed, ushort right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="long"/> and <see cref="byte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (long left, byte right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="long"/> and <see cref="byte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (long left, bool leftClosed, byte right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="long"/> and <see cref="sbyte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (long left, sbyte right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="long"/> and <see cref="sbyte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (long left, bool leftClosed, sbyte right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="long"/> and <see cref="long"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (long left, long right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="long"/> and <see cref="long"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (long left, bool leftClosed, long right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="long"/> and <see cref="ulong"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (long left, ulong right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="long"/> and <see cref="ulong"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (long left, bool leftClosed, ulong right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="long"/> and <see cref="string"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (long left, string right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="long"/> and <see cref="string"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (long left, bool leftClosed, string right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="ulong"/> and <see cref="int"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ulong left, int right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="ulong"/> and <see cref="int"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ulong left, bool leftClosed, int right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="ulong"/> and <see cref="uint"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ulong left, uint right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="ulong"/> and <see cref="uint"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ulong left, bool leftClosed, uint right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="ulong"/> and <see cref="short"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ulong left, short right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="ulong"/> and <see cref="short"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ulong left, bool leftClosed, short right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="ulong"/> and <see cref="ushort"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ulong left, ushort right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="ulong"/> and <see cref="ushort"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ulong left, bool leftClosed, ushort right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="ulong"/> and <see cref="byte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ulong left, byte right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="ulong"/> and <see cref="byte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ulong left, bool leftClosed, byte right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="ulong"/> and <see cref="sbyte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ulong left, sbyte right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="ulong"/> and <see cref="sbyte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ulong left, bool leftClosed, sbyte right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="ulong"/> and <see cref="long"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ulong left, long right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="ulong"/> and <see cref="long"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ulong left, bool leftClosed, long right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="ulong"/> and <see cref="ulong"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ulong left, ulong right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="ulong"/> and <see cref="ulong"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ulong left, bool leftClosed, ulong right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="ulong"/> and <see cref="string"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ulong left, string right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="ulong"/> and <see cref="string"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (ulong left, bool leftClosed, string right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="string"/> and <see cref="int"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (string left, int right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="string"/> and <see cref="int"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (string left, bool leftClosed, int right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="string"/> and <see cref="uint"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (string left, uint right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="string"/> and <see cref="uint"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (string left, bool leftClosed, uint right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="string"/> and <see cref="short"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (string left, short right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="string"/> and <see cref="short"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (string left, bool leftClosed, short right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="string"/> and <see cref="ushort"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (string left, ushort right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="string"/> and <see cref="ushort"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (string left, bool leftClosed, ushort right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="string"/> and <see cref="byte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (string left, byte right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="string"/> and <see cref="byte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (string left, bool leftClosed, byte right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="string"/> and <see cref="sbyte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (string left, sbyte right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="string"/> and <see cref="sbyte"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (string left, bool leftClosed, sbyte right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="string"/> and <see cref="long"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (string left, long right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="string"/> and <see cref="long"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (string left, bool leftClosed, long right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="string"/> and <see cref="ulong"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (string left, ulong right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="string"/> and <see cref="ulong"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (string left, bool leftClosed, ulong right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);
		/// <summary>
		/// Takes a <see cref="string"/> and <see cref="string"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (string left, string right) arg) 
			=> MathS.Sets.Interval(arg.left, arg.right);

		/// <summary>
		/// Takes a <see cref="string"/> and <see cref="string"/> and returns
		/// a closed interval (so that left and right ends are included)
		/// </summary>
		/// <returns>Interval</returns>
		public static Interval ToEntity(this (string left, bool leftClosed, string right, bool rightClosed) arg) 
			=> new Interval(arg.left, leftClosed, arg.right, rightClosed);

	}
}