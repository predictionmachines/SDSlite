// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data
{
	/// <summary>
	/// CoordinateSystem maps an index from a space of dimensions into coordinate values
    /// constructed from axis variables.
	/// </summary>
    /// <remarks>
    /// The <see cref="CoordinateSystem"/> class cannot be modified after it is committed.
    /// Therefore, all axes should be added to the coordinate system before the <see cref="DataSet"/>
    /// is committed.
    /// <example>
    /// The followin example creates a coordinate system with a time axis
    /// and adds a variable defined in the coordinate system.
    /// <code>
    /// DataSet ds = . . . ;
	/// ds.IsAutocommitEnabled = false;
	/// 
	/// // Creates a variable "time", adds it into the DataSet and then uses it as an axis for the cs:
	/// Variable&lt;DateTime&gt; time = ds.AddVariable&lt;DateTime&gt;("time", "t");    
    /// CoordinateSystem cs = ds.CreateCoordinateSystem("cs", time);
    /// 
    /// // Temperature variable also depends on "t" for it is defined for each time moment.
    /// Variable&lt;double&gt; temperature = ds.AddVariable&lt;double&gt;("temperature", "t");
    /// 
    /// // Each value of temperature variable has corresponded time moment from the "time".
    /// temperature.AddCoordinateSystem(cs);
    /// 
    /// ds.Commit();
    /// </code>
    /// </example>
    /// </remarks>
    [Obsolete("Coordinate systems will be removed from the future release. Use metadata attributes instead.")]
	public sealed class CoordinateSystem
	{
		#region Private members

		private string name;

		/// <summary>
		/// The field stores axes during initialization of the cs.
		/// After commit, it is null.
		/// </summary>
		private VariableCollection axes;

		// These fields are to optimize the access:
		private ReadOnlyVariableCollection roAxes = null;
		private Variable[] arrayAxes = null;
		private CoordinateSystemSchema committedSchema = null;

		private DataSet sds = null;

		private bool committed = false;

		#endregion

		#region Constructor

		internal CoordinateSystem(string name, DataSet dataSet)
		{
			if (String.IsNullOrEmpty(name))
				throw new Exception("Name cannot be empty");

			if (dataSet == null)
				throw new Exception("The coordinate system must be attached to a DataSet.");

			this.name = name;
			this.sds = dataSet;
			this.axes = new VariableCollection();
		}

		#endregion

		#region Public properties

		/// <summary>
		/// Gets the name of the coordinate system.
		/// </summary>
		public string Name { get { return name; } }

		/// <summary>
		/// Gets the collection of the coordinate system's axes.
		/// </summary>
		public ReadOnlyVariableCollection Axes
		{
			get
			{
				if (committed)
					return roAxes;
				return axes.GetReadOnlyCollection();
			}
		}

		/// <summary>
		/// Gets the collection of axes as an array, i.e. indexed not by variable's ID but index.
		/// </summary>
		public Variable[] AxesArray
		{
			get
			{
				if (committed)
				{
					if (arrayAxes == null)
						arrayAxes = roAxes.ToArray();
					return arrayAxes;
				}
				return axes.ToArray();
			}
		}

		/// <summary>
		/// Gets the number of axes in the coordinate system.
		/// </summary>
		public int AxesCount
		{
			get
			{
				if (committed)
					return roAxes.Count;
				return axes.Count;
			}
		}

		/// <summary>
		/// Gets the value indicating whether the coordinates system has already been committed or not.
		/// </summary>
		public bool HasChanges
		{
			get { return !committed; }
		}

		/// <summary>
		/// Gets the Scientific Data Set instance the coordinate system is attached to.
		/// </summary>
		public DataSet DataSet
		{
			get { return this.sds; }
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Commits all changes of the coordinate system. 
		/// </summary>
		/// <remarks>
		/// After committing no changes in the coordinate system are allowed.
		/// </remarks>
		public void Commit()
		{
			if (committed) return;
			committedSchema = GetSchema();

			roAxes = new ReadOnlyVariableCollection(axes);
			axes = null;
			arrayAxes = null;

			committed = true;
		}

		/// <summary>
		/// Gets the schema for the coordinate system describing its structure.
		/// </summary>
		public CoordinateSystemSchema GetSchema()
		{
			if (committed)
			{
				System.Diagnostics.Debug.Assert(committedSchema != null);
				return committedSchema;
			}

			int[] axesIds = new int[axes.Count];
			int i = 0;
			foreach (var axis in axes)
			{
				axesIds[i++] = axis.ID;
			}
			CoordinateSystemSchema schema = new CoordinateSystemSchema(
				name, axesIds);

			return schema;
		}

		#endregion

		#region Axes

		/// <summary>
		/// Adds the variable to the coordinate system as an axis.
		/// </summary>
		/// <remarks>
		/// The variable <paramref name="var"/> must belong to the same data set as the coordinate system does.
		/// </remarks>
		/// <param name="var">The variable that is to be considered as an axis.</param>
		/// <typeparam name="DataType">Data type of the axis.</typeparam>
		/// <returns>The variable just added as an axis.</returns>
		internal Variable<DataType> AddAxis<DataType>(Variable<DataType> var)
		{
			return (Variable<DataType>)AddAxis((Variable)var);
		}

		/// <summary>
		/// Adds the variable to the coordinate system as an axis.
		/// </summary>
		/// <remarks>
		/// The variable <paramref name="var"/> must belong to the same data set as the coordinate system does.
		/// </remarks>
		/// <param name="var">The variable that is to be considered as an axis.</param>
		/// <returns>The variable just added as an axis.</returns>
		internal Variable AddAxis(Variable var)
		{
			if (committed)
				throw new NotSupportedException("Coordinate system cannot be changed after committing.");

			if (var.DataSet != DataSet)
				throw new ArgumentException("Axis must be from to the same data set as the coordinate system is.");

			if(var.Rank == 0)
				throw new ArgumentException("Axis cannot be scalar variable");

			if (axes.Contains(var))
				throw new Exception("The variable has already been added to the coordinate system as an axis");

			axes.Add(var);

			return var;
		}

		/// <summary>
		/// Creates and adds the variable as an axis to the coordinate system and related data set.
		/// </summary>
		/// <param name="name">Name of the new variable.</param>
		/// <param name="dims">Names of dimensions the variable depends on.</param>
		/// <typeparam name="DataType">Data type of the axis.</typeparam>
		/// <returns>The variable just created and added as an axis.</returns>
		internal Variable<DataType> AddAxis<DataType>(string name, params string[] dims)
		{
			if (committed)
				throw new NotSupportedException("Coordinate system cannot be changed after committing.");

			if (axes.Contains(name))
				throw new Exception("The coordinate system already contains an axis with the same name.");

			Variable<DataType> var = sds.AddVariable<DataType>(name, dims);
			return AddAxis(var);
		}

		/// <summary>
		/// Returns committed version of the dimensions list for the coordinate system.
		/// </summary>
		/// <returns></returns>
		public ReadOnlyDimensionList GetDimensions()
		{
			return GetDimensions(SchemaVersion.Committed);
		}

		/// <summary>
		/// Returns specified version of the dimensions list for the coordinate system.
		/// </summary>
		/// <returns></returns>
		public ReadOnlyDimensionList GetDimensions(SchemaVersion version)
		{
			DimensionList dlist = new DimensionList();
			IEnumerable<Variable> myAxes = committed ?
				(IEnumerable<Variable>)roAxes : (IEnumerable<Variable>)axes;
			foreach (Variable a in myAxes)
			{
				dlist.AddRange(a.GetDimensions(version));
			}
			return new ReadOnlyDimensionList(dlist);
		}

		/// <summary>
		/// Returns the dimensions list for the coordinate system base on given changeset.
		/// </summary>
		/// <returns></returns>
		public ReadOnlyDimensionList GetDimensions(DataSet.Changes changeset)
		{
			DimensionList dlist = new DimensionList();
			IEnumerable<Variable> myAxes = committed ?
				(IEnumerable<Variable>)roAxes : (IEnumerable<Variable>)axes;
			foreach (Variable a in myAxes)
			{
				var vc = changeset.GetVariableChanges(a.ID);
				ReadOnlyDimensionList list = vc == null ? a.GetDimensions(SchemaVersion.Committed) :
					vc.GetDimensionList();

				dlist.AddRange(list);
			}
			return new ReadOnlyDimensionList(dlist);
		}

		#endregion

		#region From Object

        /// <summary>Converts this object to string</summary>
        /// <returns>String with brief information about coordinate system</returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("(Coordinate system {0}", name);

			if (AxesCount == 0) sb.Append(" with no axes");
			else
			{
				sb.Append(" with axes <");
				bool first = true;
				foreach (Variable axis in Axes)
				{
					if (!first)
					{
						sb.Append(',');
					}
					sb.AppendFormat("[{0}] {1}", axis.ID, axis.Name);
					first = false;
				}
				sb.Append(">");
			}
			if (!committed)
				sb.Append('*');
			sb.Append(')');

			return sb.ToString();
		}

		#endregion

		#region Constraints

		/// <summary>
		/// Checks the constraints and throws an exception if the check failed.
		/// </summary>
		/// <exception cref="Exception">Constraints are not satisfied.</exception>
		/// <remarks>
		/// <para>The method checks the constraints and throws an exception if the check failed.</para>
		/// <para>
		/// The constraint of a coordinate system means reversibility of its every axes' data. 
		/// Common variable provides an access to its values by an integer index or a vector of indices, 
		/// but an axis should allow finding an index (or a vector of indices) by a coordinate value 
		/// from that axis. This is a reversed index selection operation which enables retrieving a value 
		/// from a variable by its coordinate values in a given coordinate system 
		/// (see <see cref="Variable{DataType}.GetValue(object[])"/>). 
		/// The constraint in the one-dimensional case is just a claim for data to be strictly monotonic 
		/// for numeric and date/time data types, or contain no duplicate values for others 
		/// (for instance, strings).
		/// </para>
		/// </remarks>
		/// <exception cref="ConstraintsFailedException"/>
		internal void CheckConstraints(DataSet.Changes changeset)
		{
			ICollection<Variable> myAxes = committed ?
				(ICollection<Variable>)roAxes : (ICollection<Variable>)axes;
			if (myAxes == null || myAxes.Count == 0)
				throw new ConstraintsFailedException("Coordinate system must contain at least one axis");

			/*foreach (Variable var in myAxes)
			{
				if (!var.CheckReversibility(changeset.GetVariableChanges(var.ID)))
					throw new ConstraintsFailedException("Variable " + var + " is an axis but is not reversible");
			}*/
		}

		#endregion
	}
}

