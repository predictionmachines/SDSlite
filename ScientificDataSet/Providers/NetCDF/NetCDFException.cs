// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetCDFInterop;

namespace Microsoft.Research.Science.Data.NetCDF4
{
	/// <summary>
	/// A fault within the unmanaged code of the NetCDF library.
	/// </summary>
    public class NetCDFException : Exception
    {
        private int resultCode;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="resultCode"></param>
        public NetCDFException(int resultCode) : 
            base(NetCDF.nc_strerror(resultCode))
        {
            this.resultCode = resultCode;
        }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
        public NetCDFException(string message) : base(message)
        {
            this.resultCode = -1;
        }
		/// <summary>
		/// Gets the unmanaged NetCDF result code, describing the fault.
		/// </summary>
        public int ResultCode
        {
            get { return resultCode; }
        }
    }
}

