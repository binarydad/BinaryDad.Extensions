using System;
using System.Collections.Generic;

namespace BinaryDad.Extensions
{
    /// <summary>
    /// Allows for additional metadata to be applied to Enum values
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class EnumAliasAttribute : Attribute
    {
        public IEnumerable<string> EnumAliasNames { get; private set; }

        public EnumAliasAttribute(params string[] ids)
        {
            EnumAliasNames = ids;
        }
    }
}