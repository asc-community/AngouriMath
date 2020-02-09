using AngouriMath.Core.FromString;
using AngouriMath.Core.TreeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core.TreeAnalysis
{
    public class TreeException : MathSException
    {
        public TreeException(string message) : base(message)
        {
        }
    }

    internal static partial class TreeAnalyzer
    {
        /// <summary>
        /// Checks if a tree contains some bad entites,
        /// for example a variable "sumf".
        /// If something wrong, it throws exception.
        /// Recommended to wrap it into try catch
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        public static void CheckTree(Entity tree)
        {
            tree.Check();
            foreach (var child in tree.Children)
                CheckTree(child);
        }

        internal static void AssertTree(bool condition, string message)
        {
            if (!condition)
                throw new TreeException(message);
        }
    }
}

namespace AngouriMath.Core
{
    public partial class Tensor
    {
        internal override void Check()
        {
            TreeAnalyzer.AssertTree(Name == "tensort", "Tensors must have Name=tensort");
            TreeAnalyzer.AssertTree(Dimensions > 0, "Dimensions can't be equal to 0");
            for(int i = 0; i < Data.Length; i++)
                TreeAnalyzer.AssertTree(Data[i] != null, "One tensor's element is null");
        }
    }
}

namespace AngouriMath
{
    public partial class NumberEntity
    {
        internal override void Check()
        {
            TreeAnalyzer.AssertTree(Token.IsNumber(Name), "Number's name is number");
        }
    }
    public partial class VariableEntity
    {
        internal override void Check()
        {
            TreeAnalyzer.AssertTree(Token.IsVariable(Name), "Weird sequence in variable '" + Name + "'");
            TreeAnalyzer.AssertTree(!Const.IsReservedName(Name), "`" + Name + "` is a reserved word");
        }
    }
    public partial class FunctionEntity
    {
        internal override void Check()
        {
            TreeAnalyzer.AssertTree(Const.IsReservedName(Name), "Unknown function");
        }
    }
    public partial class OperatorEntity
    {
        internal override void Check()
        {
            TreeAnalyzer.AssertTree(Const.IsReservedName(Name), "Unknown operator");
        }
    }
}