﻿/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using Hl7.Fhir.Model;
using System;
using System.ComponentModel.DataAnnotations;

#nullable enable

namespace Hl7.Fhir.Validation
{
    /// <summary>
    /// Validates a date value against the FHIR rules for date.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class DatePatternAttribute : ValidationAttribute
    {
        /// <inheritdoc/>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) =>
            value switch
            {
                null => ValidationResult.Success,
                string s when Date.IsValidValue(s) => ValidationResult.Success,
                string s => DotNetAttributeValidation.BuildResult(validationContext, "'{0}' is not a correct value for a Date.", s),
                _ => throw new ArgumentException($"{nameof(DatePatternAttribute)} attributes can only be applied to string properties.")
            };
    }
}
#nullable restore