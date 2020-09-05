using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace BinaryDad.Extensions
{
    // NOTE: RJP - This extension may be generic enough to be used outside of just enums. Something to think about later.
    public static class EnumExtensions
    {
        /// <summary>
        /// Retrieves a descriptive attribute associated with the enum field using DescriptionAttribute
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum value)
        {
            #region DescriptionAttribute

            var description = value.GetCustomAttribute<DescriptionAttribute>();

            if (description != null)
            {
                return description.Description;
            }

            #endregion

            // default
            return value.ToString();
        }

        /// <summary>
        /// Retrieves a custom attribute of a specified type that is applied to a specified member
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T GetCustomAttribute<T>(this Enum value) where T : Attribute
        {
            var member = value
                .GetType()
                .GetMember(value.ToString())
                .FirstOrDefault();

            return member?.GetCustomAttribute<T>() ?? default;
        }
    }
}