// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Research.Science.Data.Climate;
using Microsoft.Research.Science.Data.Climate.Conventions;
using System.IO;
using Microsoft.Research.Science.Data.Climate.Common;
using Microsoft.Research.Science.Data;

namespace Microsoft.Research.Science.Data.Processing
{
    public class RestProcessingClient : ProcessingClientBase
    {
        private static TimeSpan timeout = TimeSpan.FromHours(40);

        /// <summary>
        /// Gets or sets request timeout.
        /// </summary>
        public static TimeSpan Timeout
        {
            get { return RestProcessingClient.timeout; }
            set { RestProcessingClient.timeout = value; }
        }

#if DEBUG
        static RestProcessingClient()
        {

            //Microsoft.Research.Science.Data.Factory.DataSetFactory.Register(
            //    typeof(Microsoft.Research.Science.Data.CSV.CsvDataSet));
            //Microsoft.Research.Science.Data.Factory.DataSetFactory.Register(
            //    typeof(Microsoft.Research.Science.Data.Memory.MemoryDataSet));
        }
#endif

        protected override DataSet ServerProcessInternal(DataSet ds)
        {
            DateTime start = DateTime.Now;

            bool resultGot = false;

            DataSet inputDs = ds;

            DataSet resultDs = null;
            while (!resultGot)
            {
                resultDs = RestApiWrapper.Instance.Process(ds);
                if (FetchClimateRequestBuilder.IsResultDataSet(resultDs))
                {
                    resultGot = true;
                }
                else
                {
                    if (FetchClimateRequestBuilder.ResendRequest(resultDs))
                        ds = inputDs;
                    else
                        ds = resultDs;

                    int expectedCalculationTime = 0;
                    string hash = string.Empty;
                    FetchClimateRequestBuilder.GetStatusCheckParams(resultDs, out expectedCalculationTime, out hash);

                    Thread.Sleep(expectedCalculationTime);
                }

                if ((!resultGot) && (DateTime.Now - start) > timeout)
                {
                    throw new TimeoutException("Request to fetch climate has timed out. Increase timeout value or try again later.");
                }
            }

            return resultDs;
        }

//#if !RELEASE_ASSEMBLY
//        protected override DataSet LocalProcess(DataSet ds)
//        {
//            DataSet resultDs = null;
//            try
//            {
//                resultDs = DataSet.Open("msds:memory2");
//                resultDs.IsAutocommitEnabled = false;
//                FetchClimateRequestBuilder.CopyRequestedDataSet(ds, resultDs, false);
//                if (resultDs.HasChanges) resultDs.Commit();
//                Microsoft.Research.Science.Data.Climate.Processing.ClimateRequestProcessor.Process(resultDs, 0);

//                if (FetchClimateRequestBuilder.IsProcessingSuccessful(resultDs))
//                    ;//cache.Add(ds,ComputeHash(request));
//                else if (!FetchClimateRequestBuilder.IsProcessingFailed(resultDs))
//                    throw new Exception("Processor hasn't finished the work.");

//                resultDs.IsAutocommitEnabled = true;
//                return resultDs;
//            }
//            catch
//            {
//                if (resultDs != null && !resultDs.IsDisposed) resultDs.Dispose();
//                throw;
//            }
//        }
//#endif
    }
}