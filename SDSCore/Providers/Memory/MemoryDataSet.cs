// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Research.Science.Data.Memory
{
	/// <summary>
    /// A <see cref="DataSet"/> provider that keeps all data in memory.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Supports variables of any non-negative rank.
	/// </para>
	/// <para>
	/// <see cref="MemoryDataSet"/> accepts following parameters in the URI
	/// (read about DataSet URIs here: <see cref="DataSet.Open(string)"/>;
    /// about preliminary URI customization <see cref="DataSetUri.Create(string)"/>):
	/// <list type="table">
	/// <listheader>
	///   <term>Parameter</term>
	///   <description>Description</description>
	/// </listheader>
	/// <item>
	///		<term>name=DataSetName</term>
	///		<description>Name of the <see cref="DataSet"/>.</description>
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
	/// <example>
    /// Creates the <see cref="MemoryDataSet"/> and adds a <see cref="Variable"/> depending on dimension "x", initialized with an array:
	/// <code>
	/// using(DataSet ds = DataSet.Open("msds:memory"))
    /// {
	///     Variable&lt;int&gt; var = ds.AddVariable&lt;int&gt;("var", "x");
	///     var.PutData(new int[] { 1, 2, 3 });
	/// }
    /// </code>
	/// </example>
	/// </remarks>
	[DataSetProviderName("memory")]
    [DataSetProviderUriTypeAttribute(typeof(MemoryUri))]
    public class MemoryDataSet : DataSet
    {
        internal const string XmlNamespace = "http://research.microsoft.com/science/dataset";
		
		/// <summary>
		/// Filters trace messages related to the DataSet.
		/// </summary>
		internal static TraceSwitch TraceMemoryDataSet = DataSet.TraceDataSet;

		/// <summary>
		/// Initializes an instance.
		/// </summary>
        public MemoryDataSet() : this(null)
        {
        }
		/// <summary>
		/// Initializes an instance.
		/// </summary>
		/// <param name="uri"></param>
		public MemoryDataSet(string uri) 
		{
			if (DataSetUri.IsDataSetUri(uri))
			{
				this.uri = new MemoryUri(uri);				
			}
			else
			{
				this.uri = MemoryUri.FromName(uri);
			}
			
			bool autocommit = IsAutocommitEnabled;			
			IsAutocommitEnabled = false;
			
			Name = ((MemoryUri)this.uri).Name;
			Commit();

			IsAutocommitEnabled = autocommit;
		}
		/// <summary>
		/// Derived class must use the constructor to instantiate the data set.
		/// </summary>
		protected MemoryDataSet(bool createGlobalMetadataVariable)
			: base(createGlobalMetadataVariable, false)
		{
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
            if (dims.Length == 1)
                return new MemoryVariable1d<DataType>(this, varName, dims);
            return new MemoryVariable<DataType>(this, varName, dims);
        }

        /// <summary>
        /// Gets the recent data from given memory variable.
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        protected static Array GetRecentData(Variable variable)
        {
            if (variable == null)
                throw new ArgumentNullException("variable");
            IInternalMemoryVariable mv = variable as IInternalMemoryVariable;
            if (mv == null)
                throw new ArgumentException("Given variable is not a MemoryVariable", "variable");
            return mv.GetRecentData();
        }
    }

    /// <summary>
    /// Allows to customize a URI with parameters specific for the <see cref="MemoryDataSet"/> provider.
    /// </summary>
    /// <seealso cref="DataSet.Create(DataSetUri)"/>
	public class MemoryUri : DataSetUri
	{
		/// <summary>
		/// Instantiates an instance of the class.
		/// </summary>
		/// <param name="uri">The DataSet uri.</param>
		public MemoryUri(string uri)
            : base(uri, typeof(MemoryDataSet))
		{
		}
        /// <summary>
        /// Instantiates an instance of the class with default parameters.
        /// </summary>
        public MemoryUri()
            : base(typeof(MemoryDataSet))
        {
        }

        /// <summary>
        /// Name of the DataSet.
        /// </summary>
		public string Name 
        { 
            get { return GetParameterValue("name", ""); } 
            set { SetParameterValue("name", value); } 
        }

		internal static MemoryUri FromName(string name)
		{
            string providerName = Microsoft.Research.Science.Data.Factory.DataSetFactory.GetProviderNameByType(typeof(MemoryDataSet)) ??
                   ((DataSetProviderNameAttribute)typeof(MemoryDataSet).GetCustomAttributes(typeof(DataSetProviderNameAttribute), false)[0]).Name;
			if (String.IsNullOrEmpty(name))
				return new MemoryUri(DataSetUri.DataSetUriScheme + ":" + providerName);
			return new MemoryUri(DataSetUri.DataSetUriScheme + ":" + providerName + "?name=" + name);
		}
	}
}

