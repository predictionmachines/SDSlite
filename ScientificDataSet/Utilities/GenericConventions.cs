using System;

namespace Microsoft.Research.Science.Data.Utilities
{
    public class Conventions
    {
        public static bool IsSpatialAxis(Variable v)
        {
            if(v.Rank != 1 || v.TypeOfData == typeof(String) || v.TypeOfData == typeof(DateTime)) 
                return false;
            return GeoConventions.IsLatitude(v) || GeoConventions.IsLongitude(v) || v.Name == "x" || v.Name == "y" ;
        }
    }
}