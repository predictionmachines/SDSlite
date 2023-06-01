// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Research.Science.Data.Utilities;
using Microsoft.Research.Science.Data;

namespace Microsoft.Research.Science.Data.Utilities
{
    /// <summary>
    /// Contains methods to clone datasets.
    /// </summary>
    public static class DataSetCloning
    {
        /// <summary>
        /// Allows to show progress update.
        /// </summary>
        /// <param name="percent"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public delegate bool ProgressUpdater(double percent, string message);


        /// <summary>
        /// Copies given dataset into dataset determined by <paramref name="dstUri"/>.
        /// </summary>
        /// <param name="src">Original dataset to clone.</param>
        /// <param name="dstUri">URI of the destination dataset.</param>
        /// <param name="updater">Delegate accepting update progressm notifications.</param>
        /// <returns>New instance of <see cref="DataSet"/> class.</returns>
        /// <remarks>
        /// This method splits the original dataser into parts and therefore is able
        /// to clone very large datasets not fitting to memory.
        /// </remarks>
        public static DataSet Clone(DataSet src, DataSetUri dstUri, ProgressUpdater updater = null)
        {
            DataSet dst = null;
            try
            {
                dst = DataSet.Open(dstUri);
                return Clone(src, dst, updater ?? DefaultUpdater);
            }
            catch
            {
                if (dst != null) dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Copies given dataset into another dataset.
        /// </summary>
        /// <param name="src">Original dataset to copy.</param>
        /// <param name="dst">Destination dataset.</param>
        /// <param name="updater">Delegate accepting update progressm notifications.</param>
        /// <returns>New instance of <see cref="DataSet"/> class.</returns>
        /// <remarks>
        /// This method splits the original dataser into parts and therefore is able
        /// to clone very large datasets not fitting to memory.
        /// </remarks>
        public static DataSet Clone(DataSet src, DataSet dst, ProgressUpdater updater)
        {
            if (src == null) throw new ArgumentNullException("src");
            if (dst == null) throw new ArgumentNullException("dst");
            if (dst.IsReadOnly)
                throw new NotSupportedException("Destination DataSet is read-only");

            // Maximum memory capacity in bytes
            ulong N = 200 * 1024 * 1024;
            // Estimated size of a single string in bytes
            int sizeofString = 100 * 1024;

            /***********************************************************************************
             * Preparing output
            ***********************************************************************************/
            bool isAutoCommit = dst.IsAutocommitEnabled;
            try
            {
                dst.IsAutocommitEnabled = false;

                DataSetSchema srcSchema = src.GetSchema();
                Dictionary<int, int> IDs = new Dictionary<int, int>();

                // Creating empty variables and copying global metadata and scalar variables
                if (updater != null)
                    updater(0, "Creating structure and copying global metadata and scalar variables...");
                VariableSchema globalMetadataVar = null;
                foreach (VariableSchema v in srcSchema.Variables)
                {
                    if (v.ID == DataSet.GlobalMetadataVariableID)
                    {
                        globalMetadataVar = v;
                        continue;
                    }

                    Variable t = dst.AddVariable(v.TypeOfData, v.Name, null, v.Dimensions.AsNamesArray());
                    IDs.Add(v.ID, t.ID);

                    foreach (var attr in v.Metadata)
                        t.Metadata[attr.Key] = attr.Value;

                    if (t.Rank == 0) // scalar
                        t.PutData(src.Variables.GetByID(v.ID).GetData());
                }
                if (globalMetadataVar != null)
                {
                    // Copying global metadata
                    foreach (var attr in globalMetadataVar.Metadata)
                        dst.Metadata[attr.Key] = attr.Value;
                }
                dst.Commit();
                // Console.Out.WriteLine("Done.\n");
                /***********************************************************************************
                 * Adjusting dimensions deltas
                ***********************************************************************************/
                Dimension[] srcDims = srcSchema.GetDimensions();
                Dictionary<string, int> deltas = new Dictionary<string, int>(srcDims.Length);
                foreach (var d in srcDims)
                    deltas[d.Name] = d.Length;

                // Console.Out.WriteLine("Total memory capacity: " + (N / 1024.0 / 1024.0).ToString("F2") + " Mb");
                ulong totalSize;
                do
                {
                    totalSize = 0;
                    foreach (var var in srcSchema.Variables)
                    {
                        if (var.Rank == 0) continue; // scalar
                        int typeSize = SizeOf(var.TypeOfData, sizeofString);

                        ulong count = 0;
                        foreach (var vdim in var.Dimensions)
                        {
                            int dimDelta = deltas[vdim.Name];
                            if (count == 0) count = (ulong)dimDelta;
                            else count *= (ulong)dimDelta;
                        }
                        totalSize += (ulong)typeSize * count;
                    }
                    if (totalSize > N)
                    {
                        string maxDim = null;
                        int max = int.MinValue;
                        foreach (var dim in deltas)
                            if (dim.Value > max)
                            {
                                max = dim.Value;
                                maxDim = dim.Key;
                            }
                        if (maxDim == null || max <= 1)
                            throw new NotSupportedException("Cannot copy the DataSet: it is too large to be copied entirely by the utility for the provided memory capacity");
                        deltas[maxDim] = max >> 1;
                    }
                } while (totalSize > N);

                // Printing deltas
                if (updater != null) updater(0, String.Format("Deltas for the dimensions adjusted (max iteration capacity: " + (totalSize / 1024.0 / 1024.0).ToString("F2") + " Mb)"));

                /***********************************************************************************
                 * Copying data
                ***********************************************************************************/
                // Console.WriteLine();
                if (updater != null) updater(0, "Copying data ...");
                Dictionary<int, int[]> origins = new Dictionary<int, int[]>(srcSchema.Variables.Length);
                Dictionary<int, int[]> shapes = new Dictionary<int, int[]>(srcSchema.Variables.Length);
                List<VariableSchema> copyVars = srcSchema.Variables.Where(vs =>
                    (vs.Rank > 0 && vs.ID != DataSet.GlobalMetadataVariableID)).ToList();

                Dictionary<string, int> dimOrigin = new Dictionary<string, int>(srcDims.Length);
                foreach (var d in srcDims)
                    dimOrigin[d.Name] = 0;

                Array.Sort(srcDims, (d1, d2) => d1.Length - d2.Length);
                int totalDims = srcDims.Length;

                do
                {
                    // for each variable:
                    for (int varIndex = copyVars.Count; --varIndex >= 0; )
                    {
                        VariableSchema var = copyVars[varIndex];
                        bool hasChanged = false;
                        // Getting its origin
                        int[] origin;
                        if (!origins.TryGetValue(var.ID, out origin))
                        {
                            origin = new int[var.Rank];
                            origins[var.ID] = origin;
                            hasChanged = true;
                        }
                        // Getting its shape
                        int[] shape;
                        if (!shapes.TryGetValue(var.ID, out shape))
                        {
                            shape = new int[var.Rank];
                            for (int i = 0; i < var.Dimensions.Count; i++)
                                shape[i] = deltas[var.Dimensions[i].Name];
                            shapes.Add(var.ID, shape);
                        }

                        // Updating origin for the variable:
                        if (!hasChanged)
                            for (int i = 0; i < shape.Length; i++)
                            {
                                int o = dimOrigin[var.Dimensions[i].Name];
                                if (origin[i] != o)
                                {
                                    hasChanged = true;
                                    origin[i] = o;
                                }
                            }
                        if (!hasChanged) // this block is already copied
                            continue;

                        bool doCopy = false;
                        bool shapeUpdated = false;
                        for (int i = 0; i < shape.Length; i++)
                        {
                            int s = origin[i] + shape[i];
                            int len = var.Dimensions[i].Length;
                            if (s > len)
                            {
                                if (!shapeUpdated)
                                {
                                    shapeUpdated = true;
                                    shape = (int[])shape.Clone();
                                }
                                shape[i] = len - origin[i];
                            }
                            if (shape[i] > 0) doCopy = true;
                        }

                        if (doCopy)
                        {
                            Array data = src.Variables.GetByID(var.ID).GetData(origin, shape);
                            // Compute real size here for strings
                            dst.Variables.GetByID(IDs[var.ID]).PutData(origin, data);
                        }
                        else // variable is copied
                        {
                            copyVars.RemoveAt(varIndex);
                        }
                    }
                    dst.Commit();

                    // Updating dimensions origin
                    bool isOver = true;
                    for (int i = 0; i < totalDims; i++)
                    {
                        Dimension dim = srcDims[i];
                        int origin = dimOrigin[dim.Name] + deltas[dim.Name];
                        if (origin < dim.Length)
                        {
                            dimOrigin[dim.Name] = origin;
                            isOver = false;
                            // Progress indicator
                            if (i == totalDims - 1)
                            {
                                double perc = (double)origin / dim.Length * 100.0;
                                if (updater != null) updater(perc, "Copying data ...");
                            }
                            break;
                        }
                        dimOrigin[dim.Name] = 0;
                    }
                    if (isOver) break;
                } while (copyVars.Count > 0);

                if (updater != null) updater(100.0, "Done.");
            }
            finally
            {
                dst.IsAutocommitEnabled = isAutoCommit;
            }

            return dst;
        }

        /// <summary>
        /// Copies given dataset into dataset determined by <paramref name="dstUri"/> and prints progress into console.
        /// </summary>
        /// <param name="src">Original dataset to clone.</param>
        /// <param name="dstUri">URI of the destination dataset.</param>
        /// <returns>New instance of <see cref="DataSet"/> class.</returns>
        /// <remarks><para>
        /// This method splits the original dataser into parts and therefore is able
        /// to clone very large datasets not fitting to memory.</para>
        /// <para>Progress is printed out into the console window.</para>
        /// </remarks>
        /// <seealso cref="DataSetCloning.Clone(DataSet,DataSetUri,ProgressUpdater)"/>
        public static DataSet Clone(DataSet src, DataSetUri dstUri)
        {
            return Clone(src, dstUri, DefaultUpdater);
        }

        private static bool DefaultUpdater(double perc, string message)
        {
            for (int i = 0; i < 80; i++)
                Console.Write("\b");
            Console.WriteLine("{0,3:F0}%: {1}", perc, message);
            return true;
        }

        private static int SizeOf(Type type, int sizeofString)
        {
            if (type == typeof(Double)) return sizeof(Double);
            if (type == typeof(Single)) return sizeof(Single);
            if (type == typeof(Int16)) return sizeof(Int16);
            if (type == typeof(Int32)) return sizeof(Int32);
            if (type == typeof(Int64)) return sizeof(Int64);
            if (type == typeof(UInt64)) return sizeof(UInt64);
            if (type == typeof(UInt32)) return sizeof(UInt32);
            if (type == typeof(UInt16)) return sizeof(UInt16);
            if (type == typeof(Byte)) return sizeof(Byte);
            if (type == typeof(SByte)) return sizeof(SByte);
            if (type == typeof(String)) return sizeofString;
            if (type == typeof(Boolean)) return sizeof(Boolean);
            if (type == typeof(DateTime)) return sizeof(long);
            if (type == typeof(EmptyValueType)) return 1;
            return Marshal.SizeOf(type);
        }
    }
}

