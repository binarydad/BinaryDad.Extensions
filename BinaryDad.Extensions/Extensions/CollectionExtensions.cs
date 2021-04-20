using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BinaryDad.Extensions
{
    public static class CollectionExtensions
    {
        public static void AddReplace<TItem, TProperty>(this ICollection<TItem> items, TItem item, Func<TItem, TProperty> matchingProperty) where TItem : class where TProperty : IComparable
        {
            var existing = items.FirstOrDefault(i => matchingProperty(i).Equals(matchingProperty(item)));

            // if existing exists, replace existing
            if (existing != null)
            {
                items.Remove(existing);
            }

            items.Add(item);
        }

        /// <summary>
        /// Adds the elements of the specified collection to the end of the <see cref="ICollection{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="items"></param>
        public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                source.Add(item);
            }
        }

        /// <summary>
        /// Returns a distinct list of elements using the first-matched item on a specific property
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="items"></param>
        /// <param name="property">Property of object to compare</param>
        /// <returns></returns>
        public static IEnumerable<TItem> Distinct<TItem, TProperty>(this IEnumerable<TItem> items, Func<TItem, TProperty> property) where TProperty : IComparable
        {
            return items
                .GroupBy(property)
                .Select(i => i.FirstOrDefault());
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> items, T additionalValue)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            return items.Concat(new[]
            {
                additionalValue
            });
        }

        #region ToDataTable

        /// <summary>
        /// Converts a typed collection into a <see cref="DataTable"/>. This method excludes the column if <see cref="NotMappedAttribute"/> is bound to a property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="columnNameModifier">If a non-null string is returned, the column name is overridden</param>
        /// <param name="useColumnAttributeName">Specifies whether the data column should use the name from <see cref="ColumnAttribute"/></param>, if bound to a property.
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static DataTable ToDataTable(this IEnumerable collection, Func<PropertyInfo, string> columnNameModifier, bool useColumnAttributeName = false, string tableName = null)
        {
            if (collection == null)
            {
                return null;
            }

            #region Get type of first record

            // NOTE: 
            // We assume all values are the same, so use the type from the first record. 
            // This allows us to use the actual instance type instead of the generic version (useful for anonymous type collections)

            Type type = null;
            var genericArguments = collection.GetType().GetGenericArguments();

            if (genericArguments.Any())
            {
                // handle anonymous types, where there are multiple generic arguments (the first is an integer)
                type = genericArguments.FirstOrDefault(g => g.IsClass);
            }
            else
            {
                var enumerator = collection.GetEnumerator();

                enumerator.MoveNext();

                type = enumerator.Current.GetType();
            }

            #endregion

            using (var table = new DataTable(tableName))
            {
                #region Build Table Schema

                var propertyInfo = type
                    .GetProperties()
                    .Where(p => !p.HasCustomAttribute<NotMappedAttribute>())
                    .Select(p =>
                    {
                        string columnName = p.Name;
                        string columnTypeName = null;

                        // set column name to be either the property name
                        // or, if specified, based on the attribute
                        if (useColumnAttributeName)
                        {
                            var columnAttributes = p.GetColumnAttributes();

                            columnName = p.GetDataColumnNames(columnAttributes).FirstOrDefault();
                            columnTypeName = columnAttributes.FirstOrDefault()?.TypeName;
                        }

                        if (columnNameModifier != null)
                        {
                            var modifiedColumnName = columnNameModifier.Invoke(p);

                            if (modifiedColumnName.IsNotEmpty())
                            {
                                columnName = modifiedColumnName;
                            }
                        }

                        return new
                        {
                            Property = p,
                            ColumnTypeName = columnTypeName,
                            Converter = p.GetAttributeTypeConverter(),
                            ColumnName = columnName
                        };
                    })
                    .ToList();

                foreach (var info in propertyInfo)
                {
                    var columnType = info.Property.PropertyType;

                    if (columnType.IsGenericType)
                    {
                        columnType = columnType.GetGenericArguments()[0];
                    }

                    if (info.ColumnTypeName.IsNotEmpty())
                    {
                        columnType = SqlTypeMap.GetType(info.ColumnTypeName);
                    }

                    table.Columns.Add(info.ColumnName, columnType);
                }

                #endregion

                #region Populate the rows

                foreach (var item in collection)
                {
                    var row = table.NewRow();

                    foreach (var info in propertyInfo)
                    {
                        var value = info.Property.GetValue(item, null);
                        var columnType = row.Table.Columns[info.ColumnName].DataType;

                        if (info.Converter != null && info.Converter.CanConvertTo(columnType))
                        {
                            value = info.Converter.ConvertTo(value, columnType);
                        }

                        if (value != null)
                        {
                            row[info.ColumnName] = value;
                        }
                    }

                    table.Rows.Add(row);
                }

                #endregion

                return table;
            }
        }

        #endregion

        #region Join

        public static string Join<T>(this IEnumerable<T> items, string separator = "") where T : IComparable
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (separator == null)
            {
                throw new ArgumentNullException(nameof(separator));
            }

            return string.Join(separator, items.ToArray());
        }

        public static string Join<T, TSelector>(this IEnumerable<T> items, Func<T, TSelector> selector, string separator = "") where TSelector : IComparable => Join(items.Select(selector), separator);

        /// <summary>
        /// Performs a simple join of list type T, returning items from source only
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="outer"></param>
        /// <param name="inner"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IEnumerable<T> Join<T>(this IEnumerable<T> outer, IEnumerable<T> inner, Func<T, object> key) => outer.Join(inner, key, key, (t, i) => t);

        #endregion

        #region Zip

        public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> combine, bool symmetric)
        {
            if (first == null)
            {
                throw new ArgumentNullException(nameof(first));
            }

            if (second == null)
            {
                throw new ArgumentNullException(nameof(second));
            }

            if (combine == null)
            {
                throw new ArgumentNullException(nameof(combine));
            }

            var iter1 = first.GetEnumerator();
            var iter2 = second.GetEnumerator();

            var mn1 = iter1.MoveNext();
            var mn2 = iter2.MoveNext();

            while ((symmetric && mn1 && mn2) || (!symmetric && (mn1 || mn2)))
            {
                var c1 = default(TFirst);
                var c2 = default(TSecond);

                if (mn1)
                {
                    c1 = iter1.Current;
                    mn1 = iter1.MoveNext();
                }

                if (mn2)
                {
                    c2 = iter2.Current;
                    mn2 = iter2.MoveNext();
                }

                yield return combine(c1, c2);
            }
        }

        #endregion

        #region RemoveEmpty

        public static IEnumerable<T> RemoveEmpty<T>(this IEnumerable<T> list) where T : class
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            return list.Where(i => i != null);
        }

        #endregion

        #region Apply

        public static T Apply<T>(this IEnumerable<Func<T, T>> functions, T input)
        {
            if (functions == null)
            {
                throw new ArgumentNullException(nameof(functions));
            }

            functions.ForEach(f => input = f(input));

            return input;
        }

        #endregion

        #region Where

        public static IEnumerable<T> Where<T>(this IEnumerable<T> list, IEnumerable<Func<T, bool>> filters)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (filters == null)
            {
                throw new ArgumentNullException(nameof(filters));
            }

            filters.ForEach(f => list = list.Where(f));

            return list;
        }

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> list)
        {
            foreach (var item in list)
            {
                if (item != null)
                {
                    yield return item;
                }
            }
        }

        #endregion

        #region ForEach

        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            if (list == null)
            {
                return;
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (var i in list)
            {
                action(i);
            }
        }

        #endregion

        #region Traverse

        public static IEnumerable<TYield> Traverse<TYield, T>(this IEnumerable<T> source, Func<T, IEnumerable<TYield>> getYield, Func<T, IEnumerable<T>> getChildren)
        {
            if (source == null)
            {
                yield break;
            }

            if (getYield == null)
            {
                throw new ArgumentNullException(nameof(getYield));
            }

            if (getChildren == null)
            {
                throw new ArgumentNullException(nameof(getChildren));
            }

            foreach (var item in source)
            {
                var yields = getYield(item);

                if (yields != null)
                {
                    foreach (var yield in yields)
                    {
                        yield return yield;
                    }
                }

                var children = getChildren(item);

                if (children != null)
                {
                    foreach (var child in Traverse(children, getYield, getChildren))
                    {
                        yield return child;
                    }
                }
            }
        }

        public static IEnumerable<T> Traverse<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> recurse)
        {
            if (source == null)
            {
                yield break;
            }
            if (recurse == null)
            {
                throw new ArgumentNullException(nameof(recurse));
            }

            foreach (var item in source)
            {
                yield return item;

                var children = recurse(item);

                if (children != null)
                {
                    foreach (var child in Traverse(children, recurse))
                    {
                        yield return child;
                    }
                }
            }
        }

        #endregion

        #region JoinAction

        /// <summary>
        /// Performs an action on a joined set of lists of the same type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="key"></param>
        /// <param name="assignment"></param>
        public static void JoinAction<T>(this IEnumerable<T> left, IEnumerable<T> right, Func<T, object> key, Action<T, T> assignment) => left.JoinAction(right, key, key, assignment);

        /// <summary>
        /// Performs an action on a joined set of lists of a different type
        /// </summary>
        /// <typeparam name="TLeft"></typeparam>
        /// <typeparam name="TRight"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="leftKey"></param>
        /// <param name="rightKey"></param>
        /// <param name="assignment"></param>
        public static void JoinAction<TLeft, TRight>(this IEnumerable<TLeft> left, IEnumerable<TRight> right, Func<TLeft, object> leftKey, Func<TRight, object> rightKey, Action<TLeft, TRight> assignment)
        {
            left
                .Join(right, leftKey, rightKey, (l, r) => new { Left = l, Right = r })
                .ForEach(j => assignment(j.Left, j.Right));
        }

        /// <summary>
        /// Performs an action on a group joined set of lists of a different type
        /// </summary>
        /// <typeparam name="TLeft"></typeparam>
        /// <typeparam name="TRight"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="leftKey"></param>
        /// <param name="rightKey"></param>
        /// <param name="assignment"></param>
        public static void GroupJoinAction<TLeft, TRight>(this IEnumerable<TLeft> left, IEnumerable<TRight> right, Func<TLeft, object> leftKey, Func<TRight, object> rightKey, Action<TLeft, IEnumerable<TRight>> assignment)
        {
            left
                .GroupJoin(right, leftKey, rightKey, (l, r) => new { Left = l, Right = r })
                .ForEach(j => assignment(j.Left, j.Right));
        }

        #endregion

        /// <summary>
        /// Returns a collection of items containing the items of a second collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="items"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static IEnumerable<T> Containing<T>(this IEnumerable<T> source, IEnumerable<T> items, IEqualityComparer<T> comparer = null) => source.Where(s => items.Contains(s, comparer));

        public static bool Contains(this string[] source, string value, StringComparison comparison) => source.AnyAndNotNull(s => s.IndexOf(value, comparison) >= 0);

        public static IEnumerable<T> Convert<T>(this IEnumerable source) where T : IConvertible
        {
            return source
                .Cast<object>()
                .Select(s => s.To<T>());
        }

        public static TSource FirstOfType<TSource>(this IEnumerable source)
        {
            return source
                .OfType<TSource>()
                .FirstOrDefault();
        }

        public static bool Matches<T>(this ICollection<T> source, ICollection<T> items, IEqualityComparer<T> comparer = null) where T : IComparable
        {
            return source.Count == items.Count
                && !source.Except(items, comparer).Any()
                && !items.Except(source, comparer).Any();
        }

        public static IEnumerable<T> Insert<T>(this IEnumerable<T> items, int index, T item)
        {
            var count = 0;

            foreach (var i in items)
            {
                if (count == index)
                {
                    yield return item;
                }

                yield return i;

                count++;
            }
        }

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<KeyValuePair<TKey, TSource>> source, IEqualityComparer<TKey> comparer = null) => source.ToDictionary(k => k.Key, v => v.Value, comparer);

        #region Replace

        /// <summary>
        /// Replaces an item at index with replacement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="index">Index for item getting replaced</param>
        /// <param name="replacement">Replacement item</param>
        /// <returns></returns>
        public static IEnumerable<T> Replace<T>(this IEnumerable<T> items, int index, T replacement) => items.Replace(index, t => replacement);

        /// <summary>
        /// Replaces an item at index with replacement (lambda)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="index">Index for item getting replaced</param>
        /// <param name="replacement">Replacement lambda</param>
        /// <returns></returns>
        public static IEnumerable<T> Replace<T>(this IEnumerable<T> items, int index, Func<T, T> replacement)
        {
            var count = 0;

            foreach (var i in items)
            {
                if (count == index)
                {
                    yield return replacement(i);
                }
                else
                {
                    yield return i;
                }

                count++;
            }
        }

        #endregion

        public static IEnumerable<T> RandomTake<T>(this ICollection<T> collection, int take) => RandomTake(collection, collection.Count, take);

        public static IEnumerable<T> RandomTake<T>(this IEnumerable<T> items, int collectionCount, int take)
        {
            var rand = new Random();
            var needed = take;
            var available = collectionCount;

            foreach (var i in items)
            {
                if (needed == 0)
                {
                    yield break;
                }

                if (rand.NextDouble() < (double)needed / available)
                {
                    yield return i;
                    needed--;
                }

                available--;
            }
        }

        public static T RandomFirstOrDefault<T>(this IEnumerable<T> items) => RandomShuffle(items).FirstOrDefault();

        public static T RandomFirstOrDefault<T>(this IEnumerable<T> items, Func<T, bool> predicate) => RandomShuffle(items).Where(predicate).FirstOrDefault();

        public static void Swap<T>(this IList<T> list, int indexX, int indexY)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (indexX < 0 || indexX >= list.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(indexX));
            }
            if (indexY < 0 || indexY >= list.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(indexY));
            }

            var tmp = list[indexX];
            list[indexX] = list[indexY];
            list[indexY] = tmp;
        }

        /// <remarks>Knuth-Fisher-Yates shuffle algorithm, see http://www.codinghorror.com/blog/2007/12/the-danger-of-naivete.html
        /// </remarks>
        public static ICollection<T> RandomShuffle<T>(this IEnumerable<T> collection)
        {
            var rnd = new Random();
            var list = collection.ToList();

            for (var i = list.Count - 1; i > 0; i--)
            {
                var n = rnd.Next(i + 1);
                list.Swap(i, n);
            }

            return list;
        }

        public static int IndexOf<T>(this IEnumerable<T> items, T value, IEqualityComparer<T> comparer)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            var index = 0;
            foreach (var item in items)
            {
                if (comparer.Equals(item, value))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        public static int IndexOf<T>(this IEnumerable<T> items, T value)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            return items.IndexOf(value, EqualityComparer<T>.Default);
        }

        /// <summary>
        /// Determines whether a sequence has any elements. Null collections return false.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static bool AnyAndNotNull<T>(this IEnumerable<T> items, Func<T, bool> predicate = null)
        {
            if (items == null)
            {
                return false;
            }

            return predicate != null ? items.Any(predicate) : items.Any();
        }

        /// <summary>
        /// Determines whether a sequence is null or is an empty set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static bool NoneOrNull<T>(this IEnumerable<T> items, Func<T, bool> predicate = null) => !items.AnyAndNotNull(predicate);

        /// <summary>
        /// If list has one item, display singular string. Otherwise, display plural string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="plural">Word to be used if not 1 item count</param>
        /// <param name="singular">Word to be used if only 1 item count</param>
        /// <returns></returns>
        public static string IfPlural<T>(this IEnumerable<T> list, string plural, string singular)
        {
            var items = list.ToList();

            return items.AnyAndNotNull() && items.Count() == 1 ? singular : plural;
        }

        /// <summary>
        /// Executes an inline conditional statement if the sequence contains at least one element
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="list"></param>
        /// <param name="anyResult"></param>
        /// <param name="noneResult"></param>
        /// <returns></returns>
        public static TResult IfAny<TSource, TResult>(this IEnumerable<TSource> list, Func<IEnumerable<TSource>, TResult> anyResult, Func<IEnumerable<TSource>, TResult> noneResult)
        {
            if (list.AnyAndNotNull())
            {
                return anyResult(list);
            }

            return noneResult(list);
        }

        #region OrderBy

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, bool ascending)
        {
            return ascending
                ? source.OrderBy(keySelector)
                : source.OrderByDescending(keySelector);
        }

        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> source, string key) => source.OrderBy(key, true);

        public static IEnumerable<T> OrderByDescending<T>(this IEnumerable<T> source, string key) => source.OrderBy(key, false);

        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> source, string key, bool ascending)
        {
            // i => i.SomeProperty
            // "i" is a parameter
            // "i.SomeProperty" is the body of the expression
            var param = Expression.Parameter(typeof(T), "i");
            var body = Expression.Property(param, key);

            // create expression using body and parameters
            var expression = Expression.Lambda<Func<T, object>>(body, param).Compile();

            return source.OrderBy(expression, ascending);
        }

        #endregion

        #region Sort

        public static IEnumerable<T> Sort<T>(this IEnumerable<T> items) where T : IComparable<T> => items.OrderBy(i => i);

        public static IEnumerable<T> SortDescending<T>(this IEnumerable<T> items) where T : IComparable<T> => items.OrderByDescending(i => i);

        #endregion

        /// <summary>
        /// Returns an empty collection if collection is null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> items) => items ?? Enumerable.Empty<T>();

        /// <summary>
        /// Retrieves form data in a key/value pair collection
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<string, string>> GetParameters(this NameValueCollection collection)
        {
            foreach (string k in collection.Keys)
            {
                yield return new KeyValuePair<string, string>(k, collection[k]);
            }
        }
    }
}