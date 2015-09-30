// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data
{
	/// <summary>
	/// Stores changes of the metadata dictionary.
	/// </summary>
	public class MetadataChanges : MetadataDictionary
	{
		internal MetadataChanges(Dictionary<string, object> dictionary)
			: base(true, dictionary)
		{
		}
        /// <summary>
        /// Gets the attribute value for the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Key of the attribute to get.</param>
        /// <returns>Attribute value.</returns>
		public override object this[string key]
		{
			get
			{
				return base[key];
			}
			set
			{
				CheckKey(key);
				CheckValue(value);

				// Constraints on name
				if (key == KeyForName)
				{
					if (value != null && !(value is string))
						throw new Exception("Name of a variable must be a string");
				}

				dictionary[key] = value;
			}
		}
	}
}

