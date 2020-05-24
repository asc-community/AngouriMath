namespace AngouriMath.Core
{
    public partial class ComplexNumber
    {
        public static ComplexNumber operator +(ComplexNumber a, ComplexNumber b)
        {
            if (!Functional.BothAreEqual(a, b, HierarchyLevel.COMPLEX))
                return Number.OpSum(a, b) as ComplexNumber;
            var Re = a.Real + b.Real;
            var Im = a.Imaginary + b.Imaginary;
            return Number.Functional.Downcast(new ComplexNumber(Re, Im)) as ComplexNumber;
        }

        public static ComplexNumber operator -(ComplexNumber a, ComplexNumber b)
        {
            if (!Functional.BothAreEqual(a, b, HierarchyLevel.COMPLEX))
                return Number.OpSub(a, b) as ComplexNumber;
            var Re = a.Real - b.Real;
            var Im = a.Imaginary - a.Imaginary;
            return Number.Functional.Downcast(new ComplexNumber(Re, Im)) as ComplexNumber;
        }

        public static ComplexNumber operator *(ComplexNumber a, ComplexNumber b)
        {
            if (!Functional.BothAreEqual(a, b, HierarchyLevel.COMPLEX))
                return Number.OpMul(a, b) as ComplexNumber;
            var Re = a.Real * b.Real - a.Imaginary * b.Imaginary;
            var Im = a.Real * b.Imaginary + a.Imaginary * b.Real;
            return Number.Functional.Downcast(new ComplexNumber(Re, Im)) as ComplexNumber;
        }

        public static ComplexNumber operator /(ComplexNumber a, ComplexNumber b)
        {
            if (!Functional.BothAreEqual(a, b, HierarchyLevel.COMPLEX))
                return Number.OpDiv(a, b) as ComplexNumber;
            /*
             * (a + ib) / (c + id) = (a + ib) * (1 / (c + id))
             * 1 / (c + id) = (c2 + d2) / (c + id) / (c2 + d2) = (c - id) / (c2 + d2)
             * => ans = (a + ib) * (c - id) / (c2 + d2)
             */
            var conj = b.Conjugate();
            var abs = b.Abs();
            var Re = conj.Real / abs;
            var Im = conj.Imaginary / abs;
            var c = new ComplexNumber(Re, Im);
            return Number.Functional.Downcast(a * c) as ComplexNumber;
        }

        public static bool operator ==(ComplexNumber a, ComplexNumber b)
            => a.Real == b.Real && a.Imaginary == b.Imaginary;

        public static bool operator !=(ComplexNumber a, ComplexNumber b)
            => !(a == b);
    }
}
