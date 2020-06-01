namespace AngouriMath.Core.Numerix
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
            var Im = a.Imaginary - b.Imaginary;
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
            var bAbs = b.Abs();
            var abs2 = bAbs * bAbs;
            var Re = conj.Real / abs2;
            var Im = conj.Imaginary / abs2;
            var c = new ComplexNumber(Re, Im);
            return Number.Functional.Downcast(a * c) as ComplexNumber;
        }

        public static bool AreEqual(ComplexNumber a, ComplexNumber b)
            => a.Real == b.Real && a.Imaginary == b.Imaginary;


        public static implicit operator ComplexNumber(int value)
            => new ComplexNumber(value);
        public static implicit operator ComplexNumber(double value)
            => new ComplexNumber(value);
        public static implicit operator ComplexNumber(decimal value)
            => new ComplexNumber(value);
        public static implicit operator ComplexNumber(float value)
            => new ComplexNumber(value);
        public static implicit operator ComplexNumber((decimal re, decimal im) value)
            => new ComplexNumber(value.re, value.im);
    }
}
