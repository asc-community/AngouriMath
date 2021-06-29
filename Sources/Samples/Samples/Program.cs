using AngouriMath;

using HonkSharp.Fluency;
using System;
using System.Collections.Generic;
using System.Collections;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;
using HonkSharp.Laziness;

Console.WriteLine(Draw("sqrt((((1/a + 1)/b + 1)/c + 1)/d)"));








static Figure Draw(Entity expr)
    => expr switch
    {
        Number or Variable => new BlockFigure(expr.ToString()),
        Divf(var a, var b) => new RationalFigure(Draw(a), Draw(b)),
        Sumf(var a, var b) => new BinaryOpFigure(Draw(a), Draw(b), '+'),
        Powf(var a, Rational(Integer(1), Integer(2))) => new RadicalFigure(Draw(a), null),
        _ => throw new()
    };




public abstract record Figure
{
    public int Width => Table.GetLength(1);
    public int Height => Table.GetLength(0);

    protected abstract char[,] GenerateTable();
    internal protected char[,] Table => table.GetValue(@this => @this.GenerateTable(), this);
    private readonly LazyPropertyA<char[,]> table;

    public override string ToString()
        => "\n"
            .Join(
                (..(Height - 1)).Select(h =>
                    (..(Width - 1)).Select(w => Table[h, w]).AsString()
                )
            );
}

public sealed record BlockFigure(string Source) : Figure
{
    protected override char[,] GenerateTable()
    {
        var res = new char[1, Source.Length];
        foreach (var (index, ch) in Source.Enumerate())
            res[0, index] = ch;
        return res;
    }
}

public sealed record RationalFigure(Figure Numerator, Figure Denominator) : Figure
{
    protected override char[,] GenerateTable()
    {
        var res = new char[Numerator.Height + 1 + Denominator.Height, Math.Max(Numerator.Width, Denominator.Width) + 2].WithSpaces();

        Numerator.Table.CopyWidthAlignedCenterTo(res, 0);
        Denominator.Table.CopyWidthAlignedCenterTo(res, Numerator.Height + 1);

        foreach (var x in 0..(res.GetLength(1) - 1))
            res[Numerator.Height, x] = '-';

        return res;
    }
}

public sealed record BinaryOpFigure(Figure Left, Figure Right, char Operator) : Figure
{
    protected override char[,] GenerateTable()
    {
        var res = new char[Math.Max(Left.Height, Right.Height), Left.Width + 3 + Right.Width].WithSpaces();

        Left.Table.CopyHeightAlignedCenterTo(res, 0);
        Right.Table.CopyHeightAlignedCenterTo(res, Left.Width + 3);

        res[res.GetLength(0) / 2, Left.Width + 1] = Operator;

        return res;
    }
}


public sealed record RadicalFigure(Figure Expression, Figure? Power) : Figure
{
    protected override char[,] GenerateTable()
    {
//      
//           /----|      <-- this is right glyph (the '|' thing)
//          / HHH
//      \  /  HHH
//       \/   HHH
//      ^
// this is left glyph
        const double LeftGlyphShare = 0.3;
        const double RightGlyphShare = 0.2;

        var leftGlyphSize = (int)(LeftGlyphShare * Expression.Height) + 1;
        var rightGlyphSize = (int)(RightGlyphShare * Expression.Height) + 1;

        var resWidth = leftGlyphSize + Expression.Height + Expression.Width + 1;
        var resHeight = Expression.Height + 1;

        var res = new char[resHeight, resWidth].WithSpaces();
        Expression.Table.CopyTo(res, 1, leftGlyphSize + Expression.Height);

        foreach (var i in 0..(leftGlyphSize - 1))
            res[resHeight - i - 1, i] = '\\';

        foreach (var (y, x) in (0..(resHeight - 1)).AsRange().Enumerate())
            res[resHeight - y - 1, x + 1] = '/';
            
        foreach (var i in resHeight..(resWidth - resHeight + 2))
            res[0, i + 1] = '-';

        foreach (var i in ..(rightGlyphSize - 1))
            res[i, resWidth - 1] = '|';

        return res;
    }
}


public static class ArrayExtensions
{
    public static void CopyWidthAlignedCenterTo(this char[,] src, char[,] dst, int heightOffset)
        => src.CopyTo(dst, heightOffset, (dst.GetLength(1) - src.GetLength(1)) / 2);

    public static void CopyHeightAlignedCenterTo(this char[,] src, char[,] dst, int widthOffset)
        => src.CopyTo(dst, (dst.GetLength(0) - src.GetLength(0)) / 2, widthOffset);

    public static void CopyTo(this char[,] src, char[,] dst, int heightOffset, int widthOffset)
    {
        foreach (var x in 0..(src.GetLength(1) - 1))
            foreach (var y in 0..(src.GetLength(0) - 1))
                dst[y + heightOffset, x + widthOffset] = src[y, x];
    }

    public static char[,] WithSpaces(this char[,] chars)
    {
        foreach (var x in 0..(chars.GetLength(0) - 1))
            foreach (var y in 0..(chars.GetLength(1) - 1))
                chars[x, y] = ' ';
        return chars;
    }
}

