// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;

namespace Microsoft.Research.Science.Data
{
	/// <summary>
	/// The exception that is thrown when a DataSet-related exceptional case occurs.
	/// </summary>
	[Serializable]
	public class DataSetException : ApplicationException
	{
		/// <summary>
		/// Initializes a new instance of the DataSetException class.
		/// </summary>
		public DataSetException()
		{
		}
		/// <summary>
		/// Initializes a new instance of the DataSetException class.
		/// </summary>
		/// <param name="message">Error message</param>
		public DataSetException(string message)
			: base(message)
		{
		}
		/// <summary>
		/// Initializes a new instance of the DataSetException class.
		/// </summary>
		/// <param name="innerException"></param>
		/// <param name="message">Error message</param>
		public DataSetException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
		/// <summary>Initializes a new instance of the <see cref="DataSetException"/> class
		/// with serialized data.
		/// </summary>
		/// <param name="info">The object that holds the serialized object data. </param>
		/// <param name="context">The contextual information about the source or destination.</param>
		protected DataSetException(System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}

