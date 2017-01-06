﻿namespace LibiadaWeb.Attributes
{
    using System;

    /// <summary>
    /// The nature attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class NatureAttribute : System.Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NatureAttribute"/> class.
        /// </summary>
        /// <param name="value">
        /// The Nature value.
        /// </param>
        public NatureAttribute(Nature value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets nature attribute value.
        /// </summary>
        public Nature Value { get; private set; }
    }
}