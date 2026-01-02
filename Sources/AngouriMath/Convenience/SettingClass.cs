//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

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
        internal Setting(T defaultValue)
        {
            Set(defaultValue);
            Default = defaultValue; 
        }

        /// <summary>
        /// Use it if you don't need to unset it
        /// Should be NOT used for industrial projects
        /// (only for some manual calculations)
        /// </summary>
        /// <param name="value">The new value of the setting</param>
        [Obsolete("Method `Global` is not threadsafe and inconvenient. Use method `Set` instead.")]
        public void Global(T value)
            => Set(value);

        /// <summary>
        /// Use it in case if you need to cancel any adjustments of the setting
        /// and rollback to the one predefined in <see cref="MathS"/>.
        /// It does not roll back to the previous value. Instead, it sets the 
        /// value which is defined right after <see cref="MathS"/> is
        /// initialized. If you need recoverable settings (aka local),
        /// use method <see cref="As"/>.
        /// </summary>
        [Obsolete("Method `RollBackToDefault` is not threadsafe and inconvenient. Use method `Set` instead.")]
        public void RollBackToDefault()
            => Global(Default);

        /// <summary>
        /// Sets the new value for the setting
        /// </summary>
        /// <param name="value">
        /// New value of a setting
        /// </param>
        /// <returns>
        /// An <see cref="IDisposable"/> struct. Make sure to use operator `using` before, so that it auto-disposes
        /// once the method is over.
        /// Example:
        /// <code>
        /// using var _ = MathS.Settings.SomeSetting.Set(4);
        /// // do something, once the method is ended, the setting is automatically returned to the initial statement
        /// </code>
        /// </returns>
        public AutoBackRollableTemporarySettingUnit Set(T value)
        {
            var guid = Guid.NewGuid();
            values.Push(guid, value);
            return new AutoBackRollableTemporarySettingUnit(this, guid);
        }

        /// <summary>
        /// For example,
        /// <code>
        /// using var _ = MathS.Settings.Precision.Set(100);
        /// // your code here
        /// </code>
        /// </summary>
        /// <param name="value">New value that will be automatically reverted after action is done</param>
        /// <param name="action">What should be done under this setting</param>
        public void As(T value, Action action)
        {
            using var _ = Set(value);
            action();
        }

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
            using var _ = Set(value);
            return action();
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
        public override string? ToString() => Value.ToString();

        private readonly KeyStack<Guid, T> values = new();

        /// <summary>
        /// The current value of the setting
        /// </summary>
        public T Value => values.Peek();

        /// <summary>
        /// The default value of the setting
        /// </summary>
        public T Default { get; }

        /// <summary>
        /// This tiny struct is needed to be under `using` operator, so that your settings
        /// are automatically rolled back on the end of your method
        /// <code>
        /// using var _ = MathS.Settings.SomeSetting.Set(4);
        /// // do something, once the method is ended, the setting is automatically returned to the initial statement
        /// </code>
        /// </summary>
        public struct AutoBackRollableTemporarySettingUnit : IDisposable
        {
            private readonly Setting<T> setting;
            private bool disposed;
            private readonly Guid guid;
            internal AutoBackRollableTemporarySettingUnit(Setting<T> settingToRollBack, Guid guid)
                => (this.setting, disposed, this.guid) = (settingToRollBack, false, guid);

            /// <inheritdoc/>
            public void Dispose()
            {
                if (disposed)
                    return;
                setting.values.Remove(guid);
                disposed = true;
            }
        }
    }


    internal sealed class KeyStack<TKey, TValue>
    {
        private readonly List<(TKey key, TValue value)> list = new();

        internal TValue Peek() => list[^1].value;

        internal bool Remove(TKey key)
        {
            int index = -1;
            for (int i = list.Count - 1; i >= 0; i--)
                if (key is not null && key.Equals(list[i].key))
                {
                    index = i;
                    break;
                }
            if (index == -1)
                return false;
            list.RemoveAt(index);
            return true;
        }

        internal void Push(TKey key, TValue value)
            => list.Add((key, value));

        internal void Pop()
            => list.RemoveAt(list.Count - 1);
    }
}
