﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using System.Windows;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will convert a boolean value to a <see cref="Visibility"/> value, where false translates to <see cref="Visibility.Collapsed"/>.
    /// <see cref="ConvertBack"/> is supported.
    /// </summary>
    /// <remarks>If the boolean value <c>false</c> is passed as converter parameter, the visibility is inverted.</remarks>
    /// <seealso cref="VisibleOrHidden"/>
    public class VisibleOrCollapsed : ValueConverterBase<VisibleOrCollapsed>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = System.Convert.ToBoolean(value);
            if (parameter is bool && (bool)parameter == false)
            {
                result = !result;
            }
            return result ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <inheritdoc/>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility = (Visibility)value;
            if (parameter is bool && (bool)parameter == false)
            {
                return visibility != Visibility.Visible;
            }
            return visibility == Visibility.Visible;
        }
    }
}
