// Copyright © Microsoft Corporation, All Rights Reserved.
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
		/// <inheritdoc />
		public CsvParsingFailedException() { }

        /// <inheritdoc />
        public CsvParsingFailedException(string message) : base(message) { }
        /// <inheritdoc />
        public CsvParsingFailedException(string message, Exception inner) : base(message, inner) { }
        /// <inheritdoc />
        protected CsvParsingFailedException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}

