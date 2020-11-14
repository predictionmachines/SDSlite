// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.ComponentModel;

namespace Microsoft.Research.Science.Data.CSV
{
    /// <summary>
    /// Allows to customize a URI with parameters specific for the <see cref="CsvDataSet"/> provider.
    /// </summary>
    /// <seealso cref="DataSet.Create(DataSetUri)"/>
    public class CsvUri : DataSetUri
    {
        private CultureInfo ci = CultureInfo.InvariantCulture;

        /// <summary>
        /// Instantiates an instance of the class.
        /// </summary>
        /// <param name="uri">The DataSet uri.</param>
        public CsvUri(string uri)
            : base(uri, typeof(CsvDataSet))
        {
            if (GetParameterOccurences("file") > 1)
                throw new ArgumentException("The given uri must define single file path");
        }
        /// <summary>
        /// Instantiates an instance of the class with default parameters.
        /// </summary>
        public CsvUri()
            : base(typeof(CsvDataSet))
        {
        }

        /// <summary>
        /// Gets the open resource mode.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The property gets the <see cref="ResourceOpenMode"/> value representing the flag "openMode"
        /// in the data set URI.
        /// </para>
        /// <para>The flag "openMode" specifies how the data set should open a file, 
        /// data base or whatever resource it uses to store the data.</para>
        /// <para>
        /// Possible values for the flag are (case-sensitive):
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
        ///		<term>create</term>
        ///		<description>Specifies that the data set should create a new resource. If the resource already exists, it will be re-created.</description>
        ///		</item>
        ///		<item>
        ///		<term>openOrCreate</term>
        ///		<description>Specifies that the data set should open a resource if it exists; otherwise, a new resource should be created.</description>
        ///		</item>
        /// </list>
        /// </para>
        /// <example>
        /// <code>
        /// DataSet dataSet = DataSet.Create("msds:csv?file=data.csv&amp;openMode=open"); 
        /// </code>
        /// </example>
        /// </remarks>
        /// <seealso cref="GetOpenModeOrDefault"/>
        [Description("Specifies how the data set should open a file,\ndata base or whatever resource it uses to store the data.")]
        public new ResourceOpenMode OpenMode
        {
            get { return base.OpenMode; }
            set { base.OpenMode = value; }
        }

        /// <summary>
        /// Gets or sets the enconding that must be used to read and/or write the CSV file
        /// (default is "utf-8").
        /// </summary>
        /// <remarks>
        /// Both code page number and name can be used as a string representation of the parameter in URI.
        /// </remarks>
        [Description("Encoding of the CSV file (default is utf-8)")]
        public Encoding Encoding
        {
            get
            {
                string val = GetParameterValue("encoding", null);
                int codePage;
                if (val == null) return null;
                if (int.TryParse(val, out codePage))
                    return Encoding.GetEncoding(codePage);
                else
                    return Encoding.GetEncoding(val);
            }
            set
            {
                if (value == null) RemoveParameter("encoding");
                else SetParameterValue("encoding", value.WebName);
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
        [Description("If the value is true, the provider appends an output file with special metadata tables,\ndescribing structure of the DataSet.\nThese tables can be a reason of incompatibility for some CSV-oriented programs.\nIn this case set the parameter value to false.\nIf the parameter is not specifed, metadata will be appended\nonly if the input file have had metadata appended.")]
        public bool AppendMetadata
        {
            get
            {
                string val = GetParameterValue("appendMetadata", "true").ToLower();
                return bool.Parse(val);
            }
            set
            {
                SetParameterValue("appendMetadata", value.ToString().ToLower());
            }
        }

        /// <summary>
        /// "noHeader=true|false"
        /// </summary>
        /// <remarks>
        /// If the parameter is false, the first line of an input file
        /// is a header containing display names of variables.
        /// Otherwise, first line contains data. 
        /// Note that this parameter doesn't affect an output file. See also
        /// parameter "saveHeader".
        /// Default is false.
        /// </remarks>
        [Description("If the parameter is false, the first line of an input file is\na header containing display names of variables.\nOtherwise, first line contains data.\nNote that this parameter doesn't affect an output file.")]
        public bool NoHeader
        {
            get
            {
                string val = GetParameterValue("noHeader", "false").ToLower();
                return bool.Parse(val);
            }
            set
            {
                SetParameterValue("noHeader", value.ToString().ToLower());
            }
        }

        /// <summary>
        /// "saveHeader=true|false"
        /// </summary>
        /// <remarks>
        /// Parameter defines whether the provider must save a header line into an output file or not.
        /// If it is not specified, its value depends on the "noHeader" parameter:
        /// if input file has no header, the header is not saved in the output file, too; and vice versa.
        /// </remarks>
        [Description("Parameter defines whether the provider must save a header line into an output file or not.\nIf it is not specified, its value depends on the \"noHeader\" parameter:\nif the input file has no header, the header is not saved in the output file, too;\nand vice versa.")]
        public bool SaveHeader
        {
            get
            {
                if (GetParameterOccurences("saveHeader") > 0)
                {
                    string val = GetParameterValue("saveHeader", "true").ToLower();
                    return bool.Parse(val);
                }
                return !NoHeader;
            }
            set
            {
                SetParameterValue("saveHeader", value.ToString().ToLower());
            }
        }

        /// <summary>
        /// Gets the value indicating whether the uri contains appendMetadata parameter or not.
        /// </summary>
        public bool AppendMetadataDefined
        {
            get
            {
                return GetParameterOccurences("appendMetadata") > 0;
            }
        }

        /// <summary>
        /// Gets the value indicating whether the CSV provider should interpreter empty values
        /// in a file as missing values or not. Default is false.
        /// </summary>
        [Description("Indicates whether the CSV provider should interpreter empty values\n(i.e. empty strings between separators) in an input file as missing values or not.")]
        public bool FillUpMissingValues
        {
            get
            {
                string val = GetParameterValue("fillUpMissingValues", "false").ToLower();
                return bool.Parse(val);
            }
            set
            {
                SetParameterValue("fillUpMissingValues", value.ToString().ToLower());
            }
        }

        /// <summary>
        /// If the property is true, CSV provider infers following types on loading:
        /// int, double, DateTime, string, bool. Otherwise, only double, DateTime, string and bool.
        /// </summary>
        [Description("If the value is true, the CSV provider infers following types on loading:\nint, double, DateTime, string, bool.\nOtherwise, it infers double, DateTime, string and bool.")]
        public bool InferInt
        {
            get
            {
                string val = GetParameterValue("inferInt", "false").ToLower();
                return bool.Parse(val);
            }
            set
            {
                SetParameterValue("inferInt", value.ToString().ToLower());
            }
        }

        /// <summary>
        /// Gets the culture that is to be used to parse a file.
        /// </summary>
        /// <remarks>
        /// Corresponds to optional parameter "culture".
        /// Default culture is an invariant culture.
        /// <example>
        /// msds:csv?file=data-gb.csv&amp;culture=en-GB
        /// </example>
        /// </remarks>
        [Description("The culture name that is used to parse an input file.\nDefault is an invariant culture. Note that this parameter does not affect an output file\nfor it is written for an invariant culture always.\nExamples: en-GB, fr-FR, ru-RU, en, fr, ru.")]
        public CultureInfo Culture
        {
            get
            {
                // Culture
                int n = GetParameterOccurences("culture");
                if (n > 1)
                    throw new ArgumentException("There are several parameters \"culture\" in the uri");
                if (n == 1)
                    return new CultureInfo(GetParameterValue("culture"));
                return CultureInfo.InvariantCulture;
            }
            set
            {
                SetParameterValue("culture", value.ToString());
            }
        }

        /// <summary>
        /// If the value is true, when parsing a file, all variables
        /// (that have no metadata defined for them)
        /// with same length are considered as sharing same dimensions.
        /// </summary>
        [Description("If the value is true, when parsing a file, all variables\n(that have no metadata defined for them)\nwith same length are considered as sharing same dimensions.")]
        public bool InferDims
        {
            get
            {
                string val = GetParameterValue("inferDims", "true").ToLower();
                return bool.Parse(val);
            }
            set
            {
                SetParameterValue("inferDims", value.ToString().ToLower());
            }
        }

        /// <summary>
        /// Corresponds to optional parameter "utf8bom=true|false".
        /// Default is true.
        /// </summary>
        [Description("If utf8bom parameter is defined and is equal to true, output encoding is UTF8 with signature;\nfalse, output encoding is UTF8 without signature.\nIf the parameter utf8bom is not defined, and file has been existing,\nits encoding is kept for an output. And if the underlying file is created, UTF8 with signature is used.")]
        public bool UTF8BomEnabled
        {
            get
            {
                string val = GetParameterValue("utf8bom", "true").ToLower();
                return bool.Parse(val);
            }
            set
            {
                SetParameterValue("utf8bom", value.ToString().ToLower());
            }
        }

        /// <summary>
        /// Gets the value indicating whether the uri contains utf8bom parameter or not.
        /// </summary>
        public bool UTF8BomDefined
        {
            get
            {
                return GetParameterOccurences("utf8bom") > 0;
            }
        }

        public bool HasSeparator
        {
            get
            {
                return GetParameterOccurences("separator") > 0;
            }
        }

        [Description("Determines a char that separates values in the file.\nAllowed delimiters are \",\" \";\" \" \" \"\\t\".")]
        public Delimiter Separator
        {
            get
            {
                string sep = this.GetParameterValue("separator", "Comma");

                return (Delimiter)Enum.Parse(typeof(Delimiter), sep, true);
            }
            set
            {
                SetParameterValue("separator", value.ToString().ToLower());
            }
        }

        /// <summary>
        /// Returns filename or null if it's not defined
        /// </summary>
        [FileNameProperty]
        [Description("Specifies the file to open or create.\nThis can be either a local file path or a HTTP URL.\nIf the HTTP URL is specified,\nthe file must exist and the constructed data set is always read only. ")]
        public string FileName
        {
            get
            {
                if (GetParameterOccurences("file") > 0)
                    return this["file"];
                else return null;
            }
            set
            {
                this["file"] = value;
            }
        }

        /// <summary>
        /// Gets the value indicating whether the file parameter is defined or not.
        /// </summary>
        public bool FileNameDefined
        {
            get
            {
                return GetParameterOccurences("file") > 0;
            }
        }

        internal static CsvUri FromFileName(string file)
        {
            string uri = DataSetUri.CreateFromPath(file,
                Microsoft.Research.Science.Data.Factory.DataSetFactory.GetProviderNameByType(typeof(CsvDataSet)) ??
                   ((DataSetProviderNameAttribute)typeof(CsvDataSet).GetCustomAttributes(typeof(DataSetProviderNameAttribute), false)[0]).Name,
                "file");
            return new CsvUri(uri);
        }
    }

    /// <summary>
    /// Contains possible delimiters for CsvDataSet files.
    /// </summary>
    public enum Delimiter
    {
        /// <summary>
        /// Delimiter character is ','.
        /// </summary>
        Comma,
        /// <summary>
        /// Delimiter character is ';'.
        /// </summary>
        Semicolon,
        /// <summary>
        /// Delimiter character is '\t'.
        /// </summary>
        Tab,
        /// <summary>
        /// Delimiter character is ' '.
        /// </summary>
        Space
    }
}

