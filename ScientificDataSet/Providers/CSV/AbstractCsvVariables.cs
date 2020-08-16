// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data.CSV
{
    internal interface ICsvVariable
    {
        /// <summary>
        /// Initializes a variable by the given array.
        /// For columned arrays the data is 1d and will be converted to 2d.
        /// </summary>
        void Initialize(Array data, int[] shape);

        /// <summary>
        /// Returns inner data as a 1d-array (note: for columned 2d-vars it is 1d too!)
        /// </summary>
        Array ReadOnlyInnerData { get; }

        Variable CloneAndRenameDims(string[] newDims);

        void ReinitializeFrom(Variable v);

        ReadOnlyDimensionList GetRecentDimensions();

        int ColumnIndex { get; }
    }

    internal interface ICsvVariableMd : ICsvVariable
    {
        /// <summary>
        /// Gets the data for the given column as a 1d array.
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        Array GetColumnData(int col);

        /// <summary>
        /// Initializes from flat 2d array
        /// </summary>
        /// <param name="data"></param>
        void Initialize(Array data);

        /// <summary>
        /// Copies column data to the array (variable is typed so it is much faster to use this method for intialization)
        /// </summary>
        void FastCopyColumn(Array entireArray, Array column, int index);
    }

    /// <summary>This abstract class is a base for all classes that work with variables in CSV files</summary>
    /// <typeparam name="DataType">Type of variable. List of supported types can be found in 
    /// DataSet specification</typeparam>
    internal abstract class CsvVariable<DataType> : DataAccessVariable<DataType>, ICsvVariable
    {
        /// <summary>Array holding in-memory copy of data for this variable. 
        /// It is <see cref="T:Microsoft.Research.Science.Data.ArrayWrapper"/> so it holds information
        /// about rank and size even if there is no data values.</summary>
        protected ArrayWrapper data;

        internal CsvVariable(CsvDataSet dataSet, CsvColumn column)
            : base(dataSet, column.VariableName, column.Dims)
        {
            DisplayName = String.IsNullOrEmpty(column.Header) ? column.VariableName : column.Header;
            if (column.MissingValue != null || !column.Type.IsValueType)
                MissingValue = column.MissingValue;
            if (column.ID > 0)
                this.ID = column.ID;

            Metadata[CsvDataSet.CsvColumnKeyName] = column.Index;
        }

        /// <summary>Initialized a new instance of CSV variable</summary>
        /// <param name="dataSet">Owner CSV DataSet</param>
        /// <param name="id">ID of variable</param>
        /// <param name="metadata">Metadata dictionary to be copied into this variable</param>
        /// <param name="dims">Array of dimension names</param>
        protected CsvVariable(CsvDataSet dataSet, int id, MetadataDictionary metadata, string[] dims)
            : base(dataSet, GetName(metadata), dims)
        {
            metadata.ForEach(e => Metadata[e.Key] = e.Value, SchemaVersion.Recent);
            this.ID = id;
        }

        /// <summary>
        /// Gets the starting column index within the file for this variable.
        /// </summary>
        public int ColumnIndex
        {
            get
            {
                if (!Metadata.ContainsKey(CsvDataSet.CsvColumnKeyName, SchemaVersion.Recent))
                    return int.MaxValue;
                else
                {
                    object csvColumnValue = Metadata[CsvDataSet.CsvColumnKeyName, SchemaVersion.Recent];
                    if (csvColumnValue == null ||
                       !(csvColumnValue is int))
                        return int.MaxValue;
                    return (int)csvColumnValue;
                }
            }
        }

        private static string GetName(MetadataDictionary vm)
        {
            if (vm == null) throw new ArgumentNullException("metadata");
            if (vm.ContainsKey(vm.KeyForName))
                return (string)vm[vm.KeyForName, SchemaVersion.Committed];
            else if (vm.ContainsKey(vm.KeyForName, SchemaVersion.Proposed))
                return (string)vm[vm.KeyForName, SchemaVersion.Proposed];
            return "";
        }

        /// <summary>Gets or sets the display name.</summary>
        public string DisplayName
        {
            get { return (string)Metadata["DisplayName"]; }
            set { Metadata["DisplayName"] = value; }
        }

        void ICsvVariable.ReinitializeFrom(Variable src)
        {
            if (!typeof(CsvVariable<DataType>).IsAssignableFrom(src.GetType()))
                throw new Exception("Source variable cannot be used to reinitialize from: it has wrong type");
            if (src.Rank != Rank)
                throw new Exception("Source variable cannot be used to reinitialize from: its rank differs");

            CsvVariable<DataType> csvVar = (CsvVariable<DataType>)src;
            foreach (var item in src.Metadata)
            {
                Metadata[item.Key] = item.Value;
            }

            this.data = csvVar.data;

            ClearChanges();
        }

        /// <summary>
        /// Initializes data internally, only when data is loaded from a csv-file.
        /// </summary>
        void ICsvVariable.Initialize(Array data, int[] shape)
        {
            InnerInitialize(data, shape);
        }

        Array ICsvVariable.ReadOnlyInnerData
        {
            get { return GetInnerData(); }
        }

        /// <summary>Creates a clone of this variable with renamed dimensions</summary>
        /// <param name="newDims">Array of new dimension names. Length of this array must
        /// match rank of variable</param>
        /// <returns>Clone of variable</returns>
        public abstract Variable CloneAndRenameDims(string[] newDims);

        /// <summary>
        /// Builds and returns inner data to be presented as a single column.
        /// </summary>
        protected abstract Array GetInnerData();

        /// <summary>
        /// Initializes data internally, only when data is loaded from a csv-file.
        /// </summary>
        protected abstract void InnerInitialize(Array data, int[] shape);

        /// <summary>Reads array of values from variable</summary>
        /// <param name="origin">Origin of region to read. Null origin means all zeros</param>
        /// <param name="shape">Shape of region to read. Null shape means entire array of data.</param>
        /// <returns>Array of values loaded from variable</returns>
        protected override Array ReadData(int[] origin, int[] shape)
        {
            return data.GetData(origin, shape);
        }

        /// <summary>
        /// Writes the data to the variable's underlying storage starting with the specified origin indices.
        /// Each operation is a part of a transaction, opened with the BeginWriteTransaction() method.
        /// *It is called in the precommit stage*
        /// </summary>
        /// <remarks>
        /// <para>A sequence of such outputs with same transaction can be either committed by CommitWrite() or
        /// rolled back by RollbackWrite().</para>
        /// <para>Parameter <paramref name="origin"/> for data piece produced by Append operation
        /// in given to the method already transformed and contains actual values.</para>
        /// </remarks>
        /// <param name="origin">Indices to start adding of data. Null means all zeros.</param>
        /// <param name="a">Data to add to the variable.</param>
        protected override void WriteData(int[] origin, Array a)
        {
            data.PutData(origin, a);
        }

        /// <summary>Gets shape of variable</summary>
        /// <returns>Array of integers with variable shape</returns>
        /// <remarks>Returned array is a copy of shape so it modification won't
        /// affect variable</remarks>
        protected override int[] ReadShape()
        {
            return data.GetShape();
        }

        ReadOnlyDimensionList ICsvVariable.GetRecentDimensions()
        {
            return GetDimensions(SchemaVersion.Recent);
        }
    }

    /// <summary>
    /// Describes a single column in a CSV file.
    /// </summary>
    internal class CsvColumn
    {
        public string Header { get; set; }

        private int index;

        public CsvColumn(int index, int id, string header)
        {
            this.Header = header;
            this.index = index;
            Rank = 1;
            Dims = new string[] { string.Format("csv_{0}", index) };
            TypeUpdated = false;
            ContainsData = false;
            Shape = null;
            DescribedInMetadata = false;
            InferredType = InferredType.Unknown;
            FlatVarBaseIndex = -1;
            ID = id;

            columnName = CsvDataSet.GetNameByIndex(index);
            if (!String.IsNullOrEmpty(header))
                VariableName = header;
            else
                VariableName = columnName;

            EndsWithEmptyLines = false;
        }

        public int FlatVarBaseIndex { get; set; }

        public int[] Shape { get; set; }

        public bool ContainsData { get; set; }

        public Type Type { get; set; }

        public int Rank { get; set; }

        public string[] Dims { get; set; }

        private string columnName;

        public string ColumnName { get { return columnName; } }

        public int Index { get { return index; } }

        public int ID { get; set; }

        public bool TypeUpdated { get; set; }

        public object MissingValue { get; set; }

        public string VariableName { get; set; }

        public bool DescribedInMetadata { get; set; }

        public bool EndsWithEmptyLines { get; set; }

        public InferredType InferredType { get; set; }

        public override string ToString()
        {
            string inferred = (InferredType == InferredType.ItIsKnown || InferredType == InferredType.Unknown) ?
                InferredType.ToString() : InferredType.ToString() + "?";
            return string.Format("{0} {1} of {2} [{3}]", ID, ColumnName, Type, inferred);
        }
    }

    internal enum InferredType { Unknown, DateTime, Integer, Double, Boolean, String, ItIsKnown }
}

