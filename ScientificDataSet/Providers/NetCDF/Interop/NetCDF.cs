﻿#pragma warning disable 1591
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace NetCDFInterop
{
    public static class NetCDF
    {
        public static string nc_strerror(int ncerror)
        {
            switch (ncerror)
            {
                case (0): return "No error";
                case (-1): return "Returned for all errors in the v2 API";
                case (-33): return "Not a netcdf id";
                case (-34): return "Too many netcdfs open";
                case (-35): return "netcdf file exists && NC_NOCLOBBER";
                case (-36): return "Invalid Argument";
                case (-37): return "Write to read only";
                case (-38): return "Operation not allowed in data mode";
                case (-39): return "Operation not allowed in define mode";
                case (-40): return "Index exceeds dimension bound. Consider cloning the file into NetCDF 4 format to enable data extdending (e.g. with sds copy command)";
                case (-41): return "NC_MAX_DIMS exceeded";
                case (-42): return "String match to name in use";
                case (-43): return "Attribute not found";
                case (-44): return "NC_MAX_ATTRS exceeded";
                case (-45): return "Not a netcdf data type. Some types are not supported by the classic NetCDF format. Consider cloning the file into NetCDF 4 format to enable use of all supported types (e.g. with sds copy command)";
                case (-46): return "Invalid dimension id or name";
                case (-47): return "NC_UNLIMITED in the wrong index";
                case (-48): return "NC_MAX_VARS exceeded";
                case (-49): return "Variable not found";
                case (-50): return "Action prohibited on NC_GLOBAL varid";
                case (-51): return "Not a netcdf file";
                case (-52): return "In Fortran, string too short";
                case (-53): return "NC_MAX_NAME exceeded";
                case (-54): return "NC_UNLIMITED size already in use";
                case (-55): return "nc_rec op when there are no record vars";
                case (-56): return "Attempt to convert between text & numbers";
                case (-57): return "Start+count exceeds dimension bound";
                case (-58): return "Illegal stride";
                case (-59): return "Attribute or variable name contains illegal characters";
                case (-60): return "Math result not representable";
                case (-61): return "Memory allocation (malloc) failure";
                case (-62): return "One or more variable sizes violate format constraints";
                case (-63): return "Invalid dimension size";
                case (-64): return "File likely truncated or possibly corrupted";
                case (-65): return "Unknown axis type.";
                // DAP errors
                case (-66): return "Generic DAP error";
                case (-67): return "Generic libcurl error";
                case (-68): return "Generic IO error";
                // netcdf-4 errors
                case (-100): return "NetCDF4 error";
                case (-101): return "Error at HDF5 layer.";
                //{
                //    //return "Error at HDF5 layer.";
                //    string path = nullptr;
                //    const char* _path = NULL;
                //    try{
                //        path = System::IO::Path::GetTempFileName();
                //        _path = ToUTF8(path);
                //        ::nc_hdf_strerror(_path);
                //        return System::IO::File::ReadAllText(path);
                //    }
                //    catch(Exception^){				
                //        return "Error at HDF5 layer.";
                //    }
                //    finally
                //    {
                //        if(_path != NULL) delete[] _path;
                //        if(path != nullptr) System::IO::File::Delete(path);
                //    }

                //    /*const int size = 10240;
                //    char* buffer = new char[size]; buffer[0] = '\0';
                //    NetCDFInterop::NetCDF::m->WaitOne();
                //    ::nc_hdf_strerror(buffer, size);
                //    NetCDFInterop::NetCDF::m->ReleaseMutex();
                //    string s = FromUTF8(buffer);
                //    delete[] buffer;
                //    return s;*/
                //}
                case (-102): return "Can't read.";
                case (-103): return "Can't write.";
                case (-104): return "Can't create.";
                case (-105): return "Problem with file metadata.";
                case (-106): return "Problem with dimension metadata.";
                case (-107): return "Problem with attribute metadata.";
                case (-108): return "Problem with variable metadata.";
                case (-109): return "Not a compound type.";
                case (-110): return "Attribute already exists.";
                case (-111): return "Attempting netcdf-4 operation on netcdf-3 file.";
                case (-112): return "Attempting netcdf-4 operation on strict nc3 netcdf-4 file.";
                case (-113): return "Attempting netcdf-3 operation on netcdf-4 file.";
                case (-114): return "Parallel operation on file opened for non-parallel access.";
                case (-115): return "Error initializing for parallel access.";
                case (-116): return "Bad group ID.";
                case (-117): return "Bad type ID.";
                case (-118): return "Type has already been defined and may not be edited.";
                case (-119): return "Bad field ID.";
                case (-120): return "Bad class.";
                case (-121): return "Mapped access for atomic types only.";
                case (-122): return "Attempt to define fill value when data already exists.";
                case (-123): return "Attempt to define var properties, like deflate, after enddef.";
                case (-124): return "Probem with HDF5 dimscales.";
                case (-125): return "No group found.";
                case (-126): return "Can't specify both contiguous and chunking.";
                case (-127): return "Bad chunksize.";
                case (-128): return "NetCDF4 error";
                default: return "NetCDF error " + ncerror;
            }
        }

        public static Type GetCLRType(NcType ncType)
        {
            switch (ncType)
            {
                case NcType.NC_BYTE:
                    return typeof(SByte);
                case NcType.NC_UBYTE:
                    return typeof(Byte);
                case NcType.NC_CHAR:
                    return typeof(Byte);
                case NcType.NC_SHORT:
                    return typeof(short);
                case NcType.NC_USHORT:
                    return typeof(UInt16);
                case NcType.NC_INT:
                    return typeof(int);
                case NcType.NC_INT64:
                    return typeof(Int64);
                case NcType.NC_UINT:
                    return typeof(UInt32);
                case NcType.NC_UINT64:
                    return typeof(UInt64);
                case NcType.NC_FLOAT:
                    return typeof(float);
                case NcType.NC_DOUBLE:
                    return typeof(double);
                case NcType.NC_STRING:
                    return typeof(String);
                default:
                    throw new ApplicationException("Unknown nc type");
            }
        }

        public static NcType GetNcType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Double:
                    return NcType.NC_DOUBLE;

                case TypeCode.Single:
                    return NcType.NC_FLOAT;

                case TypeCode.Int64:
                    return NcType.NC_INT64;

                case TypeCode.UInt64:
                    return NcType.NC_UINT64;

                case TypeCode.Int32:
                    return NcType.NC_INT;

                case TypeCode.UInt32:
                    return NcType.NC_UINT;

                case TypeCode.Int16:
                    return NcType.NC_SHORT;

                case TypeCode.UInt16:
                    return NcType.NC_USHORT;

                case TypeCode.Byte:
                    return NcType.NC_UBYTE;

                case TypeCode.SByte:
                    return NcType.NC_BYTE;

                case TypeCode.String:
                    return NcType.NC_STRING;

                case TypeCode.DateTime:
                    return NcType.NC_DOUBLE;

                case TypeCode.Boolean:
                    return NcType.NC_UBYTE;

                default:
                    throw new NotSupportedException("Not supported type of data.");
            }
        }
        private static Encoder utfEncoder;
        private static Decoder utfDecoder;
        //private static System.Threading.Mutex m = new System.Threading.Mutex();
        private static StringBuilder namebuf = new StringBuilder(256); // this is also a lock target

        static NetCDF()
        {
            Encoding encoding = new UTF8Encoding(false);
            utfEncoder = encoding.GetEncoder();
            utfDecoder = encoding.GetDecoder();
        }
        unsafe private static string ReadString(IntPtr p)
        {
            if (IntPtr.Zero == p) return null;
            byte* b = (byte*)p;
            byte* z = b; while ((byte)0 != *z) z += 1;
            int count = (int)(z - b);
            if (0 == count) return string.Empty;
            var chars = new char[utfDecoder.GetCharCount(b, count, true)];
            fixed (char* c = chars)
                utfDecoder.GetChars(b, count, c, chars.Length, true);
            return new string(chars);
        }
        /// <summary>
        /// Writes strings to a buffer as zero-terminated UTF8-encoded strings.
        /// </summary>
        /// <param name="data">An array of strings to write to a buffer.</param>
        /// <returns>A pair of a buffer with zero-terminated UTF8-encoded strings and an array of offsets to the buffer.
        /// An offset of uint.MaxValue represents null in the data.
        /// </returns>
        unsafe private static Tuple<byte[], uint[]> WriteStrings(string[] data)
        {
            // first pass -- compute offsets
            var count = data.Length;
            uint buflen = 0;
            var bytecounts = new int[count];
            for (int i = 0; i < count; i++)
                if (null != data[i])
                {
                    int bc = 0;
                    fixed (char* p = data[i])
                        bc = utfEncoder.GetByteCount(p, data[i].Length, true);
                    bytecounts[i] = bc;
                    if (uint.MaxValue - buflen - 1 < bc)
                        throw new InternalBufferOverflowException("string buffer cannot exceed 4Gbyte in one NetCDF operation");
                    buflen += (uint)bc + 1; // account for terminating zero
                }
            // now allocate the buffer and write bytes
            var buf = new byte[buflen];
            var off = new uint[count];
            fixed (byte* pbuf = buf)
            {
                int charsUsed;
                int bytesUsed;
                bool isCompleted;
                uint offset = 0;
                for (int i = 0; i < count; i++)
                    if (null == data[i]) off[i] = uint.MaxValue;
                    else
                    {
                        off[i] = offset;
                        int bc = bytecounts[i];
                        fixed (char* p = data[i])
                            utfEncoder.Convert(p, data[i].Length, pbuf + offset, bc, true, out charsUsed, out bytesUsed, out isCompleted);
                        System.Diagnostics.Debug.Assert(charsUsed == data[i].Length && bytesUsed == bc && isCompleted);
                        offset += (uint)bc;
                        *(pbuf + offset) = (byte)0;
                        offset += 1;
                    }
            }
            return Tuple.Create(buf, off);
        }
        public static int nc_open(string path, CreateMode mode, out int ncidp) { lock (namebuf) { return NetCDFNative.nc_open(path, mode, out ncidp); } }
        public static int nc_open_chunked(string path, CreateMode mode, out int ncidp, IntPtr size, IntPtr nelems, float preemption)
        {
            lock (namebuf)
            {
                var r = NetCDFNative.nc_set_chunk_cache(size, nelems, preemption);
                if (0 == r)
                    return NetCDFNative.nc_open(path, mode, out ncidp);
                else
                { ncidp = 0; return r; }
            }
        }
        public static int nc_create(string path, CreateMode mode, out int ncidp) { lock (namebuf) { return NetCDFNative.nc_create(path, mode, out ncidp); } }
        public static int nc_create_chunked(string path, CreateMode mode, out int ncidp, IntPtr size, IntPtr nelems, float preemption)
        {
            lock (namebuf)
            {
                var r = NetCDFNative.nc_set_chunk_cache(size, nelems, preemption);
                if (0 == r)
                    return NetCDFNative.nc_create(path, mode, out ncidp);
                else
                { ncidp = 0; return r; }
            }
        }
        public static int nc_close(int ncidp) { lock (namebuf) { return NetCDFNative.nc_close(ncidp); } }
        public static int nc_set_chunk_cache(IntPtr size, IntPtr nelems, float preemption) { lock (namebuf) { return NetCDFNative.nc_set_chunk_cache(size, nelems, preemption); } }
        public static int nc_get_chunk_cache(out IntPtr size, out IntPtr nelems, out float preemption) { lock (namebuf) { return NetCDFNative.nc_get_chunk_cache(out size, out nelems, out preemption); } }
        public static int nc_sync(int ncid) { lock (namebuf) { return NetCDFNative.nc_sync(ncid); } }
        public static int nc_enddef(int ncid) { lock (namebuf) { return NetCDFNative.nc_enddef(ncid); } }
        public static int nc_redef(int ncid) { lock (namebuf) { return NetCDFNative.nc_redef(ncid); } }
        public static int nc_inq(int ncid, out int ndims, out int nvars, out int ngatts, out int unlimdimid) { lock (namebuf) { return NetCDFNative.nc_inq(ncid, out ndims, out nvars, out ngatts, out unlimdimid); } }
        public static int nc_def_var(int ncid, string name, NcType xtype, int[] dimids, out int varidp) { lock (namebuf) { return NetCDFNative.nc_def_var(ncid, name, xtype, dimids.Length, dimids, out varidp); } }
        public static int nc_def_dim(int ncid, string name, IntPtr len, out int dimidp) { lock (namebuf) { return NetCDFNative.nc_def_dim(ncid, name, len, out dimidp); } }
        public static int nc_def_var_deflate(int ncid, int varid, int shuffle, int deflate, int deflate_level) { lock (namebuf) { return NetCDFNative.nc_def_var_deflate(ncid, varid, shuffle, deflate, deflate_level); } }
        public static int nc_def_var_chunking(int ncid, int varid, int contiguous, IntPtr[] chunksizes) { lock (namebuf) { return NetCDFNative.nc_def_var_chunking(ncid, varid, contiguous, chunksizes); } }
        public static int nc_inq_var(int ncid, int varid, out string name, out NcType type, out int ndims, int[] dimids, out int natts)
        {
            lock (namebuf)
            {
                var r = NetCDFNative.nc_inq_var(ncid, varid, namebuf, out type, out ndims, dimids, out natts);
                name = namebuf.ToString();
                return r;
            }
        }
        public static int nc_inq_varids(int ncid, out int nvars, int[] varids) { lock (namebuf) { return NetCDFNative.nc_inq_varids(ncid, out nvars, varids); } }
        public static int nc_inq_vartype(int ncid, int varid, out NcType xtypep) { lock (namebuf) { return NetCDFNative.nc_inq_vartype(ncid, varid, out xtypep); } }
        public static int nc_inq_varnatts(int ncid, int varid, out int nattsp) { lock (namebuf) { return NetCDFNative.nc_inq_varnatts(ncid, varid, out nattsp); } }
        //public static int nc_inq_varid(int ncid, string name, out int varidp) { lock(namebuf) { return NetCDFDynamic.nc_inq_varid(ncid, name, out varidp); } }
        //public static int nc_inq_ndims(int ncid, out int ndims) { lock(namebuf) { return NetCDFDynamic.nc_inq_ndims(ncid, out ndims); } }
        //public static int nc_inq_nvars(int ncid, out int nvars) { lock(namebuf) { return NetCDFDynamic.nc_inq_nvars(ncid, out nvars); } }
        //public static int nc_inq_varname(int ncid, int varid, out string name) { lock(namebuf) { return NetCDFDynamic.nc_inq_varname(ncid, varid, out name); } }
        public static int nc_inq_varndims(int ncid, int varid, out int ndims) { lock (namebuf) { return NetCDFNative.nc_inq_varndims(ncid, varid, out ndims); } }
        public static int nc_inq_vardimid(int ncid, int varid, int[] dimids) { lock (namebuf) { return NetCDFNative.nc_inq_vardimid(ncid, varid, dimids); } }
        //public static int nc_inq_natts(int ncid, out int ngatts) { lock(namebuf) { return NetCDFDynamic.nc_inq_natts(ncid, out ngatts); } }
        //public static int nc_inq_unlimdim(int ncid, out int unlimdimid) { lock(namebuf) { return NetCDFDynamic.nc_inq_unlimdim(ncid, out unlimdimid); } }
        //public static int nc_inq_format(int ncid, out int format) { lock(namebuf) { return NetCDFDynamic.nc_inq_format(ncid, out format); } }
        public static int nc_inq_attname(int ncid, int varid, int attnum, out string name)
        {
            lock (namebuf)
            {
                var r = NetCDFNative.nc_inq_attname(ncid, varid, attnum, namebuf);
                name = namebuf.ToString();
                return r;
            }
        }
        public static int nc_inq_atttype(int ncid, int varid, string name, out NcType type) { lock (namebuf) { return NetCDFNative.nc_inq_atttype(ncid, varid, name, out type); } }
        public static int nc_inq_att(int ncid, int varid, string name, out NcType type, out IntPtr length) { lock (namebuf) { return NetCDFNative.nc_inq_att(ncid, varid, name, out type, out length); } }
        public static int nc_get_att_text(int ncid, int varid, string name, out string value, int maxLength)
        {
            lock (namebuf)
            {
                var b = new byte[maxLength + 2]; // in case netcdf adds terminating zero
                var r = NetCDFNative.nc_get_att_text(ncid, varid, name, b, maxLength);
                var chars = new char[utfDecoder.GetCharCount(b, 0, maxLength)];
                utfDecoder.GetChars(b, 0, maxLength, chars, 0);
                value = new string(chars);
                return r;
            }
        }
        public static int nc_get_att_schar(int ncid, int varid, string name, SByte[] data) { lock (namebuf) { return NetCDFNative.nc_get_att_schar(ncid, varid, name, data); } }
        public static int nc_get_att_uchar(int ncid, int varid, string name, Byte[] data) { lock (namebuf) { return NetCDFNative.nc_get_att_uchar(ncid, varid, name, data); } }
        public static int nc_get_att_short(int ncid, int varid, string name, Int16[] data) { lock (namebuf) { return NetCDFNative.nc_get_att_short(ncid, varid, name, data); } }
        public static int nc_get_att_ushort(int ncid, int varid, string name, UInt16[] data) { lock (namebuf) { return NetCDFNative.nc_get_att_ushort(ncid, varid, name, data); } }
        public static int nc_get_att_int(int ncid, int varid, string name, Int32[] data) { lock (namebuf) { return NetCDFNative.nc_get_att_int(ncid, varid, name, data); } }
        public static int nc_get_att_uint(int ncid, int varid, string name, UInt32[] data) { lock (namebuf) { return NetCDFNative.nc_get_att_uint(ncid, varid, name, data); } }
        public static int nc_get_att_longlong(int ncid, int varid, string name, Int64[] data) { lock (namebuf) { return NetCDFNative.nc_get_att_longlong(ncid, varid, name, data); } }
        public static int nc_get_att_ulonglong(int ncid, int varid, string name, UInt64[] data) { lock (namebuf) { return NetCDFNative.nc_get_att_ulonglong(ncid, varid, name, data); } }
        public static int nc_get_att_float(int ncid, int varid, string name, float[] data) { lock (namebuf) { return NetCDFNative.nc_get_att_float(ncid, varid, name, data); } }
        public static int nc_get_att_double(int ncid, int varid, string name, double[] data) { lock (namebuf) { return NetCDFNative.nc_get_att_double(ncid, varid, name, data); } }
        public static int nc_del_att(int ncid, int varid, string name) { lock (namebuf) { return NetCDFNative.nc_del_att(ncid, varid, name); } }
        public static int nc_put_att_text(int ncid, int varid, string name, string tp)
        {
            var buf_offset = WriteStrings(new string[] { tp });
            lock (namebuf)
            {
                var buf = buf_offset.Item1;
                var r = NetCDFNative.nc_put_att_text(ncid, varid, name, new IntPtr(buf.Length - 1), buf);
                return r;
            }
        }
        unsafe public static int nc_put_att_string(int ncid, int varid, string name, string[] tp)
        {
            int r;
            var len = tp.Length;
            var bb = new IntPtr[len];
            var buf_offset = WriteStrings(tp);
            lock (namebuf)
            {
                fixed (byte* buf = buf_offset.Item1)
                {
                    for (int i = 0; i < len; i++)
                    {
                        var offset = buf_offset.Item2[i];
                        if (uint.MaxValue == offset) bb[i] = IntPtr.Zero;
                        else bb[i] = new IntPtr(buf + offset);
                    }
                    r = NetCDFNative.nc_put_att_string(ncid, varid, name, new IntPtr(bb.Length), bb);
                }
                return r;
            }
        }
        public static int nc_put_att_double(int ncid, int varid, string name, double[] tp) { lock (namebuf) { return NetCDFNative.nc_put_att_double(ncid, varid, name, NcType.NC_DOUBLE, new IntPtr(tp.Length), tp); } }
        public static int nc_put_att_int(int ncid, int varid, string name, int[] tp) { lock (namebuf) { return NetCDFNative.nc_put_att_int(ncid, varid, name, NcType.NC_INT, new IntPtr(tp.Length), tp); } }
        public static int nc_put_att_short(int ncid, int varid, string name, short[] tp) { lock (namebuf) { return NetCDFNative.nc_put_att_short(ncid, varid, name, NcType.NC_SHORT, new IntPtr(tp.Length), tp); } }
        public static int nc_put_att_longlong(int ncid, int varid, string name, Int64[] tp) { lock (namebuf) { return NetCDFNative.nc_put_att_longlong(ncid, varid, name, NcType.NC_INT64, new IntPtr(tp.Length), tp); } }
        public static int nc_put_att_ushort(int ncid, int varid, string name, UInt16[] tp) { lock (namebuf) { return NetCDFNative.nc_put_att_ushort(ncid, varid, name, NcType.NC_USHORT, new IntPtr(tp.Length), tp); } }
        public static int nc_put_att_uint(int ncid, int varid, string name, UInt32[] tp) { lock (namebuf) { return NetCDFNative.nc_put_att_uint(ncid, varid, name, NcType.NC_UINT, new IntPtr(tp.Length), tp); } }
        public static int nc_put_att_float(int ncid, int varid, string name, float[] tp) { lock (namebuf) { return NetCDFNative.nc_put_att_float(ncid, varid, name, NcType.NC_FLOAT, new IntPtr(tp.Length), tp); } }
        public static int nc_put_att_ulonglong(int ncid, int varid, string name, UInt64[] tp) { lock (namebuf) { return NetCDFNative.nc_put_att_ulonglong(ncid, varid, name, NcType.NC_UINT64, new IntPtr(tp.Length), tp); } }
        public static int nc_put_att_schar(int ncid, int varid, string name, SByte[] tp) { lock (namebuf) { return NetCDFNative.nc_put_att_schar(ncid, varid, name, NcType.NC_BYTE, new IntPtr(tp.Length), tp); } }
        public static int nc_put_att_ubyte(int ncid, int varid, string name, Byte[] tp) { lock (namebuf) { return NetCDFNative.nc_put_att_ubyte(ncid, varid, name, NcType.NC_UBYTE, new IntPtr(tp.Length), tp); } }
        //public static int nc_inq_dim(int ncid, int dimid, out string name, out IntPtr length) { lock(namebuf) { return NetCDFDynamic.nc_inq_dim(ncid, dimid, out name, out length); } }
        public static int nc_inq_dimname(int ncid, int dimid, out string name)
        {
            lock (namebuf)
            {
                var r = NetCDFNative.nc_inq_dimname(ncid, dimid, namebuf);
                name = namebuf.ToString();
                return r;
            }
        }
        public static int nc_inq_dimid(int ncid, string name, out int dimid) { lock (namebuf) { return NetCDFNative.nc_inq_dimid(ncid, name, out dimid); } }
        public static int nc_inq_dimlen(int ncid, int dimid, out IntPtr length) { lock (namebuf) { return NetCDFNative.nc_inq_dimlen(ncid, dimid, out length); } }
        public static int nc_get_att_string(int ncid, int varid, string name, string[] ip)
        {
            lock (namebuf)
            {
                var len = ip.Length;
                var parr = new IntPtr[len];
                var r = NetCDFNative.nc_get_att_string(ncid, varid, name, parr);
                if (0 == r)
                {
                    for (int i = 0; i < len; i++) ip[i] = ReadString(parr[i]);
                    r = NetCDFNative.nc_free_string(new IntPtr(len), parr);
                }
                return r;
            }
        }
        //public static int nc_get_var_text(int ncid, int varid, Byte[] data) { lock(namebuf) { return NetCDFDynamic.nc_get_var_text(ncid, varid, data); } }
        //public static int nc_get_var_schar(int ncid, int varid, SByte[] data) { lock(namebuf) { return NetCDFDynamic.nc_get_var_schar(ncid, varid, data); } }
        //public static int nc_get_var_short(int ncid, int varid, short[] data) { lock(namebuf) { return NetCDFDynamic.nc_get_var_short(ncid, varid, data); } }
        //public static int nc_get_var_int(int ncid, int varid, int[] data) { lock(namebuf) { return NetCDFDynamic.nc_get_var_int(ncid, varid, data); } }
        //public static int nc_get_var_long(int ncid, int varid, long[] data) { lock(namebuf) { return NetCDFDynamic.nc_get_var_long(ncid, varid, data); } }
        //public static int nc_get_var_float(int ncid, int varid, float[] data) { lock(namebuf) { return NetCDFDynamic.nc_get_var_float(ncid, varid, data); } }
        //public static int nc_get_var_double(int ncid, int varid, double[] data) { lock(namebuf) { return NetCDFDynamic.nc_get_var_double(ncid, varid, data); } }
        public static int nc_put_vara_text(int ncid, int varid, IntPtr[] start, IntPtr[] count, byte[] dp) { lock (namebuf) { return NetCDFNative.nc_put_vara_text(ncid, varid, start, count, dp); } }
        public static int nc_put_vara_double(int ncid, int varid, IntPtr[] start, IntPtr[] count, double[] dp) { lock (namebuf) { return NetCDFNative.nc_put_vara_double(ncid, varid, start, count, dp); } }
        public static int nc_put_vara_float(int ncid, int varid, IntPtr[] start, IntPtr[] count, float[] dp) { lock (namebuf) { return NetCDFNative.nc_put_vara_float(ncid, varid, start, count, dp); } }
        public static int nc_put_vara_short(int ncid, int varid, IntPtr[] start, IntPtr[] count, short[] dp) { lock (namebuf) { return NetCDFNative.nc_put_vara_short(ncid, varid, start, count, dp); } }
        public static int nc_put_vara_ushort(int ncid, int varid, IntPtr[] start, IntPtr[] count, UInt16[] dp) { lock (namebuf) { return NetCDFNative.nc_put_vara_ushort(ncid, varid, start, count, dp); } }
        public static int nc_put_vara_int(int ncid, int varid, IntPtr[] start, IntPtr[] count, int[] ip) { lock (namebuf) { return NetCDFNative.nc_put_vara_int(ncid, varid, start, count, ip); } }
        public static int nc_put_vara_uint(int ncid, int varid, IntPtr[] start, IntPtr[] count, UInt32[] dp) { lock (namebuf) { return NetCDFNative.nc_put_vara_uint(ncid, varid, start, count, dp); } }
        public static int nc_put_vara_longlong(int ncid, int varid, IntPtr[] start, IntPtr[] count, Int64[] dp) { lock (namebuf) { return NetCDFNative.nc_put_vara_longlong(ncid, varid, start, count, dp); } }
        public static int nc_put_vara_ulonglong(int ncid, int varid, IntPtr[] start, IntPtr[] count, UInt64[] dp) { lock (namebuf) { return NetCDFNative.nc_put_vara_ulonglong(ncid, varid, start, count, dp); } }
        public static int nc_put_vara_ubyte(int ncid, int varid, IntPtr[] start, IntPtr[] count, Byte[] dp) { lock (namebuf) { return NetCDFNative.nc_put_vara_ubyte(ncid, varid, start, count, dp); } }
        public static int nc_put_vara_schar(int ncid, int varid, IntPtr[] start, IntPtr[] count, SByte[] dp) { lock (namebuf) { return NetCDFNative.nc_put_vara_schar(ncid, varid, start, count, dp); } }
        unsafe public static int nc_put_vara_string(int ncid, int varid, IntPtr[] start, IntPtr[] count, string[] dp)
        {
            int r;
            var len = dp.Length;
            var bb = new IntPtr[len];
            var buf_offset = WriteStrings(dp);
            lock (namebuf)
            {
                fixed (byte* buf = buf_offset.Item1)
                {
                    for (int i = 0; i < len; i++)
                    {
                        var offset = buf_offset.Item2[i];
                        if (uint.MaxValue == offset) bb[i] = IntPtr.Zero;
                        else bb[i] = new IntPtr(buf + offset);
                    }
                    r = NetCDFNative.nc_put_vara_string(ncid, varid, start, count, bb);
                }
                return r;
            }
        }
        public static int nc_get_vara_text(int ncid, int varid, IntPtr[] start, IntPtr[] count, Byte[] data) { lock (namebuf) { return NetCDFNative.nc_get_vara_text(ncid, varid, start, count, data); } }
        public static int nc_get_vara_schar(int ncid, int varid, IntPtr[] start, IntPtr[] count, SByte[] data) { lock (namebuf) { return NetCDFNative.nc_get_vara_schar(ncid, varid, start, count, data); } }
        public static int nc_get_vara_short(int ncid, int varid, IntPtr[] start, IntPtr[] count, short[] data) { lock (namebuf) { return NetCDFNative.nc_get_vara_short(ncid, varid, start, count, data); } }
        public static int nc_get_vara_ushort(int ncid, int varid, IntPtr[] start, IntPtr[] count, UInt16[] data) { lock (namebuf) { return NetCDFNative.nc_get_vara_ushort(ncid, varid, start, count, data); } }
        public static int nc_get_vara_ubyte(int ncid, int varid, IntPtr[] start, IntPtr[] count, Byte[] data) { lock (namebuf) { return NetCDFNative.nc_get_vara_ubyte(ncid, varid, start, count, data); } }
        public static int nc_get_vara_longlong(int ncid, int varid, IntPtr[] start, IntPtr[] count, Int64[] data) { lock (namebuf) { return NetCDFNative.nc_get_vara_longlong(ncid, varid, start, count, data); } }
        public static int nc_get_vara_ulonglong(int ncid, int varid, IntPtr[] start, IntPtr[] count, UInt64[] data) { lock (namebuf) { return NetCDFNative.nc_get_vara_ulonglong(ncid, varid, start, count, data); } }
        public static int nc_get_vara_int(int ncid, int varid, IntPtr[] start, IntPtr[] count, int[] data) { lock (namebuf) { return NetCDFNative.nc_get_vara_int(ncid, varid, start, count, data); } }
        public static int nc_get_vara_uint(int ncid, int varid, IntPtr[] start, IntPtr[] count, UInt32[] data) { lock (namebuf) { return NetCDFNative.nc_get_vara_uint(ncid, varid, start, count, data); } }
        public static int nc_get_vara_float(int ncid, int varid, IntPtr[] start, IntPtr[] count, float[] data) { lock (namebuf) { return NetCDFNative.nc_get_vara_float(ncid, varid, start, count, data); } }
        public static int nc_get_vara_double(int ncid, int varid, IntPtr[] start, IntPtr[] count, double[] data) { lock (namebuf) { return NetCDFNative.nc_get_vara_double(ncid, varid, start, count, data); } }
        public static int nc_get_vara_string(int ncid, int varid, IntPtr[] start, IntPtr[] count, string[] data)
        {
            lock (namebuf)
            {
                var len = data.Length;
                var parr = new IntPtr[len];
                var r = NetCDFNative.nc_get_vara_string(ncid, varid, start, count, parr);
                if (0 == r)
                {
                    for (int i = 0; i < len; i++) data[i] = ReadString(parr[i]);
                    r = NetCDFNative.nc_free_string(new IntPtr(len), parr);
                }
                return r;
            }
        }
        public static int nc_get_var_string(int ncid, int varid, string[] data)
        {
            lock (namebuf)
            {
                var len = data.Length;
                var parr = new IntPtr[len];
                var r = NetCDFNative.nc_get_var_string(ncid, varid, parr);
                if (0 == r)
                {
                    for (int i = 0; i < len; i++) data[i] = ReadString(parr[i]);
                    r = NetCDFNative.nc_free_string(new IntPtr(len), parr);
                }
                return r;
            }
        }
        public static int nc_get_vars_text(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, Byte[] data) { lock (namebuf) { return NetCDFNative.nc_get_vars_text(ncid, varid, start, count, stride, data); } }
        public static int nc_get_vars_schar(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, SByte[] data) { lock (namebuf) { return NetCDFNative.nc_get_vars_schar(ncid, varid, start, count, stride, data); } }
        public static int nc_get_vars_short(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, short[] data) { lock (namebuf) { return NetCDFNative.nc_get_vars_short(ncid, varid, start, count, stride, data); } }
        public static int nc_get_vars_ushort(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, UInt16[] data) { lock (namebuf) { return NetCDFNative.nc_get_vars_ushort(ncid, varid, start, count, stride, data); } }
        public static int nc_get_vars_ubyte(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, Byte[] data) { lock (namebuf) { return NetCDFNative.nc_get_vars_ubyte(ncid, varid, start, count, stride, data); } }
        public static int nc_get_vars_longlong(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, Int64[] data) { lock (namebuf) { return NetCDFNative.nc_get_vars_longlong(ncid, varid, start, count, stride, data); } }
        public static int nc_get_vars_ulonglong(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, UInt64[] data) { lock (namebuf) { return NetCDFNative.nc_get_vars_ulonglong(ncid, varid, start, count, stride, data); } }
        public static int nc_get_vars_int(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, int[] data) { lock (namebuf) { return NetCDFNative.nc_get_vars_int(ncid, varid, start, count, stride, data); } }
        public static int nc_get_vars_uint(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, UInt32[] data) { lock (namebuf) { return NetCDFNative.nc_get_vars_uint(ncid, varid, start, count, stride, data); } }
        public static int nc_get_vars_float(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, float[] data) { lock (namebuf) { return NetCDFNative.nc_get_vars_float(ncid, varid, start, count, stride, data); } }
        public static int nc_get_vars_double(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, double[] data) { lock (namebuf) { return NetCDFNative.nc_get_vars_double(ncid, varid, start, count, stride, data); } }
        public static int nc_get_vars_string(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, string[] data)
        {
            lock (namebuf)
            {
                var len = data.Length;
                var parr = new IntPtr[len];
                var r = NetCDFNative.nc_get_vars_string(ncid, varid, start, count, stride, parr);
                if (0 == r)
                {
                    for (int i = 0; i < len; i++) data[i] = ReadString(parr[i]);
                    r = NetCDFNative.nc_free_string(new IntPtr(len), parr);
                }
                return r;
            }
        }
        /// <summary>
        /// Get netCDF library version.
        /// </summary>
        /// <returns>A version string, e.g. "4.9.2 of Mar 14 2023 15:42:34 $".</returns>
        public static string nc_inq_libvers() => Marshal.PtrToStringAnsi(NetCDFNative.nc_inq_libvers());
    }
    static class NetCDFNative
    {
        static NetCDFNative()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                // Find where netcdf.dll is and ensure its location is on PATH.
                var name = "netcdf.dll";
                // LIBNETCDFPATH takes precedence of other places
                string path = Environment.GetEnvironmentVariable("LIBNETCDFPATH");
                if (path == null)
                {
                    // If not, check netcdf.dll alongside ScientificDataSet.dll
                    path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    if (!File.Exists(Path.Combine(path, name))) { path = null; }
                }
                else
                {
                    // LIBNETCDFPATH may refer to a file or a directory,
                    // path must be a directory.
                    if (File.Exists(path))
                    {
                        path = Path.GetDirectoryName(path);
                    }
                }
                if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                {
                    Environment.SetEnvironmentVariable("PATH",
                        Environment.GetEnvironmentVariable("PATH") + ";" + path);
                }
                // try find the file in current directory, alongside the executing assembly
                // and then in directories from PATH environmental variable.
                path = new string[] { Environment.CurrentDirectory }
                    .Concat(Environment.GetEnvironmentVariable("PATH").Split(';'))
                    .FirstOrDefault(d => File.Exists(Path.Combine(d, name)));
                if (null == path)
                {
                    // alternatively try standard install paths.
                    var programFiles = Environment.GetFolderPath(
                        Environment.Is64BitProcess || !Environment.Is64BitOperatingSystem
                        ? Environment.SpecialFolder.ProgramFiles
                        : Environment.SpecialFolder.ProgramFilesX86);
                    if (string.Empty == programFiles)
                    {
                        // In windows containers `GetFolderPath` returns empty string
                        programFiles = Environment.GetEnvironmentVariable(
                        Environment.Is64BitProcess || !Environment.Is64BitOperatingSystem
                        ? "ProgramFiles" : "ProgramFiles(x86)");
                    }
                    if (!string.IsNullOrWhiteSpace(programFiles))
                    {
                        var ncdir = Directory.GetDirectories(programFiles)
                            .Reverse() // last version first
                            .Where(d => 0 < d.IndexOf("netcdf", StringComparison.InvariantCultureIgnoreCase))
                            .Select(d => Path.Combine(d, "bin"))
                            .FirstOrDefault(d => File.Exists(Path.Combine(d, name)));
                        if (null != ncdir)
                        {
                            // if found we need to add the file location to PATH environmental variable to load dependent DLLs.
                            path = ncdir;
                            Environment.SetEnvironmentVariable("PATH",
                                Environment.GetEnvironmentVariable("PATH") + ";" + ncdir);
                        }
                    }
                }
            }
        }
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_open(string path, CreateMode mode, out int ncidp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_create(string path, CreateMode mode, out int ncidp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_close(int ncidp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_set_chunk_cache(IntPtr size, IntPtr nelems, float preemption);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_chunk_cache(out IntPtr size, out IntPtr nelems, out float preemption);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_sync(int ncid);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_enddef(int ncid);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_redef(int ncid);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_inq(int ncid, out int ndims, out int nvars, out int ngatts, out int unlimdimid);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_def_var(int ncid, string name, NcType xtype, int ndims, int[] dimids, out int varidp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_def_dim(int ncid, string name, IntPtr len, out int dimidp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_def_var_deflate(int ncid, int varid, int shuffle, int deflate, int deflate_level);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_def_var_chunking(int ncid, int varid, int contiguous, IntPtr[] chunksizes);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_inq_var(int ncid, int varid, StringBuilder name, out NcType type, out int ndims, int[] dimids, out int natts);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_inq_varids(int ncid, out int nvars, int[] varids);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_inq_vartype(int ncid, int varid, out NcType xtypep);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_inq_varnatts(int ncid, int varid, out int nattsp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_inq_varid(int ncid, string name, out int varidp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_inq_ndims(int ncid, out int ndims);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_inq_nvars(int ncid, out int nvars);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_inq_varname(int ncid, int varid, StringBuilder name);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_inq_varndims(int ncid, int varid, out int ndims);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_inq_vardimid(int ncid, int varid, int[] dimids);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_inq_natts(int ncid, out int ngatts);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_inq_unlimdim(int ncid, out int unlimdimid);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_inq_format(int ncid, out int format);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_inq_attname(int ncid, int varid, int attnum, StringBuilder name);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_inq_atttype(int ncid, int varid, string name, out NcType type);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_inq_att(int ncid, int varid, string name, out NcType type, out IntPtr length);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_att_text(int ncid, int varid, string name, byte[] value, int maxLength);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_att_schar(int ncid, int varid, string name, SByte[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_att_uchar(int ncid, int varid, string name, Byte[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_att_short(int ncid, int varid, string name, Int16[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_att_ushort(int ncid, int varid, string name, UInt16[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_att_int(int ncid, int varid, string name, Int32[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_att_uint(int ncid, int varid, string name, UInt32[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_att_longlong(int ncid, int varid, string name, Int64[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_att_ulonglong(int ncid, int varid, string name, UInt64[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_att_float(int ncid, int varid, string name, float[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_att_double(int ncid, int varid, string name, double[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_del_att(int ncid, int varid, string name);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_put_att_text(int ncid, int varid, string name, IntPtr len, byte[] tp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_put_att_string(int ncid, int varid, string name, IntPtr len, IntPtr[] tp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_put_att_double(int ncid, int varid, string name, NcType type, IntPtr len, double[] tp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_put_att_int(int ncid, int varid, string name, NcType type, IntPtr len, int[] tp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_put_att_short(int ncid, int varid, string name, NcType type, IntPtr len, short[] tp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_put_att_longlong(int ncid, int varid, string name, NcType type, IntPtr len, Int64[] tp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_put_att_ushort(int ncid, int varid, string name, NcType type, IntPtr len, UInt16[] tp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_put_att_uint(int ncid, int varid, string name, NcType type, IntPtr len, UInt32[] tp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_put_att_float(int ncid, int varid, string name, NcType type, IntPtr len, float[] tp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_put_att_ulonglong(int ncid, int varid, string name, NcType type, IntPtr len, UInt64[] tp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_put_att_schar(int ncid, int varid, string name, NcType type, IntPtr len, SByte[] tp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_put_att_ubyte(int ncid, int varid, string name, NcType type, IntPtr len, Byte[] tp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_inq_dim(int ncid, int dimid, StringBuilder name, out IntPtr length);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_inq_dimname(int ncid, int dimid, StringBuilder name);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_inq_dimid(int ncid, string name, out int dimid);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_inq_dimlen(int ncid, int dimid, out IntPtr length);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_att_string(int ncid, int varid, string name, IntPtr[] ip);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_var_text(int ncid, int varid, Byte[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_var_schar(int ncid, int varid, SByte[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_var_short(int ncid, int varid, short[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_var_int(int ncid, int varid, int[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_var_long(int ncid, int varid, long[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_var_float(int ncid, int varid, float[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_var_double(int ncid, int varid, double[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_put_vara_text(int ncid, int varid, IntPtr[] start, IntPtr[] count, byte[] dp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_put_vara_double(int ncid, int varid, IntPtr[] start, IntPtr[] count, double[] dp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_put_vara_float(int ncid, int varid, IntPtr[] start, IntPtr[] count, float[] dp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_put_vara_short(int ncid, int varid, IntPtr[] start, IntPtr[] count, short[] dp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_put_vara_ushort(int ncid, int varid, IntPtr[] start, IntPtr[] count, UInt16[] dp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_put_vara_int(int ncid, int varid, IntPtr[] start, IntPtr[] count, int[] ip);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_put_vara_uint(int ncid, int varid, IntPtr[] start, IntPtr[] count, UInt32[] dp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_put_vara_longlong(int ncid, int varid, IntPtr[] start, IntPtr[] count, Int64[] dp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_put_vara_ulonglong(int ncid, int varid, IntPtr[] start, IntPtr[] count, UInt64[] dp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_put_vara_ubyte(int ncid, int varid, IntPtr[] start, IntPtr[] count, Byte[] dp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_put_vara_schar(int ncid, int varid, IntPtr[] start, IntPtr[] count, SByte[] dp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_put_vara_string(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] sp);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_vara_text(int ncid, int varid, IntPtr[] start, IntPtr[] count, Byte[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_vara_schar(int ncid, int varid, IntPtr[] start, IntPtr[] count, SByte[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_vara_short(int ncid, int varid, IntPtr[] start, IntPtr[] count, short[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_vara_ushort(int ncid, int varid, IntPtr[] start, IntPtr[] count, UInt16[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_vara_ubyte(int ncid, int varid, IntPtr[] start, IntPtr[] count, Byte[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_vara_longlong(int ncid, int varid, IntPtr[] start, IntPtr[] count, Int64[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_vara_ulonglong(int ncid, int varid, IntPtr[] start, IntPtr[] count, UInt64[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_vara_int(int ncid, int varid, IntPtr[] start, IntPtr[] count, int[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_vara_uint(int ncid, int varid, IntPtr[] start, IntPtr[] count, UInt32[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_vara_float(int ncid, int varid, IntPtr[] start, IntPtr[] count, float[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_vara_double(int ncid, int varid, IntPtr[] start, IntPtr[] count, double[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_vara_string(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_var_string(int ncid, int varid, IntPtr[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_vars_text(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, Byte[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_vars_schar(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, SByte[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_vars_short(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, short[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_vars_ushort(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, UInt16[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_vars_ubyte(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, Byte[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_vars_longlong(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, Int64[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_vars_ulonglong(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, UInt64[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_vars_int(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, int[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_vars_uint(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, UInt32[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_vars_float(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, float[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_vars_double(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, double[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_get_vars_string(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, IntPtr[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern int nc_free_string(IntPtr len, IntPtr[] data);
        [DllImport("netcdf", CallingConvention=CallingConvention.Cdecl)] public static extern IntPtr nc_inq_libvers();
    }
}

