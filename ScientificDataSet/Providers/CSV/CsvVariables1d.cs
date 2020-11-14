// Copyright © Microsoft Corporation, All Rights Reserved.
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
    internal sealed class CsvVariable1d<DataType> : CsvVariable<DataType>
    {
        internal CsvVariable1d(CsvDataSet dataSet, CsvColumn column)
            : base(dataSet, column)
        {
            if (column.Rank != 1)
                throw new Exception("This is 1d-variable and is being created with different rank.");

            data = new ArrayWrapper(1, typeof(DataType));//new OrderedArray1d(typeof(DataType), false);

            Initialize();
        }

        private CsvVariable1d(CsvDataSet dataSet, int id, MetadataDictionary metadata, ArrayWrapper data, string[] dims)
            : base(dataSet, id, metadata, dims)
        {
            this.data = data;
            Initialize();
        }

        public override Variable CloneAndRenameDims(string[] newDims)
        {
            if (newDims == null || Rank != newDims.Length)
                throw new Exception("New dimensions are wrong");
            Variable var = new CsvVariable1d<DataType>((CsvDataSet)DataSet, ID, Metadata, data, newDims);
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

