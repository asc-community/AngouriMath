using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core.Sys.Sets
{
    /// <summary>
    /// Class defines true mathematical sets
    /// It can be empty, it can contain numbers, it can contain intervals etc.
    /// It supports intersection (with & operator), union (with | operator), subtracting (with - operator)
    /// </summary>
    public class Set
    {
        internal void AddPiece(Piece piece)
        {

        }

        public Set()
        {
            
        }

        /// <summary>
        /// Adds a set of numbers to the set
        /// </summary>
        /// <param name="elements"></param>
        public void AddElements(params Entity[] elements)
        {
            foreach(var el in elements)
                AddPiece(Piece.Element(el));
        }


        /// <summary>
        /// Creates a closed interval
        /// Modify it with setters, for example,
        /// myset.AddInterval(3, 4).SetLeftClosed(true).SetRightClosed(false);
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public IntervalPiece AddInterval(Number a, Number b)
        {
            var piece = Piece.Interval(a, b);
            AddPiece(piece);
            return piece; // so we could modify it later
        }
    }
    
}
