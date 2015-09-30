// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections.Generic;

namespace Microsoft.Research.Science.Data.CSV
{
    /// <summary>Class that represents 2D variable in CSV files</summary>
    /// <typeparam name="DataType">Type of variable. For list of supported types see DataSet specification.</typeparam>
    /// <remarks>Instances of this call cannot be constructed directly. See 
    /// <see cref="M:Microsoft.Research.Science.Data.DataSet.AddVariable"/> for details about creation
    /// of new variables</remarks>
    internal sealed class CsvVariable2d<DataType> : CsvVariable<DataType>, ICsvVariableMd
    {
        internal CsvVariable2d(CsvDataSet dataSet, CsvColumn column)
            : base(dataSet, column)
        {
            if (column.Rank != 2)
                throw new Exception("This is 2d-variable and is being created with different rank.");

            data = new ArrayWrapper(2, typeof(DataType));

            Initialize();
        }

        private CsvVariable2d(CsvDataSet dataSet, int id, MetadataDictionary metadata, ArrayWrapper data, string[] dims)
            : base(dataSet, id, metadata, dims)
        {
            this.data = data;
            Initialize();
        }

        public override Variable CloneAndRenameDims(string[] newDims)
        {
            if (newDims == null || Rank != newDims.Length)
                throw new Exception("New dimensions are wrong");
            Variable var = new CsvVariable2d<DataType>((CsvDataSet)DataSet, ID, Metadata, data, newDims);
            return var;
        }

        protected override Array GetInnerData()
        {
            DataType[,] array = (DataType[,])data.Data;
            if (array == null)
                return new DataType[0] { };

            int W = array.GetLength(0);
            int H = array.GetLength(1);

            int i = 0;
            DataType[] innerData = new DataType[W * H];
            for (int col = 0; col < W; col++)
                for (int row = 0; row < H; row++)
                {
                    innerData[i++] = array[col, row];
                }

            return innerData;
        }

        protected override void InnerInitialize(Array data, int[] shape)
        {
            int i = 0;
            DataType[] typedData = (DataType[])data;
            DataType[,] array = new DataType[shape[0], shape[1]];
            for (int col = 0; col < shape[0]; col++)
                for (int row = 0; row < shape[1]; row++)
                {
                    array[col, row] = typedData[i++];
                }
            base.data.PutData(null, array);
            ChangesUpdateShape(this.changes, ReadShape());
        }

        Array ICsvVariableMd.GetColumnData(int col)
        {
            DataType[,] array = (DataType[,])data.Data;
            if (array == null)
                return new DataType[0] { };

            int W = array.GetLength(0);

            DataType[] colData = new DataType[W];
            for (int i = 0; i < W; i++)
            {
                colData[i] = array[i, col];
            }
            return colData;
        }

        void ICsvVariableMd.Initialize(Array data)
        {
            base.data.PutData(null, data);
            ChangesUpdateShape(this.changes, ReadShape());
        }

        void ICsvVariableMd.FastCopyColumn(Array entireArray, Array column, int index)
        {
            DataType[] typedData = (DataType[])column;
            DataType[,] array = (DataType[,])entireArray;
            int n = column.Length;
            for (int i = 0; i < n; i++)
            {
                array[i, index] = typedData[i];
            }
        }
    }
}

