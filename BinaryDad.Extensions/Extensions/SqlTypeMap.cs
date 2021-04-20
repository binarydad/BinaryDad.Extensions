using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BinaryDad.Extensions
{
    public static class SqlTypeMap
    {
        public static Dictionary<SqlDbType, Type> typeMap = new Dictionary<SqlDbType, Type>
        {
            [SqlDbType.NVarChar] = typeof(string),
            [SqlDbType.VarChar] = typeof(string),
            [SqlDbType.Char] = typeof(char[]),
            [SqlDbType.NChar] = typeof(char[]),
            [SqlDbType.TinyInt] = typeof(byte),
            [SqlDbType.SmallInt] = typeof(short),
            [SqlDbType.Int] = typeof(int),
            [SqlDbType.BigInt] = typeof(long),
            [SqlDbType.Bit] = typeof(bool),
            [SqlDbType.DateTime] = typeof(DateTime),
            [SqlDbType.DateTime2] = typeof(DateTime),
            [SqlDbType.SmallDateTime] = typeof(DateTime),
            [SqlDbType.Time] = typeof(TimeSpan),
            [SqlDbType.DateTimeOffset] = typeof(DateTimeOffset),
            [SqlDbType.Decimal] = typeof(decimal),
            [SqlDbType.Money] = typeof(decimal),
            [SqlDbType.SmallMoney] = typeof(decimal),
            [SqlDbType.Float] = typeof(double),
            [SqlDbType.Real] = typeof(float)
        };

        public static Type GetType(string sqlDbTypeName)
        {
            return GetType(sqlDbTypeName.ToEnum<SqlDbType>());
        }

        public static Type GetType(SqlDbType sqlDbType)
        {
            // I'm aware this is backward
            return typeMap.FirstOrDefault(t => t.Key == sqlDbType).Value;
        }
    }
}