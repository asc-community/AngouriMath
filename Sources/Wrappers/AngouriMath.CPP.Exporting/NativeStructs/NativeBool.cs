namespace AngouriMath.CPP.Exporting
{
    partial class Exports
    {
        public struct NativeBool
        {
            private int value;
            public NativeBool(bool value)
                => this.value = value ? 1 : 0;
            public static NativeBool True => new(true);
            public static NativeBool False => new(false);
            public static implicit operator NativeBool(bool value)
                => new(value);
        }
    }
}
