using System;
using System.Reflection;

namespace BinaryDad.Extensions
{
    [Serializable]
    public class DataPropertyConversionException : Exception
    {
        public object Item { get; private set; }
        public object Value { get; private set; }
        public PropertyInfo Property { get; private set; }

        /// <summary>
        /// Represents an exception that occurs upon setting the value of a property on an object through type conversion
        /// </summary>
        /// <param name="item">The parent object containing the property</param>
        /// <param name="property">The property info instance of the property</param>
        /// <param name="value">The value being set</param>
        /// <param name="ex">The original exception (assigned as inner)</param>
        public DataPropertyConversionException(object item, PropertyInfo property, object value, Exception ex)
            : base($"Unable to assign value {value ?? "null"} ({value?.GetType().Name}) to property {item?.GetType().Name}.{property?.Name} ({property?.PropertyType.Name})", ex)
        {
            Value = value;
            Item = item;
            Property = property;
        }
    }
}