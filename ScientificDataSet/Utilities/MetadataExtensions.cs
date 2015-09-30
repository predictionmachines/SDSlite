// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Science.Data;

namespace Microsoft.Research.Science.Data.Utilities
{
    public static class MetadataExtensions
    {
        /// <summary>
        /// Gets the value of the attribute with name "min" if it is presented;
        /// otherwise returns <paramref name="default"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="metadata"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public static T GetMin<T>(this Dictionary<string, object> metadata, T @default)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            if (metadata.ContainsKey("min"))
                return (T)metadata["min"];
            return @default;
        }

        /// <summary>
        /// Gets the value of the attribute with name "min" if it is presented;
        /// otherwise returns <paramref name="default"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="metadata"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public static T GetMax<T>(this Dictionary<string, object> metadata, T @default)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            if (metadata.ContainsKey("max"))
                return (T)metadata["max"];
            return @default;
        }

        /// <summary>
        /// Gets the value of the attribute with name "min" if it is presented;
        /// otherwise returns null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="metadata"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public static object GetMin(this Dictionary<string, object> metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            if (metadata.ContainsKey("min"))
                return metadata["min"];
            return null;
        }

        /// <summary>
        /// Gets the value of the attribute with name "max" if it is presented;
        /// otherwise returns null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="metadata"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public static object GetMax(this Dictionary<string, object> metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            if (metadata.ContainsKey("max"))
                return metadata["max"];
            return null;
        }

        /// <summary>
        /// Gets the value of the attribute with name "min" if it is presented;
        /// otherwise returns <paramref name="default"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="metadata"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public static T GetMin<T>(this MetadataDictionary metadata, T @default)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            if (metadata.ContainsKey("min"))
                return (T)metadata["min"];
            return @default;
        }

        /// <summary>
        /// Gets the value of the attribute with name "min" if it is presented;
        /// otherwise returns <paramref name="default"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="metadata"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public static T GetMax<T>(this MetadataDictionary metadata, T @default)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            if (metadata.ContainsKey("max"))
                return (T)metadata["max"];
            return @default;
        }

        /// <summary>
        /// Gets the value of the attribute with name "min" if it is presented;
        /// otherwise returns null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="metadata"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public static object GetMin(this MetadataDictionary metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            if (metadata.ContainsKey("min"))
                return metadata["min"];
            return null;
        }

        /// <summary>
        /// Gets the value of the attribute with name "max" if it is presented;
        /// otherwise returns null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="metadata"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public static object GetMax(this MetadataDictionary metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            if (metadata.ContainsKey("max"))
                return metadata["max"];
            return null;
        }

        public static object GetMissingValue(this Variable var)
        {
            if (var == null) throw new NullReferenceException("var is undefined");
            object mv = var.MissingValue;
            if (mv != null) return mv;
            if (var.Metadata.ContainsKey("missing_value"))
                return var.Metadata["missing_value"];
            return null;
        }

        /// <summary>
        /// Returns the display name from the metadata, if exists.
        /// Otherwise, returns the name of the variable.
        /// If there is no name, returns empty string.
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static string GetDisplayName(this MetadataDictionary metadata)
        {
            string dname = GetDisplayNameOnly(metadata);
            if (!String.IsNullOrEmpty(dname)) return dname;
            if (metadata.ContainsKey(metadata.KeyForName))
                return (string)metadata[metadata.KeyForName];
            return String.Empty;
        }

        public static string GetDisplayNameOnly(this MetadataDictionary metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            if (metadata.ContainsKey("DisplayName", true))
            {
                object o = metadata["DisplayName", true];
                if (o != null) return o.ToString();
            }
            else if (metadata.ContainsKey("long_name", true))
            {
                object o = metadata["long_name", true];
                if (o != null) return o.ToString();
            }
            else if (metadata.ContainsKey("longname", true))
            {
                object o = metadata["longname", true];
                if (o != null) return o.ToString();
            }
            return null;
        }

        /// <summary>
        /// Gets the value of "units" attribute, if exists; otherwise, returns null.
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static string GetUnits(this MetadataDictionary metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            if (metadata.ContainsKey("Units", true))
                return metadata["Units", true].ToString();
            return null;
        }

        public static string GetDisplayNameAndUnits(this MetadataDictionary metadata)
        {
            string dn = metadata.GetDisplayName();
            if (String.IsNullOrEmpty(dn)) return String.Empty;
            string units = metadata.GetUnits();
            if (String.IsNullOrEmpty(units)) return dn;

            StringBuilder sb = new StringBuilder(dn);
            sb.Append(" (").Append(units).Append(")");
            return sb.ToString();
        }

        private static readonly string[] coordinateUnits = new string[]
			{ "meter", "rad", "mile", "angstrom",
				"astronomical_unit", "feet", "inch",
				"cm"
			};

        /// <summary>
        /// Checks whether the variable is a coordinate axis or not.
        /// </summary>
        /// <param name="var"></param>
        /// <returns></returns>
        public static bool IsCoordinateAxis(this Variable var)
        {
            if (var == null)
                throw new ArgumentNullException("var");

            if (var.TypeOfData == typeof(DateTime))
                return true;

            if (GeoConventions.IsLatitude(var) || GeoConventions.IsLongitude(var))
                return true;

            string units = var.Metadata.GetUnits();
            if (String.IsNullOrEmpty(units))
                return false;

            units = units.ToLower();
            foreach (var cu in coordinateUnits)
                if (units.Contains(cu)) return true;

            return false;
        }

        public static long TotalLength(this Variable var)
        {
            if (var == null)
                throw new ArgumentNullException("var");
            return var.GetShape().Select(i => (long)i).Aggregate((i, j) => i * j);
        }
    }
}

