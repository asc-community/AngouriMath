
namespace AngouriMath
{
    partial record Entity
    {
        public partial record Variable
        {
            /// <inheritdoc/>
            public override string Stringize() => Name;
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }
    }
}