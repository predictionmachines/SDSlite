// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Research.Science.Data
{
	internal class IndexTransformVariable<DataType> : PureComputationalVariable<DataType>
	{
		private readonly Func<int[], DataType> indexLambda;


		internal IndexTransformVariable(DataSet sds, string name, string[] dims,
			Func<int[], DataType> indexTransform)
			: base(sds, name, dims)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (sds == null) throw new ArgumentNullException("sds");
			if (dims == null) throw new ArgumentNullException("dims");
			if (indexTransform == null) throw new ArgumentNullException("indexTransform");
			this.indexLambda = indexTransform;
			IsReadOnly = true;

			Initialize();
		}

		public override Array GetData(int[] origin, int[] shape)
		{
			int[] lShape = this.GetShape();
			if (shape == null) shape = this.GetShape();
			if (origin == null)
			{
				origin = new int[this.Rank];
				for (int i = 0; i < this.Rank; i++) origin[i] = 0;
			}
			for (int i = 0; i < this.Rank; i++)
				if (shape[i] <= 0) throw new Exception(
					  "Shape can't be nonpositive");
			for (int i = 0; i < this.Rank; i++)
				if (origin[i] + shape[i] > lShape[i])
					throw new Exception("Index of requested data is out of range");


			Array A = Array.CreateInstance(typeof(DataType), shape);

			int[] ind = new int[this.Rank];
			for (int i = 0; i < this.Rank; i++) ind[i] = 0;

			int[] argI = new int[this.Rank];

			while (ind[0] < shape[0])
			{
				for (int k = 0; k < this.Rank; k++) argI[k] = origin[k] + ind[k];

				A.SetValue(this.indexLambda(argI), ind);

				for (int j = this.Rank - 1; j >= 0; j--)
				{
					ind[j]++;
					if (ind[j] < shape[j]) break;
					else
						if (j > 0) ind[j] = 0;
				}
			}

			return A;
		}
	}

	internal class RangeVariable : IndexTransformVariable<int>
	{
		internal RangeVariable(DataSet sds, string name, string[] dims, int origin)
			: base(sds, name, dims, (int[] i) => ((int)(i[0] + origin)))
		{
			if ((dims != null) && (dims.Length != 1))
				throw new Exception("Must be one dimensions in RangeVariable");
		}
	}

}

