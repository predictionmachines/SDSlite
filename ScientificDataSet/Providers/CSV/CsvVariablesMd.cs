// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Research.Science.Data.CSV
{
    /// <summary>Class that represents multidimensional (>=2) variable in CSV files</summary>
    /// <typeparam name="DataType">Type of variable. For list of supported types see DataSet specification.</typeparam>
    /// <remarks>Instances of this call cannot be constructed directly. See 
    /// <see cref="M:Microsoft.Research.Science.Data.DataSet.AddVariable"/> for details about creation
    /// of new variables</remarks>
    internal sealed class CsvVariableMd<DataType> : CsvVariable<DataType>, ICsvVariableMd
    {
        internal CsvVariableMd(CsvDataSet dataSet, CsvColumn column)
            : base(dataSet, column)
        {
            if (column.Rank <= 2)
                throw new Exception("This is multidimensional variable and is being created with different rank.");

            data = new ArrayWrapper(column.Rank, typeof(DataType));

            Initialize();
        }

        private CsvVariableMd(CsvDataSet dataSet, int id, MetadataDictionary metadata, ArrayWrapper data, string[] dims)
            : base(dataSet, id, metadata, dims)
        {
            this.data = data;
            Initialize();
        }

        public override Variable CloneAndRenameDims(string[] newDims)
        {
            if (newDims == null || Rank != newDims.Length)
                throw new Exception("New dimensions are wrong");
            Variable var = new CsvVariableMd<DataType>((CsvDataSet)DataSet, ID, Metadata, data, newDims);
            return var;
        }

        protected override Array GetInnerData()
        {
            Array array = data.Data;
            if (array == null)
                return new DataType[0] { };

            int dimCount = Rank;
            int[] indices = new int[dimCount]; // Zero values by default
            int n = array.Length;

            DataType[] innerData = new DataType[n];

            Type type = typeof(DataType);

            if (type == typeof(string) || type == typeof(DateTime) || type == typeof(bool))
            {
                for (int i = 0; i < n; i++)
                {
                    innerData[i] = (DataType)array.GetValue(indices);
                    int j = dimCount - 1;
                    while (j >= 0)
                    {
                        indices[j]++;
                        if (indices[j] >= array.GetLength(j))
                            indices[j--] = 0;
                        else
                            break;
                    }
                }
            }
            else
            {
                Debug.Assert(Buffer.ByteLength(innerData) == Buffer.ByteLength(array));
                Buffer.BlockCopy(array, 0, innerData, 0, Buffer.ByteLength(array));
            }

            return innerData;
        }

        protected override void InnerInitialize(Array data, int[] shape)
        {
            if (data == null) return;

            int dimCount = Rank;
            int[] indices = new int[dimCount]; // Zero values by default
            int n = data.Length;
            DataType[] typedData = (DataType[])data;
            Array array = Array.CreateInstance(TypeOfData, shape);
            Type type = typeof(DataType);

            if (type == typeof(string) || type == typeof(DateTime) || type == typeof(bool))
            {
                for (int i = 0; i < n; i++)
                {
                    array.SetValue(typedData[i], indices);
                    int j = dimCount - 1;
                    while (j >= 0)
                    {
                        indices[j]++;
                        if (indices[j] >= shape[j])
                            indices[j--] = 0;
                        else
                            break;
                    }
                }
            }
            else
            {
                Debug.Assert(Buffer.ByteLength(typedData) == Buffer.ByteLength(array));
                Buffer.BlockCopy(typedData, 0, array, 0, Buffer.ByteLength(typedData));
            }

            base.data.PutData(null, array);
            ChangesUpdateShape(this.changes, ReadShape());
        }

        Array ICsvVariableMd.GetColumnData(int col)
        {
            Array array = data.Data;
            if (array == null)
                return new DataType[0] { };

            Type type = array.GetType().GetElementType();

            int[] arrayShape = GetArrayShape(array);

            int rank = Rank;
            int n = array.Length / array.GetLength(rank - 1);

            DataType[] colData = new DataType[n];

            int[] indices = new int[rank];
            indices[rank - 1] = col; // column ~ last index

            if (type != typeof(DateTime) && type != typeof(bool) && type != typeof(string))
            {
                int sizeOfType = GetSizeOfType(type);
                for (int i = 0; i < n; i++)
                {
                    //Even copying by element is faster than GetValue
                    Buffer.BlockCopy(array, GetScalarIndex(indices, arrayShape) * sizeOfType, colData, i * sizeOfType, sizeOfType);
                    int j = rank - 2;
                    while (j >= 0)
                    {
                        indices[j]++;
                        if (indices[j] >= arrayShape[j])
                            indices[j--] = 0;
                        else
                            break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < n; i++)
                {
                    colData[i] = (DataType)array.GetValue(indices);
                    int j = rank - 2;
                    while (j >= 0)
                    {
                        indices[j]++;
                        if (indices[j] >= arrayShape[j])
                            indices[j--] = 0;
                        else
                            break;
                    }
                }
            }
            return colData;
        }

        private static int GetScalarIndex(int[] index, int[] shape)
        {
            int result = 0;
            int mul = 1;
            for (int ind = index.Length - 1; ind >= 0; ind--)
            {
                result += index[ind] * mul;
                mul *= shape[ind];
            }
            return result;
        }

        private static int[] GetArrayShape(Array array)
        {
            int[] shape;
            if (array != null)
            {
                shape = new int[array.Rank];
                for (int i = 0; i < array.Rank; i++)
                {
                    shape[i] = array.GetLength(i);
                }
                return shape;
            }

            shape = new int[array.Rank];
            return shape;
        }

        private static int GetSizeOfType(Type type)
        {
            if (type == typeof(Double))
                return sizeof(Double);
            else if (type == typeof(Single))
                return sizeof(Single);
            else if (type == typeof(Int16))
                return sizeof(Int16);
            else if (type == typeof(Int32))
                return sizeof(Int32);
            else if (type == typeof(Int64))
                return sizeof(Int64);
            else if (type == typeof(UInt64))
                return sizeof(UInt64);
            else if (type == typeof(UInt32))
                return sizeof(UInt32);
            else if (type == typeof(UInt16))
                return sizeof(UInt16);
            else if (type == typeof(Byte))
                return sizeof(Byte);
            else if (type == typeof(SByte))
                return sizeof(SByte);
            else throw new NotSupportedException();
        }

        void ICsvVariableMd.Initialize(Array data)
        {
            base.data.PutData(null, data);
            ChangesUpdateShape(this.changes, ReadShape());
        }

        void ICsvVariableMd.FastCopyColumn(Array entireArray, Array column, int index)
        {
            DataType[] colData = (DataType[])column;

            int rank = Rank;
            int n = column.Length;

            int[] indices = new int[rank];
            indices[rank - 1] = index; // column ~ last index


            int[] entireArrayShape = GetArrayShape(entireArray);
            Type type = typeof(DataType);

            if (type != typeof(DateTime) && type != typeof(bool) && type != typeof(string))
            {
                int sizeOfType = GetSizeOfType(type);
                for (int i = 0; i < n; i++)
                {
                    //Even copying by element is faster than GetValue
                    Buffer.BlockCopy(colData, i * sizeOfType, entireArray, GetScalarIndex(indices, entireArrayShape) * sizeOfType, sizeOfType);
                    int j = rank - 2;
                    while (j >= 0)
                    {
                        indices[j]++;
                        if (indices[j] >= entireArrayShape[j])
                            indices[j--] = 0;
                        else
                            break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < n; i++)
                {
                    entireArray.SetValue(colData[i], indices);
                    int j = rank - 2;
                    while (j >= 0)
                    {
                        indices[j]++;
                        if (indices[j] >= entireArrayShape[j])
                            indices[j--] = 0;
                        else
                            break;
                    }
                }
            }
        }
    }
}

