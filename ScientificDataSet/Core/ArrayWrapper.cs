// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data
{
	/// <summary>
	/// Keeps array, its rank and type. Empty and zero-rank arrays are supported.
	/// </summary>
    [Obsolete("This class is obsolete and will be removed in the next version.")]
	public class ArrayWrapper
	{
        /// <summary>Array of holding actual data. Can be null if there is no data</summary>
		protected Array array;
        /// <summary>Rank of array. Valid even if <see cref="array"/> field is null</summary>
		protected int rank;
        /// <summary>Type of array elements. Valid even if <see cref="type"/> field is null</summary>
		protected Type type;

        /// <summary>Initializes a new instance of ArrayWrapper with specified rank and type of elements
        /// but without data</summary>
        /// <param name="rank">Rank of array. Zero-ranked arrays are also supported.</param>
        /// <param name="type">Type of array elements</param>
		public ArrayWrapper(int rank, Type type)
		{
			this.rank = rank;
			if (rank < 0)
				throw new ArgumentOutOfRangeException("Rank must be non-negative");
			this.type = type;
			array = null;
			if (rank == 0)
			{
				array = Array.CreateInstance(type, 1);
			}
		}

        /// <summary>Gets or sets data for the array</summary>
        /// <remarks>Array is not copied to the wrapper, so modification of assigned
        /// array means modification of ArrayWrapper</remarks>
		public Array Data
		{
			get
			{
				return array;
			}
			set
			{
				if (value.Rank != (rank == 0 ? 1 : rank))
					throw new ArgumentException("Rank is wrong");
				array = value;
			}
		}

        /// <summary>Returns rank of wrapper array. Note that zero-ranked array are supported</summary>
		public int Rank { get { return rank; } }

        /// <summary>Return type of elements for this array</summary>
		public Type DataType { get { return type; } }

		#region Data access routines

        /// <summary>Return array with shape of wrapper array. Returned array is a copy, so
        /// it can be modified in any way without affecting ArrayWrapper</summary>
        /// <returns>Array of integers containing sizes of dimensions</returns>
		public int[] GetShape()
		{
			int[] shape;
			if (array != null)
			{
				shape = new int[rank];
				for (int i = 0; i < rank; i++)
				{
					shape[i] = array.GetLength(i);
				}
				return shape;
			}

			shape = new int[rank];
			return shape;
		}

        /// <summary>Returns copy of data from part of wrapper array</summary>
        /// <param name="origin">The origin of the window (e.g., the left-bottom corner). Null means all zeros.</param>
        /// <param name="shape">The shape of the region. Null means entire array.</param>
        /// <returns>An array of data from the specified region.</returns>
        public virtual Array GetData(int[] origin, int[] shape)
        {
            if (rank == 0)
            {
                if (array == null)
                    throw new Exception("Array is empty");
                return array;
            }

            if ((shape == null || EqualToShape(shape)) &&
                (origin == null || IsZero(origin)))
            {
                return array != null ? (Array)array.Clone() : Array.CreateInstance(type, new int[rank]);
            }

            if (array == null)
                throw new Exception("Array is empty");

            if (origin == null)
                origin = new int[array.Rank];
            else if (origin.Length != rank)
                throw new Exception("Wrong length of origin.");

            if (shape == null)
            {
                shape = new int[array.Rank];
                for (int i = 0; i < array.Rank; i++)
                    shape[i] = array.GetLength(i) - origin[i];
            }
            else if (shape.Length != rank)
                throw new Exception("Wrong length of shape.");

            Array res = Array.CreateInstance(type, shape);
            CopyArray(array, res, origin, new int[origin.Length], false);
            return res;
        }

        /// <summary>Writes the data to the wrapped array starting with the specified origin indices.
        /// </summary>
        /// <param name="origin">Indices to start adding of data. Null means all zeros.</param>
        /// <param name="a">Data to add to the variable.</param>
        public virtual void PutData(int[] origin, Array a)
        {
            if (a == null) return;
            if (rank == 0)
            {
                if (a.Rank != 1)
                    throw new Exception("Wrong rank");
                array.SetValue(a.GetValue(0), 0);
                return;
            }

            if (rank != a.Rank)
                throw new Exception("Wrong rank");

            int[] shape = null;

            if (array == null)
            {
                if (origin == null || IsZero(origin))
                {
                    array = (Array)a.Clone();
                    return;
                }

                shape = new int[rank];
                for (int i = 0; i < rank; i++)
                    shape[i] = origin[i] + a.GetLength(i);

                array = Array.CreateInstance(type, shape);
                CopyArray(a, array, new int[origin.Length], origin, true);
            }
            else
            {
                Array oldArr = array;

                if (origin == null)
                    origin = new int[rank];

                shape = new int[array.Rank];
                bool sufficient = true;
                for (int i = 0; i < array.Rank; i++)
                {
                    int size = origin[i] + a.GetLength(i);
                    sufficient = sufficient && (size <= oldArr.GetLength(i));

                    shape[i] = Math.Max(size, oldArr.GetLength(i));
                }

                Array newArr;
                if (sufficient)
                {
                    newArr = oldArr;
                }
                else
                {
                    newArr = Array.CreateInstance(type, shape);
                    CopyArray(oldArr, newArr, new int[array.Rank], new int[array.Rank],true);
                }

                if (origin == null)
                    origin = new int[array.Rank];
                CopyArray(a, newArr, new int[origin.Length], origin, true);
                array = newArr;
            }

            return;// shape;
        }

		#region Utilities

		private bool EqualToShape(int[] shape)
		{
			if (array == null)
				return shape == null || IsZero(shape);

			for (int i = 0; i < rank; i++)
			{
				if (shape[i] != array.GetLength(i))
					return false;
			}
			return true;
		}

        private void CopyArray(Array src, Array dst, int[] srcOrigin, int[] dstOrigin, bool shapeFromSrc)
        {
            if (src.Length == 0)
                return;

            if (src.Rank == 1)
            {
                int shape = shapeFromSrc ? src.GetLength(0) : dst.GetLength(0);
                Array.Copy(src, srcOrigin[0], dst, dstOrigin[0], shape);
                return;
            }

            Type type = src.GetType().GetElementType();

            if (type == typeof(DateTime) ||
                type == typeof(bool) ||
                type == typeof(string))
            {
                CopyArrayBase(src, dst, srcOrigin, dstOrigin, shapeFromSrc);
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
                    int sizeOfType = GetSizeOfType(type);

                    int size = shapeFromSrc ?
                        Buffer.ByteLength(src) :
                        Buffer.ByteLength(dst);

                    int originCubeSize = 1;

                    for (int i = 1; i < rank; i++)
                    {
                        originCubeSize *= srcShape[i];
                    }

                    originCubeSize *= sizeOfType;

                    originCubeSize *= shapeFromSrc ?
                        srcOrigin[0] :
                        dstOrigin[0];

                    size -= originCubeSize;

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
                //Copy bytes line by line.
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

                    int copyBlockLength = shapeFromSrc ?
                        srcShape[src.Rank - 1] * sizeOfType :
                        dstShape[dst.Rank - 1] * sizeOfType;

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

        /// <summary>
        /// Base realization of Copy array and the slowest one
        /// </summary>
        private void CopyArrayBase(Array src, Array dst, int[] srcOrigin, int[] dstOrigin, bool shapeFromSrc)
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
                    if (shapeFromSrc ? srcPos[dim] < srcShape[dim] : dstPos[dim] < dstShape[dim])
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

		private bool IsZero(int[] origin)
		{
			for (int i = 0; i < origin.Length; i++)
			{
				if (origin[i] != 0) return false;
			}
			return true;
		}

		#endregion

		#endregion


        /// <summary>Creates a copy of ArrayWrapper instance</summary>
        /// <returns>Full copy of ArrayWrapper</returns>
		public virtual ArrayWrapper Copy()
		{
			ArrayWrapper aw = new ArrayWrapper(rank, type);
			if (array != null)
				aw.PutData(null, array);

			return aw;
		}
	}
}
