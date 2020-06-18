// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
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

