// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data
{
    /// <summary>
    /// Represents non-negative integer range with stride.
    /// </summary>
    /// <remarks>
    /// <para>An instance of the <see cref="Range"/> struct can be instantiated
    /// using static methods of the <see cref="DataSet"/> class:
    /// <list type="table">
    /// <item><term><see cref="DataSet.Range(int)"/></term>
    /// <description>Single index to choose.</description></item>
    /// <item><term><see cref="DataSet.Range(int,int)"/></term>
    /// <description>A range of indices from "from" up to "to".</description></item>
    /// <item><term><see cref="DataSet.Range(int,int,int)"/></term>
    /// <description>A range of indices with specified step.</description></item>
    /// <item><term><see cref="DataSet.FromToEnd(int)"/></term>
    /// <description>A range of indices from "from" up to the maximum index for a given dimension.</description></item>
    /// <item><term><see cref="DataSet.FromToEnd(int,int)"/></term>
    /// <description>A range of indices from "from" up to the maximum index for a given dimension with the specified step.</description></item>
    /// <item><term><see cref="DataSet.ReduceDim(int)"/></term>
    /// <description>Single index to choose and reduce this dimension so that the rank of the resulting array is less than the rank of the variable is read from.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Ranges are used in the procedural API available as extensions methods for the <see cref="DataSet"/>
    /// class. See <see cref="T:Microsoft.Research.Science.Data.Imperative.DataSetExtensions"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="T:Microsoft.Research.Science.Data.Imperative.DataSetExtensions"/>
    public struct Range
    {
        private int origin;
        private int stride;
        private int count;
        private bool reduce;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="stride"></param>
        /// <param name="count"></param>
        /// <remarks>
        /// <para>
        /// Negative count means that it is unlimited.
        /// </para>
        /// </remarks>
        internal Range(int origin, int stride, int count)
        {
            if (origin < 0) throw new ArgumentException("origin is negative");
            if (stride <= 0) throw new ArgumentException("stride is not positive");
            this.origin = origin;
            this.stride = stride;
            if (count < 0) count = -1; // unlimited
            this.count = count;
            this.reduce = false;
        }

        internal Range(int index, bool reduce)
        {
            if (index < 0) throw new ArgumentException("index is negative");
            this.origin = index;
            this.stride = 1;
            this.count = 1;
            this.reduce = reduce;
        }

        /// <summary>
        /// Gets the value indicating that the related dimension is to be reduced.
        /// </summary>
        public bool IsReduced { get { return reduce; } }
        /// <summary>
        /// Gets the starting value of the range.
        /// </summary>
        public int Origin { get { return origin; } }
        /// <summary>
        /// Gets the stride value.
        /// </summary>
        public int Stride { get { return stride; } }
        /// <summary>
        /// Gets the number of values in the range.
        /// </summary>
        /// <remarks>
        /// <para>If the range <see cref="IsUnlimited"/>, gets -1.</para>
        /// </remarks>
        public int Count { get { return count; } }
        /// <summary>
        /// Gets the last value of the range.
        /// </summary>
        /// <remarks>
        /// <para>If the range <see cref="IsUnlimited"/>, the property throws an exception.</para>
        /// </remarks>
        public int Last
        {
            get
            {
                if (count < 0) throw new NotSupportedException("Range is unlimited");
                return origin + stride * (count - 1);
            }
        }

        /// <summary>
        /// Gets the value indicating whether the range is unlimited.
        /// </summary>
        public bool IsUnlimited
        {
            get { return count < 0; }
        }

        /// <summary>
        /// Gets the value indicating that the range contains no values.
        /// </summary>
        public bool IsEmpty
        {
            get { return count == 0; }
        }

        /// <summary>
        /// Gets the string representation of the range.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// It is either "(empty)" or "([start]:[stride]:[final])"
        /// </remarks>
        public override string ToString()
        {
            if (IsEmpty) return "(empty)";
            if (count == 1)
                if (reduce) return origin.ToString() + " (reduce)";
                else return origin.ToString();
            return String.Format("({0}:{1}:{2})", origin, stride, IsUnlimited ? String.Empty : Last.ToString());
        }
        /// <summary>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Range)) return false;

            Range r = (Range)obj;
            if (count != r.count) return false;
            if (count == 0) return true;
            if (count == 1) return origin == r.origin && reduce == r.reduce;
            return origin == r.origin && stride == r.stride && count == r.count;
        }
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            if (count == 0) return 0;
            if (count == 1) return (origin + 1);
            return (origin + 1) ^ stride ^ count;
        }
    }
}

