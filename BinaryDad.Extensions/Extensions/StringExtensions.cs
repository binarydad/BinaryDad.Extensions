using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace BinaryDad.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Searches a list of strings with a delimiter for a match on specified value.
        /// </summary>
        /// <param name="s">The the delimited string to search</param>
        /// <param name="value">The value to match against delimited values</param>
        /// <param name="separator">The string separator</param>
        /// <param name="isCaseSensitive">True if a match is found, otherwise false</param>
        /// <returns></returns>
        public static bool ContainsMatch(this string s, string value, char separator = ';', bool isCaseSensitive = false)
        {
            if (s == null)
            {
                return false;
            }

            if (isCaseSensitive)
            {
                return s.SafeSplit(separator).Any(item => value.Trim().Contains(item.Trim()));
            }

            return s.SafeSplit(separator).Any(item => value.ToLower().Trim().Contains(item.ToLower().Trim()));
        }

        public static bool Contains(this string source, string value, StringComparison comparisonType) => source.IndexOf(value, comparisonType) >= 0;

        /// <summary>
        /// Retrieves the last length of characters
        /// </summary>
        /// <param name="source"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetLast(this string source, int length)
        {
            return length >= source.Length
                ? source
                : source.Substring(source.Length - length);
        }

        /// <summary>
        /// Parse a datetime string using DateTime.TryParse, returning null if it cannot be parsed.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime? ParseDateTime(this string value)
        {
            if (DateTime.TryParse(value, out var date))
            {
                return date;
            }

            return null;
        }

        /// <summary>
        /// Validate date string
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static bool IsDate(this string date)
        {
            try
            {
                var dt = DateTime.Parse(date);
                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Checks whether the string is an email.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsEmail(this string value)
        {
            if (value.IsNullOrWhiteSpace())
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(value,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        /// <summary>
        /// Wrapper for String.IsNullOrEmpty(value)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);

        /// <summary>
        /// Wrapper for String.IsNullOrWhiteSpace(value)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);

        /// <summary>
        /// Allows for a replacement string if the value is null
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ValueIfNull(this string obj, string value)
        {
            if (obj == null)
            {
                return value;
            }

            return obj;
        }

        /// <summary>
        /// Returns true if the string is not null and has a value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNotEmpty(this string value) => !value.IsNullOrWhiteSpace();

        /// <summary>
        /// Returns whether a value is in a particular sequence of strings
        /// </summary>
        /// <param name="value"></param>
        /// <param name="comparisonType"></param>
        /// <param name="items">The collection of items the value may belong to</param>
        /// <returns></returns>
        public static bool In(this string value, StringComparison comparisonType, params string[] items) => items.AnyAndNotNull(t => t.Equals(value, comparisonType));

        /// <summary>
        /// Checks whether the entire string is numeric. This purely evaluates numbers, not currency (decimals/thousand-separators).
        /// </summary>
        /// <param name="value"></param>
        /// <returns>True, if entire string is numeric; otherwise, false.</returns>
        public static bool IsNumeric(this string value) => decimal.TryParse(value, out _);

        /// <summary>
        /// Returns whether a string matches a regular expression pattern
        /// </summary>
        /// <param name="value">The string to check</param>
        /// <param name="regex">The regex pattern</param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        public static bool Like(this string value, string regex, bool ignoreCase = true) => Regex.IsMatch(value, regex, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);

        public static string[] Split(this string text, params string[] separator) => text.Split(separator, StringSplitOptions.None);

        public static string[] SafeSplit(this string s, char separator = ';')
        {
            if (s == null)
            {
                return new string[] { };
            }

            return (from sr in s.Split(separator)
                    let tr = sr.Trim()
                    where !string.IsNullOrEmpty(tr)
                    select tr).ToArray();
        }

        public static string NullIfWhiteSpace(this string text) => text.If(s => s.IsNullOrWhiteSpace(), s => null, s => s);

        #region ToEnum

        /// <summary>
        /// Casts or converts a string value to type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ToEnum<T>(this string value) where T : Enum
        {
            var type = typeof(T);
            var parsedEnum = value.ToEnum(type);

            if (parsedEnum == null)
            {
                throw new ApplicationException($"Could not cast value {value} to type {type.FullName}");
            }

            return (T)parsedEnum;
        }

        /// <summary>
        /// Casts or converts a string value to a type 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ToEnum(this string value, Type type)
        {
            #region Constraints

            if (value == null)
            {
                return null;
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!type.IsEnum)
            {
                throw new ArgumentException("Destination conversion type must be an Enum", nameof(type));
            }

            #endregion

            #region Helpers

            bool isDefined(string s)
            {
                return int.TryParse(s, out var num)
                    ? Enum.IsDefined(type, num)
                    : Enum.IsDefined(type, s);
            }

            #endregion

            // Try to parse normally (works for int values and literal strings matching enum field name)
            // Calling the isDefined wrapper handles checks for ints or string values, as we can't use TryParse
            if (isDefined(value))
            {
                return Enum.Parse(type, value);
            }
            else // If no match, dig up enum alias attributes for a match
            {
                return type
                    .GetFields()
                    .FirstOrDefault(field => field.GetEnumAliases().Any(a => a.Equals(value, StringComparison.OrdinalIgnoreCase)))
                    ?.GetValue(null);
            }
        }

        #endregion

        public static string NullableTrim(this string value) => value?.Trim();

        public static string ToPhoneNumber(this string value) => Regex.Replace(value, @"^\d?(\d{3})(\d{3})(\d{4})$", "($1) $2-$3");

        public static string MaskSsn(this string value)
        {
            if (!value.IsNullOrWhiteSpace())
            {
                var match = Regex.Match(value, @"\d{3}.?\d{2}.?(\d{4})");

                if (match.Success)
                {
                    return $"XXX-XX-{match.Groups[1].Value}";
                }
            }

            return null;
        }

        /// <summary>
        /// Truncates a string to a maximum length value
        /// </summary>
        /// <param name="text"></param>
        /// <param name="limit"></param>
        /// <param name="useWholeWords">Indicates whether to include whole words when truncating (i.e., whether to truncate in the middle of a word)</param>
        /// <param name="useEllipsis">Indicates whether to include an ellipsis (...) if truncating occurs.</param>
        /// <returns></returns>
        public static string Truncate(this string text, int limit, bool useWholeWords = false, bool useEllipsis = false)
        {
            var output = text;

            if (!text.IsNullOrWhiteSpace() && output.Length > limit && limit > 0)
            {
                output = output.Substring(0, limit);

                // include whole words when truncating (don't truncate in middle of a word)
                if (useWholeWords && text.Substring(output.Length, 1) != " ")
                {
                    var lastSpaceIndex = output.LastIndexOf(" ", StringComparison.OrdinalIgnoreCase);

                    if (lastSpaceIndex != -1)
                    {
                        output = output.Substring(0, lastSpaceIndex);
                    }
                }

                if (useEllipsis)
                {
                    output += "...";
                }
            }

            return output;
        }

        /// <summary>
        /// Deserializes text to a type. Wraps <see cref="JsonConvert.DeserializeObject{T}(string)"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text"></param>
        /// <returns></returns>
        public static T Deserialize<T>(this string text) => JsonConvert.DeserializeObject<T>(text);

        /// <summary>
        /// Deserializes text to a type. Wraps <see cref="JsonConvert.DeserializeObject(string, Type)"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object Deserialize(this string text, Type type) => JsonConvert.DeserializeObject(text, type);

        /// <summary>
        /// Encodes a plaintext string into Base-64
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string Base64Encode(this string plainText)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(plainText);

            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Decodes a Base-64 encoded string into plaintext
        /// </summary>
        /// <param name="encodedText"></param>
        /// <returns></returns>
        public static string Base64Decode(this string encodedText)
        {
            var bytes = Convert.FromBase64String(encodedText);

            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
}