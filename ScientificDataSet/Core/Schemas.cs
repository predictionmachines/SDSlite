// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Research.Science.Data
{
	/// <summary>Provides structure information for a variable.</summary>
	/// <remarks>This class is intended to be immutable, but metadata field
	/// and corresponding property allow modify operations.</remarks>
	public class VariableSchema
	{
		private int id;
		private int changeSetId;
		private Type dataType;
		private ReadOnlyDimensionList dimensions;
		private MetadataDictionary metadata;

		internal VariableSchema(int changeSetId, int id, Type dataType, ReadOnlyDimensionList dimensions, MetadataDictionary metadata)
        {
			if (dimensions == null)
				throw new ArgumentNullException("dimensions");
			if (metadata == null)
				throw new ArgumentNullException("metadata");
			this.changeSetId = changeSetId;
			this.id = id;
			this.dimensions = dimensions;
			this.metadata = metadata;
			this.dataType = dataType;
		}

		/// <summary>
		/// Gets the ID of the variable.
		/// </summary>
		public int ID
		{
			get { return id; }
		}
		/// <summary>
		/// Gets the changeset of the variable.
		/// </summary>
		public int ChangeSet
		{
			get { return changeSetId; }
		}
		/// <summary>
		/// Gets the name of the variable.
		/// </summary>
		public string Name
		{
			get { return metadata.GetComittedOtherwiseProposedValue<string>(metadata.KeyForName, null); }
		}
		/// <summary>
		/// Gets the data type of the variable.
		/// </summary>
		public Type TypeOfData
		{
			get { return dataType; }
		}
		/// <summary>
		/// Gets the read onlt dimension lists the variable depends on.
		/// </summary>
		public ReadOnlyDimensionList Dimensions
		{
			get { return dimensions; }
		}
		/// <summary>
		/// Gets the metadata collection.
		/// </summary>
		public MetadataDictionary Metadata
		{
			get { return metadata; }
		}
		/// <summary>
		/// Gets the rank of the variable.
		/// </summary>
		public int Rank
		{
			get { return dimensions.Count; }
		}
		/// <summary>
		/// Represents the schema as string in short.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("<[{0}]{1} of type {2}", ID,
				ID == DataSet.GlobalMetadataVariableID ? "" : Name, TypeOfData.Name);
			for (int i = 0; i < dimensions.Count; i++)
			{
				sb.Append(' ');
				sb.Append(dimensions[i]);
			}

			sb.Append('>');
			return sb.ToString();
		}
	}

	/// <summary>Provides structure information for a <see cref="DataSet"/>.</summary>
	/// <remarks>This class is immutable.</remarks>
	public class DataSetSchema
	{
		private readonly Guid guid;
		private readonly int version;
		private readonly VariableSchema[] vars;
		private readonly string uri;

		internal DataSetSchema(Guid guid, string uri, int version, VariableSchema[] vars)
        {
			this.guid = guid;
			this.vars = vars;
			this.version = version;
			this.uri = uri;
		}

		/// <summary>
		/// Gets the unique identifier of the DataSet.
		/// </summary>
		/// <seealso cref="DataSet.DataSetGuid"/>
		public Guid DataSetGuid
		{
			get { return guid; }
		}

		/// <summary>
		/// Gets the changeset number of the DataSet.
		/// </summary>
		/// <seealso cref="DataSet.Version"/>
		public int Version
		{
			get { return version; }
		}

		/// <summary>
		/// Gets the DataSet URI.
		/// </summary>
		/// <seealso cref="DataSet.URI"/>
		public string URI
		{
			get { return uri; }
		}

		/// <summary>
		/// Gets the DataSet URI.
		/// </summary>
		/// <remarks>
		/// The property is obsolete. Use <see cref="URI"/> instead.</remarks>
		[Obsolete("This property is obsolete and will be removed in the next version. Use the property \"URI\" instead.")]
		public string ConstructionString
		{
			get { return uri; }
		}

		/// <summary>
		/// Gets an array of the variables contained in the DataSet.
		/// </summary>
		public VariableSchema[] Variables
		{
			get { return vars; }
		}

		/// <summary>
		/// Gets an array of dimensions of the DataSet.
		/// </summary>
		/// <returns>An array of <see cref="Dimension"/>.</returns>
		/// <remarks>
		/// If the schema corresponds to the proposed version of the DataSet and
		/// some dimension differs for different variables, in the returning array the dimension
		/// has length equal to <c>-1</c>.
		/// </remarks>
		public Dimension[] GetDimensions()
		{
			if (vars == null || vars.Length == 0) return new Dimension[0];

			Dictionary<string, Dimension> dims = new Dictionary<string, Dimension>();
			foreach (var v in vars)
				foreach (var vd in v.Dimensions)
				{
					Dimension dim;
					if (dims.TryGetValue(vd.Name, out dim))
						dim.Length = -1;
					else
						dim = vd;
					dims[vd.Name] = vd;
				}
			Dimension[] dimsArr = new Dimension[dims.Count];
			dims.Values.CopyTo(dimsArr, 0);
			return dimsArr;
		}
	}

	/// <summary>
	/// Specifies the schema version.
	/// </summary>
	public enum SchemaVersion
	{
		/// <summary>
		/// Represents committed unmodified elements.
		/// </summary>
		Committed,
		/// <summary>
		/// Represents modified or added elements.
		/// </summary>
		Proposed,
		/// <summary>If the Proposed version is available, it is used; otherwise, the Committed version is used.</summary>
		Recent,
	}
}

