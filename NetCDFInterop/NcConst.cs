using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCDFInterop
{
	public static class NcConst 
	{
		/// <summary>
		/// 'size' argument to ncdimdef for an unlimited dimension
		/// </summary>
		public const int NC_UNLIMITED = 0;
		 /// <summary>
        /// Variable id to put/get a global attribute
        /// </summary>
        public const int NC_GLOBAL = -1;
	};
}
