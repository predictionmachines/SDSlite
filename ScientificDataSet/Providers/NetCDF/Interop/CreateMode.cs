using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCDFInterop
{
	public enum CreateMode : int
	{
		NC_NOWRITE = 0,
		/// <summary>read &amp; write</summary>
		NC_WRITE = 0x0001,
		NC_CLOBBER = 0,
		/// <summary>Don't destroy existing file on create</summary>
		NC_NOCLOBBER = 0x0004,
		/// <summary>argument to ncsetfill to clear NC_NOFILL</summary>
		NC_FILL = 0,
		/// <summary>Don't fill data section an records</summary>
		NC_NOFILL = 0x0100,
		/// <summary>Use locking if available</summary>
		NC_LOCK = 0x0400,
		/// <summary>Share updates, limit cacheing</summary>
		NC_SHARE = 0x0800,
		NC_64BIT_OFFSET = 0x0200,
		/// <summary>Enforce strict netcdf-3 rules</summary>
		NC_CLASSIC = 0x0100,
		/// <summary>causes netCDF to create a HDF5/NetCDF-4 file</summary>
		NC_NETCDF4 = 0x1000
	};
}
