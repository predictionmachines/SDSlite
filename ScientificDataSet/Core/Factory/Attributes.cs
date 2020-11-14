// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data
{
	/// <summary>
	/// Specifies the name of the DataSet provider.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The name of a provider is referred in the DataSet URI
	/// (see remarks for the <see cref="Microsoft.Research.Science.Data.DataSetUri"/> class).</para>
	/// <para>
	/// See remarks for the <see cref="Microsoft.Research.Science.Data.Factory.DataSetFactory"/> class.
	/// </para>
	/// </remarks>
	/// <seealso cref="Microsoft.Research.Science.Data.DataSetUri"/>
	/// <seealso cref="DataSetProviderFileExtensionAttribute"/>
	/// <seealso cref="DataSetProviderUriTypeAttribute"/>
	/// <seealso cref="Microsoft.Research.Science.Data.Factory.DataSetFactory"/>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class DataSetProviderNameAttribute : Attribute
	{
		private string name;

		/// <summary>
		/// Initializes the attribute.
		/// </summary>
		/// <param name="name">Name of the provider.</param>
		public DataSetProviderNameAttribute(string name)
		{
			this.name = name;
		}

		/// <summary>
		/// Gets the name of the provider.
		/// </summary>
		public string Name { get { return name; } }
	}

	/// <summary>
	/// Specifies the types of files (extensions) acceptable by the DataSet provider.
	/// </summary>
	/// <remarks> 
	/// See remarks for the <see cref="Microsoft.Research.Science.Data.Factory.DataSetFactory"/> class.
	/// </remarks>
	/// <seealso cref="DataSetProviderNameAttribute"/>
	/// <seealso cref="DataSetProviderUriTypeAttribute"/>
	/// <seealso cref="Microsoft.Research.Science.Data.Factory.DataSetFactory"/>
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
	public sealed class DataSetProviderFileExtensionAttribute : Attribute
	{
		private string ext;

		/// <summary>
		/// Initializes the attribute.
		/// </summary>
		/// <param name="extension">File extension acceptable by the provider (including ".", e.g. ".dat").</param>
		public DataSetProviderFileExtensionAttribute(string extension)
		{
			this.ext = extension;
		}

		/// <summary>
		/// Gets the file extension acceptable by the provider (including ".", e.g. ".dat").
		/// </summary>
		public string FileExtension { get { return ext; } }
	}

	/// <summary>
	/// Associates a class derived from the <see cref="DataSetUri"/> with
	/// a <see cref="DataSet"/> provider type.
	/// </summary>
	/// <seealso cref="DataSetProviderFileExtensionAttribute"/>
	/// <seealso cref="DataSetProviderNameAttribute"/>
	/// <seealso cref="DataSetUri.Create(string)"/>
	/// <seealso cref="DataSet.Open(DataSetUri)"/>
	/// <seealso cref="DataSet"/>
	/// <seealso cref="Microsoft.Research.Science.Data.Factory.DataSetFactory"/>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class DataSetProviderUriTypeAttribute : Attribute
    {
        private Type type;

        /// <summary>
        /// Initializes the attribute.
        /// </summary>
		/// <param name="uriType">Type of the provider.</param>
		/// <remarks>
		/// Type <paramref name="uriType"/> must be <see cref="DataSetUri"/> type or
		/// be derived from the <see cref="DataSetUri"/> class.
		/// </remarks>
        public DataSetProviderUriTypeAttribute(Type uriType)
        {
			if (uriType == null) throw new ArgumentNullException("uriType");
			if (uriType != typeof(DataSetUri) && !uriType.IsSubclassOf(typeof(DataSetUri)))
				throw new ArgumentException("uriType must be DataSetUri or derived from it");

            this.type = uriType;
        }

        /// <summary>
        /// Gets the type of the provider.
        /// </summary>
        public Type UriType { get { return type; } }
    }

    /// <summary>
    /// Indicates that the target property contains file name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple=false)]
    public sealed class FileNamePropertyAttribute : Attribute { }

    /// <summary>
    /// Indicates that the target property contains directory name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class DirectoryPropertyAttribute : Attribute { }

    /// <summary>
    /// Indicates that the target property contains URI
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple=false)]
    public sealed class UriPropertyAttribute : Attribute { }
}

