// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Microsoft.Research.Science.Data.CSV
{
    /// <summary>
    /// A <see cref="DataSet"/> provider that stores data in a delimited text file as a flat table.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="CsvDataSet"/> provider stores data in a delimited text file as 
    /// a flat table; in particular the CSV file format is supported. CSV stands for Comma Separated Values.
    /// Classic CSV file is a text file that represents data as a table, where each line corresponds to a row in the table.
    /// Columns within a line are separated by commas.
    /// CSV files are convenient for they are human readable and supported by many applications, including Microsoft Excel.
    /// </para>
    /// <para>
    /// The <see cref="CsvDataSet"/> provider extends the described file format. First, it supports
    /// different delimeters (comma, semicolon, tab and space; see URI parameter <c>"separator"</c>). 
    /// Second, it may append an output file
    /// by another tables (separated by empty lines from the main data table; see URI parameter <c>"appendMetadata"</c>). 
    /// These extra tables contain
    /// information about data stucture and metadata (see also <see cref="MetadataDictionary"/>). 
    /// </para>
    /// <para>
    /// First line of a file contains display names of variables.
    /// Use the <c>"noHeader"</c> or <c>"saveHeader"</c> parameters of a URI to change it.
    /// If the file doesn't contain metadata table and thus do not define variable's
    /// names explicitly, display names also are used as names of the variables.
    /// </para>
    /// <para>
    /// The provider supports variables of any non-negative rank. 
    /// Two-dimensional variables are stored as several 
    /// consequent columns within a file
    /// (first index is a row, second index is a column).
    /// Only first column of these columns has header; other headers corresponding to the variable are empty.
    /// This rule is a criteria of a  multidimensional variable within an input file. But if the first column in a file
    /// is empty, the corresponding variable is always considered as one-dimensional.
    /// </para>	
    /// <para>
    /// Three and more dimensional variables are also stored as several consequent columns.
    /// The rule is: last index corresponds to columns, the preceding index corresponds
    /// to rows, and other indices are placed consequently in rows below (last indices vary faster).
    /// </para>
    /// <para>
    /// The provider is associated with the provider name "csv" and extensions ".csv" and ".tsv"
    /// (read about the DataSet factory: <see cref="Microsoft.Research.Science.Data.Factory.DataSetFactory"/>).
    /// </para>
    /// <para>
    /// About an input and output files encodings please refer a description of a URI parameter <c>"utf8bom"</c>
    /// in the table below.
    /// </para>
    /// <para>
    /// <see cref="CsvDataSet"/> accepts following parameters in a URI
    /// (read about DataSet URIs here: <see cref="DataSet.Open(string)"/>;
    /// about preliminary URI customization <see cref="DataSetUri.Create(string)"/>):
    /// <list type="table">
    /// <listheader>
    ///   <term>Parameter</term>
    ///   <description>Description</description>
    /// </listheader>
    ///  <item>
    ///		<term>file</term>
    ///		<description>Specifies the file to open or create. This can be either
    ///		a local file path or a HTTP URL.
    ///		Examples: <c>"msds:csv?file=c:\data\data.csv"</c>, 
    ///		<c>"msds:csv?file=http://arca7.wdcb.ru/DataExplorer/data.csv"</c>.
    ///		If the HTTP URL is specified, the file must exist and the constructed data set is always read only.
    ///		If there is a need to modify it, consider cloning it into a <see cref="T:MemoryDataSet"/>.
    ///		See <see cref="DataSet.Clone(string)"/>.</description>
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
    ///     <para>If the target is read only or is a HTTP URL, the constructed DataSet is always read only.
    ///     Consider cloning it into a <see cref="T:MemoryDataSet"/> to modify.
    ///     See <see cref="DataSet.Clone(String)"/>.</para>
    ///		</description>
    /// </item>
    /// <item>
    ///		<term>appendMetadata=true|false</term>
    ///		<description>If the value is true, the provider appends an output file with special 
    ///		metadata tables, describing structure of the DataSet. These tables
    ///		can be a reason of incompatibility for some CSV-oriented programs. 
    ///		In this case set the parameter value to false.
    ///		If the parameter is not specifed, metadata will be appended only if the input file have 
    ///		had metadata appended.
    ///		</description>
    /// </item>
    /// <item>
    ///		<term>noHeader=true|false</term>
    ///		<description>If the parameter is false, the first line of an input file
    ///		is a header containing display names of variables.
    ///		Otherwise, first line contains data. 
    ///		Note that this parameter doesn't affect an output file. See also
    ///		parameter "saveHeader".
    ///		Default is false (i.e. an input file must have a header line).
    ///		</description>
    /// </item>
    /// <item>
    /// <term>
    /// saveHeader=true|false
    /// </term>
    /// <description>
    /// Parameter defines whether the provider must save a header line into an output file or not.
    /// If it is not specified, its value depends on the "noHeader" parameter:
    /// if the input file has no header, the header is not saved in the output file, too; and vice versa.
    /// </description>
    /// </item>
    /// <item>
    ///		<term>fillUpMissingValues=true|false</term>
    ///		<description>Indicates whether the CSV provider should interpreter empty values
    ///		(i.e. empty strings between separators)
    ///		in an input file as missing values or not. Default is false.</description>
    /// </item>
    ///  <item>
    ///		<term>inferInt=true|false</term>
    ///		<description>If the value is true, the CSV provider infers following types on loading:
    ///		int, double, DateTime, string. Otherwise, only double, DateTime and string are inferred.
    ///		Default is false.</description>
    /// </item>
    ///  <item>
    ///		<term>culture</term>
    ///		<description>The culture name that is used to parse an input file. Default is an invariant culture.
    ///		Note that this parameter does not affect an output file for it is written for an invariant culture
    ///		always.
    ///		Example: <c>"msds:csv?file=data-gb.csv&amp;culture=en-GB"</c>.</description>
    /// </item>
    ///  <item>
    ///		<term>inferDims=true|false</term>
    ///		<description><para>If the value is true, when parsing a file, 
    ///		all variables (that have no metadata defined for them)
    ///		with same length are considered as 
    ///		sharing same dimensions. Default is false.</para>
    ///		<para>
    ///		In the following example, if the inferDims is true,
    ///		variables <c>a</c> and <c>c</c> depend on same dimension;
    ///		<c>b</c> and <c>d</c> also depend on same dimension, different than the former one.
    ///		If the inferDims is false, all variables depend on different dimensions.
    ///		<code>
    ///		a	b	c	d
    ///		1	10	100	string1
    ///		2	20	200	string2
    ///		3	  	300	
    ///		</code>
    ///		</para>
    ///		</description>
    /// </item>
    ///  <item>
    ///		<term>utf8bom=true|false</term>
    ///		<description>If utf8bom parameter is defined and is equal to
    /// <list type="bullet">
    /// <item>true, output encoding is UTF8 with signature;</item>
    /// <item>false, output encoding is UTF8 without signature.</item>
    /// </list>
    /// If the parameter utf8bom is not defined, and if a file exists, its encoding is kept for an output,
    /// otherwise, if the underlying file is created, UTF8 with signature is used.</description>
    /// </item>
    ///  <item>
    ///		<term>separator=comma|semicolon|space|tab</term>
    ///		<description>Determines a char that separates values in the file.
    ///		Default is "comma".</description>
    /// </item>
    ///  <item>
    ///		<term>include</term>
    ///		<description>The parameter allows including variables as references from another DataSet, 
    ///		that is defined as a URI,
    ///		into this DataSet. Note, that only variable names conforming
    ///		C# or Python 3 naming rules, can be specified in the include parameter.
    ///		Example: <c>msds:memory?include=msds%3Acsv%3Ffile%example.csv%23lat%2Clon</c> 
    ///		(escaped version of <c>"msds:memory?include=escape[msds:csv?file=example.csv#lat,lon]"</c>)
    ///		includes variables <c>lat</c> and <c>lon</c> from <c>msds:csv?file=example.csv</c>. If variables names are not specified,
    ///		all variables are included.</description>
    /// </item>
    /// </list>
    /// </para>	
    /// <para>
    /// The <see cref="CsvDataSet"/> provider supports URIs containing a path
    /// and appended through '?' parameters: <c>c:\data\air0.csv?openMode=open&amp;fillUpMissingValues=true&amp;inferInt=true</c>.
    /// </para>
    /// <example>
    /// The following example creates a <see cref="CsvDataSet"/> from existing file <c>example.csv</c>
    /// and requests that all variables with same number of elements would depend on same dimension.
    /// <code>
    /// using (DataSet ds = DataSet.Create("msds:csv?file=example.csv&amp;openMode=open&amp;inferDims=true"))
    ///	{
    ///		// Working with ds . . .
    ///	}
    /// </code>
    /// </example>
    /// <para><c>CsvDataSet</c> provides a special metadata attribute with a key "csv_column". 
    /// This value is an integer zero-based
    /// number of a column within a file that corresponds a variable. 
    /// For multidimensional variables those are spead on several columns, this value is an index of its 
    /// first column. A "csv_column" attribute is generated automatically when dataset is loaded. 
    /// If a file already contains a
    /// "csv_column" attribute for some variables, these attributes are ignored and a warning is traced.</para>
    /// <para>A "csv_column" attribute is writable, but now its modification doesn't affects output file. It will be
    /// taken into account in future releases.</para>
    /// <para>
    /// The <c>CsvDataSet</c> saves <see cref="DateTime"/> variables 
    /// using the following format string: <c>"yyyy-MM-dd hh:mm:ss"</c> (e.g. <c>"1999-04-20 14:10:00"</c>), 
    /// acceptable by Microsoft Excel. But this DateTime format provides a precision to within a second.
    /// If greater precision is required, it is possible to store <see cref="P:System.DateTime.Ticks"/> using
    /// a variable of type <see cref="T:System.Int64"/>. 
    /// </para>
    /// </remarks>
    [DataSetProviderName("csv")]
    [DataSetProviderFileExtension(".csv")]
    [DataSetProviderFileExtension(".tsv")]
    [DataSetProviderUriTypeAttribute(typeof(CsvUri))]
    public class CsvDataSet : DataSet
    {
        /// <summary>Key name for special metadata key that indicates zero-based column index in CSV file</summary>
        public const string CsvColumnKeyName = "csv_column";

        /// <summary>
        /// Filters trace messages related to the DataSet.
        /// </summary>
        internal static TraceSwitch TraceCsvDataSet = DataSet.TraceDataSet;

        private bool initialized = false;
        private bool appendMetadata = true;

        private bool enableIntInTypeInfer = false;
        private bool enableFlatVariables = true;

        /// <summary>If true, puts the UTF8 Byte order mark in the begginning of the output file.</summary>
        private bool utf8BOM = true;

        /// <summary>Interpreter empty values as missing values.</summary>
        private bool fillUpMissingValues = false;

        /// <summary>If the encoding is specified explicitly, it is used on saving output file.
        /// Otherwise, UTF8 is used either with BOM or not</summary>
        private Encoding fileEncoding = null;

        /// <summary>The char that separates values in the file.</summary>
        private char separator;
        /// <summary>The char that separates values within a single column, if need.</summary>
        private char innerSeparator;

        /// <summary>
        /// If true, the during loading data set,
        /// all variables with same length are considered as sharing the same dimension.
        /// </summary>
        private bool inferDims = false;

        /// <summary>
        /// Culture determined by the "culture" parameter of the uri.
        /// </summary>
        private CultureInfo culture;

        /// <summary>
        /// Initializes the data set from the uri and returns the instance.
        /// </summary>
        /// <param name="uri">Either a file name or DataSet uri formed for the csv provider.</param>
        /// <remarks>
        /// <para>
        /// If the open mode is not specified in the <paramref name="uri"/>, the default value 
        /// <paramref name="ResourceOpenMode.OpenOrCreate"/> is used.
        /// </para>
        /// </remarks>
        public CsvDataSet(string uri)
            : base(true, true)
        {
            if (DataSetUri.IsDataSetUri(uri))
                this.uri = new CsvUri(uri);
            else
                this.uri = CsvUri.FromFileName(uri);

            DataSetUri.NormalizeUri(this.uri);
            CsvUri _uri = (CsvUri)this.uri;

            this.enableIntInTypeInfer = _uri.InferInt;
            this.utf8BOM = _uri.UTF8BomEnabled;
            this.fillUpMissingValues = _uri.FillUpMissingValues;
            this.appendMetadata = _uri.AppendMetadata;
            this.culture = _uri.Culture;

            this.separator = DelimiterToChar(((CsvUri)_uri).Separator);
            if (!((CsvUri)_uri).HasSeparator)
            {
                string ext = System.IO.Path.GetExtension(((CsvUri)_uri).FileName).ToLower();
                if (ext == ".tsv") separator = '\t';
            }
            this.innerSeparator = (separator == ' ' || separator == '\t') ? ',' : ' ';

            this.inferDims = ((CsvUri)_uri).InferDims;

            ResourceOpenMode openMode = _uri.GetOpenModeOrDefault(ResourceOpenMode.OpenOrCreate);

            bool autocommit = IsAutocommitEnabled;
            IsAutocommitEnabled = false;
            InitializeFromCsv(openMode, false);
            IsAutocommitEnabled = autocommit;
        }

        private char DelimiterToChar(Delimiter delimiter)
        {
            switch (delimiter)
            {
                case Delimiter.Comma:
                    return ',';
                case Delimiter.Semicolon:
                    return ';';
                case Delimiter.Tab:
                    return '\t';
                case Delimiter.Space:
                    return ' ';
                default:
                    throw new NotSupportedException("Unknown delimiter");
            }
        }

        /// <summary>
        /// Gets the value indicating whether the type inferring mechanism has used integers (System.Int32) or not during the lading process.
        /// </summary>
        /// <remarks>
        /// For comatibility it is often convenient not to infer numbers as integers, but as doubles, since
        /// 1,2,3 are also 1.0,2.0,3.0 and there is a chance that a user awaits doubles and the type inferer
        /// will considers them as integers, that leads to a type mismatch exception.
        /// So, if this property is set to false, the most common type - double - is used in any of this case
        /// (i.e. 1,2,3 will be considered as doubles).
        /// If a user is confident that all numbers are integers it is still possible to set the property to true.
        /// Default value is false.
        /// </remarks>
        public bool IntegersInTypeInferringEnabled
        {
            get { return enableIntInTypeInfer; }
        }

        /// <summary>
        /// Gets or sets the values indicating whether to save multidimensional variables as flat table or transform it into 
        /// a single column.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Flat representation of 2d variables uses serveral columns to store the single array:
        /// </para>
        /// <code>
        /// A	B			C
        /// 1	11	12	13	a	
        /// 2	21	22	23	b
        /// </code>
        /// <para>
        /// In the example, A and C are one-dimensional variables, and B is a variable with shape 2 rows x 3 columns.
        /// To indicate a flat variable, its first column has a header text ("B") but following columns have
        /// empty headers. If the first column has empty header, then use of flat 2d variables is prohibited.
        /// </para>
        /// <para>
        /// If flat variables are disabled, 2d-variables are transformed into a single column, row above row,
        /// so that the former example should be written as follows:
        /// </para>
        /// <code>
        /// A	B	C
        /// 1	11	a
        /// 2	21	b
        ///		12
        ///		22
        ///		13
        ///		23
        /// </code>
        /// <para>
        /// The property <see cref="EnableFlatVariables"/> makes the provider to save multidimensional variables as flat 
        /// variables. But it doesn't affect loading process which thus can has variables in both representations.
        /// Also, flat variables can be enabled in the only case if all variable have display names, otherwise an
        /// exception will be thrown during save process.
        /// </para>
        /// <para>
        /// The URI of CsvDataSet supports special parameter "flatVariables" which can be either "true"
        /// or "false" which initializes the property at construction.
        /// </para>
        /// </remarks>
        public bool EnableFlatVariables
        {
            get { return enableFlatVariables; }
            set
            {
                if (enableFlatVariables != value)
                    StartChanges();

                enableFlatVariables = value;
            }
        }

        /// <summary>
        /// Gets or sets the value indicating whether the metadata table is appended to the output file or not.
        /// </summary>
        /// <remarks>
        /// If the property is set, the provider saves to the end of an output file special metadata tables which
        /// can be a reason of incompatibility for some CSV-oriented programs. In the case you should set it false.
        /// If this property is changed, it starts a transaction to make it possible to save the data set through
        /// the Commit() method.
        /// </remarks>
        public bool AppendMetadata
        {
            get { return appendMetadata; }
            set
            {
                if (appendMetadata != value)
                    StartChanges();

                appendMetadata = value;
            }
        }

        /// <summary>
        /// Selects variables implementing ICsvVariable interface
        /// ans sorts them by their index.
        /// </summary>
        /// <param name="variables"></param>
        /// <returns></returns>
        private List<Variable> GetCsvVariables(ICollection<Variable> variables)
        {
            List<Variable> csvVars = new List<Variable>(variables.Count);
            foreach (Variable var in variables)
            {
                ICsvVariable csv = var as ICsvVariable;
                if (csv != null)
                {
                    csvVars.Add(var);
                }
            }

            csvVars.Sort((u, v) =>
                {
                    int ui = ((ICsvVariable)u).ColumnIndex;
                    int vi = ((ICsvVariable)v).ColumnIndex;
                    return ui - vi;
                });
            return csvVars;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="DataType"></typeparam>
        /// <param name="varName"></param>
        /// <param name="dims"></param>
        /// <returns></returns>
        protected override Variable<DataType> CreateVariable<DataType>(string varName, string[] dims)
        {
            if (dims == null)
                dims = new string[0]; // scalar

            // Looking for duplicate names
            ICollection<Variable> vars = GetRecentVariables();
            int index = -1;
            foreach (Variable w in vars)
            {
                ICsvVariable csvvar = w as ICsvVariable;
                if (csvvar == null) continue;

                if (csvvar.ColumnIndex > index)
                    index = csvvar.ColumnIndex;
            }
            index++;
            // Real index is to be assigned at the precommitting.
            // Creating a variable			
            CsvColumn col = new CsvColumn(index, -1 /* to be assigned by DataSet */, varName);
            col.VariableName = varName;
            col.Rank = dims.Length;
            col.Dims = dims;
            col.Type = typeof(DataType);

            Variable<DataType> var = (Variable<DataType>)ColumnToVariable(col);
            return var;
        }

        #region Transacions

        /// <summary>
        /// Updates column indices of variables before committing.
        /// </summary>
        /// <param name="changes"></param>
        /// <returns></returns>
        protected override bool OnPrecommitting(DataSet.Changes changes)
        {
            // Updating column indices in variables.
            var vars = GetCsvVariables(changes.Variables);

            Variable.Changes varChanges;
            int i = 0;
            foreach (var v in vars)
            {
                int column = ((ICsvVariable)v).ColumnIndex;
                varChanges = null;
                if (column < 0 || column != i)
                {
                    // Updating column:
                    varChanges = changes.GetVariableChanges(v.ID);
                    if (varChanges == null)
                    {
                        var initialSchema = v.GetSchema(SchemaVersion.Committed);
                        var shape = initialSchema.Dimensions.AsShape();
                        varChanges = new Variable.Changes(v.Version + 1,
                            initialSchema,
                            StartChangesFor(Metadata),
                            null, shape, new Rectangle());
                        changes.UpdateChanges(varChanges);
                    }
                    else if (varChanges.MetadataChanges == null)
                    {
                        varChanges = new Variable.Changes(varChanges.ChangeSet,
                            varChanges.InitialSchema,
                            StartChangesFor(Metadata),
                            varChanges.CoordinateSystems,
                            varChanges.Shape,
                            varChanges.AffectedRectangle);
                        changes.UpdateChanges(varChanges);
                    }
                    varChanges.MetadataChanges[CsvColumnKeyName] = i;
                }

                if (enableFlatVariables && v.Rank >= 2)// empty headers for flat variable
                {
                    int[] varShape;

                    if (varChanges == null)
                        varChanges = changes.GetVariableChanges(v.ID);
                    if (varChanges == null)
                        varShape = v.GetShape();
                    else
                        varShape = varChanges.Shape;
                    int columnsNumber = varShape[varShape.Length - 1];
                    if (columnsNumber > 0) i += columnsNumber;
                    else i++;
                }
                else i++;
            }

            return true;
        }

        /// <summary>Previous version of CSV file is here after precomitting</summary>
        private string ensconcedFileName = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="changes"></param>
        protected override void OnPrecommit(DataSet.Changes changes)
        {
            // If not initialized then we're just committing changes after loading
            if (!initialized) return;

            if (ensconcedFileName == null)
            {
                ensconcedFileName = Path.GetTempFileName();
            }

            // Saving new data as this.uri
            if (readOnly)
                throw new ReadOnlyException("DataSet opened as read only therefore cannot save changes");
            SaveToFile(GetRecentVariables(), ensconcedFileName);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnCommit()
        {
            // If not initialized then we're just committing changes after loading
            if (!initialized) return;

            if (ensconcedFileName == null)
                return;

            string fileName = ((CsvUri)uri).FileName;
            int tries = 1000;
            do
            {
                try
                {
                    File.Delete(fileName);
                    File.Move(ensconcedFileName, fileName);
                    ensconcedFileName = null;
                }
                catch (IOException ex)
                {
                    Trace.WriteLineIf(TraceCsvDataSet.TraceWarning, "The underlying file is locked by another application. Retrying operation...");
                    Thread.Sleep(10);
                    if (--tries == 0)
                    {
                        File.Delete(ensconcedFileName);
                        ensconcedFileName = null;
                        throw new DataSetException("Cannot update the underlying file for it is locked by another application", ex);
                    }
                }
            } while (ensconcedFileName != null);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnRollback()
        {
            // If not initialized then we're just committing changes after loading
            if (!initialized) return;

            if (ensconcedFileName != null)
            {
                File.Delete(ensconcedFileName);
                ensconcedFileName = null;

                ClearChanges();

                bool autocommit = IsAutocommitEnabled;
                try
                {
                    IsAutocommitEnabled = false;
                    InitializeFromCsv(ResourceOpenMode.Open, true);
                }
                finally
                {
                    IsAutocommitEnabled = autocommit;
                }
            }
        }

        #endregion

        #region Low-level routines

        #region Save

        /// <summary>
        /// Makes the string proper to write in a CSV file.
        /// </summary>
        private string SafeString(string s)
        {
            if (String.IsNullOrEmpty(s))
                return "\"\"";
            if (s.Contains('"'))
                return string.Format("\"{0}\"", s.Replace("\"", "\"\""));
            if (s.Contains(separator) || s.Contains('\n'))
                return string.Format("\"{0}\"", s);
            return s;
        }

        /// <summary>
        /// Makes the string proper to write in a CSV file.
        /// </summary>
        private string SafeStringWithInnerSeparator(string s)
        {
            if (String.IsNullOrEmpty(s))
                return "";
            if (s.Contains('"'))
                return string.Format("\"{0}\"", s.Replace("\"", "\"\""));
            if (s.Contains(separator) || s.Contains(innerSeparator) || s.Contains('\n'))
                return string.Format("\"{0}\"", s);
            return s;
        }


        /// <summary>
        /// Saves the DataSet proposed changes into the given file.
        /// </summary>
        /// <param name="vars"></param>
        /// <param name="fileName"></param>
        private void SaveToFile(ICollection<Variable> vars, string fileName)
        {
            if (vars.Count > 0)
                SaveCsv(new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write), vars);
            else
                new FileStream(fileName, FileMode.Create, FileAccess.Write).Dispose();
        }

        /// <summary>
        /// Saves the DataSet proposed changes into the given stream.
        /// </summary>
        /// <param name="outputStream"></param>
        /// <param name="variables"></param>
        private void SaveCsv(Stream outputStream, ICollection<Variable> variables)
        {
            CultureInfo originCI = null;

            try
            {
                originCI = Thread.CurrentThread.CurrentCulture;
                CultureInfo newCI = (CultureInfo)CultureInfo.InvariantCulture;
                Thread.CurrentThread.CurrentCulture = newCI;

                var csvVars = GetCsvVariables(variables);

                Encoding enc = ((CsvUri)uri).Encoding;
                if (enc == null)
                {
                    if (((CsvUri)uri).UTF8BomDefined) // specified in the uri
                        enc = ((CsvUri)uri).UTF8BomEnabled ? Encoding.UTF8 : null;
                    else
                        enc = utf8BOM ? fileEncoding : null; // might be null; then default is UTF8 without BOM
                }

#if DEBUG
                if (enc != null)
                    Debug.WriteLineIf(TraceCsvDataSet.TraceInfo, "Csv output encoding: " + enc.EncodingName);
                else
                    Debug.WriteLineIf(TraceCsvDataSet.TraceInfo, "Csv output encoding: UTF8 without BOM");
#endif

                using (StreamWriter sw = enc == null ?
                    new StreamWriter(outputStream) :
                    new StreamWriter(outputStream, enc))
                {
                    // All variables must have display names
                    if (enableFlatVariables)
                    {
                        bool isNd = false;
                        bool isEmpty = false;
                        foreach (Variable v in csvVars)
                        {
                            isNd = v.Rank >= 2;
                            isEmpty = String.IsNullOrEmpty((string)v.Metadata["DisplayName", SchemaVersion.Recent]);
                            if (isNd && isEmpty) break;
                        }
                        if (isNd && isEmpty)
                            throw new Exception("[CSV] WARNING: DataSet contains multidimensional (rank > 1) variables with empty DisplayName attribute with flat variables enabled. " +
                                "This will lead to incorrect further file loading");
                    }

                    Dictionary<int, string> idToColumn = new Dictionary<int, string>(csvVars.Count);

                    /********************************************************************************/
                    /* HEADER                                                                       */
                    int i = 0, n = 0;
                    int columnsN;
                    if (((CsvUri)uri).SaveHeader)
                    {
                        foreach (Variable v in csvVars)
                        {
                            if (n > 0)
                                sw.Write(separator);

                            if (v.Metadata["DisplayName", SchemaVersion.Recent] != null)
                                sw.Write(SafeString(v.Metadata["DisplayName", SchemaVersion.Recent].ToString()));

                            if (enableFlatVariables && v.Rank >= 2)// empty headers for flat variable
                            {
                                var dims = ((ICsvVariable)v).GetRecentDimensions();
                                int columnsNumber = dims[v.Rank - 1].Length;  // last index ~ cols

                                idToColumn[v.ID] = String.Format("{0}:{1}",
                                    CsvDataSet.GetNameByIndex(i),
                                    CsvDataSet.GetNameByIndex((columnsNumber == 0) ? i : (i + columnsNumber - 1)));

                                for (int k = 1; k < columnsNumber; k++, n++)
                                    sw.Write(separator);
                                if (columnsNumber > 0)
                                {
                                    i += columnsNumber - 1;
                                }
                            }
                            else
                            {
                                idToColumn[v.ID] = CsvDataSet.GetNameByIndex(i);
                            }

                            n++;
                            i++;
                        }
                        columnsN = n;
                        sw.WriteLine();
                    }
                    else
                    {
                        // Counting data columns
                        foreach (Variable v in csvVars)
                        {
                            if (enableFlatVariables && v.Rank >= 2)// empty headers for flat variable
                            {
                                var dims = ((ICsvVariable)v).GetRecentDimensions();
                                int shape = dims[v.Rank - 1].Length;
                                if (shape > 1)
                                    n += shape - 1;
                            }
                            n++;
                        }
                        columnsN = n;
                    }

                    /********************************************************************************/
                    /* DATA                                                                         */
                    Array[] data = new Array[columnsN];
                    i = 0; n = 0;

                    // Preparing arrays to further write
                    foreach (Variable v in csvVars)
                    {
                        if (enableFlatVariables && v.Rank >= 2)
                        {
                            var dims = ((ICsvVariable)v).GetRecentDimensions();
                            int shape = dims[v.Rank - 1].Length;
                            var csvv = ((ICsvVariableMd)v);

                            if (shape == 0)
                                data[i++] = null;
                            else
                                for (int k = 0; k < shape; k++)
                                {
                                    data[i++] = csvv.GetColumnData(k);
                                }
                        }
                        else
                        {
                            data[i++] = ((ICsvVariable)v).ReadOnlyInnerData;
                        }
                    }

                    // Writing arrays
                    int linesNumber = 0;
                    for (i = 0; i < data.Length; i++)
                        if (data[i] != null && data[i].Length > linesNumber)
                            linesNumber = data[i].Length;
                    for (int line = 0; line < linesNumber; line++)
                    {
                        for (i = 0; i < data.Length; i++)
                        {
                            Array dataCol = data[i];
                            if (dataCol != null)
                            {
                                if (line < dataCol.Length)
                                {
                                    object o = dataCol.GetValue(line);
                                    if (o != null)
                                    {
                                        string s = o.ToString();
                                        sw.Write(SafeString(s));
                                    }
                                    else
                                    {
                                        sw.Write("\"\"");
                                    }
                                }
                            }

                            if (i < data.Length - 1)
                                sw.Write(separator);
                        }
                        sw.WriteLine();
                    }

                    if (appendMetadata)
                    {
                        /* EMPTY LINE */
                        sw.WriteLine();

                        /* METADATA */
                        if (separator == ',')
                            sw.WriteLine("ID,Column,Variable Name,Data Type,Rank,Missing Value,Dimensions");
                        else if (separator == ' ')
                            sw.WriteLine("ID Column \"Variable Name\" \"Data Type\" Rank \"Missing Value\" Dimensions");
                        else
                            sw.WriteLine("ID{0}Column{0}Variable Name{0}Data Type{0}Rank{0}Missing Value{0}Dimensions", separator);

                        // Lines for variables
                        foreach (Variable v in csvVars)
                        {
                            SaveMetadataForVariable(sw, idToColumn, v);
                        }

                        /* CS */
                        sw.WriteLine();

                        var recCS = GetCoordinateSystems(SchemaVersion.Recent);
                        CoordinateSystem[] coordinateSystems = null;
                        if (recCS.Count > 0)
                            coordinateSystems = recCS.Where(
                                cs => cs.AxesArray.All(a => a is ICsvVariable)).ToArray();
                        if (coordinateSystems != null && coordinateSystems.Length > 0)
                        {
                            if (separator == ',')
                                sw.WriteLine("Coordinate System,Axes,Variables");
                            else if (separator == ' ')
                                sw.WriteLine("\"Coordinate System\" Axes Variables");
                            else
                                sw.WriteLine("Coordinate System{0}Axes{0}Variables", separator);

                            // Lines for variables
                            i = 0; n = 0;
                            foreach (CoordinateSystem cs in coordinateSystems)
                            {
                                if (Array.Exists(cs.AxesArray, a => !(a is ICsvVariable)))
                                    continue; // saving cs with csv variables only

                                // CS Name
                                sw.Write(SafeString(cs.Name));
                                sw.Write(separator);

                                // Axes
                                n = cs.AxesCount;
                                i = 0;
                                foreach (var axis in cs.Axes)
                                {
                                    if (i++ > 0) sw.Write(innerSeparator);
                                    sw.Write(axis.ID);
                                }
                                sw.Write(separator);

                                // Variables
                                i = 0;
                                foreach (Variable v in Variables)
                                {
                                    if (v.CoordinateSystems.Contains(cs.Name, SchemaVersion.Recent))
                                    {
                                        if (i > 0) sw.Write(innerSeparator);
                                        sw.Write(v.ID);
                                        i++;
                                    }
                                }
                                sw.WriteLine();
                            }
                            sw.WriteLine();
                        }

                        /* Metadata attributes */
                        bool header = false;
                        header = SaveMetadataEntries(sw, header, Variables.GetByID(DataSet.GlobalMetadataVariableID));
                        foreach (Variable var in csvVars)
                        {
                            header = SaveMetadataEntries(sw, header, var);
                        }
                    } // end of if(appendMetadata)
                }
            }
            finally
            {
                if (originCI != null)
                {
                    Thread.CurrentThread.CurrentCulture = originCI;
                }
            }
        }

        private bool SaveMetadataEntries(StreamWriter sw, bool header, Variable var)
        {
            int id = var.ID;
            foreach (KeyValuePair<string, object> item in var.Metadata.AsDictionary(SchemaVersion.Recent))
            {
                if (id != DataSet.GlobalMetadataVariableID && item.Key == "DisplayName") continue;
                if (id != DataSet.GlobalMetadataVariableID && item.Key == var.Metadata.KeyForName) continue;
                if (id != DataSet.GlobalMetadataVariableID && item.Key == var.Metadata.KeyForMissingValue) continue;
                if (item.Key == CsvColumnKeyName) continue;

                if (!header)
                {
                    if (separator == ',')
                        sw.WriteLine("Variable,Key,Type,Value");
                    else
                        sw.WriteLine("Variable{0}Key{0}Type{0}Value", separator);
                    header = true;
                }

                string value = SerializeToString(item.Value);
                string type;
                if (item.Value == null)
                    type = "";
                else
                {
                    if (item.Value is Array)
                    {
                        string end = ((Array)item.Value).Length == 0 ? "[0]" : "[]";
                        type = item.Value.GetType().GetElementType().Name + end;
                    }
                    else
                        type = item.Value.GetType().Name;
                }

                sw.WriteLine("{0}{4}{1}{4}{2}{4}{3}",
                    var.ID,
                    SafeString(item.Key),
                    SafeString(type),
                    value,
                    separator);
            }
            return header;
        }

        private void SaveMetadataForVariable(StreamWriter sw, Dictionary<int, string> idToColumn, Variable v)
        {
            MetadataDictionary metadata = v.Metadata;

            // ID
            sw.Write(v.ID);
            sw.Write(separator);

            // Column
            sw.Write(idToColumn[v.ID]);
            sw.Write(separator);

            // Name
            sw.Write(SafeString(metadata[metadata.KeyForName, SchemaVersion.Recent].ToString()));
            sw.Write(separator);

            // Type
            sw.Write(SafeString(v.TypeOfData.Name));
            sw.Write(separator);

            // Rank
            sw.Write(v.Rank); // might be 0
            sw.Write(separator);

            // Missing Value
            var missingValue = metadata[metadata.KeyForMissingValue, SchemaVersion.Recent];
            if (missingValue != null)
                sw.Write(SerializeToString(missingValue)); // TODO: serialization of supported type
            sw.Write(separator);

            // Dimensions: might be empty for scalars
            ReadOnlyDimensionList dims = ((ICsvVariable)v).GetRecentDimensions();
            for (int j = 0; j < dims.Count; j++)
            {
                sw.Write(SafeStringWithInnerSeparator(dims[j].Name));
                sw.Write(':');
                sw.Write(dims[j].Length);
                if (j < dims.Count - 1)
                    sw.Write(innerSeparator);
            }
            sw.WriteLine();
        }

        private string SerializeToString(object o)
        {
            if (o == null)
                return "";//"(null)";
            else if (o is string)
                return SafeString((string)o);
            else if (o is System.Collections.IEnumerable)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder("");
                bool first = true;
                foreach (object item in (System.Collections.IEnumerable)o)
                {
                    if (!first) sb.Append(this.separator);
                    sb.Append(SafeString(item.ToString()));
                    first = false;
                }
                return sb.ToString();
            }
            else
                return SafeString(o.ToString());
        }

        #endregion

        #region Load

        /// <summary>
        /// Parses csv-file and thus initializes the data set.
        /// </summary>
        private void InitializeFromCsv(ResourceOpenMode openMode, bool isRollback)
        {
            initialized = false;
            List<Variable> variables = new List<Variable>();
            List<string> csList = new List<string>();
            Dictionary<int, Dictionary<string, object>> metadata = new Dictionary<int, Dictionary<string, object>>();

            string fileName = ((CsvUri)uri).FileName;
            Uri resourceUri;
            bool isLocalFile = false;
            bool res = Uri.TryCreate(fileName, UriKind.RelativeOrAbsolute, out resourceUri);
            isLocalFile = !res || !resourceUri.IsAbsoluteUri || resourceUri.Scheme == Uri.UriSchemeFile;
            bool exists = !isLocalFile || File.Exists(fileName);

            if (openMode == ResourceOpenMode.Create && exists)
            {
                if (!isLocalFile)
                    throw new NotSupportedException("Data file cannot be created through HTTP");
                File.Delete(fileName);
                exists = false;
            }

            // Parsing existing file
            if (exists)
            {
                if (openMode == ResourceOpenMode.CreateNew)
                    throw new IOException("The open mode is createNew but the file already exists");

                CultureInfo originCI = null;
                try
                {
                    originCI = Thread.CurrentThread.CurrentCulture;
                    Thread.CurrentThread.CurrentCulture = culture;

                    Stream inputStream = null;

                    if (isLocalFile)
                        inputStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    else
                    {
#if SILVERLIGHT
                        WebRequest req = WebRequest.Create(resourceUri);
                        AutoResetEvent allDone = new AutoResetEvent(false);
                        Exception ex = null;

                        var ar = req.BeginGetResponse(
                            delegate(IAsyncResult asynchronousResult)
                            {
                                try
                                {
                                    // State of request is asynchronous.							
                                    WebResponse resp = req.EndGetResponse(asynchronousResult);
                                    inputStream = resp.GetResponseStream();
                                }
                                catch (Exception exc)
                                {
                                    ex = exc;
                                }
                                allDone.Set();
                            }, null);
                        if (!allDone.WaitOne(TimeSpan.FromSeconds(30)))
                            throw new TimeoutException("Timeout waiting for server response");
                        if (ex != null)
                            throw new Exception("Failed to get server response", ex);
#else
                        WebRequest req = WebRequest.Create(resourceUri);
                        WebResponse resp = req.GetResponse();
                        inputStream = resp.GetResponseStream();
#endif
                        openMode = ResourceOpenMode.ReadOnly;
                    }

                    variables = ParseCsv(inputStream, csList, metadata);
                }
                finally
                {
                    Thread.CurrentThread.CurrentCulture = originCI;
                }
            }
            else // creating new empty file
            {
                if (openMode == ResourceOpenMode.Open || openMode == ResourceOpenMode.ReadOnly)
                    throw new ResourceNotFoundException(fileName);

                File.CreateText(((CsvUri)uri).FileName).Dispose();
                variables = new List<Variable>();
            }

            //--------------------------------------------------------
            // Initializing inner collections
            if (isRollback)
            {
                // Restoring variables in their current instances.
                var committedVars = base.variables;
                List<Variable> resultVariables = new List<Variable>();
                foreach (var cv in committedVars)
                {
                    var v = variables.Find(p => p.ID == cv.ID);
                    if (v == null) // it must be ref var or computational var
                    {
                        if (cv is ICsvVariable)
                            throw new Exception("File kept for rollback doesn't have all set of variables");
                        else
                            resultVariables.Add(cv);
                    }
                    else // data access variable
                    {
                        ((ICsvVariable)cv).ReinitializeFrom(v);
                        resultVariables.Add(cv);
                    }
                }

                variables = resultVariables;
                ClearVariableCollection();
            }
            foreach (Variable v in variables)
            {
                AddVariableToCollection(v);

                if (metadata.ContainsKey(v.ID))
                {
                    Dictionary<string, object> vm = metadata[v.ID];
                    foreach (var item in vm)
                    {
                        if (item.Key == CsvColumnKeyName)
                            Trace.WriteLineIf(TraceCsvDataSet.TraceWarning,
                                String.Format("{0} metadata key is ignored for CSV dataset", CsvColumnKeyName));
                        else
                            v.Metadata[item.Key] = item.Value;
                    }
                }
            }
            // global metadata:
            if (metadata.ContainsKey(DataSet.GlobalMetadataVariableID))
            {
                Dictionary<string, object> vm = metadata[DataSet.GlobalMetadataVariableID];
                MetadataDictionary globalMetadata = Variables.GetByID(DataSet.GlobalMetadataVariableID).Metadata;
                foreach (var item in vm)
                {
                    globalMetadata[item.Key] = item.Value;
                }
            }

            /* CS */
            if (!isRollback)
            {
                ClearCSCollection();
                if (csList.Count > 0)
                {
                    // csname, axes, var1 var2 ...
                    foreach (string csDesc in csList)
                    {
                        string[] items = CsvSplit(csDesc, separator);
                        string name = items[0];
                        int[] axisIds = CsvSplitNumbers(items[1], innerSeparator); // Like this: "12 14"
                        int[] vars = CsvSplitNumbers(items[2], innerSeparator);

                        Variable[] axes = new Variable[axisIds.Length];
                        for (int i = 0; i < axisIds.Length; i++)
                            axes[i] = Variables.GetByID(axisIds[i]);

                        CoordinateSystem cs = CreateCoordinateSystem(name, axes);

                        foreach (int vid in vars)
                        {
                            Variable v = Variables.GetByID(vid);
                            v.AddCoordinateSystem(cs);
                        }
                    }
                }
            }

            Commit();

            if (openMode == ResourceOpenMode.ReadOnly ||
                (File.GetAttributes(((CsvUri)uri).FileName) & FileAttributes.ReadOnly) != 0)
                SetCompleteReadOnly();
            initialized = true;
            splitStringBuilder = null;
        }

        private int[] CsvSplitNumbers(string str, char sep)
        {
            str = str.Trim();
            if (String.IsNullOrEmpty(str))
                return new int[0];

            var items = str.Split(sep);
            int[] nums = new int[items.Length];
            for (int i = 0; i < nums.Length; i++)
            {
                nums[i] = int.Parse(items[i].Trim());
            }
            return nums;
        }

        /// <summary>
        /// Splits the string read from CSV file into columns (unquotes quoted items)
        /// </summary>
        private string[] CsvSplit(string s, char separator)
        {
            return CsvSplit(s, separator, false);
        }

        /// <summary>
        /// Splits the string read from CSV file into columns 
        /// </summary>
        private string[] CsvSplit(string s, char separator, bool leaveQuotes)
        {
            if (String.IsNullOrEmpty(s)) return new string[] { };

            List<string> items = new List<string>();
     
            if (splitStringBuilder == null) splitStringBuilder = new StringBuilder();
            StringBuilder current = splitStringBuilder;
            bool quoted = false;
            bool wordStart = true;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (!quoted && c == '\n') break;

                if (wordStart)
                {
                    current.Clear();
                    if (c == '"')
                    {
                        quoted = true;
                        if (leaveQuotes)
                            current.Append('"');
                    }
                    else // first char is not '"'
                    {
                        if (c == separator)
                        {
                            items.Add("");
                            continue;
                        }
                        current.Append(c);
                    }
                    wordStart = false;
                }
                else // not first char in a word
                {
                    if (quoted)
                    {
                        if (c == '"') // end of quoted string
                        {
                            if (i + 1 < s.Length && s[i + 1] == '"')
                            {
                                i++;
                                current.Append(c);
                                continue;
                            }

                            quoted = false;
                            for (i++; i < s.Length; i++)
                            {
                                c = s[i];
                                if (c == separator || c == '\n') break;
                                current.Append(c);
                            }
                            if (leaveQuotes)
                            {
                                current.Append('"');
                            }
                            items.Add(current.ToString());
                            if (c == separator)
                                wordStart = true;
                            else
                                current.Clear();
                        }
                        else // inside the string
                        {
                            current.Append(c);
                        }
                    }
                    else // not quoted
                    {
                        if (c == '\n') break;
                        if (c == separator)
                        {
                            items.Add(current.ToString());
                            wordStart = true;
                        }
                        else
                        {
                            current.Append(c);
                        }
                    }
                }
            }

            if (wordStart)
                items.Add("");
            else if (current.Length != 0)
            {
                if (quoted)
                    new NotSupportedException("Multiline strings are not supported by the provider.");
                items.Add(current.ToString());
            }

            return items.ToArray();
        }

        /// <summary>
        /// Splits the string read from CSV file into columns 
        /// </summary>
        private string[] CsvSplit(StreamReader reader, char separator, out bool isEmpty)
        {
            return CsvSplit(reader, separator, false, out isEmpty);
        }

        private StringBuilder splitStringBuilder;

        /// <summary>
        /// Splits the string read from CSV file into columns 
        /// </summary>
        private string[] CsvSplit(StreamReader reader, char separator, bool leaveQuotes, out bool isEmpty)
        {
            string s = reader.ReadLine();
            isEmpty = true;
            if (s == null) return null;
            if (String.IsNullOrEmpty(s)) return new string[] { };

            List<string> items = new List<string>(16);

            if (splitStringBuilder == null) splitStringBuilder = new StringBuilder();
            StringBuilder current = splitStringBuilder;
            bool quoted = false;
            bool wordStart = true;
            bool emptyItem = true;
            do
            {
                // Parsing single line
                int n = s.Length;
                for (int i = 0; i < n; i++)
                {
                    char c = s[i];

                    if (wordStart)
                    {
                        emptyItem = true;
                        current.Clear();
                        if (c == '"')
                        {
                            quoted = true;
                            isEmpty = false;
                            emptyItem = false;
                            if (leaveQuotes)
                                current.Append('"');
                        }
                        else // first char is not '"'
                        {
                            if (c == separator)
                            {
                                // Another column parsed:
                                items.Add(null); // empty item
                                continue;
                            }
                            current.Append(c);
                            isEmpty = false;
                            emptyItem = false;
                        }
                        wordStart = false;
                    }
                    else // not first char in a word
                    {
                        if (quoted)
                        {
                            if (c == '"') // end of quoted string
                            {
                                if (i < n - 1 && s[i + 1] == '"') // "" == "
                                {
                                    i++;
                                    current.Append(c);
                                    emptyItem = false;
                                    continue;
                                }

                                quoted = false;
                                for (i++; i < n; i++)
                                {
                                    c = s[i];
                                    if (c == separator) break;
                                    current.Append(c);
                                    emptyItem = false;
                                }
                                if (leaveQuotes)
                                {
                                    current.Append('"');
                                }

                                // Another column parsed:
                                items.Add(emptyItem ? null : current.ToString());
                                isEmpty = isEmpty || emptyItem;

                                if (c == separator)
                                    wordStart = true;
                                else
                                    current.Clear();
                            }
                            else // inside the string
                            {
                                current.Append(c);
                            }
                        }
                        else // not quoted
                        {
                            if (c == separator)
                            {
                                // Another column parsed:
                                items.Add(emptyItem ? null : current.ToString());
                                isEmpty = isEmpty || emptyItem;
                                wordStart = true;
                            }
                            else
                            {
                                current.Append(c);
                                emptyItem = false;
                            }
                        }
                    }
                } // eof parsing a single line
                if (quoted)
                    current.Append('\n');
            } while (quoted && (s = reader.ReadLine()) != null);

            if (quoted)
                throw new FormatException("No matching end quotes");

            if (wordStart)
                // Another column parsed:
                items.Add(null);
            else if (current.Length != 0)
            {
                // Another column parsed:
                items.Add(emptyItem ? null : current.ToString());
                isEmpty = isEmpty || emptyItem;
            }

            return items.ToArray();
        }

        /// <summary>
        /// Parses data from *existing* csv-file.
        /// </summary>
        private List<Variable> ParseCsv(Stream s, List<string> csList, Dictionary<int, Dictionary<string, object>> metadata)
        {
            List<CsvColumn> columns = new List<CsvColumn>();
            List<string[]> readData = new List<string[]>();
            Encoding enc = ((CsvUri)uri).Encoding;

            using (StreamReader sr = enc != null ? new StreamReader(s, enc) : new StreamReader(s))
            {
                if (!sr.EndOfStream)
                {
                    fileEncoding = sr.CurrentEncoding;
                    if (s.CanSeek && fileEncoding is UTF8Encoding)
                    {
                        long pos = s.Position;
                        s.Position = 0;
                        byte b1 = (byte)s.ReadByte();
                        byte b2 = (byte)s.ReadByte();
                        byte b3 = (byte)s.ReadByte();
                        utf8BOM = (b1 == 0xEF && b2 == 0xBB && b3 == 0xBF);
                        s.Seek(0, SeekOrigin.Begin);
                        s.Position = pos;
                    }
                }

                bool hasHeader = !((CsvUri)uri).NoHeader;
                bool isEmpty;

                /******************************* HEADER *********************************/
                // First line is a header with display names
                bool flatEnabled = false;
                if (hasHeader)
                {
                    string[] ss = CsvSplit(sr, separator, out isEmpty);
                    int i = 0;
                    int ibase = 0;
                    if (ss != null)
                        foreach (string item in ss)
                        {
                            string name = item != null ? item.Trim() : null;
                            if (!flatEnabled && !String.IsNullOrEmpty(name))
                                flatEnabled = true;

                            CsvColumn col = CreateColumn(i, -1, 1, name); // let it be 1d at this moment
                            columns.Add(col);

                            if (flatEnabled && String.IsNullOrEmpty(name))
                            {
                                columns[ibase].Rank = 2; // at least 2
                                col.Rank = 2;
                                col.FlatVarBaseIndex = ibase;
                            }
                            else if (flatEnabled)
                            {
                                ibase = i;
                            }

                            i++;
                        }
                }

                /********************************* DATA ***********************************/
                // Reading lines with data, until an *empty* line is found
                string[] splitLine = null;
                while ((splitLine = CsvSplit(sr, separator, out isEmpty)) != null)
                {
                    if (isEmpty) //(IsEmptyLine(splitLine)) // empty
                        break;

                    string[] dataLine = ParseDataLine(columns, splitLine, flatEnabled);
                    readData.Add(dataLine);
                }
                if (splitLine == null) // no metadata
                {
                    return ContinueWithoutMetadata(columns, readData);
                }

                // Skipping empty lines, thus looking for an optional metadata table
                while ((splitLine = CsvSplit(sr, separator, out isEmpty)) != null)
                {
                    if (!isEmpty) //(!IsEmptyLine(splitLine))  // found
                        break;
                }
                if (splitLine == null) // no metadata
                {
                    return ContinueWithoutMetadata(columns, readData);
                }

                /****************************** METADATA ********************************/
                // Line 0: Name of the data set				
                // Skipping line with header for metadata table.
                try
                {
                    string line;

                    // Every line is a line containing metadata for a variable:
                    // ID1, Column1, Name1, DataType1, Rank, MissingValue, dim1 dim2 ..., CS:axis CS2:axis ...
                    while ((splitLine = CsvSplit(sr, separator, true, out isEmpty)) != null)
                    {
                        if (isEmpty) //(IsEmptyLine(splitLine))
                            break;

                        ParseMetadata(splitLine, columns);
                    }

                    // Coordinate Systems Metadata Table
                    // CS Name, Axes, Vars
                    // CS1, 1, 2 3 4...
                    // Skipping empty lines, thus looking for an optional metadata table
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (!IsEmptyLine(line))
                            break;
                    }
                    bool hasCSSection = line != null && line.Contains("Coordinate System");
                    if (hasCSSection) // table found
                    {
                        // Skipping line with header for metadata table.
                        while ((line = CsvReadLine(sr)) != null)
                        {
                            if (IsEmptyLine(line))
                                break;

                            csList.Add(line);
                        }
                    }

                    // Variable Metadata Table
                    // Variable Key Type Value
                    // A,Key1,Type1,Value1[,...]
                    // ...
                    // Skipping empty lines, thus looking for an optional metadata table
                    // "(null)" for strings means null, 
                    // for all others "" also means null.
                    if (hasCSSection)
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (!IsEmptyLine(line))
                                break;
                        }
                    if (line != null) // table found
                    {
                        // Skipping line with header for metadata table.
                        while ((splitLine = CsvSplit(sr, separator, out isEmpty)) != null)
                        {
                            string[] entries = splitLine;
                            if (isEmpty) //(IsEmptyLine(entries)) 
                                break;

                            int vid = int.Parse(entries[0]);

                            Dictionary<string, object> vm;
                            if (!metadata.TryGetValue(vid, out vm))
                            {
                                vm = new Dictionary<string, object>();
                                metadata.Add(vid, vm);
                            }

                            string key = entries[1];
                            string stype = (entries.Length > 2) ? entries[2] : null;
                            string svalue = (entries.Length > 3) ? entries[3] : null;
                            if (stype == null) // string
                            {
                                vm[key] = svalue;
                                continue;
                            }
                            if (stype.Length == 0) // empty
                            {
                                vm[key] = null;
                                continue;
                            }

                            object ovalue = null;
                            bool isEmptyArray = stype.EndsWith("[0]");
                            bool isArray = isEmptyArray || stype.EndsWith("[]"); // we've got an array
                            if (isArray)
                            {
                                stype = stype.Substring(0, stype.Length - (isEmptyArray ? 3 : 2)); // element type
                                if (!stype.StartsWith("System.")) stype = "System." + stype;
                                Type etype = DataSet.GetType(stype);
                                if (etype == null)
                                    throw new NotSupportedException("Type " + stype + " is unknown");
                                int n = isEmptyArray ? 0 : (entries.Length - 3);
                                Array array = Array.CreateInstance(etype, n);
                                for (int j = 0; j < n; j++)
                                {
                                    string entry = entries[j + 3];
                                    array.SetValue(TypeUtils.Parse(entry, etype, culture), j);
                                }
                                ovalue = array;
                            }
                            else
                            {
                                if (!stype.StartsWith("System.")) stype = "System." + stype;
                                ovalue = TypeUtils.Parse(svalue, DataSet.GetType(stype), culture);
                            }
                            vm[key] = ovalue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLineIf(TraceCsvDataSet.TraceError, "Cannot parse metadata (" + ex + ")");
                    throw new CsvParsingFailedException("Cannot parse metadata", ex);
                }
            }

            List<Variable> cvars = ColumnsToVariables(columns);
            InitializeVariables(columns, readData, cvars);

            return cvars;
        }

        /// <summary>
        /// Reads the line from the stream taking into account quotes, 
        /// i.e. supports \n within quoted string.		
        /// </summary>
        /// <param name="sr"></param>
        /// <returns></returns>
        /// <exception cref="FormatException" />
        private string CsvReadLine(StreamReader sr)
        {
            string s = sr.ReadLine();
            string sres = null;
            if (String.IsNullOrEmpty(s)) return s;

            bool quoted = false;
            do
            {
                int n = s.Length;

                for (int i = 0; i < n; i++)
                {
                    char c = s[i];
                    if (c == '"')
                    {
                        if (!quoted) // start of quoting found
                        {
                            quoted = true;
                            continue;
                        }
                        // Quoting is on:
                        if (i < n - 1) // last char
                        {
                            if (s[i + 1] == '"') // "" = "
                            {
                                i++;
                                continue;
                            }
                        }
                        quoted = false;
                    }
                }
                if (sres == null) sres = s;
                else sres += '\n' + s;
            } while (quoted && (s = sr.ReadLine()) != null);
            if (quoted) throw new FormatException("There is not end quotes matching start quotes");

            return sres;
        }

        private List<Variable> ContinueWithoutMetadata(List<CsvColumn> columns, List<string[]> readData)
        {
            if (!((CsvUri)uri).AppendMetadataDefined)
            {
                appendMetadata = false; // keeping output file as it is: without metadata.
            }

            List<Variable> vars = ColumnsToVariables(columns);
            InitializeVariables(columns, readData, vars);
            if (inferDims)
            {
                List<Variable> updatedVars = new List<Variable>(vars.Count);

                // Variables with same shape depend on same dimension
                Dictionary<int, string[]> inferredDims = new Dictionary<int, string[]>();
                int i = 0;

                foreach (var var in vars)
                {
                    var dims = ((ICsvVariable)var).GetRecentDimensions();
                    for (int k = 0; k < dims.Count; k++)
                    {
                        bool contains = inferredDims.ContainsKey(dims[k].Length);
                        if (!contains || inferredDims[dims[k].Length][k] == null)
                        {
                            if (!contains)
                                inferredDims[dims[k].Length] = new string[2];
                            inferredDims[dims[k].Length][k] = (k == 0) ? ("csv_" + i) : ("csv_" + i + "_" + k);
                            i++;
                        }
                    }
                }
                foreach (var var in vars)
                {
                    string[] upddims = new string[var.Rank];
                    var dims = ((ICsvVariable)var).GetRecentDimensions();
                    for (int k = 0; k < dims.Count; k++)
                        upddims[k] = inferredDims[dims[k].Length][k];
                    updatedVars.Add(((ICsvVariable)var).CloneAndRenameDims(upddims));
                }
                vars = updatedVars;
            }
            return vars;
        }

        /// <summary>
        /// Puts data from read columns into variables
        /// </summary>
        private void InitializeVariables(List<CsvColumn> columns, List<string[]> readData, List<Variable> vars)
        {
            /* Putting read data to variables using additionally read metadata */
            for (int i = 0; i < vars.Count; i++)
            {
                Variable var = vars[i];

                int iStart = columns.FindIndex(c => c.ID == var.ID);
                int iStop = iStart + 1;
                for (; iStop < columns.Count; iStop++)
                {
                    if (columns[iStop].FlatVarBaseIndex != iStart) break;
                }
                iStop--;

                if (iStop == iStart) // One column variable					
                {
                    InitializeVariable(columns[iStart], readData, vars[i]);
                }
                else // flat variable
                {
                    InitializeVariable(columns, iStart, iStop, readData, vars[i]);
                }
            }
        }

        /// <summary>
        /// Puts data into the variable from "flat" csv columns
        /// </summary>
        private void InitializeVariable(List<CsvColumn> columns, int iStart, int iStop, List<string[]> readData, Variable var)
        {
            CsvColumn column = columns[iStart];
            Type type = column.Type;
            int columnIndex = column.Index;

            var csv = (ICsvVariableMd)var;
            Array data = GetDataForColumn(readData, column, type, iStart);
            Array totalData;
            if (column.Rank == 2)
                totalData = Array.CreateInstance(type, data.Length, iStop - iStart + 1);
            else
                totalData = Array.CreateInstance(type, column.Shape);

            for (int i = iStart; i <= iStop; i++)
            {
                column = columns[i];
                if (i > iStart)
                    data = GetDataForColumn(readData, column, type, i);
                csv.FastCopyColumn(totalData, data, i - iStart);
            }
            csv.Initialize(totalData);
        }

        /// <summary>
        /// Puts data into the variable from csv column
        /// </summary>
        private void InitializeVariable(CsvColumn column, List<string[]> readData, Variable var)
        {
            Type type = column.Type;
            int columnIndex = column.Index;

            Array data = GetDataForColumn(readData, column, type, columnIndex);

            // Shape detection
            if (var.Rank == 0)
            {
                if (column.Shape != null && column.Shape[0] != -1 && column.Shape[0] > 1)
                    throw new Exception("Actual size of array " + column.ColumnName + " is incorrect for scalar variable");
            }
            else if (var.Rank == 1)
            {
                if (column.Shape != null && column.Shape[0] != -1 && column.Shape[0] != data.Length)
                    throw new Exception("Actual size of array " + column.ColumnName + " differs from specified in the file's metadata");
            }
            else if (var.Rank == 2)
            {
                if (data.Length > 0)
                {
                    if (column.Shape == null || column.Shape[0] == -1 && column.Shape[1] == -1)
                        throw new Exception("For 2d columned variable there must be defined at least one dimenion's length");
                    if (column.Shape[0] == -1)
                        column.Shape[0] = checked(data.Length / column.Shape[1]);
                    else if (column.Shape[1] == -1)
                        column.Shape[1] = checked(data.Length / column.Shape[0]);
                    if (column.Shape[0] * column.Shape[1] != data.Length)
                        throw new Exception("Actual size of columned 2d-array " + column.ColumnName + " differs from specified in the file's metadata");
                }
                else
                {
                    if (column.Shape == null) // not specified in the metadata
                    {
                        column.Shape = new int[2]; // zeros
                    }
                    else
                    {
                        if (column.Shape[0] > 0 && column.Shape[1] > 0)
                            throw new Exception("Incorrect shape specified at the metadata table");
                    }
                }
            }
            else if (var.Rank > 2)
            {
                if (data.Length > 0)
                {
                    if (column.Shape == null)
                        throw new Exception("For Md columned variable there must be all dimenions' lengths");
                    int n = 1;
                    for (int i = 0; i < column.Shape.Length; i++)
                    {
                        if (column.Shape[i] < 0)
                            throw new Exception("For Md columned variable there must be all dimenions' lengths");
                        n *= column.Shape[i];
                    }

                    if (n != data.Length)
                        throw new Exception("Actual size of columned Md-array " + column.ColumnName + " differs from specified in the file's metadata");
                }
                else
                {
                    if (column.Shape == null) // not specified in the metadata
                    {
                        column.Shape = new int[var.Rank]; // zeros
                    }
                    else
                    {
                        bool allNZero = true;
                        for (int i = 0; i < column.Shape.Length; i++)
                        {
                            if (column.Shape[i] == 0)
                            {
                                allNZero = false;
                                break;
                            }
                        }
                        if (allNZero)
                            throw new Exception("Incorrect shape specified at the metadata table");
                    }
                }
            }
            else throw new Exception("Rank must be non negative");

            ((ICsvVariable)var).Initialize(data, column.Shape);
        }

        private Array GetDataForColumn(List<string[]> readData, CsvColumn column, Type type, int columnIndex)
        {
            Array data = Array.CreateInstance(type, readData.Count);
            object missingValue = column.MissingValue;
            int fixedLength = -1;
            if (!fillUpMissingValues && type == typeof(string))
            {
                // in this case, strings won't contain nulls, just empty strings
                missingValue = ""; // this is local variable, just to convert null to ""							
            }
            if (fillUpMissingValues && column.Shape != null && column.Shape.Length > 0)
                fixedLength = column.Shape[0];

            // Type update during inferring or from metadata entry: needs a conversion
            if (column.TypeUpdated || column.EndsWithEmptyLines)
            {
                bool cut = missingValue == null && type.IsValueType;
                bool? dontConvert = null;
                int rowIndex;
                for (rowIndex = 0; rowIndex < readData.Count; rowIndex++)
                {
                    var a = readData[rowIndex];
                    string o = a.Length > columnIndex ? a[columnIndex] : null;

                    // The empty cell is found
                    if (o == null)
                    {
                        // Looking forward whether the data continues or this is the end
                        bool continues = false;
                        int forwardIndex;
                        for (forwardIndex = rowIndex + 1; forwardIndex < readData.Count; forwardIndex++)
                        {
                            a = readData[forwardIndex];
                            if (a.Length > columnIndex && a[columnIndex] != null)
                            {
                                continues = true;
                                break;
                            }
                        }

                        // If continues, we have to fill the gap with missing values
                        if (continues)
                        {
                            // If cut then for this type there cannot be gaps so this must be the end of data lines
                            if (cut)
                                throw new Exception("Variables of value types without defined missing value cannot have gaps.");

                            // Filling the gap with missing values
                            for (; rowIndex < forwardIndex; rowIndex++)
                                data.SetValue(missingValue, rowIndex);
                            rowIndex--;
                            continue;
                        }
                        else // that's all for this column - no more data
                        {
                            Array dataCut;
                            if (fixedLength >= 0 && rowIndex < fixedLength) // if fillUpMissingValues = true
                            {
                                if (fixedLength > readData.Count)
                                    throw new Exception("Expected data not presented");
                                dataCut = Array.CreateInstance(type, fixedLength);
                                for (int l = rowIndex; l < fixedLength; l++)
                                    dataCut.SetValue(missingValue, l);
                            }
                            else
                                dataCut = Array.CreateInstance(type, rowIndex);
                            Array.Copy(data, dataCut, rowIndex);
                            data = dataCut;
                            break;
                        }
                    }
                    else
                    {
                        if (dontConvert == null)
                            dontConvert = type == typeof(String);
                        if (dontConvert.Value)
                            data.SetValue(o, rowIndex);
                        else
                        {
                            data.SetValue(TypeUtils.Parse(o, type, missingValue, culture), rowIndex);
                        }
                    }
                }
            }
            else // Type wasn't updated, just copying into an array
            {
                for (int j = 0; j < readData.Count; j++)
                {
                    var a = readData[j];
                    string v = a.Length > columnIndex ? a[columnIndex] : null;
                    if (v == null)
                        data.SetValue(missingValue, j);
                    else
                        data.SetValue(v, j);
                }
            }
            if (column.Rank == 0 && data.Length > 1)
                throw new FormatException("Scalar variable has more than single value in the file");
            return data;
        }

        private void ParseMetadata(string[] items, List<CsvColumn> columns)
        {
            items = TrimEnd(items);

            int id = -1;

            if (items.Length > 0) // id
            {
                id = int.Parse(items[0]);
                if (id < 0)
                    throw new Exception("Wrong variable ID in CSV file");
                if (id == DataSet.GlobalMetadataVariableID)
                    throw new Exception("Wrong variable ID: global metadata variable with ID " + id +
                        " must not be specified in the metadata table");
            }

            if (items.Length > 1) // column
            {
                // It is just for user information so we don't need to parse it

                //string item = items[1];
                //int colIndex = columns.FindIndex(c => c.ColumnName == item);
                //if (index < 0)
                //    index = GetIndexByName(item);
            }

            string name = null;
            if (items.Length > 2) // name
            {
                name = Unquote(items[2]);
            }

            // Let's take a look at the rank entry 
            int rank = 1;
            if (items.Length > 4) // rank
            {
                rank = int.Parse(items[4]);
                if (rank < 0)
                    throw new NotSupportedException("Not supported rank: " + rank);
            }
            // Dimensions names
            string[] dims = null;
            int[] shape = null;
            if (items.Length > 6) // dims
            {
                string[] sdims = CsvSplit(TrimEnd(items[6]), innerSeparator);
                if (sdims.Length != rank)
                    throw new Exception("Metadata is incorrect: number of specified dimensions is different than the rank");

                shape = new int[rank];
                for (int i = 0; i < sdims.Length; i++)
                {
                    int size = -1;
                    string dim = sdims[i];
                    int k = dim.IndexOf(':');
                    if (k >= 0)
                    {
                        size = int.Parse(dim.Substring(k + 1, dim.Length - k - 1));
                        sdims[i] = dim.Substring(0, k);
                    }
                    shape[i] = size;
                }
                dims = sdims;
            }

            // Creating or finding a column with specified rank (if not specified, using 1 by default)
            int index = columns.FindIndex(c => c.ID == id);
            CsvColumn col;
            if (index < 0)
            {
                index = (items.Length > 1) ? GetIndexByNameEx(items[1]) : columns.Count;
                if (index < columns.Count)
                    col = columns[index];
                else
                {
                    col = CreateColumn(index, id, rank, "");
                    columns.Add(col);
                }
            }
            else col = columns[index];

            col.ID = id;
            col.Rank = rank;
            if (dims == null)
                col.Dims = InferDimensions(index, rank);
            else
                col.Dims = dims;
            col.Shape = shape;
            col.DescribedInMetadata = true;

            // Variable Name
            if (name != null)
                col.VariableName = name;
            else if (!String.IsNullOrEmpty(col.Header))
                col.VariableName = col.Header;

            // Display Name
            if (String.IsNullOrEmpty(col.Header) && name != null)
                col.Header = col.VariableName;

            // If there is a metadata entry for this column, it cannot be 
            // a part of flat variable
            if (col.FlatVarBaseIndex >= 0 && col.FlatVarBaseIndex != index)
            {
                col.ID = id;
                col.FlatVarBaseIndex = index;
                for (int i = index + 1; i < columns.Count; i++)
                {
                    if (columns[i].FlatVarBaseIndex < index)
                        columns[i].FlatVarBaseIndex = index;
                    else
                        break;
                }
            }

            // Reading metadata fields
            if (items.Length > 3) //        Type
            {
                string stype = items[3];
                if (!String.IsNullOrEmpty(stype))
                {
                    if (!stype.StartsWith("System."))
                        stype = "System." + stype;
                    Type nt = DataSet.GetType(stype);
                    if (nt == null || !DataSet.IsSupported(nt))
                        throw new CsvParsingFailedException("Unexpected type of a variable in the metadata");

                    if (col.Type != nt)
                    {
                        col.Type = nt;
                        col.TypeUpdated = true;
                    }
                    col.InferredType = InferredType.ItIsKnown;
                }
            }
            if (items.Length > 5) // missing value
            {
                object o = TypeUtils.Parse(Unquote(items[5]), col.Type, culture);
                col.MissingValue = o;
            }
        }

        private int GetIndexByNameEx(string colname)
        {
            int i = colname.IndexOf(':');
            if (i >= 0) colname = colname.Substring(0, i);
            return GetIndexByName(colname);
        }

        private static string Unquote(string str)
        {
            if (String.IsNullOrEmpty(str) || str.Length == 1) return str;

            if (str[0] == '"')
            {
                if (str[str.Length - 1] != '"')
                    throw new FormatException("String has only starting quotes");
                str = str.Substring(1, str.Length - 2);
            }

            return str.Replace("\"\"", "\"");
        }

        private string[] ParseDataLine(List<CsvColumn> columns, string[] splitLine, bool enableFlat)
        {
            string[] items = splitLine;
            int itemsLength = items.Length;
            for (; --itemsLength >= columns.Count; )
            {
                if (splitLine[itemsLength] != null)
                    break;
            }
            itemsLength++;

            if (itemsLength > columns.Count)
            {
                int flatBase = -1;
                if (columns.Count > 0)
                {
                    if (enableFlat)
                    {
                        flatBase = columns[columns.Count - 1].FlatVarBaseIndex;
                        if (flatBase == -1)
                            flatBase = columns.Count - 1;
                        columns[flatBase].Rank = 2; // at least
                    }
                }
                for (int i = columns.Count; i < itemsLength; i++)
                {
                    CsvColumn col = CreateColumn(i, -1, 1, ""); // for now it is 1d column
                    col.FlatVarBaseIndex = flatBase;
                    if (flatBase >= 0) col.Rank = 2;
                    columns.Add(col);
                }
            }

            string[] lineData = new string[columns.Count];

            // Reading values from columns of the line
            for (int j = 0; j < lineData.Length; j++)
            {
                CsvColumn c = columns[j];
                if (j < items.Length)
                {
                    string s = items[j];
                    lineData[j] = s;
                    c.EndsWithEmptyLines = s == null; // at least the last cell is an empty

                    // Auto Type Inferring Mechanism goes here!
                    if (!c.EndsWithEmptyLines)
                        switch (c.InferredType)
                        {
                            case InferredType.Unknown:
                                if (enableIntInTypeInfer && TypeUtils.IsInt(s, culture))
                                {
                                    c.InferredType = InferredType.Integer;
                                    break;
                                }
                                if (TypeUtils.IsDouble(s, culture))
                                {
                                    c.InferredType = InferredType.Double;
                                    break;
                                }
                                if (TypeUtils.IsDateTime(s, culture))
                                {
                                    c.InferredType = InferredType.DateTime;
                                    break;
                                }
                                if (TypeUtils.IsBool(s))
                                {
                                    c.InferredType = InferredType.Boolean;
                                    break;
                                }
                                c.InferredType = InferredType.String;
                                break;

                            case InferredType.DateTime:
                                if (!TypeUtils.IsDateTime(s, culture))
                                {
                                    c.InferredType = InferredType.String;
                                }
                                break;

                            case InferredType.Boolean:
                                if (!TypeUtils.IsBool(s))
                                {
                                    c.InferredType = InferredType.String;
                                }
                                break;
                            case InferredType.Integer:
                                if (TypeUtils.IsInt(s, culture))
                                {
                                    break;
                                }
                                if (TypeUtils.IsDouble(s, culture))
                                {
                                    c.InferredType = InferredType.Double;
                                    break;
                                }
                                c.InferredType = InferredType.String;
                                break;

                            case InferredType.Double:
                                if (!TypeUtils.IsDouble(s, culture))
                                {
                                    c.InferredType = InferredType.String;
                                }
                                break;
                        }

                    if (!c.ContainsData)
                        c.ContainsData = s != null;
                }
                else
                {
                    lineData[j] = null;
                    c.EndsWithEmptyLines = true;
                }
            }
            return lineData;
        }

        private CsvColumn CreateColumn(int index, int id, int rank, string header)
        {
            CsvColumn col = new CsvColumn(index, id, header);
            col.Rank = rank;
            col.Dims = InferDimensions(index, rank);
            col.InferredType = InferredType.Unknown;
            col.Type = typeof(string);
            return col;
        }

        /// <summary>
        /// Converts CsvColumns parsed from a file to CsvVariable#d.
        /// </summary>
        private List<Variable> ColumnsToVariables(List<CsvColumn> columns)
        {
            List<Variable> vars = new List<Variable>(columns.Count);
            int flatLast = 0;
            Type commonType;
            Variable v;
            for (int i = 0; i < columns.Count; i++)
            {
                CsvColumn col = columns[i];
                if (!col.ContainsData && !col.DescribedInMetadata)
                    continue;

                // Uncomment this if auto-remove empty variable column:
                // if (!col.ContainsData && String.IsNullOrEmpty(col.Header)) continue;

                // Auto Type Inferring Mechanism continues...
                FinalInferType(col);
                commonType = col.Type;

                // Finding all columns those belong to the flat variable (if there are)
                flatLast = i;
                for (int j = i + 1; j < columns.Count; j++)
                {
                    if (columns[j].FlatVarBaseIndex == i)
                    {
                        flatLast = j;
                        if (col.InferredType != InferredType.ItIsKnown) // if the type isn't specified in the metadata
                        {
                            FinalInferType(columns[j]);
                            commonType = GetCommonType(commonType, columns[j].Type);
                        }
                    }
                    else break;
                }


                // Inferring MissingValue
                if (fillUpMissingValues && col.MissingValue == null && commonType.IsValueType)
                    col.MissingValue = TypeUtils.GetDefaultMissingValue(commonType);

                // Updating types of flat columns
                if (flatLast > i)
                {
                    for (int j = i; j <= flatLast; j++)
                    {
                        CsvColumn c = columns[j];
                        if (c.Type != commonType)
                        {
                            c.Type = commonType;
                            c.TypeUpdated = true;
                        }
                        c.MissingValue = col.MissingValue;
                    }
                    v = ColumnsToVariable(columns, i, flatLast);
                }
                else
                    v = ColumnToVariable(col);
                ProposeNextVarID(v.ID + 1); // the id of this variable may be taken from the input file
                vars.Add(v);
                i = flatLast;
            }

            return vars;
        }

        private Type GetCommonType(Type t1, Type t2)
        {
            if (t1 == t2) return t1;
            if (t1 == typeof(double))
            {
                if (t2 == typeof(int))
                    return t1;
                if (t2 == typeof(string))
                    return t2;
                return typeof(string);
                //throw new Exception("Flat variable has columns of incompatible types: " + t1 + " and " + t2);
            }
            if (t1 == typeof(string))
            {
                return t1;
            }
            if (t1 == typeof(DateTime))
            {
                if (t2 == typeof(string)) return t2;
                return typeof(string);
                //throw new Exception("Flat variable has columns of incompatible types: " + t1 + " and " + t2);
            }
            if (t1 == typeof(bool))
            {
                if (t2 == typeof(string)) return t2;
                return typeof(string);
                //throw new Exception("Flat variable has columns of incompatible types: " + t1 + " and " + t2);
            }
            if (t1 == typeof(int))
            {
                if (t2 == typeof(DateTime))
                    return typeof(string);
                //throw new Exception("Flat variable has columns of incompatible types: " + t1 + " and " + t2);
                return t2;
            }
            return typeof(string);
            //throw new Exception("Unexpected type");
        }

        private void FinalInferType(CsvColumn col)
        {
            if (col.InferredType != InferredType.ItIsKnown && col.InferredType != InferredType.Unknown)
            {
                Type colType = col.Type;
                switch (col.InferredType)
                {
                    case InferredType.DateTime:
                        colType = typeof(DateTime);
                        break;
                    case InferredType.Integer:
                        colType = typeof(int);
                        break;
                    case InferredType.Double:
                        colType = typeof(double);
                        break;
                    case InferredType.String:
                        colType = typeof(string);
                        break;
                    case InferredType.Boolean:
                        colType = typeof(bool);
                        break;
                }
                if (col.Type != colType)
                {
                    if (col.Type != typeof(string))
                        throw new ApplicationException("Unexpected case: most common type is not a string");

                    col.Type = colType;
                    col.TypeUpdated = true;
                }
            }
        }

        /// <summary>
        /// Converts CsvColumn to CsvVariable#d.
        /// </summary>
        private Variable ColumnToVariable(CsvColumn col)
        {
            Variable v = null;
            if (col.Rank == 0)
                v = (Variable)typeof(CsvVariableScalar<>).MakeGenericType(col.Type).
                    GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(CsvDataSet), typeof(CsvColumn) }, null).
                    Invoke(new object[] { this, col });
            else if (col.Rank == 1)
                v = (Variable)typeof(CsvVariable1d<>).MakeGenericType(col.Type).
                    GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(CsvDataSet), typeof(CsvColumn) }, null).
                    Invoke(new object[] { this, col });
            else if (col.Rank == 2)
                v = (Variable)typeof(CsvVariable2d<>).MakeGenericType(col.Type).
                    GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(CsvDataSet), typeof(CsvColumn) }, null).
                    Invoke(new object[] { this, col });
            else if (col.Rank > 2)
                v = (Variable)typeof(CsvVariableMd<>).MakeGenericType(col.Type).
                    GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(CsvDataSet), typeof(CsvColumn) }, null).
                    Invoke(new object[] { this, col });
            else throw new NotSupportedException("Given rank " + col.Rank + " not supported.");

            col.ID = v.ID;
            return v;
        }

        /// <summary>
        /// Converts a number of "flat" CsvColumns to CsvVariable2d.
        /// </summary>
        private Variable ColumnsToVariable(List<CsvColumn> columns, int flatFirst, int flatLast)
        {
            CsvColumn col = columns[flatFirst];
            if (col.Dims.Length != col.Rank)
            {
                col.Dims = InferDimensions(col.Index, col.Rank);
            }
            Variable v;
            if (col.Rank == 2)
                v = (Variable)typeof(CsvVariable2d<>).MakeGenericType(col.Type).
                    GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(CsvDataSet), typeof(CsvColumn) }, null).
                    Invoke(new object[] { this, col });
            else
                v = (Variable)typeof(CsvVariableMd<>).MakeGenericType(col.Type).
                    GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(CsvDataSet), typeof(CsvColumn) }, null).
                    Invoke(new object[] { this, col });

            col.ID = v.ID;
            return v;
        }

        #endregion

        #region Static utils

        /// <summary>
        /// Provides standard names for dimensions, where columnIndex is an index of the column.
        /// </summary>
        private static string[] InferDimensions(int columnIndex, int rank)
        {
            string[] dims = new string[rank];
            if (rank != 1)
                for (int i = 0; i < rank; i++)
                    dims[i] = String.Format("csv_{0}_{1}", columnIndex, i);
            else
                dims[0] = String.Format("csv_{0}", columnIndex);

            return dims;
        }

        /// <summary>
        /// Line is empty if it has only spaces and separators.
        /// </summary>
        private bool IsEmptyLine(string line)
        {
            if (line == null) return true;

            for (int i = 0; i < line.Length; i++)
            {
                if (!(char.IsWhiteSpace(line[i]) || line[i] == separator))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Line is empty if it has only spaces and separators.
        /// </summary>
        private bool IsEmptyLine(string[] splitLine)
        {
            int n = splitLine.Length;
            if (splitLine == null || n == 0) return true;

            for (int i = 0; i < n; i++)
            {
                string item = splitLine[i];
                if (!String.IsNullOrEmpty(item)) return false;
            }
            return true;
        }

        /// <summary>
        /// Removes empty cells from the end of the string.
        /// </summary>
        private string TrimEnd(string str)
        {
            int i;
            for (i = str.Length; --i >= 0; )
            {
                if (!(char.IsWhiteSpace(str[i]) || str[i] == separator))
                    break;
            }
            if (i < 0) return "";

            return str.Substring(0, i + 1);
        }

        /// <summary>
        /// Removes empty cells from the end of the line.
        /// </summary>
        private string[] TrimEnd(string[] items)
        {
            if (items.Length == 0) return items;
            int i;
            for (i = items.Length; --i >= 0; )
            {
                if (!String.IsNullOrEmpty(items[i])) break;
            }
            if (i == items.Length - 1) return items;
            string[] items2 = new string[i + 1];
            for (; i >= 0; i--)
                items2[i] = items[i];
            return items2;
        }

        /// <summary>
        /// Gets an index (starting with zero) of the column by its name, e.g. "AA" -> 26.
        /// Case insensitive.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetIndexByName(string name)
        {
            int ibase = 0;
            int b = letters.Length;
            int bj = 1;

            for (int j = 1; j < name.Length; j++)
            {
                bj *= b;
                ibase += bj;
            }

            int index = 0;
            for (int i = 0; i < name.Length; i++)
            {
                index = index * b + Eval(name[i]) - 1;
            }

            return index + ibase;
        }

        private static int Pow(int a, int p)
        {
            int res = 1;
            for (int i = 1; i < p; i++)
                res *= a;
            return res;
        }

        /// <summary>
        /// Gets the name of the column by its index (starting with zero), e.g. 28 -> "AC". 
        /// </summary>
        public static string GetNameByIndex(int index)
        {
            int k = letters.Length;
            int n = 1;
            int ibase = 0;

            int kj = 1;
            for (int j = 1; ; j++)
            {
                kj *= k;
                if (index - ibase < kj)
                {
                    n = j;
                    break;
                }

                ibase += kj;
            }

            index -= ibase;

            StringBuilder res = new StringBuilder(n);
            do
            {
                res.Insert(0, letters[(index % k)]);
                index = index / k;
            }
            while (index != 0);

            // Plus leading "zeros" ('A')
            while (res.Length < n)
            {
                res.Insert(0, letters[0]);
            }

            return res.ToString();
        }

        private static char[] letters = { 
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 
                'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'};

        private static int Eval(char a)
        {
            switch (Char.ToUpper(a))
            {
                case 'A':
                    return 1;
                case 'B':
                    return 2;
                case 'C':
                    return 3;
                case 'D':
                    return 4;
                case 'E':
                    return 5;
                case 'F':
                    return 6;
                case 'G':
                    return 7;
                case 'H':
                    return 8;
                case 'I':
                    return 9;
                case 'J':
                    return 10;
                case 'K':
                    return 11;
                case 'L':
                    return 12;
                case 'M':
                    return 13;
                case 'N':
                    return 14;
                case 'O':
                    return 15;
                case 'P':
                    return 16;
                case 'Q':
                    return 17;
                case 'R':
                    return 18;
                case 'S':
                    return 19;
                case 'T':
                    return 20;
                case 'U':
                    return 21;
                case 'V':
                    return 22;
                case 'W':
                    return 23;
                case 'X':
                    return 24;
                case 'Y':
                    return 25;
                case 'Z':
                    return 26;
                default:
                    throw new Exception("Wrong name of the column, it might contain only letters of english alphabet.");
            }
        }

        #endregion

        #endregion
    }
}

