using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCDFInterop
{
	public enum  ResultCode : int
	{
		/// <summary>No Error</summary>
		NC_NOERR = 0,
		/// <summary>Invalid dimension id or name</summary>
		NC_EBADDIM = -46,
		/// <summary>Attribute not found</summary>
		NC_ENOTATT = -43,
		/// <summary>Variable name is in use</summary>
		NC_ENAMEINUSE = -42
	};
}
