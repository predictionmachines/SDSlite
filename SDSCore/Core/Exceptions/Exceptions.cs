// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;

namespace Microsoft.Research.Science.Data
{
	/// <summary>
	/// Represents an error that occurs when an action cannot be performed.
	/// </summary>
	public class CannotPerformActionException : DataSetException
	{
		/// <summary>
		/// 
		/// </summary>
		public CannotPerformActionException()
			: base("Value not found")
		{
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		public CannotPerformActionException(string message)
			: base(message)
		{
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public CannotPerformActionException(string message, Exception innerException)
			: base(message, innerException)
		{

		}
	}

    /// <summary>
    /// Represents an error that occurs when there is no corresponded value or it cannot be calculated.
    /// </summary>
    public class ValueNotFoundException : ApplicationException
    {
		/// <summary>
		/// 
		/// </summary>
        public ValueNotFoundException()
            : base("Value not found")
        {
        }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
        public ValueNotFoundException(string message)
            : base(message)
        {
        }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
        public ValueNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }

	/// <summary>
	/// Represents an error when someone tries to modify a read only instance.
	/// </summary>
	[Serializable]
	public class ReadOnlyException : DataSetException
	{
		/// <summary>
		/// 
		/// </summary>
		public ReadOnlyException() { }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		public ReadOnlyException(string message) : base(message) { }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <param name="inner"></param>
		public ReadOnlyException(string message, Exception inner) : base(message, inner) { }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected ReadOnlyException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}

