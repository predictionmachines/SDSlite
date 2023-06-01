// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Science.Data;

namespace Microsoft.Research.Science.Data.Utilities
{
    /// <summary>
    /// Provides extension methods for a metadata dictionary.
    /// </summary>
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
        /// <param name="metadata"></param>
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
        /// <param name="metadata"></param>
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
        /// <param name="metadata"></param>
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
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static object GetMax(this MetadataDictionary metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            if (metadata.ContainsKey("max"))
                return metadata["max"];
            return null;
        }

        /// <summary>
        /// A value to signify an empty slot in variable data.
        /// </summary>
        /// <param name="var">The variable.</param>
        /// <returns><see cref="Variable.MissingValue"/> or a value of "missing_value" attribute.</returns>
        /// <exception cref="NullReferenceException"></exception>
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

        /// <summary>
        /// Display name.
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
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

        /// <summary>
        /// Display name and units.
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Total number of data elements in the variable.
        /// </summary>
        /// <param name="var"></param>
        /// <returns>A product of all dimensions.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static long TotalLength(this Variable var)
        {
            if (var == null)
                throw new ArgumentNullException("var");
            return var.GetShape().Select(i => (long)i).Aggregate((i, j) => i * j);
        }
    }
}

