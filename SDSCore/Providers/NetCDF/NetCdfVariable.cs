// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using NetCDFInterop;

namespace Microsoft.Research.Science.Data.NetCDF4
{
    /// <summary>
    /// Provides an access to NetCDF 4.0 variables.
    /// </summary>
    internal class NetCdfVariable<DataType> : DataAccessVariable<DataType>, INetCDFVariable
    {
        #region Private fields

        /// <summary>Variable's id in terms of the NetCDF file.</summary>
        private int varid;
        /// <summary>Variable's dimensions ids in terms of the NetCDF file.</summary>
        private int[] dimids;

        private bool initializing = true;

        private bool isPresentedAttrActualShape = false;

        #endregion

        #region Constructors

        protected NetCdfVariable(NetCDFDataSet dataSet, int varid, string name, string[] dims, bool assignID) :
            base(dataSet, name, dims, assignID)
        {
            this.initializing = true;
            this.varid = varid;

            int res;
            UpdateDimIds();

            // Reading attributes
            int nattrs;
            res = NetCDF.nc_inq_varnatts(dataSet.NcId, varid, out nattrs);
            AttributeTypeMap atm = new AttributeTypeMap(dataSet.NcId, varid);
            NetCDFDataSet.HandleResult(res);
            for (int i = 0; i < nattrs; i++)
            {
                // Name
                string aname;
                res = NetCDF.nc_inq_attname(dataSet.NcId, varid, i, out aname);
                NetCDFDataSet.HandleResult(res);

                // Skip out internal attribute
                if (aname == AttributeTypeMap.AttributeName ||
                    aname == AttributeTypeMap.AttributeVarSpecialType)
                    continue;
                if (aname == AttributeTypeMap.AttributeVarActualShape)
                {
                    isPresentedAttrActualShape = true;
                    continue;
                }

                // Type
                object value = dataSet.ReadNetCdfAttribute(varid, aname, atm);
                if (aname == Metadata.KeyForMissingValue && typeof(DateTime) == typeof(DataType))
                {
                    if (value is string)
                    {
                        value = DateTime.Parse((string)value);
                    }
                }
                Metadata[aname] = value;
            }

            Initialize();
            initializing = false;
        }

        private NetCdfVariable(NetCDFDataSet dataSet, int varid, string name, string[] dims) :
            this(dataSet, varid, name, dims, true)
        {
        }

        #endregion

        #region Initialization

        ///<summary>
        /// Getting dimensions' ids from the NetCDF file.
        /// </summary>
        private void UpdateDimIds()
        {
            if (varid == NcConst.NC_GLOBAL)
            {
                this.dimids = new int[0];
                return;
            }
            int[] dimids = new int[Rank];
            int res = NetCDF.nc_inq_vardimid(NcId, varid, dimids);
            NetCDFDataSet.HandleResult(res);
            this.dimids = dimids;
        }

        void INetCDFVariable.UpdateDimiIds()
        {
            UpdateDimIds();
        }

        internal static NetCdfVariable<DataType> Create(NetCDFDataSet dataSet, string name, string[] dims)
        {
            if (dims == null) // scalar variable
                dims = new string[0];

            int res;

            /* Getting exsting dims and creating new */
            int[] dimids = new int[dims.Length];
            for (int i = 0; i < dims.Length; i++)
            {
                int id;
                res = NetCDF.nc_inq_dimid(dataSet.NcId, dims[i], out id);
                if (res == (int)ResultCode.NC_NOERR)
                {
                    dimids[i] = id;
                }
                else if (res == (int)ResultCode.NC_EBADDIM)
                {
                    // Creating new dimension
                    res = NetCDF.nc_def_dim(dataSet.NcId, dims[i], new IntPtr(NcConst.NC_UNLIMITED), out id);
                    NetCDFDataSet.HandleResult(res);
                    dimids[i] = id;
                }
                else
                {
                    NetCDFDataSet.HandleResult(res);
                }
            }

            /* Creating variable */
            int varid;

            string nc_name = GetNetCDFName(name);
            if (nc_name != name)
                Debug.WriteLineIf(NetCDFDataSet.TraceNetCDFDataSet.TraceVerbose, "NetCDF: name has illegal chars; internal variable name is " + nc_name);

            res = NetCDF.nc_def_var(dataSet.NcId, nc_name, NetCDF.GetNcType(typeof(DataType)), dimids, out varid);
            for (int count = 1; res == (int)ResultCode.NC_ENAMEINUSE; )
            {
                // Keep trying to create variable with different names
                res = NetCDF.nc_def_var(dataSet.NcId, String.Format("{0}_{1}", nc_name, count++), NetCDF.GetNcType(typeof(DataType)), dimids, out varid);
            }
            NetCDFDataSet.HandleResult(res);

            // Non scalar variables can be compressed.
            // It is regulated by the uri parameter "deflate".
            if (dims.Length > 0)
            {
                if (dataSet.Deflate == DeflateLevel.Off)
                    res = NetCDF.nc_def_var_deflate(dataSet.NcId, varid, 0, 0, 0);
                else
                    res = NetCDF.nc_def_var_deflate(dataSet.NcId, varid, 0, 1, (int)dataSet.Deflate);
                NetCDFDataSet.HandleResult(res);
            }

            if (dims.Length > 0) // non-scalar variable
            {
                IntPtr[] chunksizes = null;
                if (dataSet.ChunkSizes != null && dataSet.ChunkSizes.Length == dims.Length)
                    if (dataSet.ChunkSizes.All(sz => sz >= 1))
                        chunksizes = dataSet.ChunkSizes.Select(sz => new IntPtr(sz)).ToArray();
                    else throw new NotSupportedException("Chunk size cannot be less than 1");

                if (chunksizes == null) // then use default chunk sizes
                {
                    chunksizes = new IntPtr[dims.Length];
                    int chunk = ChunkSizeSelector.GetChunkSize(typeof(DataType), dims.Length);

#if DEBUG
                    if (NetCDFDataSet.TraceNetCDFDataSet.TraceInfo)
                    {
                        StringBuilder sbChunk = new StringBuilder();
                        sbChunk.Append("NetCDF: Chunk sizes for rank ").Append(dims.Length).Append(": ");
                        for (int i = 0; i < dims.Length; i++)
                            sbChunk.Append(chunk).Append(",");
                        Debug.WriteLineIf(NetCDFDataSet.TraceNetCDFDataSet.TraceInfo, sbChunk.ToString());
                    }
#endif

                    for (int i = 0; i < dims.Length; i++)
                    {
                        chunksizes[i] = new IntPtr(chunk);
                    }
                }

                res = NetCDF.nc_def_var_chunking(dataSet.NcId, varid, 0, chunksizes);
                NetCDFDataSet.HandleResult(res);
            }

            if (typeof(DataType) == typeof(DateTime))
            {
                NetCDFDataSet.NcPutAttText(dataSet.NcId, varid, "Units", NetCDFDataSet.dateTimeUnits);
            }
            else if (typeof(DataType) == typeof(bool))
            {
                NetCDFDataSet.NcPutAttText(dataSet.NcId, varid, AttributeTypeMap.AttributeVarSpecialType, typeof(bool).ToString());
            }

            return new NetCdfVariable<DataType>(dataSet, varid, name, dims);
        }

        /// <summary>
        /// Replaces all illegal for NetCDF name chars with '_';
        /// null and empty strings become '_'.
        /// Maximal length: 100 symbols.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string GetNetCDFName(string name)
        {
            if (String.IsNullOrEmpty(name))
                return "_";

            char[] chars = name.ToCharArray(0, Math.Min(100, name.Length));

            int n = chars.Length;
            for (; --n >= 0; )
            {
                if (!char.IsWhiteSpace(chars[n])) break; // removing trailing spaces
            }
            n++;
            if (n == 0) return "_";
            for (int i = 0; i < n; i++)
            {
                char c = chars[i];
                if (c == '/' ||
                        (c >= '\x00' && c <= '\x1F') ||
                        (c >= '\x7F' && c <= '\xFF'))
                    chars[i] = '_';
            }

            if (!(chars[0] >= 'A' && chars[0] <= 'Z' ||
                chars[0] >= 'a' && chars[0] <= 'z' ||
                Char.IsDigit(chars[0]) ||
                chars[0] == '_'))
            {
                // starts with illegal char
                if (n == 255)
                    return '_' + new string(chars, 0, 254);
                else
                    return '_' + new string(chars, 0, n);
            }
            return new string(chars, 0, n);
        }

        private static int Pow10(int p)
        {
            int val = 1;
            for (; --p >= 0; )
                val *= 10;
            return val;
        }

        internal static NetCdfVariable<DataType> Read(NetCDFDataSet dataSet, int varid)
        {
            string name;
            NcType ncType;
            int[] dimIds;
            int natts, res, ndims;

            // Number of dimensions
            res = NetCDF.nc_inq_varndims(dataSet.NcId, varid, out ndims);
            NetCDFDataSet.HandleResult(res);

            dimIds = new int[ndims];

            // About variable
            res = NetCDF.nc_inq_var(dataSet.NcId, varid, out name, out ncType, out ndims, dimIds, out natts);
            NetCDFDataSet.HandleResult(res);

            // Dimensions names
            string[] dims = new string[ndims];
            for (int i = 0; i < ndims; i++)
            {
                string dimName;
                res = NetCDF.nc_inq_dimname(dataSet.NcId, dimIds[i], out dimName);
                NetCDFDataSet.HandleResult(res);

                dims[i] = dimName.ToString();
            }

#if DEBUG
            StringBuilder msg = new StringBuilder();
            for (int i = 0; i < dims.Length; i++)
            {
                msg.Append(dims[i]);
                if (i < dims.Length - 1)
                    msg.Append(',');
            }
            Debug.WriteLineIf(NetCDFDataSet.TraceNetCDFDataSet.TraceInfo, String.Format("Variable {0}: {1} dimensions ({2}), {3} attributes.",
                name, ndims, msg.ToString(), natts));
#endif

            var var = new NetCdfVariable<DataType>(dataSet, varid, name.ToString(), dims);
            if (var.Rank > 0 && var.changes != null)
            {
                // Providing correct "proposed" shape taken from the input file
                int[] shape = var.ReadShape();
                if (shape == null) shape = new int[dims.Length];
                Rectangle ar = new Rectangle(new int[dims.Length], shape);
                DataChanges oldch = var.changes;
                DataChanges newch = new DataChanges(oldch.ChangeSet, oldch.InitialSchema, oldch.MetadataChanges,
                    oldch.CoordinateSystems,
                    shape, ar, oldch.Data);
                var.changes = newch;
            }
            return var;
        }

        #endregion

        #region Properties

        private int NcId
        {
            get { return ((NetCDFDataSet)DataSet).NcId; }
        }

        #endregion

        #region Transactions

        /// <summary>
        /// 
        /// </summary>
        /// <param name="proposedChanges"></param>
        internal protected override void OnPrecommit(Variable.Changes proposedChanges)
        {
            NetCDF.nc_enddef(NcId);
            base.OnPrecommit(proposedChanges);
            if (((NetCDFDataSet)DataSet).Initializing)
                return;
            NetCDF.nc_redef(NcId);

            NetCDFDataSet dataSet = (NetCDFDataSet)this.DataSet;
            AttributeTypeMap atm = new AttributeTypeMap(dataSet.NcId, varid);
            /* Updating attributes in the netCDF file */
            foreach (var item in proposedChanges.MetadataChanges)
                dataSet.WriteNetCdfAttribute(varid, item.Key, item.Value, atm);

            // NetCDF doesn't supports storing of arrays of kind A[x:0, y:10]
            // so we keep this actual shape in the reserved attribute.
            if (Rank > 1)
            {
                int min, max;
                GetMinMax(proposedChanges.Shape, out min, out max);
                if (min == 0 && max > 0)
                {
                    dataSet.WriteNetCdfAttribute(varid, AttributeTypeMap.AttributeVarActualShape,
                        proposedChanges.Shape, atm);
                    isPresentedAttrActualShape = true;
                }
                else if (isPresentedAttrActualShape)
                {
                    NetCDF.nc_del_att(NcId, varid, AttributeTypeMap.AttributeVarActualShape);
                    isPresentedAttrActualShape = false;
                }
            }

            atm.Store();

            /* Saving attached coordinate systems as "coordinates" attributes.
             * Common netCDF model supports for only one such attribute,
             * so all others CS except first are not compatible with that model.
             * Their attribute names are "coordinates2","coordinates3" etc.
             * Names of coordinate systems are provided with corresponed 
             * attributes "coordinatesName","coordinates2Name",...
             */
            if (proposedChanges.CoordinateSystems != null)
            {
                int n = proposedChanges.InitialSchema.CoordinateSystems.Length;

                foreach (var cs in proposedChanges.CoordinateSystems)
                {
                    if (Array.Exists(proposedChanges.InitialSchema.CoordinateSystems, css => css == cs.Name))
                        continue;

                    // This CS has been added.
                    string name = "coordinates";
                    if (n > 0) name += n.ToString();
                    n++;

                    StringBuilder sb = new StringBuilder();
                    bool first = true;
                    foreach (Variable axis in cs.Axes)
                    {
                        if (!first) sb.Append(' ');
                        else first = false;
                        sb.Append(axis.Name);
                    }
                    dataSet.WriteNetCdfAttribute(varid, name, sb.ToString(), null);
                    dataSet.WriteNetCdfAttribute(varid, name + "Name", cs.Name, null);
                }
            }
        }

        #endregion

        #region Utils

        private static T[] FromMultidimArray<T>(Array array)
        {
            if (array == null || array.Length == 0)
                return new T[0];

            if (array.Rank == 1)
            {
                return (T[])array;
            }

            T[] result = new T[array.Length];

            Debug.Assert(Buffer.ByteLength(result) == Buffer.ByteLength(array));
            Buffer.BlockCopy(array, 0, result, 0, Buffer.ByteLength(result));
            return result;
        }

        private static string[] FromMultidimArrayString(Array array)
        {
            if (array == null || array.Length == 0)
                return new string[0];

            if (array.Rank == 1)
            {
                return (string[])array;
            }

            string[] result = new string[array.Length];

            int[] index = new int[array.Rank];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (string)array.GetValue(index);

                for (int j = index.Length; --j >= 0; )
                {
                    index[j]++;
                    if (index[j] < array.GetLength(j))
                        break;
                    index[j] = 0;
                }
            }
            return result;
        }

        private static byte[] FromMultidimArrayBool(Array array)
        {
            if (array == null || array.Length == 0)
                return new byte[0];

            if (array.Rank == 1)
            {
                return Array.ConvertAll((bool[])array, p => p ? (byte)1 : (byte)0);
            }

            byte[] result = new byte[array.Length];
            int[] index = new int[array.Rank];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (bool)array.GetValue(index) ? (byte)1 : (byte)0;

                for (int j = index.Length; --j >= 0; )
                {
                    index[j]++;
                    if (index[j] < array.GetLength(j))
                        break;
                    index[j] = 0;
                }
            }

            return result;
        }

        private static double[] FromMultidimArrayDateTime(Array array)
        {
            if (array == null || array.Length == 0)
                return new double[0];

            if (array.Rank == 1)
            {
                double[] dta = new double[array.Length];
                DateTime[] src = (DateTime[])array;
                for (int i = 0; i < dta.Length; i++)
                {
                    dta[i] = (double)src[i].Ticks;
                }
                return dta;
            }

            double[] result = new double[array.Length];
            int[] index = new int[array.Rank];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (double)((DateTime)array.GetValue(index)).Ticks;

                for (int j = index.Length; --j >= 0; )
                {
                    index[j]++;
                    if (index[j] < array.GetLength(j))
                        break;
                    index[j] = 0;
                }
            }

            return result;
        }

        private static Array ToMultidimArray<T>(T[] ncArray, int[] shape)
        {
            if (ncArray.Length == 0 || shape.Length == 0)
                return null;

            if (shape.Length == 1)
                return ncArray;

            Array array = Array.CreateInstance(typeof(T), shape);

            Debug.Assert(Buffer.ByteLength(ncArray) == Buffer.ByteLength(array));
            Buffer.BlockCopy(ncArray, 0, array, 0, Buffer.ByteLength(array));
            return array;
        }

        private static Array ToMultidimArrayString(string[] ncArray, int[] shape)
        {
            if (ncArray.Length == 0 || shape.Length == 0)
                return null;

            if (shape.Length == 1)
                return ncArray;

            Array array = Array.CreateInstance(typeof(string), shape);
            int[] index = new int[shape.Length];

            for (int i = 0; i < ncArray.Length; i++)
            {
                array.SetValue(ncArray[i], index);

                for (int j = index.Length; --j >= 0; )
                {
                    index[j]++;
                    if (index[j] < shape[j])
                        break;
                    index[j] = 0;
                }
            }
            return array;
        }

        private static Array ToMultidimArrayBool(byte[] ncArray, int[] shape)
        {
            if (ncArray.Length == 0 || shape.Length == 0)
                return null;

            if (shape.Length == 1)
                return Array.ConvertAll(ncArray, p => p > 0);

            Array array = Array.CreateInstance(typeof(bool), shape);
            int[] index = new int[shape.Length];

            for (int i = 0; i < ncArray.Length; i++)
            {
                array.SetValue(ncArray[i] > 0, index);

                for (int j = index.Length; --j >= 0; )
                {
                    index[j]++;
                    if (index[j] < shape[j])
                        break;
                    index[j] = 0;
                }
            }

            return array;
        }

        private static Array ToMultidimArrayDateTime(double[] ncArray, int[] shape)
        {
            if (ncArray.Length == 0 || shape.Length == 0)
                return null;

            if (shape.Length == 1)
            {
                if (ncArray.Length == 1)
                {
                    long ticks = (long)ncArray[0];
                    if (ticks < DateTime.MinValue.Ticks)
                        ticks = DateTime.MinValue.Ticks;
                    return new DateTime[] { new DateTime(ticks) };
                }

                DateTime[] dta = new DateTime[ncArray.Length];
                for (int i = 0; i < dta.Length; i++)
                {
                    dta[i] = new DateTime((long)ncArray[i]);
                }
                return dta;
            }

            Array array = Array.CreateInstance(typeof(DateTime), shape);

            int[] index = new int[shape.Length];

            for (int i = 0; i < ncArray.Length; i++)
            {
                array.SetValue(new DateTime((long)ncArray[i]), index);

                for (int j = index.Length; --j >= 0; )
                {
                    index[j]++;
                    if (index[j] < shape[j])
                        break;
                    index[j] = 0;
                }
            }

            return array;
        }

        #endregion

        #region Data Access

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override int[] ReadShape()
        {
            if (initializing)
                return null;

            int res;
            IntPtr len;
            int[] shape = new int[dimids.Length];

            for (int i = 0; i < dimids.Length; i++)
            {
                res = NetCDF.nc_inq_dimlen(NcId, dimids[i], out len);
                NetCDFDataSet.HandleResult(res);

                shape[i] = (int)len;
            }

            if (isPresentedAttrActualShape)
            {
                // Actual shape of the variable is in the special attribute
                // for the NetCDF doesn't support variables like A[0,10]
                NetCDFDataSet nc = ((NetCDFDataSet)DataSet);

                NcType type;
                res = NetCDF.nc_inq_atttype(nc.NcId, varid, AttributeTypeMap.AttributeVarActualShape, out type);
                if (res == (int)ResultCode.NC_NOERR)
                {
                    if (type != NcType.NC_INT)
                        throw new NetCDFException("Reserved attribute " + AttributeTypeMap.AttributeVarActualShape + " has wrong type");

                    AttributeTypeMap atm = new AttributeTypeMap(nc.NcId, varid);
                    atm[AttributeTypeMap.AttributeVarActualShape] = typeof(int[]);
                    shape = (int[])nc.ReadNetCdfAttribute(varid, AttributeTypeMap.AttributeVarActualShape, atm);
                    if (shape.Length != Rank)
                        throw new NetCDFException("Value of reserved attribute " + AttributeTypeMap.AttributeVarActualShape + " has wrong length");
                }
                else if (res != (int)ResultCode.NC_ENOTATT)
                    NetCDFDataSet.HandleResult(res);
            }

            return shape;
        }

        private IntPtr[] ConvertToIntPtr(int[] array)
        {
            IntPtr[] result = new IntPtr[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                result[i] = new IntPtr(array[i]);
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="shape"></param>
        /// <returns></returns>
        protected override Array ReadData(int[] origin, int[] shape)
        {
            if (origin == null)
                origin = new int[Rank];

            if (shape == null)
            {
                shape = ReadShape();
                for (int i = 0; i < shape.Length; i++)
                    shape[i] -= origin[i];
            }
            if (Rank == 0)
                shape = new int[] { 1 }; // the place for data

            // Total number of elements in the array
            int n = 1;
            for (int i = 0; i < shape.Length; i++)
                n *= shape[i];

            int res;
            switch (Type.GetTypeCode(TypeOfData))
            {
                case TypeCode.Boolean:
                    if (n == 0) return Array.CreateInstance(typeof(bool), Rank == 0 ? new int[1] : new int[Rank]);
                    byte[] blData = new byte[n];
                    res = NetCDF.nc_get_vara_ubyte(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), blData);
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArrayBool(blData, shape);

                case TypeCode.DateTime:
                    double[] dtData = new double[n];
                    if (n == 0) return Array.CreateInstance(typeof(DateTime), Rank == 0 ? new int[1] : new int[Rank]);
                    res = NetCDF.nc_get_vara_double(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), dtData);
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArrayDateTime(dtData, shape);

                case TypeCode.Double:
                    double[] data = new double[n];
                    if (n == 0) return Array.CreateInstance(typeof(double), Rank == 0 ? new int[1] : new int[Rank]);
                    res = NetCDF.nc_get_vara_double(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), data);
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArray(data, shape);

                case TypeCode.Single:
                    float[] fdata = new float[n];
                    if (n == 0) return Array.CreateInstance(typeof(float), Rank == 0 ? new int[1] : new int[Rank]);
                    res = NetCDF.nc_get_vara_float(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), fdata);
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArray(fdata, shape);

                case TypeCode.Int64:
                    long[] ldata = new long[n];
                    if (n == 0) return Array.CreateInstance(typeof(long), Rank == 0 ? new int[1] : new int[Rank]);
                    res = NetCDF.nc_get_vara_longlong(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), ldata);
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArray(ldata, shape);

                case TypeCode.UInt64:
                    ulong[] uldata = new ulong[n];
                    if (n == 0) return Array.CreateInstance(typeof(ulong), Rank == 0 ? new int[1] : new int[Rank]);
                    res = NetCDF.nc_get_vara_ulonglong(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), uldata);
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArray(uldata, shape);


                case TypeCode.Int32:
                    int[] idata = new int[n];
                    if (n == 0) return Array.CreateInstance(typeof(int), Rank == 0 ? new int[1] : new int[Rank]);
                    res = NetCDF.nc_get_vara_int(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), idata);
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArray(idata, shape);

                case TypeCode.UInt32:
                    uint[] uidata = new uint[n];
                    if (n == 0) return Array.CreateInstance(typeof(uint), Rank == 0 ? new int[1] : new int[Rank]);
                    res = NetCDF.nc_get_vara_uint(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), uidata);
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArray(uidata, shape);

                case TypeCode.Int16:
                    short[] sdata = new short[n];
                    if (n == 0) return Array.CreateInstance(typeof(short), Rank == 0 ? new int[1] : new int[Rank]);
                    res = NetCDF.nc_get_vara_short(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), sdata);
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArray(sdata, shape);

                case TypeCode.UInt16:
                    ushort[] usdata = new ushort[n];
                    if (n == 0) return Array.CreateInstance(typeof(ushort), Rank == 0 ? new int[1] : new int[Rank]);
                    res = NetCDF.nc_get_vara_ushort(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), usdata);
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArray(usdata, shape);


                case TypeCode.Byte:
                    byte[] bdata = new byte[n];
                    if (n == 0) return Array.CreateInstance(typeof(byte), Rank == 0 ? new int[1] : new int[Rank]);
                    res = NetCDF.nc_get_vara_ubyte(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), bdata);
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArray(bdata, shape);

                case TypeCode.SByte:
                    SByte[] cdata = new SByte[n];
                    if (n == 0) return Array.CreateInstance(typeof(SByte), Rank == 0 ? new int[1] : new int[Rank]);
                    res = NetCDF.nc_get_vara_schar(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), cdata);
                    /*if (res == -56)
                        res = NetCDF.nc_get_vara_text(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), cdata);*/
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArray(cdata, shape);

                case TypeCode.String:
                    if (n == 0) return Array.CreateInstance(typeof(string), Rank == 0 ? new int[1] : new int[Rank]);
                    string[] strings = new string[n];
                    res = NetCDF.nc_get_vara_string(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), strings);
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArrayString(strings, shape);

                default:
                    throw new NotSupportedException("Unsupported type of data.");
            }
        }
        protected override Array ReadData(int[] origin, int[] stride, int[] shape)
        {
            // TODO: there is a problem using nc_get_vars_string!!!
            // nc_get_vars_string still doesn't work in NetCDF4.1.3 "Mapped access for atomic types only."
            // remove this trick:
            if (TypeOfData == typeof(string))
            {
                return base.ReadData(origin, stride, shape);
            }

            if (origin == null)
                origin = new int[Rank];

            if (shape == null)
            {
                shape = ReadShape();
                for (int i = 0; i < shape.Length; i++)
                    shape[i] -= origin[i];
            }
            if (stride == null)
            {
                origin = new int[Rank];
                for (int i = 0; i < Rank; i++) stride[i] = 1;
            }
            if (Rank == 0)
                shape = new int[] { 1 }; // the place for data

            // Total number of elements in the array
            int n = 1;
            for (int i = 0; i < shape.Length; i++)
                n *= shape[i];

            int res;
            switch (Type.GetTypeCode(TypeOfData))
            {
                case TypeCode.Boolean:
                    if (n == 0) return Array.CreateInstance(typeof(bool), Rank == 0 ? new int[1] : new int[Rank]);
                    byte[] blData = new byte[n];
                    res = NetCDF.nc_get_vars_ubyte(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), ConvertToIntPtr(stride), blData);
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArrayBool(blData, shape);

                case TypeCode.DateTime:
                    double[] dtData = new double[n];
                    if (n == 0) return Array.CreateInstance(typeof(DateTime), Rank == 0 ? new int[1] : new int[Rank]);
                    res = NetCDF.nc_get_vars_double(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), ConvertToIntPtr(stride), dtData);
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArrayDateTime(dtData, shape);

                case TypeCode.Double:
                    double[] data = new double[n];
                    if (n == 0) return Array.CreateInstance(typeof(double), Rank == 0 ? new int[1] : new int[Rank]);
                    res = NetCDF.nc_get_vars_double(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), ConvertToIntPtr(stride), data);
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArray(data, shape);

                case TypeCode.Single:
                    float[] fdata = new float[n];
                    if (n == 0) return Array.CreateInstance(typeof(float), Rank == 0 ? new int[1] : new int[Rank]);
                    res = NetCDF.nc_get_vars_float(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), ConvertToIntPtr(stride), fdata);
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArray(fdata, shape);

                case TypeCode.Int64:
                    long[] ldata = new long[n];
                    if (n == 0) return Array.CreateInstance(typeof(long), Rank == 0 ? new int[1] : new int[Rank]);
                    res = NetCDF.nc_get_vars_longlong(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), ConvertToIntPtr(stride), ldata);
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArray(ldata, shape);

                case TypeCode.UInt64:
                    ulong[] uldata = new ulong[n];
                    if (n == 0) return Array.CreateInstance(typeof(ulong), Rank == 0 ? new int[1] : new int[Rank]);
                    res = NetCDF.nc_get_vars_ulonglong(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), ConvertToIntPtr(stride), uldata);
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArray(uldata, shape);


                case TypeCode.Int32:
                    int[] idata = new int[n];
                    if (n == 0) return Array.CreateInstance(typeof(int), Rank == 0 ? new int[1] : new int[Rank]);
                    res = NetCDF.nc_get_vars_int(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), ConvertToIntPtr(stride), idata);
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArray(idata, shape);

                case TypeCode.UInt32:
                    uint[] uidata = new uint[n];
                    if (n == 0) return Array.CreateInstance(typeof(uint), Rank == 0 ? new int[1] : new int[Rank]);
                    res = NetCDF.nc_get_vars_uint(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), ConvertToIntPtr(stride), uidata);
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArray(uidata, shape);

                case TypeCode.Int16:
                    short[] sdata = new short[n];
                    if (n == 0) return Array.CreateInstance(typeof(short), Rank == 0 ? new int[1] : new int[Rank]);
                    res = NetCDF.nc_get_vars_short(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), ConvertToIntPtr(stride), sdata);
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArray(sdata, shape);

                case TypeCode.UInt16:
                    ushort[] usdata = new ushort[n];
                    if (n == 0) return Array.CreateInstance(typeof(ushort), Rank == 0 ? new int[1] : new int[Rank]);
                    res = NetCDF.nc_get_vars_ushort(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), ConvertToIntPtr(stride), usdata);
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArray(usdata, shape);


                case TypeCode.Byte:
                    byte[] bdata = new byte[n];
                    if (n == 0) return Array.CreateInstance(typeof(byte), Rank == 0 ? new int[1] : new int[Rank]);
                    res = NetCDF.nc_get_vars_ubyte(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), ConvertToIntPtr(stride), bdata);
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArray(bdata, shape);

                case TypeCode.SByte:
                    SByte[] cdata = new SByte[n];
                    if (n == 0) return Array.CreateInstance(typeof(SByte), Rank == 0 ? new int[1] : new int[Rank]);
                    res = NetCDF.nc_get_vars_schar(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), ConvertToIntPtr(stride), cdata);
                    //if (res == -56)
                    //    res = NetCDF.nc_get_vars_text(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), ConvertToIntPtr(stride), cdata);
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArray(cdata, shape);

                case TypeCode.String:
                    if (n == 0) return Array.CreateInstance(typeof(string), Rank == 0 ? new int[1] : new int[Rank]);
                    string[] strings = new string[n];
                    res = NetCDF.nc_get_vars_string(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), ConvertToIntPtr(stride), strings);
                    NetCDFDataSet.HandleResult(res);
                    return ToMultidimArrayString(strings, shape);

                default:
                    throw new NotSupportedException("Unsupported type of data.");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="a"></param>
        protected override void WriteData(int[] origin, Array a)
        {
            if (origin == null)
                origin = new int[Rank];

            int[] shape = new int[origin.Length];
            for (int i = 0; i < shape.Length; i++)
            {
                shape[i] = a.GetLength(i);
            }

            int res;
            switch (Type.GetTypeCode(TypeOfData))
            {
                case TypeCode.Boolean:
                    byte[] bldata = FromMultidimArrayBool(a);
                    res = NetCDF.nc_put_vara_ubyte(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), bldata);
                    NetCDFDataSet.HandleResult(res);
                    return;

                case TypeCode.DateTime:
                    double[] dtData = FromMultidimArrayDateTime(a);
                    res = NetCDF.nc_put_vara_double(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), dtData);
                    NetCDFDataSet.HandleResult(res);
                    return;

                case TypeCode.Double:
                    double[] data = FromMultidimArray<double>(a);
                    res = NetCDF.nc_put_vara_double(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), data);
                    NetCDFDataSet.HandleResult(res);
                    return;

                case TypeCode.Single:
                    float[] fdata = FromMultidimArray<float>(a);
                    res = NetCDF.nc_put_vara_float(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), fdata);
                    NetCDFDataSet.HandleResult(res);
                    return;

                case TypeCode.Int64:
                    long[] ldata = FromMultidimArray<long>(a);
                    res = NetCDF.nc_put_vara_longlong(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), ldata);
                    NetCDFDataSet.HandleResult(res);
                    return;

                case TypeCode.UInt64:
                    ulong[] uldata = FromMultidimArray<ulong>(a);
                    res = NetCDF.nc_put_vara_ulonglong(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), uldata);
                    NetCDFDataSet.HandleResult(res);
                    return;

                case TypeCode.Int32:
                    int[] idata = FromMultidimArray<int>(a);
                    res = NetCDF.nc_put_vara_int(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), idata);
                    NetCDFDataSet.HandleResult(res);
                    return;

                case TypeCode.UInt32:
                    uint[] uidata = FromMultidimArray<uint>(a);
                    res = NetCDF.nc_put_vara_uint(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), uidata);
                    NetCDFDataSet.HandleResult(res);
                    return;

                case TypeCode.Int16:
                    short[] sdata = FromMultidimArray<short>(a);
                    res = NetCDF.nc_put_vara_short(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), sdata);
                    NetCDFDataSet.HandleResult(res);
                    return;

                case TypeCode.UInt16:
                    ushort[] usdata = FromMultidimArray<ushort>(a);
                    res = NetCDF.nc_put_vara_ushort(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), usdata);
                    NetCDFDataSet.HandleResult(res);
                    return;

                case TypeCode.Byte:
                    byte[] bdata = FromMultidimArray<byte>(a);
                    res = NetCDF.nc_put_vara_ubyte(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), bdata);
                    NetCDFDataSet.HandleResult(res);
                    return;

                case TypeCode.SByte:
                    sbyte[] sbdata = FromMultidimArray<sbyte>(a);
                    res = NetCDF.nc_put_vara_schar(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), sbdata);
                    NetCDFDataSet.HandleResult(res);
                    return;

                case TypeCode.String:
                    string[] strdata = FromMultidimArrayString(a);
                    res = NetCDF.nc_put_vara_string(NcId, varid, ConvertToIntPtr(origin), ConvertToIntPtr(shape), strdata);
                    NetCDFDataSet.HandleResult(res);
                    return;

                default:
                    throw new NotSupportedException("Unsupported type of data.");
            }
        }

        private void GetMinMax(int[] a, out int min, out int max)
        {
            if (a.Length == 0) throw new ArgumentException("Array is empty");
            max = min = a[0];
            for (int i = 1; i < a.Length; i++)
                if (a[i] > max)
                    max = a[i];
                else if (a[i] < min)
                    min = a[i];
        }

        #endregion
    }

    internal interface INetCDFVariable
    {
        void UpdateDimiIds();
    }
}

