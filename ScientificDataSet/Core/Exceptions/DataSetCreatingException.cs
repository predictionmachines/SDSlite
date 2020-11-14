// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data
{
    /// <summary>
    /// The exception that is thrown when the <see cref="Microsoft.Research.Science.Data.Factory.DataSetFactory"/> fails to create a DataSet.
    /// </summary>
    [Serializable]
    public class DataSetCreateException : DataSetException
    {
        private string uri = "";

        private static string FormatMessage(string uri, string outerMessage)
        {
            if (String.IsNullOrEmpty(uri))
                if (String.IsNullOrEmpty(outerMessage))
                    return "Failed to create DataSet instance";
                else
                    return String.Format("Failed to create DataSet instance: {0}", outerMessage);
            if (String.IsNullOrEmpty(outerMessage))
                return String.Format("Failed to create DataSet instance from uri {0}", uri);
            else
                return String.Format("Failed to create DataSet instance from uri {0}: {1}", uri, outerMessage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri">Uri that caused the exception.</param>
        public DataSetCreateException(string uri) : base(FormatMessage(uri, null)) { this.uri = uri; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri">Uri that caused the exception.</param>
        /// <param name="message"></param>
        public DataSetCreateException(string uri, string message) : base(FormatMessage(uri, message)) { this.uri = uri; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri">Uri that caused the exception.</param>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public DataSetCreateException(string uri, string message, Exception inner) : base(FormatMessage(uri, message), inner) { this.uri = uri; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected DataSetCreateException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

        /// <summary>
        /// Gets the costruction URI that caused the exception.
        /// </summary>
        public string FailedUri
        {
            get { return uri; }
        }
    }

    /// <summary>
    /// The exception that is thrown when the <see cref="Microsoft.Research.Science.Data.Factory.DataSetFactory"/> fails to create a DataSet 
    /// because it is not registered.
    /// </summary>
    [Serializable]
    public class ProviderNotRegisteredException : DataSetCreateException
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri">Uri that caused the exception.</param>
        public ProviderNotRegisteredException(string uri) : base(uri) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri">Uri that caused the exception.</param>
        /// <param name="message"></param>
        public ProviderNotRegisteredException(string uri, string message) : base(uri, message) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri">Uri that caused the exception.</param>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public ProviderNotRegisteredException(string uri, string message, Exception inner) : base(uri, message, inner) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ProviderNotRegisteredException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}

