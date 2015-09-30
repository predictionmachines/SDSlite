using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace  Microsoft.Research.Science.Data.Climate
{
    /// <summary>
    /// Static class with different constants, used in Fetch Climate REST API.
    /// </summary>
    public static class RestApiNamings
    {
        /// <summary>
        /// Binary request type name.
        /// </summary>
        public const string binaryRequestTypeName = "application/x-www-form-urlencoded";

        /// <summary>
        /// Csv request type name.
        /// </summary>
        public const string csvRequestTypeName = "text/csv";

        /// <summary>
        /// Text request type name.
        /// </summary>
        public const string textRequestTypeName = "text/plain";

        /// <summary>
        /// Xml request type name.
        /// </summary>
        public const string xmlRequestTypeName = "application/xml";

        /// <summary>
        /// POST request method name.
        /// </summary>
        public const string postRequestMethodName = "POST";
    }
}
