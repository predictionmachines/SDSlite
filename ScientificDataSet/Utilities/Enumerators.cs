// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Research.Science.Data.Imperative;

namespace Microsoft.Research.Science.Data.Utilities
{
    public struct Triple<T1, T2, T3>
    {
        T1 first;
        T2 second;
        T3 third;

        public Triple(T1 first, T2 second, T3 third)
        {
            this.first = first;
            this.second = second;
            this.third = third;
        }
    }

    /// <summary>Enumerates table of three variables on one dimension. Converts
    /// each three values to array of three doubles. Triples with missing values
    /// or NaN are ignored</summary>
    public class PointSetEnumerator : IEnumerable, IEnumerable<double[]>
    {
        const int DataIdx = 2; // Index of data variable in sources

        private Variable[] sources;
        private Func<object[], double[]> func;
        private double min = 0.0;
        private double max = 1.0;
        private Array cachedDataVar;

        public PointSetEnumerator(Variable y, Variable x, Variable data, Func<object[], double[]> func)
        {
            // TODO: Add checks for prerequisites
            if (y.Rank != 1)
                throw new ArgumentException("Variable y should be 1D");
            if (x.Rank != 1)
                throw new ArgumentException("Variable x should be 1D");
            if (data.Rank != 1)
                throw new ArgumentException("Variable y should be 1D");
            if (y.Dimensions[0].Name != x.Dimensions[0].Name ||
                x.Dimensions[0].Name != data.Dimensions[0].Name)
                throw new ArgumentException("All three variables should be defined on the same dimension");
            this.sources = new Variable[] { y, x, data };
            this.func = func;
            RefreshMinMax();
        }

        public double Min
        {
            get { return min; }
        }

        public double Max
        {
            get { return max; }
        }

        public void RefreshMinMax()
        {
            double min = double.MaxValue, max = double.MinValue;

            var metadata = sources[DataIdx].Metadata;
            object _min = metadata.GetMin();
            object _max = metadata.GetMax();

            cachedDataVar = null;

            if (_min == null) // Min is not specified in metadata
            {
                cachedDataVar = sources[DataIdx].GetData();
                RefreshMinMax(cachedDataVar, 0, cachedDataVar.Length, ref min, ref max);
                if (_max == null) _max = max;
            }
            else
                min = Convert.ToDouble(_min);

            if (_max == null) // Max is not specified in metadata
            {
                double fakeMin = min;
                if (cachedDataVar == null)
                    cachedDataVar = sources[DataIdx].GetData();
                RefreshMinMax(cachedDataVar, 0, cachedDataVar.Length, ref fakeMin, ref max);
            }
            else
                max = Convert.ToDouble(_max);

            this.min = min;
            this.max = max;
        }

        private void RefreshMinMax(Array data, int startIndex, int count, ref double min, ref double max)
        {
            object mv = sources[2].GetMissingValue();
            if (mv == null)
                for (int i = startIndex; i < startIndex + count; i++)
                {
                    double v = Convert.ToDouble(data.GetValue(i));
                    if (Double.IsNaN(v))
                        continue; // Ignore NaN elements 
                    if (v < min)
                        min = v;
                    if (v > max)
                        max = v;
                }
            else
            {
                double dmv = Convert.ToDouble(mv);
                for (int i = startIndex; i < startIndex + count; i++)
                {
                    double v = Convert.ToDouble(data.GetValue(i));
                    if (v == dmv || Double.IsNaN(v))
                        continue;  // Ignore NaN elements 
                    if (v < min)
                        min = v;
                    if (v > max)
                        max = v;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<double[]>)this).GetEnumerator();
        }

        IEnumerator<double[]> IEnumerable<double[]>.GetEnumerator()
        {
            object[] row = new object[sources.Length];
            Array[] data = new Array[sources.Length];

            data[0] = sources[0].GetData();
            int len = data[0].Length;
            for (int i = 1; i < sources.Length; i++)
            {
                if (i == 1)
                    data[i] = sources[i].GetData();
                else // i == 2
                {
                    if (cachedDataVar == null)
                        cachedDataVar = sources[2].GetData();
                    data[2] = cachedDataVar;
                }
                if (data[i].Length < len)
                    len = data[i].Length;
            }

            for (int i = 0; i < len; i++)
            {
                bool isValid = true;
                for (int j = 0; j < sources.Length && isValid; j++)
                {
                    object obj = data[j].GetValue(i);
                    if (obj.Equals(sources[j].GetMissingValue()))
                        isValid = false;
                    else
                        row[j] = obj;
                }
                if (isValid)
                {
                    double[] triplet = func(row);
                    if(!Double.IsNaN(triplet[0]) &&
                       !Double.IsNaN(triplet[1]) &&
                       !Double.IsNaN(triplet[2]))
                        yield return triplet;
                }
            }
            cachedDataVar = null; // no need to keep the copy in memory
            yield break;
        }
    }

    public class MissingValueEnumeratorForDims : IEnumerable, IEnumerable<Point>
    {
        private Variable source;
        private int[] dims;

        public MissingValueEnumeratorForDims(Variable source)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (source.Rank != 1) throw new ArgumentException("Source array's rank != 1");

            this.dims = new int[source.Rank];
            for (int i = 0; i < this.dims.Length; i++)
                this.dims[i] = -1;
            this.source = source;
        }

        public MissingValueEnumeratorForDims(Variable source, int[] dims)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (source.Rank != dims.Length) throw new ArgumentException("Array's length != variable's rank");
            
            this.dims = new int[source.Rank];
            for (int i = 0; i < this.dims.Length; i++)
                this.dims[i] = dims[i];
            this.source = source;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Array array = GetData();
            object mv = source.GetMissingValue();

            Type type = source.TypeOfData;
            if (type == typeof(double))
            {
                double[] darr = (double[])array;
                for (int i = 0; i < array.Length; i++)
                {
                    double d = darr[i];
                    if (!d.Equals(mv) && !double.IsNaN(d))
                        yield return new Point(i, d);
                }
            }
            else if (type == typeof(float))
            {
                float[] farr = (float[])array;
                for (int i = 0; i < array.Length; i++)
                {
                    float f = farr[i];
                    if (!f.Equals(mv) && !float.IsNaN(f))
                        yield return new Point(i, (double)f);
                }
            }
            else
            {
                for (int i = 0; i < array.Length; i++)
                {
                    object t = array.GetValue(i);
                    if (!t.Equals(mv))
                        yield return new Point(i, Convert.ToDouble(t));
                }
            }
            yield break;
        }

        IEnumerator<Point> IEnumerable<Point>.GetEnumerator()
        {
            Array array = GetData();
            object mv = source.GetMissingValue();

            Type type = source.TypeOfData;
            if (type == typeof(double))
            {
                double[] darr = (double[])array;
                for (int i = 0; i < array.Length; i++)
                {
                    double d = darr[i];
                    if (!d.Equals(mv) && !double.IsNaN(d))
                        yield return new Point(i, d);
                }
            }
            else if (type == typeof(float))
            {
                float[] farr = (float[])array;
                for (int i = 0; i < array.Length; i++)
                {
                    float f = farr[i];
                    if (!f.Equals(mv) && !float.IsNaN(f))
                        yield return new Point(i, (double)f);
                }
            }
            else
            {
                for (int i = 0; i < array.Length; i++)
                {
                    object t = array.GetValue(i);
                    if (!t.Equals(mv))
                        yield return new Point(i, Convert.ToDouble(t));
                }
            }
            yield break;
        }

        private Array GetData()
        {
            int rank = source.Rank;

            int[] start = new int[rank];
            int[] stride = new int[rank];
            int[] count = new int[rank];
            for (int i = 0; i < rank; i++)
                if (dims[i] != -1)
                {
                    start[i] = dims[i];
                    stride[i] = 1;
                    count[i] = 1;
                }
                else
                {
                    start[i] = 0;
                    stride[i] = 1;
                    count[i] = source.Dimensions[i].Length;
                }
            Array array = source.GetData(start, stride, count);
            Type type = TypeUtils.GetElementType(array);
            Array res = Array.CreateInstance(type, array.Length);
            int j = 0;
            foreach (var x in array)
            {
                res.SetValue(x, j);
                j++;
            }
            return res;
        }
    }

    public class MissingValueEnumeratorFor1dCS : IEnumerable, IEnumerable<Point>
    {
        private Variable grid;
        private Variable data;
        private Func<object, double> gridValueConverter;

        public MissingValueEnumeratorFor1dCS(Variable grid, Variable data, Func<object, double> gridValueConverter)
        {
            if (grid == null) throw new ArgumentNullException("grid");
            if (grid.Rank != 1) throw new ArgumentException("grid array's rank != 1");
            if (data == null) throw new ArgumentNullException("data");
            if (data.Rank != 1) throw new ArgumentException("data array's rank != 1");
            if (gridValueConverter == null) throw new ArgumentNullException("gridValueConverter");

            this.grid = grid;
            this.data = data;
            this.gridValueConverter = gridValueConverter;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetPoints();
        }

        IEnumerator<Point> IEnumerable<Point>.GetEnumerator()
        {
            return GetPoints();
        }

        private IEnumerator<Point> GetPoints()
        {
            Array gridA = grid.GetData();
            Array dataA = data.GetData();
            object gmv = grid.GetMissingValue();
            object dmv = data.GetMissingValue();

            int len = Math.Min(gridA.Length, dataA.Length);

            Type gt = grid.TypeOfData;
            Type dt = data.TypeOfData;

            if (dt == typeof(double))
            {
                if (gt == typeof(double)) // data: double, grid: double
                {
                    double[] darr = (double[])dataA;
                    double[] garr = (double[])gridA;
                    for (int i = 0; i < len; i++)
                    {
                        double g = garr[i];
                        double d = darr[i];

                        if (g.Equals(gmv) || double.IsNaN(g))
                            continue;
                        if (d.Equals(dmv) || double.IsNaN(d))
                            continue;
                        yield return new Point(gridValueConverter(g), d);
                    }
                }
                else if (gt == typeof(float)) // data: double, grid: float
                {
                    double[] darr = (double[])dataA;
                    float[] garr = (float[])gridA;
                    for (int i = 0; i < len; i++)
                    {
                        float g = garr[i];
                        double d = darr[i];

                        if (g.Equals(gmv) || float.IsNaN(g))
                            continue;
                        if (d.Equals(dmv) || double.IsNaN(d))
                            continue;
                        yield return new Point(gridValueConverter(g), d);
                    }
                }
                else  // data: double, grid: other
                {
                    double[] darr = (double[])dataA;
                    for (int i = 0; i < len; i++)
                    {
                        object g = gridA.GetValue(i);
                        if (g.Equals(gmv))
                            continue;
                        double d = darr[i];
                        if (d.Equals(dmv) || double.IsNaN(d))
                            continue;
                        yield return new Point(gridValueConverter(g), d);
                    }
                }
            }
            else if (dt == typeof(float))
            {
                if (gt == typeof(double))  // data: float, grid: double
                {
                    float[] darr = (float[])dataA;
                    double[] garr = (double[])gridA;
                    for (int i = 0; i < len; i++)
                    {
                        double g = garr[i];
                        float d = darr[i];

                        if (g.Equals(gmv) || double.IsNaN(g))
                            continue;
                        if (d.Equals(dmv) || float.IsNaN(d))
                            continue;
                        yield return new Point(gridValueConverter(g), (double)d);
                    }
                }
                else if (gt == typeof(float)) // data: float, grid: float
                {
                    float[] darr = (float[])dataA;
                    float[] garr = (float[])gridA;
                    for (int i = 0; i < len; i++)
                    {
                        float g = garr[i];
                        float d = darr[i];

                        if (g.Equals(gmv) || float.IsNaN(g))
                            continue;
                        if (d.Equals(dmv) || float.IsNaN(d))
                            continue;
                        yield return new Point(gridValueConverter(g), (double)d);
                    }
                }
                else  // data: float, grid: other
                {
                    float[] darr = (float[])dataA;
                    for (int i = 0; i < len; i++)
                    {
                        object g = gridA.GetValue(i);
                        if (g.Equals(gmv))
                            continue;
                        float d = darr[i];
                        if (d.Equals(dmv) || float.IsNaN(d))
                            continue;
                        yield return new Point(gridValueConverter(g), (double)d);
                    }
                }
            }
            else
            {
                if (gt == typeof(double))  // data: other, grid: double
                {
                    double[] garr = (double[])gridA;
                    for (int i = 0; i < len; i++)
                    {
                        double g = garr[i];

                        if (g.Equals(gmv) || double.IsNaN(g))
                            continue;
                        object d = dataA.GetValue(i);
                        if (d.Equals(dmv))
                            continue;
                        yield return new Point(gridValueConverter(g), Convert.ToDouble(d));
                    }
                }
                else if (gt == typeof(float))  // data: other, grid: float
                {
                    float[] garr = (float[])gridA;
                    for (int i = 0; i < len; i++)
                    {
                        float g = garr[i];

                        if (g.Equals(gmv) || float.IsNaN(g))
                            continue;
                        object d = dataA.GetValue(i);
                        if (d.Equals(dmv))
                            continue;
                        yield return new Point(gridValueConverter(g), Convert.ToDouble(d));
                    }
                }
                else  // data: other, grid: other
                {
                    for (int i = 0; i < len; i++)
                    {
                        object g = gridA.GetValue(i);
                        if (g.Equals(gmv))
                            continue;
                        object d = dataA.GetValue(i);
                        if (d.Equals(dmv))
                            continue;
                        yield return new Point(gridValueConverter(g), Convert.ToDouble(d));
                    }
                }
            }
            yield break;
        }
    }
}

