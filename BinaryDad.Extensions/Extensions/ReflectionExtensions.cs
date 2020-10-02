using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;

namespace BinaryDad.Extensions
{
    public static class ReflectionExtensions
    {
        #region Public

        /// <summary>
        /// Determines whether a property has a specific <see cref="Attribute"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public static bool HasCustomAttribute<T>(this PropertyInfo property) where T : Attribute
        {
            return property.GetCustomAttributes(typeof(T), false).Any();
        }

        /// <summary>
        /// Sets the value of a property using the matched column from the data row, using  or <see cref="ColumnAttribute"/> for binding.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="row"></param>
        /// <param name="instance"></param>
        public static void SetValue(this PropertyInfo property, DataRow row, object instance)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (row == null)
            {
                return;
            }

            // get the column name from the row
            var matchedColumnName = property.GetDataColumnName(row);

            //Set the value if we matched a column. If we don't have a match, there's simply no way to set a value.
            if (matchedColumnName != null)
            {
                // raw value from the row, matching on column name
                var tableValue = row[matchedColumnName];

                // if DBNull, set as null
                var value = tableValue == DBNull.Value ? null : tableValue;

                // if the value is null, we know to just set as null
                if (value == null)
                {
                    property.SetValue(instance, null, null);
                }
                else
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

        #endregion

        #region Internal

        /// <summary>
        /// Retrieves a list of available property binding aliases using <see cref="ColumnAttribute"/>.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        internal static IEnumerable<string> GetDataColumnNames(this PropertyInfo property)
        {
            #region Null checks

            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            #endregion

            var dataRowFieldNames = new List<string>();

            var attributes = property
                .GetCustomAttributes(true);

            var columnAttribute = attributes.FirstOfType<ColumnAttribute>();

            if (columnAttribute != null)
            {
                dataRowFieldNames.Add(columnAttribute.Name);
            }

            // add the property's name at the end, so it's the last in the lookup
            dataRowFieldNames.Add(property.Name);

            return dataRowFieldNames;
        }

        /// <summary>
        /// Retrieves a list of available property binding aliases using <see cref="EnumAliasAttribute"/>
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        internal static IEnumerable<string> GetEnumAliases(this FieldInfo field)
        {
            #region Null checks

            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }

            #endregion

            var aliasNames = new List<string>();

            var enumAlias = field
                .GetCustomAttributes(true)
                .FirstOfType<EnumAliasAttribute>();

            if (enumAlias != null)
            {
                aliasNames.AddRange(enumAlias.EnumAliasNames);
            }

            aliasNames.Add(field.Name);

            return aliasNames;
        }

        /// <summary>
        /// Retrieves the first matched column name from the data row. Uses <see cref="PropertyAliasAttribute"></see> or <see cref="ColumnAttribute"/>.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        internal static string GetDataColumnName(this PropertyInfo property, DataRow row)
        {
            return property.GetDataColumnName(row.Table.Columns);
        }

        /// <summary>
        /// Retrieves the first matched column name from the collection of data columns. Uses <see cref="PropertyAliasAttribute"></see> or <see cref="ColumnAttribute"/>.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        internal static string GetDataColumnName(this PropertyInfo property, DataColumnCollection columns)
        {
            //Check whether we have any columns with the data row field names. This should not be null if the property
            //was configured correctly with PropertyAliasAttribute. 
            return property
                .GetDataColumnNames()
                .FirstOrDefault(f => columns.Contains(f));
        }

        /// <summary>
        /// Retrieves an instance of the <see cref="TypeConverter"/> associated with the property's <see cref="TypeConverterAttribute"/>
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        internal static TypeConverter GetAttributeTypeConverter(this PropertyInfo property)
        {
            var converter = property.GetCustomAttribute<TypeConverterAttribute>();

            if (converter != null)
            {
                var converterType = Type.GetType(converter.ConverterTypeName);

                if (converterType == typeof(EnumConverter))
                {
                    return (TypeConverter)Activator.CreateInstance(converterType, property.PropertyType);
                }

                return (TypeConverter)Activator.CreateInstance(converterType);
            }

            return null;
        }

        #endregion
    }
}