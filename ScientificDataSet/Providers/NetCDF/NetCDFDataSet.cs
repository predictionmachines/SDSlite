// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using NetCDFInterop;

namespace Microsoft.Research.Science.Data.NetCDF4
{
    /// <summary>
    /// A <see cref="DataSet"/> provider that wraps the NetCDF4 interface.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The provider requires a managed NetCDF.Interop.dll library (either x86 or x64 version, depending on the platform) to work.
    /// The current Scientific DataSet release runtime package includes netcdf4.dll
    /// built from Unidata's source codes for NetCDF Release 4.1.3
    /// with statically linked satellite libraries (including HDF5 1.8.7).
    /// (Default chunk cache size is 32 Mb, number of elements is 1009 and preemption policy is 0.75.)
    /// </para>
    /// <para>
    /// The provider supports variables of any non-negative rank.
    /// </para>
    /// <para>
    /// The provider is associated with the provider name "nc" and extensions ".nc".
    /// </para>
    /// <para>
    /// <see cref="NetCDFDataSet"/> accepts following parameters in the URI
    /// (read about DataSet URIs here: <see cref="DataSet.Open(string)"/>;
    /// about preliminary URI customization <see cref="DataSetUri.Create(string)"/>):
    /// <list type="table">
    /// <listheader>
    ///   <term>Parameter</term>
    ///   <description>Description</description>
    /// </listheader>
    ///  <item>
    ///		<term>file=path_to_file</term>
    ///		<description>Specifies the file to open or create.</description>
    /// </item>
    ///  <item>
    ///		<term>openMode=createNew|create|open|openOrCreate|readOnly</term>
    ///		<description>
    ///		The flag "openMode" specifies how the data set should open a file,
    /// data base or whatever resource it uses to store the data.
    /// Possible values for the flag are:
    /// <list type="table">
    ///		<listheader>
    ///		<term>Mode</term>
    ///		<description>Description</description>
    ///		</listheader>
    ///		<item>
    ///		<term>createNew</term>
    ///		<description>Specifies that the data set should create new resource. If the resource already exists, an exception IOException is thrown.</description>
    ///		</item>
    ///		<item>
    ///		<term>open</term>
    ///		<description>Specifies that the data set should open an existing resource. If the resource does not exist, an exception ResourceNotFoundException is thrown.</description>
    ///		</item>
    ///		<item>
    ///		<term>readOnly</term>
    ///		<description>Specifies that the data set should open an existing resource for reading only. If the resource does not exist, an exception ResourceNotFoundException is thrown.</description>
    ///		</item>
    ///		<item>
    ///		<term>create</term>
    ///		<description>Specifies that the data set should create a new resource. If the resource already exists, it will be re-created.</description>
    ///		</item>
    ///		<item>
    ///		<term>openOrCreate</term>
    ///		<description>Specifies that the data set should open a resource if it exists; otherwise, a new resource should be created.</description>
    ///		</item>
    /// </list>
    ///		</description>
    ///		If the input file is read only, the constructed data set is always read only.
    ///		Consider cloning it into a <see cref="T:MemoryDataSet"/> to modify.
    ///     See <see cref="DataSet.Clone(String)"/>.
    /// </item>
    /// <item>
    ///     <term>deflate=off|store|fastest|fast|normal|good|best</term>
    ///     <description>
    ///     Defines the data compression level. Affects only new variables,
    ///     added to the DataSet.
    ///     Default is <c>normal</c>.
    ///     Example: <c>msds:nc?file=output.nc&amp;openMode=create&amp;deflate=good</c>.
    ///     </description>
    /// </item>
    /// <item>
    ///     <term>trimZero=true|false</term>
    ///     <description>
    ///     Indicates whether the provider should trim trailing zero (\x00 symbol) when reads attribute values or not.
    ///     This enables correct reading of attributes from NetCDF files created by C code with \x00 appended.
    ///     Default is false.
    ///     </description>
    /// </item>
    ///  <item>
    ///		<term>enableRollback=true|false</term>
    ///		<description>Indicates whether the rollback is enabled or not. If enabled, each transaction will
    /// lead to making a back-up of the underlying file for further possible restoration. If disabled, rollback will throw
    /// an exception when it is invoked. Set it for datasets having not only NetCDF variables and consider set it false
    /// to significantly increase performance. Default is true.</description>
    /// </item>
    ///  <item>
    ///		<term>include</term>
    ///		<description>Allows including variables as references from another <see cref="DataSet"/>, defined as a URI,
    ///		into this <see cref="DataSet"/>.
    ///		Example: <c>msds:memory?include=msds%3Acsv%3Ffile%example.csv%23lat%2Clon</c>
    ///		(escaped version of <c>"msds:memory?include=escape[msds:csv?file=example.csv#lat,lon]"</c>)
    ///		includes variables <c>lat</c> and <c>lon</c> from <c>msds:csv?file=example.csv</c>. If variables names are not specified,
    ///		all variables are included.</description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// The <see cref="NetCDFDataSet"/> provider supports URIs containing a path
    /// and appended through '?' parameters: <c>c:\data\air0.nc?openMode=open&amp;enableRollback=false</c>.
    /// </para>
    /// <para>
    /// In a NetCDF4 file all variables are stored as a set of chunks. The chunk size
    /// must be specified for each new variable and it cannot be changed later. Both read/write performance and output file size depend on the selection of the chunk size.
    /// Too small or too large chunks or wrong shape of a multidimensional chunk, relatively to the actually stored array's size and shape, can decrease performance and increase output file size.
    /// The chunk size selection algorithm, which is implemented in the <see cref="NetCDFDataSet"/>, is aimed at a general case, therefore
    /// we define square chunks with size providing good performance and file size when storing arrays with length from 0 to about a million of elements.
    /// The algorithm parameters are based on empirical analysis.
    /// In short, the chunk size for each dimension is selected in a way to make total chunk size in bytes from 8 Kb to 128 Kb maximum;
    /// if it is impossible, chunk size in bytes must be the greatest possible, but less than 8 Kb.
    /// </para>
    /// <para>
    /// If you need performance tuning, or you have a stretched array which doesn't fit well into square chunks, you should manually set the chunk size.
    /// To enable it, <see cref="NetCDFDataSet"/> implements the <see cref="IChunkSizesAdjustable"/> interface.
    /// </para>
    /// <para>
    /// Names. If a variable name is not complied with NetCDF naming rules,
    /// its simplified version is used at the NetCDF layer, but through the <see cref="DataSet"/> it is still visible
    /// unmodified.
    /// </para>
    /// <para>
    /// <see cref="DateTime"/> support.
    /// Since the unmanaged NetCDF4 doesn't support a date/time type, it is internally stored
    /// as a double variable with special attribute
    /// <c>Units = "100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001"</c>.
    /// NetCDF variables with this attribute are loaded as <see cref="DateTime"/> variables.
    /// In the current release, any other units descriptions are not considered as a date/time and
    /// hence the variable's final type of data is <see cref="Double"/>.
    /// </para>
    /// <example>
    /// The following example creates an empty <see cref="NetCDFDataSet"/> for a new data file:
    /// <code>
    /// using (DataSet ds = DataSet.Open("msds:nc?file=example.nc&amp;openMode=create"))
    ///	{
    ///		// Working with ds . . .
    ///	}
    /// </code>
    /// </example>
    /// <para><see cref="NetCDFDataSet"/> implements interface <see cref="IChunkSizesAdjustable"/>, therefore it allows
    /// a user to choose size explicitly. See remarks for the <see cref="IChunkSizesAdjustable"/> fore details.</para>
    /// <para>There is an issue in the current version of NetCDF library: when defining a variable depending on a new dimension with name
    /// equal to a name of already existing variable, it fails. The only known solution is first to define the variable with this dimension, and
    /// then the variable with that dimension's name.</para>
    /// </remarks>
    [DataSetProviderName("nc")]
    [DataSetProviderFileExtension(".nc")]
    [DataSetProviderUriTypeAttribute(typeof(NetCDFUri))]
    public class NetCDFDataSet : DataSet, IChunkSizesAdjustable
    {
        #region Private Fields

        /// <summary>
        /// Filters trace messages related to the DataSet.
        /// </summary>
        internal static TraceSwitch TraceNetCDFDataSet = DataSet.TraceDataSet;

        internal new const int GlobalMetadataVariableID = DataSet.GlobalMetadataVariableID;

        private int ncid = -1;
        private bool initializing = true; // means that we're inializing schema from the file
        private bool rollbackEnabled = false;
        private bool trimTrailingZero;

        private int defaultCacheSize = 32 * 1024 * 1024; // 32 Mb
        private int defaultCacheNElems = 1009;
        private float defaultCachePreemption = 0.75f;

        private string tempFileName = null;

        private int[] chunkSizes;
        private DeflateLevel deflate = DeflateLevel.Off;

        internal const string dateTimeUnits = "100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001";


        #endregion

        #region Ctors

        /// <summary>
        /// Initializes new instance of the NetCDFDataSet class.
        /// </summary>
        /// <param name="uri">DataSet URI (see remarks for <see cref="NetCDFDataSet"/>).</param>
        /// <remarks>
        /// <para>
        /// If the file specified by <paramref name="fileName"/> exists, the NetCDF will be
        /// initialized with that file. Otherwise, the new file will be created and
        /// the resulting NetCDFDataSet will be empty.
        /// </para>
        /// <para>
        /// This constructor sets the property <see cref="RollbackEnabled"/> to false
        /// thus allows to work with large scale files.
        /// </para>
        /// </remarks>
        public NetCDFDataSet(string uri)
            : base(false, true)
        {
            RollbackEnabled = false;

            if (DataSetUri.IsDataSetUri(uri))
                this.uri = new NetCDFUri(uri);
            else
                this.uri = NetCDFUri.FromFileName(uri);
            DataSetUri.NormalizeUri(this.uri);

            rollbackEnabled = ((NetCDFUri)this.uri).EnableRollback;
            deflate = ((NetCDFUri)this.uri).Deflate;
            trimTrailingZero = ((NetCDFUri)this.uri).TrimTrailingZero;

            Debug.WriteLineIf(TraceNetCDFDataSet.TraceInfo, "Deflate mode: " + deflate);

            ResourceOpenMode openMode = this.uri.GetOpenModeOrDefault(ResourceOpenMode.OpenOrCreate);
            InitializeFromFile(((NetCDFUri)this.uri).FileName, openMode);
        }

        /// <summary>
        /// Initializes new instance of the NetCDFDataSet class.
        /// </summary>
        /// <param name="uri">DataSet URI (see remarks for <see cref="NetCDFDataSet"/>).</param>
        /// <param name="openMode">The open mode (see
        /// <seealso cref="Microsoft.Research.Science.Data.ResourceOpenMode"/>).</param>
        /// <remarks>
        /// <para>
        /// This constructor sets the property <see cref="RollbackEnabled"/> to false
        /// thus allows to work with large scale files.
        /// </para>
        /// </remarks>
        public NetCDFDataSet(string uri, ResourceOpenMode openMode)
        {
            RollbackEnabled = false;

            if (DataSetUri.IsDataSetUri(uri))
                this.uri = new NetCDFUri(uri);
            else
                this.uri = NetCDFUri.FromFileName(uri);
            DataSetUri.NormalizeUri(this.uri);

            rollbackEnabled = ((NetCDFUri)this.uri).EnableRollback;
            deflate = ((NetCDFUri)this.uri).Deflate;
            trimTrailingZero = ((NetCDFUri)this.uri).TrimTrailingZero;
            InitializeFromFile(((NetCDFUri)this.uri).FileName, openMode);
        }

        private void InitializeFromFile(string fileName, ResourceOpenMode openMode)
        {
            bool autoCommit = IsAutocommitEnabled;
            IsAutocommitEnabled = false;
            try
            {
                initializing = true;
                int res;

                bool exists = true;
                bool isUrl = false;

                if (((NetCDFUri)this.uri).FileName.StartsWith("http")) {
                    openMode = ResourceOpenMode.ReadOnly;
                    isUrl = true;
                } else {
                    exists = File.Exists(fileName);
                }

                if (openMode == ResourceOpenMode.Create && exists)
                {
                    File.Delete(fileName);
                    exists = false;
                }

                if (NetCDFDataSet.TraceNetCDFDataSet.TraceInfo)
                {
                    IntPtr cacheSize, nelems;
                    float preemption;
                    if (NetCDF.nc_get_chunk_cache(out cacheSize, out nelems, out preemption) == (int)ResultCode.NC_NOERR)
                    {
                        string s = String.Format("NetCDF chunk cache size: {0}, nelems: {1}, preemption: {2}", cacheSize, nelems, preemption);
                        Trace.WriteLineIf(NetCDFDataSet.TraceNetCDFDataSet.TraceInfo, s);
                    }
                }

                // Opening or creating file
                if (exists)
                {
                    if (openMode == ResourceOpenMode.CreateNew)
                        throw new IOException("The open mode is createNew but the file already exists");

                    if (!isUrl && ((File.GetAttributes(fileName) & FileAttributes.ReadOnly) != 0) && openMode != ResourceOpenMode.ReadOnly)
                    {
                        openMode = ResourceOpenMode.ReadOnly;
                        Trace.WriteLineIf(TraceNetCDFDataSet.TraceWarning, "NetCDFDataSet: Opening file in read only mode");
                    }

                    if (openMode == ResourceOpenMode.ReadOnly)
                        res = NetCDF.nc_open_chunked(fileName, CreateMode.NC_NOWRITE /*| NetCDF.CreateMode.NC_SHARE*/, out ncid, new IntPtr(defaultCacheSize), new IntPtr(defaultCacheNElems), defaultCachePreemption);
                    else
                        res = NetCDF.nc_open_chunked(fileName, CreateMode.NC_WRITE /*| NetCDF.CreateMode.NC_SHARE*/, out ncid, new IntPtr(defaultCacheSize), new IntPtr(defaultCacheNElems), defaultCachePreemption);
                }
                else
                {
                    if (openMode == ResourceOpenMode.Open || openMode == ResourceOpenMode.ReadOnly)
                        throw new ResourceNotFoundException(fileName);

                    if (!Path.IsPathRooted(fileName))
                        fileName = Path.Combine(Environment.CurrentDirectory, fileName);
                    res = NetCDF.nc_create_chunked(fileName, CreateMode.NC_NETCDF4 | CreateMode.NC_CLOBBER /*| NetCDF.CreateMode.NC_SHARE*/, out ncid, new IntPtr(defaultCacheSize), new IntPtr(defaultCacheNElems), defaultCachePreemption);

                    Variable globalMetaVar = new NetCDFGlobalMetadataVariable(this);
                    AddVariableToCollection(globalMetaVar);
                }

                HandleResult(res);

                if (exists)
                {
                    //********************************************************
                    // Loading schema
                    int ndims, nvars, ngatts, unlimdimid;
                    res = NetCDF.nc_inq(ncid, out ndims, out nvars, out ngatts, out unlimdimid);
                    HandleResult(res);

                    Debug.WriteLineIf(NetCDFDataSet.TraceNetCDFDataSet.TraceInfo, String.Format("NetCDF dataset {0} opened: {1} dimensions, {2} variables, {3} global attributes.",
                        fileName, ndims, nvars, ngatts));

                    /********************************************************
                    // Reading Global Attributes
                    int nattrs;
                    res = NetCDF.nc_inq_varnatts(ncid, NetCDF.NC_GLOBAL, out nattrs);
                    AttributeTypeMap atm = new AttributeTypeMap(ncid, NetCDF.NC_GLOBAL);
                    NetCDFDataSet.HandleResult(res);
                    MetadataDictionary globalMetadata = Metadata;
                    for (int i = 0; i < nattrs; i++)
                    {
                        // Name
                        StringBuilder attName = new StringBuilder(512);
                        res = NetCDF.nc_inq_attname(ncid, NetCDF.NC_GLOBAL, i, attName);
                        NetCDFDataSet.HandleResult(res);
                        string aname = attName.ToString();

                        // Skip out internal attribute
                        if (aname == AttributeTypeMap.AttributeName)
                            continue;

                        // Type
                        object value = ReadNetCdfAttribute(NetCDF.NC_GLOBAL, aname, atm);
                        globalMetadata[aname] = value;
                    } /**/

                    //********************************************************
                    // Loading variables
                    int[] varIds = new int[nvars];
                    res = NetCDF.nc_inq_varids(ncid, out nvars, varIds);
                    HandleResult(res);

                    for (int i = 0; i < nvars; i++)
                    {
                        Variable var = ReadNetCdfVariable(varIds[i]);
                        AddVariableToCollection(var);
                    }

                    Variable globalMetaVar = new NetCDFGlobalMetadataVariable(this);
                    AddVariableToCollection(globalMetaVar);

                    //********************************************************
                    // Loading coordinate systems
                    // CS stored in two places: as global attributes and as variable's attributes
                    // for backward compatibility. We're going to use the global attributes,
                    // as it works even when there is no any variable containing the coordinate system.
                    for (int i = 0; ; i++)
                    {
                        string attName = "coordinates" + (i + 1).ToString();

                        // Inquire information about cs attribute
                        NcType type;
                        IntPtr len;
                        res = NetCDF.nc_inq_att(ncid, NcConst.NC_GLOBAL, attName, out type, out len);
                        if (res == (int)ResultCode.NC_ENOTATT) // that's all: no more cs
                            break;
                        if (res != (int)ResultCode.NC_NOERR) // an error has occurred
                            HandleResult(res);

                        if (type != NcType.NC_CHAR)
                            throw new Exception("Coordinate system defining attribute is not a string");

                        // Getting the value of the attribute
                        string attValue = NcGetAttText(ncid, NcConst.NC_GLOBAL, attName, (int)len, out res);
                        HandleResult(res);

                        string[] items = attValue.Split(' ');
                        string csName = items[0]; // csname axis1 axis2 ...

                        // Creating the coordinate system instance
                        Variable[] axes = new Variable[items.Length - 1];
                        bool csFound = true;
                        for (int j = 1; j < items.Length; j++) // axes names
                        {
                            int aid;
                            Variable a;
                            if (int.TryParse(items[j], out aid))
                            {
                                csFound = Variables.TryGetByID(aid, out a);
                                if (!csFound) break;
                                a = Variables.GetByID(aid);
                            }
                            else
                            {
                                csFound = ContainsRecent(Variables, items[j]);
                                if (!csFound) break;
                                a = Variables[items[j], SchemaVersion.Recent];
                            }
                            axes[j - 1] = a;
                        }
                        if (!csFound) break;
                        CoordinateSystem cs = CreateCoordinateSystem(csName, axes);
                    }

                    // Loading coordinate systems for variable's metadata (backward compatibility)
                    foreach (Variable var in Variables)
                    {
                        if (var.Metadata.ContainsKey("coordinates", SchemaVersion.Recent) &&
                            !var.Metadata.ContainsKey("coordinatesName", SchemaVersion.Recent))
                        {
                            string[] axes = var.Metadata["coordinates", SchemaVersion.Recent].ToString().Split(' ');
                            List<Variable> axesList = new List<Variable>();
                            bool axisNotFound = false;
                            foreach (var axis in axes)
                            {
                                try
                                {
                                    Variable a = Variables[axis, SchemaVersion.Recent];
                                    axesList.Add(a);
                                }
                                catch
                                {
                                    Trace.WriteLineIf(TraceNetCDFDataSet.TraceError, "Coordinate named " + axis + " is not found in nc file");
                                    axisNotFound = true;
                                    break;
                                }
                            }
                            if (!axisNotFound)
                            {
                                // This is a standart NC file without SDS coordinate systems metadata!
                                //string csName = "_cs_" + var.Name + "_" + (Guid.NewGuid()).ToString();
                                //CreateCoordinateSystem(csName, axesList.ToArray());
                                break;
                            }
                        }
                    }

                    // Adding coordinate systems to variables
                    foreach (Variable var in Variables)
                    {
                        // Each variable has an entry named coordinates#Name, # - integer index
                        var.Metadata.ForEach(
                            delegate (KeyValuePair<string, object> entry)
                            {
                                if (!entry.Key.StartsWith("coordinates") || !entry.Key.EndsWith("Name"))
                                    return;

                                try
                                {
                                    CoordinateSystem cs = CoordinateSystems[(string)entry.Value, SchemaVersion.Recent];
                                    var.AddCoordinateSystem(cs);
                                }
                                catch (Exception ex)
                                {
                                    Trace.WriteLineIf(TraceNetCDFDataSet.TraceError, "Coordinate system with name " + entry.Value + " not found");
                                }
                            },
                            SchemaVersion.Proposed);
                    }
                } // eo if exists

                Commit();

                if (openMode == ResourceOpenMode.ReadOnly)
                    SetCompleteReadOnly();
                initializing = false;
            }
            finally
            {
                IsAutocommitEnabled = autoCommit;
            }
        }

        private static bool ContainsRecent(ReadOnlyVariableCollection vars, string var)
        {
            try
            {
                var v = vars[var, SchemaVersion.Recent];
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="enableRollback">
        /// Gets or sets the value indicating if rollback is enabled. If it does, the each transaction will
        /// lead to copying source file for further possible restoraition. If it doesn't, the rollback will throw
        /// an exception in case of its invoke. Set it only for data sets having not NetCDF variables.
        /// </param>
        /// <returns></returns>
        internal static NetCDFDataSet Open(string fileName, bool enableRollback)
        {
            NetCDFDataSet nc = new NetCDFDataSet(fileName, ResourceOpenMode.Open);
            nc.RollbackEnabled = enableRollback;
            return nc;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="enableRollback">
        /// Gets or sets the value indicating if rollback is enabled. If it does, the each transaction will
        /// lead to copying source file for further possible restoraition. If it doesn't, the rollback will throw
        /// an exception in case of its invoke. Set it only for data sets having not NetCDF variables.
        /// </param>
        /// <returns></returns>
        internal static NetCDFDataSet Create(string fileName, bool enableRollback)
        {
            NetCDFDataSet nc = new NetCDFDataSet(fileName, ResourceOpenMode.Create);
            nc.RollbackEnabled = enableRollback;
            return nc;
        }

        #endregion

        #region Properties

        internal DeflateLevel Deflate
        {
            get { return deflate; }
        }

        /// <summary>
        /// Gets or sets the value indicating if rollback is enabled. If it does, the each transaction will
        /// lead to copying source file for further possible restoraition. If it doesn't, the rollback will throw
        /// an exception in case of its invoke. Set it only for data sets having not NetCDF variables.
        /// </summary>
        internal bool RollbackEnabled
        {
            get { return rollbackEnabled; }
            set { rollbackEnabled = value; }
        }

        internal int NcId { get { return ncid; } }

        internal bool Initializing { get { return initializing; } }

        #endregion

        #region Overriden Methods

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="DataType"></typeparam>
        /// <param name="varName"></param>
        /// <param name="dims"></param>
        /// <returns></returns>
        protected override Variable<DataType> CreateVariable<DataType>(string varName, string[] dims)
        {
            StartChanges();

            return (Variable<DataType>)CreateNetCdfVariable(varName, dims, typeof(DataType));
        }

        /// <summary>
        ///
        /// </summary>
        protected override void OnTransactionOpened()
        {
            /* Storing existing file for possible rollback */
            if (!initializing && rollbackEnabled)
            {
                tempFileName = Path.GetTempFileName();

                int res = NetCDF.nc_close(ncid);
                HandleResult(res);

                File.Copy(((NetCDFUri)this.uri).FileName, tempFileName, true);

                res = NetCDF.nc_open(((NetCDFUri)this.uri).FileName, CreateMode.NC_WRITE, out ncid);
                HandleResult(res);
                // After the open, dimensions id can change.
                UpdateDimIds();
            }
        }

        private void UpdateDimIds()
        {
            // After the open, dimensions id can change.
            foreach (var var in this.variables)
            {
                INetCDFVariable ncVar = var as INetCDFVariable;
                if (ncVar != null)
                    ncVar.UpdateDimiIds();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="changes"></param>
        /// <returns></returns>
        protected override bool OnPrecommitting(DataSet.Changes changes)
        {
            if (initializing) return true;

            int res = NetCDF.nc_redef(ncid);
            return true;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="changes"></param>
        protected override void OnPrecommit(DataSet.Changes changes)
        {
            if (initializing) return;

            // Saving proposed coordinate systems as global attributes
            var proposedCS = GetCoordinateSystems(SchemaVersion.Proposed);
            if (proposedCS.Count > 0)
            {
                // saving global attribute: csname axis1.ID axis2.ID etc
                for (int i = 0; i < proposedCS.Count; i++)
                {
                    CoordinateSystem cs = proposedCS[i];
                    if (Array.Exists(cs.AxesArray, a => !(a is INetCDFVariable)))
                        continue; // saving cs with native axes only

                    string attName = "coordinates" + (i + 1).ToString();
                    StringBuilder attValue = new StringBuilder(cs.Name);
                    foreach (var axis in cs.Axes)
                    {
                        attValue.Append(' ');
                        attValue.Append(axis.ID);
                    }

                    WriteNetCdfAttribute(NcConst.NC_GLOBAL, attName, attValue.ToString(), null);
                }
            }
        }
        /// <summary>
        ///
        /// </summary>
        protected override void OnCommit()
        {
            if (initializing) return;

            int res;
            res = NetCDF.nc_enddef(ncid);
            res = NetCDF.nc_sync(ncid);
            HandleResult(res);

            if (tempFileName != null && File.Exists(tempFileName))
                File.Delete(tempFileName);
            tempFileName = null;
        }
        /// <summary>
        ///
        /// </summary>
        protected override void OnRollback()
        {
            if (initializing) return;
            if (!rollbackEnabled)
                throw new Exception("Rollback is disabled due to performance requirements. Set RollbackEnabled property to true to enable it.");

            if (tempFileName != null && File.Exists(tempFileName))
            {
                int res = NetCDF.nc_close(ncid);
                HandleResult(res);

                string fileName = ((NetCDFUri)this.uri).FileName;
                int tries = 1000;
                do
                {
                    try
                    {
                        File.Delete(fileName);
                        File.Move(tempFileName, fileName);
                        tempFileName = null;
                    }
                    catch (IOException ex)
                    {
                        Trace.WriteLineIf(TraceNetCDFDataSet.TraceWarning, "The underlying file is locked by another application. Retrying operation...");
                        System.Threading.Thread.Sleep(10);
                        if (--tries == 0)
                        {
                            File.Delete(tempFileName);
                            tempFileName = null;
                            throw new DataSetException("Cannot update the underlying file for it is locked by another application", ex);
                        }
                    }
                } while (tempFileName != null);
                res = NetCDF.nc_open(fileName, CreateMode.NC_WRITE /*| NetCDF.CreateMode.NC_SHARE*/, out ncid);
                HandleResult(res);
                UpdateDimIds();
            }
        }

        #endregion

        #region Utils

        /// <summary>
        ///
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (ncid >= 0)
            {
                try
                {
                    int res = NetCDF.nc_close(ncid);
                    if (disposing)
                        HandleResult(res);
                    if (tempFileName != null)
                        File.Delete(tempFileName);
                }
                catch (Exception ex)
                {
                    Trace.WriteLineIf(TraceNetCDFDataSet.TraceError, "NetCDF Dispose exception: " + ex.Message);
                }
            }
            base.Dispose(disposing);
        }

        private Variable ReadNetCdfVariable(int varid)
        {
            NcType type;
            Type clrType;
            int res = NetCDF.nc_inq_vartype(ncid, varid, out type);
            NetCDFDataSet.HandleResult(res);

            clrType = NetCDF.GetCLRType(type);

            // Int64: old files were written with this type for DateTime.
            // Double is more correct and is used recently.
            // Remove it in near future.
            if (type == NcType.NC_INT64 || type == NcType.NC_DOUBLE)
            {
                // It might be DateTime as well
                NcType atype;
                IntPtr alen1;
                res = NetCDF.nc_inq_att(ncid, varid, "Units", out atype, out alen1);
                int alen = (int)alen1;
                if (res == (int)ResultCode.NC_NOERR && atype == NcType.NC_CHAR)
                {
                    string units = NcGetAttText(ncid, varid, "Units", alen, out res);
                    HandleResult(res);

                    if (units == dateTimeUnits)
                        clrType = typeof(DateTime);
                }
                else if (res != (int)ResultCode.NC_ENOTATT)
                {
                    HandleResult(res);
                }
            }
            else if (type == NcType.NC_UBYTE) // might be bool as well
            {
                NcType atype;
                IntPtr alen1;
                res = NetCDF.nc_inq_att(ncid, varid, AttributeTypeMap.AttributeVarSpecialType, out atype, out alen1);
                int alen = (int)alen1;
                if (res == (int)ResultCode.NC_NOERR && atype == NcType.NC_CHAR)
                {
                    string specType = NcGetAttText(ncid, varid, AttributeTypeMap.AttributeVarSpecialType, alen, out res);
                    HandleResult(res);

                    if (specType == typeof(Boolean).ToString())
                        clrType = typeof(Boolean);
                }
                else if (res != (int)ResultCode.NC_ENOTATT)
                {
                    HandleResult(res);
                }
            }

            Variable v = (Variable)typeof(NetCdfVariable<>).MakeGenericType(clrType).
                GetMethod("Read", BindingFlags.NonPublic | BindingFlags.Static).
                Invoke(null, new object[] { this, varid });
            return v;
        }

        private Variable CreateNetCdfVariable(string name, string[] dims, Type dataType)
        {
            Variable v = (Variable)typeof(NetCdfVariable<>).MakeGenericType(dataType).
                GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Static).
                Invoke(null, new object[] { this, name, dims });
            return v;
        }

        internal object ReadNetCdfAttribute(int varid, string aname, AttributeTypeMap atm)
        {
            NcType type;
            IntPtr len1;
            int res = NetCDF.nc_inq_att(NcId, varid, aname, out type, out len1);
            int len = (int)len1;
            NetCDFDataSet.HandleResult(res);
            object value;
            switch (type)
            {
                case NcType.NC_UBYTE:
                    bool bcustomType = (atm != null && atm.Contains(aname));
                    if (bcustomType && atm[aname] == null)
                    {
                        value = null;
                    }
                    else
                    {
                        byte[] bvalue = new byte[len];
                        res = NetCDF.nc_get_att_uchar(ncid, varid, aname, bvalue);
                        if (len == 1 && (atm == null || !atm.IsArray(aname)))
                        {
                            if (bcustomType && atm[aname] == typeof(bool))
                                value = bvalue[0] > 0; // bool
                            else
                                value = bvalue[0]; // byte
                        }
                        else
                        {
                            if (bcustomType && atm[aname] == typeof(bool[]))
                                value = Array.ConvertAll(bvalue, p => p > 0); // bool[]
                            else
                                value = bvalue; // byte[]
                        }
                        NetCDFDataSet.HandleResult(res);
                    }
                    break;
                case NcType.NC_BYTE:
                    sbyte[] sbvalue = new sbyte[len];
                    res = NetCDF.nc_get_att_schar(ncid, varid, aname, sbvalue);
                    if (len == 1 && (atm == null || !atm.IsArray(aname)))
                        value = sbvalue[0]; // byte
                    else
                        value = sbvalue;
                    NetCDFDataSet.HandleResult(res);
                    break;
                case NcType.NC_CHAR:
                    string strvalue = NcGetAttText(ncid, varid, aname, len, out res);
                    NetCDFDataSet.HandleResult(res);
                    if (atm != null && atm.Contains(aname))
                    {
                        if (atm[aname] == typeof(DateTime[]))
                            value = StringToDateTimes(strvalue);
                        else if (atm[aname] == typeof(DateTime))
                            value = StringToDateTimes(strvalue)[0];
                        else
                            value = strvalue;

                    }
                    else
                    {
                        value = trimTrailingZero ? strvalue.TrimEnd('\0') : strvalue;
                    }
                    break;
                case NcType.NC_STRING:
                    string[] strings = new string[len];
                    res = NetCDF.nc_get_att_string(NcId, varid, aname, strings);
                    NetCDFDataSet.HandleResult(res);
                    if (atm != null && atm.Contains(aname) && atm[aname] == null)
                    {
                        value = null;
                    }
                    else // not string[]
                    {
                        if (trimTrailingZero)
                        {
                            // Trimming '\0' that could be appended by C code
                            for (int i = 0; i < len; i++)
                            {
                                strings[i] = strings[i].TrimEnd('\0');
                            }
                        }
                        value = strings;
                    }

                    break;
                case NcType.NC_SHORT:
                    short[] svalue = new short[len];
                    res = NetCDF.nc_get_att_short(NcId, varid, aname, svalue);
                    if (len == 1 && (atm == null || !atm.IsArray(aname)))
                        value = svalue[0];
                    else
                        value = svalue;
                    NetCDFDataSet.HandleResult(res);
                    break;
                case NcType.NC_USHORT:
                    ushort[] usvalue = new ushort[len];
                    res = NetCDF.nc_get_att_ushort(NcId, varid, aname, usvalue);
                    if (len == 1 && (atm == null || !atm.IsArray(aname)))
                        value = usvalue[0];
                    else
                        value = usvalue;
                    NetCDFDataSet.HandleResult(res);
                    break;
                case NcType.NC_INT:
                    int[] ivalue = new int[len];
                    res = NetCDF.nc_get_att_int(NcId, varid, aname, ivalue);
                    if (len == 1 && (atm == null || !atm.IsArray(aname)))
                        value = ivalue[0];
                    else
                        value = ivalue;
                    NetCDFDataSet.HandleResult(res);
                    break;
                case NcType.NC_UINT:
                    uint[] uivalue = new uint[len];
                    res = NetCDF.nc_get_att_uint(NcId, varid, aname, uivalue);
                    if (len == 1 && (atm == null || !atm.IsArray(aname)))
                        value = uivalue[0];
                    else
                        value = uivalue;
                    NetCDFDataSet.HandleResult(res);
                    break;
                case NcType.NC_INT64:
                    Int64[] livalue = new Int64[len];
                    res = NetCDF.nc_get_att_longlong(NcId, varid, aname, livalue);
                    if (len == 1 && (atm == null || !atm.IsArray(aname)))
                        value = livalue[0];
                    else
                        value = livalue;
                    NetCDFDataSet.HandleResult(res);
                    break;
                case NcType.NC_UINT64:
                    UInt64[] ulivalue = new UInt64[len];
                    res = NetCDF.nc_get_att_ulonglong(NcId, varid, aname, ulivalue);
                    if (len == 1 && (atm == null || !atm.IsArray(aname)))
                        value = ulivalue[0];
                    else
                        value = ulivalue;
                    NetCDFDataSet.HandleResult(res);
                    break;
                case NcType.NC_FLOAT:
                    float[] fvalue = new float[len];
                    res = NetCDF.nc_get_att_float(NcId, varid, aname, fvalue);
                    if (len == 1 && (atm == null || !atm.IsArray(aname))) value = fvalue[0];
                    else value = fvalue;
                    NetCDFDataSet.HandleResult(res);
                    break;
                case NcType.NC_DOUBLE:
                    double[] dvalue = new double[len];
                    res = NetCDF.nc_get_att_double(NcId, varid, aname, dvalue);
                    if (len == 1 && (atm == null || !atm.IsArray(aname))) value = dvalue[0];
                    else value = dvalue;
                    NetCDFDataSet.HandleResult(res);
                    break;
                default:
                    throw new NotSupportedException("Invalid attribute type value");
            }
            return value;
        }

        internal static string NcGetAttText(int ncid, int varid, string aname, int len, out int res)
        {
            string strvalue = null;
            res = NetCDF.nc_get_att_text(ncid, varid, aname, out strvalue, len);
            NetCDFDataSet.HandleResult(res);
            return strvalue;
        }

        internal static int NcPutAttText(int ncid, int varid, string aname, string value)
        {
            int res = NetCDF.nc_put_att_text(ncid, varid, aname, value);
            NetCDFDataSet.HandleResult(res);
            return res;
        }

        internal static string DateTimesToString(DateTime[] dates)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < dates.Length; i++)
            {
                sb.Append(dates[i].ToString(CultureInfo.InvariantCulture));
                if (i < dates.Length - 1)
                    sb.Append('|');
            }
            return sb.ToString();
        }

        internal static DateTime[] StringToDateTimes(string s)
        {
            if (String.IsNullOrEmpty(s)) return new DateTime[0];
            string[] items = s.Split('|');
            DateTime[] result = new DateTime[items.Length];
            for (int i = 0; i < items.Length; i++)
                result[i] = DateTime.Parse(items[i], CultureInfo.InvariantCulture);
            return result;
        }


        /// <summary>
        /// Writes an attribute of an arbitrary type for a given variable.
        /// </summary>
        internal void WriteNetCdfAttribute(int varid, string name, object value, AttributeTypeMap atm)
        {
            if (atm != null)
                atm.Remove(name); // type of an attribute may change!

            int res;
            if (value == null)
            {
                if (atm != null)
                    atm[name] = null;
                res = NetCDF.nc_put_att_string(NcId, varid, name, new string[] { null });
                HandleResult(res);
            }
            else if (value is string strValue)
            {
                res = NcPutAttText(NcId, varid, name, strValue);
                HandleResult(res);
            }
            else if (value is string[] strArr)
            {
                res = NetCDF.nc_put_att_string(NcId, varid, name, strArr);
                HandleResult(res);
            }
            else if (value is DateTime)
            {
                if (atm == null)
                    throw new InvalidOperationException("Cannot write DateTime without AttributeTypeMap");
                atm[name] = typeof(DateTime);
                string s = DateTimesToString(new DateTime[] { (DateTime)value });
                res = NcPutAttText(NcId, varid, name, s);
                HandleResult(res);
            }
            else if (value is DateTime[])
            {
                if (atm == null)
                    throw new InvalidOperationException("Cannot write array of DateTime without AttributeTypeMap");
                atm[name] = typeof(DateTime[]);
                string s = DateTimesToString((DateTime[])value);
                res = NcPutAttText(NcId, varid, name, s);
                HandleResult(res);
            }
            else if (value is bool)
            {
                if (atm == null)
                    throw new InvalidOperationException("Cannot write DateTime without AttributeTypeMap");
                atm[name] = typeof(bool);
                byte bvalue = ((bool)value) ? (byte)1 : (byte)0;
                res = NetCDF.nc_put_att_ubyte(NcId, varid, name, new byte[] { bvalue });
                HandleResult(res);
            }
            else if (value is bool[])
            {
                if (atm == null)
                    throw new InvalidOperationException("Cannot write DateTime without AttributeTypeMap");
                atm[name] = typeof(bool[]);
                byte[] bvalue = Array.ConvertAll((bool[])value, p => p ? (byte)1 : (byte)0);
                res = NetCDF.nc_put_att_ubyte(NcId, varid, name, bvalue);
                HandleResult(res);
            }
            else // Numeric types
            {
                if (value is Array && ((Array)value).Length == 1)
                    if (atm != null)
                        atm[name] = value.GetType();

                if (value is byte)
                    value = new byte[] { ((byte)value) };
                else if (value is sbyte)
                    value = new sbyte[] { ((sbyte)value) };
                else if (value is short)
                    value = new short[] { ((short)value) };
                else if (value is int)
                    value = new int[] { ((int)value) };
                else if (value is ushort)
                    value = new ushort[] { ((ushort)value) };
                else if (value is uint)
                    value = new uint[] { ((uint)value) };
                else if (value is Int64)
                    value = new Int64[] { ((Int64)value) };
                else if (value is UInt64)
                    value = new UInt64[] { ((UInt64)value) };
                else if (value is float)
                    value = new float[] { ((float)value) };
                else if (value is double)
                    value = new double[] { ((double)value) };

                if (value.GetType().GetElementType() == typeof(sbyte))
                    NetCDF.nc_put_att_schar(NcId, varid, name, ((sbyte[])value));
                else if (value.GetType().GetElementType() == typeof(byte))
                    NetCDF.nc_put_att_ubyte(NcId, varid, name, ((byte[])value));
                else if (value.GetType().GetElementType() == typeof(short))
                    NetCDF.nc_put_att_short(NcId, varid, name, ((short[])value));
                else if (value.GetType().GetElementType() == typeof(int))
                    NetCDF.nc_put_att_int(NcId, varid, name, ((int[])value));
                else if (value.GetType().GetElementType() == typeof(ushort))
                    NetCDF.nc_put_att_ushort(NcId, varid, name, ((ushort[])value));
                else if (value.GetType().GetElementType() == typeof(uint))
                    NetCDF.nc_put_att_uint(NcId, varid, name, ((uint[])value));
                else if (value is float[])
                    NetCDF.nc_put_att_float(NcId, varid, name, ((float[])value));
                else if (value is double[])
                    NetCDF.nc_put_att_double(NcId, varid, name, ((double[])value));
                else if (value.GetType().GetElementType() == typeof(Int64))
                    NetCDF.nc_put_att_longlong(NcId, varid, name, ((Int64[])value));
                else if (value.GetType().GetElementType() == typeof(UInt64))
                    NetCDF.nc_put_att_ulonglong(NcId, varid, name, ((UInt64[])value));
                else
                    throw new ArgumentException("Unsupported type of attribute", "value");

            }
        }

        /// <summary>
        /// Handles the result code for NetCDF operations.
        /// In case of an error throws the NetCDFException exception.
        /// </summary>
        /// <exception cref="NetCDFException" />
        internal static void HandleResult(int resultCode)
        {
            if (resultCode == (int)ResultCode.NC_NOERR)
                return;

            throw new NetCDFException(resultCode);
        }

        #endregion

        #region IChunkSizesAdjustable implementation

        /// <summary>
        /// Sets the chunk sizes (in data elements).
        /// </summary>
        /// <seealso cref="IChunkSizesAdjustable"/>
        public void SetChunkSizes(int[] sizes)
        {
            chunkSizes = sizes;
        }

        internal int[] ChunkSizes { get { return chunkSizes; } }

        #endregion
    }

    internal class AttributeTypeMap
    {
        public const string AttributeName = "_dataSetAttTypeMap";
        public const string AttributeVarSpecialType = "_varSpecType";
        public const string AttributeVarActualShape = "_varActualShape";

        private int ncid;
        private int varid;
        private Dictionary<string, Type> map = new Dictionary<string, Type>();
        private bool modified = false;

        public AttributeTypeMap(int ncid, int varid)
        {
            this.ncid = ncid;
            this.varid = varid;
            NcType type;
            IntPtr len;
            int resultCode = NetCDF.nc_inq_att(ncid, varid, AttributeName, out type, out len);
            if (resultCode == (int)ResultCode.NC_NOERR &&
                (type == NcType.NC_STRING || type == NcType.NC_CHAR))
            {
                string typeMapString = NetCDFDataSet.NcGetAttText(ncid, varid, AttributeName, (int)len, out resultCode);
                if (resultCode == (int)ResultCode.NC_NOERR)
                    Parse(typeMapString.Substring(0, Math.Min((int)len, typeMapString.Length)));
            }
        }

        public void Store()
        {
            if (modified)
            {
                string s = ToString();
                int res = NetCDFDataSet.NcPutAttText(ncid, varid, AttributeName, s);
                NetCDFDataSet.HandleResult(res);
                modified = false;
            }
        }

        private void Parse(string s)
        {
            modified = false;
            map.Clear();
            Dictionary<string, Type> m = new Dictionary<string, Type>();
            foreach (string def in s.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] pair = def.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (pair.Length != 2)
                {
                    Trace.WriteLineIf(NetCDFDataSet.TraceNetCDFDataSet.TraceError, "Syntax error while parsing _dataSetAttTypeMap attribute");
                    return;
                }
                try
                {
                    if (pair[1] == "(null)")
                        m.Add(pair[0], null);
                    else
                        m.Add(pair[0], DataSet.GetType(pair[1]));
                }
                catch (Exception exc)
                {
                    Trace.WriteLineIf(NetCDFDataSet.TraceNetCDFDataSet.TraceError, "Exception while parsing _dataSetAttTypeMap: " + exc.Message);
                    return;
                }
            }
            map = m;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, Type> pair in map)
                sb.AppendFormat("{0} {1};", pair.Key, pair.Value == null ? "(null)" : pair.Value.FullName);
            return sb.ToString();
        }

        public void Remove(string name)
        {
            modified = modified || map.Remove(name);
        }

        public Type this[string name]
        {
            get
            {
                return map[name];
            }
            set
            {
                if (map.ContainsKey(name))
                {
                    if (map[name] != value)
                    {
                        map[name] = value;
                        modified = true;
                    }
                }
                else
                {
                    map.Add(name, value);
                    modified = true;
                }
            }
        }



        public bool IsArray(string name)
        {
            Type type;
            if (map.TryGetValue(name, out type))
                return type.IsArray;
            else
                return false;
        }

        public bool Contains(string name)
        {
            return map.ContainsKey(name);
        }
    }
}

