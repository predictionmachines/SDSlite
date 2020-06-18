// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data.CSV
{
    /// <summary>
    /// The exception that is thrown when <see cref="CsvDataSet"/> cannot parse input file.
    /// </summary>
	[global::System.Serializable]
	public class CsvParsingFailedException : ApplicationException
	{
		public CsvParsingFailedException() { }
		public CsvParsingFailedException(string message) : base(message) { }
		public CsvParsingFailedException(string message, Exception inner) : base(message, inner) { }
		protected CsvParsingFailedException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}

