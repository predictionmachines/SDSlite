// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data
{
	/// <summary>
	/// Designated to contain metadata dictionary without data.
	/// </summary>
	internal class MetadataContainerVariable : Variable<EmptyValueType>
	{
		private MetadataDictionary metadata;

		public MetadataContainerVariable(DataSet dataSet) :
			base(dataSet, new string[0] { })
		{
			metadata = new MetadataDictionary();

			Initialize();
		}

		public override MetadataDictionary Metadata
		{
			get { return metadata; }
		}

		public override Array GetData(int[] origin, int[] shape)
		{
			if ((origin == null || origin.Length == 0) &&
				(shape == null || shape.Length == 0))
				return new EmptyValueType[] { new EmptyValueType() };
			throw new ArgumentException("The variable is scalar therefore given arguments are incorrect");
		}

		protected override int[] ReadShape()
		{
			return new int[0];
		}

		public override void PutData(int[] origin, Array a)
		{
			throw new NotSupportedException("MetadataContainerVariable contains metadata only");
		}

		public override void Append(Array a, int dimToAppend)
		{
			throw new NotSupportedException("MetadataContainerVariable contains metadata only");
		}
	}
}

