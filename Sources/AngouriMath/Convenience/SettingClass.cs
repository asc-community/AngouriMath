/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
using System;

namespace AngouriMath.Convenience
{
    /// <summary>
    /// This class for configuring some internal mechanisms from outside
    /// </summary>
    /// <typeparam name="T">
    /// Those configurations can be of different types
    /// </typeparam>
    public sealed class Setting<T> where T : notnull
    {
        internal Setting(T defaultValue) { Value = defaultValue; Default = defaultValue; }

        /// <summary>
        /// For example,
        /// <code>
        /// MathS.Settings.Precision.As(100, () => { /* some code considering precision = 100 */ });
        /// </code>
        /// </summary>
        /// <param name="value">New value that will be automatically reverted after action is done</param>
        /// <param name="action">What should be done under this setting</param>
        public void As(T value, Action action)
        {
            var previousValue = Value;
            Value = value;
            try
            {
                action();
            }
            finally
            {
                Value = previousValue;
            }
        }

        /// <summary>
        /// Use it if you don't need to unset it
        /// Should be NOT used for industrial projects
        /// (only for some manual calculations)
        /// </summary>
        /// <param name="value">The new value of the setting</param>
        public void Global(T value)
            => Value = value;

        /// <summary>
        /// Use it in case if you need to cancel any adjustments of the setting
        /// and rollback to the one predefined in <see cref="MathS"/>.
        /// It does not roll back to the previous value. Instead, it sets the 
        /// value which is defined right after <see cref="MathS"/> is
        /// initialized. If you need recoverable settings (aka local),
        /// use method <see cref="As"/>.
        /// </summary>
        public void RollBackToDefault()
            => Global(Default);

        /// <summary>
        /// For example,
        /// <code>
        /// var res = MathS.Settings.Precision.As(100, () => { /* some code considering precision = 100 */ return 4; });
        /// </code>
        /// </summary>
        /// <param name="value">New value that will be automatically reverted after action is done</param>
        /// <param name="action">What should be done under this setting</param>
        public TReturnType As<TReturnType>(T value, Func<TReturnType> action)
        {
            var previousValue = Value;
            Value = value;
            try
            {
                return action();
            }
            finally
            {
                Value = previousValue;
            }
        }

        /// <summary>
        /// An implicit operator so that one does not have to call <see cref="Value"/>
        /// </summary>
        /// <param name="s">The setting</param>
        public static implicit operator T(Setting<T> s) => s.Value;

        /// <summary>
        /// An implicit operator so that one does not have to call the ctor
        /// </summary>
        /// <param name="a">The value</param>
        public static implicit operator Setting<T>(T a) => new(a);

        /// <summary>
        /// Overriden ToString so that one could see the value of the setting
        /// (if overriden)
        /// </summary>
        public override string ToString() => Value.ToString();

        /// <summary>
        /// The current value of the setting
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// The default value of the setting
        /// </summary>
        public T Default { get; }
    }
}
