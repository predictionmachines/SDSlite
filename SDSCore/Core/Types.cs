// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Globalization;


namespace Microsoft.Research.Science.Data
{
	/// <summary>
	/// Contains utilties facilitating work with <see cref="System.Type"/> instance.
	/// </summary>
	public static class TypeUtils
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsRealNumber(Type type)
		{
			return (type == typeof(float)) ||
				(type == typeof(double));
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsIntegerNumber(Type type)
		{
			return (type == typeof(System.Int16)) ||
				(type == typeof(System.Int32)) ||
				(type == typeof(System.Int64)) ||
				(type == typeof(System.UInt16)) ||
				(type == typeof(System.UInt32)) ||
				(type == typeof(System.UInt64)) ||
				// decimal is not supported by DataSet 1.0
				//(type == typeof(System.Decimal)) ||
				(type == typeof(System.Byte));
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsDateTime(Type type)
		{
			return (type == typeof(DateTime));
		}

		/// <summary>
		/// Checks whether the type is an integer or float number.
		/// </summary>
		public static bool IsNumeric(Type type)
		{
			return IsIntegerNumber(type) || IsRealNumber(type);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsEnum(Type type)
		{
			return (type.IsEnum);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		internal static object Subtract(object a, object b)
		{
			Expression expa = Expression.Constant(a);
			Expression expb = Expression.Constant(a);
			Expression sub = Expression.Subtract(expa, expb);
			LambdaExpression lsub = Expression.Lambda(sub);
			Delegate func = lsub.Compile();

			return func.DynamicInvoke();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static object GetDefaultMissingValue(Type type)
		{
			if (type == typeof(Double))
			{
				return Double.NaN;
			}
			else if (type == typeof(Single))
			{
				return Single.NaN;
			}
			else if (type == typeof(Int16))
			{
				return Int16.MinValue;
			}
			else if (type == typeof(Int32))
			{
				return Int32.MinValue;
			}
			else if (type == typeof(Int64))
			{
				return Int64.MinValue;
			}
			else if (type == typeof(UInt64))
			{
				return UInt64.MaxValue;
			}
			else if (type == typeof(UInt32))
			{
				return UInt32.MaxValue;
			}
			else if (type == typeof(UInt16))
			{
				return UInt16.MaxValue;
			}
			else if (type == typeof(Byte))
			{
				return Byte.MaxValue;
			}
			else if (type == typeof(SByte))
			{
				return SByte.MinValue;
			}
			else if (type == typeof(DateTime))
			{
				return DateTime.MinValue;
            }
            else if (type == typeof(Boolean))
            {
                return false;
            }
			return null;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="s"></param>
		/// <param name="type"></param>
		/// <param name="missingValue"></param>
		/// <returns></returns>
		public static object Parse(string s, Type type, object missingValue)
		{
			return Parse(s, type, missingValue, CultureInfo.InvariantCulture);
		}
		/// <summary>
		/// Converts a string into a type specified.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="type"></param>
		/// <param name="missingValue"></param>
        /// <param name="formatProvider"></param>
		/// <returns></returns>
		public static object Parse(string s, Type type, object missingValue, IFormatProvider formatProvider)
		{
			if (String.IsNullOrEmpty(s))
				return missingValue;
			return Parse(s, type, formatProvider);
		}
		/// <summary>
		/// Converts a string into a type specified.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static object Parse(string s, Type type)
		{
			return Parse(s, type, CultureInfo.InvariantCulture);
		}
		/// <summary>
		/// Converts a string into a type specified.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="type"></param>
		/// <param name="formatProvider"></param>
		/// <returns></returns>
		public static object Parse(string s, Type type, IFormatProvider formatProvider)
		{
			if (type == typeof(string))
				return s;
			if (String.IsNullOrEmpty(s))
				return null;

			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Boolean:
					return Boolean.Parse(s);

				case TypeCode.Char:
					return Char.Parse(s);

				case TypeCode.SByte:
					return SByte.Parse(s, formatProvider);

				case TypeCode.Byte:
					return Byte.Parse(s, formatProvider);

				case TypeCode.Int16:
					return Int16.Parse(s, formatProvider);

				case TypeCode.UInt16:
					return UInt16.Parse(s, formatProvider);

				case TypeCode.Int32:
					return Int32.Parse(s, formatProvider);

				case TypeCode.UInt32:
					return UInt32.Parse(s, formatProvider);

				case TypeCode.Int64:
					return Int64.Parse(s, formatProvider);

				case TypeCode.UInt64:
					return UInt64.Parse(s, formatProvider);

				case TypeCode.Single:
					return Single.Parse(s, formatProvider);

				case TypeCode.Double:
					return Double.Parse(s, formatProvider);

				case TypeCode.Decimal:
					return Decimal.Parse(s, formatProvider);

				case TypeCode.DateTime:
					return DateTime.Parse(s, formatProvider);

				case TypeCode.String:
					return s;
			}

			throw new NotSupportedException("Not supported type.");
		}
		/// <summary>
		/// Represents an object as a string. 
		/// If the object is IEnumerable, elements are separated
		/// with given separator.
		/// </summary>
		/// <param name="o"></param>
		/// <param name="separator"></param>
		/// <returns></returns>
		public static string SerializeToString(object o, char separator)
		{
			if (o == null)
				return "(null)";
			else if (o is string)
				return (string)o;
			else if (o is System.Collections.IEnumerable)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder("");
				bool first = true;
				foreach (object item in (System.Collections.IEnumerable)o)
				{
					if (!first) sb.Append(separator);
					sb.Append(item.ToString());
					first = false;
				}
				return sb.ToString();
			}
			else
				return o.ToString();
		}
		/// <summary>
		/// Checks the type of the value in string.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="formatProvider"></param>
		/// <returns></returns>
		public static bool IsDateTime(string s, IFormatProvider formatProvider)
		{
			DateTime dt;
			return DateTime.TryParse(s, formatProvider, DateTimeStyles.None, out dt);
		}
		/// <summary>
		/// Checks the type of the value in string.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="formatProvider"></param>
		/// <returns></returns>
		public static bool IsInt(string s, IFormatProvider formatProvider)
		{
			int i;
			return int.TryParse(s, NumberStyles.Integer, formatProvider, out i);
		}
		/// <summary>
		/// Checks the type of the value in string.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="formatProvider"></param>
		/// <returns></returns>
		public static bool IsDouble(string s, IFormatProvider formatProvider)
		{
			double d;
			return double.TryParse(s, NumberStyles.Float, formatProvider, out d);
		}
        /// <summary>
		/// Checks the type of the value in string.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsBool(string s)
        {
            bool b;
            return bool.TryParse(s, out b);
        }

		/// <summary>
		/// Checks the type of the value in string.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static Type GetElementType(Array a)
		{
			if (a == null)
				throw new ArgumentNullException("Cannot get type of elements for an empty array");
			return a.GetType().GetElementType();
		}
	}
}

