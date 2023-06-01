// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Microsoft.Research.Science.Data
{
    /// <summary>    
    /// Represents a single array or a scalar value with attached metadata.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Variable"/> represents a single array or a scalar value with attached metadata. 
    /// <see cref="Variable"/> cannot exist separately but always belongs to some <see cref="Microsoft.Research.Science.Data.DataSet"/> instance. 
    /// Typically, a variable does not completely load all underlying data into memory, 
    /// and performs on-demand data access. But in common it depends on a particular 
    /// implementation of a variable which varies for different <see cref="Microsoft.Research.Science.Data.DataSet"/> providers. 
    /// </para>
    /// <para>
    /// Each variable has unique <see cref="ID"/>, <see cref="Name"/> (optional and not unique), <see cref="TypeOfData"/> (i.e. type of an element of variable’s array), 
    /// <see cref="Rank"/> (number of dimensions of the array), and shape (<see cref="GetShape()"/>; lengths of all array’s dimensions). 
    /// Type of data and rank are specified when variable is created and cannot be modified afterwards. 
    /// If a variable is not read-only (see <see cref="IsReadOnly"/>), shape is changing as a user puts data into a variable 
    /// (i.e. into an underlying array). It is important that the Scientific DataSet model doesn’t limit 
    /// any dimension, i.e. all dimensions are unbounded. In other words, it is possible to extend 
    /// a variable by each dimension. But a particular <see cref="Microsoft.Research.Science.Data.DataSet"/> providers still may force extra constraints. 
    /// Methods <see cref="Variable.GetShape(SchemaVersion)"/> should be used to get a recent shape of a variable.
    /// </para>
    /// <para>
    /// The <see cref="Variable"/> supports types given in the table below.
    /// An attempt to create a variable for a type not listed in the table, 
    /// leads to an exception <see cref="InvalidOperationException"/>.
    /// The static method <see cref="Microsoft.Research.Science.Data.DataSet.IsSupported(Type)"/> makes dynamic check whether 
    /// a type is supported or not.
    /// <list type="bullet">
    /// <item><description><see cref="T:System.Double"/></description></item>
    /// <item><description><see cref="T:System.Single"/></description></item>
    /// <item><description><see cref="T:System.Int16"/></description></item>
    /// <item><description><see cref="T:System.Int32"/></description></item>
    /// <item><description><see cref="T:System.Int64"/></description></item>
    /// <item><description><see cref="T:System.UInt64"/></description></item>
    /// <item><description><see cref="T:System.UInt32"/></description></item>
    /// <item><description><see cref="T:System.UInt16"/></description></item>
    /// <item><description><see cref="T:System.Byte"/></description></item>
    /// <item><description><see cref="T:System.SByte"/></description></item>
    /// <item><description><see cref="T:System.DateTime"/></description></item>
    /// <item><description><see cref="T:System.String"/></description></item>
    /// <item><description><see cref="T:System.Boolean"/></description></item>
    /// </list>
    /// Note that <see cref="T:System.Char"/> is not supported!
    /// </para>
    /// <para>
    /// Not read-only variable may be renamed at any time through the <see cref="Variable.Name" /> property. 
    /// The property updates the proposed name of the variable, which becomes actual only after committing. 
    /// To get the proposed name, use <see cref="Metadata"/> accessor for the <see cref="SchemaVersion.Proposed"/> version.
    /// </para>
    /// <para>Besides a name, a variable has an integer <see cref="ID"/> that is given on definition and cannot be modified. 
    /// The ID is unique inside its <see cref="Microsoft.Research.Science.Data.DataSet"/> and can be used for identification of a variable 
    /// after it’s been renamed. Note that
    /// several variables with same name within a <see cref="Microsoft.Research.Science.Data.DataSet"/> are allowed, but the <see cref="ID"/> is unique.
    /// </para>
    /// <para>As mentioned above, along with an array a variable contains metadata attached to the data. 
    /// Metadata is a dictionary of keys and values represented as an instance of the <see cref="MetadataDictionary"/> class
    /// and available through the <see cref="Variable.Metadata"/> property. 
    /// Please see remarks for the <see cref="MetadataDictionary"/> class to read about constraints on metadata.
    /// </para>
    /// <para>
    /// In fact, the name of a variable is just a metadata entry with name defined in
    /// <see cref="MetadataDictionary.KeyForName"/>. The same is for a missing value.
    /// Therefore, properties <see cref="Variable.Name"/> and <see cref="Variable.MissingValue"/>
    /// just refer the <see cref="Variable.Metadata"/> collection. 
    /// </para>
    /// <para>
    /// How to . . .
    /// <list type="table">
    /// <listheader>
    /// <term>Action</term>
    /// <description>Solution</description>
    /// </listheader>
    /// <item>
    /// <term>
    /// Add new variable.
    /// </term>
    /// <description>
    /// <para>
    /// Methods of the <see cref="Microsoft.Research.Science.Data.DataSet"/> class, starting with <c>AddVariable...</c>.
    /// For example, <see cref="Microsoft.Research.Science.Data.DataSet.AddVariable{DataType}(string, string[])"/>.	
    /// </para>
    /// <para>
    /// See also <see cref="Microsoft.Research.Science.Data.DataSet.AddVariableByReference(Variable, string[])"/>.
    /// </para>
    /// </description>
    /// </item>
    /// <item>
    /// <term>
    /// Put data into a variable.
    /// </term>
    /// <description>
    /// Methods <see cref="PutData(Array)"/> and <see cref="PutData(int[],Array)"/>
    /// put an array into a variable. Methods <see cref="Append(Array)"/>,
    /// <see cref="Append(Array,int)"/> and <see cref="Append(Array,string)"/> puts an
    /// array at the end of the varaible by a certain dimension.
    /// </description>
    /// </item>
    /// <item>
    /// <term>
    /// Get data from a variable.
    /// </term>
    /// <description>
    /// Method <see cref="GetData()"/> returns entire array of the variables.
    /// Methods <see cref="GetData(int[],int[])"/> and 
    /// <see cref="GetData(int[],int[],int[])"/> return a part of the data.
    /// Generic class <see cref="Variable{DataType}"/> also has
    /// an indexer returning a single value for specified indices:
    /// <code>
    /// Variable&lt;int&gt; v = (Variable&lt;int&gt;)dataSet["var"];
    /// int i = v[new int[] { 0 }]; // gets the value at the index { 0 }
    /// </code>
    /// </description>
    /// </item>
    /// <item>
    /// <term>
    /// Work with metadata.
    /// </term>
    /// <description>
    /// Use a <see cref="MetadataDictionary"/> collection available through
    /// the <see cref="Metadata"/> property.
    /// <code>
    /// Variable v = dataSet["var"];
    /// v.Metadata["Units"] = "m/s";
    /// . . .
    /// string units = (string)v.Metadata["Units"];
    /// </code>
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// Zero-rank variables (also called scalar variables) have <see cref="Variable.Rank"/> 
    /// equal to zero and can contain a single value. To create a scalar variable, common methods like
    /// <see cref="Microsoft.Research.Science.Data.DataSet.AddVariable{DataType}(string,string[])"/> should be used. 
    /// To put data or get data from such
    /// variable, methods like <see cref="Variable.GetData()"/> and <see cref="Variable.PutData(Array)"/>
    /// should be used. The value is represented as a one-dimensional array with a single element.
    /// Please note, the <see cref="Variable.Append(Array)"/> doesn't support scalars and 
    /// throws an exception if is invoked for a scalar variable.
    /// Another way to put data or get data from scalar variable is to use typed indexer
    /// from the <see cref="Variable{DataType}"/> (as in the example below).
    /// In all these methods, an <c>origin</c> parameter (if it is required) must be either null, an array of <see cref="Int32"/>
    /// or special constant <see cref="Variable.DefaultIndices"/>.
    /// <example>
    /// <code>
    ///	Variable&lt;double&gt; scalarDbl = ds.AddVariable&lt;double&gt;("scalarDbl", new string[0]);
    ///	Assert.IsTrue(scalarDbl.Rank == 0);
    ///	ds.Commit();
    ///
    ///	Debug.WriteLine("Default: " + scalarDbl[null]); // prints "Default: 0"
    ///
    ///	// Using PutData and GetData methods:
    ///	scalarDbl.PutData(new double[] { Math.PI });
    ///	ds.Commit();
    ///	Assert.AreEqual(1, scalarDbl.GetData().Length); // contains only one element
    ///	Assert.AreEqual(Math.PI, ((double[])scalarDbl.GetData())[0]); // true
    ///	
    ///	// Using indexer:
    ///	scalarDbl[Variable.DefaultIndices] = 2*Math.PI;
    ///	ds.Commit();
    ///	Assert.AreEqual(2*Math.PI, scalarDbl[Variable.DefaultIndices]); // true
    /// </code>
    /// </example>
    /// </para>
    /// </remarks>    
    /// <example>
    /// <code>
    ///using(DataSet ds = DataSet.Open("msds:memory"))
    ///{
    /// ds.IsAutocommitEnabled = false;
    /// double[] a = new double[] { 12, 34, 43, 67 };
    /// double[,] b = new double[,] { { 4, 5 }, { 3, 4 }, { 5, 4 } };
    /// double[] x = new double[] { 100, 101, 102 };
    /// 
    /// Variable&lt;double&gt; va = ds.AddVariable&lt;double&gt;("a", a, "x");
    /// Variable&lt;double&gt; vb = ds.AddVariable&lt;double&gt;("b", b, "x", "t");
    ///
    /// vb.PutData(new int[] { 1, 1 }, new double[,] { { 8, 9 }, { 13, 14 }, { 15, 14 } });
    ///
    /// ds.Commit();
    ///
    /// #region Modifying data in variables
    ///
    /// double[] suba = (double[])va.GetData(new int[] { 1 }, null);
    /// double[,] subb = (double[,])vb.GetData(new int[] { 0, 1 }, new int[] { 2, 2 });
    ///
    /// #endregion
    ///
    /// Print(ds);
    /// ds.Commit();
    ///
    /// vnames.Append(new string[] { "A", "B", "C", "D" });
    /// vx.PutData(new int[] { 1, 2, 3 });
    ///
    /// Print(vx);
    /// ds.Commit();
    ///}
    ///</code>
    /// </example>
    /// <seealso cref="Variable{DataType}"/>
    /// <seealso cref="Microsoft.Research.Science.Data.DataSet"/>
    public abstract partial class Variable : IDisposable
    {
        #region Private members

        private int id;
        /// <summary>
        /// Current changeset of the variable (not data set!)
        /// </summary>
        private int changeSetId; // starts with 0

        /// <summary>
        /// Committed array of dimensions.
        /// </summary>
        protected internal string[] dimensions;

        /// <summary>Collection of coordinate systems of the variable.</summary>

        private DataSet dataSet;

        private VariableSchema committedSchema;

        /// <summary>Represents changes made in the recent transaction (including schema and data).</summary>
        protected DataChanges changes; //new DataChanges();

        /// <summary>Determines the current stage of the committing process.</summary>
        private CommitStage commitStage;

        private bool readOnly; // false by default

        private bool disposed;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the <see cref="Microsoft.Research.Science.Data.DataSet"/> instance the <see cref="Variable"/> belongs to.
        /// </summary>
        public DataSet DataSet
        {
            get { return dataSet; }
        }

        /// <summary>
        /// Gets the data type for the <see cref="Variable"/>.
        /// </summary>
        public abstract Type TypeOfData { get; }

        /// <summary>
        /// Gets the committed name or sets the new name of the <see cref="Variable"/>.
        /// </summary>
        /// <remarks>
        ///	<para>
        /// Name of a variable is a custom string (possibly, empty).
        /// </para>
        /// <para>Changing the property changes the proposed version of the name of the variable.
        /// If the variable has just been added to a <see cref="Microsoft.Research.Science.Data.DataSet"/>, there is only a proposed 
        /// version of the name, therefore it will be updated.</para>
        /// <para>The method never throws an exception. If the variable has no name, it returns empty string.</para>
        /// </remarks>	
        /// <seealso cref="Variable.ProposedName"/>
        public string Name
        {
            get { return Metadata.GetComittedOtherwiseProposedValue<string>(Metadata.KeyForName, String.Empty); }
            set { Metadata[Metadata.KeyForName] = value; }
        }

        /// <summary>
        /// Gets the proposed name if it exists. If not, gets the committed name.
        /// </summary>
        /// <see cref="Variable.Name"/>
        protected internal string ProposedName
        {
            get { return (string)Metadata[Metadata.KeyForName, SchemaVersion.Recent]; }
        }

        /// <summary>
        /// Gets the current version number of the variable.
        /// </summary>
        public int Version
        {
            get { return changeSetId; }
        }

        /// <summary>
        /// Gets the rank of the variable.
        /// </summary>
        /// <remarks>
        /// Returns non-negative value. Zero means scalar variable.
        /// </remarks>
        public int Rank
        {
            get { return dimensions.Length; }
        }

        /// <summary>
        /// Gets the dimensions list of the <see cref="Variable"/>.
        /// </summary>
        /// <remarks>
        /// <para>The property returns committed dimensions.
        /// To get proposed dimensions, use <see cref="GetSchema(SchemaVersion)"/> method.</para>
        /// </remarks>
        public ReadOnlyDimensionList Dimensions
        {
            get
            {
                return GetDimensions(SchemaVersion.Committed);
            }
        }

        /// <summary>
        /// Gets the metadata associated with the variable.
        /// </summary>
        public abstract MetadataDictionary Metadata { get; }

        /// <summary>
        /// Gets the ID of the variable.
        /// </summary>
        /// <remarks>
        /// It is guaranteed that (1) the ID is unique within the <see cref="Microsoft.Research.Science.Data.DataSet"/> and
        /// (2) it is constant for each variable during life time of the <see cref="Microsoft.Research.Science.Data.DataSet"/>. 
        /// Since a variable might be renamed, the ID should be used for indexing instead of the name
        /// to be confident of correct identification of a variable.
        /// </remarks>
        public int ID
        {
            get { return id; }
            protected set { id = value; }
        }

        /// <summary>
        /// Gets the value indicating whether the variable is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return readOnly; }
            protected internal set { readOnly = value; }
        }

        /// <summary>
        /// Gets the value indicating whether the variable is read-only.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The property is obsolete. Use the propery <see cref="IsReadOnly"/> instead.
        /// </para>
        /// </remarks>
        [Obsolete("This property is obsolete and will be removed in the next version. Use the property \"IsReadOnly\" instead.")]
        public bool ReadOnly
        {
            get { return IsReadOnly; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of the variable.
        /// </summary>
        internal Variable(DataSet dataSet, string[] dims, bool assignID)
        {
            if (dataSet == null)
                throw new ArgumentNullException("dataSet");

            //this.initializing = true;
            this.dataSet = dataSet;

            if (assignID)
                this.id = dataSet.GetNextVariableId();

            ConstructDimensions(dims);
        }

        /// <summary>
        /// Initializes an instance of the variable.
        /// </summary>
        internal Variable(DataSet dataSet, string[] dims)
            : this(dataSet, dims, true)
        {
        }

        /// <summary>
        /// This method must be called by derived class AT THE END of their constructors.
        /// </summary>
        /// <remarks>
        /// The method initializes the schema of the variable.
        /// </remarks>
        protected virtual void Initialize()
        {
            StartChanges();

            Metadata.Changing += OnMetadataChanging;
            Metadata.Changed += OnMetadataChanged;

            //initializing = false;
        }

        private void ConstructDimensions(string[] dims)
        {
            if (dims == null)
                dims = new string[0]; // scalar variable
            this.dimensions = dims;
        }


        #endregion

        #region Events

        /// <summary>
        /// Occurs when the variable is rolled back.
        /// </summary>
        internal event VariableRolledBackEventHandler RolledBack;

        private void FireEventVariableRolledback()
        {
            if (RolledBack != null)
                RolledBack(this, new VariableRolledBackEventArgs(this));
        }

        /// <summary>
        /// Occurs on every change of the variable (i.e. a change in the *proposed* schema, not *committed*).
        /// </summary>
        internal event VariableChangedEventHandler Changed;

        /// <summary>
        /// Fires the Variable.Changed event.
        /// </summary>
        protected void FireEventVariableChanged(VariableChangeAction action)
        {
            if (Changed != null)
                Changed(this, new VariableChangedEventArgs(this, changes, action));
        }

        /// <summary>
        /// Occurs when the variable is committing.
        /// </summary>
        internal event VariableCommittingEventHandler Committing;

        /// <summary>
        /// Returns false if it is required cancel the committing.
        /// </summary>
        private bool FireEventVariableCommitting(Changes changes, out Exception cancelReason)
        {
            cancelReason = null;

            try
            {
                VariableCommittingEventArgs e = new VariableCommittingEventArgs(this, changes);
                if (Committing != null)
                    Committing(this, e);

                return !e.Cancel;
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(DataSet.TraceDataSet.TraceWarning, "OnVariableCommitting: " + ex.Message);

                cancelReason = ex;
                return false;
            }
        }

        /// <summary>
        /// Occurs when the variable has been committed successully.
        /// </summary>
        internal event VariableCommittedEventHandler Committed;

        private void FireEventVariableCommitted(Changes changes)
        {
            try
            {
                if (Committed != null)
                    Committed(this, new VariableCommittedEventArgs(this, changes));
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(DataSet.TraceDataSet.TraceWarning, "Exception in event VariableCommitted: " + ex.Message + "\n" + ex.StackTrace);
                throw ex;
            }
        }

        /// <summary>
        /// Occurs when the variable is changing.
        /// </summary>
        internal event VariableChangingEventHandler Changing;

        /// <summary>
        /// Fires the <see cref="Variable.Changing"/> event and
        /// returns false if it is required to cancel changing.
        /// </summary>
        /// <param name="proposedChanges"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        protected bool FireEventVariableChanging(VariableChangeAction action, Changes proposedChanges)
        {
            try
            {
                lock (DataSet)
                {
                    if (Changing != null)
                    {
                        VariableChangingEventArgs e = new VariableChangingEventArgs(this, proposedChanges, action);
                        Changing(this, e);

                        return !e.Cancel;
                    }
                    else
                        return true;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(DataSet.TraceDataSet.TraceWarning, "OnVariableChanging: " + ex);
                return false;
            }
        }

        #endregion

        #region Metadata

        /// <summary>
        /// Gets or sets a value to be considered as a missing value for the variable's data.
        /// </summary>
        /// <remarks>
        /// <para>Type of the value must be same as the vatiable's <see cref="TypeOfData"/>.</para>
        /// </remarks>
        public object MissingValue
        {
            get
            {
                return Metadata.GetComittedOtherwiseProposedValue<object>(Metadata.KeyForMissingValue, null);
            }
            set
            {
                Metadata[Metadata.KeyForMissingValue] = value;
            }
        }

        /// <summary>
        /// The method is invoked when variable's metadata is changed.
        /// </summary>
        /// <remarks>
        /// The method is invoked every time variable's <see cref="Metadata"/> is changed.
        /// It starts new transaction (see <see cref="StartChanges"/> method).
        /// </remarks>
        protected virtual void OnMetadataChanged(object sender, VariableMetadataChangedEventArgs e)
        {
            StartChanges();

            // Firing the event
            FireEventVariableChanged(VariableChangeAction.UpdateMetadata);
        }

        /// <summary>
        /// The method is invoked before variable's metadata is changed.
        /// </summary>
        protected virtual void OnMetadataChanging(object sender, VariableMetadataChangingEventArgs e)
        {
            lock (DataSet)
            {
                if (Changing != null)
                {
                    if (e.Key == Metadata.KeyForMissingValue && e.ProposedValue != null &&
                        e.ProposedValue.GetType() != TypeOfData)
                        throw new DataSetException("Type of MissingValue differs from the variable's data type");

                    MetadataDictionary vm = new MetadataDictionary();
                    vm[e.Key] = e.ProposedValue;
                    var metadataChanges = vm.Clone(SchemaVersion.Proposed, true);

                    Changes changes = new Changes(Version, GetSchema(SchemaVersion.Committed), metadataChanges, null, new Rectangle());
                    // Firing the event
                    e.Cancel = !FireEventVariableChanging(VariableChangeAction.UpdateMetadata, changes);
                }
            }
        }


        #endregion

        #region Get Data

        /// <summary>
        /// Gets the data for the variable from specified rectangular region.
        /// </summary>
        /// <param name="origin">The origin of the rectangle (left-bottom corner). Null means all zeros.</param>
        /// <param name="shape">The shape of the rectangle. Null means maximal shape.</param>
        /// <returns>An array of data from specified rectangle.</returns>
        /// <remarks>
        /// <para>
        /// A variable is not a data storage itself; 
        /// it is just a representation of an array and its metadata stored in the underlying data storage, 
        /// and it provides an on-demand access to that storage. 
        /// This enables work with very large arrays, stored in some format, when a researcher might need 
        /// only a part of the data in memory.
        /// </para><para>
        /// To get a committed shape (i.e. vector of lengths by each dimension) of the underlying array the 
        /// <see cref="GetShape()"/> methods should be used. 
        /// </para>
        /// <para>
        /// The method <see cref="GetData(int[],int[])"/> enables retrieving a sub-array of the array. 
        /// Its parameters specify a rectangle of indices that defines which part of the array 
        /// should be retrieved. First parameter is an <paramref name="origin"/> of the rectangle, 
        /// which is an index vector. 
        /// Second parameter defines <paramref name="shape"/> of the rectangle, 
        /// which is a vector of lengths of the rectangle by each dimension. 
        /// Lengths of both <paramref name="origin"/> and <paramref name="shape"/> parameters 
        /// must be equal to the <see cref="Rank"/> of the variable, 
        /// i.e. the method doesn’t reduce the dimensionality of an array. </para>
        /// <para>
        /// Requested rectangle must be less or equal to the entire shape of a variable (<see cref="GetShape()"/>). 
        /// If the <paramref name="origin"/> is null, it will be considered as a vector of zeros.  
        /// If the <paramref name="shape"/> is null, the maximum available shape will be used. 
        /// For instance, <c>GetData(null, null)</c> returns entire array.
        /// </para>
        /// <para>
        /// The GetData methods always get committed data, even if the <see cref="Microsoft.Research.Science.Data.DataSet"/> has changes.
        /// </para>
        /// <para>For scalar variables <paramref name="origin"/> and <paramref name="shape"/> should be 
        /// either null or has zero length. For scalar variables GetData returns one dimensional array 
        /// with one item.</para>
        /// </remarks>
        public abstract Array GetData(int[] origin, int[] shape);

        /// <summary>
        /// Gets the data for the variable from specified stridden slices.
        /// </summary>
        /// <param name="origin">The origin of the rectangle (left-bottom corner). Null means all zeros.</param>
        /// <param name="count">The shape of the rectangle.</param>
        /// <param name="stride">Steps to stride the variable.</param>
        /// <returns>An array of data from specified rectangle.</returns>
        /// <remarks>
        /// <para>
        /// Resulting array contains values from the <see cref="Variable"/> and has the same <see cref="Rank"/> as the variable does. 
        /// Shape of the resulting array is equal to <paramref name="count"/>,
        /// except when the <paramref name="count"/> is null. In this case,
        /// the real shape of the stridden array is "as much as possible" depending on the shape of the 
        /// variable (<see cref="GetShape()"/>).
        /// If <paramref name="origin"/> is null, an integer array with all zeros is inferred.
        /// The <paramref name="stride"/> contains a step to stride values of the variable.
        /// For 1d variables, value of a stridden variable with the index <c>i</c> corresponds
        /// to the value with index <c>start[0] + i*stride[0]</c> of the variable.
        /// For multidimensional variables, the principle is similar.
        /// </para>
        /// <para>
        /// See also remarks for <see cref="GetData(int[],int[])"/>.
        /// </para>
        /// <example>
        /// <code>
        /// using (DataSet target = CreateDataSet())
        /// {
        ///		/* Preparing the data set */
        ///		Variable&lt;int&gt; v1d = target.AddVariable&lt;int&gt;("test1d", "x");
        ///		v1d.PutData(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
        ///		
        ///		Array arr = v1d.GetData(new int[] { 1 }, new int[] { 2 }, null);
        ///		
        ///		Assert.IsTrue(helper_Compare(arr, new int[] { 1, 3, 5, 7, 9 })); // true
        ///	}
        /// </code>
        /// </example>
        /// <para>For scalar variables <paramref name="origin"/>, <paramref name="count"/> and 
        /// <paramref name="stride"/> should be either null or has zero length. For scalar variables GetData returns one dimensional array 
        /// with one item.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Research.Science.Data.DataSet.StrideVariable{DataType}(Variable{DataType},int[],int[],int[],string,IList{string},IList{string})"/>
        public virtual Array GetData(int[] origin, int[] stride, int[] count)
        {
            if (Rank == 0)
            {
                if (origin != null && origin.Length > 0)
                    throw new ArgumentException("origin");
                if (count != null && count.Length > 0)
                    throw new ArgumentException("count");
                if (stride != null && stride.Length > 0)
                    throw new ArgumentException("stride");
                return GetData(null, null);
            }

            if (stride == null)
            {
                // default stride is 1
                return GetData(origin, count);
            }

            int rank = Rank;

            // If stride == 1 we can use simple GetData method
            int k = 0;
            for (k = 0; k < rank; k++)
            {
                if (stride[k] != 1) break;
            }
            if (k == rank)
                return GetData(origin, count);

            // Going further...
            if (origin == null)
                origin = new int[rank];

            if (count == null)
            {
                // taking count as large as possible
                count = new int[rank];
                int[] entireShape = GetShape(SchemaVersion.Committed);
                for (int i = 0; i < rank; i++)
                {
                    count[i] = 1 + (entireShape[i] - 1 - origin[i]) / stride[i];
                    //(int)Math.Ceiling(((double)(entireShape[i] - origin[i])) / stride[i]);
                }
            }
            else if (Array.IndexOf(count, 0) >= 0)
                return Array.CreateInstance(TypeOfData, count);

            int[] shape = new int[rank];
            for (int i = 0; i < rank; i++)
                shape[i] = (count[i] - 1) * stride[i] + 1;

            Array entireData = GetData(origin, shape);

            // Extracting required elements
            return GetStride(entireData, stride, count);
        }

        #region Striding

        private T[] GetStride1d<T>(Array a, int s, int outputN)
        {
            T[] array = (T[])a;
            if (s == 1)
            {
                if (array.Length != outputN)
                    throw new Exception();
                return array;
            }
            T[] output = new T[outputN];
            for (int i = 0, j = 0; i < array.Length; i += s, j++)
            {
                output[j] = array[i];
            }
            return output;
        }

        /// <summary>
        /// Subsets the array with given strides and count parameters.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="stride"></param>
        /// <param name="outputCount"></param>
        /// <returns>An array that is a subset of the source array.</returns>
        protected Array GetStride(Array a, int[] stride, int[] outputCount)
        {
            Type t = TypeUtils.GetElementType(a);

            /*if (a.Rank == 1)
            {
                return (Array)this.GetType().GetMethod("GetStride1d", System.Reflection.BindingFlags.NonPublic).MakeGenericMethod(t).Invoke(this,
                    new object[] { a, stride[0], outputCount[0] });
            }*/

            int[] index = new int[a.Rank];
            int[] sindex = new int[a.Rank];

            Array output = Array.CreateInstance(t, outputCount);

            int length = a.Length;
            for (int i = 0; i < length; i++)
            {
                output.SetValue(a.GetValue(index), sindex);

                for (int j = 0; j < index.Length; j++)
                {
                    index[j] += stride[j];
                    sindex[j]++;
                    if (sindex[j] < outputCount[j])
                        break;
                    index[j] = 0;
                    sindex[j] = 0;
                }
            }
            return output;
        }

        #endregion

        /// <summary>
        /// Gets the entire data for the variable.
        /// </summary>
        /// <remarks>
        /// See remarks for <see cref="GetData(int[],int[])"/>.
        /// </remarks>
        public Array GetData()
        {
            return GetData(null, null);
        }

        #endregion

        #region Put Data

        /// <summary>
        /// Puts the data to the variable starting with specified origin indices.
        /// </summary>
        /// <param name="origin">Indices to start adding of data. Null means all zeros.</param>
        /// <param name="data">Data to put to the variable.</param>        
        /// <remarks>
        /// <para>If a variable is not read-only (see <see cref="IsReadOnly"/>), 
        /// then data (that is an array of a correct type and rank) 
        /// can be put into a variable at any time. All dimensions of a variable are unbounded, 
        /// so it is possible to extend it by any dimension.
        /// </para>
        /// <para>
        /// The <paramref name="data"/> array is copied by the PutData method, therefore after this method call, it is allowed
        /// to change the array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// <para>Usually, an SDS provider doesn’t write affected changes into the underlying storage 
        ///     immediately (but its behavior still depends on a particular implementation). 
        ///     To perform it, after all planned actions with the <see cref="Microsoft.Research.Science.Data.DataSet"/> are completed, commit 
        ///     changes as described in remarks for <see cref="Microsoft.Research.Science.Data.DataSet.Commit()"/>.
        /// </para>
        /// <para>Important: all arrays as parameters in the PutData methods must have a rank equal 
        /// to the variable’s rank (see <see cref="Rank"/>).
        /// </para>
        /// <para>The method <see cref="PutData(Array)"/> of the <see cref="Variable"/> allows putting an array into variable 
        /// starting with zero indices.
        /// </para>
        /// <para>
        /// Another method <see cref="PutData(int[],Array)"/> puts data into a rectangle 
        /// defined by a shape of a given array and 
        /// an <paramref name="origin"/> parameter, which is an index vector of the starting 
        /// corner of the rectangle to place data in.
        /// </para>
        /// <para>If the variable was not empty, then overlapping values are replaced 
        /// by values from the proposed array. If new array is larger than existing, 
        /// variable’s shape is extended. In this case, 
        /// there can be areas those are not overlapped by both existing data and proposed.</para>
        /// <para>
        /// Append methods allow appending an array to existing data. See more in remarks for
        /// <see cref="Append(Array,string)"></see>.
        /// </para>
        /// <para>For scalar variables <paramref name="origin"/> should be either null or has zero length. 
        /// Array <paramref name="data"/> should be one dimensional array containing one item.</para>
        /// </remarks>
        /// <example>
        /// <code>
        ///void DemoAppendPut(DataSet sds)
        ///{
        /// Variable&lt;int&gt; var = sds.AddVariable&lt;int&gt;("demoVar", 1);
        /// // var contains an empty array at the moment
        ///
        /// var.PutData(new int[] { 1, 2, 3 } );
        /// // var contains: 1, 2, 3
        ///
        /// var.PutData(new int[] { 4 }, new int[] { 3, 2, 1 });
        /// // var contains: 1, 2, 3, 0, 3, 2, 1
        ///
        /// var.PutData(new int[] { 3 }, new int[] { -1 });
        /// // var contains: 1, 2, 3, -1, 3, 2, 1
        ///
        /// var.Append(new int[] { 0, 0, 0 });
        /// // var contains: 1, 2, 3, -1, 3, 2, 1, 0, 0, 0
        ///}
        ///</code>
        ///<para>
        ///Don’t forget either commit changes to make them actual or enable autocommit
        ///(see <see cref="Microsoft.Research.Science.Data.DataSet.IsAutocommitEnabled"/>).
        ///</para>
        /// </example>
        /// <seealso cref="Append(Array,string)"/> 
        /// <seealso cref="GetData(int[],int[])"/>
        /// <seealso cref="Microsoft.Research.Science.Data.DataSet.AddVariable(string,string[])"/>
        /// <seealso cref="Microsoft.Research.Science.Data.DataSet.Commit()"/>
        /// <exception cref="ObjectDisposedException">DataSet is disposed.</exception>
        /// <exception cref="ReadOnlyException">Variable is read-only.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        /// <exception cref="ArgumentException">Arguments are incorrect.</exception>
        /// <exception cref="CannotPerformActionException">Cannot put data into the variable.</exception>
        public virtual void PutData(int[] origin, Array data)
        {
            if (DataSet.IsDisposed)
                throw new ObjectDisposedException("DataSet is disposed");
            if (readOnly)
                throw new ReadOnlyException("Variable is read only.");
            if (data == null)
                throw new ArgumentNullException("Data array");
            if (data.GetType().GetElementType() != TypeOfData)
                throw new ArgumentException("Given array has wrong element type");
            if (Rank == 0)
            {
                if (origin != null && origin.Length > 0)
                    throw new ArgumentException("PutData for scalar variable requires either null or empty origin");
            }
            else
            {
                if (origin != null && origin.Length != Rank)
                    throw new ArgumentException("Origin contains incorrect number of dimensions.");
                if (data.Rank != Rank)
                    throw new ArgumentException("Array has wrong rank.");
            }

            /***********************************************************************/
            int rank = Rank;
            // Preparing proposed changes description			
            int[] propShape;
            Rectangle propAR = new Rectangle();

            if (origin == null)
                origin = new int[rank];
            else
            {
                var safeOrigin = new int[rank];
                origin.CopyTo(safeOrigin, 0);
                origin = safeOrigin;
            }

            lock (DataSet)
            {
                // Updating shape
                int[] cshape = GetShape(); // committed shape
                propShape = changes == null ? null : changes.Shape;	// Proposed Changes: Shape
                int[] proposedShape = propShape;
                if (proposedShape == null)
                {
                    proposedShape = (int[])cshape.Clone();
                    propShape = proposedShape;
                }

                for (int i = 0; i < rank; i++)
                {
                    int shape = origin[i] + data.GetLength(i);
                    if (shape > proposedShape[i])
                        proposedShape[i] = shape;
                }
                if (rank > 0)
                {
                    // Updating affected rectangle
                    if (changes == null || !changes.HasData)
                    {
                        // Proposed Changes: AffectedRectangle
                        propAR = new Rectangle(data.Rank);
                        for (int i = 0; i < data.Rank; i++)
                        {
                            propAR.Origin[i] = origin[i];
                            propAR.Shape[i] = data.GetLength(i);
                        }
                    }
                    else // if there is a proposed data, updating affected rectangle on its basis
                    {
                        propAR = changes.AffectedRectangle;
                        for (int i = 0; i < origin.Length; i++)
                        {
                            int max = propAR.Shape[i] + propAR.Origin[i];

                            propAR.Origin[i] = Math.Min(propAR.Origin[i], origin[i]);

                            if (max < origin[i] + data.GetLength(i))
                                max = origin[i] + data.GetLength(i);
                            propAR.Shape[i] = max - propAR.Origin[i];
                        }
                    }
                    DataSet.ExtendAffectedRectangle(propAR, cshape);
                }

                Changes proposedChanges = new Changes(Version, GetSchema(SchemaVersion.Committed), null, propShape,
                    propAR);

                /***********************************************************************/
                // Firing an event
                if (!FireEventVariableChanging(VariableChangeAction.PutData, proposedChanges))
                    throw new CannotPerformActionException("PutData is cancelled.");

                /***********************************************************************/
                // Putting data
                StartChanges();
                changes.Shape = proposedChanges.Shape;
                changes.AffectedRectangle = proposedChanges.AffectedRectangle;

                /***********************************************************************/
                // Adding new data piece to the change list
                DataPiece piece = new DataPiece();
                piece.Origin = origin != null ? (int[])origin.Clone() : null;
                piece.Data = (Array)data.Clone();
                changes.Data.Add(piece);
            }
            /***********************************************************************/
            // Firing the event
            FireEventVariableChanged(VariableChangeAction.PutData);
        }

        /// <summary>
        /// Puts the data to the variable starting with zero indices.
        /// </summary>
        /// <param name="a">Data to add to the variable.</param>
        /// <remarks>See remarks for <see cref="PutData(int[],Array)"></see>.</remarks>
        public void PutData(Array a)
        {
            PutData(null, a);
        }

        /// <summary>
        /// Appends the data to the variable by first dimension.
        /// </summary>
        /// <param name="a">Data to append to the variable.</param>
        /// <remarks>See remarks for <see cref="Append(Array,string)"></see>.</remarks>
        public void Append(Array a)
        {
            if (Rank == 0)
                throw new NotSupportedException("Scalar variable doesn't support Append operation");
            Append(a, 0);
        }

        /// <summary>
        /// Appends the data to the variable by specified dimension.
        /// </summary>
        /// <param name="a">Data to append to the variable.</param>
        /// <param name="dimToAppend">Recent name of the dimension to append by.</param>
        /// <remarks>
        /// <para>
        /// Append methods allow appending an array to existing data. The simplest version of the method
        /// <see cref="Append(Array)"/> accepts only an array and appends 
        /// a variable by its first dimension (i.e. a dimension with index 0).
        /// </para>
        /// <para>
        /// The other Append methods allow to specify either index of a dimension to append by or 
        /// its name (if dimensions names have been explicitly specified on a definition of the variable, 
        /// see remarks for <see cref="Microsoft.Research.Science.Data.DataSet.AddVariable(string,string[])"/>).
        /// </para>
        /// <para>
        /// All changes becomes actual only after committing (<see cref="Microsoft.Research.Science.Data.DataSet.Commit"/>).
        /// </para>
        /// <para>
        /// The <paramref name="a"/> array is copied by the method, therefore after this method call, it is allowed
        /// to change the array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// <para>
        /// Note that an array <paramref name="a"/> may have the same rank as the variable does or
        /// rank of <paramref name="a"/> may be one less than variable.
        /// So if you need to append a single slice to a variable, 
        /// it can be presented as an array of rank <see cref="Rank"/> or
        /// as an array of rank <see cref="Rank"/> minus one.
        /// <code>
        /// // 1 4     7
        /// // 2 5  +  8 :
        /// // 3 6     9
        /// Variable&lt;int&gt; v = dataSet.AddVariable&lt;int&gt;("v", "1", "2");
        /// v.PutData(new int[2,3] { {1,2,3}, {4,5,6} });
        /// v.Append(new int[1,3] { { 7, 8, 9 } }); // OK
        /// v.Append(new int[3] { 7, 8, 9 }); // OK
        /// </code>
        /// </para>
        /// </remarks>
        /// <example>
        /// See the example for <see cref="PutData(int[], Array)"/>.
        /// </example>
        /// <seealso cref="PutData(int[],Array)"/>
        /// <seealso cref="Microsoft.Research.Science.Data.DataSet.Commit"/>
        public void Append(Array a, string dimToAppend)
        {
            if (Rank == 0)
                throw new NotSupportedException("Scalar variable doesn't support Append operation");
            if (a.GetType().GetElementType() != TypeOfData)
                throw new ArgumentException("Given array has wrong element type");

            int shapeIndex;

            lock (DataSet)
            {
                shapeIndex = Array.IndexOf<string>(dimensions, dimToAppend);
            }

            Append(a, shapeIndex);
        }

        /// <summary>
        /// Appends the data to the variable by specified dimension.
        /// </summary>
        /// <param name="a">Data to append to the variable. Rank of this array should be equal or be one less than
        /// rank of the variable</param>
        /// <param name="dimToAppend">Zero-based index of the dimension to append by.</param>
        /// <remarks>See remarks for <see cref="Append(Array,int)"></see>.</remarks>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <exception cref="ReadOnlyException"/>
        public virtual void Append(Array a, int dimToAppend)
        {
            if (DataSet.IsDisposed)
                throw new ObjectDisposedException("DataSet is disposed");
            if (Rank == 0)
                throw new NotSupportedException("Scalar variable doesn't support Append operation");
            if (dimToAppend < 0 || dimToAppend >= Rank)
                throw new ArgumentOutOfRangeException("Incorrect index of dimension to append.");
            if (readOnly)
                throw new ReadOnlyException("Variable is read only.");
            if (a == null)
                throw new ArgumentNullException("Data array");
            if (a.GetType().GetElementType() != TypeOfData)
                throw new ArgumentException("Given array has wrong element type");

            // Fill lengthes for array to be appended.
            // Lengthes array always has length equal to Rank even if a.Rank < Rank. 
            int[] alength = new int[Rank];
            if (a.Rank == Rank)
            {
                for (int i = 0; i < a.Rank; i++)
                    alength[i] = a.GetLength(i);
            }
            else if (a.Rank == Rank - 1)
            {
                for (int i = 0; i < dimToAppend; i++)
                    alength[i] = a.GetLength(i);
                alength[dimToAppend] = 1;
                for (int i = dimToAppend + 1; i < Rank; i++)
                    alength[i] = a.GetLength(i - 1);
            }
            else
                throw new ArgumentException("Array has wrong rank.");

            // Preparing origin
            int[] origin = new int[Rank];
            int[] supposedOrigin = new int[Rank];

            lock (DataSet)
            {
                if (HasChanges && changes.Shape != null)
                {
                    supposedOrigin[dimToAppend] = changes.Shape[dimToAppend];
                }
                else
                {
                    int[] shape = GetShape();
                    if (shape != null && shape.Length > 0)
                        supposedOrigin[dimToAppend] = shape[dimToAppend];
                }

                // origin allows to distinct appended data piece from put:
                // it has all zeros except one containing -1, related to the dimension it is appended by.
                origin[dimToAppend] = -1;

                //--------------------------------------------------------------------------
                // Saving changes
                //--------------------------------------------------------------------------

                /***********************************************************************/
                // Preparing proposed changes description				
                // Updating shape
                int[] propShape;
                Rectangle propAR = new Rectangle();
                propShape = changes == null ? null : changes.Shape;	// Proposed Changes: Shape
                int[] proposedShape = propShape;
                int[] cshape = GetShape();
                if (proposedShape == null)
                {
                    proposedShape = (int[])cshape.Clone();
                    propShape = proposedShape;
                }

                for (int i = 0; i < proposedShape.Length; i++)
                {
                    int shape = supposedOrigin[i] + alength[i];
                    if (shape > proposedShape[i])
                        proposedShape[i] = shape;
                }

                // Updating affected rectangle
                if (changes == null || !changes.HasData)
                {
                    // Proposed Changes: AffectedRectangle
                    propAR = new Rectangle(Rank);
                    for (int i = 0; i < Rank; i++)
                    {
                        propAR.Origin[i] = supposedOrigin[i];
                        propAR.Shape[i] = alength[i];
                    }
                }
                else // if there is a proposed data, updating affected rectangle on its basis
                {
                    propAR = changes.AffectedRectangle;
                    for (int i = 0; i < origin.Length; i++)
                    {
                        int max = propAR.Shape[i] + propAR.Origin[i];

                        propAR.Origin[i] = Math.Min(propAR.Origin[i], supposedOrigin[i]);

                        if (max < supposedOrigin[i] + alength[i])
                            max = supposedOrigin[i] + alength[i];
                        propAR.Shape[i] = max - propAR.Origin[i];
                    }
                }
                DataSet.ExtendAffectedRectangle(propAR, cshape);
                Changes proposedChanges = new Changes(Version, GetSchema(SchemaVersion.Committed), null, propShape,
                    propAR);

                /***********************************************************************/
                // Firing an event
                if (!FireEventVariableChanging(VariableChangeAction.PutData, proposedChanges))
                    throw new OperationCanceledException("PutData is cancelled.");

                /***********************************************************************/
                // Putting data
                StartChanges();
                changes.Shape = proposedChanges.Shape;
                changes.AffectedRectangle = proposedChanges.AffectedRectangle;

                /***********************************************************************/
                // Adding new data piece to the change list
                DataPiece piece = new DataPiece();
                piece.Origin = origin;
                if (a.Rank == Rank)
                    piece.Data = (Array)a.Clone();
                else
                {
                    int[] srcIdx = new int[a.Rank];
                    int[] dstIdx = new int[Rank];
                    Array a2 = Array.CreateInstance(TypeOfData, alength);
                    if (a2.Length != 0)
                        while (true)
                        {
                            a2.SetValue(a.GetValue(srcIdx), dstIdx);
                            int i = 0;
                            while (i < a.Rank && ++srcIdx[i] >= a.GetLength(i))
                            {
                                srcIdx[i] = 0;
                                i++;
                            }
                            if (i >= a.Rank)
                                break;
                            for (int j = 0; j < dimToAppend; j++)
                                dstIdx[j] = srcIdx[j];
                            for (int j = dimToAppend + 1; j < Rank; j++)
                                dstIdx[j] = srcIdx[j - 1];
                        }
                    piece.Data = a2;
                }
                changes.Data.Add(piece);
            }

            /***********************************************************************/
            // Firing the event
            FireEventVariableChanged(VariableChangeAction.PutData);
        }

        /// <summary>
        /// Sets the read only flag.
        /// </summary>
        protected internal void SetCompleteReadOnly()
        {
            if (HasChanges)
                throw new Exception("Cannot set read only flag to a variable while it has changes");
            readOnly = true;
            Metadata.ReadOnly = true;
        }


        #endregion

        #region Dimensions

        /// <summary>
        /// Returns the actual shape of the variable.
        /// </summary>
        protected abstract int[] ReadShape();

        /// <summary>
        /// Gets the shape of the variable.
        /// </summary>
        /// <remarks>
        /// <para>Shape of the variable is the array of number of elements in each dimension.</para>
        /// <para>The method returns the committed shape of the variable.</para>
        /// </remarks>
        /// <seealso cref="GetShape(SchemaVersion)"/>
        /// <seealso cref="Microsoft.Research.Science.Data.DataSet.Commit"/>
        public int[] GetShape()
        {
            return GetShape(SchemaVersion.Committed);
        }

        /// <summary>
        /// Computes and returns the specific version of a shape for the variable.
        /// </summary>
        /// <remarks>
        /// <para>Shape of the variable is the array of number of elements in each dimension.</para>
        /// <para>The method returns the shape of the variable for the given schema version.</para>
        /// </remarks>
        /// <seealso cref="GetShape()"/>
        /// <seealso cref="SchemaVersion"/>
        /// <seealso cref="Microsoft.Research.Science.Data.DataSet.Commit"/>
        protected internal int[] GetShape(SchemaVersion version)
        {
            if (Rank == 0) return new int[0];

            int[] shape;
            lock (DataSet)
            {
                if (HasChanges && (version == SchemaVersion.Recent || version == SchemaVersion.Proposed))
                {
                    return changes.Shape;
                }

                var schema = GetCommittedSchema(false);
                if (schema == null)
                    shape = ReadShape();
                else
                    shape = schema.Dimensions.AsShape();
            }
            if (shape == null)
                return new int[Rank];
            else if (shape.Length != Rank)
                throw new ApplicationException("ReadShape returned a shape with wrong number of elements (different than the rank)");

            return shape;
        }

        /// <summary>
        /// Gets the dimension list of the variable for the specified schema version.
        /// </summary>
        protected internal ReadOnlyDimensionList GetDimensions(SchemaVersion version)
        {
            if (version == SchemaVersion.Proposed && !HasChanges)
                throw new DataSetException("Variable has been committed and there is no proposed version.");

            lock (DataSet)
            {
                if ((!HasChanges && version == SchemaVersion.Recent) ||
                    version == SchemaVersion.Committed)
                {
                    int[] shape = GetShape(version);
                    Dimension[] dimensions = new Dimension[shape.Length];

                    if (shape == null)
                        for (int i = 0; i < dimensions.Length; i++)
                        {
                            dimensions[i] = new Dimension(this.dimensions[i], 0);
                        }
                    else
                        for (int i = 0; i < shape.Length; i++)
                        {
                            dimensions[i] = new Dimension(this.dimensions[i], shape[i]);
                        }
                    return new ReadOnlyDimensionList(dimensions);
                }
                else
                {
                    Dimension[] dimensions = new Dimension[changes.Shape.Length];
                    for (int i = 0; i < changes.Shape.Length; i++)
                    {
                        dimensions[i] = new Dimension(this.dimensions[i], changes.Shape[i]);
                    }

                    return new ReadOnlyDimensionList(dimensions);
                }
            }
        }

        #endregion

        #region Schemas and transactions

        /// <summary>
        /// Gets the value indicating whether the variable is modified.
        /// </summary>
        /// <remarks>
        /// The <see cref="Variable"/> is modified, if a user changed its metadata or data.
        /// If a variable becomes modified, all <see cref="Microsoft.Research.Science.Data.DataSet"/> becomes modified, too
        /// (see <see cref="Microsoft.Research.Science.Data.DataSet.HasChanges"/>).
        /// To commit changes, use <see cref="Microsoft.Research.Science.Data.DataSet.Commit"/> method.
        /// To roll changes back, use <see cref="Microsoft.Research.Science.Data.DataSet.Rollback"/> method.
        /// </remarks>
        public bool HasChanges
        {
            get
            {
                lock (DataSet)
                {
                    return changes != null;
                }
            }//changes.HasChanges; }
        }

        /// <summary>
        /// Rolls back variable changes.
        /// </summary>
        /// <remarks>
        /// Rolls back changes for this variable. <see cref="Microsoft.Research.Science.Data.DataSet"/> remains modified.
        /// See also <see cref="Microsoft.Research.Science.Data.DataSet.Rollback"/>.
        /// </remarks>
        /// <seealso cref="HasChanges"/>
        public void Rollback()
        {
            lock (DataSet)
            {
                if (!HasChanges) return;

                try
                {
                    Metadata.Rollback();
                    OnRollback(changes);
                }
                catch (Exception ex)
                {
                    Trace.WriteLineIf(DataSet.TraceDataSet.TraceError, "Exception in rollback: " + ex.Message);
                    throw ex;
                }

                ClearChanges();

                FireEventVariableRolledback();

                if (changeSetId == 0)
                {
                    // The variable was added and rolled back. 
                    // It no more belongs to a DataSet.
                    this.Changing = null;
                    this.Changed = null;
                    this.Committing = null;
                    this.Committed = null;
                    this.RolledBack = null;
                }
            }
        }

        /// <summary>
        /// Makes checks of internal constraints.
        /// </summary>
        /// <exception cref="ConstraintsFailedException"/>
        internal void DoCheckConstraints(DataSet.Changes proposedChanges)
        {
            Variable.Changes vchanges = proposedChanges.GetVariableChanges(ID);
            if (vchanges == null)
                return; // no changes to check
            if (commitStage >= CommitStage.ConstrainsChecked)
                return;

            CheckCoordinateSystems(proposedChanges);
            CheckConstraints(vchanges);

            this.commitStage = CommitStage.ConstrainsChecked;
        }

        /// <summary>
        /// Set of dimensions for each CS must be equal to the set of dimensions for the variable.
        /// </summary>
        /// <exception cref="ConstraintsFailedException"/>		
        private void CheckCoordinateSystems(DataSet.Changes dataSetChangeset)
        {
            Variable.Changes proposedChanges = dataSetChangeset.GetVariableChanges(ID);
            if (proposedChanges == null)
                return; // no changes to check
            ReadOnlyDimensionList dlist = proposedChanges.GetDimensionList();
        }

        /// <summary>
        /// If the variable needs for some extra checks before committing this method must be overriden.
        /// </summary>
        /// <remarks>
        /// It shall throw an exception in case of a check failure.
        /// </remarks>
        /// <exception cref="ConstraintsFailedException"></exception>
        protected virtual void CheckConstraints(Changes proposedChanges)
        {
        }

        /// <summary>
        /// The method is called at the precommit stage of the variable. 
        /// </summary>
        /// <remarks>
        /// At Precommit stage it is required to change the state.
        /// Override it if you want to perform precommit stage for the variable on your own way in addition to basic.
        /// On errors an exception should be thrown.
        /// </remarks>
        /// <exception cref="ConstraintsFailedException"></exception>
        internal protected virtual void OnPrecommit(Variable.Changes proposedChanges)
        {
        }

        /// <summary>
        /// The method is called at the commit stage of the variable.
        /// </summary>
        /// <remarks>
        /// At Commit stage it is required to commit the change of state.
        /// Override it if you want to perform commit stage for the data set on your own way in addition to basic.
        /// </remarks>
        internal protected virtual void OnCommit(Variable.Changes proposedChanges)
        {
        }

        /// <summary>
        /// The method is called when the precommit fails. Providers can cancel possible preparations for committed.
        /// </summary>
        internal protected virtual void Undo()
        {
        }

        /// <summary>
        /// The method is called at the the rollback stage of the variable.
        /// </summary>
        /// <remarks>
        /// At Rollback stage it is required to rollback the change of state.
        /// Override it if you want to perform rollback stage for the data set on your own way in addition to basic.
        /// </remarks>
        internal protected virtual void OnRollback(Variable.Changes proposedChanges)
        {
        }

        /// <summary>Checks constraints for the given changeset.</summary>
        /// <remarks>
        /// Checks all constraints related to variable for given changeset and returns
        /// true if all of them are satisfied. No events are thrown during the process.
        /// </remarks>
        /// <param name="changeset"></param>
        /// <returns></returns>
        internal protected void CheckChangeset(DataSet.Changes changeset)
        {
            if (changeset == null) return;
            /* Checking internal constraints */
            DoCheckConstraints(changeset);
        }

        /// <summary>
        /// Uses data set's local changes to checks constraints.
        /// </summary>
        internal void PrecommitLocalChanges()
        {
            if (!DataSet.HasChanges) return;
            Precommit(DataSet.GetInnerChanges());
        }

        /// <summary>
        /// Checks both internal and external constraints and, if successful,
        /// changes the state of the variable in accordance with requests (including data writing).
        /// </summary>
        internal void Precommit(DataSet.Changes datasetChanges)
        {
            if (datasetChanges == null) return;
            Variable.Changes proposedChanges = datasetChanges.GetVariableChanges(ID);
            if (proposedChanges == null)
                return; // nothing to commit
            if (commitStage > CommitStage.Precommitting)
                return; // precommit has already started

            try
            {
                /* Checking internal constraints */
                DoCheckConstraints(datasetChanges);

                commitStage = CommitStage.Precommitting;

                /* Checking external constraints */
                Exception cancelReason;
                if (!FireEventVariableCommitting(proposedChanges, out cancelReason))
                    throw new ConstraintsFailedException("Committing is cancelled", cancelReason);

                /* To avoid duplicate execution of the following code,
                 * because it might be executed in Precommit of dependent DataSet as
                 * a part of the VariableCommitting event handling */
                if (commitStage > CommitStage.Precommitting)
                    return;

                OnPrecommit(proposedChanges);
            }
            catch
            {
                /* If operation failed rolling back to "committed" state (i.e. transaction stopped).
                 * Now it is possible either to Rollback to previous committed state or 
                 * make further changes to fix errors.*/
                commitStage = CommitStage.Committed;
                throw;
            }

            /* Success: this variable is ready for committing */
            commitStage = CommitStage.Precommitted;
        }

        /// <summary>
        /// This method is called after all variables has changed their state successfully and
        /// commits the state of the variable, so it cannot be rolled back any more.
        /// </summary>
        internal void FinalCommit(Changes proposedChanges)
        {
            if (proposedChanges == null)
                return;
            if (commitStage < CommitStage.Precommitted)
                throw new ApplicationException("Cannot perform final commit because the precommit stage is skipped.");

            try
            {
                Metadata.ApplyChanges(proposedChanges.MetadataChanges);
                OnCommit(proposedChanges);
            }
            catch (Exception ex)
            {
                /* Exception during final committing. It is too late to try to roll back changes. */
                Trace.WriteLineIf(DataSet.TraceDataSet.TraceError, "Exception in final commit: " + ex.Message);
                throw new Exception("Exception in final commit", ex);
            }

            changeSetId++;

            Changes appliedChanges = proposedChanges.Clone();
            appliedChanges.MetadataChanges.Commit();
            if (changes == proposedChanges)
                ClearChanges();

            committedSchema = null; // invalidating committed schema 

            commitStage = CommitStage.Committed;
            FireEventVariableCommitted(appliedChanges);
        }

        /// <summary>
        /// Applies given changeset to the variable.
        /// </summary>
        /// <param name="datasetChanges"></param>
        /// <remarks>
        /// The process includes both precommit and final commit stages.
        /// </remarks>
        protected void ApplyChanges(DataSet.Changes datasetChanges)
        {
            Variable.Changes proposedChanges = datasetChanges.GetVariableChanges(ID);
            if (proposedChanges == null) return; // nothing to commit

            Precommit(datasetChanges);
            FinalCommit(proposedChanges);
        }

        /// <summary>
        /// Initiates new change transaction (if it isn't started yet).
        /// </summary>
        /// <remarks>
        /// This concerns changeset accumulated within the <see cref="Microsoft.Research.Science.Data.DataSet"/>.
        /// </remarks>
        protected internal void StartChanges()
        {
            lock (DataSet)
            {
                if (!HasChanges)
                {
                    var initialSchema = GetCommittedSchema(true);
                    int[] shape = new int[Rank];
                    for (int i = 0; i < shape.Length; i++)
                        shape[i] = initialSchema.Dimensions[i].Length;

                    DataChanges changes = new DataChanges(changeSetId + 1,
                        initialSchema,
                        Metadata.StartChanges(),
                        shape,
                        new Rectangle());
                    changes.Data = new List<DataPiece>();
                    this.changes = changes;
                }
            }
        }

        /// <summary>
        /// Gets a copy of the Variable that contains all changes made to it since last committing.
        /// </summary>
        private Variable GetChanges()
        {
            if (!HasChanges)
                return null;

            throw new NotImplementedException("Sorry, not yet...");
        }

        /// <summary>
        /// Gets the inner representation of changes made to the variable.
        /// </summary>
        internal Variable.DataChanges GetInnerChanges()
        {
            // It mustn't be cloned for it's reference is used to
            // determine whether it is local changes or not (during commit).
            return changes;
        }

        /// <summary>
        /// Gets the variable schema.
        /// </summary>
        /// <remarks>
        /// <para>The <see cref="VariableSchema"/> describes the structure of the <see cref="Variable"/>.</para>
        /// <para>The method returns the committed version of the schema. 
        /// Read more about version in remarks for <see cref="Microsoft.Research.Science.Data.DataSet.GetSchema(SchemaVersion)"/>.
        /// To get a custom version of the schema, use <see cref="GetSchema(SchemaVersion)"/>.</para>
        /// </remarks>
        /// <seealso cref="GetSchema(SchemaVersion)"/>
        public VariableSchema GetSchema()
        {
            return GetSchema(SchemaVersion.Committed);
        }

        /// <summary>
        /// Gets specified version of the variable schema.
        /// </summary>
        /// <param name="version">Version of the schema to return.</param>
        /// <remarks>
        /// <para>The <see cref="VariableSchema"/> describes the structure of the <see cref="Variable"/>.</para>
        /// <para>The method returns the specified <paramref name="version"/> of the schema. 
        /// Read more about version in remarks for <see cref="Microsoft.Research.Science.Data.DataSet.GetSchema(SchemaVersion)"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="GetSchema()"/>
        public VariableSchema GetSchema(SchemaVersion version)
        {
            lock (DataSet)
            {
                if (HasChanges)
                {
                    if (version == SchemaVersion.Committed)
                    {
                        return GetCommittedSchema(true);
                    }

                    // Making a schema for proposed version of the variable
                    string name = (string)Metadata[Metadata.KeyForName, version];
                    ReadOnlyDimensionList roDims = GetDimensions(version);
                    MetadataDictionary metadata = Metadata.Clone(version, true);

                    // Schema
                    VariableSchema schema = new VariableSchema(
                        changeSetId,
                        id,
                        TypeOfData,
                        roDims,
                        metadata);
                    return schema;
                }
                else
                {
                    if (version == SchemaVersion.Proposed)
                        throw new DataSetException("Variable is commited and has no changes.");
                    return GetCommittedSchema(true);
                }
            }
        }

        /// <summary>
        /// Builds committed schema for the data set and saves it in the
        /// field committedSchema.
        /// </summary>
        internal void CaptureCommittedSchema()
        {
            committedSchema = BuildCommittedSchema();
        }

        private VariableSchema BuildCommittedSchema()
        {
            int[] shape = ReadShape();
            if (shape == null) shape = new int[Rank]; // initialization: shape is empty
            Dimension[] dimensions = new Dimension[shape.Length];
            for (int i = 0; i < shape.Length; i++)
                dimensions[i] = new Dimension(this.dimensions[i], shape[i]);

            return new VariableSchema(
                changeSetId,
                id,
                TypeOfData,
                new ReadOnlyDimensionList(dimensions),
                Metadata.Clone(SchemaVersion.Committed, true));
        }

        private VariableSchema GetCommittedSchema(bool doRebuild)
        {
            var copy = committedSchema;
            if (copy == null && doRebuild)
                copy = BuildCommittedSchema();
            return copy;
        }

        /// <summary>
        /// Resets the commit stage to "Committed".
        /// It makes it possible to restart committing process for the variable anew.
        /// </summary>
        internal void ResetCommitStage()
        {
            commitStage = CommitStage.Committed;
            Undo();
        }

        #region Nested types

        /// <summary>
        /// Clears all variable changes.
        /// </summary>
        /// <remarks>
        /// The metohod makes <see cref="HasChanges"/> equal to false.
        /// </remarks>
        protected internal void ClearChanges()
        {
            lock (DataSet)
            {
                changes = null;
                commitStage = CommitStage.Committed;
            }
        }

        /// <summary>
        /// Enables provider implementation to update a shape within changes instance.
        /// </summary>
        /// <param name="changes">Existing changes.</param>
        /// <param name="shape">New shape.</param>
        protected static void ChangesUpdateShape(Variable.Changes changes, int[] shape)
        {
            changes.Shape = shape;
        }

        /// <summary>
        /// Enables provider implementation to update a metadata changes within changes instance.
        /// </summary>
        /// <param name="changes">Existing changes.</param>
        /// <param name="metadataChanges">New metadata changes dictionary.</param>
        protected static void ChangesUpdateMetadataChanges(Variable.Changes changes, MetadataDictionary metadataChanges)
        {
            changes.MetadataChanges = metadataChanges;
        }
        /// <summary>
        /// Enables provider implementation to update the affected rectangle within changes instance.
        /// </summary>
        /// <param name="changes">Existing changes.</param>
        /// <param name="affectedRect">New affected rectangle.</param>
        protected static void ChangesUpdateAffectedRect(Variable.Changes changes, Rectangle affectedRect)
        {
            changes.AffectedRectangle = affectedRect;
        }

        /// <summary>
        /// Represents changes in the variable schema.
        /// </summary>
        public class Changes : ICloneable
        {
            /// <summary>
            /// Version of the changeset.
            /// </summary>
            protected int changeSet;
            /// <summary>
            /// Variable id.
            /// </summary>
            protected int id;
            /// <summary>
            /// Changed/added attributes.
            /// </summary>
            protected MetadataDictionary metadataChanges;
            /// <summary>
            /// Proposed shape of the variable.
            /// </summary>
            protected int[] shape;
            /// <summary>
            /// Rectangle affected by data changes.
            /// </summary>
            protected Rectangle affectedRectangle;
            /// <summary>
            /// Committed schema that is being changed.
            /// </summary>
            protected VariableSchema initialSchema;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="version"></param>
            /// <param name="initialSchema"></param>
            /// <param name="metadataChanges"></param>
            /// <param name="shape"></param>
            /// <param name="affectedRect"></param>
            /// 
            public Changes(int version, VariableSchema initialSchema, MetadataDictionary metadataChanges,
                int[] shape, Rectangle affectedRect)
            {
                if (initialSchema == null) throw new ArgumentNullException("initialSchema");

                this.changeSet = version;
                this.initialSchema = initialSchema;
                this.id = initialSchema.ID;
                this.metadataChanges = metadataChanges;
                this.shape = shape;
                this.affectedRectangle = affectedRect;
            }

            /// <summary>
            /// Gets the new version number.
            /// </summary>
            public int ChangeSet
            {
                get
                {
                    return changeSet;
                }
            }
            /// <summary>
            /// Gets the ID of the variable.
            /// </summary>
            public int ID
            {
                get
                {
                    return id;
                }
            }
            /// <summary>
            /// Gets added and updated metadata attributes.
            /// </summary>
            public MetadataDictionary MetadataChanges
            {
                get { return metadataChanges; }
                internal set { metadataChanges = value; }
            }
            /// <summary>
            /// Gets the new shape of the variable.
            /// </summary>
            public int[] Shape
            {
                get
                {
                    return shape;
                }
                internal set
                {
                    shape = value;

                    if (shape.Length != initialSchema.Rank)
                        throw new ArgumentException("Arguments are incorrect");
                }
            }
            /// <summary>
            /// Gets the rectangle in the index space where data is changed.
            /// </summary>
            public Rectangle AffectedRectangle
            {
                get
                {
                    return affectedRectangle;
                }
                internal set
                {
                    affectedRectangle = value;
                }
            }

            /// <summary>
            /// Gets the schema of the previous version.
            /// </summary>
            public VariableSchema InitialSchema
            {
                get
                {
                    return initialSchema;
                }
            }

            /// <summary>
            /// Gets the value describing what is changed in the variable.
            /// </summary>
            public ChangeTypes ChangeType
            {
                get
                {
                    ChangeTypes ch = ChangeTypes.None;
                    if (metadataChanges != null && metadataChanges.Count > 0)
                        ch |= ChangeTypes.MetadataUpdated;
                    if (!affectedRectangle.IsEmpty)
                        ch |= ChangeTypes.DataUpdated;
                    return ch;
                }
            }

            /// <summary>
            /// Builds and returns the read only dimension list using
            /// its fields with dimensions names and proposed shape.
            /// </summary>
            /// <returns></returns>
            internal ReadOnlyDimensionList GetDimensionList()
            {
                Dimension[] dimensions = new Dimension[Shape.Length];
                for (int i = 0; i < Shape.Length; i++)
                {
                    dimensions[i] = new Dimension(initialSchema.Dimensions[i].Name, Shape[i]);
                }

                return new ReadOnlyDimensionList(dimensions);
            }

            /// <summary>Clones all changes.</summary>
            /// <returns>Exact copy of changes.</returns>
            public virtual Changes Clone()
            {
                Changes clone = new Changes(
                    changeSet, initialSchema, metadataChanges, shape, affectedRectangle);
                return clone;
            }

            object ICloneable.Clone()
            {
                return Clone();
            }
        }

        /// <summary>
        /// Represents changes both in schema and data for the variable.
        /// </summary>
        public sealed class DataChanges : Changes
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="version"></param>
            /// <param name="initialSchema"></param>
            /// <param name="metadataChanges"></param>
            /// <param name="shape"></param>
            /// <param name="affectedRect"></param>
            /// 
            public DataChanges(int version, VariableSchema initialSchema, MetadataDictionary metadataChanges,
                int[] shape, Rectangle affectedRect) :
                base(version, initialSchema, metadataChanges, shape, affectedRect)
            {
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="version"></param>
            /// <param name="initialSchema"></param>
            /// <param name="metadataChanges"></param>
            /// <param name="shape"></param>
            /// <param name="affectedRect"></param>
            /// <param name="data"></param>
            /// 
            public DataChanges(int version, VariableSchema initialSchema, MetadataDictionary metadataChanges,
                int[] shape, Rectangle affectedRect, List<DataPiece> data) :
                this(version, initialSchema, metadataChanges, shape, affectedRect)
            {
                dataPieces = data;
            }

            /// <summary>Contains proposed data arrays put with PutData or AppendData methods.</summary>
            private List<DataPiece> dataPieces;

            /// <summary>
            /// Gets the list of proposed data pieces.
            /// </summary>
            /// <remarks>
            /// Contains proposed data arrays put with PutData or AppendData methods.
            /// </remarks>
            public List<DataPiece> Data
            {
                get { return dataPieces; }
                internal set { dataPieces = value; }
            }

            /// <summary>
            /// Gets the value indicating whether the changeset has proposed data or not.
            /// </summary>
            public bool HasData
            {
                get
                {
                    return (dataPieces != null && dataPieces.Count != 0);
                }
            }

            /// <summary>Clones all changes.</summary>
            /// <returns>Exact copy of changes.</returns>
            public override Changes Clone()
            {
                DataChanges clone = new DataChanges(changeSet,
                    initialSchema, metadataChanges, shape, affectedRectangle);
                if (dataPieces != null)
                    clone.dataPieces = new List<DataPiece>(dataPieces);

                return clone;
            }
        }

        /// <summary>
        /// Describes what is actually changed in a variable.
        /// </summary>
        [Flags]
        public enum ChangeTypes
        {
            /// <summary>No changes.</summary>
            None,
            /// <summary>Data of a variable is updated.</summary>
            DataUpdated = 1,
            /// <summary>Metadata of a variable is updated.</summary>
            MetadataUpdated = 2
        }

        /// <summary>
        /// Represents an array that is a subarray of an entire array, arbitrary positioned.
        /// </summary>
        public struct DataPiece
        {
            /// <summary>
            /// Origin is the indices to put the data.
            /// </summary>
            /// <remarks>
            /// Append operations prodices pieces containing all zeros except one element,
            /// which is -1.
            /// </remarks>
            public int[] Origin;

            /// <summary>
            /// Data to put.
            /// </summary>
            public Array Data;

            /// <summary>
            /// Fills the origin with real values if it is a part of append procedure.
            /// </summary>
            /// <param name="origin"></param>
            /// <param name="shape">Real shape of the data.</param>
            /// <exception cref="ArgumentException">Origin is a part of append but is incorrect.</exception>
            /// <returns>Updated origin</returns>
            public static int[] UpdateAppendedOrigin(int[] origin, int[] shape)
            {
                bool zeros = true;
                int k = -1;
                for (int i = 0; zeros && i < origin.Length; i++)
                {
                    if (origin[i] == -1)
                    {
                        k = i;
                        continue;
                    }
                    if (origin[i] != 0)
                        zeros = false;
                }

                if (k >= 0 && !zeros)
                    throw new ArgumentException("DataPiece is a part of append but has incorret origin");

                if (k >= 0)
                {
                    int[] updated = new int[origin.Length];
                    updated[k] = shape[k];
                    return updated;
                }
                return origin;
            }

            /// <summary>
            /// Fills the origin with real values if it is a part of append procedure.
            /// </summary>
            /// <param name="origin"></param>
            /// <param name="shape">Real shape of the data: returns length for given dimension.</param>
            /// <exception cref="ArgumentException">Origin is a part of append but is incorrect.</exception>
            public static int[] UpdateAppendedOrigin(int[] origin, Func<int, int> shape)
            {
                bool zeros = true;
                int k = -1;
                for (int i = 0; zeros && i < origin.Length; i++)
                {
                    if (origin[i] == -1)
                    {
                        k = i;
                        continue;
                    }
                    if (origin[i] != 0)
                        zeros = false;
                }

                if (k >= 0 && !zeros)
                    throw new ArgumentException("DataPiece is a part of append but has incorret origin");

                if (k >= 0)
                {
                    int[] updated = new int[origin.Length];
                    updated[k] = shape(k);
                    return updated;
                }
                return origin;
            }
        }

        private enum CommitStage
        {
            Committed,
            ConstrainsChecked,
            Precommitting,
            Precommitted
        }

        #endregion

        #endregion

        #region From System.Object

        /// <summary>
        /// Represents the <see cref="Variable"/> as a string.
        /// </summary>
        /// <remarks>
        /// <para>
        /// See remarks for the <see cref="Microsoft.Research.Science.Data.DataSet.ToString()"/> method.
        /// </para>
        /// </remarks>
        public override string ToString()
        {
            return ToString("");
        }

        /// <summary>
        /// Produces a string with given prefix.
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        protected string ToString(string prefix)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}[{1}]", prefix, ID);
            if (HasChanges) sb.Append('*');
            sb.AppendFormat(" {0} of type {1}",
                ID == DataSet.GlobalMetadataVariableID ? String.Empty : Name, TypeOfData.Name);
            ReadOnlyDimensionList dims = GetDimensions(SchemaVersion.Committed);
            if (!HasChanges)
                for (int i = 0; i < dims.Count; i++)
                {
                    sb.Append(' ');
                    sb.Append(dims[i]);
                }
            else
                for (int i = 0; i < dims.Count; i++)
                {
                    sb.Append(' ');
                    sb.AppendFormat("({0}:{1})", dimensions[i], changes.Shape[i]);
                }

            return sb.ToString();
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return dataSet.GetHashCode() ^ id;
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Finalizes the instance.
        /// </summary>
        ~Variable()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes the variable.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="Variable"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        /// <remarks>
        /// <para>This method is called by the public Dispose method and the Finalize method. 
        /// Dispose invokes the protected Dispose method with the disposing parameter set to true. 
        /// Finalize invokes Dispose with disposing set to false.
        /// </para>
        /// <para>Notes to Inheritors
        /// </para>
        /// <para>Dispose can be called multiple times by other objects. 
        /// When overriding Dispose, be careful not to reference objects that have been previously disposed of in an earlier call to Dispose.
        /// </para>
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                }
                // Disposing unmanaged resources.
            }
            disposed = true;
        }

        #endregion

        #region Static properties

        /// <summary>
        /// Gets the default indices array that doesn't depend on a variable's rank.
        /// </summary>
        /// <remarks>
        /// The property always returns null. 
        /// This value that can be used as an origin in put/get data operation with scalar variables;
        /// also, if used as an origin in all PutData operations, it means "all zeros" array.
        /// </remarks>
        public static int[] DefaultIndices { get { return null; } }

        /// <summary>
        /// Creates metadata collection via the default constructor.
        /// </summary>
        /// <returns>New instance of the <see cref="MetadataDictionary"/>.</returns>
        /// <remarks><see cref="MetadataDictionary"/> has internal constructor therefore
        /// this method should be used by derived classes to create the metadata dictionary.</remarks>
        protected MetadataDictionary CreateMetadata()
        {
            return new MetadataDictionary();
        }

        #endregion
    }

    /// <summary>    
    /// Represents a single typed array or a scalar with attached metadata.
    /// </summary>
    /// <typeparam name="DataType">Type of data for the variable.</typeparam>
    /// <remarks>
    /// <para>
    /// For type safety and early binding a base class of all variable’s implementations 
    /// is a generic class <see cref="Variable{DataType}"/>, where DataType is a type of an array’s element.  
    /// To provide a uniform work with variables of unknown or different types, 
    /// <see cref="Variable{DataType}"/> class is derived from a non-generic class <see cref="Variable"/>. 
    /// For example, the collection <see cref="Microsoft.Research.Science.Data.DataSet.Variables"/> 
    /// yields objects of the type 
    /// <see cref="Variable"/> which then might be casted to the required generic type.
    /// </para>
    /// <para>
    /// The generic <see cref="Variable{DataType}"/> class adds to the basic <see cref="Variable"/> class
    /// <see cref="Append(DataType)"/> method to append a single value and a
    /// typed indexer <see cref="Variable{DataType}.this[int[]]"></see> obtaining a single value from the data array.
    /// </para>
    /// </remarks>
    /// <seealso cref="Variable"/>
    /// <see cref="Microsoft.Research.Science.Data.DataSet"/>
    public abstract partial class Variable<DataType> : Variable
    {
        #region Private & protected fields
        // Arrays with size equal to variable rank filled with ones and zeros
        int[] ones = null;
        int[] zeros = null;
        #endregion

        #region Public properties

        /// <summary>
        /// Gets the data type for the variable.
        /// </summary>
        public override Type TypeOfData
        {
            get { return typeof(DataType); }
        }

        /// <summary>
        /// Gets or sets single value.
        /// </summary>
        /// <param name="indices">Indices of a postion to get or set value</param>
        /// <returns>Single variable value</returns>
        /// <remarks>No input out optimization is done when getting/setting single values of a variable.
        /// In general, it is more efficient to get values in arrays using GetData overloads.</remarks>
        public DataType this[params int[] indices]
        {
            get
            {
                int rank = Rank;
                if (rank != 0 && indices == null)
                    throw new ArgumentNullException("indices can be null for scalar variables only");
                if (indices != null && indices.Length != rank)
                    throw new ArgumentException("Number of indices must be equal to the rank of the variable, " + Rank.ToString() + " in this case");
                int[] shape = GetShape();
                for (int i = 0; i < shape.Length; i++)
                    if (indices[i] < 0 || indices[i] >= shape[i])
                        throw new ArgumentOutOfRangeException(string.Format("Index {0} must be >=0 and <{1}, but it is {2}", i, shape[i], indices[i]));
                if (rank > 0 && ones == null)
                {
                    ones = new int[rank];
                    for (int i = 0; i < rank; i++) ones[i] = 1;
                }
                if (zeros == null)
                {
                    zeros = new int[rank == 0 ? 1 : rank];
                    for (int i = 0; i < zeros.Length; i++) zeros[i] = 0;
                }
                return (DataType)GetData(indices, ones).GetValue(zeros);
            }
            set
            {
                if (IsReadOnly)
                    throw new NotSupportedException("Variable is read only");
                int rank = Rank;
                if (rank != 0 && indices == null)
                    throw new ArgumentNullException("indices can be null for scalar variables only");
                if (indices != null && indices.Length != rank)
                    throw new ArgumentException("Number of indices must be equal to the rank of the variable, " + Rank.ToString() + " in this case");
                int[] shape = GetShape();
                for (int i = 0; i < shape.Length; i++)
                    if (indices[i] < 0)
                        throw new ArgumentOutOfRangeException(string.Format("Index {0} must be >=0, but it is {2}", i, shape[i], indices[i]));
                PutData(indices, new DataType[] { value });
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of the variable.
        /// </summary>
        protected Variable(DataSet sds, string[] dims) :
            this(sds, dims, true)
        {
        }

        /// <summary>
        /// Initializes an instance of the variable.
        /// </summary>
        protected Variable(DataSet sds, string[] dims, bool assignID) :
            base(sds, dims, assignID)
        {
            Type dataType = typeof(DataType);
            if (!DataSet.IsSupported(dataType))
                throw new InvalidOperationException(String.Format(
                    "Variables of type {0} are not supported in DataSet 1.0", dataType.Name));
        }

        #endregion

        #region Put Data

        /// <summary>
        /// For one dimensional variables appends a single value.
        /// </summary>
        /// <param name="value">A value to append.</param>
        /// <seealso cref="Variable.Append(Array)"/>
        public void Append(DataType value)
        {
            if (this.Rank != 1)
                throw new InvalidOperationException("This method overload is valid for one-dimentional variables only");
            Append(new DataType[] { value });
        }

        #endregion

    }

    #region Events Type Definitions

    /// <summary>
    /// Kind of changes in a variable.
    /// </summary>
    public enum VariableChangeAction
    {
        /// <summary>Data array of a variable is changed.</summary>
        PutData,
        /// <summary>Metadata of a variable is changed.</summary>
        UpdateMetadata,
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    internal delegate void VariableCommittingEventHandler(object sender, VariableCommittingEventArgs e);

    /// <summary>
    /// 
    /// </summary>
    internal class VariableCommittingEventArgs : EventArgs
    {
        private Variable var;

        private Variable.Changes changes;

        private bool cancel;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="var"></param>
        /// <param name="changes"></param>
        public VariableCommittingEventArgs(Variable var, Variable.Changes changes)
        {
            this.var = var;
            this.changes = changes;
            this.cancel = false;
        }

        /// <summary>
        /// Get the proposed changes to be applied to the variable.
        /// </summary>
        public Variable.Changes ProposedChanges
        {
            get { return changes; }
        }

        /// <summary>
        /// Gets the variable that is being committed.
        /// </summary>
        public Variable Variable
        {
            get { return var; }
        }

        /// <summary>
        /// Gets or sets the value which indicates whether to cancel the commit or not.
        /// </summary>
        public bool Cancel
        {
            get { return cancel; }
            set { cancel = value; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    internal delegate void VariableRolledBackEventHandler(object sender, VariableRolledBackEventArgs e);

    /// <summary>
    /// 
    /// </summary>
    internal class VariableRolledBackEventArgs : EventArgs
    {
        private Variable var;

        public VariableRolledBackEventArgs(Variable var)
        {
            this.var = var;
        }


        /// <summary>
        /// Gets the variable that has just been rolled back.
        /// </summary>
        public Variable Variable
        {
            get { return var; }
        }
    }

    internal delegate void VariableCommittedEventHandler(object sender, VariableCommittedEventArgs e);

    internal class VariableCommittedEventArgs : EventArgs
    {
        private Variable var;

        private Variable.Changes changes;

        public VariableCommittedEventArgs(Variable var, Variable.Changes changes)
        {
            this.var = var;
            this.changes = changes;
        }

        /// <summary>
        /// Gets the changes that have just been applied to the variable.
        /// </summary>
        public Variable.Changes CommittedChanges
        {
            get { return changes; }
        }

        /// <summary>
        /// Gets the variable that has just been committed.
        /// </summary>
        public Variable Variable
        {
            get { return var; }
        }
    }

    internal delegate void VariableChangedEventHandler(object sender, VariableChangedEventArgs e);

    internal class VariableChangedEventArgs : EventArgs
    {
        private Variable var;

        private Variable.Changes changes;

        private VariableChangeAction action;

        public VariableChangedEventArgs(Variable var, Variable.Changes changes, VariableChangeAction action)
        {
            this.var = var;
            this.action = action;
            this.changes = changes;
        }

        /// <summary>
        /// Gets the variable that is changed (but not committed yet).
        /// </summary>
        public Variable Variable
        {
            get { return var; }
        }

        /// <summary>
        /// Gets the kind of the change.
        /// </summary>
        public VariableChangeAction Action
        {
            get { return action; }
        }

        /// <summary>
        /// Get the proposed changes to be applied to the variable.
        /// </summary>
        public Variable.Changes Changes
        {
            get { return changes; }
        }
    }

    internal delegate void VariableChangingEventHandler(object sender, VariableChangingEventArgs e);

    internal class VariableChangingEventArgs : EventArgs
    {
        private Variable var;

        private Variable.Changes changes;

        private bool cancel;

        private VariableChangeAction action;

        public VariableChangingEventArgs(Variable var, Variable.Changes proposedChanges, VariableChangeAction action)
        {
            this.var = var;
            this.changes = proposedChanges;
            this.cancel = false;
            this.action = action;
        }

        /// <summary>
        /// Gets the kind of the change.
        /// </summary>
        public VariableChangeAction Action
        {
            get { return action; }
        }

        /// <summary>
        /// Get the proposed changes to be applied to the variable.
        /// </summary>
        public Variable.Changes ProposedChanges
        {
            get { return changes; }
        }

        /// <summary>
        /// Gets the variable that is being committed.
        /// </summary>
        public Variable Variable
        {
            get { return var; }
        }

        /// <summary>
        /// Gets or sets the value which indicates whether to cancel the commit or not.
        /// </summary>
        public bool Cancel
        {
            get { return cancel; }
            set { cancel = value; }
        }
    }

    #endregion

    #region Additional types

    /// <summary>
    /// Represents a stride range which is a set of numbers 
    /// Xn = X0 + s*n, where 0 &lt;= n &lt; N.
    /// </summary>
    internal struct StrideRange
    {
        private int start;
        private int stride;
        private int count;


        /// <summary>
        /// Creates a range with the only value <param name="singleValue"></param>.
        /// </summary>	
        public StrideRange(int singleValue)
        {
            this.start = singleValue;
            this.stride = 1;
            this.count = 1;
        }

        /// <summary>
        /// Start value for the range.
        /// </summary>		
        public int Start { get { return start; } }

        /// <summary>
        /// Stride value for the range.
        /// </summary>
        public int Stride { get { return stride; } }

        /// <summary>
        /// Number of values in the range.
        /// </summary>
        public int Count { get { return count; } }

        /// <summary>
        /// Last value of the range.
        /// </summary>
        public int End { get { return start + (count - 1) * stride; } }

        /// <summary>
        /// Returns the range as a string in the format "start:stride:end".
        /// </summary>
        public override string ToString()
        {
            if (stride == 1)
            {
                if (count == 1)
                    return start.ToString();
                return String.Format("{0}:{1}", start, start + count - 1);
            }
            return String.Format("{0}:{1}:{2}", start, stride, End);
        }
    }


    #endregion
}

