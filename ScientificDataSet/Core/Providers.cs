// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Microsoft.Research.Science.Data
{  
	/// <summary>
	/// Determines how to find out the value.
	/// </summary>
    public enum ReverseIndexSelection
    {
		/// <summary>
		/// Return an exact value when given coordinates are exactly found;
        /// otherwise, throw an exception.
		/// </summary>
        Exact, 
		/// <summary>
		/// If no exact value is found for given coordinates, take nearest exact value.
		/// </summary>
		Nearest, 
		/// <summary>
        /// If no exact value is found for given coordinates, interpolate the value from 
        /// its exact neighbours.
		/// </summary>
		Interpolation
    }
}

