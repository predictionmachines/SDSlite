// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Linq;
using Microsoft.Research.Science.Data.Factory;

namespace Microsoft.Research.Science.Data
{
    /// <summary>
    /// Facilitates creating, reading and modifying of a scientific data set.
    /// </summary>
    /// <remarks> 
    /// <para>The <see cref="DataSet"/> is a top-level class of the Scientific DataSet library that is aimed to
    /// provide a single data model for different scientific data sets. 
    /// Applications are able to store and retrieve data uniformly, having an abstract view on 
    /// various custom data storages. This makes an application less 
    /// dependent on data formats and significantly eases data transfer between software components.</para>
    /// <para>
    /// The <see cref="DataSet"/> bundles several related arrays and associated metadata in 
    /// a single self-descriptive package and enforces certain constraints on arrays' shapes to 
    /// ensure data consistency. An underlying data model of the <see cref="DataSet"/> 
    /// is based on a long-term community experience. 
    /// The SDS Data Model has commonality to the Unidata’s Common Data Model, 
    /// being chosen since CDM has been successfully tested by time. 
    /// </para>
    /// <para>To create or open a data set, use <see cref="DataSet.Open(string)"/> method with a proper 
    /// DataSet URI:
    /// <example>
    /// <code>
    /// DataSet dataSet = DataSet.Open("csv_for_autotypes.csv"); 
    /// DataSet dataSet = DataSet.Open("msds:csv?file=csv_for_autotypes.csv"); 
    /// DataSet dataSet = DataSet.Open(@"c:\data\ncfile.nc"); 
    /// DataSet dataSet = DataSet.Open("msds:nc?file=ncfile.nc"); 
    /// DataSet dataSet = DataSet.Open("msds:as?server=(local)&amp;database=ActiveStorage&amp;integrated security=true&amp;GroupName=mm5&amp;UseNetcdfConventions=true");
    /// </code>
    /// </example>
    /// </para>
    /// <para>
    /// Particular <see cref="DataSet"/> implementation, providing an access to the specified
    /// data storage, is chosen on a basis of the URI. Such implementation
    /// named <see cref="DataSet"/> provider.
    /// </para>
    /// <para>
    /// Current release contains following providers:
    /// <list type="bullet">
    /// <item><description>
    /// <see cref="T:Microsoft.Research.Science.Data.Memory.MemoryDataSet"></see></description></item>
    /// <item><description>
    /// <see cref="T:Microsoft.Research.Science.Data.CSV.CsvDataSet"></see></description></item>
    /// <item><description><see cref="T:Microsoft.Research.Science.Data.NetCDF4.NetCDFDataSet"/></description></item>
    /// </list>
    /// </para>
    /// <para>A provider might have extra parameters specified in the URI. 
    /// For example, it can be security credentials, behavioral properties etc. 
    /// For more information see documentation of a particular provider. 
    /// To customize a URI prior to opening a <see cref="DataSet"/>, use capabilities
    /// exposed by the <see cref="DataSetUri"/> class.
    /// </para>
    /// <para>
    /// The <see cref="DataSet"/> consists of a collection of <see cref="Variable"/> objects.
    /// Each <see cref="Variable"/> represents a single array with a collection of attributes
    /// (see <see cref="Variable.Metadata"/>) attached.
    /// If a provider works with large data sets, typically it doesn't loads all data in memory,
    /// but provides on-demand data access through variables.
    /// <example>
    /// The following code iterates through all variables of a <see cref="DataSet"/> 
    /// loaded from "sample.csv" file and prints information about the <see cref="DataSet"/> to the console:
    /// <code>
    ///using(DataSet ds = new DataSet.Open("sample.csv"))
    ///{
    ///     Console.WriteLine ("Scientfic DataSet " + ds.Name + "(" + ds.DataSetGuid + ")" + " contents: ");
    ///
    ///     Console.WriteLine (" Variables:");
    ///     foreach (Variable v in sds.Variables)
    ///     {
    ///         Console.WriteLine (v.ToString());
    ///     }
    ///}
    /// </code>
    /// </example>
    /// </para>
    /// <para>
    /// Relationships between variables are expressed using a concept of shared dimensions. 
    /// A DataSet dimension is a distinct named index space. 
    /// For example, saying that variable <c>Observation</c> shares the dimension with variable <c>X</c>
    /// we mean that <c>Observation[i]</c> relate somehow to <c>X[i]</c> for all indices i 
    /// from the shared index space. Additionally this introduces a constraint to the <see cref="DataSet"/>: 
    /// the lengths of these two variables must always be the same.</para>
    /// <para>
    /// In common the rule says that lengths of a shared dimension for all variables, depending on it, must be equal.
    /// </para>
    /// <para>In addition to metadata attached to each <see cref="Variable"/>, there is a global
    /// metadata attached to the <see cref="DataSet"/> (see <see cref="DataSet.Metadata"/>).
    /// Metadata is a dictionary of string keys and typed values, 
    /// represented as a <see cref="MetadataDictionary"/> class.
    /// </para>
    /// <para>
    /// The <see cref="DataSet"/> instance can be read-only (see <see cref="IsReadOnly"/>). Such
    /// <see cref="DataSet"/> cannot be modified.
    /// </para>
    /// <para>
    /// New <see cref="Variable"/> can be added to a <see cref="DataSet"/> using 
    /// <see cref="DataSet.AddVariable(string,string[])"/>.
    /// <example>
    /// The following code adds a column to a comma separated text file "Tutorial1.csv":
    /// <code>
    /// using(DataSet ds = DataSet.Open("Tutorial1.csv"))
    /// {
    ///     // read input data
    ///     double[] x = (double[])ds["X"].GetData();
    ///     double[] y = (double[])ds["Observation"].GetData();
    ///     // compute
    ///     var xm = x.Sum() / x.Length;
    ///     var ym = y.Sum() / y.Length;
    ///     var a = x.Zip(y, (xx, yy) => (xx - xm) * (yy - ym)).Sum()
    ///      / x.Select(xx => (xx - xm) * (xx - xm)).Sum();
    ///     var b = ym - a * xm;
    ///     var model = x.Select(xx => a * xx + b).ToArray();
    ///     // Adding new variable
    ///     ds.AddVariable&lt;double&gt;("Model", model);
    /// }
    /// </code>
    /// </example>
    /// </para>
    /// <para>
    /// The extensions <see cref="T:Microsoft.Research.Science.Data.Imperative.DataSetExtensions"/>
    /// enable work with <see cref="DataSet"/> in a way close to a procedural API. The previous example
    /// can be rewritten using the extensions:
    /// <example>
    /// <code>
    /// using(DataSet ds = sds.DataSet.Open("Tutorial1.csv"))
    /// {
    ///     // read input data 
    ///     var x = ds.GetData&lt;double[]&gt;("X");
    ///     var y = ds.GetData&lt;double[]&gt;("Observation");
    ///     // compute
    ///     var xm = x.Sum() / x.Length;
    ///     var ym = y.Sum() / y.Length;
    ///     var a = x.Zip(y, (xx, yy) => (xx - xm) * (yy - ym)).Sum()
    ///       / x.Select(xx => (xx - xm) * (xx - xm)).Sum();
    ///     var b = ym - a * xm;
    ///     var model = x.Select(xx => a * xx + b).ToArray();
    ///     // Adding new variable
    ///     ds.Add&lt;double[]&gt;("Model");
    ///     ds.PutData&lt;double[]&gt;("Model", model);
    /// }
    /// </code>
    /// </example>
    /// </para>
    /// <para>
    /// The <see cref="DataSet"/> uses two-phase transaction mechanism to commit changes. See remarks for the
    /// <see cref="Commit"/> method for details. See also remarks for the
    /// <see cref="IsAutocommitEnabled"/> property. 
    /// </para>    
    /// </remarks>
    /// <seealso cref="DataSet.Open(string)"/>
    /// <seealso cref="DataSetUri"/>
    /// <seealso cref="Variable"/>
    /// <seealso cref="Metadata"/>
    /// <seealso cref="AddVariable(string,string[])"/>
    /// <seealso cref="T:Microsoft.Research.Science.Data.Imperative.DataSetExtensions"/>
    /// <seealso cref="Commit"/>
    /// <seealso cref="IsAutocommitEnabled"/>
    public abstract partial class DataSet : IDisposable, IEnumerable<Variable>
    {
        #region Static

        /// <summary>
        /// Filters trace messages related to the DataSet.
        /// </summary>
        protected internal static TraceSwitch TraceDataSet = new TraceSwitch("DataSet", "Filters trace messages related to the DataSet core classes.", "Error");

        /// <summary>
        /// ID of the variable that contains global metadata of the DataSet.
        /// </summary>
        public const int GlobalMetadataVariableID = 0;

        /// <summary>
        /// Opens connection to a data set for the specified <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">Uri of the <see cref="DataSet"/> to open.</param>
        /// <returns>New instance of the <see cref="DataSet"/>.</returns>
        /// <remarks>
        /// The method is obsolete. Use <see cref="DataSet.Open(string)"/> instead.
        /// </remarks>
        /// <seealso cref="Open(DataSetUri)"/> 
        /// <seealso cref="DataSetUri.Create(string)"/>
        /// <seealso cref="DataSetUri"/>		
        /// <seealso cref="DataSetFactory"/>
        /// <exception cref="KeyNotFoundException">The provider is not registered.</exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        [Obsolete("This method is obsolete and will be removed in the next version. Use the property \"DataSet.Open\" instead.")]
        public static DataSet Create(string uri)
        {
            return DataSetFactory.Create(uri);
        }
        /// <summary>
        /// Opens connection to a <see cref="DataSet"/> for the specified <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">URI of the <see cref="DataSet"/> to open.</param>
        /// <returns>New instance of the <see cref="DataSet"/>.</returns>
        /// <remarks>
        /// <para>
        /// The method uses the <see cref="Microsoft.Research.Science.Data.Factory.DataSetFactory"/> class
        /// that enables creating an instance of a particular <see cref="DataSet"/> provider on a basis of the
        /// <paramref name="uri"/>. The URI is either a file path (with possible parameters appended
        /// through '?') or a URI with schema "msds" and correct provider name and set of parameters. 
        /// See also <see cref="DataSetUri"/> class.
        /// <example>
        /// <para>For example, provider <see cref="T:Microsoft.Research.Science.Data.CSV.CsvDataSet"/> accepts following URIs:</para>
        /// <code>
        /// c:\data\test.csv
        /// c:\data\test.csv?openMode=createNew
        /// msds:csv?file=c:\data\test.csv&amp;openMode=createNew
        /// </code>
        /// <para>The URI "msds:nc?file=c:\data.test.csv" will cause an error for it has wrong provider name ("nc" instead of "csv").</para>
        /// </example>
        /// </para>
        /// <para>
        /// A provider should have the <see cref="DataSetProviderNameAttribute"/> attribute specifying 
        /// a provider name. There might also be declared attributes 
        /// <see cref="DataSetProviderFileExtensionAttribute"/> to 
        /// associate extensions with the provider,
        /// and <see cref="DataSetProviderUriTypeAttribute"/> to associate a concrete <see cref="DataSetUri"/>
        /// type with the provider (see also <see cref="DataSetFactory.CreateUri(string)"/>).
        /// <example>
        /// <code>
        /// [DataSetProviderName("csv")]
        /// [DataSetProviderFileExtension(".csv")] 
        /// [DataSetProviderUriTypeAttribute(typeof(CsvUri))]
        /// public class CsvDataSet : DataSet
        /// </code>
        /// </example>
        /// </para>
        /// <para>
        /// The <see cref="DataSet"/> class has property <see cref="URI"/> returning a URI of a <see cref="DataSet"/> instance. 
        /// </para>
        /// <para>For developers: there is a class <see cref="DataSetUri"/>
        /// implementing basic functionality on parsing and 
        /// verifying DataSet URIs and it should be used to embed the support of URI to 
        /// DataSet provider.</para>
        /// <para>
        /// See also remarks for <see cref="Microsoft.Research.Science.Data.Factory.DataSetFactory.Create(string)"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="Open(DataSetUri)"/> 
        /// <seealso cref="Open(string,ResourceOpenMode)"/>
        /// <seealso cref="DataSetUri.Create(string)"/>
        /// <seealso cref="DataSetUri"/>		
        /// <seealso cref="DataSetFactory"/>
        /// <exception cref="KeyNotFoundException">The provider is not registered.</exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static DataSet Open(string uri)
        {
            return DataSetFactory.Create(uri);
        }

        /// <summary>
        /// Opens connection to a <see cref="DataSet"/> for the specified <paramref name="uri"/> with given open mode.
        /// </summary>
        /// <param name="uri">URI of the <see cref="DataSet"/> to open.</param>
        /// <param name="openMode">The mode to open dataset.</param>
        /// <returns>New instance of the <see cref="DataSet"/>.</returns>
        /// <seealso cref="Open(string)"/> 
        public static DataSet Open(string uri, ResourceOpenMode openMode)
        {
            var dsUri = DataSetUri.Create(uri);
            dsUri.OpenMode = openMode;
            return DataSet.Open(dsUri);
        }

        /// <summary>
        /// Opens a shared connection to a DataSet for a given <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">A file path or a URI string that points to a DataSet storage.</param>
        /// <returns>An instance of a shared <see cref="DataSet"/>.</returns>
        /// <remarks>
		/// In this SDSlite implementation the method is equivalent to <see cref="DataSet.Open(string)"/>
        /// </remarks>
        /// <seealso cref="OpenSharedCopy(string)"/>
        public static DataSet OpenShared(string uri)
        {
            return Open(uri);
        }

        /// <summary>
        /// Shares a copy of the dataset from a given <paramref name="uri"/>.
        /// </summary>
		/// In this SDSlite implementation the method returns a memory copy of existing dataset.
        /// </remarks>
        /// <seealso cref="Open(string)"/>
        public static DataSet OpenSharedCopy(string uri)
        {
			using (var frm = DataSet.Open (uri)) {
				var to = DataSet.Open ("msds:memory");
				CopyAll (frm, to);
				return to;
			}
        }


        /// <summary>
        /// Opens connection to a <see cref="DataSet"/> for the specified <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">URI of the <see cref="DataSet"/> to open.</param>
        /// <returns>New instance of the <see cref="DataSet"/>.</returns>        
        /// <remarks>
        /// The method is obsolete. Use <see cref="DataSet.Open(DataSetUri)"/> instead.
        /// </remarks>
        /// <seealso cref="Open(string)"/> 
        /// <seealso cref="DataSetUri.Create(string)"/>
        [Obsolete("This method is obsolete and will be removed in the next version. Use the property \"DataSet.Open\" instead.")]
        public static DataSet Create(DataSetUri uri)
        {
            return DataSetFactory.Create(uri);
        }

        /// <summary>
        /// Opens connection to a data set for the specified <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">URI of the <see cref="DataSet"/> to open.</param>
        /// <returns>New instance of the <see cref="DataSet"/>.</returns>        
        /// <remarks>
        /// <para>
        /// The method creates a <see cref="DataSet"/> instance for the specified <paramref name="uri"/>.
        /// See remarks for <see cref="Open(string)"/> and <see cref="DataSetUri.Create(string)"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="Open(string)"/> 
        /// <seealso cref="DataSetUri.Create(string)"/>
        /// <exception cref="KeyNotFoundException">The provider is not registered.</exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static DataSet Open(DataSetUri uri)
        {
            return DataSetFactory.Create(uri);
        }

        /// <summary>
        /// Creates <see cref="MetadataDictionary"/> instance.
        /// </summary>
        /// <returns>New instance of the <see cref="MetadataDictionary"/>.</returns>
        /// <remarks>
        /// <para>
        /// Constructor of the <see cref="MetadataDictionary"/> is internal.
        /// Therefore providers should use this method to create an instance of the 
        /// <see cref="MetadataDictionary"/>.
        /// </para>
        /// </remarks>
        protected static MetadataDictionary CreateMetadata()
        {
            return new MetadataDictionary();
        }

        /// <summary>
        /// Starts changes for the <see cref="MetadataDictionary"/>.
        /// </summary>
        /// <returns>Instance containing changes of the <see cref="MetadataDictionary"/>.</returns>
        /// <remarks>
        /// <para>
        /// The StartChanges methods of the <see cref="MetadataDictionary"/> is internal.
        /// Therefore providers should use <see cref="StartChangesFor"/> to start changes for the
        /// <see cref="MetadataDictionary"/>.
        /// </para>
        /// </remarks>
        protected static MetadataChanges StartChangesFor(MetadataDictionary metadata)
        {
            return metadata.StartChanges();
        }

        /// <summary>
        /// Creates <see cref="MultipleDataResponse"/> instance.
        /// </summary>
        /// <returns>New instance of the <see cref="MultipleDataResponse"/>.</returns>
        /// <remarks>
        /// <para>
        /// Constructor of the <see cref="MultipleDataResponse"/> is internal.
        /// Therefore providers should use this method to create an instance of the 
        /// <see cref="MultipleDataResponse"/>.
        /// </para>
        /// </remarks>
        protected static MultipleDataResponse CreateMultipleDataResponse(int version, DataResponse[] responses)
        {
            return new MultipleDataResponse(version, responses);
        }

        /// <summary>
        /// Creates <see cref="DataResponse"/> instance.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="request"></param>
        /// <returns>New instance of the <see cref="DataResponse"/>.</returns>
        /// <remarks>
        /// <para>
        /// Constructor of the <see cref="DataResponse"/> is internal.
        /// Therefore providers should use this method to create an instance of the 
        /// <see cref="DataResponse"/>.
        /// </para>
        /// </remarks>
        protected static DataResponse CreateDataResponse(DataRequest request, Array data)
        {
            return new DataResponse(request, data);
        }

        /// <summary>
        /// Gets the <see cref="T:System.Type"/> with the specified name, performing a case-sensitive search.
        /// </summary>
        /// <param name="typeName">The assembly-qualified name of the type to get. If the type is in the core <see cref="DataSet"/> assembly or in Mscorlib.dll, it is sufficient to supply the type name qualified by its namespace.</param>
        /// <returns>The <see cref="T:System.Type"/> with the specified name, if found; otherwise, a null reference.</returns>
        /// <remarks>This method is equivalent to <see cref="M:System.Type.GetType(string)"/> except when assembly is not specified in <paramref name="typeName"/>. In that case it searches the type in the <see cref="DataSet"/> assembly, not in the calling assembly.</remarks>
        public static Type GetType(string typeName)
        {
            return Type.GetType(typeName);
        }

        #endregion

        #region Private & protected members

        private readonly bool supportsCoordinateSystems = true;

        private readonly Guid guid = Guid.NewGuid();

        /// <summary>Unique number of committed changeset (version). 
        /// Changeset ids can be compared. Greater changeset id
        /// means more recent changeset</summary>
        private int changeSetId; // initial value is 0;

        /// <summary>If autocommit is true, tries to commit after every change.</summary>
        private bool autocommit = true;

        /// <summary>
        /// URI of the DataSet.
        /// </summary>
        protected DataSetUri uri;

        private bool isDisposed; // initially false

        private bool rollingBack; // initially false

        /// <summary>
        /// Determines whether data set is read only.
        /// </summary>
        protected bool readOnly; // false by default

        /// <summary>Committed collection of variables.</summary>
        protected ReadOnlyVariableCollection variables;

        private int maxVarId = 1;

        private ReadOnlyCoordinateSystemCollection csystems;

        private bool dimInferring = true;

        /// <summary>Represents recent changes in data set.</summary>
        private Changes changes;

        private DataSetSchema committedSchema;

        /// <summary>List of current committed dimensions.</summary>
        private ReadOnlyDimensionList commitedDimensions;

        /// <summary>Other data sets that were referenced in uri.</summary>
        private List<DataSet> includedDataSets = new List<DataSet>();

        /// <summary>List of data sets referring this data set.</summary>
        private DataSetLinkCollection incomingReferences;
        /// <summary>List of data sets referred by this data set.</summary>
        private DataSetLinkCollection outcomingReferences;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an instance of the <see cref="DataSet"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Initializes the instance's collections of variables and coordinate systems
        /// and starts changes (see <see cref="StartChanges()"></see>).
        /// </para>
        /// <para>
        /// Initializes the <see cref="DataSet"/> instance with coordinate systems support enabled
        /// and generic global metadata variable. See also <see cref="DataSet(bool,bool)"/>.
        /// </para>
        /// </remarks>
        protected DataSet()
            : this(true, true)
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="DataSet"/>.
        /// </summary>
        /// <param name="createGlobalMetadataVariable">Determines whether to create a generic global metadata variable
        /// or not.</param>
        /// <param name="supportsCoordinateSystems">If true, the <see cref="DataSet"/> provider supports coordinate systems; otherwise, it doesn't.</param>
        /// <remarks>
        /// <para>
        /// Initializes the instance's collections of variables and coordinate systems
        /// and starts changes (see <see cref="StartChanges()"></see>).
        /// </para>
        /// <para>
        /// If <paramref name="createGlobalMetadataVariable"/> is <c>true</c>, 
        /// an isntance of the <see cref="MetadataContainerVariable"/> with <see cref="Variable.ID"/>
        /// equal to <see cref="GlobalMetadataVariableID"/> is created to store global metadata of the <see cref="DataSet"/>.
        /// Otherwise, the provider must add (<see cref="AddVariableToCollection(Variable)"/>) a custom variable with 
        /// <see cref="Variable.ID"/> == <see cref="GlobalMetadataVariableID"/>
        /// on its own. Read about global metadata in remarks for <see cref="Metadata"/> property.
        /// </para>
        /// </remarks>
        protected DataSet(bool createGlobalMetadataVariable, bool supportsCoordinateSystems)
        {
            bool _autocommit = this.autocommit;
            this.autocommit = false;
            this.supportsCoordinateSystems = supportsCoordinateSystems;

            variables = new ReadOnlyVariableCollection();
            csystems = new ReadOnlyCoordinateSystemCollection();

            StartChanges();

            if (createGlobalMetadataVariable)
            {
                int currentID = maxVarId;
                this.maxVarId = 0; // to be assigned to the global metadata variable
                Variable globalMetadata = new MetadataContainerVariable(this);
                AddVariableToCollection(globalMetadata);
                this.maxVarId = currentID;
            }
            autocommit = _autocommit;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the metadata attached to the <see cref="DataSet"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The property gets the global <see cref="MetadataDictionary"/> attached to the DataSet.
        /// </para>
        /// <example>
        /// The following example creates a <see cref="DataSet"/> and fills its global metadata with different
        /// types of values;
        /// then it creates a <see cref="Variable"/>, puts data and sets metadata of the variable.
        /// <code>
        /// string[] strings = new string[] { "a", "b", "c" };
        ///	DateTime[] dates = new DateTime[] { DateTime.MinValue, DateTime.Today };
        ///
        ///	using (DataSet ds = DataSet.Open("sample.csv?openMode=create"))
        ///	{
        ///		ds.IsAutocommitEnabled = false;
        ///
        ///		// Updating global metadata of the data set:
        ///		ds.Metadata["custom"] = "Custom Metadata";
        ///		ds.Metadata["strings"] = strings;
        ///		ds.Metadata["dates"] = dates;
        ///		ds.Metadata["empty"] = new int[0];
        ///
        ///		// Adding a variable
        ///		var var = ds.AddVariable&lt;string&gt;("var", "x");
        ///		// Updating local metadata of the variable
        ///		var.Metadata["strings"] = strings;
        ///		var.PutData(strings);
        ///
        ///		// Committing all changes.
        ///		ds.Commit();
        ///	}
        /// </code>
        /// </example>
        /// <para>
        /// To developers of a custom <see cref="DataSet"/> provider:
        /// internally the global metadata dictionary is stored as a zero-rank variable of type 
        /// <see cref="EmptyValueType"/> with <see cref="Variable.ID"/> equal to 
        /// <see cref="DataSet.GlobalMetadataVariableID"/>.
        /// Creating of such variable can be delegated to the <see cref="DataSet"/> constructor
        /// or performed by the custom provider itself 
        /// (see constructor <see cref="DataSet(bool,bool)"/>).
        /// The <see cref="Metadata"/> property just refers the dictionary of that variable.
        /// Also the variable is hidden and <see cref="ReadOnlyVariableCollection"/>
        /// doesn't include it in a common enumeration list and <see cref="ReadOnlyVariableCollection.Count"/>
        /// value. To enumerate through all variables, use <see cref="ReadOnlyVariableCollection.All"/> property.
        /// Nevertheless, the variable is presented in the <see cref="DataSetSchema"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="Variable.Metadata"/>
        public MetadataDictionary Metadata
        {
            get
            {
                Variable globalMetadataVar;
                if (!Variables.TryGetByID(DataSet.GlobalMetadataVariableID, out globalMetadataVar))
                    throw new NotSupportedException("Variable with global metadata is not presented in the DataSet");

                return globalMetadataVar.Metadata;
            }
        }

        /// <summary>
        /// Gets or sets the value indicating whether the autocommit is enabled.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The property is initialized with <c>true</c> at the <see cref="DataSet"/> construction.
        /// Please consider following remarks and think about turning it off.
        /// </para>
        /// <para>
        /// The <see cref="DataSet"/> uses two-phase transaction mechanism to commit accumulated changes.
        /// It is described in remarks for the <see cref="Commit"/> method.
        /// But the <see cref="DataSet"/> enables an option named "autocommitting" to simplify
        /// a code working with a <see cref="DataSet"/>. 
        /// If the <see cref="IsAutocommitEnabled"/> property is <c>true</c>,
        /// an instance of the <see cref="DataSet"/> provider tries to commit itself on every
        /// change. Therefore, if the proposed schema satisfies all constraints, the <see cref="DataSet"/> becomes
        /// committed; otherwise, it continues to be modifed until a change satisfies constraints.
        /// </para>
        /// <para>
        /// The use of the autocomitting option is convenient but less effecient comparing to
        /// manual committing. The reason is that manual commit allows to save a number of accumulated changes
        /// as one and once; autocommitting may lead to series of saving, each of them is usually
        /// an expensive operation. 
        /// This can significantly decrease performance, depending on a particular provider type.
        /// Also autocomitting can prevent from using <see cref="Rollback"/> in some cases
        /// as it makes it possible to roll back only changes accumulated since last erroneous change,
        /// i.e. change that broke constraints (see example below for demonstration).
        /// Therefore, autocomitting should be turned off for complex tasks.
        /// </para>
        /// <para>
        /// When using autocomitting, there is a way to find out whether the <see cref="DataSet"/> is committed or not
        /// after any change: it is the <see cref="HasChanges"/> property that should be checked 
        /// (see the example below). If the <see cref="DataSet"/> is not committed, but expected to be,
        /// it is still possible to get the problem description by an explicit call of the
        /// <see cref="Commit"/> method and catching an exception.
        /// </para>
        /// <example>
        /// <code>
        ///	int[] a1 = new int[] { 1, 2, 3 };
        ///	int[] a2 = new int[] { 4, 5, 6 };
        ///	string[] a3 = new string[] { "1", "2", "3" };
        ///
        ///	using (DataSet ds = DataSet.Open("sample.csv?openMode=createNew"))
        ///	{
        ///		ds.IsAutocommitEnabled = true;
        ///
        ///		var v1 = ds.AddVariable&lt;int&gt;("v1", "1");				
        ///		Assert.IsFalse(ds.HasChanges);	// ds is committed		
        ///		// Note: each commit leads to saving all changes into the underlying storage!
        ///
        ///		var v2 = ds.AddVariable&lt;int&gt;("v2", "1");
        ///		Assert.IsFalse(ds.HasChanges);	// ds is committed 
        ///
        ///		var v3 = ds.AddVariable&lt;string&gt;("v3", a3, "1");
        ///		Assert.IsTrue(ds.HasChanges);	// ds isn't committed since v1,v2,v3 depend on "1"
        ///										// but shape of v1 and v2 is 0, and shape of v3 is 3.
        ///
        ///		v1.PutData(a1);					// ds still is not committed: shape of v2 is still 0,
        ///		Assert.IsTrue(ds.HasChanges);   // all changes are accumulated in a proposed schema.
        ///		v2.PutData(a2);					// ds is committed: v1,v2 and v3 have same shape
        ///		Assert.IsFalse(ds.HasChanges);
        ///
        ///		v1.Metadata["Units"] = "Days";  // ds is committed (metadata of v1 updated)
        ///		Assert.IsFalse(ds.HasChanges);
        ///
        ///		var cs = ds.CreateCoordinateSystem("cs", v3);
        ///		Assert.IsFalse(ds.HasChanges);  // ds is committed: cs with the single axis v3 is added
        ///
        ///		v1.AddCoordinateSystem(cs);		// ds is committed: 
        ///		Assert.IsFalse(ds.HasChanges);  // cs added to v1
        ///
        ///		var v4 = ds.AddVariable&lt;string&gt;("v4", "2");
        ///		Assert.IsFalse(ds.HasChanges);  // ds is committed: new variable v4 depending on "2"
        ///
        ///		var cs2 = ds.CreateCoordinateSystem("cs2", v4);
        ///		Assert.IsFalse(ds.HasChanges);	// ds is committed: new cs with an axis v4.
        ///		v1.AddCoordinateSystem(cs2);	// ds isn't committed: 
        ///		Assert.IsTrue(ds.HasChanges);	//	v1 and c2 depend on different dimensions
        ///
        ///		// Note: now we already cannot rollback adding of "cs2"!
        ///		ds.Rollback();	// rolling back only adding of cs2 into v4
        ///		Assert.IsFalse(ds.HasChanges);	// ds has no changes.		
        ///	}
        /// </code>
        /// </example>
        /// </remarks>
        /// <seealso cref="Commit"/>
        public bool IsAutocommitEnabled
        {
            get { return autocommit; }
            set { autocommit = value; }
        }

        /// <summary>
        /// Gets or sets the value indicating whether the autocommit is enabled.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The property is obsolete. Use the propery <see cref="IsAutocommitEnabled"/> instead.
        /// </para>
        /// </remarks>
        [Obsolete("This property is obsolete and will be removed in the next version. Use the property \"IsAutocommitEnabled\" instead.")]
        public bool AutocommitEnabled
        {
            get { return IsAutocommitEnabled; }
            set { IsAutocommitEnabled = value; }
        }

        /// <summary>
        /// Gets or sets the name of the <see cref="DataSet"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property refers the <see cref="DataSet"/> <see cref="Metadata"/> entry named 
        /// <see cref="MetadataDictionary.KeyForName"/>.
        /// </para>
        /// </remarks>
        public string Name
        {
            get
            {
                if (!Metadata.ContainsKey(Metadata.KeyForName, SchemaVersion.Recent))
                    return "";
                return Metadata.GetComittedOtherwiseProposedValue<string>(Metadata.KeyForName);
            }
            set
            {
                Metadata[Metadata.KeyForName] = value;
            }
        }

        /// <summary>
        /// Gets the globally unique identifier of the <see cref="DataSet"/> instance.
        /// </summary>
        public Guid DataSetGuid
        {
            get { return guid; }
        }

        /// <summary>
        /// Gets the current version of the <see cref="DataSet"/>.
        /// </summary>
        /// <remarks>
        /// After a <see cref="DataSet"/> instance is created, its <see cref="Version"/> is greater than zero.
        /// On every successful commit the <see cref="Version"/> number increases by 1.
        /// </remarks>
        public int Version
        {
            get { lock (this) { return changeSetId; } }
            protected set { changeSetId = value; }
        }

        /// <summary>
        /// Gets the value indicating whether the <see cref="DataSet"/> is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return readOnly; }
        }

        /// <summary>
        /// Gets the value indicating whether the <see cref="DataSet"/> is read-only.
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

        /// <summary>
        /// Gets the URI identifying the <see cref="DataSet"/>.
        /// </summary>
        /// <remarks>
        /// More about DataSet URIs read in remarks for the <see cref="DataSetUri"/> class.
        /// </remarks>
        /// <seealso cref="DataSetUri"/>
        /// <seealso cref="DataSetFactory"/>
        public string URI
        {
            get
            {
                if (uri == null) return "";
                return uri.ToString();
            }
        }

        /// <summary>
        /// Gets the URI identifying the <see cref="DataSet"/>.
        /// </summary>
        /// <remarks>The property is obsolete. Use <see cref="URI"/> instead.</remarks>
        /// <seealso cref="DataSetUri"/>
        /// <seealso cref="DataSetFactory"/>
        [Obsolete("This property is obsolete and will be removed in the next version. Use the property \"URI\" instead.")]
        public string ConstructionString
        {
            get { return URI; }
        }

        /// <summary>
        /// Gets the value indicating whether the <see cref="DataSet"/> has been disposed or not.
        /// </summary>
        /// <remarks>
        /// Disposed <see cref="DataSet"/> throws <see cref="ObjectDisposedException"/> on every 
        /// invoke of its API.
        /// </remarks>
        /// <seealso cref="Dispose()"/>
        public bool IsDisposed
        {
            get
            {
                return isDisposed;
            }
        }

        /// <summary>
        /// Gets the variable with the specified index.
        /// </summary>
        /// <param name="index">Zero-based index of the variable to get.</param>
        /// <returns>The variable with specified index.</returns>
        /// <remarks>This is a short form of the <c>DataSet.Variables[int index]</c> property.</remarks>
        /// <seealso cref="Variables"/>
        /// <seealso cref="ReadOnlyVariableCollection"/>
        public Variable this[int index]
        {
            get { return Variables[index]; }
        }

        /// <summary>
        /// Gets the first variable with the specified name.
        /// </summary>
        /// <param name="variableName">The name of the variable to get.</param>
        /// <returns>The first variable with the specified name.</returns>
        /// <remarks>This is a short form of the <c>Variables[string name]</c> property.</remarks>
        /// <seealso cref="Variables"/>
        /// <seealso cref="ReadOnlyVariableCollection.this[string]"/>
        public Variable this[string variableName]
        {
            get { return Variables[variableName]; }
        }

        /// <summary>
        /// Gets the value indicating whether the <see cref="DataSet"/> has incoming or outcoming references
        /// to variables from another <see cref="DataSet"/>.
        /// </summary>
        /// <seealso cref="GetLinkedDataSets()"/>
        /// <seealso cref="AddVariableByReference{DataType}(Variable{DataType},string[])"/>
        public bool IsLinked
        {
            get
            {
                return incomingReferences != null && incomingReferences.Count > 0 ||
                        outcomingReferences != null && outcomingReferences.Count > 0;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Invoked when something has changed within the <see cref="DataSet"/>.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="changes"></param>
        /// <param name="target"></param>
        private void OnDataSetChanged(DataSetChangeAction action, Changes changes, object target)
        {
            if (autocommit)
            {
                TryCommit();
            }
        }

        /// <summary>
        /// Occurs when the <see cref="DataSet"/> is rolled back.
        /// </summary>
        private event DataSetRolledBackEventHandler RolledBack;

        private void FireEventRolledback()
        {
            if (RolledBack != null)
                RolledBack(this, new DataSetRolledBackEventArgs(this));
        }

        /// <summary>
        /// Occurs on every change of the <see cref="DataSet"/>.
        /// </summary>
        internal event DataSetChangedEventHandler Changed;

        /// <summary>
        /// Fires the Changed event.
        /// </summary>
        protected void FireEventChanged(DataSetChangeAction action, Changes changes, object target)
        {
            if (Changed != null)
                Changed(this, new DataSetChangedEventArgs(this, action, target, new DataSetChangeset(this, changes)));

            OnDataSetChanged(action, changes, target);
        }

        /// <summary>
        /// Occurs when the <see cref="DataSet"/> is committing.
        /// </summary>
        internal event DataSetCommittingEventHandler Committing;

        /// <summary>
        /// Returns false if it is required cancel the committing.
        /// </summary>
        private bool FireEventCommitting(Changes changes)
        {
            try
            {
                DataSetCommittingEventArgs e = new DataSetCommittingEventArgs(this, changes);
                if (Committing != null)
                    Committing(this, e);

                return !e.Cancel;
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(DataSet.TraceDataSet.TraceWarning, "OnDataSetCommitting: " + ex.Message + "\n" + ex.StackTrace);
                return false;
            }
        }


        /// <summary>
        /// Private event Committed handlers.
        /// </summary>
        private DataSetCommittedEventHandler committedHandlers;

        /// <summary>
        /// Occurs when the <see cref="DataSet"/> has been committed successully.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The event is fired within the <see cref="Commit()"/> or <see cref="TryCommit()"/>
        /// methods, if the commit succeeded. Therefore, the <see cref="Commit()"/> returns 
        /// only after handlers of the event finish.
        /// </para>
        /// <para>
        /// Please note, if within a handler of the <see cref="Committed"/> 
        /// the <see cref="DataSet"/> is modified and the <see cref="Commit"/> invoked once again,
        /// the endless recursion can happen.
        /// </para>
        /// </remarks>
        public event DataSetCommittedEventHandler Committed
        {
            add
            {
                committedHandlers += value;
            }
            remove
            {
                committedHandlers -= value;
            }
        }

        /// <summary>
        /// Fires the <see cref="Committed"/> event.
        /// </summary>
        /// <param name="changes">Committed changes.</param>
        /// <param name="committedSchema">Committed schema.</param>
        protected virtual void FireEventCommitted(DataSetChangeset changes, DataSetSchema committedSchema)
        {
            try
            {
                var handlers = committedHandlers;
                if (handlers != null)
                    handlers(this, new DataSetCommittedEventArgs(this, changes, committedSchema));
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(DataSet.TraceDataSet.TraceWarning, "Exception in event DataSet.Committed: " + ex.Message + "\n" + ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Occurs when the <see cref="DataSet"/> is changing.
        /// </summary>
        internal event DataSetChangingEventHandler Changing;

        /// <summary>
        /// Fires the Changing event and
        /// returns false, if changing is canceled.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="target"></param>
        private bool FireEventChanging(DataSetChangeAction action, object target)
        {
            try
            {
                DataSetChangingEventArgs e = new DataSetChangingEventArgs(this, action, target);
                if (Changing != null)
                    Changing(this, e);

                return !e.Cancel;
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(DataSet.TraceDataSet.TraceWarning, "DataSet.Changing: " + ex.Message);
                return false;
            }
        }

        #endregion

        #region Variables

        /// <summary>
        /// Gets the read-only collection of <see cref="Variable"/> objects.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The property returns the read-only collection of variables, 
        /// including both committed and proposed variables
        /// at the moment of the property access. In other words, if a variable is added
        /// to the <see cref="DataSet"/> right after the collection is returned by this property,
        /// new variable is not contained in the collection.
        /// </para>
        /// <para>
        /// The returned collection <see cref="ReadOnlyVariableCollection"/> provides special logic to 
        /// work separately with different schema versions. By default, without an explicit specification,
        /// the collection provides an access to the committed version only.
        /// </para>
        /// <para>
        /// The following example iterates through all variables of a <see cref="DataSet"/> 
        /// being loaded from sample.csv file and prints information about the <see cref="DataSet"/> to the console:
        /// </para>
        /// <example>
        /// <code>
        /// using(DataSet sds = DataSet.Open("sample.csv"))
        /// {
        ///     Console.WriteLine ("ScientficDataSet " + sds.Name + (HasChanges ? "*" : "") + " contents: ");
        /// 
        ///     Console.WriteLine (" Variables:");
        ///     foreach (Variable v in sds.Variables)
        ///     {
        ///		    Console.WriteLine (v.ToString());
        ///     }
        /// }
        /// </code>
        /// </example>
        /// </remarks>
        /// <seealso cref="Variable"/>
        /// <seealso cref="ReadOnlyVariableCollection"/>
        /// <seealso cref="CoordinateSystems"/>
        /// <seealso cref="DataSet.Commit"/>		
        public ReadOnlyVariableCollection Variables
        {
            get
            {
                lock (this)
                {
                    if (HasChanges && changes.Variables != null)
                        return changes.Variables.GetReadOnlyCollection();
                }

                return variables;
            }
        }

        /// <summary>
        /// Gets next id for variable. Used by Variable ctor.
        /// </summary>
        internal int GetNextVariableId()
        {
            return maxVarId++;
        }

        /// <summary>
        /// Proposes the <see cref="Variable.ID"/> that is to be given to the next added 
        /// <see cref="Variable"/>.
        /// </summary>
        /// <param name="nextVarID">Proposed ID for the next <see cref="Variable"/></param>
        /// <remarks>If the <paramref name="nextVarID"/> is less then current, the current ID remains unchanged.</remarks>
        protected void ProposeNextVarID(int nextVarID)
        {
            maxVarId = Math.Max(maxVarId, nextVarID);
        }

        private void OnVariableCollectionChanged(object sender, VariableCollectionChangedEventArgs e)
        {
            if (e.Action == VariableCollectionChangedAction.Add)
            {
                lock (this)
                {
                    Variable v = e.AddedVariable;
                    v.Changing += OnVariableChanging;
                    v.Changed += OnVariableChanged;
                    v.CoordinateSystemAdded += OnVariableCoordinateSystemAdded;
                }
            }
        }

        private void OnVariableCoordinateSystemAdded(object sender, CoordinateSystemAddedEventArgs e)
        {
            Variable var = (Variable)sender;
            if (var.HasChanges)
            {
                StartChanges();
                FireEventChanged(DataSetChangeAction.UpdateOfVariable, changes, var);
            }
        }

        private void OnVariableChanging(object sender, VariableChangingEventArgs e)
        {
            e.Cancel = !FireEventChanging(DataSetChangeAction.UpdateOfVariable, e.Variable);
        }

        private void OnVariableChanged(object sender, VariableChangedEventArgs e)
        {
            if (e.Variable.HasChanges)
            {
                StartChanges();

                FireEventChanged(DataSetChangeAction.UpdateOfVariable, changes, e.Variable);
            }
        }

        /// <summary>
        /// Removes all variables from the collection.
        /// Starts new change transaction.
        /// </summary>
        protected void ClearVariableCollection()
        {
            StartChanges();
            changes.Variables.Clear();
        }

        /// <summary>
        /// Removes all cs from the collection.
        /// Starts new change transaction.
        /// </summary>
        /// <remarks>
        /// For example, can be used by the provider on loading from file
        /// to clear existing collection.
        /// </remarks>
        protected void ClearCSCollection()
        {
            lock (this)
            {
                StartChanges();
                changes.CoordinateSystems.Clear();
            }
        }

        #region Adding variable

        /// <summary>Checks whether one variable is reference to another</summary>
        /// <param name="v1">Referencing variable</param>
        /// <param name="v2">Referenced variable</param>
        /// <returns>true is v1 references v2 or false otherwise</returns>
        private static bool IsReference(Variable v1, Variable v2)
        {
            IRefVariable r = v1 as IRefVariable;
            return (r != null && r.ReferencedVariable == v2);
        }

        /// <summary>Includes variables from DataSet</summary>
        /// <param name="uri">DataSet uri</param>
        /// <remarks>All variables are added if no fragment part present in uri</remarks>
        internal void IncludeDataSet(string uri)
        {
            DataSet included = DataSet.Open(uri);
            int includeID = includedDataSets.Count; // Unique ID of this include
            string suffix = String.Format("_i{0}", includeID); // Unique suffix to be added to dimensions
            includedDataSets.Add(included);

            VariableReferenceName[] refs = null;
            if (uri.StartsWith(DataSetUri.DataSetUriScheme + ":"))
            {
                DataSetUri dsu = new DataSetUri(uri);
                refs = dsu.VariableReferences;
            }
            bool ae = IsAutocommitEnabled;
            try
            {
                IsAutocommitEnabled = false;
                if (refs == null || refs.Length == 0)
                {
                    foreach (Variable v in included.Variables)
                        if (!Variables.Any(v2 => IsReference(v2, v)))
                            AddVariableByReference(v, v.Dimensions.Select(d => d.Name + suffix).ToArray());
                }
                else
                {
                    foreach (VariableReferenceName r in refs)
                    {
                        if (!included.Variables.Contains(r.Name))
                            throw new KeyNotFoundException("Cannot find variable " + r.Name + " referenced in include");

                        Variable v = included[r.Name];
                        if (!Variables.Any(v2 => v2.Name == r.Alias && IsReference(v2, v)))
                        {
                            Variable refVar = AddVariableByReference(v, v.Dimensions.Select(d => d.Name + suffix).ToArray());
                            if (r.Name != r.Alias)
                                refVar.Name = r.Alias;
                        }

                    }
                }
            }
            finally
            {
                IsAutocommitEnabled = ae;
            }
        }

        /// <summary>
        /// Adds the variable to the inner collection of variables.
        /// Should be used by derived classes and the <see cref="Variable"/> only to extend the collection.
        /// </summary>
        internal protected void AddVariableToCollection(Variable var)
        {
            if (!FireEventChanging(DataSetChangeAction.NewVariable, var))
                throw new CannotPerformActionException("Cannot add a variable");
            
            StartChanges();

            lock (this)
            {
                changes.Variables.InsertFirst(var);
            }

            FireEventChanged(DataSetChangeAction.NewVariable, changes, var);
        }

        /// <summary>
        /// Adds the variable to the inner collection of variables.
        /// Should be used by derived classes and the 
        /// <see cref="Variable "/>class only to extend the collection.
        /// Parameter <paramref name="proposedChanges"/> must be prepared.
        /// </summary>
        /// <param name="proposedChanges">Container for the changes to be accumulated in.</param>
        /// <param name="var"></param>
        internal protected void AddVariableToCollection(Variable var, DataSet.Changes proposedChanges)
        {
            if (!FireEventChanging(DataSetChangeAction.NewVariable, var))
                throw new CannotPerformActionException("Cannot add a variable");
            
            Debug.Assert(proposedChanges.Variables != null);
            proposedChanges.Variables.InsertFirst(var);

            //FireEventChanged(DataSetChangeAction.NewVariable, proposedChanges, var);
        }


        /// <summary>
        /// Creates new specific variable (doesn't add it to the collection).
        /// </summary>
        /// <typeparam name="DataType"></typeparam>
        /// <param name="varName"></param>
        /// <param name="dims"></param>
        /// <returns></returns>
        protected abstract Variable<DataType> CreateVariable<DataType>(string varName, string[] dims);

        /// <summary>
        /// Untyped version of <see cref="CreateVariable{DataType}"/>.
        /// </summary>
        protected Variable CreateVariable(Type dataType, string varName, string[] dims)
        {
            MethodInfo[] methods = this.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            foreach (MethodInfo m in methods)
            {
                if (m.Name == "CreateVariable" || m.IsGenericMethod)
                {
                    return (Variable)m.MakeGenericMethod(dataType).Invoke(this, new object[] { varName, dims });
                }
            }

            throw new MissingMemberException("DataSet.CreateVariable<DataType> method not found.");
        }

        /// <summary>
        /// Creates new <see cref="Variable"/> and adds it to the <see cref="DataSet"/>.
        /// </summary>
        /// <typeparam name="DataType">Type of data for the variable.</typeparam>
        /// <param name="varName">Name of the variable.</param>
        /// <param name="dims">List of names of the variable's dimensions.</param>
        /// <returns>New variable.</returns>
        /// <remarks>
        /// <para>See list of supported types in remarks for the <see cref="Variable"/> class.</para>
        /// <para>
        /// See remarks for the method <see cref="AddVariable(Type, string, Array, string[])"/>.
        /// </para>
        /// </remarks>
        public Variable<DataType> AddVariable<DataType>(string varName, params string[] dims)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("DataSet");
            if (IsReadOnly)
                throw new ReadOnlyException("DataSet is read only.");
            if (varName == null)
                varName = "";
            if (dims != null && HasDuplicates(dims))
                throw new DataSetException("Duplicate dimensions for a variable are not allowed.");

            Variable<DataType> var = CreateVariable<DataType>(varName, dims);
            AddVariableToCollection(var);

            return var;
        }

        private static bool HasDuplicates(string[] dims)
        {
            if (dims.Length == 0 || dims.Length == 1) return false;

            for (int i = 0; i < dims.Length - 1; i++)
                for (int j = i + 1; j < dims.Length; j++)
                    if (dims[i] == dims[j])
                        return true;
            return false;
        }

        /// <summary>
        /// Creates new <see cref="Variable"/> and adds it to the <see cref="DataSet"/>.
        /// </summary>
        /// <typeparam name="DataType">Type of data for the variable.</typeparam>
        /// <param name="varName">Name of the variable.</param>
        /// <param name="array">Initial data for the variable.</param>
        /// <param name="dims">List of names of the variable's dimensions. If null, dimensions are auto-named.</param>
        /// <returns>New variable.</returns>
        /// <remarks>
        /// <para>See list of supported types in remarks for the <see cref="Variable"/> class.</para>
        /// <para>
        /// If both <paramref name="array"/> and <paramref name="dims"/> are null,
        ///  or  <paramref name="array"/> is null and <paramref name="dims"/> is an empty array,
        /// then <see cref="DataSet"/> creates a scalar <see cref="Variable"/>.
        /// </para>
        /// <para>
        /// The <paramref name="array"/> is copied by the method, therefore after this method call, it is allowed
        /// to change the array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// <para>
        /// See also remarks for the method <see cref="AddVariable(Type, string, Array, string[])"/>.
        /// </para>
        /// </remarks>
        public Variable<DataType> AddVariable<DataType>(string varName, Array array, params string[] dims)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("DataSet");
            if (IsReadOnly)
                throw new ReadOnlyException("DataSet is read only.");

            if (dims == null || dims.Length == 0)
            {
                if (array == null)
                {
                    // Zero-rank variable.
                    dims = new string[0];
                }
                else
                {
                    dims = new string[array.Rank]; // unnamed dimensions
                    if (dimInferring)
                        GetAutoDimensions(dims);
                }
            }

            Variable<DataType> var = AddVariable<DataType>(varName, dims);
            if (array != null && array.Length > 0)
                var.PutData(array);

            return var;
        }

        /// <summary>
        /// Creates new <see cref="Variable"/> and adds it to the <see cref="DataSet"/>.
        /// </summary>
        /// <typeparam name="DataType">Type of data for the variable.</typeparam>
        /// <param name="varName">Name of the variable.</param>
        /// <param name="dims">List of names of the variable's dimensions. If null, names are inferred.</param>
        /// <param name="cs">Default coordinate system for the variable being added.</param>
        /// <returns>New variable.</returns>
        /// <remarks>
        /// See remarks for the method <see cref="AddVariable(Type, string, Array, string[])"/>.
        /// </remarks>
        [Obsolete("Coordinate systems will be removed from the future release. Use metadata attributes instead.")]
        public Variable<DataType> AddVariable<DataType>(string varName, CoordinateSystem cs, params string[] dims)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("DataSet");
            if (IsReadOnly)
                throw new ReadOnlyException("DataSet is read only.");
            if (cs.DataSet != this)
                throw new ArgumentException("Coordinate system must be attached to the same data set to be added to the variable.");

            if (dims == null || dims.Length == 0)
            {
                ReadOnlyDimensionList csDims = cs.GetDimensions();
                dims = new string[csDims.Count];
                for (int i = 0; i < csDims.Count; i++)
                {
                    dims[i] = csDims[i].Name;
                }
            }

            Variable<DataType> var = AddVariable<DataType>(varName, dims);
            var.AddCoordinateSystem(cs);

            return var;
        }

        /// <summary>
        /// Creates new <see cref="Variable"/> and adds it to the <see cref="DataSet"/>.
        /// </summary>
        /// <typeparam name="DataType">Type of data for the variable.</typeparam>
        /// <param name="varName">Name of the variable.</param>
        /// <param name="array">Initial data for the variable.</param>
        /// <param name="dims">List of names of the variable's dimensions.</param>
        /// <param name="cs">Default coordinate system for the variable being added.</param>
        /// <returns>New variable.</returns>
        /// <remarks>
        /// See also remarks for the method <see cref="AddVariable(Type, string, Array, string[])"/>.
        /// </remarks>
        [Obsolete("Coordinate systems will be removed from the future release. Use metadata attributes instead.")]
        public Variable<DataType> AddVariable<DataType>(string varName, Array array, CoordinateSystem cs, params string[] dims)
        {
            Variable<DataType> var = AddVariable<DataType>(varName, array, dims);
            if (cs != null)
                var.AddCoordinateSystem(cs);

            return var;
        }

        /// <summary>
        /// Creates new <see cref="Variable"/> and adds it to the <see cref="DataSet"/>.
        /// </summary>
        /// <param name="dataType">Type of data for the variable.</param>
        /// <param name="varName">Name of the variable.</param>
        /// <param name="array">Initial data for the variable.</param>
        /// <param name="dims">List of names of the variable's dimensions.</param>
        /// <returns>New variable.</returns>		
        /// <remarks>
        /// <para>See list of supported types in remarks for the <see cref="Variable"/> class.</para>
        /// <para>New variable can be defined and added into a <see cref="DataSet"/> at any time 
        /// (obviously, this procedure changes the schema of a <see cref="DataSet"/>). 
        /// The creating and adding of a variable are performed as a single operation by 
        /// AddVariable methods of the <see cref="DataSet"/> class. 
        /// These methods accept at least a name of a variable, its type and dimensions, and return new variable 
        /// instance.
        /// </para>		
        /// <para>
        /// Parameters <paramref name="array"/> and <paramref name="dims"/> can be null.
        /// A rank of the variable (i.e. a number of dimensions) is either equal to the rank of <paramref name="array"/>
        /// (if it is not a null) or a length of the <paramref name="dims"/>. Initial data of the new variable is
        /// an <paramref name="array"/>. If both array and dims are nulls (or <paramref name="dims"/> 
        /// is an empty array), then the variable is a scalar, 
        /// i.e. has rank zero (read more in remarks for <see cref="Variable"/>).
        /// </para>
        /// <para>
        /// The <paramref name="array"/> is copied by the method, therefore after this method call, it is allowed
        /// to change the array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// <example>
        /// <para>
        /// Let’s get an example of the methods use:
        /// </para>
        /// <code>
        /// public void AddVariables1(DataSet sds)
        /// {
        ///		// Variable "A" is a 1d variable of type DateTime depending on dimension "x"
        ///     Variable&lt;DateTime&gt; time = sds.AddVariable&lt;DateTime&gt;("A", "x");
        ///     time.Metadata["DisplayName"] = "Time";
        /// 
        ///     Variable&lt;double&gt; u = sds.AddVariable&lt;double&gt;("B", "x");
        ///     u.Metadata["DisplayName"] = "U-component of velocity";
        /// 
        ///     Variable&lt;double&gt; v = sds.AddVariable&lt;double&gt;("C", "x");
        ///     v.Metadata["DisplayName"] = "V-component of velocity";
        /// } 
        /// </code>
        /// <para>
        /// In the example, three one-dimensional variables A, B and C are added to the given <see cref="DataSet"/> instance.
        /// All variables depends on same dimension named <c>"x"</c>.
        /// The <see cref="Variable.Metadata"/> property defines corresponded metadata entry.
        /// </para>
        /// </example>
        /// <para>
        /// The very important is that the <see cref="DataSet"/> class sets relations between added variables. 
        /// The rule is: all dimensions of different variables with same name are considered as related 
        /// (i.e. they share same dimension). This means that values with equal indices by related 
        /// dimensions in different variables are considered as corresponded, and lengths 
        /// of related dimensions in different variable must be equal. See <see cref="Commit"/> 
        /// about constraints on data.
        /// </para>
        /// <para>Therefore in the given example all arrays depend on the same dimension named "x". 
        /// This leads to the constraint when lengths of A, B and C must be equal, and A, 
        /// for instance, can be an axis for B and C.
        /// </para>	
        /// <para>
        /// Dimension name is an arbitrary string, used only to specify 
        /// which dimensions of different variables are related.
        /// </para>
        /// <para><see cref="Variable.Rank"/> of a variable is computed as a number of specified dimensions. 
        /// Thus all dimensions must be given.
        /// </para>
        /// <example>
        /// <code>
        ///public void AddVariables2(DataSet sds)
        ///{
        /// sds.AddVariable&lt;double&gt;("lat", "y");
        /// sds.AddVariable&lt;double&gt;("lon", "x");
        /// sds.AddVariable&lt;double&gt;("pressure", "x", "y");
        ///} 
        /// </code>
        /// <para>
        /// In this example, variables <c>lat and lon</c> are one-dimensional, 
        /// pressure is two-dimensional variable, and a pair <c>lon</c> and <c>pressure</c> 
        /// (by first dimension), and another pair lat and pressure (by second dimension) 
        /// depend on a same dimension. In other words, values <c>pressure[i,j]</c> 
        /// corresponds to both <c>lon[i]</c> and <c>lat[j]</c>.
        /// </para>
        /// </example>
        /// <para>
        /// The constraint on variables requests that their dimensions with same name must have same length.
        /// </para>		
        /// </remarks>
        /// <seealso cref="Variable"/>
        public Variable AddVariable(Type dataType, string varName, Array array, params string[] dims)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("DataSet");
            if (IsReadOnly)
                throw new ReadOnlyException("DataSet is read-only.");

            MethodInfo addVarG = this.GetType().GetMethod("AddVariable", new Type[] { typeof(string), typeof(Array), typeof(string[]) });
            MethodInfo addVar = addVarG.MakeGenericMethod(dataType);

            Variable var = (Variable)addVar.Invoke(this, new object[] { varName, array, dims });
            return var;
        }

        /// <summary>
        /// Creates new <see cref="Variable"/> and adds it to the <see cref="DataSet"/>.
        /// </summary>
        /// <param name="dataType">Type of data for the variable.</param>
        /// <param name="varName">Name of the variable.</param>
        /// <param name="array">Initial data for the variable.</param>
        /// <param name="dims">List of names of the variable's dimensions.</param>
        /// <param name="cs">Default coordinate system for the variable being added.</param>
        /// <returns>New variable.</returns>		
        /// <remarks>
        /// The method is obsolete. See method
        /// <see cref="AddVariable(Type, string, Array, string[])"/>
        /// instead.
        /// </remarks>
        [Obsolete("Coordinate systems will be removed from the future release. Use metadata attributes instead.")]
        public Variable AddVariable(Type dataType, string varName, Array array, CoordinateSystem cs, params string[] dims)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("DataSet");
            if (IsReadOnly)
                throw new ReadOnlyException("DataSet is read only.");

            MethodInfo addVarG = this.GetType().GetMethod("AddVariable", new Type[] { typeof(string), typeof(Array), typeof(CoordinateSystem), typeof(string[]) });
            MethodInfo addVar = addVarG.MakeGenericMethod(dataType);

            Variable var = (Variable)addVar.Invoke(this, new object[] { varName, array, cs, dims });
            return var;
        }

        /// <summary>
        /// Creates a reference variable.
        /// </summary>
        /// <param name="var">Target variable.</param>
        /// <param name="dims">If not null, defines dimensions of the variable within this <see cref="DataSet"/>.</param>
        /// <returns>Reference variable to the given variable.</returns>
        /// <remarks>
        /// <para>See remarks for the <see cref="AddVariableByReference(Variable, string[])"/>.
        /// </para></remarks>
        /// <seealso cref="AddVariableByReference"/>
        /// <exception cref="NotSupportedException">References within the <see cref="DataSet"/> are prohibited.</exception>
        public Variable<DataType> AddVariableByReference<DataType>(Variable<DataType> var, params string[] dims)
        {
            return (Variable<DataType>)AddVariableByReference((Variable)var, dims);
        }

        /// <summary>
        /// Creates a reference variable.
        /// </summary>
        /// <param name="var">Target variable.</param>
        /// <param name="dims">If not null, defines dimensions of the variable within this <see cref="DataSet"/>.</param>
        /// <returns>Reference variable to the given variable.</returns>
        /// <remarks>
        /// <para>It is prohibited to create a reference to a variable from the same <see cref="DataSet"/>.
        /// </para>
        /// <para>
        /// If <paramref name="dims"/> is null or an empty array, the reference variable has the same dimensions,
        /// as the <paramref name="var"/> does.
        /// </para>
        /// <para>
        /// Reference variable is a special kind of variables whose purpose is to refer another variable.
        /// All requests to data and metadata are translated to corresponding requests for the
        /// underlying variable.
        /// Changes to underlying variable generate corresponding events for the reference variable, etc.
        /// Both target and reference variables share the metadata collection.
        /// Names of dimensions and coordinate systems can change independently and 
        /// don't cause another variable to be changed.
        /// </para>
        /// <para>
        /// If a <see cref="DataSet"/> has incoming or outcoming reference, it is linked to another
        /// <see cref="DataSet"/> and they can be committed only together (i.e. "distributed" commit happens).
        /// The <see cref="DataSet.IsLinked"/> property gets the value indicating whether the <see cref="DataSet"/>
        /// has incoming or outcoming references or not.
        /// The <see cref="DataSet.GetLinkedDataSets()"/> method returns list of linked DataSets.
        /// </para>
        /// </remarks>
        /// <example>
        /// The following example creates two DtaSets; creates a variable in the first and
        /// adds a reference to the variable into the second.
        /// Then it changes both data and metadata for both variables and checks how these
        /// changes are propogated.
        /// <code>
        /// using (DataSet ds1 = DataSet.Open("msds:csv?file=test.csv&amp;openMode=create"))
        /// using (DataSet ds2 = DataSet.Open("msds:csv?file=test2.csv&amp;openMode=create"))
        ///	{
        ///		ds1.IsAutocommitEnabled = false;
        ///		ds2.IsAutocommitEnabled = false;
        /// 
        ///		// Adding a variable "v1" into ds1
        ///		Variable&lt;int&gt; v = ds1.AddVariable&lt;int&gt;("v1", "1");
        ///		// Adding a reference to "v1" into ds2
        ///		Variable&lt;int&gt; rf = ds2.AddVariableByReference&lt;int&gt;(v);
        ///
        ///		// ds1 and ds2 are linked through the variable that is changed.
        ///		// Therefore, a distributed commit happens and both DataSets are to be committed 
        ///		// within a single transaction.
        ///		ds1.Commit();
        ///		Assert.IsFalse(ds1.HasChanges); 
        ///		Assert.IsFalse(ds2.HasChanges); // ds2 is committed, too
        ///
        ///		//-- Changing var
        ///		int[] a1 = new int[] { 1, 2, 3 };
        ///		v.Append(a1);  // changing "v1"
        ///		Assert.IsTrue(ds1.HasChanges);
        ///		Assert.IsTrue(ds2.HasChanges); // true for the rf is changed
        ///		ds2.Commit(); // distributed commit commits ds1 and ds2
        ///
        ///		// Both variables have same data:
        ///		Assert.IsTrue(Compare(a1, v.GetData()));  
        ///		Assert.IsTrue(Compare(a1, rf.GetData()));
        ///
        ///		//-- Changing ref var
        ///		int[] a2 = new int[] { 1, 2, 3, 4, 5, 6 };
        ///		rf.PutData(a2); // changing reference to "v1"
        ///		Assert.IsTrue(ds1.HasChanges); // v1 is changed
        ///		Assert.IsTrue(ds2.HasChanges);
        ///		ds1.Commit(); // distributed commit
        ///		Assert.IsTrue(Compare(a2, v.GetData())); // same data
        ///		Assert.IsTrue(Compare(a2, rf.GetData()));
        ///
        ///		//-- Changing var metadata
        ///		// Metadata changes are propogated, too.
        ///		v.Metadata["test"] = 10.0;
        ///		Assert.IsTrue(ds1.HasChanges);
        ///		Assert.IsTrue(ds2.HasChanges);
        ///		ds1.Commit();
        ///		Assert.AreEqual(10.0, (double)v.Metadata["test"]); 
        ///		Assert.AreEqual(10.0, (double)rf.Metadata["test"]); 
        ///
        ///		//-- Changing ref var
        ///		// Metadata changes are propogated, too.
        ///		rf.Metadata["test"] = 9.0;
        ///		Assert.IsTrue(ds1.HasChanges);
        ///		Assert.IsTrue(ds2.HasChanges);
        ///		ds1.Commit();
        ///		Assert.AreEqual(9.0, (double)v.Metadata["test"]);
        ///		Assert.AreEqual(9.0, (double)rf.Metadata["test"]);
        ///
        ///		//-- Changing both
        ///		rf.Metadata["test"] = 8.0;
        ///		v.Metadata["test"] = 7.0; // latest must be actual
        ///		Assert.IsTrue(ds1.HasChanges);
        ///		Assert.IsTrue(ds2.HasChanges);
        ///		ds1.Commit();
        ///		Assert.AreEqual(7.0, (double)v.Metadata["test"]);
        ///		Assert.AreEqual(7.0, (double)rf.Metadata["test"]);
        ///	}
        /// </code>
        /// </example>
        /// <seealso cref="AddVariableByReference{DataType}"/>
        /// <exception cref="NotSupportedException">References within the DataSet are prohibited.</exception>
        public Variable AddVariableByReference(Variable var, params string[] dims)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("DataSet");
            if (IsReadOnly)
                throw new ReadOnlyException("DataSet is read only.");
            if (var.DataSet == this)
                throw new DataSetException("References within the DataSet are prohibited.");

            if (variables.Contains(var))
                return var;

            if (dims == null || dims.Length == 0)
                dims = var.GetDimensions(SchemaVersion.Committed).AsNamesArray();

            // Creating reference variable 
            Variable refVar = CreateRefVariable(var, dims);

            if (outcomingReferences == null)
                outcomingReferences = new DataSetLinkCollection();
            outcomingReferences.AddLink(refVar, var);

            var.DataSet.AddIncomingReference(refVar);

            AddVariableToCollection(refVar); // autocommit happens here

            return refVar;
        }

        /// <summary>
        /// Adds incoming refence to the data set.
        /// The method is called from another data set when it adds a reference to the
        /// variable from this data set.
        /// </summary>
        /// <param name="refVar"></param>
        private void AddIncomingReference(Variable refVar)
        {
            IRefVariable irefVar = (IRefVariable)refVar;
            if (irefVar.ReferencedVariable.DataSet != this)
                throw new DataSetException("Given variable must refer to the data set it is added to as an incoming reference.");

            if (incomingReferences == null)
                incomingReferences = new DataSetLinkCollection();
            lock (incomingReferences)
            {
                incomingReferences.AddLink(refVar, irefVar.ReferencedVariable);
            }
        }

        /// <summary>
        /// Removes all incoming references whose source is given data set.
        /// This method is invoked by the referal data set.
        /// </summary>
        /// <param name="sourceDataSet"></param>
        private void RemoveIncomingReference(DataSet sourceDataSet)
        {
            lock (this)
            {
                if (incomingReferences == null || incomingReferences.Count == 0) return;
                incomingReferences.RemoveLinks(sourceDataSet);
            }
        }

        /// <summary>
        /// Removes given incomingLink:
        /// finds the entry that refers the variable from the given link and
        /// removes it.
        /// This method is invoked by the referal data set.
        /// </summary>
        /// <param name="incomingLink"></param>
        private void RemoveIncomingReference(DataSetLink incomingLink)
        {
            if (incomingReferences == null) return;

            DataSetLink link = null;
            foreach (var item in incomingReferences)
            {
                if (item == incomingLink)
                {
                    link = item;
                    break;
                }
            }
            if (link != null)
                incomingReferences.RemoveLink(link);

        }

        /// <summary>
        /// This method handles cases when a cs has been added to the 
        /// referenced variable, hence we have to add the same cs to our cs collection.
        /// </summary>
        private void ReferencedVariableGotNewCoordinateSystem(object sender, CoordinateSystemAddedEventArgs e)
        {
            // Disabled for Release 1.0

            //IRefVariable refVar = (IRefVariable)sender;
            //if (refVar.ReferencedVariable.DataSet == e.CoordinateSystem.DataSet)
            //{
            //    AddCoordinateSystemToCollection(e.CoordinateSystem);
            //}
        }

        private Variable CreateRefVariable(Variable referencedVar, /*string varName,*/ string[] dims)
        {
            Type varType = typeof(Variable<>).MakeGenericType(referencedVar.TypeOfData);
            System.Reflection.ConstructorInfo cinfo =
                typeof(RefVariable<>).MakeGenericType(referencedVar.TypeOfData)
                .GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(DataSet), varType, /*typeof(string),*/ typeof(string[]) }, null);
            return (Variable)cinfo.Invoke(new object[] { this, referencedVar, /*varName,*/ dims });
        }

        /// <summary>
        /// Fills the prepared string array <paramref name="dims"/> with names of dimensions
        /// those are assigned automicatically.
        /// </summary>
        /// <param name="dims">An array to fill with dimensions names.</param>
        /// <remarks>
        /// <para>If the <see cref="DataSet.AddVariable(Type,string,Array,string[])"/>
        /// has no dimensions specified, it automatically names the dimensions.
        /// These names can be found using <see cref="GetAutoDimensions"/> method.
        /// </para>
        /// </remarks>
        public static void GetAutoDimensions(string[] dims)
        {
            if (dims == null) throw new ArgumentNullException("dims");
            for (int i = 0; i < dims.Length; i++)
            {
                dims[i] = "_" + (i + 1).ToString();
            }
        }

        /// <summary>
        /// Sets up and adds a <see cref="Variable"/> to the <see cref="DataSet"/> by value, not as a reference.		
        /// </summary>
        /// <param name="var">A variable to add.</param>
        /// <param name="varName">The name of the variable in the <see cref="DataSet"/>.</param>
        /// <param name="dims">List of names of the variable's dimensions in the <see cref="DataSet"/>.</param>
        /// <param name="includeCoordinateSystems"></param>
        /// <returns>Just added variable.</returns>
        /// <remarks>
        /// All the data and metadata of the <paramref name="var"/> 
        /// (and optionally of all related coordinate systems and their axes)
        /// will be copied to the new just created variable using the data set's data access provider.
        /// Real copy process is done during committing (at the precommit phase).
        /// </remarks>
        [Obsolete]
        private Variable AddVariableByValue(Variable var, bool includeCoordinateSystems, string varName, params string[] dims)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("DataSet");
            if (readOnly)
                throw new ReadOnlyException("The data set is read only therefore cannot add a variable");
            if (includeCoordinateSystems)
                throw new DataSetException("Coordinate systems copying is not implemented in this version");
            if (var.ID == DataSet.GlobalMetadataVariableID)
                throw new DataSetException("Cannot add by value a global metadata variable");

            if (varName == null)
                varName = var.Name;
            if (dims == null)
                dims = var.GetDimensions(SchemaVersion.Committed).AsNamesArray();

            StartChanges();
            Variable var2 = CreateVariable(var.TypeOfData, varName, dims);

            // Copying committed data from var
            Array committedData = var.GetData();
            if (committedData != null && committedData.Length != 0)
            {
                var2.PutData(committedData);
            }

            // Copying proposed data from var
            if (var.HasChanges)
            {
                var varChanges = var.GetInnerChanges();
                if (varChanges.Data != null && varChanges.Data.Count != 0)
                {
                    int length = varChanges.Data.Count;
                    for (int i = 0; i < length; i++)
                    {
                        Variable.DataPiece piece = varChanges.Data[i];
                        var2.PutData(piece.Origin, piece.Data);
                    }
                }
            }

            // Copying metadata
            var dict = var.Metadata.AsDictionary();
            foreach (var attr in dict)
            {
                var2.Metadata[attr.Key] = attr.Value;
            }

            AddVariableToCollection(var2);
            return var2;
        }

        /// <summary>
        /// Adds a variable to the data set by value.
        /// It means that all its data (and optionally of all related coordinate systems and their axes)
        /// will be copied using the data set's data access provider.
        /// </summary>
        /// <param name="var">A variable to add.</param>    
        /// <param name="includeCoordinateSystems"></param>
        /// <returns>Just added variable.</returns>
        [Obsolete]
        private Variable AddVariableByValue(Variable var, bool includeCoordinateSystems)
        {
            return AddVariableByValue(var, includeCoordinateSystems, null, null);
        }

        /// <summary>
        /// Sets up and adds a <see cref="Variable"/> to the <see cref="DataSet"/> by value.		
        /// </summary>
        /// <param name="var">A variable to add.</param>
        /// <returns>Created variable.</returns>
        /// <remarks>
        /// See remarks for <see cref="DataSet.AddVariableByValue{DataType}(Variable{DataType},string,string[])"/>.
        /// </remarks>
        public Variable AddVariableByValue(Variable var)
        {
            return AddVariableByValue(var, false, null, null);
        }

        /// <summary>
        /// Sets up and adds a <see cref="Variable"/> to the <see cref="DataSet"/> by value.
        /// </summary>
        /// <param name="var">A variable to add.</param>
        /// <param name="name">The name of the created variable in the <see cref="DataSet"/>.</param>
        /// <param name="dims">The created variable's dimensions.</param>
        /// <returns>Created variable.</returns>
        /// <remarks>
        /// See remarks for <see cref="DataSet.AddVariableByValue{DataType}(Variable{DataType},string,string[])"/>.
        /// </remarks>
        public Variable AddVariableByValue(Variable var, string name, string[] dims)
        {
            return AddVariableByValue(var, false, name, dims);
        }

        /// <summary>
        /// Sets up and adds a <see cref="Variable"/> to the <see cref="DataSet"/> by value.
        /// </summary>
        /// <param name="var">A variable to add.</param>
        /// <returns>Created variable.</returns>
        /// <remarks>
        /// See remarks for <see cref="DataSet.AddVariableByValue{DataType}(Variable{DataType},string,string[])"/>.
        /// </remarks>
        public Variable<DataType> AddVariableByValue<DataType>(Variable<DataType> var)
        {
            return AddVariableByValue<DataType>(var, false, null, null);
        }

        /// <summary>
        /// Sets up and adds a <see cref="Variable"/> to the <see cref="DataSet"/> by value.	
        /// </summary>
        /// <typeparam name="DataType">Type of data.</typeparam>
        /// <param name="var">A variable to add.</param>
        /// <param name="name">The name of the created variable in the <see cref="DataSet"/>.</param>
        /// <param name="dims">The created variable's dimensions.</param>
        /// <returns>Created variable.</returns>
        /// <remarks>
        /// <para>
        /// The method creates new <see cref="Variable"/> in the <see cref="DataSet"/> with the same rank and data type as <paramref name="var"/> 
        /// and copies all the data and metadata from the <paramref name="var"/> 
        /// to the created <see cref="Variable"/>.
        /// </para>
        /// <para>
        /// Please note, if the <paramref name="var"/> has changes (see <see cref="Variable.HasChanges"></see>),
        /// proposed data also is copied into the target <see cref="Variable"/>. But all modifications of the <paramref name="var"/>
        /// done after the method is finished, do not affect the method's resulting <see cref="Variable"/>.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        ///	int[] a1 = new int[] { 1, 2, 3 };
        ///	string[,] a2 = new string[,] { { "a", "b", "c" }, { "d", "e", "f" }, { "g", "h", "i" } };
        ///
        ///	using (DataSet src = DataSet.Open("msds:csv?file=test.csv&amp;openMode=create"))
        ///	using (DataSet dst = DataSet.Open("msds:memory"))		
        /// {
        ///		src.IsAutocommitEnabled = false;
        ///		dst.IsAutocommitEnabled = false;
        ///		
        ///		var var1 = src.AddVariable&lt;int&gt;("var1", "x");
        ///		var var2 = src.AddVariable&lt;string&gt;("var2", "y", "z");
        ///
        ///		var1.PutData(a1);
        ///		var2.PutData(a2);
        ///		src.Commit();
        ///
        ///		var1.Append(new int[] { 4 }); // adding "4" to proposed schema of var1
        ///		// dvar1 is a deep copy of var1, including proposed schema:
        ///		var dvar1 = dst.AddVariableByValue&lt;int&gt;(var1); 
        ///		var dvar2 = dst.AddVariableByValue&lt;string&gt;(var2);
        ///
        ///		// Adding "5" to proposed schema of dvar1.
        ///		// It doesn't affect var1!
        ///		dvar1.Append(new int[] { 5 });
        ///
        ///		// Committing both data sets 
        ///		dst.Commit();
        ///		src.Commit();
        ///
        ///		Assert.IsFalse(src.HasChanges);
        ///		Assert.IsFalse(dst.HasChanges);
        /// 
        ///		Assert.IsTrue(Compare(new int[] { 1, 2, 3, 4 }, var1.GetData()));
        ///		Assert.IsTrue(Compare(a2, var2.GetData()));
        ///		Assert.IsTrue(Compare(a2, dvar2.GetData()));
        ///		Assert.IsTrue(Compare(new int[] { 1, 2, 3, 4, 5 }, dvar1.GetData()));
        ///	}
        /// </code>
        /// </example>
        /// <seealso cref="DataSet.Clone(string)">Creates a deep copy of a DataSet.</seealso>
        public Variable<DataType> AddVariableByValue<DataType>(Variable<DataType> var, string name, params string[] dims)
        {
            return AddVariableByValue<DataType>(var, false, name, dims);
        }


        /// <summary>
        /// Sets up and adds a variable to the data set by value, not as a reference.
        /// It means that all its data (and optionally of all related coordinate systems and their axes)
        /// will be copied using the data set's data access provider.
        /// </summary>
        /// <param name="var">A variable to add.</param>
        /// <param name="varName">The name of the variable in the data set.</param>
        /// <param name="dims">List of names of the variable's dimensions in the data set.</param>
        /// <param name="includeCoordinateSystems"></param>
        /// <returns>Just added variable.</returns>
        private Variable<DataType> AddVariableByValue<DataType>(Variable<DataType> var, bool includeCoordinateSystems, string varName, params string[] dims)
        {
            return (Variable<DataType>)AddVariableByValue((Variable)var, includeCoordinateSystems, varName, dims);
        }

        /// <summary>
        /// Adds a variable to the data set by value, not as a reference.
        /// It means that all its data (and optionally of all related coordinate systems and their axes)
        /// will be copied using the data set's data access provider.
        /// </summary>
        /// <param name="var">A variable to add.</param>   
        /// <param name="includeCoordinateSystems"></param>
        /// <returns>Just added variable.</returns>
        private Variable<DataType> AddVariableByValue<DataType>(Variable<DataType> var, bool includeCoordinateSystems)
        {
            return (Variable<DataType>)AddVariableByValue((Variable)var, includeCoordinateSystems);
        }

        #endregion

        #endregion

        #region Coordinate systems

        /// <summary>
        /// Gets the value, indicating whether the data set supports for coordinate systems.
        /// </summary> 
        [Obsolete("Coordinate systems will be removed from the future release. Use metadata attributes instead.")]
        internal bool SupportsCoordinateSystems
        {
            get { return supportsCoordinateSystems; }
        }

        /// <summary>
        /// Gets the collection of coordinate systems attached to the DataSet.
        /// </summary>
        /// <remarks>
        /// <para>Collection contains all coordinate system of the data set, including 
        /// committed and proposed coordinate systems, but the simple by name indexer of the 
        /// <see cref="CoordinateSystemCollectionBase"/> collection looks up among committed coordinate
        /// systems only. To get a coordinate system from a particular schema version use special indexer 
        /// <see cref="CoordinateSystemCollectionBase.this[string,SchemaVersion]"/> collection:
        /// </para>
        /// <example>
        /// <code>
        /// CoordinateSystem cs = dataSet.CoordinateSystems["cs", SchemaVersion.Recent];
        /// </code>
        /// </example>
        /// <para>
        /// There is a method <see cref="GetCoordinateSystems(SchemaVersion)" />
        /// that returns a collection of coordinate systems for a given schema version only.
        /// </para>
        /// <para>
        /// The following example iterates through all coordinate systems of a data set 
        /// being loaded from sample.csv file and prints information about the data set to the console:
        /// </para>
        /// <example>
        /// <code>
        /// DataSet sds = new CsvDataSet("sample.csv");
        /// 
        /// Console.WriteLine ("ScientficDataSet " + sds.Name +
        /// (HasChanges ? "*" : "") + " contents: ");
        /// 
        /// Console.WriteLine (" Coordinate Systems:");
        /// foreach (CoordinateSystem s in sds.CoordinateSystems)
        /// {
        ///		Console.WriteLine (s.ToString());
        /// }
        /// </code>
        /// </example>
        /// </remarks>
        /// <seealso cref="CoordinateSystem"/>
        /// <seealso cref="CoordinateSystemCollectionBase"/>
        /// <seealso cref="GetCoordinateSystems(SchemaVersion)"/>
        /// <seealso cref="Variables"/>
        /// <seealso cref="DataSet.Commit"/>
        [Obsolete("Coordinate systems will be removed from the future release. Use metadata attributes instead.")]
        public ReadOnlyCoordinateSystemCollection CoordinateSystems
        {
            get
            {
                return GetCoordinateSystems(SchemaVersion.Recent);
            }
        }

        /// <summary>
        /// Gets the specified version of read-only collection of coordinate systems.
        /// </summary>
        /// <param name="version">The version of schema to take CoordinateSystems from.</param>
        [Obsolete("Coordinate systems will be removed from the future release. Use metadata attributes instead.")]
        protected internal ReadOnlyCoordinateSystemCollection GetCoordinateSystems(SchemaVersion version)
        {
            if (version == SchemaVersion.Committed)
                return csystems;

            if (version == SchemaVersion.Proposed)
            {
                if (!HasChanges)
                    throw new DataSetException("Request for a proposed version meanwhile the variable has no changes.");

                // Build and return the list of proposed cs
                if (changes.CoordinateSystems == null)
                    return new ReadOnlyCoordinateSystemCollection();

                List<CoordinateSystem> proposedCs = new List<CoordinateSystem>();
                foreach (var cs in changes.CoordinateSystems)
                {
                    if (cs.HasChanges)
                        proposedCs.Add(cs);
                }
                return new ReadOnlyCoordinateSystemCollection(proposedCs);
            }

            // Recent
            if (changes == null || changes.CoordinateSystems == null)
                return csystems;
            return changes.CoordinateSystems.GetReadOnlyCollection();
        }

        /// <summary>
        /// Adds the coordinate system to the internal collection of coordinate systems 
        /// and fires appropriate events.
        /// </summary>
        protected void AddCoordinateSystemToCollection(CoordinateSystem cs)
        {
            if (!FireEventChanging(DataSetChangeAction.NewCoordinateSystem, cs))
                throw new CannotPerformActionException("Coordinate system add procedure is canceled.");

            StartChanges();

            changes.CoordinateSystems.Add(cs);

            FireEventChanged(DataSetChangeAction.NewCoordinateSystem, changes, cs);
        }

        /// <summary>
        /// Adds the coordinate system to the internal collection of coordinate systems 
        /// and fires appropriate events.
        /// </summary>
        protected void AddCoordinateSystemToCollection(CoordinateSystem cs, Changes proposedChanges)
        {
            if (!FireEventChanging(DataSetChangeAction.NewCoordinateSystem, cs))
                throw new CannotPerformActionException("Coordinate system add procedure is cancelled.");

            Debug.Assert(proposedChanges.CoordinateSystems != null);
            proposedChanges.CoordinateSystems.Add(cs);
        }

        /// <summary>
        /// Creates a coordinate system with specified name and attaches it to the data set.
        /// </summary>
        /// <param name="name">Name of the coordinate system.</param>
        /// <param name="axes">Axes contained by the coordinate system.</param>
        /// <returns>The <see cref="CoordinateSystem"/> instance.</returns>
        /// <remarks>
        /// <para>
        /// <see cref="DataSet"/> prohibits different coordinate systems with the same name.
        /// An attempt to add a coordinate system with existing name leads to an exception.
        /// </para>
        /// <para>
        /// After a coordinate system is added, it should be filled with axes
        /// using <see cref="CoordinateSystem.AddAxis(Variable)"/> methods. After a coordinate system
        /// is committed, it cannot be modified.
        /// </para>
        /// <para>
        /// A coordinate system can be added to a variable, it means that the variable
        /// is defined in that coordinate system. And there are constraints on this type
        /// of relations. See details in remarks for <see cref="DataSet.Commit"/> method.
        /// </para>
        /// <example>
        /// The following example creates a variable "time" to store time moments;
        /// a coordinate system "Time" with one axis "time";
        /// and a variable "airTemp" defined in that coordinate system.
        /// Note that "time" and "airTemp" depend on the same dimension.
        /// <code>
        /// using (DataSet ds = CreateDataSet())
        /// {
        ///		ds1.IsAutocommitEnabled = false;
        ///		
        ///		Variable time = ds.AddVariable&lt;int&gt;("time", "t");
        ///		CoordinateSystem cs = ds.CreateCoordinateSystem("Time", time);
        ///		// cs has one axis "time"
        ///		
        ///		Variable var = ds.AddVariable&lt;int&gt;("airTemp", "time");
        ///		var.AddCoordinateSystem(cs);
        ///		ds.Commit();
        /// }
        /// </code>
        /// </example>
        /// </remarks>
        [Obsolete("Coordinate systems will be removed from the future release. Use metadata attributes instead.")]
        public CoordinateSystem CreateCoordinateSystem(string name, params Variable[] axes)
        {
            if (axes == null)
                throw new ArgumentNullException("axes");
            if (axes.Length == 0)
                throw new ArgumentException("Coordinate system must contain at least one axis");

            if (IsDisposed)
                throw new ObjectDisposedException("DataSet");
            if (IsReadOnly)
                throw new NotSupportedException("DataSet is read only.");

            if (!SupportsCoordinateSystems)
                throw new NotSupportedException("Coordinate systems are not supported by the data set.");

            CoordinateSystem cs = new CoordinateSystem(name, this);
            foreach (var axis in axes)
                cs.AddAxis(axis);

            AddCoordinateSystemToCollection(cs);

            return cs;
        }

        /// <summary>
        /// Creates a coordinate system with specified name and places it in the given changeset.
        /// </summary>
        [Obsolete("Coordinate systems will be removed from the future release. Use metadata attributes instead.")]
        protected CoordinateSystem CreateCoordinateSystem(string name, Variable[] axes, Changes changes)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("DataSet");
            if (IsReadOnly)
                throw new NotSupportedException("DataSet is read only.");

            if (!SupportsCoordinateSystems)
                throw new NotSupportedException("Coordinate systems are not supported by the data set.");

            CoordinateSystem cs = new CoordinateSystem(name, this);
            foreach (var axis in axes)
                cs.AddAxis(axis);

            AddCoordinateSystemToCollection(cs, changes);

            return cs;
        }

        #endregion

        #region Dimensions

        /// <summary>
        /// Gets the list of dimensions of the <see cref="DataSet"/>.
        /// </summary>
        /// <remarks>
        /// <para>The property returns committed dimensions.
        /// To get proposed dimensions, use <see cref="GetSchema(SchemaVersion)"/> method.</para>
        /// </remarks>
        public ReadOnlyDimensionList Dimensions
        {
            get
            {
                return commitedDimensions;
            }
        }

        #endregion

        #region Schema and transacted changes

        /// <summary>
        /// Gets a copy of the DataSet that contains all changes made to it since last committing.
        /// </summary>
        /// <returns></returns>
        private DataSet GetChanges()
        {
            if (!HasChanges)
                return null;

            throw new NotImplementedException("Sorry, not yet...");
        }

        /// <summary>
        /// Gets the value indicating whether the <see cref="DataSet"/> is modified.
        /// </summary>
        /// <remarks>
        /// The <see cref="DataSet"/> is modified, if its schema or data are changed.
        /// To commit changes, use <see cref="DataSet.Commit"/> method.
        /// To rollback changes, use <see cref="DataSet.Rollback"/> method.
        /// </remarks>
        public bool HasChanges
        {
            get
            {
                lock (this)
                {
                    return changes != null;
                }
            }
        }

        /// <summary>
        /// Gets the schema of the <see cref="DataSet"/>.
        /// </summary>
        /// <returns>Committed schema of the <see cref="DataSet"/>.</returns>
        /// <remarks>The method returns the committed schema of the <see cref="DataSet"/>.
        /// To get proposed schema, use <see cref="GetSchema(SchemaVersion)"/>.</remarks>
        /// <seealso cref="GetSchema(SchemaVersion)"/>
        public DataSetSchema GetSchema()
        {
            return GetSchema(SchemaVersion.Committed);
        }

        /// <summary>Gets the specified version of <see cref="DataSet"/> schema.</summary>
        /// <param name="version">Version of the schema to get.</param>
        /// <returns>Specified version of schema of the <see cref="DataSet"/>.</returns>
        /// <remarks>
        /// If the <see cref="DataSet"/> is modified (see <see cref="HasChanges"/> property),
        /// it has two versions of the schema. First is a committed schema (the version named <see cref="SchemaVersion.Committed"/>),
        /// second is a proposed schema (the version named <see cref="SchemaVersion.Proposed"/>). 
        /// The committed schema describes the actual content of the underlying storage and always
        /// satisfies the <see cref="DataSet"/> constraints.
        /// The proposed schema may not to satisfy <see cref="DataSet"/> constraints and is being accumulated
        /// as a user modifies the <see cref="DataSet"/> (if the <see cref="IsAutocommitEnabled"/> is false).
        /// After sucessful <see cref="DataSet.Commit"/>, the proposed schema becomes committed.
        /// Version <see cref="SchemaVersion.Recent"/> corresponds to the proposed version, if it exists;
        /// or to the committed version, otherwise.
        /// </remarks>
        public DataSetSchema GetSchema(SchemaVersion version)
        {
            lock (this)
            {
                if (version == SchemaVersion.Committed || !HasChanges)
                {
                    if (version == SchemaVersion.Proposed)
                        throw new DataSetException("DataSet is committed and has no proposed version.");

                    return GetCommittedSchema(true);
                }

                // HasChanges == true && (version != SchemaVersion.Committed):
                IEnumerable<Variable> vars = changes.Variables == null ?
                    (IEnumerable<Variable>)variables.All :
                    (IEnumerable<Variable>)changes.Variables;

                ICollection<CoordinateSystem> coords = changes.CoordinateSystems == null ?
                    (ICollection<CoordinateSystem>)csystems :
                    (ICollection<CoordinateSystem>)changes.CoordinateSystems;

                return new DataSetSchema(
                   guid,
                   URI,
                   changeSetId,
                   vars.Select(v => v.GetSchema(SchemaVersion.Recent)).ToArray(),
                   coords.Select(cs => cs.GetSchema()).ToArray());
            }
        }

        /// <summary>
        /// Builds committed schema for the data set and saves it in the
        /// field committedSchema.
        /// </summary>
        private void CaptureCommittedSchema()
        {
            committedSchema = BuildCommittedSchema();
        }

        private DataSetSchema BuildCommittedSchema()
        {
            List<VariableSchema> committed = new List<VariableSchema>();
            foreach (var var in variables.All)
            {
                var.CaptureCommittedSchema();
                committed.Add(var.GetSchema(SchemaVersion.Committed));
            }

            return new DataSetSchema(
                guid,
                URI,
                changeSetId,
                committed.ToArray(),
                csystems.Select(cs => cs.GetSchema()).ToArray());
        }

        private DataSetSchema GetCommittedSchema(bool doRebuild)
        {
            var copy = committedSchema;
            if (copy == null && doRebuild)
                copy = BuildCommittedSchema();
            return copy;
        }

        /// <summary>
        /// Checks constraints and if the check succeeds, commits all recent changes.
        /// </summary>
        /// <returns>If commit succeeds, returns true; otherwise, returns false or throws an exception.</returns>
        /// <remarks>
        /// <para>
        /// If commit succeeds, returns true.
        /// If commit is failed because of failed constraints check, returns false; 
        /// otherwise, throws an exception describing the failure reason.
        /// See also <see cref="Commit"/> method.
        /// </para>
        /// </remarks>
        public bool TryCommit()
        {
            try
            {
                Commit();
                return true;
            }
            catch (ConstraintsFailedException ex)
            {
                Trace.WriteLineIf(DataSet.TraceDataSet.TraceInfo, " /// Committing failed: " + ex.Message);
                return false;
            }
            catch (DistributedCommitFailedException ex)
            {
                if (!(ex.InnerException is ConstraintsFailedException))
                    throw;
                Trace.WriteLineIf(DataSet.TraceDataSet.TraceInfo, " /// Committing failed: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Checks constraints and if the check succeeds, commits all recent changes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Two-phase transaction mechanism is used to commit the <see cref="DataSet"/> or a number
        /// of linked by reference variables <see cref="DataSet"/>s (see also 
        /// <see cref="GetLinkedDataSets"/>).
        /// This approach guarantees that persistent data are always in a consistent state, 
        /// i.e. satisfy required constraints.
        /// The rule says that all variables sharing a dimension must have equal length
        /// by that dimension. The dimensions are shared between two or more variables if 
        /// they have same name. Dimensions names are specified when a <see cref="Variable"/>
        /// is added to the <see cref="DataSet"/> (see <see cref="AddVariable{DataType}(string,Array,string[])"/>).
        /// </para>
        /// <para>
        /// If any changes are made in a schema or data of a <see cref="DataSet"/>, these changes are 
        /// accumulated until a user tries to commit them. 
        /// Therefore, if a <see cref="DataSet"/> has any changes (see <see cref="HasChanges"/>), 
        /// there are two versions of a schema available in the <see cref="DataSet"/>
        /// (see <see cref="SchemaVersion"/>): 
        /// first is a committed schema (it is always consistent) and second is a proposed schema 
        /// (it has no constraints); see also <see cref="GetSchema(SchemaVersion)"/>. 
        /// <see cref="Commit"/> of proposed changes can succeed only if all 
        /// constraints are satisfied; in this case proposed changes become new consistent state of the <see cref="DataSet"/>.
        /// If the check fails, a user can either try to fix changes or 
        /// <see cref="Rollback"/> to a previous consistent state.
        /// </para>
        /// <para>
        /// To ease the work with the <see cref="DataSet"/>, the autocommitting capability is added 
        /// and is enabled by default (see <see cref="IsAutocommitEnabled"/> property). 
        /// When it is enabled, the <see cref="DataSet"/> tries to commit after every change of the <see cref="DataSet"/>.
        /// Consider disable it to increase the performance.
        /// </para>
        /// <example>
        /// <code>
        /// ds.IsAutocommitEnabled = false;
        /// . . .
        /// 
        /// try {
        ///		// Renaming just added (not yet committed) variable "A", i.e. changing its schema:
        ///		ds.Variables["A", SchemaVersion.Recent].Name = "D"; 
        ///		. . .
        ///		ds.Commit();
        ///	}
        ///	catch  { // commit failed
        ///		ds.Rollback();
        ///	}
        /// </code>
        /// </example>
        /// <para>
        /// If constraints check fails, the <see cref="Commit"/> method throws 
        /// <see cref="ConstraintsFailedException"/> exception with information about the fault.
        /// </para>
        /// <para>
        /// See also method <see cref="TryCommit"/> and property
        /// <see cref="IsAutocommitEnabled"/>.
        /// </para>
        /// </remarks>
        /// <exception cref="ConstraintsFailedException">Thrown if commit has failed.</exception>
        /// <exception cref="ReadOnlyException">Thrown if the DataSet is read only.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the DataSet is disposed.</exception>
        /// <seealso cref="TryCommit"/>
        /// <seealso cref="IsAutocommitEnabled"/>
        public virtual void Commit()
        {
            lock (this)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException("DataSet");
                if (IsReadOnly)
                    throw new ReadOnlyException("DataSet is read only.");

                if (!HasChanges)
                {
                    Trace.WriteLineIf(DataSet.TraceDataSet.TraceInfo, string.Format("DataSet {0} has no changes.", DataSetGuid));
                    return;
                }

                // Obtaining changes from variables
                foreach (var var in changes.Variables)
                {
                    if (var.HasChanges)
                        changes.UpdateChanges(var.GetInnerChanges());
                }

                PrecommitOutput precommitOut;
                if (IsLinked)
                    precommitOut = DistributedCommitChanges(changes);
                else
                    precommitOut = ApplyChanges(changes);

                DataSetChangeset actualChanges = new DataSetChangeset(this, precommitOut.ActualChanges, true);
                ClearChanges();

                CaptureCommittedSchema();

                // Firing event DataSet.Committed
                FireEventCommitted(actualChanges, committedSchema);
            }
        }

        /// <summary>
        /// Commits custom changes.
        /// </summary>
        /// <param name="proposedChanges"></param>
        protected void CommitCustomChanges(DataSet.Changes proposedChanges)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("DataSet");
            if (IsReadOnly)
                throw new ReadOnlyException("DataSet is read only.");
            if (proposedChanges == null)
                return;

            // No more changes can be done from now, so we're preparing
            // changeset to apply:
            PrecommitOutput precommitOut;
            if (IsLinked)
                precommitOut = DistributedCommitChanges(proposedChanges);
            else
                precommitOut = ApplyChanges(proposedChanges);

            CaptureCommittedSchema();

            // Firing event DataSet.Committed
            FireEventCommitted(
                new DataSetChangeset(this, precommitOut.ActualChanges, true),
                GetSchema(SchemaVersion.Committed));
        }

        /// <summary>
        /// Applies given changeset to the data set.
        /// The process includes both precommit and final commit stages.
        /// </summary>
        /// <param name="proposedChanges"></param>
        private PrecommitOutput ApplyChanges(Changes proposedChanges)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("DataSet");
            if (proposedChanges == null)
                throw new DataSetException("DataSet " + URI + " has no changes");

            PrecommitOutput precommitOutput = Precommit(proposedChanges);
            FinalCommit(precommitOutput);
            return precommitOutput;
        }

        /// <summary>
        /// Rolls back all changes.
        /// </summary>
        /// <remarks>Throws no exceptions.</remarks>
        public void Rollback()
        {
            lock (this)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException("DataSet");
                if (IsReadOnly)
                    throw new ReadOnlyException("DataSet is read only.");

                if (!HasChanges) return;

                if (IsLinked)
                    RollbackLinkedDataSets();
                else
                    RollbackCore();
            }
        }

        /// <summary>
        /// Implements rollback mechanism for this DataSet (not distributed).
        /// </summary>
        protected virtual void RollbackCore()
        {
            lock (this)
            {
                try
                {
                    rollingBack = true;

                    ICollection<Variable> vars = GetRecentVariables();

                    foreach (Variable var in variables)
                    {
                        try
                        {
                            var.Rollback();
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLineIf(DataSet.TraceDataSet.TraceError, "Rollback has errors: " + ex.Message);
                        }
                    }

                    /* Rolling back new variables and removing them from the Data Set */
                    if (changes.Variables != null)
                    {
                        foreach (Variable var in changes.Variables)
                        {
                            if (!variables.Contains(var)) // this variable was added as a part of this changeset
                            {
                                var.Changed -= OnVariableChanged;
                                var.CoordinateSystemAdded -= OnVariableCoordinateSystemAdded;
                                try
                                {
                                    var.Rollback();
                                    var.Dispose();
                                }
                                catch (Exception ex)
                                {
                                    Trace.WriteLineIf(DataSet.TraceDataSet.TraceError, "Rollback has error: " + ex.Message);
                                }
                            }
                        }

                        if (outcomingReferences != null)
                        {
                            foreach (var link in outcomingReferences)
                            {
                                if (!link.Committed)
                                    link.TargetDataSet.RemoveIncomingReference(link);
                            }
                            outcomingReferences.Rollback();
                        }
                    }
                    try
                    {
                        OnRollback();
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLineIf(DataSet.TraceDataSet.TraceError, "Rollback has errors: " + ex.Message);
                        Debug.WriteLineIf(DataSet.TraceDataSet.TraceError, ex.StackTrace);
                    }

                    if (HasChanges)
                        ClearChanges();
                }
                finally
                {
                    rollingBack = false;
                }

                FireEventRolledback();
            }
        }

        /// <summary>
        /// Makes precommit procedure for every variable of the data set and
        /// returns recent dimension list.
        /// </summary>
        /// <exception cref="ConstraintsFailedException"/>
        private PrecommitOutput Precommit(Changes proposedChanges)
        {
            if (proposedChanges == null)
                throw new ArgumentNullException("proposedChanges");

            committedSchema = null;
            ICollection<Variable> vars = null;
            ReadOnlyDimensionList updatedDimensions = null;
            try
            {
                // Copying and modifying proposedChanges:
                // all appended data pieces transforms into put pieces.
                proposedChanges = proposedChanges.Clone();

                vars = proposedChanges.Variables;
                TransformAppendToPut(proposedChanges);

                updatedDimensions = CheckConstraints(proposedChanges);

                /* Precommitting variables */
                /* Checking external constraints */
                if (!FireEventCommitting(proposedChanges))
                    throw new CannotPerformActionException("Committing is cancelled.");

                if (!OnPrecommitting(proposedChanges))
                    throw new CannotPerformActionException("Committing is cancelled.");

                foreach (Variable v in vars)
                {
                    v.Precommit(proposedChanges);
                }

                OnPrecommit(proposedChanges);
            }
            catch
            {
                updatedDimensions = null;
                UndoPrecommit(proposedChanges);
                throw;
            }
            finally
            {
            }

            PrecommitOutput output = new PrecommitOutput();
            output.ActualChanges = proposedChanges;
            output.UpdatedDimensions = updatedDimensions;

            return output;
        }

        private void UndoPrecommit(Changes proposedChanges)
        {
            if (proposedChanges != null && proposedChanges.Variables != null)
            {
                foreach (Variable v in proposedChanges.Variables)
                    v.ResetCommitStage();
            }
            Undo();
        }

        private void UndoPrecommitForLocalChanges()
        {
            UndoPrecommit(this.changes);
        }

        /// <summary>
        /// Performs final committing procedure and is called after all changes have been succeeded.
        /// </summary>
        private void FinalCommit(PrecommitOutput precommitOutput)
        {
            CommitAllChanges(precommitOutput);
            OnCommit();

            commitedDimensions = precommitOutput.UpdatedDimensions;

            if (!rollingBack)
                changeSetId++;
        }

        /// <summary>
        /// Transforms all appended data pieces transforms into put pieces.
        /// </summary>
        /// <param name="proposedChanges"></param>
        protected static void TransformAppendToPut(Changes proposedChanges)
        {
            foreach (var var in proposedChanges.Variables)
            {
                Variable.Changes vc = proposedChanges.GetVariableChanges(var.ID);
                if (vc == null) continue;

                vc = vc.Clone();
                TransformAppendToPut(vc);
                proposedChanges.UpdateChanges(vc);
            }
        }

        /// <summary>
        /// Transforms all appended data pieces transforms into put pieces
        /// </summary>
        /// <param name="proposedChanges"></param>
        private static void TransformAppendToPut(Variable.Changes proposedChanges)
        {
            /* Changing state of the variable */
            Variable.DataChanges dataChanges = proposedChanges as Variable.DataChanges;
            if (dataChanges != null && dataChanges.HasData)
            {
                int[] shape = dataChanges.InitialSchema.Dimensions.AsShape();
                int[] cshape = (int[])shape.Clone();
                int rank = shape.Length;
                Rectangle affected = new Rectangle(rank);
                bool affectedInitialized = false;

                for (int j = 0; j < dataChanges.Data.Count; j++)
                {
                    Variable.DataPiece piece = dataChanges.Data[j];
                    piece.Origin = Variable.DataPiece.UpdateAppendedOrigin(piece.Origin, shape);

                    // Updating shape
                    for (int i = 0; i < rank; i++)
                    {
                        int s = piece.Origin[i] + piece.Data.GetLength(i);
                        if (s > shape[i])
                            shape[i] = s;
                    }
                    // Updating affected rectangle
                    if (!affectedInitialized)
                    {
                        for (int i = 0; i < rank; i++)
                        {
                            affected.Origin[i] = piece.Origin[i];
                            affected.Shape[i] = piece.Data.GetLength(i);
                        }
                        affectedInitialized = true;
                    }
                    else // if there is a proposed data, updating affected rectangle on its basis
                    {
                        for (int i = 0; i < rank; i++)
                        {
                            int max = affected.Shape[i] + affected.Origin[i];
                            int pieceMax = piece.Origin[i] + piece.Data.GetLength(i);

                            affected.Origin[i] = Math.Min(affected.Origin[i], piece.Origin[i]);

                            if (max < pieceMax)
                                max = pieceMax;
                            affected.Shape[i] = max - affected.Origin[i];
                        }
                    }

                    dataChanges.Data[j] = piece;
                }

                ExtendAffectedRectangle(affected, cshape);

                // Updating shape & affected rectangle to real values.
                dataChanges.Shape = shape;
                dataChanges.AffectedRectangle = affected;
            }
        }

        internal static void ExtendAffectedRectangle(Rectangle ar, int[] shape)
        {
            int rank = shape.Length;

            // If new data is placed outside existing shape, it can implicitly update more area.
            int n = 0;
            for (int i = 0; i < rank && n < 2; i++)
            {
                if (ar.Origin[i] + ar.Shape[i] > shape[i])
                    n++;
            }
            if (n == 1) // one dimension is greater than existing
            {
                for (int i = 0; i < rank; i++)
                {
                    int end = ar.Origin[i] + ar.Shape[i];
                    if (end > shape[i])
                    {
                        ar.Origin[i] = Math.Min(ar.Origin[i], shape[i]);
                        ar.Shape[i] = end - ar.Origin[i];
                    }
                    else
                    {
                        ar.Origin[i] = 0;
                        ar.Shape[i] = shape[i];
                    }
                }
            }
            else if (n > 1) // several dimensions greater than existing
            {
                for (int i = 0; i < rank; i++)
                {
                    int end = ar.Origin[i] + ar.Shape[i];
                    ar.Origin[i] = 0;
                    ar.Shape[i] = Math.Max(end, shape[i]);
                }
            }
        }

        /// <summary>
        /// Checks all constraints for given changeset and if
        /// the check failed, throws an exception. 
        /// No events are thrown during the process.
        /// </summary>
        /// <param name="changeset"></param>
        /// <returns></returns>
        protected void CheckChangeset(DataSet.Changes changeset)
        {
            if (changeset == null) return;
            CheckConstraints(changeset);
            foreach (Variable v in changeset.Variables)
            {
                v.CheckChangeset(changeset);
            }
        }

        /// <summary>
        /// Forces dependant variables to update their changes
        /// (possibly, take it from their source variables).
        /// </summary>
        /// <param name="proposedChanges"></param>
        private static void UpdateChanges(Changes proposedChanges)
        {
            foreach (var var in proposedChanges.Variables)
            {
                if (var is IDependantVariable)
                    ((IDependantVariable)var).UpdateChanges(proposedChanges);
            }
        }

        /// <summary>
        /// Checks global contraints on variables and coordinate systems
        /// </summary>
        /// <param name="proposedChanges"></param>
        private ReadOnlyDimensionList CheckConstraints(Changes proposedChanges)
        {
            UpdateChanges(proposedChanges);

            /* Checking constraints on variables */
            var updatedDimensions = CheckComplementarityConstraints(proposedChanges);

            /* Checking constraints on coordinate systems */
            ICollection<CoordinateSystem> coordinates = proposedChanges.CoordinateSystems == null ?
                (ICollection<CoordinateSystem>)csystems : proposedChanges.CoordinateSystems;
            foreach (CoordinateSystem cs in coordinates)
            {
                cs.CheckConstraints(proposedChanges);
            }
            return updatedDimensions;
        }


        private void CommitAllChanges(PrecommitOutput precommitOutput)
        {
            DataSet.Changes proposedChanges = precommitOutput.ActualChanges;
            ICollection<Variable> vars = GetRecentVariables(proposedChanges);

            lock (this)
            {
                foreach (Variable v in vars)
                {
                    v.Changed -= OnVariableChanged;
                    v.Changing -= OnVariableChanging;
                }

                try
                {
                    foreach (Variable v in vars)
                    {
                        v.FinalCommit(proposedChanges.GetVariableChanges(v.ID));
                    }

                    if (outcomingReferences != null)
                        outcomingReferences.CommitLinks();
                }
                finally
                {
                    foreach (Variable v in vars)
                    {
                        v.Changed += OnVariableChanged;
                        v.Changing += OnVariableChanging;
                    }
                }
            }

            /* Commits structure of SDS */
            ICollection<CoordinateSystem> cs = proposedChanges.CoordinateSystems == null ?
                (ICollection<CoordinateSystem>)csystems :
                (ICollection<CoordinateSystem>)proposedChanges.CoordinateSystems;
            foreach (CoordinateSystem s in cs)
            {
                if (s.HasChanges)
                    s.Commit();
            }

            // Committing inner SDS structures
            if (proposedChanges.Variables != null)
            {
                proposedChanges.Variables.CollectionChanged -= OnVariableCollectionChanged;
                this.variables = new ReadOnlyVariableCollection(proposedChanges.Variables);
            }
            if (proposedChanges.CoordinateSystems != null)
            {
                this.csystems = proposedChanges.CoordinateSystems.GetReadOnlyCollection();
            }
        }

        /// <summary>
        /// Returns the recent collection of the variables.
        /// </summary>
        /// <remarks> 
        /// When the <see cref="DataSet"/> is committed it is committed collection of variables,
        /// when proposed it is proposed and committed variables.</remarks>
        protected ICollection<Variable> GetRecentVariables()
        {
            lock (this)
            {
                ICollection<Variable> vars = (changes == null || changes.Variables == null) ?
                    (ICollection<Variable>)variables :
                    (ICollection<Variable>)changes.Variables;
                return vars;
            }
        }

        /// <summary>
        /// Returns the collection of the variables including committed and added variables
        /// from proposed changes.
        /// </summary>
        /// <param name="proposedChanges"></param>
        protected ICollection<Variable> GetRecentVariables(Changes proposedChanges)
        {
            if (proposedChanges == null || proposedChanges.Variables == null ||
                proposedChanges.Variables.Count == 0)
                return variables;

            return proposedChanges.Variables;
        }

        /// <summary>
        /// Starts the state changing transaction.
        /// </summary>
        /// <remarks>
        /// <para>
        /// It is used to accumulate changes from public data set API.
        /// </para>
        /// <para>
        /// If the changes has already been started, the method does nothing.
        /// </para>
        /// </remarks>
        protected void StartChanges()
        {
            if (!HasChanges)
            {
                DataSetSchema initialSchema = GetSchema(SchemaVersion.Committed);

                lock (this)
                {
                    changes = new Changes(
                        this,
                        initialSchema.Version + 1,
                        initialSchema,
                        new VariableCollection(variables.All),
                        new CoordinateSystemCollection(csystems),
                        ChangesetSource.Local);
                    changes.Variables.CollectionChanged += OnVariableCollectionChanged;
                }

                OnTransactionOpened();
            }
        }

        /// <summary>
        /// Checks constraints when the variable has been committed internally, i.e. in local Commit() call.
        /// </summary>
        private ReadOnlyDimensionList CheckComplementarityConstraints(Changes proposedChanges)
        {
            /**********************************************/
            ICollection<Variable> vars = GetRecentVariables(proposedChanges);

            /**********************************************
            * Data Model constraints
            * */
            bool success = true;
            Dictionary<string, Dimension> lengths = new Dictionary<string, Dimension>();
            foreach (Variable v in vars)
            {
                Variable.Changes vc = proposedChanges.GetVariableChanges(v.ID);
                v.DoCheckConstraints(proposedChanges);

                ReadOnlyDimensionList proposed = vc == null ? v.Dimensions : vc.GetDimensionList();
                for (int j = 0; j < proposed.Count; j++)
                {
                    Dimension d = proposed[j];
                    if (lengths.ContainsKey(d.Name))
                    {
                        if (d.Length != lengths[d.Name].Length)
                        {
                            success = false;
                            lengths[d.Name] = new Dimension(d.Name, -1);
                        }
                    }
                    else
                    {
                        lengths[d.Name] = d;
                    }
                }
            }

            if (!success)
            {
                StringBuilder sb = new StringBuilder();
                int n = 0;
                foreach (var dim in lengths)
                {
                    if (dim.Value.Length == -1)
                    {
                        sb.Append(dim.Key).Append(" ");
                        n++;
                    }
                }
                Trace.WriteLineIf(DataSet.TraceDataSet.TraceWarning, "Dimensions " + sb.ToString() + "differ");
                if (n == 1)
                    throw new ConstraintsFailedException("Dimension " + sb.ToString() + "differs");
                throw new ConstraintsFailedException("Dimensions " + sb.ToString() + "differ");
            }

            return new ReadOnlyDimensionList(lengths.Values.ToList());
        }

        #region Overridables enabling adding customs steps to commit process

        /// <summary>
        /// The method is called when new transaction is started.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The method is called when a first change has been made in committed data set or its dependencies.
        /// </para>
        /// <para>
        /// Caution: this method can be invoked from the DataSet constructor to add a metadata global variable,
        /// until the provider type's constructor is called. 
        /// This case must be supported, for example, by using "isInitialized" field to find out that the method 
        /// is called from the constructor and, possibly, do nothing in that case.
        /// </para>
        /// </remarks>
        protected virtual void OnTransactionOpened()
        {
        }

        /// <summary>
        /// The method is called at the beggining of the precommit stage of the <see cref="DataSet"/>.
        /// </summary>
        /// <remarks>
        /// The method is called at the beggining of the precommit stage of the <see cref="DataSet"/>, right before individual 
        /// precommit calls for every its variable, but after the Committing event has fired. 
        /// At Precommit stage it is required to change the state.
        /// Override it if you want to perform precommit stage for the data set on your own way.
        /// For example, it is possible to make actual change of the variables' state simultaneously here.
        /// On errors an exception should be thrown.
        /// </remarks>
        /// <param name="changes">Proposed changes</param>
        /// <returns>True to continue committing; False to cancel.</returns>
        /// <exception cref="ConstraintsFailedException"/>
        protected virtual bool OnPrecommitting(DataSet.Changes changes)
        {
            return true;
        }

        /// <summary>
        ///  The method is called at the end of the precommit stage of the <see cref="DataSet"/>.
        /// </summary>
        /// <remarks>
        /// The method is called at the end of the precommit stage of the <see cref="DataSet"/>, right after individual 
        /// precommit calls for every its variable. 
        /// At Precommit stage it is required to change the state.
        /// Override it if you want to perform precommit stage for the <see cref="DataSet"/> on your own way.
        /// For example, it is possible to make actual change of the variables' state simultaneously here.
        /// On errors an exception should be thrown.
        /// </remarks>
        /// <param name="changes">Proposed changes</param>
        /// <exception cref="ConstraintsFailedException"/>
        protected virtual void OnPrecommit(DataSet.Changes changes)
        {
        }

        /// <summary>
        /// The method is called at the end of the commit stage of the <see cref="DataSet"/>.
        /// </summary>
        /// <remarks>
        /// The method is called at the end of the commit stage of the <see cref="DataSet"/>, right after individual 
        /// commit calls for every variable. 
        /// At Commit stage it is required to commit the change of state.
        /// Override it if you want to perform commit stage for the <see cref="DataSet"/> on your own way.
        /// For example, it is possible to commit actual change of the variables' state simultaneously here.
        /// </remarks>
        protected virtual void OnCommit()
        {
        }

        /// <summary>
        /// The method is called when the precommit fails. Providers can cancel possible preparations for committed.
        /// </summary>
        protected virtual void Undo()
        {
        }

        /// <summary>
        /// The method is called at the end of the rollback stage of the <see cref="DataSet"/>.
        /// </summary>
        /// <remarks>
        /// The method is called at the end of the rollback stage of the <see cref="DataSet"/>, right after individual 
        /// rollback calls for every variable. 
        /// At Rollback stage it is required to rollback the change of state.
        /// Override it if you want to perform rollback stage for the <see cref="DataSet"/> on your own way.
        /// For example, it is possible to rollback actual change of the variables' state simultaneously here.
        /// </remarks>
        protected virtual void OnRollback()
        {
        }

        #endregion

        #region Distributed operations

        /// <summary>
        /// Makes distributed rollback operations for linked data sets.
        /// </summary>
        private void RollbackLinkedDataSets()
        {
            lock (this)
            {
                Trace.WriteLineIf(DataSet.TraceDataSet.TraceInfo, "Distributed rollback started.");
                List<DataSet> dataSets = GetActiveLinkedDataSet();
                Trace.WriteLineIf(DataSet.TraceDataSet.TraceVerbose, dataSets.Count + " DataSets involved in the rollback.");

                foreach (var ds in dataSets)
                {
                    Debug.WriteIf(DataSet.TraceDataSet.TraceVerbose, "Rolling back " + ds.URI + " . . . ");
                    ds.RollbackCore();
                    Debug.WriteLineIf(DataSet.TraceDataSet.TraceVerbose, "Done.");
                }
                Trace.WriteLineIf(DataSet.TraceDataSet.TraceInfo, "Distributed rollback succeeded.");
            }
        }

        /// <summary>
        /// Makes distributed commit;
        /// given changes are for this data set;
        /// all others do commit their local changes.
        /// </summary>
        /// <param name="proposedChangesForMe">Changes for the DataSet initiated the commit.</param>
        private PrecommitOutput DistributedCommitChanges(Changes proposedChangesForMe)
        {
            UpdateChanges(proposedChangesForMe);

            Trace.WriteLineIf(DataSet.TraceDataSet.TraceInfo, "Distributed commit started via " + URI);
            List<DataSet> dataSets = GetActiveLinkedDataSet(proposedChangesForMe);
            Trace.WriteLineIf(DataSet.TraceDataSet.TraceVerbose, dataSets.Count + " DataSets involved in the commit.");

            /**************************************************************
             * Commit Phase 1: Precommitting all data sets
             * ***********************************************************/
            Dictionary<DataSet, PrecommitOutput> precommits = new Dictionary<DataSet, PrecommitOutput>();
            for (int i = 0; i < dataSets.Count; i++)
            {
                DataSet ds = dataSets[i];
                try
                {
                    Debug.WriteIf(DataSet.TraceDataSet.TraceVerbose, "Precommitting DataSet " + ds.URI + " . . . ");
                    if (ds == this)
                    {
                        precommits[ds] = Precommit(proposedChangesForMe);
                    }
                    else
                    {
                        precommits[ds] = ds.PrecommitLocalChanges();
                    }
                    Debug.WriteLineIf(DataSet.TraceDataSet.TraceVerbose, "Done.");
                }
                catch (Exception ex)
                {
                    Trace.WriteLineIf(DataSet.TraceDataSet.TraceWarning, "Precommit failed for DataSet " + ds.URI);
                    for (int j = 0; j <= i; j++)
                    {
                        if (ds == this)
                            dataSets[i].UndoPrecommit(proposedChangesForMe);
                        else
                            dataSets[i].UndoPrecommitForLocalChanges();
                    }
                    throw new DistributedCommitFailedException(ds, ex);
                }
            }

            /**************************************************************
             * Commit Phase 2: Committing all data sets
             * ***********************************************************/
            for (int i = 0; i < dataSets.Count; i++)
            {
                DataSet ds = dataSets[i];
                try
                {
                    Debug.WriteIf(DataSet.TraceDataSet.TraceVerbose, "Committing DataSet " + ds.URI + " . . . ");
                    PrecommitOutput precommitOut = precommits[ds];
                    if (ds == this)
                    {
                        FinalCommit(precommitOut);
                    }
                    else
                    {
                        ds.FinalCommitLocalChanges(precommitOut);
                    }
                    Debug.WriteLineIf(DataSet.TraceDataSet.TraceVerbose, "Done.");
                }
                catch (Exception ex)
                {
                    Trace.WriteLineIf(DataSet.TraceDataSet.TraceError, "Final commit failed for DataSet " + ds.URI);
                    throw new DistributedCommitFailedException(ds, ex);
                }
            }

            /**************************************************************
             * Commit Phase 3: Capturing committed schemas
             * ***********************************************************/
            for (int i = 0; i < dataSets.Count; i++)
            {
                DataSet ds = dataSets[i];
                if (ds == this) continue;
                try
                {
                    ds.CaptureCommittedSchema();
                }
                catch (Exception)
                {
                    Trace.WriteLineIf(DataSet.TraceDataSet.TraceWarning, "Exception during DataSet.CaptureCommittedSchema for " + ds.URI);
                    throw;
                }
            }

            /**************************************************************
             * Commit Phase 4: Firing events
             * ***********************************************************/
            for (int i = 0; i < dataSets.Count; i++)
            {
                DataSet ds = dataSets[i];
                if (ds == this) continue;
                PrecommitOutput precommitOut = precommits[ds];
                try
                {
                    ds.FireEventCommitted(
                        new DataSetChangeset(ds, precommitOut.ActualChanges),
                        ds.GetSchema(SchemaVersion.Committed));
                }
                catch (Exception)
                {
                    Trace.WriteLineIf(DataSet.TraceDataSet.TraceWarning, "Exception during DataSet.Committed for " + ds.URI);
                    throw;
                }
            }

            Trace.WriteLineIf(DataSet.TraceDataSet.TraceInfo, "Distributed commit succeeded.");
            return precommits[this];
        }

        /// <summary>
        /// Gets DataSets those are linked with this <see cref="DataSet"/>.
        /// </summary>
        /// <returns>An array of DataSets.</returns>
        /// <remarks>
        /// <para>If the <see cref="DataSet"/> is linked (see <see cref="IsLinked"/>),
        /// this method returns an array of DataSets those have references to this <see cref="DataSet"/> or
        /// targets of references contained in this <see cref="DataSet"/>, and
        /// recursively for those DataSets. Also, the returned array always contains
        /// this <see cref="DataSet"/>.
        /// </para>
        /// <para>If the <see cref="DataSet"/> is not linked, the returned array contains one element - this <see cref="DataSet"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="IsLinked"/>
        public DataSet[] GetLinkedDataSets()
        {
            // Building list of DataSets involved in the commit
            List<DataSet> dataSets = new List<DataSet>();
            Queue<DataSet> dataSetsToProcess = new Queue<DataSet>();
            dataSetsToProcess.Enqueue(this);

            do
            {
                DataSet current = dataSetsToProcess.Dequeue();
                dataSets.Add(current);

                if (current.outcomingReferences != null)
                    foreach (DataSetLink refDs in current.outcomingReferences)
                    {
                        if (dataSets.Contains(refDs.TargetDataSet) || dataSetsToProcess.Contains(refDs.TargetDataSet))
                            continue; // it is alread processed or is to be processed
                        dataSetsToProcess.Enqueue(refDs.TargetDataSet);
                    }
                if (current.incomingReferences != null)
                    lock (current.incomingReferences)
                    {
                        foreach (DataSetLink refDs in current.incomingReferences)
                        {
                            bool contained = dataSets.Contains(refDs.SourceDataSet) || dataSetsToProcess.Contains(refDs.SourceDataSet);
                            if (contained) continue; // it is alread processed or is to be processed
                            dataSetsToProcess.Enqueue(refDs.SourceDataSet);
                        }
                    }
            } while (dataSetsToProcess.Count != 0);
            return dataSets.ToArray();
        }

        /// <summary>
        /// Builds the list of data sets linked to this data sets with active links
        /// (i.e. via variables with changes).
        /// </summary>
        /// <returns></returns>
        private List<DataSet> GetActiveLinkedDataSet()
        {
            return GetActiveLinkedDataSet(null);
        }

        /// <summary>
        /// Builds the list of data sets linked to this data sets with active links
        /// (i.e. via variables with changes).
        /// </summary>
        /// <returns></returns>
        private List<DataSet> GetActiveLinkedDataSet(Changes proposedChangesForMe)
        {
            // All involved DataSets should have AutocommitEnabled = false
            var allDS = GetLinkedDataSets();
            bool[] autocommitEnabled = new bool[allDS.Length];
            for (int i = 0; i < allDS.Length; i++)
            {
                autocommitEnabled[i] = allDS[i].IsAutocommitEnabled;
                allDS[i].IsAutocommitEnabled = false;
            }

            List<DataSet> dataSets = new List<DataSet>();
            try
            {
                // Building list of DataSets involved in the commit
                Queue<DataSet> dataSetsToProcess = new Queue<DataSet>();
                dataSetsToProcess.Enqueue(this);

                do
                {
                    DataSet current = dataSetsToProcess.Dequeue();
                    dataSets.Add(current);

                    if (current.outcomingReferences != null)
                        foreach (DataSetLink refDs in current.outcomingReferences)
                        {
                            if (refDs.TargetDataSet.IsDisposed)
                                throw new ObjectDisposedException("Referred DataSet is disposed");
                            if (!refDs.TargetDataSet.HasChanges) continue;
                            if (!refDs.IsActive) continue;

                            if (dataSets.Contains(refDs.TargetDataSet) || dataSetsToProcess.Contains(refDs.TargetDataSet))
                                continue; // it is alread processed or is to be processed

                            dataSetsToProcess.Enqueue(refDs.TargetDataSet);
                        }
                    if (current.incomingReferences != null)
                        lock (current.incomingReferences)
                        {
                            foreach (DataSetLink refDs in current.incomingReferences)
                            {
                                if (refDs.SourceDataSet.IsDisposed) continue;

                                bool contained = dataSets.Contains(refDs.SourceDataSet) || dataSetsToProcess.Contains(refDs.SourceDataSet);

                                Variable rvar = refDs.Reference;
                                Variable tvar = refDs.Target;

                                Variable.Changes customChanges;
                                if (current == this)
                                    customChanges = proposedChangesForMe != null ?
                                        proposedChangesForMe.GetVariableChanges(tvar.ID) : null;
                                else
                                    customChanges = tvar.GetInnerChanges();

                                //if (refDs.DataSet.HasChanges && customChanges != null && !contained)
                                //	continue; // we shouldn't touch it with custom changes as it is being changed locally

                                if (!rvar.HasChanges && !tvar.HasChanges && customChanges == null)
                                    continue;

                                if (customChanges != null &&
                                    !(refDs.SourceDataSet.HasChanges && customChanges != null && !contained))
                                    ((IRefVariable)rvar).UpdateChanges(customChanges);

                                if (contained)
                                    continue; // it is alread processed or is to be processed

                                dataSetsToProcess.Enqueue(refDs.SourceDataSet);
                            }
                        }
                } while (dataSetsToProcess.Count != 0);
            }
            finally
            {
                for (int i = 0; i < allDS.Length; i++)
                    allDS[i].IsAutocommitEnabled = autocommitEnabled[i];
            }
            return dataSets;
        }

        private PrecommitOutput PrecommitLocalChanges()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(this.URI);
            if (!HasChanges) throw new DataSetException("DataSet " + URI + " has no changes");

            // Obtaining changes from variables
            foreach (var var in changes.Variables)
            {
                if (var.HasChanges)
                    changes.UpdateChanges(var.GetInnerChanges());
            }

            return Precommit(changes);
        }

        private void FinalCommitLocalChanges(PrecommitOutput precommitOutput)
        {
            FinalCommit(precommitOutput); // produces actual changes
            ClearChanges();
        }

        #endregion

        #endregion

        #region Computational Variables

        /// <summary>
        /// Creates a stridden variable and adds it to the <see cref="DataSet"/>.
        /// </summary>
        /// <typeparam name="DataType">Type of data element.</typeparam>
        /// <param name="name">Name of the output variable. <c>null</c> is supported.</param>
        /// <param name="dimensions">Names of stridden dimensions or null.</param>
        /// <param name="raw">Source variable to stride.</param>
        /// <param name="origin">Starting indices. <c>null</c> means all zeros.</param>
        /// <param name="stride">Stride indices, must contain values greater or equal to 0. <c>null</c> means all 1.</param>
        /// <param name="count">A number of values in a stridden variable. count[i] == 0 means unlimited i-th dimension,
        /// <c>null</c> means all zeros.</param>
        /// <param name="hiddenEntries">Collection of attributes which are not to be inherited from the underlying metadata.
        /// These entries are changed independently by source and stridden variable.</param>
        /// <param name="readOnlyEntries">Collection of attributes of a source variable that cannot be changed through this collection.</param>
        /// <returns>A stridden variable.</returns>
        /// <remarks>
        /// <para>
        /// See <see cref="DataSet.StrideVariable{DataType}(Variable{DataType},int[],int[],int[],string,IList{string},IList{string})"/>
        /// for details. This method additionally enables specification names of output (stridden) dimensions through
        /// <paramref name="dimensions"/>. If <paramref name="dimensions"/> is null, output dimensions are named automatically.
        /// </para>        
        /// </remarks>
        /// <seealso cref="ScaleVariable{DataType,RawType}(Variable{RawType},DataType,DataType,string)"/>
        /// <seealso cref="TransformVariable{DataType,RawType}(Variable{RawType},Func{RawType,DataType},Func{DataType,RawType},string)"/>
        /// <seealso cref="CreateIndexTransformVariable{DataType}"/>
        /// <seealso cref="CreateRangeVariable"/>
        public Variable<DataType> StrideVariable<DataType>(Variable<DataType> raw, int[] origin, int[] stride, int[] count, string name, string[] dimensions,
            IList<string> hiddenEntries, IList<string> readOnlyEntries)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("DataSet");
            if (IsReadOnly)
                throw new ReadOnlyException("DataSet is read only.");
            if (raw == null)
                throw new ArgumentNullException("raw");
            int rawRank = raw.Rank;
            if (rawRank == 0)
                throw new NotSupportedException("Striding is not supported by scalar variables");

            if (origin == null)
            {
                origin = new int[rawRank]; // filled with zeros
            }
            if (stride == null)
            {
                stride = new int[rawRank];
                for (int i = 0; i < rawRank; i++) stride[i] = 1;
            }
            if (count == null)
            {
                count = new int[rawRank];
                for (int i = 0; i < rawRank; i++)
                    if (stride[i] == 0) count[i] = 1; // else equals to 0
            }

            int n = 0; // rank of the stridden variable
            for (int i = 0; i < rawRank; i++)
                if (stride[i] > 0) n++;
            if (dimensions != null && dimensions.Length != n)
                throw new ArgumentException("Wrong number of dimensions given");

            string[] dims = new string[n];

            int k = 0;
            for (int i = 0; i < rawRank; i++)
                if (stride[i] > 0)
                {
                    string dname = raw.Dimensions[i].Name;
                    // StrideVariable depends on the same dimension (i.e. it doesn't create new strided dimensions),
                    // if start=0, stride=1, count=0, i.e. the dimension is not strided.
                    if (origin[i] == 0 && stride[i] == 1 && count[i] == 0)
                    {
                        dims[k] = dname;
                    }
                    else
                    {
                        if (dimensions != null)
                            dims[k] = dimensions[k];
                        else
                            dims[k] = "_stride_" + dname + "_" + Guid.NewGuid();
                    }
                    k++;
                }

            if (name == null)
                name = raw.Name;
            StriddenVariable<DataType> sv = new StriddenVariable<DataType>(this, name, dims, raw, origin, stride, count, hiddenEntries, readOnlyEntries);
            AddVariableToCollection(sv);

            return sv;
        }

        /// <summary>
        /// Creates a stridden variable and adds it to the <see cref="DataSet"/>.
        /// </summary>
        /// <typeparam name="DataType">Type of data element.</typeparam>
        /// <param name="name">Name of the output variable. <c>null</c> is supported.</param>
        /// <param name="raw">Source variable to stride.</param>
        /// <param name="origin">Starting indices. <c>null</c> means all zeros.</param>
        /// <param name="stride">Stride indices, must contain values greater or equal to 0. <c>null</c> means all 1.</param>
        /// <param name="count">A number of values in a stridden variable. count[i] == 0 means unlimited i-th dimension,
        /// <c>null</c> means all zeros.</param>
        /// <param name="hiddenEntries">Collection of attributes which are not to be inherited from the underlying metadata.
        /// These entries are changed independently by source and stridden variable.</param>
        /// <param name="readOnlyEntries">Collection of attributes of a source variable that cannot be changed through this collection.</param>
        /// <returns>A stridden variable.</returns>
        /// <remarks>
        /// <para>
        /// The method creates a stridden variable, implementing the interface
        /// <see cref="IStriddenVariable"/>, and returns it. 
        /// The stridden variable has the same type as the <paramref name="raw"/> does.
        /// The values of that variable are taken from <paramref name="raw"/>.</para>
        /// <para>
        /// Subsetting parameters for a stridden variable are: <paramref name="origin"/>, <paramref name="stride"/>,
        /// <paramref name="count"/>. They are defined when a variable is created.
        /// Length of the parameters must be equal to the rank of the source variable.
        /// </para>
        /// <para>
        /// <list type="bullet">
        /// <item><description>origin == null: all start indices are zeros;</description></item>
        /// <item><description>count == null: all dimensions of a stridden variable are unlimited and depend on the source variable;</description></item>
        /// <item><description>stride == null: stride is 1 for all dimensions;</description></item>
        /// <item><description>count[i] == 0: i-th dimension is unlimited and always is equal to the length of the source variable;</description></item>
        /// <item><description>stride[j] == 0: removes dimension j from result, corresponding count[j] must be 1.</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// The method creates for a stridden variable new dimensions with unique complex name,
        /// except for the case when origin=0, stride=1, count=0 (i.e. the dimension is not strided); in this case
        /// the stridden variable depends on the same dimension as the source variable does.
        /// </para>
        /// <para>
        /// Parameters of the striding are reflected in the <see cref="Variable.Metadata"/> of the stridden variable.
        /// Attribute names are available through const fields of the class <see cref="StriddenVariableKeys"/>.
        /// Metadata attribute "indexSpaceOrigin" of type int[] describes starting indices for stridden variable.
        /// <list type="bullet">
        /// <item><description>change of the origin parameter must not lead to change of the stridden variable's shape. 
        /// The reason is that a shape of any variable can grow only. 
        /// Therefore, a stridden variable can grow only if the source variable grows.</description></item>
        /// <item><description>value indexSpaceOrigin[i] is changeable only if corresponding count[i] > 0 
        /// (so that the shape of a stridden variable is fixed and cannot change).</description></item>
        /// </list>
        /// If a value of the indexSpaceOrigin is incorrect, the <see cref="Commit"/> will fail. 
        /// </para>
        /// <para>
        /// Metadata attributes "indexSpaceStride" and "indexSpaceCount" contain arrays of strides and counts 
        /// respectively, thus describing parameters of the striding. 
        /// This are the read only attributes that never change (otherwise they would change the shape).
        /// </para>
        /// <para>
        /// Metadata attribute "indexSpaceSourceDims" contains an array of dimension names of
        /// the source variable, corresponding to origin indices. This is the read only attribute that never changes.
        /// </para>
        /// <para>
        /// Stridden variable implements <see cref="IStriddenVariable"/>.
        /// The <see cref="IStriddenVariable"/> interface can be used to imperatively update the origin parameter.
        /// See method <see cref="IStriddenVariable.SetIndexSpaceOrigin"/>.
        /// </para>
        /// <para>
        /// Changes to the underlying variable are reflected in the stridden variable, but not vice versa.
        /// Hence a stridden variable is always read-only.
        /// </para>
        /// <para>
        /// If the <paramref name="name"/> is null, the name of the output variable will be the same as the name 
        /// of the input variable <paramref name="raw"/>.
        /// </para>
        /// <para>Both source and target variable share the metadata collection.
        /// But some attributes can be separated or markes as read-only for a stridden variable.
        /// See parameters <paramref name="hiddenEntries"/> and <paramref name="readOnlyEntries"/>.
        /// </para>
        /// <example>
        /// <code>
        /// using (DataSet target = OpenDataSet())
        /// {
        ///		target.IsAutocommitEnabled = true;
        ///		
        ///		/* Preparing the data set */
        ///		Variable&lt;int&gt; v1d = target.AddVariable&lt;int&gt;("test1d", "x");
        ///		v1d.PutData(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
        ///		
        ///		Variable&lt;int&gt; sv1d1 = target.StrideVariable(v1d, new int[] { 1 }, new int[] { 2 }, null, null, new string[] { "units" }, null);
        ///		sv1d1.Metadata["units"] = "DegC";
        ///		// Variable sv1d1 also has name "test1d" and depends on new dimension named automatically
        ///		
        ///		Assert.IsTrue(Compare(sv1d1.GetData(), new int[] { 1, 3, 5, 7, 9 })); // true
        ///	}
        /// </code>
        /// <code>
        /// using (DataSet target = CreateDataSet())
        /// {
        ///		target.IsAutocommitEnabled = true;
        ///		
        ///     /* Preparing the data set */
        ///		Variable&lt;int&gt; v2d = target.AddVariable&lt;int&gt;("test2d", new string[]{"x", "y"});
        ///		v2d.PutData(new int[,] { 0, 1, 2}, {4, 5, 6}, {7, 8, 9});
        ///		Variable&lt;int&gt; sv2d1 = target.StrideVariable(v1d, new int[,] { 1, 0 }, new int[] { 0, 1 }, null, null);
        ///		
        ///		// Dimension "x" is removed:
        ///     Assert.IsTrue(Compare(sv2d1.GetData(), new int[] { 4, 5, 6})); // true
        /// }
        /// </code>
        /// <code>
        /// using (DataSet target = OpenDataSet())
        /// {
        ///		target.IsAutocommitEnabled = true;
        /// 
        ///		/* Preparing the data set */
        ///		Variable&lt;int&gt; v1d = target.AddVariable&lt;int&gt;("test1d", "x");
        ///		v1d.PutData(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
        ///		
        ///		Variable&lt;int&gt; sv1d1 = target.StrideVariable(v1d, new int[] { 0 }, new int[] { 2 }, new int[] { 2 }, null);
        ///		
        ///		Assert.IsTrue(Compare(sv1d1.GetData(), new int[] { 0, 2 })); // true
        ///	
        ///		// Changing stride origin through metadata:
        ///		sv1d1.Metadata[StriddenVariableKeys.KeyForOrigin] = new int[] { 2 };
        ///		
        ///		Assert.IsTrue(Compare(sv1d1.GetData(), new int[] { 2, 4 })); // true
        ///		
        ///	}
        /// </code>
        /// </example>
        /// </remarks>
        /// <seealso cref="ScaleVariable{DataType,RawType}(Variable{RawType},DataType,DataType,string)"/>
        /// <seealso cref="TransformVariable{DataType,RawType}(Variable{RawType},Func{RawType,DataType},Func{DataType,RawType},string)"/>
        /// <seealso cref="CreateIndexTransformVariable{DataType}"/>
        /// <seealso cref="CreateRangeVariable"/>
        public Variable<DataType> StrideVariable<DataType>(Variable<DataType> raw, int[] origin, int[] stride, int[] count, string name,
            IList<string> hiddenEntries, IList<string> readOnlyEntries)
        {
            return StrideVariable<DataType>(raw, origin, stride, count, name, null, hiddenEntries, readOnlyEntries);
        }

        /// <summary>
        /// Creates a stridden variable and adds it to the <see cref="DataSet"/>.
        /// </summary>
        /// <typeparam name="DataType">Type of data element.</typeparam>
        /// <param name="name">Name of the output variable. <c>null</c> is supported.</param>
        /// <param name="raw">Source variable to stride.</param>
        /// <param name="origin">Starting indices. <c>null</c> means all zeros.</param>
        /// <param name="stride">Stride indices, must contain values greater or equal to 0. <c>null</c> means all 1.</param>
        /// <param name="count">A number of values in a stridden variable. count[i] == 0 means unlimited i-th dimension
        /// <c>null</c> means all zeros.</param>
        /// <remarks>
        /// Read remarks for <see cref="DataSet.StrideVariable{DataType}(Variable{DataType},int[],int[],int[],string,IList{string},IList{string})"/>.
        /// </remarks>
        /// <seealso cref="DataSet.StrideVariable{DataType}(Variable{DataType},int[],int[],int[],string,IList{string},IList{string})"/>
        /// <seealso cref="ScaleVariable{DataType,RawType}(Variable{RawType},DataType,DataType,string)"/>
        /// <seealso cref="TransformVariable{DataType,RawType}(Variable{RawType},Func{RawType,DataType},Func{DataType,RawType},string)"/>
        /// <seealso cref="CreateIndexTransformVariable{DataType}"/>
        /// <seealso cref="CreateRangeVariable"/>
        public Variable<DataType> StrideVariable<DataType>(Variable<DataType> raw, int[] origin, int[] stride, int[] count, string name)
        {
            return this.StrideVariable<DataType>(raw, origin, stride, count, name, null, null);
        }

        /// <summary>
        /// Creates a scaled variable and adds it to the <see cref="DataSet"/>.
        /// </summary>
        /// <typeparam name="DataType">The type of data after scaling.</typeparam>
        /// <typeparam name="RawType">The type of data of the variable to scale.</typeparam>
        /// <param name="name">The name of the scaled variable.</param>
        /// <param name="raw">The variable to scale.</param>
        /// <param name="scale">The scale factor.</param>
        /// <param name="offset">The offset for scaling.</param>
        /// <returns>The scaled variable.</returns>
        /// <remarks>
        /// <para>
        /// The method creates a scaled variable and returns it.
        /// The scaled variable has the same rank and shape as the <paramref name="raw"/> does.
        /// The values of that variable are linear function of the underlying variable <paramref name="raw"/>.
        /// Changes to the scaled variable propagate to the underlying variable <paramref name="raw"/>.  
        /// Changes to the underlying variable are reflected in the scaled variable.
        /// </para>
        /// <para>Only limited set of primitive blittable type parameters are supported by this generic variable. 
        /// Use <see cref="TransformedVariable{DataType}"/> for other data types.
        /// These types are <see cref="Single"/> and <see cref="Double"/> for <typeparamref name="DataType"/>;
        /// <see cref="Byte"/>, <see cref="SByte"/>, <see cref="Int16"/>, 
        /// <see cref="UInt16"/>, <see cref="Int32"/>, <see cref="UInt32"/>, <see cref="Int64"/>, 
        /// <see cref="UInt64"/>, <see cref="Single"/> and <see cref="Double"/> for <typeparamref name="RawType"/>.
        /// </para>
        /// <para>
        /// The variable <paramref name="raw"/> must belong to the same DataSet.
        /// Otherwise, consider first add a reference to the variable 
        /// (see <see cref="DataSet.AddVariableByValue(Variable,string,string[])"/>).
        /// </para>
        /// <para>Both source and targe variable share the metadata collection.
        /// See <see cref="TransformVariable{DataType,RawType}(Variable{RawType},Func{RawType,DataType},Func{DataType,RawType},IList{string},IList{string},string)"/>
        /// if there is need not to share metadata.
        /// </para>
        /// </remarks>
        /// <example>
        /// In the following example, a scalar variable (see details in remarks for 
        /// <see cref="Variable"/>)
        /// is scaled by the formula <c>2*x - 3</c> and then transformed
        /// to a DateTime. The example also demonstrates two-way changing of the transformed variable and
        /// source scalar.
        /// <code>
        ///	using (DataSet ds = CreateDataSet())
        ///	{
        ///		ds.IsAutocommitEnabled = false;	
        /// 
        ///		DateTime now = DateTime.Now;
        ///
        ///		var scalar = ds.AddVariable&lt;int&gt;("scalar");
        ///		scalar[null] = 5;
        ///		var scale = ds.ScaleVariable&lt;double, int&gt;(scalar, 2, -3, "scaled");
        ///		var trans = ds.TransformVariable&lt;DateTime, double&gt;(scale,
        ///			i =&gt; now.AddDays(i),
        ///			dt =&gt; dt.Subtract(now).TotalDays,
        ///			"date");
        ///
        ///		ds.Commit();
        ///
        ///		Assert.AreEqual(now.AddDays(7), trans[Variable.DefaultIndices]);
        ///		
        ///		trans[Variable.DefaultIndices] = now.AddDays(5);
        ///		ds.Commit();
        ///		
        ///		Assert.AreEqual(4, scalar[Variable.DefaultIndices]);
        ///	}		
        /// </code>
        /// </example>
        /// <seealso cref="StrideVariable{DataType}(Variable{DataType},int[],int[],int[],string)"/>
        /// <seealso cref="TransformVariable{DataType,RawType}(Variable{RawType},Func{RawType,DataType},Func{DataType,RawType},string)"/>
        /// <seealso cref="TransformedVariable{DataType}"/>
        /// <seealso cref="CreateIndexTransformVariable{DataType}"/>
        /// <seealso cref="CreateRangeVariable"/>
        public Variable<DataType> ScaleVariable<DataType, RawType>(Variable<RawType> raw, DataType scale, DataType offset, string name)
            where DataType : IConvertible
            where RawType : IConvertible
        {

            return this.ScaleVariable<DataType, RawType>(raw, scale, offset, name, null, null);
        }

        /// <summary>
        /// Creates a scaled variable and adds it to the <see cref="DataSet"/>.
        /// </summary>
        /// <typeparam name="DataType">The type of data after scaling.</typeparam>
        /// <typeparam name="RawType">The type of data of the variable to scale.</typeparam>
        /// <param name="name">The name of the scaled variable.</param>
        /// <param name="raw">The variable to scale.</param>
        /// <param name="scale">The scale factor.</param>
        /// <param name="offset">The offset for scaling.</param>
        /// <param name="hiddenEntries">Collection of keys which are not to be inherited from the underlying metadata.
        /// These entries are changed independently.</param>
        /// <param name="readOnlyEntries">Collection of keys that cannot be changed through this collection.</param>
        /// <returns>The scaled variable.</returns>
        /// <remarks>
        /// <para>
        /// The method creates a scaled variable and returns it.
        /// The scaled variable has the same rank and shape as the <paramref name="raw"/> does.
        /// The values of that variable are linear function of the underlying variable <paramref name="raw"/>.
        /// Changes to the scaled variable propagate to the underlying variable <paramref name="raw"/>.  
        /// Changes to the underlying variable are reflected in the scaled variable.
        /// </para>
        /// <para>Only limited set of primitive blittable type parameters are supported by this generic variable. 
        /// Use <see cref="TransformedVariable{DataType}"/> for other data types.
        /// These types are <see cref="Single"/> and <see cref="Double"/> for <typeparamref name="DataType"/>;
        /// <see cref="Byte"/>, <see cref="SByte"/>, <see cref="Int16"/>, 
        /// <see cref="UInt16"/>, <see cref="Int32"/>, <see cref="UInt32"/>, <see cref="Int64"/>, 
        /// <see cref="UInt64"/>, <see cref="Single"/> and <see cref="Double"/> for <typeparamref name="RawType"/>.
        /// </para>
        /// <para>
        /// The variable <paramref name="raw"/> must belong to the same DataSet.
        /// Otherwise, consider first add a reference to the variable 
        /// (see <see cref="DataSet.AddVariableByValue(Variable,string,string[])"/>).
        /// </para>
        /// <para>Both source and targe variable share the metadata collection.
        /// See <see cref="TransformVariable{DataType,RawType}(Variable{RawType},Func{RawType,DataType},Func{DataType,RawType},IList{string},IList{string},string)"/>
        /// if there is need not to share metadata.
        /// </para>
        /// </remarks>
        /// <example>
        /// In the following example, a scalar variable (see details in remarks for 
        /// <see cref="Variable"/>)
        /// is scaled by the formula <c>2*x - 3</c> and then transformed
        /// to a DateTime. The example also demonstrates two-way changing of the transformed variable and
        /// source scalar.
        /// <code>
        ///	using (DataSet ds = CreateDataSet())
        ///	{
        ///		ds.IsAutocommitEnabled = false;
        ///		
        ///		DateTime now = DateTime.Now;
        ///
        ///		var scalar = ds.AddVariable&lt;int&gt;("scalar");
        ///		scalar[null] = 5;
        ///		var scale = ds.ScaleVariable&lt;double, int&gt;(scalar, 2, -3, "scaled",new string[] { "units" }, null);
        ///		var trans = ds.TransformVariable&lt;DateTime, double&gt;(scale,
        ///			i =&gt; now.AddDays(i),
        ///			dt =&gt; dt.Subtract(now).TotalDays,
        ///			"date");
        ///     
        ///     scale.Metadata["units"] = "DegC";
        /// 
        ///		ds.Commit();
        ///
        ///		Assert.AreEqual(now.AddDays(7), trans[Variable.DefaultIndices]);
        ///		
        ///		trans[Variable.DefaultIndices] = now.AddDays(5);
        ///		ds.Commit();
        ///		
        ///		Assert.AreEqual(4, scalar[Variable.DefaultIndices]);
        ///	}		
        /// </code>
        /// </example>
        /// <seealso cref="StrideVariable{DataType}(Variable{DataType},int[],int[],int[],string)"/>
        /// <seealso cref="TransformVariable{DataType,RawType}(Variable{RawType},Func{RawType,DataType},Func{DataType,RawType},string)"/>
        /// <seealso cref="TransformedVariable{DataType}"/>
        /// <seealso cref="CreateIndexTransformVariable{DataType}"/>
        /// <seealso cref="CreateRangeVariable"/>
        public Variable<DataType> ScaleVariable<DataType, RawType>(Variable<RawType> raw, DataType scale, DataType offset, string name, IList<string> hiddenEntries, IList<string> readOnlyEntries)
            where DataType : IConvertible
            where RawType : IConvertible
        {
            if (raw.DataSet != this)
                throw new ArgumentException("Raw variable must belong to the DataSet");
            return CreateScaledVariable<DataType, RawType>(raw, scale, offset, name, hiddenEntries, readOnlyEntries);
        }

        /// <summary>
        /// Creates a transformed variable and adds it to the <see cref="DataSet"/>.
        /// </summary>
        /// <typeparam name="DataType">Type of data after the transform.</typeparam>
        /// <typeparam name="RawType">Source variable type of data.</typeparam>
        /// <param name="raw">Variable to transfrom.</param>
        /// <param name="transform">Forward transform function.</param>
        /// <param name="name">Name of the transformed variable.</param>
        /// <returns>The transformed variable.</returns>
        /// <remarks>
        /// See remarks for 
        /// <see cref="TransformVariable{DataType,RawType}(Variable{RawType},Func{RawType,DataType},Func{DataType,RawType},string)"/>.
        /// </remarks>
        /// <seealso cref="CreateIndexTransformVariable{DataType}"/>
        /// <seealso cref="CreateRangeVariable"/>
        /// <seealso cref="TransformVariable{DataType,RawType}(Variable{RawType},Func{RawType,DataType},Func{DataType,RawType},string)"/>
        public Variable<DataType> TransformVariable<DataType, RawType>(Variable<RawType> raw, Func<RawType, DataType> transform, string name)
        {
            if (raw.DataSet != this)
                throw new ArgumentException("Raw variable must belong to the DataSet");

            var scaled = new LambdaTransformVariable<DataType, RawType>(name, raw, transform, null, null, null);
            AddVariableToCollection(scaled);
            return scaled;
        }

        /// <summary>
        /// Creates a transformed variable and adds it to the <see cref="DataSet"/>.
        /// </summary>
        /// <typeparam name="DataType">Type of data after the transform.</typeparam>
        /// <typeparam name="RawType">Source variable type of data.</typeparam>
        /// <param name="raw">Variable to transfrom.</param>
        /// <param name="transform">Forward transform function.</param>
        /// <param name="backwardTransform">Backward transform function.</param>
        /// <param name="name">Name of the transformed variable.</param>
        /// <returns>The transformed variable.</returns>
        /// <remarks>
        /// <para>
        /// The method creates a new variable and returns it.
        /// The transformed variable has the same rank and shape as the <paramref name="raw"/> does.
        /// The values of that variable are computed on-the-request via the function <paramref name="transform"/>
        /// on the basis of the underlying variable <paramref name="raw"/>.
        /// Changes done to the scaled variable propogate to the underlying variable <paramref name="raw"/>.  
        /// </para>
        /// <para>
        /// If the <paramref name="backwardTransform"/> is not null,  
        /// changes done to the underlying variable are reflected in the transformed variable using the
        /// <paramref name="backwardTransform"/>. Otherwise, the transformed variable is read only.
        /// </para>
        /// <para>Both source and targe variable share the metadata collection.
        /// See <see cref="TransformVariable{DataType,RawType}(Variable{RawType},Func{RawType,DataType},Func{DataType,RawType},IList{string},IList{string},string)"/>
        /// if there is need not to share metadata.
        /// </para>
        /// <para>Example is given in the remarks for 
        /// <see cref="ScaleVariable{DataType,RawType}(Variable{RawType},DataType,DataType,string)"/>.</para>
        /// </remarks>
        /// <seealso cref="ScaleVariable{DataType,RawType}(Variable{RawType},DataType,DataType,string)"/>
        /// <seealso cref="StrideVariable{DataType}(Variable{DataType},int[],int[],int[],string)"/>
        /// <seealso cref="CreateIndexTransformVariable{DataType}"/>
        /// <seealso cref="CreateRangeVariable"/>
        public Variable<DataType> TransformVariable<DataType, RawType>(Variable<RawType> raw, Func<RawType, DataType> transform, Func<DataType, RawType> backwardTransform, string name)
        {
            return TransformVariable<DataType, RawType>(raw, transform, backwardTransform, null, null, name);
        }

        /// <summary>
        /// Creates a transformed variable and adds it to the <see cref="DataSet"/>.
        /// </summary>
        /// <typeparam name="DataType">Type of data after the transform.</typeparam>
        /// <typeparam name="RawType">Source variable type of data.</typeparam>
        /// <param name="raw">Variable to transfrom.</param>
        /// <param name="transform">Forward transform function.</param>
        /// <param name="backwardTransform">Backward transform function.</param>
        /// <param name="name">Name of the transformed variable.</param>
        /// <param name="hiddenMetadataEntries">Collection of keys which are not to be inherited from the underlying metadata.
        /// These entries are changed independently.</param>
        /// <param name="readonlyMetadataEntries">Collection of keys that cannot be changed through this collection.</param>
        /// <returns>The transformed variable.</returns>
        /// <remarks>
        /// <para>The <paramref name="hiddenMetadataEntries"/> and <paramref name="readonlyMetadataEntries"/> parameters 
        /// allow to make some metadata entries indpendent from the metadata of the underlying variable.
        /// These parameters can be null and this will be considered as an empty collection.</para>
        /// <para>The name entry is always independent from the underlying metadata.</para>
        /// <para>
        /// See details in remarks for 
        /// <see cref="TransformVariable{DataType,RawType}(Variable{RawType},Func{RawType,DataType},Func{DataType,RawType},string)"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="CreateIndexTransformVariable{DataType}"/>
        /// <seealso cref="CreateRangeVariable"/>
        /// <seealso cref="TransformVariable{DataType,RawType}(Variable{RawType},Func{RawType,DataType},Func{DataType,RawType},string)"/>
        public Variable<DataType> TransformVariable<DataType, RawType>(Variable<RawType> raw, Func<RawType, DataType> transform, Func<DataType, RawType> backwardTransform, IList<string> hiddenMetadataEntries, IList<string> readonlyMetadataEntries, string name)
        {
            if (raw.DataSet != this)
                throw new ArgumentException("Raw variable must belong to the DataSet");

            var scaled = new LambdaTransformVariable<DataType, RawType>(name, raw, transform, backwardTransform, hiddenMetadataEntries, readonlyMetadataEntries);
            AddVariableToCollection(scaled);
            return scaled;
        }

        /// <summary>
        /// Creates a computational variable that calculates data values basing on indexes.
        /// </summary>
        /// <typeparam name="DataType">Resulting variable's data type.</typeparam>
        /// <param name="name">Name of a resulting variable.</param>
        /// <param name="indexTransform">The function that maps an index into a value.</param>
        /// <param name="dims">Dimensions of a resulting variable.</param>
        /// <returns>A computational variable.</returns>		
        /// <remarks>
        /// <para>
        /// The method creates an index-transform variable and adds it to the <see cref="DataSet"/>.
        /// </para>
        /// <para>
        /// Length of a variable is equal to a length of the dimensions it depends on 
        /// (<paramref name="dims"/>). If there is no any other variable depending on the same
        /// dimensions, their lengths are 0.
        /// </para>
        /// <example>
        /// <code>
        /// DataSet ds = . . .;
        /// 
        /// Variable&lt;double&gt; var = ds.AddVariable&lt;double&gt;("var", new double[] { 3.0, 3.1, 2.9 }, "x");
        /// Variable&lt;int&gt; trvar = ds.CreateIndexTransformVariable&lt;int&gt;(
        ///		"transform", indices => 2 * indices[0] + 1, "x");
        ///		
        /// int[] data = (int[])trvar.GetData(); // data: { 1, 3, 5 } 
        /// </code>
        /// </example>
        /// </remarks>
        /// <seealso cref="CreateRangeVariable"/>
        /// <seealso cref="TransformVariable{DataType,RawType}(Variable{RawType},Func{RawType,DataType},Func{DataType,RawType},string)"/>
        public Variable<DataType> CreateIndexTransformVariable<DataType>(string name, Func<int[], DataType> indexTransform, params string[] dims)
        {
            var indTransf = new IndexTransformVariable<DataType>(this, name, dims, indexTransform);
            AddVariableToCollection(indTransf);
            return indTransf;
        }

        /// <summary>
        /// Creates one-dimensional range variable.
        /// </summary>
        /// <param name="name">Name of a variable.</param>
        /// <param name="origin">Starting value.</param>
        /// <param name="dim">Dimension it depends on.</param>
        /// <returns>A range variable.</returns>
        /// <remarks>
        /// <para>The method creates one-dimensional computational variable and adds it to the <see cref="DataSet"/>.
        /// The variable contains data starting with <paramref name="origin"/> and increased by 1
        /// with their index increasing.
        /// </para>
        /// <para>
        /// Length of a variable is equal to a length of the dimension it depends on 
        /// (<paramref name="dim"/>). If there is no any other variable depending on the same
        /// dimension, its length is 0.
        /// </para>
        /// <example>
        /// <code>
        /// DataSet ds = . . .;
        /// 
        /// Variable&lt;double&gt; var = ds.AddVariable&lt;double&gt;("var", new double[] { 3.0, 3.1, 2.9 }, "x");
        /// Variable&lt;int&gt; range = ds.CreateRangeVariable("range", 100, "x");
        /// int[] data = (int[])range.GetData(); // data: { 100, 101, 102 } 
        /// </code>
        /// </example>
        /// </remarks>
        /// <seealso cref="CreateIndexTransformVariable{DataType}"/>
        /// <seealso cref="TransformVariable{DataType,RawType}(Variable{RawType},Func{RawType,DataType},Func{DataType,RawType},string)"/>
        public Variable CreateRangeVariable(string name, int origin, string dim)
        {
            var range = new RangeVariable(this, name, new string[] { dim }, origin);
            AddVariableToCollection(range);
            return range;
        }

        private static Variable CreateStriddenVariable(DataSet dataSet, string name, string[] dims, Variable sourceVariable, int[] start, int[] stride, int[] count)
        {
            Type varType = typeof(Variable<>).MakeGenericType(sourceVariable.TypeOfData);
            System.Reflection.ConstructorInfo cinfo =
                typeof(StriddenVariable<>).MakeGenericType(sourceVariable.TypeOfData)
                .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(DataSet), typeof(string), typeof(string[]), varType, typeof(int[]), typeof(int[]), typeof(int[]) }, null);
            return (Variable)cinfo.Invoke(new object[] { 
				dataSet, name, dims, sourceVariable, start, stride, count});
        }

        private static Variable<DataType> CreateScaledVariable<DataType, RawType>(Variable<RawType> raw, DataType scale, DataType offset, string name,
            IList<string> hiddenEntries, IList<string> readonlyEntries)
            where DataType : IConvertible
            where RawType : IConvertible
        {
            var scaled = new ScaledVariable<DataType, RawType>(raw, scale, offset, name, hiddenEntries, readonlyEntries);
            raw.DataSet.AddVariableToCollection(scaled);
            return scaled;
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Finalizes DataSet instance.
        /// </summary>
        ~DataSet()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes the <see cref="DataSet"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the <see cref="DataSet"/>, that is to be disposed, has changes, the <see cref="Rollback"/> method is called
        /// to cancel proposed changes.
        /// </para>
        /// <para>
        /// It is very important to dispose the <see cref="DataSet"/> when there is no need in it anymore.
        /// At disposal, the <see cref="DataSet"/> can free locked resources and unmanaged memory, if required.
        /// </para>
        /// <para>
        /// If the disposing <see cref="DataSet"/> has incoming references (see also <see cref="IsLinked"/>),
        /// all <see cref="DataSet"/>s, referring it, will be disposed, too. 
        /// In contrast, referred <see cref="DataSet"/>s are not affected by this disposal.
        /// </para>
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the <see cref="DataSet"/>.
        /// </summary>
        /// <param name="disposing">True, if the method is called from user's code; false, if called from finalizer.</param>
        /// <remarks>
        /// <para>
        /// If the <see cref="DataSet"/>, that is to be disposed, has changes, the <see cref="Rollback"/> method is called
        /// to cancel proposed changes.
        /// </para>
        /// <para>
        /// It is very important to dispose the <see cref="DataSet"/> when there is no need in it anymore.
        /// At disposal, the <see cref="DataSet"/> can free locked resources and unmanaged memory, if required.
        /// </para>
        /// <para>
        /// If the disposing <see cref="DataSet"/> has incoming references (see also <see cref="IsLinked"/>),
        /// all <see cref="DataSet"/>s, referring it, will be disposed, too. 
        /// In contrast, referred <see cref="DataSet"/>s are not affected by this disposal.
        /// </para>
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                // Dispose managed resources.
                lock (this)
                {
                    // Removing all subscriptions. This also should removes extra strong references and 
                    // avoid memory leaks caused by the event.
                    committedHandlers = null;

                    if (HasChanges)
                        Rollback();

                    Debug.Assert(HasChanges == false);
                    if (variables != null)
                        foreach (Variable v in variables)
                        {
                            v.Dispose();
                        }

                    if (outcomingReferences != null)
                        foreach (DataSetLink linkedDs in outcomingReferences)
                            linkedDs.TargetDataSet.RemoveIncomingReference(this);
                    if (incomingReferences != null)
                    {
                        var refs = incomingReferences;
                        incomingReferences = null;
                        foreach (DataSetLink refToMe in refs)
                            refToMe.SourceDataSet.Dispose();
                    }

                    foreach (DataSet ds in includedDataSets)
                        ds.Dispose();
                }
            }

            isDisposed = true;
        }

        #endregion

        #region Data Set Nested Types

        /// <summary>
        /// Gets the inner representation of the <see cref="DataSet"/>'s changes.
        /// </summary>
        /// <returns></returns>
        protected internal DataSet.Changes GetInnerChanges()
        {
            lock (this)
            {
                if (changes == null) return null;

                // Obtaining changes from variables
                foreach (var var in changes.Variables)
                {
                    if (var.HasChanges)
                        changes.UpdateChanges(var.GetInnerChanges());
                }

                return changes;
            }
        }

        /// <summary>
        /// Clears local changes of the <see cref="DataSet"/>.
        /// </summary>
        /// <remarks>
        /// Clears local changes of the <see cref="DataSet"/> (i.e. makes <see cref="HasChanges"/> equal to false),
        /// including changes of its variables.
        /// </remarks>
        protected void ClearChanges()
        {
            lock (this)
            {
                if (changes == null) return;

                if (changes.Variables != null)
                    changes.Variables.CollectionChanged -= OnVariableCollectionChanged;

                foreach (var v in variables)
                {
                    if (v.HasChanges)
                    {
                        v.ClearChanges();
                    }
                }

                foreach (var v in changes.Variables) // added variables
                {
                    // v.DataSet == null => the added variable was rolled back
                    if (v.DataSet != null && v.HasChanges)
                    {
                        v.ClearChanges();
                    }
                }

                changes = null;
            }
        }

        /// <summary>
        /// Represents proposed version of the <see cref="DataSet"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="Changes"/> instance accumulates changes as a user
        /// modifies the <see cref="DataSet"/>. As a result, it contains proposed version of the 
        /// <see cref="DataSet"/>. Its schema may not satisfy <see cref="DataSet"/> constraints.
        /// </remarks>
        public class Changes
        {
            private DataSet dataSet;
            private int version;
            private DataSetSchema initialSchema;
            private VariableCollection variables;
            private CoordinateSystemCollection coords;
            private ChangesetSource source;

            /// <summary>Maps variable ID to Variable.Changes.</summary>
            private Dictionary<int, Variable.Changes> varChanges;

            /// <summary>
            /// Initializes an instance of the class.
            /// </summary>
            /// <param name="dataSet"></param>
            /// <param name="version"></param>
            /// <param name="initialSchema"></param>
            /// <param name="variables"></param>
            /// <param name="coordinateSystems"></param>
            /// <param name="source"></param>
            public Changes(DataSet dataSet, int version, DataSetSchema initialSchema, VariableCollection variables, CoordinateSystemCollection coordinateSystems, ChangesetSource source)
            {
                if (initialSchema == null)
                    throw new ArgumentNullException("initialSchema");
                if (variables == null)
                    throw new ArgumentNullException("variables");
                if (coordinateSystems == null)
                    throw new ArgumentNullException("coordinateSystems");
                if (dataSet == null)
                    throw new ArgumentNullException("dataSet");
                if (version <= initialSchema.Version)
                    throw new ArgumentException("changeset id is incorrect");

                this.initialSchema = initialSchema;
                this.variables = variables;
                this.coords = coordinateSystems;
                this.version = version;
                this.source = source;
                this.dataSet = dataSet;
            }

            /// <summary>
            /// Gets the Guid of the changed DataSet.
            /// </summary>
            public Guid DataSetGuid { get { return dataSet.DataSetGuid; } }

            /// <summary>
            /// Gets the DataSet instance being changed.
            /// </summary>
            public DataSet DataSet { get { return dataSet; } }

            /// <summary>
            /// Gets the proposed version for the changeset.
            /// </summary>
            public int Version { get { return version; } }

            /// <summary>
            /// Currently committed schema of the data set.
            /// </summary>
            public DataSetSchema InitialSchema { get { return initialSchema; } }

            /// <summary>
            /// Gets the updated collection of variables including unmodified, updated and added variables.
            /// </summary>
            public VariableCollection Variables { get { return variables; } }

            /// <summary>
            /// Gets the updated collection of coordinate systems including unmodified and updated coordinate systems.
            /// </summary>
            public CoordinateSystemCollection CoordinateSystems { get { return coords; } }


            /// <summary>
            /// Gets the source of the changeset: it can be either local or remote or both.
            /// </summary>
            public ChangesetSource ChangesetSource
            {
                get { return source; }
            }

            /// <summary>
            /// Returns the changes for a given variable (null if it has no changes).
            /// </summary>
            public Variable.Changes GetVariableChanges(int varID)
            {
                if (varChanges == null) return null; // no changes

                Variable.Changes ch;
                if (varChanges.TryGetValue(varID, out ch))
                    return ch;
                return null;
            }

            /// <summary>
            /// Updates the collection of changes for the given variable with specified change set.
            /// </summary>
            /// <param name="changes"></param>
            public void UpdateChanges(Variable.Changes changes)
            {
                if (changes == null)
                    throw new ArgumentNullException("changes");
                if (varChanges == null)
                    varChanges = new Dictionary<int, Variable.Changes>();
                varChanges[changes.ID] = changes;
            }

            /// <summary>
            /// Makes a shallow copy of the changes.
            /// </summary>
            /// <returns></returns>
            public DataSet.Changes Clone()
            {
                DataSet.Changes ch = new Changes(dataSet, version, initialSchema, variables, this.coords, source);
                ch.varChanges = this.varChanges == null ? null : new Dictionary<int, Variable.Changes>(this.varChanges);
                return ch;
            }

            /// <summary>
            /// Gets the changes description for a public demonstration.
            /// </summary>
            /// <returns></returns>
            public DataSetChangeset GetChangeset()
            {
                return new DataSetChangeset(dataSet, this);
            }
        }

        private struct PrecommitOutput
        {
            public DataSet.Changes ActualChanges;
            public ReadOnlyDimensionList UpdatedDimensions;
        }

        #endregion

        #region From System.Object

        /// <summary>
        /// Returns a string representation of the <see cref="DataSet"/>.
        /// </summary>
        /// <returns>A string describing the <see cref="DataSet"/>.</returns>
        /// <remarks>
        /// <para>
        /// The result of the <see cref="ToString"/> looks like the following sample text:
        /// </para>
        /// <example>
        /// msds:nc?file=C:\ScientificDataSet\Data\air.sig995.2007.nc
        /// air.sig995.2007 [1]
        /// DSID: f1969410-8896-4ab8-bec5-7150af1db2ca
        /// [4] air of type Int16 (time:1460) (lat:73) (lon:144)
        /// [3] time of type Double (time:1460)
        /// [2] lon of type Single (lon:144)
        /// [1] lat of type Single (lat:73)
        /// </example>
        /// <para>
        /// The first line shows standard <see cref="DataSet.URI"/>. It consists of mandatory URI schema ‘msds’ and 
        /// SDS provider name ‘nc’ followed by optional provider parameters. 
        /// </para>
        /// <para>
        /// The second line starts with optional <see cref="DataSet.Name"/>.
        /// Integer number at the end of the line is the <see cref="DataSet.Version"/> number. 
        /// Each time <see cref="DataSet"/> commits changes requested by a user, it increments a version number. 
        /// If the <see cref="DataSet"/> is modified (see <see cref="DataSet.HasChanges"/>), an asterisk <c>‘*’</c> is shown
        /// after the version number.
        /// </para>
        /// <para>
        /// The third line contains unique identifier of the <see cref="DataSet"/> instance 
        /// (see <see cref="DataSet.DataSetGuid"/>).
        /// </para>
        /// <para>
        /// Below goes a list of variables. 
        /// Each variable summary shows variable <see cref="Variable.ID"/>, <see cref="Variable.Name"/>, 
        /// <see cref="Variable.TypeOfData"/> and shape (see also <see cref="Variable.Dimensions"/>). 
        /// Variable shape includes one dimension for vectors, two dimensions for matrices etc. 
        /// Variable summary shows both name and length for each of the dimensions.
        /// </para>
        /// </remarks>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(URI);
            if (!String.IsNullOrEmpty(Name))
            {
                sb.Append(Name);
                sb.Append(" ");
            }
            sb.Append("[");
            sb.Append(changeSetId);
            sb.Append("]");
            if (HasChanges)
                sb.Append('*');
            sb.AppendLine();
            sb.Append("DSID: ").AppendLine(DataSetGuid.ToString());
            if (Variables.Count > 0)
            {
                foreach (Variable v in Variables)
                {
                    sb.AppendLine(v.ToString());
                }
            }
            if (CoordinateSystems.Count > 0)
            {
                sb.AppendLine(" Coordinate Systems");
                foreach (CoordinateSystem s in CoordinateSystems)
                {
                    sb.Append("  ");
                    sb.AppendLine(s.ToString());
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return guid.GetHashCode();
        }

        #endregion

        #region Different

        /// <summary>
        /// Sets the read-only flag for the <see cref="DataSet"/> and all its variables.
        /// </summary>
        /// <remarks>
        /// If the <see cref="DataSet"/> has changes, the method will throw <see cref="NotSupportedException"/>.
        /// After the flag is set, no modifications are allowed to the <see cref="DataSet"/>.
        /// </remarks>
        /// <exception cref="NotSupportedException"><see cref="DataSet"/> has changes</exception>
        protected void SetCompleteReadOnly()
        {
            if (HasChanges) throw new NotSupportedException("Cannot set read-only flag in the modified DataSet");
            readOnly = true;
            foreach (var var in variables)
            {
                var.SetCompleteReadOnly();
            }
        }

        /// <summary>Checks whether specified type is supported according to <see cref="DataSet"/> specification.</summary>
        /// <param name="type">Type to check.</param>
        /// <returns>True, if <see cref="DataSet"/> supports variables of this type; false, otherwise.</returns>
        /// <remarks>
        /// For a complete list of supported types, see remarks for the <see cref="Variable"/> class.
        /// </remarks>
        public static bool IsSupported(Type type)
        {
            return type == typeof(Double) ||
                type == typeof(Single) ||
                type == typeof(Int16) ||
                type == typeof(Int32) ||
                type == typeof(Int64) ||
                type == typeof(UInt64) ||
                type == typeof(UInt32) ||
                type == typeof(UInt16) ||
                type == typeof(Byte) ||
                type == typeof(SByte) ||
                type == typeof(DateTime) ||
                type == typeof(String) ||
                type == typeof(Boolean) ||
                type == typeof(EmptyValueType);
        }

        /// <summary>
        /// Copies the dataset <paramref name="source"/> to the dataset <paramref name="destination"/>.
        /// </summary>
        /// <param name="source">The original dataset.</param>
        /// <param name="destination">The destination dataset.</param>
        public static void CopyAll(DataSet source, DataSet destination)
        {
            Utilities.DataSetCloning.Clone(source, destination, null);
        }


        /// <summary>
        /// Creates a deep copy of the <see cref="DataSet"/> instance.
        /// </summary>
        /// <param name="targetUri">URI of the output <see cref="DataSet"/>.</param>
        /// <returns>New <see cref="DataSet"/> instance that contains copy of the input <see cref="DataSet"/>.</returns>
        /// <remarks>
        /// <para>
        /// Opens the <see cref="DataSet"/> using given <paramref name="targetUri"/> and copies the current <see cref="DataSet"/> content
        /// into the output <see cref="DataSet"/>, restoring the structure and adding variables by value (see
        /// <see cref="AddVariableByValue(Variable)"/>) to the target <see cref="DataSet"/>.
        /// The resulting <see cref="DataSet"/> is committed. Note: it is required to dispose the returned DataSet;
        /// if you don't need the instance, you should dispose it immediately:
        /// <code>ds.Clone("copy.csv").Dispose();</code>.
        /// </para>
        /// <para>
        /// The <see cref="DataSet"/> must not be changed during cloning.
        /// </para>
        /// <example>
        /// The following example constructs a read-only <see cref="DataSet"/> based on a HTTP resource.
        /// Then the <see cref="DataSet"/> is cloned into a <see cref="T:MemoryDataSet"/> which then
        /// can be modified.
        /// <code> 
        /// DataSet ds1 = DataSet.Open("msds:csv?file=http://arca7.wdcb.ru/DataExplorer/data.csv&amp;inferDims=true");
        /// DataSetUri uri = DataSetUri.Create("msds:memory");
        /// ... // It is possible to change properties of the uri
        /// DataSet ds2 = ds1.Clone(uri);
        /// 
        /// ds1.Dispose();
        /// ds2.Dispose();
        /// </code>
        /// </example>
        /// <para>
        /// About DataSet URI see also remarks for <see cref="DataSet.Open(string)"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="Clone(string)"/>
        /// <seealso cref="DataSetUri"/>
        /// <seealso cref="Microsoft.Research.Science.Data.Factory.DataSetFactory"/>
        /// <exception cref="KeyNotFoundException">The provider is not registered.</exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public DataSet Clone(DataSetUri targetUri)
        {
            return Microsoft.Research.Science.Data.Utilities.DataSetCloning.Clone(this, targetUri, null);
        }


        /// <summary>
        /// Creates a deep copy of the <see cref="DataSet"/> instance.
        /// </summary>
        /// <param name="targetUri">URI of the output <see cref="DataSet"/>.</param>
        /// <returns>New <see cref="DataSet"/> instance that contains copy of the input <see cref="DataSet"/>.</returns>
        /// <remarks>
        /// See remarks for the <see cref="DataSet.Clone(DataSetUri)"/>.
        /// </remarks>
        /// <seealso cref="Clone(DataSetUri)"/>
        /// <seealso cref="DataSetUri"/>
        /// <seealso cref="Microsoft.Research.Science.Data.Factory.DataSetFactory"/>
        /// <exception cref="KeyNotFoundException">The provider is not registered.</exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public DataSet Clone(string targetUri)
        {
            DataSetUri dsUri = DataSetFactory.CreateUri(targetUri);
            return Clone(dsUri);
        }

        #endregion

        #region IEnumerable<Variable> Members

        /// <summary>
        /// Gets an enumerator for the committed variables collection.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Variable> GetEnumerator()
        {
            return variables.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Gets an enumerator for the committed variables collection.
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return variables.GetEnumerator();
        }

        #endregion

        #region Ranges

        /// <summary>
        /// Gets the range of a single index.
        /// </summary>
        /// <param name="index">An index.</param>
        /// <returns>A range of a single index.</returns>
        /// <seealso cref="T:Microsoft.Research.Science.Data.Imperative.DataSetExtensions"/>
        public static Range Range(int index)
        {
            return new Range(index, 1, 1);
        }
        /// <summary>
        /// Gets the range of indices from "from" up to "to". 
        /// </summary>
        /// <param name="from">Origin index of the range.</param>
        /// <param name="to">Last index of the range.</param>
        /// <returns>A range containing given interval.</returns>
        /// <seealso cref="T:Microsoft.Research.Science.Data.Imperative.DataSetExtensions"/>
        public static Range Range(int from, int to)
        {
            if (from > to) throw new ArgumentException("'from' is greater than 'to'");
            return new Range(from, 1, to - from + 1);
        }
        /// <summary>
        /// Gets the range of indices from "from" up to "to". 
        /// </summary>
        /// <param name="from">Origin index of the range.</param>
        /// <param name="step">Range stride.</param>
        /// <param name="to">Last index of the range.</param>
        /// <returns>A stridden range.</returns>
        /// <seealso cref="T:Microsoft.Research.Science.Data.Imperative.DataSetExtensions"/>
        public static Range Range(int from, int step, int to)
        {
            if (from > to) throw new ArgumentException("'from' is greater than 'to'");
            if (step <= 0) throw new ArgumentException("step is not positive", "step");
            int cnt = (to - from) / step + 1;
            return new Range(from, step, cnt);
        }
        /// <summary>
        /// Gets the range of indices from <paramref name="from"/> up to the maximum index for a given dimension.
        /// </summary>
        /// <param name="from">Origin index of the range.</param>
        /// <returns>An unlimited range.</returns>
        /// <seealso cref="T:Microsoft.Research.Science.Data.Imperative.DataSetExtensions"/>
        public static Range FromToEnd(int from)
        {
            return new Range(from, 1, -1);
        }
        /// <summary>
        /// Gets the range of indices from <paramref name="from"/> up to the maximum index for a given dimension.
        /// </summary>
        /// <param name="from">Origin index of the range.</param>
        /// <param name="step">Range stride.</param>
        /// <returns>An unlimited range.</returns>
        /// <seealso cref="T:Microsoft.Research.Science.Data.Imperative.DataSetExtensions"/>
        public static Range FromToEnd(int from, int step)
        {
            return new Range(from, step, -1);
        }
        /// <summary>
        /// Gets the range of a single index to choose and reduce a dimension.
        /// </summary>
        /// <param name="index">An index.</param>
        /// <returns>A range of a single index.</returns>
        /// <remarks>
        /// <para>
        /// The rank of the resulting array is less than the rank of the variable 
        /// the data is read from or written to. 
        /// Thus <c>ds.Get&lt;double&gt;("mat", i, j)</c> is equivalent to 
        /// <c>ds.Get&lt;double&gt;("mat", DataSet.ReduceDim(i), DataSet.ReduceDim(j))</c>.
        /// </para>
        /// </remarks>
        /// <seealso cref="T:Microsoft.Research.Science.Data.Imperative.DataSetExtensions"/>
        public static Range ReduceDim(int index)
        {
            return new Range(index, true);
        }

        #endregion
    }
}

