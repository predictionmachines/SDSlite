// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data.Utilities
{
	public static class GeoConventions
	{
		public static bool IsLatitude(Variable v)
		{
			string units = v.Metadata.GetUnits();
			if (!String.IsNullOrEmpty(units))
			{
				units = units.ToLower();

				// The recommended unit of latitude is degrees_north. 
				// Also acceptable are degree_north, degree_N, degrees_N, degreeN, and degreesN. 
                if (units.Contains("degree") && (units.EndsWith("north") || units.EndsWith("n")))
					return true;
			}
			// Check if name indicates latitute
			string name = v.Name.ToLower();
			return (name.StartsWith("lat") || name.StartsWith("_lat") || name.Contains("latitude"));
		}

		public static bool IsLongitude(Variable v)
		{
			string units = v.Metadata.GetUnits();
			if (!String.IsNullOrEmpty(units))
			{
				units = units.ToLower();

				// The recommended unit of longitude is degrees_east. 
				// Also acceptable are degree_east, degree_E, degrees_E, degreeE, and degreesE. 
                if (units.Contains("degree") && (units.EndsWith("east") || units.EndsWith("e")))
					return true;
			}
			// Check if name indicates longitude
			string name = v.Name.ToLower();
            return (name.StartsWith("lon") || name.StartsWith("_lon") || name.Contains("longitude"));
		}
	}
}

