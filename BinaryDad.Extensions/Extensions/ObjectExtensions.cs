using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BinaryDad.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Target is the object wanted to be merged to, if source has the value and target does not, copy source value to target
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="source"></param>

        public static void CoalesceValues<T>(this T target, T source)
        {
            var t = typeof(T);

            var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

            foreach (var prop in properties)
            {
                var valueT = prop.GetValue(target, null);
                var valueS = prop.GetValue(source, null);

                if (valueT == null)
                {
                    if (valueS != null)
                    {
                        prop.SetValue(target, valueS, null);
                    }
                }

            }

        }

        #region To

        /// <summary>
        /// Casts or converts a value to type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T To<T>(this object value)
        {
            if (value is T castValue)
            {
                return castValue;
            }

            return (T)value.To(typeof(T));
        }

        /// <summary>
        /// Casts or converts a value to a specified type
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object To(this object value, Type type)
        {
            #region Check if null

            // if value is null and is value type, return "default" value
            // otherwise, return null
            if (value == null || value == DBNull.Value)
            {
                if (type.IsValueType)
                {
                    return Activator.CreateInstance(type);
                }

                return null;
            }

            #endregion

            #region Check if empty string

            if (type.IsValueType && value is string stringValue && stringValue == string.Empty)
            {
                return Activator.CreateInstance(type);
            }

            #endregion

            var convertType = Nullable.GetUnderlyingType(type) ?? type;

            #region Check if Enum

            if (convertType.IsEnum)
            {
                return value.ToString().ToEnum(convertType);
            }

            #endregion

            #region Attempt convert using TypeConverter.ConvertFrom

            var converter = TypeDescriptor.GetConverter(type);

            if (converter.CanConvertFrom(value.GetType()))
            {
                return converter.ConvertFrom(value);
            }

            #endregion

            #region Convert using ChangeType

            return Convert.ChangeType(value, convertType);

            #endregion
        }

        #endregion

        /// <summary>
        /// Serializes an object to a JSON string. Wraps <see cref="JSON.Serialize{T}(T)"/>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Serialize(this object value) => JsonConvert.SerializeObject(value);

        /// <summary>
        /// Returns a dictionary of properties and values for an object or an empty dictionary if value is null or no properties (e.g., value types and string)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IDictionary<string, object> GetPropertyValues(this object value)
        {
            // check for null
            if (value == null)
            {
                return null;
            }

            var valueType = value.GetType();

            // check for value types, and string
            if (valueType.IsValueType || valueType == typeof(string))
            {
                return null;
            }

            return valueType
                .GetProperties()
                .EmptyIfNull()
                .Select(p => new KeyValuePair<string, object>(p.Name, p.GetValue(value, null)))
                .ToDictionary(k => k.Key, k => k.Value);
        }
    }
}