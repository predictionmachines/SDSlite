// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data
{
	/// <summary>
	/// The exception that is thrown if during distributed commit of DataSets linked by reference
	/// variables, some of these DataSets failed to commit its changes.
	/// </summary>
	[Serializable]
	public class DistributedCommitFailedException : DataSetException
	{
		private DataSet failed;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="failedDataSet"></param>
		public DistributedCommitFailedException(DataSet failedDataSet) : base("DataSet " + failedDataSet.URI + " commit failed") 
		{
			failed = failedDataSet;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="failedDataSet"></param>
		/// <param name="inner"></param>
		public DistributedCommitFailedException(DataSet failedDataSet, Exception inner)
			: base("DataSet " + failedDataSet.URI + " commit failed", inner) 
		{
			failed = failedDataSet;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected DistributedCommitFailedException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }

		/// <summary>
		/// Gets the data set that is unable to commit.
		/// </summary>
		public DataSet FailedDataSet
		{
			get { return failed; }
		}
	}
}

