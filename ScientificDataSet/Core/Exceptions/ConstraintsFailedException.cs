// Copyright © Microsoft Corporation, All Rights Reserved.
using System;

namespace Microsoft.Research.Science.Data
{
	/// <summary>
	/// The exception that is thrown when the consistency constraints are failed for the data set.
	/// </summary>
	[Serializable]
	public class ConstraintsFailedException : DataSetException
	{
        /// <summary>Initializes a new instance of the <see cref="ConstraintsFailedException"/> class. </summary>
		public ConstraintsFailedException() { }
        /// <summary>Initializes a new instance of the <see cref="ConstraintsFailedException"/> 
        /// class with a specified error message. </summary>
        /// <param name="message">Error message</param>
        public ConstraintsFailedException(string message) : base(message) { }
        /// <summary>Initializes a new instance of the <see cref="ConstraintsFailedException"/> class
        /// with a specified error message and a reference to the inner exception that is the cause of this exception. 
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="inner">Exception that causes this exception</param>
        public ConstraintsFailedException(string message, Exception inner) : base(message, inner) { }
        /// <summary>Initializes a new instance of the <see cref="ConstraintsFailedException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data. </param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected ConstraintsFailedException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}

