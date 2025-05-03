using System;
using System.ComponentModel.DataAnnotations;

namespace __Cross_cutting_Concerns.SiteAttributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class TermsMustBeTrueAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            return value is bool boolValue && boolValue;
        }
    }
}
