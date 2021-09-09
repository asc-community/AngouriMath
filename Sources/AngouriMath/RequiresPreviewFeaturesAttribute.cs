//
// Copyright (c) 2019-2021 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace System.Runtime.Versioning
{
    /// Yes.
    [AttributeUsage(AttributeTargets.Assembly |
                AttributeTargets.Module |
                AttributeTargets.Class |
                AttributeTargets.Interface |
                AttributeTargets.Delegate |
                AttributeTargets.Struct |
                AttributeTargets.Enum |
                AttributeTargets.Constructor |
                AttributeTargets.Method |
                AttributeTargets.Property |
                AttributeTargets.Field |
                AttributeTargets.Event, Inherited = false)]
    public sealed class RequiresPreviewFeaturesAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <seealso cref="RequiresPreviewFeaturesAttribute"/> class.
        /// </summary>
        public RequiresPreviewFeaturesAttribute() { }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="RequiresPreviewFeaturesAttribute"/> class with the specified message.
        /// </summary>
        /// <param name="message">An optional message associated with this attribute instance.</param>
        public RequiresPreviewFeaturesAttribute(string? message)
        {
            Message = message;
        }

        /// <summary>
        /// Returns the optional message associated with this attribute instance.
        /// </summary>
        public string? Message { get; }

        /// <summary>
        /// Returns the optional URL associated with this attribute instance.
        /// </summary>
        public string? Url { get; set; }
    }
}