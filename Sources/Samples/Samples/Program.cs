using AngouriMath;

using HonkSharp.Fluency;
using System;
using System.Collections.Generic;
using System.Collections;
using static AngouriMath.Entity;


Console.WriteLine(Draw("213/43 + 3/4"));








static Figure Draw(Entity expr)
    => expr switch
    {
        Number n => new BlockFigure(n.ToString()),
        Divf(var a, var b) => new RationalFigure(Draw(a), Draw(b)),
        Sumf(var a, var b) => new BinaryOpFigure(Draw(a), Draw(b), '+'),
        _ => throw new()
    };




public abstract class Figure
{
    // should be protected
    public readonly char[,] table;

    public int Width => table.GetLength(1);
    public int Height => table.GetLength(0);

    private protected Figure(char[,] table)
        => this.table = table;

    public override string ToString()
        => "\n"
            .Join(
                (..(Height - 1)).Select(h =>
                    (..(Width - 1)).Select(w => table[h, w]).AsString()
                )
            );
}

public sealed class BlockFigure : Figure
{
    public BlockFigure(string s) : base(GenerateTable(s)) { }

    private static char[,] GenerateTable(string source)
    {
        var res = new char[1, source.Length];
        foreach (var (index, ch) in source.Enumerate())
            res[0, index] = ch;
        return res;
    }
}

public sealed class RationalFigure : Figure
{
    public RationalFigure(Figure num, Figure den) : base(GenerateTable(num, den)) { }

    private static char[,] GenerateTable(Figure num, Figure den)
    {
        var res = new char[num.Height + 1 + den.Height, Math.Max(num.Width, den.Width) + 2].WithSpaces();

        num.table.CopyWidthAlignedCenterTo(res, 0);
        den.table.CopyWidthAlignedCenterTo(res, num.Height + 1);

        foreach (var x in 0..(res.GetLength(1) - 1))
            res[num.Height, x] = '-';

        return res;
    }
}

public sealed class BinaryOpFigure : Figure
{
    public BinaryOpFigure(Figure left, Figure right, char op) : base(GenerateTable(left, right, op)) { }

    private static char[,] GenerateTable(Figure left, Figure right, char op)
    {
        var res = new char[Math.Max(left.Height, right.Height), left.Width + 3 + right.Width].WithSpaces();

        left.table.CopyHeightAlignedCenterTo(res, 0);
        right.table.CopyHeightAlignedCenterTo(res, left.Width + 3);

        res[res.GetLength(0) / 2, left.Width + 1] = op;

        return res;
    }
}



public static class ArrayExtensions
{
    public static void CopyWidthAlignedCenterTo(this char[,] src, char[,] dst, int heightOffset)
    {
        var widthOffset = (dst.GetLength(1) - src.GetLength(1)) / 2;
        foreach (var x in 0..(src.GetLength(1) - 1))
            foreach (var y in 0..(src.GetLength(0) - 1))
                dst[y + heightOffset, x + widthOffset] = src[y, x];
    }

    public static void CopyHeightAlignedCenterTo(this char[,] src, char[,] dst, int widthOffset)
    {
        var heightOffset = (dst.GetLength(0) - src.GetLength(0)) / 2;
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

