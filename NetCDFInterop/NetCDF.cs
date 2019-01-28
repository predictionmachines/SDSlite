using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

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
                    return typeof(SByte);
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
        public static int nc_open(string path, CreateMode mode, out int ncidp) { lock (namebuf) { return NetCDFDynamic.f_nc_open.Invoke(path, mode, out ncidp); } }
        public static int nc_open_chunked(string path, CreateMode mode, out int ncidp, IntPtr size, IntPtr nelems, float preemption)
        {
            lock (namebuf)
            {
                var r = NetCDFDynamic.f_nc_set_chunk_cache.Invoke(size, nelems, preemption);
                if (0 == r)
                    return NetCDFDynamic.f_nc_open.Invoke(path, mode, out ncidp);
                else
                { ncidp = 0; return r; }
            }
        }
        public static int nc_create(string path, CreateMode mode, out int ncidp) { lock (namebuf) { return NetCDFDynamic.f_nc_create.Invoke(path, mode, out ncidp); } }
        public static int nc_create_chunked(string path, CreateMode mode, out int ncidp, IntPtr size, IntPtr nelems, float preemption)
        {
            lock (namebuf)
            {
                var r = NetCDFDynamic.f_nc_set_chunk_cache.Invoke(size, nelems, preemption);
                if (0 == r)
                    return NetCDFDynamic.f_nc_create.Invoke(path, mode, out ncidp);
                else
                { ncidp = 0; return r; }
            }
        }
        public static int nc_close(int ncidp) { lock (namebuf) { return NetCDFDynamic.f_nc_close.Invoke(ncidp); } }
        public static int nc_set_chunk_cache(IntPtr size, IntPtr nelems, float preemption) { lock (namebuf) { return NetCDFDynamic.f_nc_set_chunk_cache.Invoke(size, nelems, preemption); } }
        public static int nc_get_chunk_cache(out IntPtr size, out IntPtr nelems, out float preemption) { lock (namebuf) { return NetCDFDynamic.f_nc_get_chunk_cache.Invoke(out size, out nelems, out preemption); } }
        public static int nc_sync(int ncid) { lock (namebuf) { return NetCDFDynamic.f_nc_sync.Invoke(ncid); } }
        public static int nc_enddef(int ncid) { lock (namebuf) { return NetCDFDynamic.f_nc_enddef.Invoke(ncid); } }
        public static int nc_redef(int ncid) { lock (namebuf) { return NetCDFDynamic.f_nc_redef.Invoke(ncid); } }
        public static int nc_inq(int ncid, out int ndims, out int nvars, out int ngatts, out int unlimdimid) { lock (namebuf) { return NetCDFDynamic.f_nc_inq.Invoke(ncid, out ndims, out nvars, out ngatts, out unlimdimid); } }
        public static int nc_def_grp(int ncid, string name, out int groupId) { return NetCDFDynamic.f_nc_def_grp.Invoke(ncid, name, out groupId); }
        public static int nc_inq_grp_ncid(int ncid, string name, out int groupId) { return NetCDFDynamic.f_nc_inq_grp_ncid.Invoke(ncid, name, out groupId); }
        public static int nc_def_var(int ncid, string name, NcType xtype, int[] dimids, out int varidp) { lock (namebuf) { return NetCDFDynamic.f_nc_def_var.Invoke(ncid, name, xtype, dimids.Length, dimids, out varidp); } }
        public static int nc_def_dim(int ncid, string name, IntPtr len, out int dimidp) { lock (namebuf) { return NetCDFDynamic.f_nc_def_dim.Invoke(ncid, name, len, out dimidp); } }
        public static int nc_def_var_deflate(int ncid, int varid, int shuffle, int deflate, int deflate_level) { lock (namebuf) { return NetCDFDynamic.f_nc_def_var_deflate.Invoke(ncid, varid, shuffle, deflate, deflate_level); } }
        public static int nc_def_var_chunking(int ncid, int varid, int contiguous, IntPtr[] chunksizes) { lock (namebuf) { return NetCDFDynamic.f_nc_def_var_chunking.Invoke(ncid, varid, contiguous, chunksizes); } }
        public static int nc_inq_var(int ncid, int varid, out string name, out NcType type, out int ndims, int[] dimids, out int natts)
        {
            lock (namebuf)
            {
                var r = NetCDFDynamic.f_nc_inq_var.Invoke(ncid, varid, namebuf, out type, out ndims, dimids, out natts);
                name = namebuf.ToString();
                return r;
            }
        }
        public static int nc_inq_varids(int ncid, out int nvars, int[] varids) { lock (namebuf) { return NetCDFDynamic.f_nc_inq_varids.Invoke(ncid, out nvars, varids); } }
        public static int nc_inq_vartype(int ncid, int varid, out NcType xtypep) { lock (namebuf) { return NetCDFDynamic.f_nc_inq_vartype.Invoke(ncid, varid, out xtypep); } }
        public static int nc_inq_varnatts(int ncid, int varid, out int nattsp) { lock (namebuf) { return NetCDFDynamic.f_nc_inq_varnatts.Invoke(ncid, varid, out nattsp); } }
        //public static int nc_inq_varid(int ncid, string name, out int varidp) { lock(namebuf) { return NetCDFDynamic.f_nc_inq_varid.Invoke(ncid, name, out varidp); } }
        //public static int nc_inq_ndims(int ncid, out int ndims) { lock(namebuf) { return NetCDFDynamic.f_nc_inq_ndims.Invoke(ncid, out ndims); } }
        //public static int nc_inq_nvars(int ncid, out int nvars) { lock(namebuf) { return NetCDFDynamic.f_nc_inq_nvars.Invoke(ncid, out nvars); } }
        //public static int nc_inq_varname(int ncid, int varid, out string name) { lock(namebuf) { return NetCDFDynamic.f_nc_inq_varname.Invoke(ncid, varid, out name); } }
        public static int nc_inq_varndims(int ncid, int varid, out int ndims) { lock (namebuf) { return NetCDFDynamic.f_nc_inq_varndims.Invoke(ncid, varid, out ndims); } }
        public static int nc_inq_vardimid(int ncid, int varid, int[] dimids) { lock (namebuf) { return NetCDFDynamic.f_nc_inq_vardimid.Invoke(ncid, varid, dimids); } }
        //public static int nc_inq_natts(int ncid, out int ngatts) { lock(namebuf) { return NetCDFDynamic.f_nc_inq_natts.Invoke(ncid, out ngatts); } }
        //public static int nc_inq_unlimdim(int ncid, out int unlimdimid) { lock(namebuf) { return NetCDFDynamic.f_nc_inq_unlimdim.Invoke(ncid, out unlimdimid); } }
        //public static int nc_inq_format(int ncid, out int format) { lock(namebuf) { return NetCDFDynamic.f_nc_inq_format.Invoke(ncid, out format); } }
        public static int nc_inq_attname(int ncid, int varid, int attnum, out string name)
        {
            lock (namebuf)
            {
                var r = NetCDFDynamic.f_nc_inq_attname.Invoke(ncid, varid, attnum, namebuf);
                name = namebuf.ToString();
                return r;
            }
        }
        public static int nc_inq_atttype(int ncid, int varid, string name, out NcType type) { lock (namebuf) { return NetCDFDynamic.f_nc_inq_atttype.Invoke(ncid, varid, name, out type); } }
        public static int nc_inq_att(int ncid, int varid, string name, out NcType type, out IntPtr length) { lock (namebuf) { return NetCDFDynamic.f_nc_inq_att.Invoke(ncid, varid, name, out type, out length); } }
        public static int nc_get_att_text(int ncid, int varid, string name, out string value, int maxLength)
        {
            lock (namebuf)
            {
                var b = new byte[maxLength + 2]; // in case netcdf adds terminating zero
                var r = NetCDFDynamic.f_nc_get_att_text.Invoke(ncid, varid, name, b, maxLength);
                var chars = new char[utfDecoder.GetCharCount(b, 0, maxLength)];
                utfDecoder.GetChars(b, 0, maxLength, chars, 0);
                value = new string(chars);
                return r;
            }
        }
        public static int nc_get_att_schar(int ncid, int varid, string name, SByte[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_att_schar.Invoke(ncid, varid, name, data); } }
        public static int nc_get_att_uchar(int ncid, int varid, string name, Byte[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_att_uchar.Invoke(ncid, varid, name, data); } }
        public static int nc_get_att_short(int ncid, int varid, string name, Int16[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_att_short.Invoke(ncid, varid, name, data); } }
        public static int nc_get_att_ushort(int ncid, int varid, string name, UInt16[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_att_ushort.Invoke(ncid, varid, name, data); } }
        public static int nc_get_att_int(int ncid, int varid, string name, Int32[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_att_int.Invoke(ncid, varid, name, data); } }
        public static int nc_get_att_uint(int ncid, int varid, string name, UInt32[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_att_uint.Invoke(ncid, varid, name, data); } }
        public static int nc_get_att_longlong(int ncid, int varid, string name, Int64[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_att_longlong.Invoke(ncid, varid, name, data); } }
        public static int nc_get_att_ulonglong(int ncid, int varid, string name, UInt64[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_att_ulonglong.Invoke(ncid, varid, name, data); } }
        public static int nc_get_att_float(int ncid, int varid, string name, float[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_att_float.Invoke(ncid, varid, name, data); } }
        public static int nc_get_att_double(int ncid, int varid, string name, double[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_att_double.Invoke(ncid, varid, name, data); } }
        public static int nc_del_att(int ncid, int varid, string name) { lock (namebuf) { return NetCDFDynamic.f_nc_del_att.Invoke(ncid, varid, name); } }
        public static int nc_put_att_text(int ncid, int varid, string name, string tp)
        {
            var buf_offset = WriteStrings(new string[] { tp });
            lock (namebuf)
            {
                var buf = buf_offset.Item1;
                var r = NetCDFDynamic.f_nc_put_att_text.Invoke(ncid, varid, name, new IntPtr(buf.Length - 1), buf);
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
                    r = NetCDFDynamic.f_nc_put_att_string.Invoke(ncid, varid, name, new IntPtr(bb.Length), bb);
                }
                return r;
            }
        }
        public static int nc_put_att_double(int ncid, int varid, string name, double[] tp) { lock (namebuf) { return NetCDFDynamic.f_nc_put_att_double.Invoke(ncid, varid, name, NcType.NC_DOUBLE, new IntPtr(tp.Length), tp); } }
        public static int nc_put_att_int(int ncid, int varid, string name, int[] tp) { lock (namebuf) { return NetCDFDynamic.f_nc_put_att_int.Invoke(ncid, varid, name, NcType.NC_INT, new IntPtr(tp.Length), tp); } }
        public static int nc_put_att_short(int ncid, int varid, string name, short[] tp) { lock (namebuf) { return NetCDFDynamic.f_nc_put_att_short.Invoke(ncid, varid, name, NcType.NC_SHORT, new IntPtr(tp.Length), tp); } }
        public static int nc_put_att_longlong(int ncid, int varid, string name, Int64[] tp) { lock (namebuf) { return NetCDFDynamic.f_nc_put_att_longlong.Invoke(ncid, varid, name, NcType.NC_INT64, new IntPtr(tp.Length), tp); } }
        public static int nc_put_att_ushort(int ncid, int varid, string name, UInt16[] tp) { lock (namebuf) { return NetCDFDynamic.f_nc_put_att_ushort.Invoke(ncid, varid, name, NcType.NC_USHORT, new IntPtr(tp.Length), tp); } }
        public static int nc_put_att_uint(int ncid, int varid, string name, UInt32[] tp) { lock (namebuf) { return NetCDFDynamic.f_nc_put_att_uint.Invoke(ncid, varid, name, NcType.NC_UINT, new IntPtr(tp.Length), tp); } }
        public static int nc_put_att_float(int ncid, int varid, string name, float[] tp) { lock (namebuf) { return NetCDFDynamic.f_nc_put_att_float.Invoke(ncid, varid, name, NcType.NC_FLOAT, new IntPtr(tp.Length), tp); } }
        public static int nc_put_att_ulonglong(int ncid, int varid, string name, UInt64[] tp) { lock (namebuf) { return NetCDFDynamic.f_nc_put_att_ulonglong.Invoke(ncid, varid, name, NcType.NC_UINT64, new IntPtr(tp.Length), tp); } }
        public static int nc_put_att_schar(int ncid, int varid, string name, SByte[] tp) { lock (namebuf) { return NetCDFDynamic.f_nc_put_att_schar.Invoke(ncid, varid, name, NcType.NC_BYTE, new IntPtr(tp.Length), tp); } }
        public static int nc_put_att_ubyte(int ncid, int varid, string name, Byte[] tp) { lock (namebuf) { return NetCDFDynamic.f_nc_put_att_ubyte.Invoke(ncid, varid, name, NcType.NC_UBYTE, new IntPtr(tp.Length), tp); } }
        //public static int nc_inq_dim(int ncid, int dimid, out string name, out IntPtr length) { lock(namebuf) { return NetCDFDynamic.f_nc_inq_dim.Invoke(ncid, dimid, out name, out length); } }
        public static int nc_inq_dimname(int ncid, int dimid, out string name)
        {
            lock (namebuf)
            {
                var r = NetCDFDynamic.f_nc_inq_dimname.Invoke(ncid, dimid, namebuf);
                name = namebuf.ToString();
                return r;
            }
        }
        public static int nc_inq_dimid(int ncid, string name, out int dimid) { lock (namebuf) { return NetCDFDynamic.f_nc_inq_dimid.Invoke(ncid, name, out dimid); } }
        public static int nc_inq_dimlen(int ncid, int dimid, out IntPtr length) { lock (namebuf) { return NetCDFDynamic.f_nc_inq_dimlen.Invoke(ncid, dimid, out length); } }
        public static int nc_get_att_string(int ncid, int varid, string name, string[] ip)
        {
            lock (namebuf)
            {
                var len = ip.Length;
                var parr = new IntPtr[len];
                var r = NetCDFDynamic.f_nc_get_att_string.Invoke(ncid, varid, name, parr);
                if (0 == r)
                {
                    for (int i = 0; i < len; i++) ip[i] = ReadString(parr[i]);
                    r = NetCDFDynamic.f_nc_free_string(new IntPtr(len), parr);
                }
                return r;
            }
        }
        //public static int nc_get_var_text(int ncid, int varid, Byte[] data) { lock(namebuf) { return NetCDFDynamic.f_nc_get_var_text.Invoke(ncid, varid, data); } }
        //public static int nc_get_var_schar(int ncid, int varid, SByte[] data) { lock(namebuf) { return NetCDFDynamic.f_nc_get_var_schar.Invoke(ncid, varid, data); } }
        //public static int nc_get_var_short(int ncid, int varid, short[] data) { lock(namebuf) { return NetCDFDynamic.f_nc_get_var_short.Invoke(ncid, varid, data); } }
        //public static int nc_get_var_int(int ncid, int varid, int[] data) { lock(namebuf) { return NetCDFDynamic.f_nc_get_var_int.Invoke(ncid, varid, data); } }
        //public static int nc_get_var_long(int ncid, int varid, long[] data) { lock(namebuf) { return NetCDFDynamic.f_nc_get_var_long.Invoke(ncid, varid, data); } }
        //public static int nc_get_var_float(int ncid, int varid, float[] data) { lock(namebuf) { return NetCDFDynamic.f_nc_get_var_float.Invoke(ncid, varid, data); } }
        //public static int nc_get_var_double(int ncid, int varid, double[] data) { lock(namebuf) { return NetCDFDynamic.f_nc_get_var_double.Invoke(ncid, varid, data); } }
        public static int nc_put_vara_double(int ncid, int varid, IntPtr[] start, IntPtr[] count, double[] dp) { lock (namebuf) { return NetCDFDynamic.f_nc_put_vara_double.Invoke(ncid, varid, start, count, dp); } }
        public static int nc_put_vara_float(int ncid, int varid, IntPtr[] start, IntPtr[] count, float[] dp) { lock (namebuf) { return NetCDFDynamic.f_nc_put_vara_float.Invoke(ncid, varid, start, count, dp); } }
        public static int nc_put_vara_short(int ncid, int varid, IntPtr[] start, IntPtr[] count, short[] dp) { lock (namebuf) { return NetCDFDynamic.f_nc_put_vara_short.Invoke(ncid, varid, start, count, dp); } }
        public static int nc_put_vara_ushort(int ncid, int varid, IntPtr[] start, IntPtr[] count, UInt16[] dp) { lock (namebuf) { return NetCDFDynamic.f_nc_put_vara_ushort.Invoke(ncid, varid, start, count, dp); } }
        public static int nc_put_vara_int(int ncid, int varid, IntPtr[] start, IntPtr[] count, int[] ip) { lock (namebuf) { return NetCDFDynamic.f_nc_put_vara_int.Invoke(ncid, varid, start, count, ip); } }
        public static int nc_put_vara_uint(int ncid, int varid, IntPtr[] start, IntPtr[] count, UInt32[] dp) { lock (namebuf) { return NetCDFDynamic.f_nc_put_vara_uint.Invoke(ncid, varid, start, count, dp); } }
        public static int nc_put_vara_longlong(int ncid, int varid, IntPtr[] start, IntPtr[] count, Int64[] dp) { lock (namebuf) { return NetCDFDynamic.f_nc_put_vara_longlong.Invoke(ncid, varid, start, count, dp); } }
        public static int nc_put_vara_ulonglong(int ncid, int varid, IntPtr[] start, IntPtr[] count, UInt64[] dp) { lock (namebuf) { return NetCDFDynamic.f_nc_put_vara_ulonglong.Invoke(ncid, varid, start, count, dp); } }
        public static int nc_put_vara_ubyte(int ncid, int varid, IntPtr[] start, IntPtr[] count, Byte[] dp) { lock (namebuf) { return NetCDFDynamic.f_nc_put_vara_ubyte.Invoke(ncid, varid, start, count, dp); } }
        public static int nc_put_vara_schar(int ncid, int varid, IntPtr[] start, IntPtr[] count, SByte[] dp) { lock (namebuf) { return NetCDFDynamic.f_nc_put_vara_schar.Invoke(ncid, varid, start, count, dp); } }
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
                    r = NetCDFDynamic.f_nc_put_vara_string.Invoke(ncid, varid, start, count, bb);
                }
                return r;
            }
        }
        //public static int nc_get_vara_text(int ncid, int varid, IntPtr[] start, IntPtr[] count, Byte[] data) { lock(namebuf) { return NetCDFDynamic.f_nc_get_vara_text.Invoke(ncid, varid, start, count, data); } }
        public static int nc_get_vara_schar(int ncid, int varid, IntPtr[] start, IntPtr[] count, SByte[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_vara_schar.Invoke(ncid, varid, start, count, data); } }
        public static int nc_get_vara_short(int ncid, int varid, IntPtr[] start, IntPtr[] count, short[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_vara_short.Invoke(ncid, varid, start, count, data); } }
        public static int nc_get_vara_ushort(int ncid, int varid, IntPtr[] start, IntPtr[] count, UInt16[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_vara_ushort.Invoke(ncid, varid, start, count, data); } }
        public static int nc_get_vara_ubyte(int ncid, int varid, IntPtr[] start, IntPtr[] count, Byte[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_vara_ubyte.Invoke(ncid, varid, start, count, data); } }
        public static int nc_get_vara_longlong(int ncid, int varid, IntPtr[] start, IntPtr[] count, Int64[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_vara_longlong.Invoke(ncid, varid, start, count, data); } }
        public static int nc_get_vara_ulonglong(int ncid, int varid, IntPtr[] start, IntPtr[] count, UInt64[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_vara_ulonglong.Invoke(ncid, varid, start, count, data); } }
        public static int nc_get_vara_int(int ncid, int varid, IntPtr[] start, IntPtr[] count, int[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_vara_int.Invoke(ncid, varid, start, count, data); } }
        public static int nc_get_vara_uint(int ncid, int varid, IntPtr[] start, IntPtr[] count, UInt32[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_vara_uint.Invoke(ncid, varid, start, count, data); } }
        public static int nc_get_vara_float(int ncid, int varid, IntPtr[] start, IntPtr[] count, float[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_vara_float.Invoke(ncid, varid, start, count, data); } }
        public static int nc_get_vara_double(int ncid, int varid, IntPtr[] start, IntPtr[] count, double[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_vara_double.Invoke(ncid, varid, start, count, data); } }
        public static int nc_get_vara_string(int ncid, int varid, IntPtr[] start, IntPtr[] count, string[] data)
        {
            lock (namebuf)
            {
                var len = data.Length;
                var parr = new IntPtr[len];
                var r = NetCDFDynamic.f_nc_get_vara_string.Invoke(ncid, varid, start, count, parr);
                if (0 == r)
                {
                    for (int i = 0; i < len; i++) data[i] = ReadString(parr[i]);
                    r = NetCDFDynamic.f_nc_free_string(new IntPtr(len), parr);
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
                var r = NetCDFDynamic.f_nc_get_var_string.Invoke(ncid, varid, parr);
                if (0 == r)
                {
                    for (int i = 0; i < len; i++) data[i] = ReadString(parr[i]);
                    r = NetCDFDynamic.f_nc_free_string(new IntPtr(len), parr);
                }
                return r;
            }
        }
        public static int nc_get_vars_text(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, Byte[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_vars_text.Invoke(ncid, varid, start, count, stride, data); } }
        public static int nc_get_vars_schar(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, SByte[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_vars_schar.Invoke(ncid, varid, start, count, stride, data); } }
        public static int nc_get_vars_short(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, short[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_vars_short.Invoke(ncid, varid, start, count, stride, data); } }
        public static int nc_get_vars_ushort(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, UInt16[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_vars_ushort.Invoke(ncid, varid, start, count, stride, data); } }
        public static int nc_get_vars_ubyte(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, Byte[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_vars_ubyte.Invoke(ncid, varid, start, count, stride, data); } }
        public static int nc_get_vars_longlong(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, Int64[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_vars_longlong.Invoke(ncid, varid, start, count, stride, data); } }
        public static int nc_get_vars_ulonglong(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, UInt64[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_vars_ulonglong.Invoke(ncid, varid, start, count, stride, data); } }
        public static int nc_get_vars_int(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, int[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_vars_int.Invoke(ncid, varid, start, count, stride, data); } }
        public static int nc_get_vars_uint(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, UInt32[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_vars_uint.Invoke(ncid, varid, start, count, stride, data); } }
        public static int nc_get_vars_float(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, float[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_vars_float.Invoke(ncid, varid, start, count, stride, data); } }
        public static int nc_get_vars_double(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, double[] data) { lock (namebuf) { return NetCDFDynamic.f_nc_get_vars_double.Invoke(ncid, varid, start, count, stride, data); } }
        public static int nc_get_vars_string(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, string[] data)
        {
            lock (namebuf)
            {
                var len = data.Length;
                var parr = new IntPtr[len];
                var r = NetCDFDynamic.f_nc_get_vars_string.Invoke(ncid, varid, start, count, stride, parr);
                if (0 == r)
                {
                    for (int i = 0; i < len; i++) data[i] = ReadString(parr[i]);
                    r = NetCDFDynamic.f_nc_free_string(new IntPtr(len), parr);
                }
                return r;
            }
        }
    }
    static class NetCDFDynamic
    {
        static DynamicInterop.UnmanagedDll native;
        private static string GetPath()
        {
            var platform = DynamicInterop.PlatformUtility.GetPlatform();
            switch (platform)
            {
                case PlatformID.Win32NT:
                    var name = "netcdf.dll";
                    // try find the file in current directory and then in directories from PATH environmental variable.
                    var path = Enumerable.Repeat(Environment.CurrentDirectory, 1)
                        .Concat(Environment.GetEnvironmentVariable("PATH").Split(';'))
                        .FirstOrDefault(d => File.Exists(Path.Combine(d, name)));
                    if (null == path)
                    {
                        // alternatively try standard install paths.
                        var ncdir = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86))
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
                    if (null == path) throw new FileNotFoundException(name + " not found in current directory nor on system path nor in ProgramFilesX86");
                    return Path.Combine(path, name);
                case PlatformID.Unix:
                    return "/usr/lib/libnetcdf.so.7";
                default:
                    throw new NotSupportedException(String.Format("Platform not supported: {0}", platform));
            }
        }
        static NetCDFDynamic()
        {
            // locate netcdf library
            var platform = DynamicInterop.PlatformUtility.GetPlatform();
            native = new DynamicInterop.UnmanagedDll(GetPath());
            f_nc_open = native.GetFunction<nc_open>();
            f_nc_create = native.GetFunction<nc_create>();
            f_nc_close = native.GetFunction<nc_close>();
            f_nc_set_chunk_cache = native.GetFunction<nc_set_chunk_cache>();
            f_nc_get_chunk_cache = native.GetFunction<nc_get_chunk_cache>();
            f_nc_sync = native.GetFunction<nc_sync>();
            f_nc_enddef = native.GetFunction<nc_enddef>();
            f_nc_redef = native.GetFunction<nc_redef>();
            f_nc_inq = native.GetFunction<nc_inq>();
            f_nc_def_var = native.GetFunction<nc_def_var>();
            f_nc_def_dim = native.GetFunction<nc_def_dim>();
            f_nc_def_var_deflate = native.GetFunction<nc_def_var_deflate>();
            f_nc_def_var_chunking = native.GetFunction<nc_def_var_chunking>();
            f_nc_def_grp = native.GetFunction<nc_def_grp>();
            f_nc_inq_grp_ncid = native.GetFunction<nc_inq_grp_ncid>();
            f_nc_inq_var = native.GetFunction<nc_inq_var>();
            f_nc_inq_varids = native.GetFunction<nc_inq_varids>();
            f_nc_inq_vartype = native.GetFunction<nc_inq_vartype>();
            f_nc_inq_varnatts = native.GetFunction<nc_inq_varnatts>();
            f_nc_inq_varid = native.GetFunction<nc_inq_varid>();
            f_nc_inq_ndims = native.GetFunction<nc_inq_ndims>();
            f_nc_inq_nvars = native.GetFunction<nc_inq_nvars>();
            f_nc_inq_varname = native.GetFunction<nc_inq_varname>();
            f_nc_inq_varndims = native.GetFunction<nc_inq_varndims>();
            f_nc_inq_vardimid = native.GetFunction<nc_inq_vardimid>();
            f_nc_inq_natts = native.GetFunction<nc_inq_natts>();
            f_nc_inq_unlimdim = native.GetFunction<nc_inq_unlimdim>();
            f_nc_inq_format = native.GetFunction<nc_inq_format>();
            f_nc_inq_attname = native.GetFunction<nc_inq_attname>();
            f_nc_inq_atttype = native.GetFunction<nc_inq_atttype>();
            f_nc_inq_att = native.GetFunction<nc_inq_att>();
            f_nc_get_att_text = native.GetFunction<nc_get_att_text>();
            f_nc_get_att_schar = native.GetFunction<nc_get_att_schar>();
            f_nc_get_att_uchar = native.GetFunction<nc_get_att_uchar>();
            f_nc_get_att_short = native.GetFunction<nc_get_att_short>();
            f_nc_get_att_ushort = native.GetFunction<nc_get_att_ushort>();
            f_nc_get_att_int = native.GetFunction<nc_get_att_int>();
            f_nc_get_att_uint = native.GetFunction<nc_get_att_uint>();
            f_nc_get_att_longlong = native.GetFunction<nc_get_att_longlong>();
            f_nc_get_att_ulonglong = native.GetFunction<nc_get_att_ulonglong>();
            f_nc_get_att_float = native.GetFunction<nc_get_att_float>();
            f_nc_get_att_double = native.GetFunction<nc_get_att_double>();
            f_nc_del_att = native.GetFunction<nc_del_att>();
            f_nc_put_att_text = native.GetFunction<nc_put_att_text>();
            f_nc_put_att_string = native.GetFunction<nc_put_att_string>();
            f_nc_put_att_double = native.GetFunction<nc_put_att_double>();
            f_nc_put_att_int = native.GetFunction<nc_put_att_int>();
            f_nc_put_att_short = native.GetFunction<nc_put_att_short>();
            f_nc_put_att_longlong = native.GetFunction<nc_put_att_longlong>();
            f_nc_put_att_ushort = native.GetFunction<nc_put_att_ushort>();
            f_nc_put_att_uint = native.GetFunction<nc_put_att_uint>();
            f_nc_put_att_float = native.GetFunction<nc_put_att_float>();
            f_nc_put_att_ulonglong = native.GetFunction<nc_put_att_ulonglong>();
            f_nc_put_att_schar = native.GetFunction<nc_put_att_schar>();
            f_nc_put_att_ubyte = native.GetFunction<nc_put_att_ubyte>();
            f_nc_inq_dim = native.GetFunction<nc_inq_dim>();
            f_nc_inq_dimname = native.GetFunction<nc_inq_dimname>();
            f_nc_inq_dimid = native.GetFunction<nc_inq_dimid>();
            f_nc_inq_dimlen = native.GetFunction<nc_inq_dimlen>();
            f_nc_get_att_string = native.GetFunction<nc_get_att_string>();
            f_nc_get_var_text = native.GetFunction<nc_get_var_text>();
            f_nc_get_var_schar = native.GetFunction<nc_get_var_schar>();
            f_nc_get_var_short = native.GetFunction<nc_get_var_short>();
            f_nc_get_var_int = native.GetFunction<nc_get_var_int>();
            f_nc_get_var_long = native.GetFunction<nc_get_var_long>();
            f_nc_get_var_float = native.GetFunction<nc_get_var_float>();
            f_nc_get_var_double = native.GetFunction<nc_get_var_double>();
            f_nc_put_vara_double = native.GetFunction<nc_put_vara_double>();
            f_nc_put_vara_float = native.GetFunction<nc_put_vara_float>();
            f_nc_put_vara_short = native.GetFunction<nc_put_vara_short>();
            f_nc_put_vara_ushort = native.GetFunction<nc_put_vara_ushort>();
            f_nc_put_vara_int = native.GetFunction<nc_put_vara_int>();
            f_nc_put_vara_uint = native.GetFunction<nc_put_vara_uint>();
            f_nc_put_vara_longlong = native.GetFunction<nc_put_vara_longlong>();
            f_nc_put_vara_ulonglong = native.GetFunction<nc_put_vara_ulonglong>();
            f_nc_put_vara_ubyte = native.GetFunction<nc_put_vara_ubyte>();
            f_nc_put_vara_schar = native.GetFunction<nc_put_vara_schar>();
            f_nc_put_vara_string = native.GetFunction<nc_put_vara_string>();
            f_nc_get_vara_text = native.GetFunction<nc_get_vara_text>();
            f_nc_get_vara_schar = native.GetFunction<nc_get_vara_schar>();
            f_nc_get_vara_short = native.GetFunction<nc_get_vara_short>();
            f_nc_get_vara_ushort = native.GetFunction<nc_get_vara_ushort>();
            f_nc_get_vara_ubyte = native.GetFunction<nc_get_vara_ubyte>();
            f_nc_get_vara_longlong = native.GetFunction<nc_get_vara_longlong>();
            f_nc_get_vara_ulonglong = native.GetFunction<nc_get_vara_ulonglong>();
            f_nc_get_vara_int = native.GetFunction<nc_get_vara_int>();
            f_nc_get_vara_uint = native.GetFunction<nc_get_vara_uint>();
            f_nc_get_vara_float = native.GetFunction<nc_get_vara_float>();
            f_nc_get_vara_double = native.GetFunction<nc_get_vara_double>();
            f_nc_get_vara_string = native.GetFunction<nc_get_vara_string>();
            f_nc_get_var_string = native.GetFunction<nc_get_var_string>();
            f_nc_get_vars_text = native.GetFunction<nc_get_vars_text>();
            f_nc_get_vars_schar = native.GetFunction<nc_get_vars_schar>();
            f_nc_get_vars_short = native.GetFunction<nc_get_vars_short>();
            f_nc_get_vars_ushort = native.GetFunction<nc_get_vars_ushort>();
            f_nc_get_vars_ubyte = native.GetFunction<nc_get_vars_ubyte>();
            f_nc_get_vars_longlong = native.GetFunction<nc_get_vars_longlong>();
            f_nc_get_vars_ulonglong = native.GetFunction<nc_get_vars_ulonglong>();
            f_nc_get_vars_int = native.GetFunction<nc_get_vars_int>();
            f_nc_get_vars_uint = native.GetFunction<nc_get_vars_uint>();
            f_nc_get_vars_float = native.GetFunction<nc_get_vars_float>();
            f_nc_get_vars_double = native.GetFunction<nc_get_vars_double>();
            f_nc_get_vars_string = native.GetFunction<nc_get_vars_string>();
            f_nc_free_string = native.GetFunction<nc_free_string>();
        }

        public static nc_open f_nc_open { get; private set; }
        public static nc_create f_nc_create { get; private set; }
        public static nc_close f_nc_close { get; private set; }
        public static nc_set_chunk_cache f_nc_set_chunk_cache { get; private set; }
        public static nc_get_chunk_cache f_nc_get_chunk_cache { get; private set; }
        public static nc_sync f_nc_sync { get; private set; }
        public static nc_enddef f_nc_enddef { get; private set; }
        public static nc_redef f_nc_redef { get; private set; }
        public static nc_inq f_nc_inq { get; private set; }
        public static nc_def_var f_nc_def_var { get; private set; }
        public static nc_def_dim f_nc_def_dim { get; private set; }
        public static nc_def_var_deflate f_nc_def_var_deflate { get; private set; }
        public static nc_def_var_chunking f_nc_def_var_chunking { get; private set; }
        public static nc_def_grp f_nc_def_grp { get; private set; }
        public static nc_inq_grp_ncid f_nc_inq_grp_ncid { get; private set; }
        public static nc_inq_var f_nc_inq_var { get; private set; }
        public static nc_inq_varids f_nc_inq_varids { get; private set; }
        public static nc_inq_vartype f_nc_inq_vartype { get; private set; }
        public static nc_inq_varnatts f_nc_inq_varnatts { get; private set; }
        public static nc_inq_varid f_nc_inq_varid { get; private set; }
        public static nc_inq_ndims f_nc_inq_ndims { get; private set; }
        public static nc_inq_nvars f_nc_inq_nvars { get; private set; }
        public static nc_inq_varname f_nc_inq_varname { get; private set; }
        public static nc_inq_varndims f_nc_inq_varndims { get; private set; }
        public static nc_inq_vardimid f_nc_inq_vardimid { get; private set; }
        public static nc_inq_natts f_nc_inq_natts { get; private set; }
        public static nc_inq_unlimdim f_nc_inq_unlimdim { get; private set; }
        public static nc_inq_format f_nc_inq_format { get; private set; }
        public static nc_inq_attname f_nc_inq_attname { get; private set; }
        public static nc_inq_atttype f_nc_inq_atttype { get; private set; }
        public static nc_inq_att f_nc_inq_att { get; private set; }
        public static nc_get_att_text f_nc_get_att_text { get; private set; }
        public static nc_get_att_schar f_nc_get_att_schar { get; private set; }
        public static nc_get_att_uchar f_nc_get_att_uchar { get; private set; }
        public static nc_get_att_short f_nc_get_att_short { get; private set; }
        public static nc_get_att_ushort f_nc_get_att_ushort { get; private set; }
        public static nc_get_att_int f_nc_get_att_int { get; private set; }
        public static nc_get_att_uint f_nc_get_att_uint { get; private set; }
        public static nc_get_att_longlong f_nc_get_att_longlong { get; private set; }
        public static nc_get_att_ulonglong f_nc_get_att_ulonglong { get; private set; }
        public static nc_get_att_float f_nc_get_att_float { get; private set; }
        public static nc_get_att_double f_nc_get_att_double { get; private set; }
        public static nc_del_att f_nc_del_att { get; private set; }
        public static nc_put_att_text f_nc_put_att_text { get; private set; }
        public static nc_put_att_string f_nc_put_att_string { get; private set; }
        public static nc_put_att_double f_nc_put_att_double { get; private set; }
        public static nc_put_att_int f_nc_put_att_int { get; private set; }
        public static nc_put_att_short f_nc_put_att_short { get; private set; }
        public static nc_put_att_longlong f_nc_put_att_longlong { get; private set; }
        public static nc_put_att_ushort f_nc_put_att_ushort { get; private set; }
        public static nc_put_att_uint f_nc_put_att_uint { get; private set; }
        public static nc_put_att_float f_nc_put_att_float { get; private set; }
        public static nc_put_att_ulonglong f_nc_put_att_ulonglong { get; private set; }
        public static nc_put_att_schar f_nc_put_att_schar { get; private set; }
        public static nc_put_att_ubyte f_nc_put_att_ubyte { get; private set; }
        public static nc_inq_dim f_nc_inq_dim { get; private set; }
        public static nc_inq_dimname f_nc_inq_dimname { get; private set; }
        public static nc_inq_dimid f_nc_inq_dimid { get; private set; }
        public static nc_inq_dimlen f_nc_inq_dimlen { get; private set; }
        public static nc_get_att_string f_nc_get_att_string { get; private set; }
        public static nc_get_var_text f_nc_get_var_text { get; private set; }
        public static nc_get_var_schar f_nc_get_var_schar { get; private set; }
        public static nc_get_var_short f_nc_get_var_short { get; private set; }
        public static nc_get_var_int f_nc_get_var_int { get; private set; }
        public static nc_get_var_long f_nc_get_var_long { get; private set; }
        public static nc_get_var_float f_nc_get_var_float { get; private set; }
        public static nc_get_var_double f_nc_get_var_double { get; private set; }
        public static nc_put_vara_double f_nc_put_vara_double { get; private set; }
        public static nc_put_vara_float f_nc_put_vara_float { get; private set; }
        public static nc_put_vara_short f_nc_put_vara_short { get; private set; }
        public static nc_put_vara_ushort f_nc_put_vara_ushort { get; private set; }
        public static nc_put_vara_int f_nc_put_vara_int { get; private set; }
        public static nc_put_vara_uint f_nc_put_vara_uint { get; private set; }
        public static nc_put_vara_longlong f_nc_put_vara_longlong { get; private set; }
        public static nc_put_vara_ulonglong f_nc_put_vara_ulonglong { get; private set; }
        public static nc_put_vara_ubyte f_nc_put_vara_ubyte { get; private set; }
        public static nc_put_vara_schar f_nc_put_vara_schar { get; private set; }
        public static nc_put_vara_string f_nc_put_vara_string { get; private set; }
        public static nc_get_vara_text f_nc_get_vara_text { get; private set; }
        public static nc_get_vara_schar f_nc_get_vara_schar { get; private set; }
        public static nc_get_vara_short f_nc_get_vara_short { get; private set; }
        public static nc_get_vara_ushort f_nc_get_vara_ushort { get; private set; }
        public static nc_get_vara_ubyte f_nc_get_vara_ubyte { get; private set; }
        public static nc_get_vara_longlong f_nc_get_vara_longlong { get; private set; }
        public static nc_get_vara_ulonglong f_nc_get_vara_ulonglong { get; private set; }
        public static nc_get_vara_int f_nc_get_vara_int { get; private set; }
        public static nc_get_vara_uint f_nc_get_vara_uint { get; private set; }
        public static nc_get_vara_float f_nc_get_vara_float { get; private set; }
        public static nc_get_vara_double f_nc_get_vara_double { get; private set; }
        public static nc_get_vara_string f_nc_get_vara_string { get; private set; }
        public static nc_get_var_string f_nc_get_var_string { get; private set; }
        public static nc_get_vars_text f_nc_get_vars_text { get; private set; }
        public static nc_get_vars_schar f_nc_get_vars_schar { get; private set; }
        public static nc_get_vars_short f_nc_get_vars_short { get; private set; }
        public static nc_get_vars_ushort f_nc_get_vars_ushort { get; private set; }
        public static nc_get_vars_ubyte f_nc_get_vars_ubyte { get; private set; }
        public static nc_get_vars_longlong f_nc_get_vars_longlong { get; private set; }
        public static nc_get_vars_ulonglong f_nc_get_vars_ulonglong { get; private set; }
        public static nc_get_vars_int f_nc_get_vars_int { get; private set; }
        public static nc_get_vars_uint f_nc_get_vars_uint { get; private set; }
        public static nc_get_vars_float f_nc_get_vars_float { get; private set; }
        public static nc_get_vars_double f_nc_get_vars_double { get; private set; }
        public static nc_get_vars_string f_nc_get_vars_string { get; private set; }
        public static nc_free_string f_nc_free_string { get; private set; }

        public delegate int nc_open(string path, CreateMode mode, out int ncidp);
        public delegate int nc_create(string path, CreateMode mode, out int ncidp);
        public delegate int nc_close(int ncidp);
        public delegate int nc_set_chunk_cache(IntPtr size, IntPtr nelems, float preemption);
        public delegate int nc_get_chunk_cache(out IntPtr size, out IntPtr nelems, out float preemption);
        public delegate int nc_sync(int ncid);
        public delegate int nc_enddef(int ncid);
        public delegate int nc_redef(int ncid);
        public delegate int nc_inq(int ncid, out int ndims, out int nvars, out int ngatts, out int unlimdimid);
        public delegate int nc_def_var(int ncid, string name, NcType xtype, int ndims, int[] dimids, out int varidp);
        public delegate int nc_def_dim(int ncid, string name, IntPtr len, out int dimidp);
        public delegate int nc_def_var_deflate(int ncid, int varid, int shuffle, int deflate, int deflate_level);
        public delegate int nc_def_var_chunking(int ncid, int varid, int contiguous, IntPtr[] chunksizes);
        public delegate int nc_def_grp(int ncid, string name, out int new_ncid);
        public delegate int nc_inq_grp_ncid(int ncid, string name, out int groupId);
        public delegate int nc_inq_var(int ncid, int varid, StringBuilder name, out NcType type, out int ndims, int[] dimids, out int natts);
        public delegate int nc_inq_varids(int ncid, out int nvars, int[] varids);
        public delegate int nc_inq_vartype(int ncid, int varid, out NcType xtypep);
        public delegate int nc_inq_varnatts(int ncid, int varid, out int nattsp);
        public delegate int nc_inq_varid(int ncid, string name, out int varidp);
        public delegate int nc_inq_ndims(int ncid, out int ndims);
        public delegate int nc_inq_nvars(int ncid, out int nvars);
        public delegate int nc_inq_varname(int ncid, int varid, StringBuilder name);
        public delegate int nc_inq_varndims(int ncid, int varid, out int ndims);
        public delegate int nc_inq_vardimid(int ncid, int varid, int[] dimids);
        public delegate int nc_inq_natts(int ncid, out int ngatts);
        public delegate int nc_inq_unlimdim(int ncid, out int unlimdimid);
        public delegate int nc_inq_format(int ncid, out int format);
        public delegate int nc_inq_attname(int ncid, int varid, int attnum, StringBuilder name);
        public delegate int nc_inq_atttype(int ncid, int varid, string name, out NcType type);
        public delegate int nc_inq_att(int ncid, int varid, string name, out NcType type, out IntPtr length);
        public delegate int nc_get_att_text(int ncid, int varid, string name, byte[] value, int maxLength);
        public delegate int nc_get_att_schar(int ncid, int varid, string name, SByte[] data);
        public delegate int nc_get_att_uchar(int ncid, int varid, string name, Byte[] data);
        public delegate int nc_get_att_short(int ncid, int varid, string name, Int16[] data);
        public delegate int nc_get_att_ushort(int ncid, int varid, string name, UInt16[] data);
        public delegate int nc_get_att_int(int ncid, int varid, string name, Int32[] data);
        public delegate int nc_get_att_uint(int ncid, int varid, string name, UInt32[] data);
        public delegate int nc_get_att_longlong(int ncid, int varid, string name, Int64[] data);
        public delegate int nc_get_att_ulonglong(int ncid, int varid, string name, UInt64[] data);
        public delegate int nc_get_att_float(int ncid, int varid, string name, float[] data);
        public delegate int nc_get_att_double(int ncid, int varid, string name, double[] data);
        public delegate int nc_del_att(int ncid, int varid, string name);
        public delegate int nc_put_att_text(int ncid, int varid, string name, IntPtr len, byte[] tp);
        public delegate int nc_put_att_string(int ncid, int varid, string name, IntPtr len, IntPtr[] tp);
        public delegate int nc_put_att_double(int ncid, int varid, string name, NcType type, IntPtr len, double[] tp);
        public delegate int nc_put_att_int(int ncid, int varid, string name, NcType type, IntPtr len, int[] tp);
        public delegate int nc_put_att_short(int ncid, int varid, string name, NcType type, IntPtr len, short[] tp);
        public delegate int nc_put_att_longlong(int ncid, int varid, string name, NcType type, IntPtr len, Int64[] tp);
        public delegate int nc_put_att_ushort(int ncid, int varid, string name, NcType type, IntPtr len, UInt16[] tp);
        public delegate int nc_put_att_uint(int ncid, int varid, string name, NcType type, IntPtr len, UInt32[] tp);
        public delegate int nc_put_att_float(int ncid, int varid, string name, NcType type, IntPtr len, float[] tp);
        public delegate int nc_put_att_ulonglong(int ncid, int varid, string name, NcType type, IntPtr len, UInt64[] tp);
        public delegate int nc_put_att_schar(int ncid, int varid, string name, NcType type, IntPtr len, SByte[] tp);
        public delegate int nc_put_att_ubyte(int ncid, int varid, string name, NcType type, IntPtr len, Byte[] tp);
        public delegate int nc_inq_dim(int ncid, int dimid, StringBuilder name, out IntPtr length);
        public delegate int nc_inq_dimname(int ncid, int dimid, StringBuilder name);
        public delegate int nc_inq_dimid(int ncid, string name, out int dimid);
        public delegate int nc_inq_dimlen(int ncid, int dimid, out IntPtr length);
        public delegate int nc_get_att_string(int ncid, int varid, string name, IntPtr[] ip);
        public delegate int nc_get_var_text(int ncid, int varid, Byte[] data);
        public delegate int nc_get_var_schar(int ncid, int varid, SByte[] data);
        public delegate int nc_get_var_short(int ncid, int varid, short[] data);
        public delegate int nc_get_var_int(int ncid, int varid, int[] data);
        public delegate int nc_get_var_long(int ncid, int varid, long[] data);
        public delegate int nc_get_var_float(int ncid, int varid, float[] data);
        public delegate int nc_get_var_double(int ncid, int varid, double[] data);
        public delegate int nc_put_vara_double(int ncid, int varid, IntPtr[] start, IntPtr[] count, double[] dp);
        public delegate int nc_put_vara_float(int ncid, int varid, IntPtr[] start, IntPtr[] count, float[] dp);
        public delegate int nc_put_vara_short(int ncid, int varid, IntPtr[] start, IntPtr[] count, short[] dp);
        public delegate int nc_put_vara_ushort(int ncid, int varid, IntPtr[] start, IntPtr[] count, UInt16[] dp);
        public delegate int nc_put_vara_int(int ncid, int varid, IntPtr[] start, IntPtr[] count, int[] ip);
        public delegate int nc_put_vara_uint(int ncid, int varid, IntPtr[] start, IntPtr[] count, UInt32[] dp);
        public delegate int nc_put_vara_longlong(int ncid, int varid, IntPtr[] start, IntPtr[] count, Int64[] dp);
        public delegate int nc_put_vara_ulonglong(int ncid, int varid, IntPtr[] start, IntPtr[] count, UInt64[] dp);
        public delegate int nc_put_vara_ubyte(int ncid, int varid, IntPtr[] start, IntPtr[] count, Byte[] dp);
        public delegate int nc_put_vara_schar(int ncid, int varid, IntPtr[] start, IntPtr[] count, SByte[] dp);
        public delegate int nc_put_vara_string(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] sp);
        public delegate int nc_get_vara_text(int ncid, int varid, IntPtr[] start, IntPtr[] count, Byte[] data);
        public delegate int nc_get_vara_schar(int ncid, int varid, IntPtr[] start, IntPtr[] count, SByte[] data);
        public delegate int nc_get_vara_short(int ncid, int varid, IntPtr[] start, IntPtr[] count, short[] data);
        public delegate int nc_get_vara_ushort(int ncid, int varid, IntPtr[] start, IntPtr[] count, UInt16[] data);
        public delegate int nc_get_vara_ubyte(int ncid, int varid, IntPtr[] start, IntPtr[] count, Byte[] data);
        public delegate int nc_get_vara_longlong(int ncid, int varid, IntPtr[] start, IntPtr[] count, Int64[] data);
        public delegate int nc_get_vara_ulonglong(int ncid, int varid, IntPtr[] start, IntPtr[] count, UInt64[] data);
        public delegate int nc_get_vara_int(int ncid, int varid, IntPtr[] start, IntPtr[] count, int[] data);
        public delegate int nc_get_vara_uint(int ncid, int varid, IntPtr[] start, IntPtr[] count, UInt32[] data);
        public delegate int nc_get_vara_float(int ncid, int varid, IntPtr[] start, IntPtr[] count, float[] data);
        public delegate int nc_get_vara_double(int ncid, int varid, IntPtr[] start, IntPtr[] count, double[] data);
        public delegate int nc_get_vara_string(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] data);
        public delegate int nc_get_var_string(int ncid, int varid, IntPtr[] data);
        public delegate int nc_get_vars_text(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, Byte[] data);
        public delegate int nc_get_vars_schar(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, SByte[] data);
        public delegate int nc_get_vars_short(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, short[] data);
        public delegate int nc_get_vars_ushort(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, UInt16[] data);
        public delegate int nc_get_vars_ubyte(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, Byte[] data);
        public delegate int nc_get_vars_longlong(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, Int64[] data);
        public delegate int nc_get_vars_ulonglong(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, UInt64[] data);
        public delegate int nc_get_vars_int(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, int[] data);
        public delegate int nc_get_vars_uint(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, UInt32[] data);
        public delegate int nc_get_vars_float(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, float[] data);
        public delegate int nc_get_vars_double(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, double[] data);
        public delegate int nc_get_vars_string(int ncid, int varid, IntPtr[] start, IntPtr[] count, IntPtr[] stride, IntPtr[] data);
        public delegate int nc_free_string(IntPtr len, IntPtr[] data);
    }
}

