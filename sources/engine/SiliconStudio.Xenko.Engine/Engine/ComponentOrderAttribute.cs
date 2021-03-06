﻿using System;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Defines the order of a component type. This information is used to determine the most "important" component of a collection
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentOrderAttribute : EntityComponentAttributeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentOrderAttribute"/> class.
        /// </summary>
        /// <param name="order">The order of the associated component.</param>
        public ComponentOrderAttribute(int order)
        {
            Order = order;
        }

        /// <summary>
        /// Gets the order of the associated component.
        /// </summary>
        public int Order { get; }
    }
}