// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using Microsoft.Research.Science.Data.Factory;

namespace Microsoft.Research.Science.Data
{
    /// <summary>This class holds information about referenced variables in a DataSet uri.</summary>
    public struct VariableReferenceName
    {
        private string name;
        private string alias;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public VariableReferenceName(string name)
        {
            this.name = name;
            this.alias = name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="alias"></param>
        public VariableReferenceName(string name, string alias)
        {
            this.name = name;
            this.alias = alias;
        }

        /// <summary>Gets name of variable in referenced DataSet</summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>Gets alias of referenced variable - name of referenced variable in 
        /// referencing dataset</summary>
        public string Alias
        {
            get { return alias; }
        }
    }

    /// <summary>
    /// The class represents a special kind of URI that is a DataSet identification string.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DataSet URI consists of mandatory URI schema ‘msds’ and SDS <see cref="DataSetUri.ProviderName"/> followed by 
    /// optional provider parameters.
    /// The identification string has following syntax:
    /// </para>
    /// <para>
    /// msds:provider-name?param1=value1&amp;param2=value2&amp;...#include-var1,include-var2
    /// </para>
    /// <para>More formally the URI format is: </para>
    /// <para>DataSetURI ::= msds:provider[?parameters][#include-vars] </para>
    /// <para>provider-name ::= identifier</para>
    /// <para>parameters ::= parameters&amp;parameters|parameter-name=parameter-value</para>
    /// <para>include-vars := variable-name|variable-name,include-vars</para>
    /// <para>parameter-name ::= identifier</para>
    /// <para>parameter-value ::= character-string </para>
    /// <para>Provider-identifier and set of parameter-names depends on a particular data set provider. 
    /// Please note that the parameters string can contain duplicate parameter names. 
    /// </para>
    /// <para>
    /// A list of supported parameters depends on which provider name is specified in a URI. 
    /// When a URI is parsed, all parameters, those are not supported by the particular provider, are ignored.
    /// </para>
    /// <para>
    /// Note that DataSet URI is case-sensitive.
    /// </para>
    /// <para>Examples of DataSet URIs are <c>"msds:csv?file=c:\data\test.csv"</c>,
    /// <c>"msds:as?server=(local)&amp;database=ActiveStorage&amp;integrated security=true&amp;GroupName=mm5&amp;UseNetcdfConventions=true"</c>.
    /// </para>
    /// <para>The <see cref="DataSetUri"/> class makes verification and parsing of DataSet URIs and provides methods
    /// to access parameters of the given URI. It should be used by provider implementators to parse DataSet URI.</para>
    /// <para>
    /// A DataSet provider might have a related special type derived from <see cref="DataSetUri"/> describing
    /// its parameters. This enables customization of a URI through such class before
    /// creating the <see cref="DataSet"/> instance. See <see cref="DataSetUri.Create(string)"/>
    /// and <see cref="DataSet.Open(DataSetUri)"/>.
    /// </para>
    /// <para>Read also about DataSet factoring here <see cref="Microsoft.Research.Science.Data.Factory.DataSetFactory"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="DataSetUri.Create(string)"/>
    /// <seealso cref="DataSet.Open(DataSetUri)"/>
    /// <seealso cref="Microsoft.Research.Science.Data.Factory.DataSetFactory"/>
    [Serializable]
    public class DataSetUri : Uri
    {
        /// <summary>
        /// The scheme name for the DataSet uri: "msds".
        /// </summary>
        public const string DataSetUriScheme = "msds";

        private string provider;
        private VariableReferenceName[] variables;
        private List<KeyValuePair<string, string>> parameters;

        private string cachedToString = null;

        /// <summary>
        /// Initializes the instance of the class.
        /// </summary>
        /// <param name="uri">DataSet uri string.</param>
        /// <exception cref="ArgumentException"><paramref name="uri"/> is not data set uri.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="uri"/> is null.</exception>
        /// <exception cref="UriFormatException"><paramref name="uri"/> is empty or doesn't conform to URI syntax. See <see cref="System.Uri.Uri(System.String)"/> for details.</exception>
        public DataSetUri(string uri)
            : base(uri)
        {
            InitFromString();
        }

        /// <summary>
        /// Initializes the instance of the class.
        /// </summary>
        /// <param name="uri">DataSet uri string.</param>
        /// <param name="providerType">Type of the DataSet provider to check that URI is correct.</param>
        /// <exception cref="ArgumentException"><paramref name="uri"/> is not data set uri.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="uri"/> is null.</exception>
        /// <exception cref="UriFormatException"><paramref name="uri"/> is empty or doesn't conform to URI syntax. See <see cref="System.Uri.Uri(System.String)"/> for details.</exception>
        protected DataSetUri(string uri, Type providerType)
            : this(uri)
        {
            // Check that provider name corresponds to the provider type.
            var regNames = Microsoft.Research.Science.Data.Factory.DataSetFactory.GetProviderNamesByType(providerType);
            if (!regNames.Contains(ProviderName))
            {
                if (ProviderName != GetProviderName(providerType))
                    throw new ArgumentException("The provider name " + ProviderName + " is not registered for the provider type " + providerType);
            }
        }

        /// <summary>
        /// Initializes the instance of the class.
        /// </summary>
        /// <param name="providerType">Type of the DataSet provider to create a default URI for.</param>
        /// <exception cref="ArgumentNullException" />
        protected DataSetUri(Type providerType)
            : this(BuildBaseUriFromProviderType(providerType))
        {
        }

        private static string BuildBaseUriFromProviderType(Type providerType)
        {
            string providerName = Microsoft.Research.Science.Data.Factory.DataSetFactory.GetProviderNameByType(providerType);
            if (providerName == null)
                providerName = GetProviderName(providerType);
            if (String.IsNullOrEmpty(providerName))
                throw new ArgumentException("Unknown provider type");
            return DataSetUriScheme + ":" + providerName;
        }

        /// <summary>
        /// Gets provider name from <see cref="DataSetProviderNameAttribute"/>
        /// of the type. If the attribute is missing, throws an exception.
        /// </summary>
        /// <param name="providerType"></param>
        /// <returns></returns>
        private static string GetProviderName(Type providerType)
        {
            if (providerType == null) throw new ArgumentNullException("providerType");
            object[] objs = providerType.GetCustomAttributes(typeof(DataSetProviderNameAttribute), false);
            if (objs == null || objs.Length == 0)
                throw new NotSupportedException("Cannot create DataSet URI for the provider type that has no provider name associated");
            DataSetProviderNameAttribute attr = (DataSetProviderNameAttribute)objs[0];
            return attr.Name;
        }

        /// <summary>
        /// Asserts whether the provider name is associated with the given provider type.
        /// </summary>
        /// <param name="providerName">Name of the provider to check.</param>
        /// <param name="providerType">Provider type.</param>
        protected static void AssertProviderName(string providerName, Type providerType)
        {
            if (String.IsNullOrEmpty(providerName))
                throw new ArgumentNullException("providerName");
            var names = Microsoft.Research.Science.Data.Factory.DataSetFactory.GetProviderNamesByType(providerType);
            if (!names.Contains(providerName))
                throw new DataSetException("Given provider name is not associated with the provider type");
        }

        /// <summary>Uri reserved characters according to RFC 2396</summary>
        private const string ReservedCharacters = ";/?:@&=+,$";

        private static string GetVariableName(string s, out string name)
        {
            if (String.IsNullOrEmpty(s))
                throw new ArgumentException("Input string is empty", "s");
            if (!MetadataDictionary.IsValidFirstNameSymbol(s[0]))
                throw new ArgumentException("Input string doesn\'t start with variable name");
            int i = 1;
            while (i < s.Length && MetadataDictionary.IsValidNextNameSymbol(s[i]))
                i++;
            name = s.Substring(0, i);
            if (i < s.Length)
                return s.Substring(i);
            else
                return "";
        }

        private void InitFromString()
        {
            if (Scheme != DataSetUriScheme)
                throw new ArgumentException("Not a DataSet uri", "uriString");

            provider = AbsolutePath;
            if (String.IsNullOrEmpty(provider))
                throw new ArgumentException("Provider name cannot be null", "uriString");
            if (String.IsNullOrEmpty(Fragment))
                variables = new VariableReferenceName[0];
            else
                variables = GetVariableReferences(UnescapeDataString(Fragment.Substring(1)));

            // Parsing parameters
            parameters = new List<KeyValuePair<string, string>>();
            string paramString = Query;
            if (!String.IsNullOrEmpty(paramString))
            {
                string[] @params = paramString.Substring(1).Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

                Regex regex = new Regex(@"^([\w ]+)(=(.*))?$");
                foreach (string param in @params)
                {
                    string p = UnescapeDataString(param);
                    string key, value;
                    Match match = regex.Match(p);
                    if (!match.Success)
                        throw new ArgumentException("The given URI is incorrect");
                    key = match.Groups[1].Value;
                    value = match.Groups[2].Success ? match.Groups[3].Value : "";

                    parameters.Add(new KeyValuePair<string, string>(key, value));
                }
            }
        }

        private static VariableReferenceName[] GetVariableReferences(string fragment)
        {
            List<VariableReferenceName> refs = new List<VariableReferenceName>();
            while (fragment.Length > 0)
            {
                string name;
                fragment = GetVariableName(fragment, out name);
                if (fragment.StartsWith("="))
                {
                    string alias = name;
                    fragment = GetVariableName(fragment.Substring(1), out name);
                    refs.Add(new VariableReferenceName(name, alias));
                }
                else
                    refs.Add(new VariableReferenceName(name));
                if (fragment.Length > 0)
                    if (!fragment.StartsWith(","))
                        throw new ArgumentException("Incorrect list of referenced variables");
                    else
                        fragment = fragment.Substring(1);
            }
            return refs.ToArray();
        }

        /// <summary>
        /// Gets the provider name specified in the uri.
        /// </summary>
        public string ProviderName
        {
            get
            {
                return provider;
            }
        }

        /// <summary>
        /// Gets or sets the value of the specified parameter.
        /// </summary>
        /// <param name="parameter">The name of a parameter.</param>
        /// <returns>The value for the specified parameter.</returns>
        /// <remarks>
        /// <para>Parameter name and value are case-sensitive.</para>
        /// <para>If the uri doesn't contain the parameter an exception is thrown.</para>
        /// <para>
        /// If there are several parameters with the same name the method returns first one.
        /// While setting value considers that parameter with this name is unique.
        /// </para>
        /// </remarks>
        /// <exception cref="KeyNotFoundException">The uri doesn't contain the parameter.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="parameter"/> is null.</exception>
        /// <seealso cref="GetParameterValue(string)"/>
        /// <seealso cref="GetParameterValue(string, string)"/>
        /// <seealso cref="SetParameterValue(string, string)"/>
        /// <seealso cref="SetParameterValue(string, string, bool)"/>
        public string this[string parameter]
        {
            get { return GetParameterValue(parameter); }
            set { SetParameterValue(parameter, value); }
        }


        /// <summary>Gets an array of referenced variable names.</summary>
        /// <remarks>
        /// <para>
        /// Variable names can be referenced in a uri appended through the '#' mark:
        /// <c>msds:csv?file=data.csv#a,b,c</c>. In the example, <c>a,b,c</c> are
        /// variables names referenced in the uri.</para>
        /// </remarks>
        public VariableReferenceName[] VariableReferences
        {
            get { return variables; }
        }

        /// <summary>
        /// Gets the value of the specified parameter.
        /// </summary>
        /// <param name="parameter">The name of a parameter.</param>
        /// <returns>The value for the specified parameter.</returns>
        /// <remarks>
        /// <para>Parameter name is case-sensitive.</para>
        /// <para>If the uri doesn't contain the parameter an exception is thrown.</para>
        /// <para>
        /// If there are several parameters with the same name the method returns first one.
        /// </para>
        /// </remarks>
        /// <exception cref="KeyNotFoundException">The uri doesn't contain the parameter.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="parameter"/> is null.</exception>
        /// <seealso cref="this[string]"/>
        /// <seealso cref="GetParameterValue(string, string)"/>
        /// <seealso cref="GetParameterValues(string)"/>
        /// <seealso cref="GetParameterValues(string, string)"/>
        public string GetParameterValue(string parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException("parameter");
            var res = parameters.FindIndex(p => p.Key == parameter);
            if (res < 0)
                throw new KeyNotFoundException("Parameter " + parameter + " not found");
            return parameters[res].Value;
        }

        /// <summary>
        /// Sets the value of the specified parameter.
        /// </summary>
        /// <param name="key">The name of a parameter.</param>
        /// <param name="value">New value of parameter</param>
        /// <param name="multiplyParams">True if this parameter can occur multiple times in this uri.</param>
        /// <returns>The value for the specified parameter.</returns>
        /// <remarks>
        /// <para>Parameter name <paramref name="key"/> and <paramref name="value"/> are case-sensitive.</para>
        /// <para>If the uri doesn't contain the parameter an exception is thrown.</para>
        /// <para>
        /// If there are several parameters with the same name and <paramref name="multiplyParams"/> is false, 
        /// the method replaces first one. Otherwise, new parameter added.
        /// </para>
        /// </remarks>
        /// <exception cref="KeyNotFoundException">The uri doesn't contain the parameter.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <seealso cref="this[string]"/>
        public void SetParameterValue(string key, string value, bool multiplyParams)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            if (value == null)
                throw new ArgumentNullException("value");
            var res = parameters.FindIndex(p => p.Key == key);
            if ((res >= 0) && (!multiplyParams))
                parameters.RemoveAll(p => p.Key == key);
            parameters.Add(new KeyValuePair<string, string>(key, value));
            cachedToString = null;
        }

        /// <summary>
        /// Removes the parameter from the list.
        /// </summary>
        /// <param name="key"></param>
        public void RemoveParameter(string key)
        {
            if (key == null) throw new ArgumentNullException("key");
            var res = parameters.FindIndex(p => p.Key == key);
            if (res >= 0)
                parameters.RemoveAll(p => p.Key == key);
        }

        /// <summary>
        /// Sets the value of the specified parameter, each parameter considers as unique.
        /// </summary>
        /// <param name="key">The name of a parameter.</param>
        /// <param name="value">New value of parameter</param>
        /// <returns>The value for the specified parameter.</returns>
        /// <remarks>
        /// <para>Parameter name <paramref name="key"/> and <paramref name="value"/> are case-sensitive.</para>
        /// <para>If the uri doesn't contain the parameter an exception is thrown.</para>
        /// <para>
        /// If there is existing parameter with the same name, the method replaces first one.
        /// </para>
        /// </remarks>
        /// <exception cref="KeyNotFoundException">The uri doesn't contain the parameter.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <seealso cref="this[string]"/>
        public void SetParameterValue(string key, string value)
        {
            this.SetParameterValue(key, value, false);
        }

        /// <summary>
        /// Gets the value of the specified parameter or specified default value, if 
        /// the uri doesn't contain the parameter.
        /// </summary>
        /// <param name="parameter">The name of a parameter.</param>
        /// <param name="default">Default value that is returned if the uri doesn't contain the parameter.</param>
        /// <remarks>
        /// <para>Parameter name is case-sensitive.</para>
        /// <para>
        /// If there are several parameters with the same name, the method returns first one.
        /// </para>
        /// </remarks>
        /// <returns>The value for the specified parameter or default value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="parameter"/> is null.</exception>
        /// <seealso cref="this[string]"/>
        /// <seealso cref="GetParameterValue(string)"/>
        /// <seealso cref="GetParameterValues(string)"/>
        /// <seealso cref="GetParameterValues(string, string)"/>
        public string GetParameterValue(string parameter, string @default)
        {
            if (parameters.Exists(p => p.Key == parameter))
                return GetParameterValue(parameter);
            return @default;
        }

        /// <summary>
        /// Gets the list of values for the specified parameter.
        /// </summary>
        /// <param name="parameter">The name of a parameter.</param>
        /// <returns>The value for the specified parameter.</returns>
        /// <remarks>
        /// <para>Parameter name is case-sensitive.</para>
        /// <para>If the uri doesn't contain the parameter, the output collection is empty.</para>		
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="parameter"/> is null.</exception>
        /// <seealso cref="this[string]"/>
        /// <seealso cref="GetParameterValue(string)"/>
        /// <seealso cref="GetParameterValue(string, string)"/>
        /// <seealso cref="GetParameterValues(string, string)"/>
        public List<string> GetParameterValues(string parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException("parameter");
            List<string> res = new List<string>();
            foreach (var item in parameters)
            {
                if (item.Key == parameter)
                    res.Add(item.Value);
            }
            return res;
        }

        /// <summary>
        /// Gets the list of values for the specified parameter or the default value, if 
        /// the uri doesn't contain the parameter.
        /// </summary>
        /// <param name="parameter">The name of a parameter.</param>
        /// <param name="default">Default value that is returned if the uri doesn't contain the parameter.</param>
        /// <remarks>		
        /// <para>Parameter name is case-sensitive.</para>
        /// </remarks>
        /// <returns>The list of values for the specified parameter or a list with the default value only.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="parameter"/> is null.</exception>
        /// <seealso cref="this[string]"/>
        /// <seealso cref="GetParameterValue(string)"/>
        /// <seealso cref="GetParameterValues(string)"/>
        /// <seealso cref="GetParameterValues(string, string)"/>
        public List<string> GetParameterValues(string parameter, string @default)
        {
            var res = GetParameterValues(parameter);
            if (res.Count == 0)
                res.Add(@default);
            return res;
        }

        /// <summary>
        /// Gets the keys of the parameters.
        /// </summary>
        public List<string> ParameterKeys
        {
            get
            {
                List<string> keys = new List<string>();
                foreach (var item in parameters)
                {
                    keys.Add(item.Key);
                }
                keys.Sort();
                for (int i = keys.Count - 1; --i >= 0; )
                {
                    if (keys[i + 1] == keys[i]) keys.RemoveAt(i + 1);
                }
                return keys;
            }
        }

        /// <summary>
        /// Returns the number of occurences of the parameter in the uri.
        /// </summary>
        /// <param name="parameter">The parameter to locate in the uri.</param>
        /// <returns>Returns the number of occurences.</returns>
        /// <remarks>
        /// <para>Parameter name is case-sensitive.</para>
        /// </remarks>
        /// <seealso cref="this[string]"/>
        /// <seealso cref="GetParameterValue(string)"/>
        /// <seealso cref="GetParameterValue(string, string)"/>
        /// <seealso cref="GetParameterValues(string)"/>
        /// <seealso cref="GetParameterValues(string, string)"/>
        public int GetParameterOccurences(string parameter)
        {
            int n = 0;
            foreach (var item in parameters)
            {
                if (item.Key == parameter) n++;
            }
            return n;
        }

        /// <summary>
        /// Determines whether the uri contains the parameter.
        /// </summary>
        /// <param name="parameter">The parameter to locate in the uri.</param>
        /// <returns>Returns the value indicating whether the uri contains the parameter.</returns>
        /// <remarks>
        /// <para>Parameter name is case-sensitive.</para>
        /// </remarks>
        /// <seealso cref="this[string]"/>
        /// <seealso cref="GetParameterValue(string)"/>
        /// <seealso cref="GetParameterValue(string, string)"/>
        /// <seealso cref="GetParameterValues(string)"/>
        /// <seealso cref="GetParameterValues(string, string)"/>
        public bool ContainsParameter(string parameter)
        {
            return GetParameterOccurences(parameter) > 0;
        }

        /// <summary>
        /// Converts the URI into a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (cachedToString == null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(DataSetUriScheme).Append(':').Append(this.provider);
                if ((parameters != null) && (parameters.Count > 0))
                {
                    sb.Append('?');
                    int i = 0;
                    foreach (var v in parameters)
                    {
                        i++;
                        sb.Append(v.Key).Append('=');
                        if (v.Key == "include")
                            sb.Append(EscapeDataString(v.Value));
                        else sb.Append(v.Value);
                        if (i < parameters.Count) sb.Append('&');
                    }
                }
                cachedToString = sb.ToString();
            }
            return cachedToString;
        }

        #region Standard Flags
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
        /// DataSet dataSet = DataSet.Open("msds:nc?file=data.nc&amp;openMode=open"); 
        /// DataSet dataSet = DataSet.Open("msds:as?server=(local)&amp;database=ActiveStorage&amp;integrated security=true&amp;GroupName=mm5&amp;UseNetcdfConventions=true&amp;openMode=createNew");
        /// </code>
        /// </example>
        /// </remarks>
        /// <seealso cref="GetOpenModeOrDefault"/>
        public ResourceOpenMode OpenMode
        {
            get
            {
                // Predefined parameters
                int openModeOccurences = GetParameterOccurences("openMode");
                if (openModeOccurences == 1)
                {
                    string mode = GetParameterValue("openMode");
                    switch (mode)
                    {
                        case "createNew":
                            return ResourceOpenMode.CreateNew;
                        case "create":
                            return ResourceOpenMode.Create;
                        case "open":
                            return ResourceOpenMode.Open;
                        case "openOrCreate":
                            return ResourceOpenMode.OpenOrCreate;
                        case "readOnly":
                            return ResourceOpenMode.ReadOnly;
                        default:
                            throw new Exception("Wrong value of the openMode flag");
                    }
                }
                else if (openModeOccurences > 1)
                    throw new Exception("openMode parameter cannot occur more than once");

                // default
                return ResourceOpenMode.OpenOrCreate;
            }
            set
            {
                switch (value)
                {
                    case ResourceOpenMode.CreateNew:
                        SetParameterValue("openMode", "createNew");
                        break;
                    case ResourceOpenMode.Create:
                        SetParameterValue("openMode", "create");
                        break;
                    case ResourceOpenMode.Open:
                        SetParameterValue("openMode", "open");
                        break;
                    case ResourceOpenMode.OpenOrCreate:
                        SetParameterValue("openMode", "openOrCreate");
                        break;
                    case ResourceOpenMode.ReadOnly:
                        SetParameterValue("openMode", "readOnly");
                        break;
                }
            }
        }

        /// <summary>
        /// Returns the open mode if it is specified in the uri; otherwise, returns <paramref name="default"/> value.
        /// </summary>
        /// <param name="default">The mode that should be returned if it is not specified in the uri.</param>
        /// <returns>The open mode.</returns>
        /// <seealso cref="OpenMode"/>
        public ResourceOpenMode GetOpenModeOrDefault(ResourceOpenMode @default)
        {
            int n = GetParameterOccurences("openMode");
            if (n == 0)
                return @default;
            if (n > 1)
                throw new NotSupportedException("There are multiple openMode parameter in the uri");
            return OpenMode;
        }


        #endregion

        /// <summary>
        /// Checks whether the given string is a DataSet uri.
        /// </summary>
        /// <param name="uri">The string to check.</param>
        /// <remarks>
        /// The method checks whether the string <paramref name="uri"/>
        /// identifies the data set uri scheme (see <see cref="DataSetUriScheme"/>).
        /// For example, "msds:csv?file=tests.csv" is a DataSet uri,
        /// and "http://microsoft.com" or "c:\Data\tests.csv" are not DataSet uri.
        /// </remarks>
        /// <returns>True, if the <paramref name="uri"/> is a DataSet uri.</returns>
        public static bool IsDataSetUri(string uri)
        {
            if (String.IsNullOrEmpty(uri)) return false;
            return uri.StartsWith(DataSetUri.DataSetUriScheme + ":");
        }

        /// <summary>
        /// Transforms the given <paramref name="path"/> into a DataSet URI string.
        /// </summary>
        /// <param name="path">Path to a resource the DataSet should be created for.</param>
        /// <param name="providerName">Name of the provider for the DataSetUri.</param>
        /// <param name="resourceParamName">The parameter name that should be equal to <paramref name="path"/>
        /// (usually, <c>"file"</c>).</param>
        /// <returns>A DataSet URI string containing
        /// a reference to the resource identified by <paramref name="path"/>.</returns>
        /// <remarks>
        /// <para>
        /// The method has to convert a path with possible parameters appended through '?' into
        /// a DataSet URI string.
        /// For example, a path <c>c:\data\air0.csv</c> or even <c>c:\data\air0.csv?inferDims=true&amp;culture=ru-RU</c>
        /// are to be converted into 
        /// <c>msds:csv?file=c:\data\air0.csv</c> and <c>msds:csv?file=c:\data\air0.csv&amp;inferDims=true&amp;culture=ru-RU</c>,
        /// if <paramref name="resourceParamName"/> is <c>"file"</c> (the usual name for the parameter
        /// referring to a DataSet underlying resource file) and <paramref name="providerName"/>
        /// is <c>"csv"</c>.
        /// </para>
        /// <para>All <paramref name="path"/>, <paramref name="providerName"/> and <paramref name="resourceParamName"/>
        /// cannot be null or an empty string; otherwise an exception is thrown.</para>
        /// </remarks>
        /// <exception cref="ArgumentException">Null or empty string specified as any of the parameters</exception>
        [Obsolete("Use DataSetUri.Create() instead.")]
        public static string CreateFromPath(string path, string providerName, string resourceParamName)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentException("path mustn't be empty");
            if (String.IsNullOrEmpty(resourceParamName))
                throw new ArgumentException("resourceParamName mustn't be empty");
            if (String.IsNullOrEmpty(providerName))
                throw new ArgumentException("providerName mustn't be empty");

            int qm = path.IndexOf('?');
            string parameters = null;
            if (qm >= 0 && qm < path.Length - 1)
            {
                parameters = path.Substring(qm + 1, path.Length - qm - 1);
                path = path.Substring(0, qm);
            }
            else if (qm == path.Length - 1)
            {
                path = path.Substring(0, path.Length - 1);
            }
            CheckPath(path);

            StringBuilder sb = new StringBuilder(DataSetUri.DataSetUriScheme);
            sb.Append(':').Append(providerName).Append('?');
            sb.Append(resourceParamName).Append('=').Append(path);
            if (parameters != null)
            {
                sb.Append('&').Append(parameters);
            }
            return sb.ToString();
        }

        private static void CheckPath(string path)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be emty");
            var invalidChars = System.IO.Path.GetInvalidPathChars();
            for (int i = 0; i < path.Length; i++)
            {
                if (Array.IndexOf(invalidChars, path[i]) >= 0 || path[i] == '&')
                    throw new ArgumentException("Path has invalid character");
            }
        }


        /// <summary>
        /// Creates the <see cref="DataSetUri"/> instance for the specified <paramref name="uri"></paramref>.
        /// </summary>
        /// <param name="uri">Uri containing provider name and probably parameters.</param>
        /// <returns><see cref="DataSetUri"/> instance.</returns>
        /// <remarks>
        /// <para>
        /// Each DataSet provider type can have a related class derived from the <see cref="DataSetUri"/> type
        /// linked to it through the <see cref="DataSetProviderUriTypeAttribute"/> attribute.
        /// The instance of the class enables a prior customization of parameters specific to the DataSet provider
        /// through public properties of the class. Then the Uri instance can be used to construct
        /// a particular DataSet (see <see cref="DataSet.Open(DataSetUri)"/>).
        /// </para>
        /// <example>
        /// <see cref="DataSetUri"/> can be created from a name of a DataSet provider
        /// (see <see cref="DataSetProviderNameAttribute"/>).
        /// All parameters have default values.
        /// <code>
        /// DataSetUri uri = DataSetUri.Create("msds:csv"); // creates CsvUri
        /// uri.Separator = Delimiter.Semicolon;  // changing default separator
        /// uri.FileName = "something.csv"; // specifying file name
        /// DataSet ds = DataSet.Open(uri); // constructing a data set from the customized uri
        /// </code>
        /// <see cref="DataSetUri"/> can be created from a URI with extra parameters defined:
        /// <code>
        /// DataSetUri uri = DataSetUri.Create("msds:csv?file=something.csv");
        /// uri.Separator = Delimiter.Semicolon;
        /// uri.SaveHeader = false;
        /// DataSet ds = DataSet.Open(uri);
        /// </code>
        /// <see cref="DataSetUri"/> can be created from one of extension associated with the DataSet provider
        /// (see <see cref="DataSetProviderFileExtensionAttribute"/>).
        /// <code>
        /// DataSet uri = DataSetUri.Create(".csv");
        /// uri.Separator = Delimiter.Semicolon;
        /// uri.FileName = "something.csv";
        /// DataSet ds = DataSet.Open(uri);
        /// </code>
        /// <see cref="DataSetUri"/> can be created from a file path with some extra parameters defined:
        /// <code>
        /// DataSetUri uri = DataSetUri.Create("something.csv?separator=tab&amp;openMode=readOnly");
        /// uri.SaveHeader = false;
        /// DataSet ds = DataSet.Open(uri);
        /// </code> 
        /// <see cref="DataSetUri"/> can be created from a DataSet provider type:
        /// <code>
        /// DataSetUri uri = DataSetUri.Create(typeof(CsvDataSet));
        /// uri.SaveHeader = false;
        /// uri.FileName = "something.csv";
        /// DataSet ds = DataSet.Open(uri);
        /// </code> 
        /// </example>
        /// </remarks>
        /// <seealso cref="Create(Type)"/>
        /// <seealso cref="DataSet.Open(DataSetUri)"></seealso>
        /// <seealso cref="DataSet.Open(string)"></seealso>
        public static DataSetUri Create(string uri)
        {
            return Factory.DataSetFactory.CreateUri(uri);
        }

        /// <summary>
        /// Creates the <see cref="DataSetUri"/> instance for the <paramref name="providerType"/>.
        /// </summary>
        /// <param name="providerType">Type of the provider whose URI is to be created.</param>
        /// <returns><see cref="DataSetUri"/> instance.</returns>
        /// <remarks>
        /// See remarks for <see cref="Create(string)"/>.
        /// </remarks>
        /// <seealso cref="DataSet.Open(DataSetUri)"/>
        /// <seealso cref="Create(string)"/>
        public static DataSetUri Create(Type providerType)
        {
            return Factory.DataSetFactory.CreateUri(providerType);
        }

        /// <summary>
        /// Updates the properties of the <paramref name="uri"/> marked with <see cref="FileNamePropertyAttribute"/>,
        /// <see cref="UriPropertyAttribute"/>
        /// according to <see cref="Microsoft.Research.Science.Data.Factory.DataSetFactory.BaseUri"/> property.
        /// </summary>
        /// <param name="uri"></param>
        /// <remarks>
        /// <para>
        /// The method finds all properties of the <paramref name="uri"/> marked
        /// with <see cref="FileNamePropertyAttribute"/>, <see cref="UriPropertyAttribute"/> and having value
        /// that is a relative local path, and then updates these properties
        /// according to current value of <see cref="Microsoft.Research.Science.Data.Factory.DataSetFactory.BaseUri"/>
        /// property.
        /// </para>
        /// </remarks>
        public static void NormalizeUri(DataSetUri uri)
        {
            NormalizeFileNames(uri);
            NormalizeDirectoryPath(uri);
            NormalizeUris(uri);
        }

        /// <summary>
        /// Updates the properties of the <paramref name="uri"/> marked with <see cref="FileNamePropertyAttribute"/>
        /// according to <see cref="Microsoft.Research.Science.Data.Factory.DataSetFactory.BaseUri"/> property.
        /// </summary>
        /// <param name="uri"></param>
        /// <remarks>
        /// <para>
        /// The method finds all properties of the <paramref name="uri"/> marked
        /// with <see cref="FileNamePropertyAttribute"/> and having value
        /// that is a relative local path, and then updates these properties
        /// according to current value of <see cref="Microsoft.Research.Science.Data.Factory.DataSetFactory.BaseUri"/>
        /// property.
        /// </para>
        /// </remarks>
        [Obsolete("Use DataSetUri.NormalizeUri(DataSetUri) instead.")]
        public static void NormalizeFileNames(DataSetUri uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            Type uriType = uri.GetType();
            PropertyInfo[] properties = uriType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            foreach (var prop in properties)
            {
                if (prop.PropertyType != typeof(string) || !prop.CanWrite || !prop.CanRead) continue;
                object[] objs = prop.GetCustomAttributes(typeof(FileNamePropertyAttribute), false);
                if (objs == null || objs.Length == 0) continue; // not a file name property

                string file = (string)prop.GetValue(uri, null);
                if (!String.IsNullOrEmpty(file))
                {
                    Uri resourceUri;
                    bool uriCreated = Uri.TryCreate(file, UriKind.RelativeOrAbsolute, out resourceUri);
                    if (!uriCreated || !resourceUri.IsAbsoluteUri || resourceUri.Scheme == UriSchemeFile) // 
                    {
                        if (!System.IO.Path.IsPathRooted(file))
                            if (DataSetFactory.BaseUri != null)
                                file = System.IO.Path.Combine(DataSetFactory.BaseUri, file);
                            else
                                file = System.IO.Path.Combine(Environment.CurrentDirectory, file);
                        prop.SetValue(uri, file, null);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the properties of the <paramref name="uri"/> marked with <see cref="DirectoryPropertyAttribute"/>
        /// according to <see cref="Microsoft.Research.Science.Data.Factory.DataSetFactory.BaseUri"/> property.
        /// </summary>
        /// <param name="uri"></param>
        /// <remarks>
        /// <para>
        /// The method finds all properties of the <paramref name="uri"/> marked
        /// with <see cref="DirectoryPropertyAttribute"/> and having value
        /// that is a relative local path, and then updates these properties
        /// according to current value of <see cref="Microsoft.Research.Science.Data.Factory.DataSetFactory.BaseUri"/>
        /// property.
        /// </para>
        /// </remarks>
        private static void NormalizeDirectoryPath(DataSetUri uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            Type uriType = uri.GetType();
            PropertyInfo[] properties = uriType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            foreach (var prop in properties)
            {
                if (prop.PropertyType != typeof(string) || !prop.CanWrite || !prop.CanRead) continue;
                object[] objs = prop.GetCustomAttributes(typeof(DirectoryPropertyAttribute), false);
                if (objs == null || objs.Length == 0) continue; // not a directory path property

                string dir = (string)prop.GetValue(uri, null);
                if (!String.IsNullOrEmpty(dir))
                {
                    Uri resourceUri;
                    bool uriCreated = Uri.TryCreate(dir, UriKind.RelativeOrAbsolute, out resourceUri);
                    if (!uriCreated || !resourceUri.IsAbsoluteUri || resourceUri.Scheme == UriSchemeFile) // 
                    {
                        if (!System.IO.Path.IsPathRooted(dir))
                            if (DataSetFactory.BaseUri != null)
                                dir = System.IO.Path.Combine(DataSetFactory.BaseUri, dir);
                            else
                                dir = System.IO.Path.Combine(Environment.CurrentDirectory, dir);
                        prop.SetValue(uri, dir, null);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the properties of the <paramref name="uri"/> marked with <see cref="UriPropertyAttribute"/>
        /// to include <see cref="Microsoft.Research.Science.Data.Factory.DataSetFactory.BaseUri"/> if present.
        /// </summary>
        /// <param name="uri"></param>
        /// <remarks>
        /// <para>
        /// The method finds all properties of the <paramref name="uri"/> marked
        /// with <see cref="UriPropertyAttribute"/> and having value
        /// that is a relative Uri, and then updates these properties
        /// using value of <see cref="Microsoft.Research.Science.Data.Factory.DataSetFactory.BaseUri"/> property.
        /// </para>
        /// </remarks>
        [Obsolete("Use DataSetUri.NormalizeUri(DataSetUri) instead.")]
        public static void NormalizeUris(DataSetUri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            if (String.IsNullOrEmpty(DataSetFactory.BaseUri))
                return;
            Type uriType = uri.GetType();
            PropertyInfo[] properties = uriType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            foreach (var prop in properties)
            {
                if (prop.PropertyType == typeof(string) && prop.CanWrite && prop.CanRead)
                {
                    object[] objs = prop.GetCustomAttributes(typeof(UriPropertyAttribute), false);
                    if (objs != null && objs.Length > 0)
                    {
                        string v = (string)prop.GetValue(uri, null);
                        if (!String.IsNullOrEmpty(v))
                        {
                            Uri localUri;
                            if (Uri.TryCreate(v, UriKind.RelativeOrAbsolute, out localUri) && !localUri.IsAbsoluteUri)
                            {
                                v = new Uri(new Uri(DataSetFactory.BaseUri), v).ToString();
                            }
                            prop.SetValue(uri, v, null);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the parameter "file" in the <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri"></param>
        /// <remarks>
        /// <para>
        /// If the <paramref name="uri"/> contains parameter "file" and its value
        /// is a relative local path, the method updates the <paramref name="uri"/>,
        /// assigning to the "file" full path using current directory.
        /// </para>
        /// </remarks>
        [Obsolete("Use method DataSetUri.NormalizeFileNames instead.")]
        public static void NormalizeFileName(DataSetUri uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (!uri.ContainsParameter("file")) return;

            string file = uri["file"];
            if (String.IsNullOrEmpty(file)) return;

            Uri resourceUri;
            bool uriCreated = Uri.TryCreate(file, UriKind.RelativeOrAbsolute, out resourceUri);
            if (!uriCreated || !resourceUri.IsAbsoluteUri || resourceUri.Scheme == UriSchemeFile) // 
            {
                if (!System.IO.Path.IsPathRooted(file))
                    if (DataSetFactory.BaseUri != null)
                        file = System.IO.Path.Combine(DataSetFactory.BaseUri, file);
                    else
                        file = System.IO.Path.Combine(Environment.CurrentDirectory, file);
                uri["file"] = file;
            }
        }

        /// <summary>
        /// Clones the given uri, replacing absolute file and directory paths with relative.
        /// </summary>
        /// <param name="uri">Input DataSetUri.</param>
        /// <param name="basePath">The base path to make relative paths.</param>
        /// <returns>New DataSetUri.</returns>
        public static DataSetUri GetRelativeUri(DataSetUri uri, string basePath)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (basePath == null) throw new ArgumentNullException("basePath");
            if (!System.IO.Path.IsPathRooted(basePath)) throw new ArgumentException("basePath must be rooted");

            uri = DataSetUri.Create(uri.ToString()); // cloning
            if (basePath[basePath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                basePath += System.IO.Path.DirectorySeparatorChar;

            Type uriType = uri.GetType();
            PropertyInfo[] properties = uriType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            foreach (var prop in properties)
            {
                if (prop.PropertyType == typeof(string) && prop.CanWrite && prop.CanRead)
                {
                    bool attributed = prop.GetCustomAttributes(typeof(FileNamePropertyAttribute), false).Length > 0 ||
                        prop.GetCustomAttributes(typeof(DirectoryPropertyAttribute), false).Length > 0;
                    if (attributed)
                    {
                        string v = (string)prop.GetValue(uri, null);
                        if (!String.IsNullOrEmpty(v))
                        {
                            Uri localUri;
                            if (Uri.TryCreate(v, UriKind.Absolute, out localUri) && v.StartsWith(basePath))
                            {                                
                                v = new Uri(basePath).MakeRelativeUri(localUri).ToString();
                                prop.SetValue(uri, v, null);
                            }
                        }
                    }
                }
            }
            return uri;
        }
    }

    /// <summary>
    /// The flag "openMode" specifies how the data set should open a file, data base or whatever resource it uses to store the data.
    /// </summary>
    /// <seealso cref="DataSetUri.OpenMode"/>
    public enum ResourceOpenMode
    {
        /// <summary>
        /// Specifies that the data set should create new resource. If the resource already exists, an exception IOException is thrown.  
        /// </summary>
        CreateNew,
        /// <summary>
        /// Specifies that the data set should create a new resource. If the resource already exists, it will be re-created.
        /// </summary>
        Create,
        /// <summary>
        /// Specifies that the data set should open an existing resource. If the resource does not exist, an exception ResourceNotFoundException is thrown.
        /// </summary>
        Open,
        /// <summary>
        /// Specifies that the data set should open a resource if it exists; otherwise, a new resource should be created.
        /// </summary>
        OpenOrCreate,
        /// <summary>
        /// Specified that the data set should open an existing resource in read-only mode
        /// </summary>
        ReadOnly
    }
}

