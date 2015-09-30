using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Science.Data;
using System.Net;
using System.IO;
using System.Configuration;
using Microsoft.Research.Science.Data.Climate;
using System.Diagnostics;
using System.Threading;
using System.IO.Compression;
using System.Security.Cryptography;
using Microsoft.Research.Science.Data.Climate.Common;

namespace Microsoft.Research.Science.Data.Processing
{
    /// <summary>
    /// Wrapper class for Fetch CLimate REST API. Provides methods for processing <see cref="DataSet"/> via FC.
    /// </summary>
    public class RestApiWrapper
    {
        private string serviceUrl;

        const string DefaultServiceURI = "http://fetchclimatesvc.cloudapp.net/fetch2";

        private static RestApiWrapper instance;

        /// <summary>
        /// Filters trace messages related to the DataSetAzureCache.
        /// </summary>
        public static readonly TraceSwitch Tracer = new TraceSwitch("RestApiWrapperTraceSwitch", "Filters trace messages related to the RestApiWrapper.", "Error");

        /// <summary>
        /// Maximum request execution time without result return in milliseconds.
        /// </summary>
        /// <remarks>
        /// If request exceeds this time, <see cref="DataSet"/> instance with requets hash next time to request will be returned.
        /// Pass it to the <see cref="Process"/> method after specified time interval to get the result.
        /// </remarks>
        public const int maxExecutionTimeWithoutReturns = 40000;

        /// <summary>
        /// Request timeout in milliseconds.
        /// </summary>
        private const int requestTimeout = 2000000;

        /// <summary>
        /// Number of retry attempts.
        /// </summary>
        private const int retriesCount = 7;

        /// <summary>
        /// Starting time delta between retries in milliseconds.
        /// </summary>
        private const int retryStartTimeDelta = 2000;

        /// <summary>
        /// Increase coefficient of time delta.
        /// </summary>
        private const double retryTimeIncreaseCoeff = 2;

        /// <summary>
        /// Gets service url of this <see cref="RestApiWrapper"/> instance.
        /// </summary>
        public string ServiceUrl
        {
            get { return this.serviceUrl; }
        }

        /// <summary>
        /// Gets instance of <see cref="RestApiWrapper"/> class with <see cref="ServiceUrl"/> from config.
        /// </summary>
        /// <remarks>
        /// <see cref="ServiceUrl"/> for this instance is specified in <see cref="RestServiceConfiguration"/> section of
        /// app.config file. To set service url manually, or to change it, use <see cref="SetServiceUrl"/> method.
        /// </remarks>
        public static RestApiWrapper Instance
        {
            get
            {
                if (RestApiWrapper.instance == null)
                {
                    try
                    {
                        var settings = ServiceLocationConfiguration.Current;
                        string serviceUrl = (settings == null) ? (DefaultServiceURI) : (settings.ServiceURL);
                        RestApiWrapper.instance = new RestApiWrapper(serviceUrl);
                    }
                    catch (ConfigurationErrorsException ex)
                    {
                        throw new ConfigurationErrorsException(
                            "Failed to create RestApiWrapper instance from app.config." + '\n' +
                            "Ensure that there are needed attributes, or set instance configuration" + '\n' +
                            "manually by calling SetServiceUrl method.", ex);
                    }
                }
                return RestApiWrapper.instance;
            }
        }

        /// <summary>
        /// Creates new instance of <see cref="RestApiWrapper"/>
        /// </summary>
        /// <param name="serviceUrl">Service url of this <see cref="RestApiWrapper"/> instance.</param>
        public RestApiWrapper(string serviceUrl)
        {
            ServicePoint servicePoint = ServicePointManager.FindServicePoint(new Uri(serviceUrl));
            servicePoint.Expect100Continue = false;

            if (string.IsNullOrEmpty(serviceUrl))
            {
                throw new ArgumentException("Wrong input service url string", "serviceUrl");
            }

            if (!Uri.IsWellFormedUriString(serviceUrl, UriKind.Absolute))
            {
                throw new ArgumentException("Service uri must be well formed uri string", "serviceUri");
            }

            this.serviceUrl = serviceUrl;
        }

        /// <summary>
        /// Processes request <see cref="DataSet"/> via Fetch Climate.
        /// </summary>
        /// <param name="ds">Request <see cref="DataSet"/> with fetching parameters.</param>
        /// <returns>DataSet with response.</returns>
        ///  <remarks>
        /// <para>
        /// There are two types of responses from server:
        /// If request time is lower than <see cref="maxExecutionTimeWithoutReturns"/> time, 
        /// <see cref="DataSet"/> with request result will be returned.
        /// If request time exceeds <see cref="maxExecutionTimeWithoutReturns"/> time, <see cref="DataSet"/> 
        /// instance with requets hash and next time to request ("status response") will be returned instead of <see cref="DataSet"/> with result.
        /// Pass it to the <see cref="Process"/> method after specified time interval again to get the result.
        /// If client got status response, it must send it back to server to get next status response, or result. But there is exception in this rule (see next).
        /// For status response, there are three matdata attributes in it:
        /// -"ExpectedCalculationTime" is expected time, calculation will take.
        /// Client should request next time after this time will pass.
        /// -"Hash" is hash of original request dataset.
        /// -"ReplyWithRequestDs" indicates, whether client needs to resend request
        /// next time or not. If so, client needs to send entire request next time
        /// instead of status response, it got from server.
        /// </para>
        /// </remarks>
        public DataSet Process(DataSet ds)
        {
            int retryAttempt = 0;

            int retryTimeDelta = RestApiWrapper.retryStartTimeDelta;

            while (true)
            {
                if (retryAttempt > 1)
                {
                    //Increase retry time delta, if not first attempt and not first retry.
                    retryTimeDelta = (int)(Math.Round(retryTimeDelta * RestApiWrapper.retryTimeIncreaseCoeff));
                }

                if (retryAttempt > retriesCount)
                    break;
                else if (retryAttempt > 0)
                    Thread.Sleep(retryTimeDelta);

                try
                {
                    string login = null;
                    string password = null;

                    CredentialsManager.GetCredentials(out login, out password);

                    HttpWebRequest request = WebRequest.Create(serviceUrl) as HttpWebRequest;
                    var credentialCache = new CredentialCache();
                    credentialCache.Add(
                        new Uri(serviceUrl),
                        "Digest",
                        new NetworkCredential(login, password)
                    );
                    request.Credentials = credentialCache;
                    request.PreAuthenticate = true;
                    request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
                    request.Method = RestApiNamings.postRequestMethodName;
                    //request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                    request.ContentType = RestApiNamings.textRequestTypeName;
                    request.Timeout = RestApiWrapper.requestTimeout;

                    byte[] csvBytes = RestApiUtilities.GetCsvBytes(ds);

                    request.ContentLength = csvBytes.Length;

                    using (Stream dataStream = request.GetRequestStream())
                    {
                        dataStream.Write(csvBytes, 0, csvBytes.Length);
                    }

                    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                    {
                        if (response.ContentType == RestApiNamings.textRequestTypeName)
                        {
                            //DataSet is in response
                            using (Stream dataStream = response.GetResponseStream())
                            {
                                byte[] responseBytes = RestApiUtilities.ReadBytes(dataStream, (int)response.ContentLength);
                                DataSet resultDs = RestApiUtilities.GetDataSet(responseBytes);
                                return resultDs;
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException("Unexpected exception. You should never see this");
                        }
                    }
                }
                catch (WebException)
                {
                    Trace.WriteIf(RestApiWrapper.Tracer.TraceWarning, string.Format("Web exception occured while processing input dataSet. {0} retry attempts left", RestApiWrapper.retriesCount - retryAttempt));
                    retryAttempt++;
                }
            }

            throw new WebException("Failed to connect to service. Make sure, it's available.");
        }

        /// <summary>
        /// Sets new Service Url for <see cref="Instance"/> of <see cref="RestApiWrapper"/>
        /// </summary>
        /// <param name="serviceUrl">New service url.</param>
        public static void SetServiceUrl(string serviceUrl)
        {
            RestApiWrapper.instance = new RestApiWrapper(serviceUrl);
        }
    }
}