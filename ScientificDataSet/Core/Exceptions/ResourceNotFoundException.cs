// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data
{
	/// <summary>
	/// The exception that is thrown when a particular resource is not found.
	/// </summary>
	[Serializable]
	public class ResourceNotFoundException : DataSetException
	{
		/// <summary>
		/// 
		/// </summary>
		public ResourceNotFoundException() { }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="resourceName"></param>
		public ResourceNotFoundException(string resourceName) 
			: base(String.Format("Resource {0} not found", resourceName)) { }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="resourceName"></param>
		/// <param name="inner"></param>
		public ResourceNotFoundException(string resourceName, Exception inner)
			: base(String.Format("Resource {0} not found", resourceName), inner) { }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected ResourceNotFoundException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}

