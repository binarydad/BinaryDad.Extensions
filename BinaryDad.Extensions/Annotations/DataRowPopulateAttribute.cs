using System;

namespace BinaryDad.Extensions
{
    /// <summary>
    /// Allows for a complex property to be populated via ToType().
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class DataRowPopulateAttribute : Attribute { }
}