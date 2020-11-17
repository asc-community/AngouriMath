using System;

namespace AngouriMath.Core
{
    /// <summary>
    /// Provides lazy initialization experience. Is better than the Lazy class as:
    /// 1) Is a struct
    /// 2) Does not affect record's Equals in a bad way Lazy does
    /// <code>
    /// public int MyProperty => myProperty.Value;
    /// public Container int myProperty = new(() => some method);
    /// </code>
    /// </summary>
    /// <typeparam name="T">
    /// The type to store inside
    /// If its state becomes null, it is considered to be incorrect, and <see cref="Exceptions.AngouriBugException"/>
    /// </typeparam>
    public struct Container<T> : IEquatable<Container<T>>
    {
        /// <param name="ctor">Expression to initialize the given property</param>
        public Container(Func<T> ctor)
        {
            this.ctor = ctor;
            value = default;
            initted = false;
        }

        private T? value;
        private bool initted;
        private readonly Func<T> ctor;

        /// <summary>
        /// Use this in your property's getter
        /// </summary>
        public T Value
        {
            get
            {
                if (!initted)
                    lock (ctor)
                    {
                        if (!initted)
                        {
                            initted = true;
                            value = ctor();
                        }
                    }
                return value ?? throw new Exceptions.AngouriBugException("T shouldn't be null!");
            }
        }

        /// <summary>
        /// So that when records get compared, this field will not affect the result
        /// </summary>
        public bool Equals(Container<T> _)
            => true;

        /// <summary>
        /// So that when records get compared, this field will not affect the result
        /// </summary>
        public override bool Equals(object obj)
            => true;

        /// <summary>
        /// So that when records get compared, this field will not affect the result
        /// </summary>
        public override int GetHashCode() => 0;

        /// <summary>
        /// Convenient to use instead of .Value
        /// </summary>
        public static implicit operator T(Container<T> container) => container.Value;

        /// <summary>
        /// Convenient to use instead of new()
        /// </summary>
        public static implicit operator Container<T>(Func<T> action) => new(action);
    }
}
