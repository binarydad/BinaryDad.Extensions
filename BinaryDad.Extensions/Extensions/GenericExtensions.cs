using System;
using System.Collections.Generic;
using System.Linq;

namespace BinaryDad.Extensions
{
    public static class GenericExtensions
    {
        /// <summary>
        /// Returns true if the value is between the lower and upper range. This is inclusive in its comparison.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        public static bool Between<T>(this T value, T lower, T upper) where T : IComparable
        {
            return Comparer<T>.Default.Compare(value, lower) >= 0
                && Comparer<T>.Default.Compare(value, upper) <= 0;
        }

        #region IfNotNull

        /// <summary>
        /// Executes an inline statement if the source value is not null
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TResult IfNotNull<TSource, TResult>(this TSource source, Func<TSource, TResult> value, TResult defaultValue)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (source != null)
            {
                return value(source);
            }

            return defaultValue == null ? default : defaultValue;
        }

        /// <summary>
        /// Executes an inline statement if the source value is not null
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TResult IfNotNull<TSource, TResult>(this TSource source, Func<TSource, TResult> value) => source.IfNotNull(value, default(TResult));

        /// <summary>
        /// Executes an inline statement if the source value is not null
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TResult IfNotNull<TSource, TResult>(this TSource source, TResult value, TResult defaultValue) => source.IfNotNull(o => value, defaultValue);

        /// <summary>
        /// Executes an inline statement if the source value is not null
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TResult IfNotNull<TSource, TResult>(this TSource source, TResult value) => source.IfNotNull(o => value, default);

        #endregion

        #region If

        /// <summary>
        /// Executes an inline conditional statement, allowing for an evaluation for true or false
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="condition"></param>
        /// <param name="trueResult"></param>
        /// <param name="falseResult"></param>
        /// <returns></returns>
        public static TResult If<TSource, TResult>(this TSource source, Func<TSource, bool> condition, Func<TSource, TResult> trueResult, Func<TSource, TResult> falseResult = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            return source.If(condition(source), trueResult, falseResult);
        }

        /// <summary>
        /// Executes an inline conditional statement, allowing for an evaluation for true or false
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="condition"></param>
        /// <param name="trueResult"></param>
        /// <param name="falseResult"></param>
        /// <returns></returns>
        public static TResult If<TSource, TResult>(this TSource source, bool condition, Func<TSource, TResult> trueResult, Func<TSource, TResult> falseResult = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (trueResult == null)
            {
                throw new ArgumentNullException(nameof(trueResult));
            }

            if (condition)
            {
                return trueResult(source);
            }

            if (falseResult != null)
            {
                return falseResult(source);
            }

            return default;
        }

        #endregion

        #region In

        /// <summary>
        /// Returns whether a value is in a particular sequence of items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="items">The collection of items the value may belong to</param>
        /// <returns></returns>
        public static bool In<T>(this T value, params T[] items) where T : IComparable => items.AnyAndNotNull(t => t.Equals(value));

        /// <summary>
        /// Returns whether a value is in a particular sequence of items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="items">The collection of items the value may belong to</param>
        /// <returns></returns>
        public static bool In<T>(this T value, IEnumerable<T> items) where T : IComparable => In(value, items.ToArray());

        #endregion

        #region With

        /// <summary>
        /// Performs an action on an object and returns that instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static T With<T>(this T value, Action<T> action)
        {
            if (action == null)
            {
                // could throw null arg exception but "action" is the whole point of the method
                throw new InvalidOperationException("No action specified in With<T> or is null. Please omit use of this method.");
            }

            action(value);

            return value;
        }

        #endregion
    }
}