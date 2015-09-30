// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Microsoft.Research.Science.Data
{
	internal sealed class OrderedArray1d : ArrayWrapper
	{
		private ArrayOrder order;

		public OrderedArray1d(Type type, bool ordered)
			: base(1, type)
		{
			if (ordered && TypeUtils.IsNumeric(type))
				order = ArrayOrder.Unknown;
			else
				order = ArrayOrder.None;
		}

		public OrderedArray1d(Type type)
			: this(type, true)
		{
		}

		public void EnforceOrder(bool enforce)
		{
			if (!TypeUtils.IsNumeric(type) && !TypeUtils.IsDateTime(type) && !(type == typeof(string))) return;

			if (enforce)
				order = ArrayOrder.Unknown;
			else
				order = ArrayOrder.None;
		}

		public ArrayOrder Order { get { return order; } }

		/// <summary>
		/// Works as Array.BinarySearch()
		/// </summary>
		public int IndexOf(object value)
		{
			if (order == ArrayOrder.None)
				throw new Exception("Axis is not ordered.");

			if (order == ArrayOrder.Unknown)
			{
				SetUpOrder(array);
			}

			if (array == null || array.Length == 0)
				throw new Exception("Array is empty.");
			if (array.Length == 1)
				return 0;

			if (!TypeUtils.IsNumeric(type) && !TypeUtils.IsDateTime(type))
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (value == array.GetValue(i))
						return i;
				}
				throw new Exception("Value not found");
			}

			if (order == ArrayOrder.Ascendant)
				return Array.BinarySearch(array, value);

			return Array.BinarySearch(array, value, new DescComparer());
		}

		public override void PutData(int[] origin, Array a)
		{
			base.PutData(origin, a);

			if (order == ArrayOrder.None) return;
			if (!TypeUtils.IsNumeric(type)) return;

			int iStart = (origin == null) ? 0 : origin[0];
			int iEnd = Math.Min(iStart + a.Length, array.Length);

			if (iStart > 0) iStart--;
			if (iStart > 0) iStart--;
			if (iEnd < array.Length) iEnd++;
			if (iEnd < array.Length) iEnd++;


			if (order == ArrayOrder.Unknown)
			{
				SetUpOrder(array);
				iStart = 0;
				iEnd = array.Length;
			}

			Comparer cmp = Comparer.DefaultInvariant;

			if (order == ArrayOrder.Ascendant)
			{
				for (int i = iStart; i < iEnd - 1; i++)
					if (cmp.Compare(array.GetValue(i + 1), array.GetValue(i)) <= 0)
						throw new Exception("Axis is not sorted.");
			}
			else if (order == ArrayOrder.Descendant)
			{
				for (int i = iStart; i < iEnd - 1; i++)
					if (cmp.Compare(array.GetValue(i + 1), array.GetValue(i)) >= 0)
						throw new Exception("Axis is not sorted.");
			}
		}

		private void SetUpOrder(Array a)
		{
			if (a.Length <= 1) return;
			int res = GetOrder(a);
			if (res == 0) throw new Exception("Axis must be strictly monotonic.");
			order = (res > 0) ? ArrayOrder.Ascendant : ArrayOrder.Descendant;
		}

		public override ArrayWrapper Copy()
		{
			ArrayWrapper aw = new OrderedArray1d(type);
			if (array != null)
				aw.PutData(null, array);

			return aw;
		}

		private class DescComparer : IComparer
		{
			#region IComparer Members

			public int Compare(object x, object y)
			{
				return Comparer.Default.Compare(y, x);
			}

			#endregion
		}

		/// <summary>
		/// Determines the order of the array.
		/// </summary>
		public static ArrayOrder IsOrdered(Array a)
		{
			if (a == null || a.Length <= 1)
				return ArrayOrder.Unknown;

			int diff = GetOrder(a);

			if (diff == 0) return ArrayOrder.None;
			ArrayOrder order = (diff > 0) ? ArrayOrder.Ascendant : ArrayOrder.Descendant;

			Comparer cmp = Comparer.DefaultInvariant;
			if (order == ArrayOrder.Ascendant)
			{
				for (int i = 0; i < a.Length - 1; i++)
					if (cmp.Compare(a.GetValue(i + 1), a.GetValue(i)) <= 0)
						return ArrayOrder.None;
			}
			else if (order == ArrayOrder.Descendant)
			{
				for (int i = 0; i < a.Length - 1; i++)
					if (cmp.Compare(a.GetValue(i + 1), a.GetValue(i)) >= 0)
						return ArrayOrder.None;
			}
			return order;
		}

		/// <summary>
		/// Determines the order of the array.
		/// </summary>
		public static ArrayOrder IsOrdered<T>(T[] a)
		{
			if (a == null || a.Length <= 1)
				return ArrayOrder.Unknown;

			Comparer c = Comparer.DefaultInvariant;
			int diff = c.Compare(a[1], a[0]);

			if (diff == 0) return ArrayOrder.None;
			ArrayOrder order = (diff > 0) ? ArrayOrder.Ascendant : ArrayOrder.Descendant;

			
			if (order == ArrayOrder.Ascendant)
			{
				for (int i = 1; i < a.Length - 1; i++)
					if (c.Compare(a[i + 1], a[i]) <= 0)
						return ArrayOrder.None;
			}
			else if (order == ArrayOrder.Descendant)
			{
				for (int i = 1; i < a.Length - 1; i++)
					if (c.Compare(a[i + 1], a[i]) >= 0)
						return ArrayOrder.None;
			}
			return order;
		}
		

		/// <summary>
		/// <para>Positive: ascend</para>
		/// <para>Negative: descend</para>
		/// </summary>
		private static int GetOrder(Array a)
		{
			return Comparer.DefaultInvariant.Compare(a.GetValue(1), a.GetValue(0));
		}

		/// <summary>
		/// <para>Positive: ascend</para>
		/// <para>Negative: descend</para>
		/// </summary>
		private static int GetOrder<T>(T[] a)
		{
			return Comparer.DefaultInvariant.Compare(a[1], a[0]);
		}
	}

	/// <summary>
	/// Describes an order of an array.
	/// </summary>
	public enum ArrayOrder
	{
		/// <summary>
		/// An array is not ordered.
		/// </summary>
		None,
		/// <summary>
		/// An array is not checked for an order.
		/// </summary>
		NotChecked,
		/// <summary>
		/// An array is ordered, but its order is still unknown.
		/// </summary>
		Unknown,
		/// <summary>
		/// An array has an ascendant order.
		/// </summary>
		Ascendant,
		/// <summary>
		/// An array has a descendant order.
		/// </summary>
		Descendant
	}

}

