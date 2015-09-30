using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCDFInterop
{
   /// <summary>
    /// The netcdf external data types
    /// </summary>
    public enum NcType : int
    {
        /// <summary>signed 1 byte integer</summary>
        NC_BYTE = 1,
		/// <summary>unsigned 1 byte int</summary>
		NC_UBYTE = 7,	
        /// <summary>ISO/ASCII character</summary>
        NC_CHAR = 2,
        /// <summary>signed 2 byte integer</summary>
        NC_SHORT = 3,
        /// <summary>unsigned 2 byte integer</summary>
        NC_USHORT = 8,
        /// <summary>signed 4 byte integer</summary>
        NC_INT = 4,
		/// <summary>unsigned 4-byte int </summary>
		NC_UINT = 9,
        /// <summary>single precision floating point number</summary>
        NC_FLOAT = 5,
        /// <summary>double precision floating point number</summary>
        NC_DOUBLE = 6,
        /// <summary>signed 8-byte int</summary>
        NC_INT64 = 10,
        /// <summary>unsigned 8-byte int</summary>
        NC_UINT64 = 11,
        /// <summary>string</summary>
        NC_STRING =	12	
    };	
}
