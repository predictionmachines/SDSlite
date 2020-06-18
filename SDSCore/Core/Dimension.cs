// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data
{
	/// <summary>
	/// Represents a dimension of a variable.
	/// </summary>
    public struct Dimension
    {
        private string name;
        private int length;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="length"></param>
        internal Dimension(string name, int length)
        {
            this.name = name;
            this.length = length;
        }

		/// <summary>
		/// Gets the name of the dimension.
		/// </summary>
        public string Name 
        { 
            get { return name; }
            internal set { name = value; }
        }

		/// <summary>
		/// Gets the length of the dimension.
		/// </summary>
		/// <remarks>
		/// This is a length of data of all variables depending on it, by the corresponding dimension.
		/// </remarks>
        public int Length
        {
            get
            {
                return length;
            }
            internal set
            {
                length = value;
            }
        }       

		/// <summary>
		/// Represents the dimension as a text.
		/// </summary>
		/// <returns></returns>
        public override string ToString()
        {
            return string.Format("({0}:{1})", String.IsNullOrEmpty(name) ? "_" : name, length);
        }

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>The hash code for this instance.</returns>
		public override int GetHashCode()
		{
			if (name != null)
				return name.GetHashCode();
			return 0;
		}
    }
}

