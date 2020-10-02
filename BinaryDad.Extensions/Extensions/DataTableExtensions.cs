using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BinaryDad.Extensions
{
    public static class DataTableExtensions
    {
        /// <summary>
        /// Maps table data to a collection of objects with matching properties. Either <see cref="PropertyAliasAttribute"></see> or <see cref="ColumnAttribute"/> may be used to bind columns to properties.
        /// </summary>
        /// <typeparam name="T">The type to convert each <see cref="DataRow"/> row to</typeparam>
        /// <param name="table"></param>
        /// <param name="afterRowBinding">Action used to modify each object with additional data from row. This is applied after binding.</param>
        /// <returns></returns>
        public static IList<T> ToList<T>(this DataTable table, Action<T, DataRow> afterRowBinding = null)
        {
            #region Null checks

            if (table == null)
            {
                return null;
            }

            #endregion

            return table.Rows
                .Cast<DataRow>()
                .ToList(afterRowBinding);
        }

        /// <summary>
        /// Maps a collection of rows to a collection of objects with matching properties. Either <see cref="PropertyAliasAttribute"></see> or <see cref="ColumnAttribute"/> may be used to bind columns to properties.
        /// </summary>
        /// <typeparam name="T">The type to convert each <see cref="DataRow"/> row to</typeparam>
        /// <param name="rows"></param>
        /// <param name="afterRowBinding">Action used to modify each object with additional data from row. This is applied after binding.</param>
        /// <returns></returns>
        public static IList<T> ToList<T>(this IEnumerable<DataRow> rows, Action<T, DataRow> afterRowBinding = null)
        {
            #region Null checks

            if (rows == null)
            {
                return null;
            }

            #endregion

            var type = typeof(T);

            // Prevent deferred execution: http://stackoverflow.com/questions/3628425/ienumerable-vs-list-what-to-use-how-do-they-work
            return rows
                .Select(row =>
                {
                    // map the row to the object
                    var instance = (T)row.To(type);

                    // invoke any modifiers
                    afterRowBinding?.Invoke(instance, row);

                    return instance;
                })
                .ToList();
        }

        /// <summary>
        /// Maps the first <see cref="DataRow"/> of a <see cref="DataTable"/> to an object type. Either <see cref="PropertyAliasAttribute"/> or <see cref="ColumnAttribute"/> may be used to bind columns to properties.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static T To<T>(this DataTable table) => table.FirstRow().To<T>();

        /// <summary>
        /// Maps a <see cref="DataRow"/> to an object type. Either <see cref="PropertyAliasAttribute"/> or <see cref="ColumnAttribute"/> may be used to bind columns to properties.
        /// </summary>
        /// <typeparam name="T">The type to convert the <see cref="DataRow"/> to</typeparam>
        /// <param name="row"></param>
        /// <returns></returns>
        public static T To<T>(this DataRow row) => (T)row.To(typeof(T));

        /// <summary>
        /// Maps a <see cref="DataRow"/> to an object type. Either <see cref="PropertyAliasAttribute"/> or <see cref="ColumnAttribute"/> may be used to bind columns to properties.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="type">The type to convert the <see cref="DataRow"/> to</param>
        /// <returns></returns>
        public static object To(this DataRow row, Type type)
        {
            #region Null checks

            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            #endregion

            var properties = type.GetProperties();

            if (type.IsValueType || type == typeof(string))
            {
                // If the object is a value/string type, we can only pull one item, so assume it's the first in the row
                return row[0].To(type);
            }

            var instance = Activator.CreateInstance(type);

            properties.ForEach(p => SetValueFromRow(p, row, instance));

            return instance;
        }

        /// <summary>
        /// Returns a dictionary of key (column header) and value (cell value) pairs
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static IDictionary<string, object> ToDictionary(this DataRow row)
        {
            return row.Table.Columns
                .Cast<DataColumn>()
                .ToDictionary(c => c.ColumnName, c => row[c]);
        }

        /// <summary>
        /// Converts a data table to a CSV string
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static string ToCsv(this DataTable table)
        {
            using (var stream = new MemoryStream())
            {
                table.ToCsv(stream);

                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        /// <summary>
        /// Converts data table to a CSV string and writes to a file
        /// </summary>
        /// <param name="table"></param>
        /// <param name="filePath"></param>
        public static void ToCsv(this DataTable table, string filePath)
        {
            using (var file = File.OpenWrite(filePath))
            {
                table.ToCsv(file);
            }
        }

        /// <summary>
        /// Converts data table to a CSV string and writes to a stream
        /// </summary>
        /// <param name="table"></param>
        /// <param name="stream"></param>
        public static void ToCsv(this DataTable table, Stream stream)
        {
            var encoding = new UTF8Encoding(false, true);

            // leave the underlying stream open (uses default parameters)
            using (var sw = new StreamWriter(stream, encoding, 1024, true))
            {
                #region Write Columns

                var columnNames = table.Columns.Cast<DataColumn>()
                    .Select(column => column.ColumnName)
                    .ToArray();

                sw.WriteLine(string.Join(",", columnNames));

                #endregion

                #region Write Rows

                foreach (DataRow row in table.Rows)
                {
                    var fields = row.ItemArray
                        .Select(field => QuoteValue(field.ToString()))
                        .ToArray();

                    sw.WriteLine(string.Join(",", fields));
                }

                #endregion

                sw.Close();
            }
        }

        /// <summary>
        /// Performs an inline replacement of a <see cref="DataRow"/> value
        /// </summary>
        /// <param name="row"></param>
        /// <param name="columnName"></param>
        /// <param name="value">Allows for update/modification of the existing value</param>
        public static void SetField(this DataRow row, string columnName, Func<object, object> value) => row[columnName] = value(row[columnName]);

        /// <summary>
        /// Iterates through a collection of <see cref="DataRow"/>
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="predicate"></param>
        public static void ForEach(this DataRowCollection rows, Action<DataRow> predicate)
        {
            #region Null checks

            if (rows == null)
            {
                return;
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            #endregion

            rows
                .Cast<DataRow>()
                .ForEach(predicate);
        }

        /// <summary>
        /// Returns whether the <see cref="DataTable"/> is not null and has rows
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool HasRows(this DataTable data)
        {
            if (data?.Rows == null)
            {
                return false;
            }

            return data.Rows.Count > 0;
        }

        /// <summary>
        /// Returns the first row of a data table, or null if there are no rows
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DataRow FirstRow(this DataTable data)
        {
            if (data.HasRows())
            {
                return data.Rows[0];
            }

            return null;
        }

        /// <summary>
        /// Projects <see cref="DataRow"/> of a <see cref="DataTable"/> into a new form
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IEnumerable<T> Select<T>(this DataTable data, Func<DataRow, T> selector) => data.Rows.Cast<DataRow>().Select(selector);

        #region Private Methods

        private static string QuoteValue(string value) => string.Concat("\"", value.Replace("\"", "\"\""), "\"");

        /// <summary>
        /// Sets the value of a property using the matched column from the data row. Supports using <see cref="ColumnAttribute"/> and <see cref="NotMappedAttribute"/> for column mapping.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="row"></param>
        /// <param name="instance"></param>
        private static void SetValueFromRow(PropertyInfo property, DataRow row, object instance)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            // ignore the property if specified
            if (property.HasCustomAttribute<NotMappedAttribute>())
            {
                return;
            }

            if (row == null)
            {
                return;
            }

            // get the column name from the row
            var matchedColumnName = property.GetDataColumnName(row.Table.Columns);

            //Set the value if we matched a column. If we don't have a match, there's simply no way to set a value.
            if (matchedColumnName != null)
            {
                // raw value from the row, matching on column name
                var tableValue = row[matchedColumnName];

                // RJP - we'll come back to this
                tableValue = ApplyTypeConversion(property, tableValue);

                // if DBNull, set as null
                var value = tableValue == DBNull.Value ? null : tableValue;

                if (value != null)
                {
                    try
                    {
                        property.SetValue(instance, value.To(property.PropertyType), null);
                    }
                    catch (Exception ex)
                    {
                        // encapsulate more detail about why the conversion failed
                        throw new DataPropertyConversionException(instance, property, value, ex);
                    }
                }
            }
        }

        private static object ApplyTypeConversion(PropertyInfo property, object sourceValue)
        {
            var converter = property.GetAttributeTypeConverter();

            if (converter != null && converter.CanConvertFrom(sourceValue.GetType()))
            {
                sourceValue = converter.ConvertFrom(sourceValue);
            }

            return sourceValue;
        }

        #endregion
    }
}