// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Microsoft.Research.Science.Data.CSV
{
    internal sealed class CsvVariableScalar<DataType> : CsvVariable<DataType>
    {
        internal CsvVariableScalar(CsvDataSet dataSet, CsvColumn column)
            : base(dataSet, column)
        {
            if (column.Rank != 0)
                throw new Exception("This is a scalar variable and is being created with different rank.");

			data = new ArrayWrapper(0, typeof(DataType));//new OrderedArray1d(typeof(DataType), false);

            Initialize();
        }

		private CsvVariableScalar(CsvDataSet dataSet, int id, MetadataDictionary metadata, ArrayWrapper data, string[] dims)
			: base(dataSet, id, metadata, dims)
		{
			this.data = data;
			Initialize();
		}

		public override Variable CloneAndRenameDims(string[] newDims)
		{
			if (newDims != null && newDims.Length != 0)
				throw new Exception("New dimensions are wrong");
			Variable var = new CsvVariableScalar<DataType>((CsvDataSet)DataSet, ID, Metadata, data, newDims);
			return var;
		}

        /// <summary>
        /// Builds and returns inner data to be presented as a single column.
        /// </summary>
        protected override Array GetInnerData()
        {
            return data.Data;
        }

        protected override void InnerInitialize(Array data, int[] shape)
        {
            this.data.PutData(null, data);
            ChangesUpdateShape(this.changes, ReadShape());
        } 
    }
}

