// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace Microsoft.Research.Science.Data.Utilities
{
    public static class ArrayHelper
    {
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

        public static int IndexOf<T>(this T[] array, Predicate<T> pred)
        {
            for (int i = 0; i < array.Length; i++)
                if (pred(array[i]))
                    return i;
            return -1;
        }

        public static void Replace(this Array array, object oldValue, object newValue)
        {
            int[] pos = new int[array.Rank];
            while (true)
            {
                object item = array.GetValue(pos);
                if (Object.Equals(item, oldValue))
                    array.SetValue(newValue, pos);
                int dim = 0;
                while (++pos[dim] >= array.GetLength(dim))
                {
                    pos[dim] = 0;
                    if (++dim >= array.Rank)
                        return;
                }                
            }
        }

        public static void SetValues(this Array array, object newValue)
        {
			if (array.Length == 0) return;
            int[] pos = new int[array.Rank];
            while (true)
            {
                array.SetValue(newValue, pos);
                int dim = 0;
                while (++pos[dim] >= array.GetLength(dim))
                {
                    pos[dim] = 0;
                    if (++dim >= array.Rank)
                        return;
                }
            }
        }

		public static void CopyValues(this Array dst, Array src, int[] srcOrigin, int[] dstOrigin)
		{

            if (src.Length == 0)
                return;

            if (src.Rank == 1)
            {
                int shape =  src.GetLength(0);
                Array.Copy(src, srcOrigin[0], dst, dstOrigin[0], shape);
                return;
            }

            Type type = src.GetType().GetElementType();

            if (type == typeof(DateTime) ||
                type == typeof(bool) ||
                type == typeof(string))
            {
                CopyValuesBase(src, dst, srcOrigin, dstOrigin);
            }
            else
            {

                int[] srcShape = GetArrayShape(src);
                int[] dstShape = GetArrayShape(dst);

                bool isOriginZeroAllExceptFirst = true;
                bool isShapeEqualsAllExceptFirstOrAll = true;
                for (int i = 1; i < dstOrigin.Length; i++)
                {
                    if (srcOrigin[i] != 0)
                        isOriginZeroAllExceptFirst = false;
                    if (dstOrigin[i] != 0)
                        isOriginZeroAllExceptFirst = false;
                    if (srcShape[i] != dstShape[i])
                        isShapeEqualsAllExceptFirstOrAll = false;
                }
                if ((srcOrigin[0] == 0) && (dstOrigin[0] == 0))
                    isOriginZeroAllExceptFirst = false;
                bool isOriginZero = IsZero(srcOrigin) && IsZero(dstOrigin);
                //If all bytes can be copied by one Buffer.BlockCopy
                if ((isOriginZero || isOriginZeroAllExceptFirst) &&
                    isShapeEqualsAllExceptFirstOrAll)
                {
                    int size = Buffer.ByteLength(src);

                    int sizeOfType = GetSizeOfType(type);

                    int originCubeSize = 1;

                    for (int i = 1; i < src.Rank; i++)
                    {
                        originCubeSize *= srcShape[i];
                    }


                    size -= originCubeSize * sizeOfType * srcOrigin[0];

                    int srcOffset = 0;
                    int dstOffset = 0;

                    if (isOriginZeroAllExceptFirst)
                    {
                        srcOffset = GetScalarIndex(srcOrigin, srcShape);
                        dstOffset = GetScalarIndex(dstOrigin, dstShape);
                        srcOffset *= sizeOfType;
                        dstOffset *= sizeOfType;
                    }

                    Buffer.BlockCopy(src, srcOffset, dst, dstOffset, size);
                }
                //Copy bytes line by line
                else
                {
                    int sizeOfType = GetSizeOfType(type);

                    int[] getStride = new int[src.Rank];
                    int[] setStride = new int[src.Rank];

                    int[] getIndex = new int[src.Rank];
                    int[] setIndex = new int[src.Rank];

                    dstOrigin.CopyTo(setIndex, 0);
                    srcOrigin.CopyTo(getIndex, 0);

                    for (int i = 0; i < getStride.Length; i++)
                    {
                        getStride[i] = 1;
                        setStride[i] = 1;
                    }

                    getStride[getStride.Length - 1] = src.GetLength(src.Rank - 1);
                    setStride[setStride.Length - 1] = dst.GetLength(dst.Rank - 1);

                    bool isInBounds = true;

                    int copyBlockLength = srcShape[src.Rank - 1] * sizeOfType;

                    while (isInBounds)
                    {
                        Buffer.BlockCopy(src, GetScalarIndex(getIndex, srcShape) * sizeOfType, dst, GetScalarIndex(setIndex, dstShape) * sizeOfType, copyBlockLength);
                        int curDim = src.Rank - 1;
                        while (curDim >= 0 && (
                            (getIndex[curDim] += getStride[curDim]) >= srcShape[curDim] ||
                            (setIndex[curDim] += setStride[curDim]) >= dstShape[curDim]))
                        {
                            if (curDim == 0)
                                isInBounds = false;
                            getIndex[curDim] = srcOrigin[curDim];
                            setIndex[curDim] = dstOrigin[curDim--];
                        }
                    }
                }
            }
        }

        public static void CopyValues(this Array dst, Array src, int[] dstOrigin)
        {
            CopyValues(dst, src, new int[dstOrigin.Length], dstOrigin);
        }


        private static void CopyValuesBase(Array src, Array dst, int[] srcOrigin, int[] dstOrigin)
        {
            int[] srcShape = GetArrayShape(src);
            int[] dstShape = GetArrayShape(dst);

            int[] srcPos = new int[src.Rank];
            int[] dstPos = new int[dst.Rank];

            for (int i = 0; i < dstPos.Length; i++)
            {
                srcPos[i] += srcOrigin[i];
                dstPos[i] += dstOrigin[i];
            }

            while (true)
            {
                dst.SetValue(src.GetValue(srcPos), dstPos);
                int dim = 0;
                while (true)
                {
                    srcPos[dim]++;
                    dstPos[dim]++;
                    if (srcPos[dim] < srcShape[dim])
                        break;
                    else
                    {
                        srcPos[dim] = srcOrigin[dim];
                        dstPos[dim] = dstOrigin[dim];
                        if (++dim >= src.Rank)
                            return;
                    }
                }
            }
        }

        private static int GetSizeOfType(Type type)
        {
            if (type == typeof(Double))
                return sizeof(Double);
            else if (type == typeof(Single))
                return sizeof(Single);
            else if (type == typeof(Int16))
                return sizeof(Int16);
            else if (type == typeof(Int32))
                return sizeof(Int32);
            else if (type == typeof(Int64))
                return sizeof(Int64);
            else if (type == typeof(UInt64))
                return sizeof(UInt64);
            else if (type == typeof(UInt32))
                return sizeof(UInt32);
            else if (type == typeof(UInt16))
                return sizeof(UInt16);
            else if (type == typeof(Byte))
                return sizeof(Byte);
            else if (type == typeof(SByte))
                return sizeof(SByte);
            else throw new NotSupportedException();
        }

        private static int[] GetArrayShape(Array array)
        {
            int[] shape;
            if (array != null)
            {
                shape = new int[array.Rank];
                for (int i = 0; i < array.Rank; i++)
                {
                    shape[i] = array.GetLength(i);
                }
                return shape;
            }

            shape = new int[array.Rank];
            return shape;
        }

        private static int GetScalarIndex(int[] index, int[] shape)
        {
            int result = 0;
            int mul = 1;
            for (int ind = index.Length - 1; ind >= 0; ind--)
            {
                result += index[ind] * mul;
                mul *= shape[ind];
            }
            return result;
        }

        private static bool IsZero(int[] origin)
        {
            for (int i = 0; i < origin.Length; i++)
            {
                if (origin[i] != 0) return false;
            }
            return true;
        }

        public static Array Resize(this Array array, int[] newShape, object missingValue)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (array.Rank != newShape.Length)
                throw new ArgumentException("Ranks of array and new shape do not match");
            if (array.ShapeEquals(newShape))
                return array;
            if (newShape.Any(dim => dim < 0))
                throw new ArgumentException("New shape contains negative elements");

            Array newArray = Array.CreateInstance(array.GetType().GetElementType(), newShape);
            if (array.Rank == 1)
            {
                int i;
                for (i = 0; i < array.Length; i++)
                    newArray.SetValue(array.GetValue(i), i);
                for (; i < newShape[0]; i++)
                    newArray.SetValue(missingValue, i);
                return newArray;
            }
            int[] pos = new int[array.Rank];
            int[] oldShape = array.GetShape();
            while (true)
            {
                bool insideOldArray = true;
                for (int i = 0; i < oldShape.Length && insideOldArray; i++)
                    if (pos[i] >= oldShape[i])
                        insideOldArray = false;
                if (insideOldArray)
                    newArray.SetValue(array.GetValue(pos), pos);
                else
                    newArray.SetValue(missingValue, pos);
                int dim = 0;
                while (++pos[dim] >= newShape[dim])
                {
                    pos[dim] = 0;
                    if (++dim >= newShape.Length)
                        return newArray;
                }
            }
        }

        public static int[] GetShape(this Array array)
        {
            int[] shape = new int[array.Rank];
            for (int i = 0; i < array.Rank; i++)
                shape[i] = array.GetLength(i);
            return shape;
        }

        public static bool ShapeEquals(this Array array, int[] shape)
        {
            if (array.Rank != shape.Length)
                return false;
            for (int i = 0; i < shape.Length; i++)
                if (array.GetLength(i) != shape[i])
                    return false;
            return true;
        }

        public static bool ShapeIsSmaller(this Array array, int[] shape)
        {
            if (array.Rank != shape.Length)
                return false;
			for (int i = 0; i < shape.Length; i++)
				if (array.GetLength(i) < shape[i])
					return true;
			return false;
        }

        public static bool ShapeIsEmpty(int[] shape)
        {
            foreach (int dim in shape)
                if (dim != 0)
                    return false;
            return true;
        }
    }
}

