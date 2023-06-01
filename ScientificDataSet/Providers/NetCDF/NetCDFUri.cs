// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Microsoft.Research.Science.Data.NetCDF4
{
    /// <summary>
    /// Allows to customize a URI with parameters specific for the <see cref="NetCDFDataSet"/> provider.
    /// </summary>
    /// <seealso cref="DataSet.Create(DataSetUri)"/>
    public class NetCDFUri : DataSetUri
    {
        /// <summary>
        /// Instantiates an instance of the class.
        /// </summary>
        /// <param name="uri">The DataSet uri.</param>
        public NetCDFUri(string uri)
            : base(uri, typeof(NetCDFDataSet))
        {
            if (GetParameterOccurences("file") > 1)
                throw new ArgumentException("The given uri must define single file path");
        }
        /// <summary>
        /// Instantiates an instance of the class with default parameters.
        /// </summary>
        public NetCDFUri()
            : base(typeof(NetCDFDataSet))
        {
        }

        /// <summary>
        /// Indicates whether the provider should trim trailing zero (\x00 symbol) when reads attribute values or not.
        /// Default is false.
        /// </summary>
        [Description("Indicates whether the provider must trim trailing zero (\\x00 symbol) when reads attribute values or not.\nDefault is false.")]
        public bool TrimTrailingZero
        {
            get
            {
                if (GetParameterOccurences("trimZero") == 0)
                    return false;
                string trim = this["trimZero"];

                return bool.Parse(trim);
            }
            set
            {
                this["trimZero"] = value.ToString().ToLower();
            }
        }

        /// <summary>
        /// Specifies how the data set should open a file.
        /// </summary>
        [Description("Specifies how the data set should open a file.")]
        public new ResourceOpenMode OpenMode
        {
            get { return base.OpenMode; }
            set { base.OpenMode = value; }
        }

        /// <summary>
        /// Defines the compression level.
        /// </summary>
        [Description("Defines the data compression level. Affects only DataSet modification.")]
        public DeflateLevel Deflate
        {
            get
            {
                if (GetParameterOccurences("deflate") == 0)
                    return DeflateLevel.Normal;
                string defl = this["deflate"];

                return (DeflateLevel)Enum.Parse(typeof(DeflateLevel), defl, true);
            }
            set
            {
                this["deflate"] = value.ToString().ToLower();
            }
        }

        /// <summary>
        /// Specifies the file to open or create.
        /// </summary>
        [FileNameProperty]
        [Description("Specifies the file to open or create.")]
        public string FileName
        {
            get
            {
                if (GetParameterOccurences("file") == 0)
                    return "";
                return this["file"];
            }
            set { this["file"] = value; }
        }

        /// <summary>
        /// Indicates whether the rollback is enabled or not.
        /// </summary>
        /// <remarks>
        /// Indicates whether the rollback is enabled or not. Rollback can happen if commit fails. If it is enabled, each transaction copies source file for further possible restoration. Otherwise, rollback will throw an exception in case of its invoke. Set it for data sets having not only NetCDF variables and consider set it false for very large files. Default is false.
        /// </remarks>
        [Description("Indicates whether the rollback is enabled or not. Rollback can happen if commit fails.\nIf it is enabled, each transaction copies source file for further possible restoration. Otherwise, rollback will throw an exception in case of its invoke.\nSet it for data sets having not only NetCDF variables and consider set it false for very large files.\nDefault is false.")]
        public bool EnableRollback
        {
            get
            {
                string val = GetParameterValue("enableRollback", "false").ToLower();
                return bool.Parse(val);
            }
            set
            {
                SetParameterValue("enableRollback", value.ToString().ToLower());
            }
        }
    }

#pragma warning disable 1591
    /// <summary>
    /// Represents data deflate level for NetCDF variables.
    /// </summary>
    /// <seealso cref="NetCDFDataSet"/>
    public enum DeflateLevel
    {
        Off = -1,
        Store = 0,
        Fastest = 1,
        Fast = 3,
        Normal = 5,
        Good = 7,
        Best = 9,
        BestWithShuffle = 10
    }
}

