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
            // Can't have other name
            TreeAnalyzer.AssertTree(Name == "tensort", "Tensors must have Name=tensort");
            // Tensor can't be scalar
            TreeAnalyzer.AssertTree(Dimensions > 0, "Dimensions can't be equal to 0");

            // All elements are not null
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
            // Number has no children
            TreeAnalyzer.AssertTree(Children.Count == 0, "A number cannot have children");
            // Is null?
            TreeAnalyzer.AssertTree(Token.IsNumber(Name), "Number's name is not number");
        }
    }
    public partial class VariableEntity
    {
        internal override void Check()
        {
            // Var has no children
            TreeAnalyzer.AssertTree(Children.Count == 0, "A variable cannot have children");
            // Correct name for var
            TreeAnalyzer.AssertTree(Token.IsVariable(Name), "Weird sequence in variable '" + Name + "'");
            // Reserved word (e. g. "sumf") can't be a var's name
            TreeAnalyzer.AssertTree(!Const.IsReservedName(Name), "`" + Name + "` is a reserved word");
        }
    }
    public partial class FunctionEntity
    {
        internal override void Check()
        {
            // Number of children fits required number of args
            TreeAnalyzer.AssertTree(Children.Count == SyntaxInfo.GetFuncArg(Name), "Wrong number of children");
            // Checks whether the function exist
            TreeAnalyzer.AssertTree(Const.IsReservedName(Name), "Unknown function");
        }
    }
    public partial class OperatorEntity
    {
        internal override void Check()
        {
            // Only binary operators are available so far
            TreeAnalyzer.AssertTree(Children.Count == 2, "Wrong number of children");
            // If operator exists
            TreeAnalyzer.AssertTree(Const.IsReservedName(Name), "Unknown operator");
            // Detect division by 0
            TreeAnalyzer.AssertTree(Name != "divf" || Children[1] != 0, "Division by zero");
        }
    }
}