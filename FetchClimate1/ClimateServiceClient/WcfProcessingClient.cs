// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ServiceModel.Channels;
using System.ServiceModel;
using Microsoft.Research.Science.Data.Climate;
using Microsoft.Research.Science.Data.Climate.Conventions;
using System.IO;
using Microsoft.Research.Science.Data.Climate.Common;
using Microsoft.Research.Science.Data.Imperative;
using Microsoft.Research.Science.Data;

#if FC_WCFCLIENT
using Microsoft.Research.Science.Data.Proxy;
using Microsoft.Research.Science.Data.Proxy.WCF;
using Microsoft.Research.Science.Data.Utils;

namespace Microsoft.Research.Science.Data.Processing
{
    public class WcfProcessingClient : ProcessingClientBase
    {
        private static DispatcherQueue taskQueue;
        private string serviceUri;

        private NonDisposingServicePort _servicePort = null;

        protected NonDisposingServicePort ServicePort
        {
            get
            {
                lock (taskQueue)
                {
                    if (_servicePort == null)
                        _servicePort = new NonDisposingServicePort(WcfDataSetFactory.GetRemoteServicePort(this.serviceUri, "sdsControlEP", "sdsDataEP"));
                }
                return _servicePort;
            }
        }

        static WcfProcessingClient()
        {         
#if DEBUG
            //   Microsoft.Research.Science.Data.Factory.DataSetFactory.Register(typeof(Microsoft.Research.Science.Data.Memory.MemoryDataSet));
            //Microsoft.Research.Science.Data.Factory.DataSetFactory.Register(
            //    typeof(Microsoft.Research.Science.Data.CSV.CsvDataSet));
            //Microsoft.Research.Science.Data.Factory.DataSetFactory.Register(
            //    typeof(Microsoft.Research.Science.Data.Proxy.WCF.WcfDataSetFactory));
            //Console.WriteLine(Microsoft.Research.Science.Data.Factory.DataSetFactory.RegisteredToString());
#endif
            taskQueue = new DispatcherQueue("ClimateServiceClient", dispatcher);
        }

        static Dispatcher dispatcher = new Dispatcher(2, ThreadPriority.Normal, true, "ClimateClient");                    

        public WcfProcessingClient(string serviceUri)
            : base()
        {
            if (serviceUri == null)
                throw new ArgumentNullException("serviceUri");
            if (string.IsNullOrEmpty(serviceUri))
                throw new ArgumentException("Service URI is not specified", "serviceUri");
            
            this.serviceUri = serviceUri;
            
        }

        protected override DataSet ServerProcessInternal(DataSet ds)
        {
            if (serviceUri == "(local)")
                return LocalProcess(ds);

            string hash = DataSetDiskCache.ComputeHash(ds);

            DataSet proxyDataSet = null;

            // Creating new DataSet at the service.
            // TODO: fix following:       
            try
            {
                try
                {
                    proxyDataSet = ProxyDataSet.CreateProxySync(taskQueue, ServicePort, "msds:memory", false, 10 * 60 * 1000);
                }
                catch (CommunicationObjectFaultedException)
                {
                    //Connection to server closed.
                    //Recreate service port and try again.
                    if (proxyDataSet != null && !proxyDataSet.IsDisposed) proxyDataSet.Dispose();
                    this._servicePort = null;
                    proxyDataSet = ProxyDataSet.CreateProxySync(taskQueue, ServicePort, "msds:memory", false, 10 * 60 * 1000);
                }
                AutoResetEvent completed = new AutoResetEvent(false);
                OnCommittedHandler onCommitted = new OnCommittedHandler(completed, OnDataSetCommitted);
                proxyDataSet.Committed += onCommitted.Handler;

                proxyDataSet.IsAutocommitEnabled = false;
                FetchClimateRequestBuilder.CopyRequestedDataSet(ds, proxyDataSet, false);
                proxyDataSet.Metadata[Namings.metadataNameHash] = hash;
                proxyDataSet.Commit();

                if (proxyDataSet.HasChanges) proxyDataSet.Commit();

                completed.WaitOne();
                proxyDataSet.IsAutocommitEnabled = true;
                return proxyDataSet;
            }
            catch
            {
                if (proxyDataSet != null && !proxyDataSet.IsDisposed) proxyDataSet.Dispose();
                throw;
            }
        }

#if !RELEASE_ASSEMBLY
        protected override DataSet LocalProcess(DataSet ds)
        {
            DataSet resultDs = null;
            try
            {
                //Microsoft.Research.Science.Data.Factory.DataSetFactory.Register(typeof(Microsoft.Research.Science.Data.Memory2.ChunkedMemoryDataSet));
                resultDs = DataSet.Open("msds:memory");
                resultDs.IsAutocommitEnabled = false;
                FetchClimateRequestBuilder.CopyRequestedDataSet(ds, resultDs, false);
                if (resultDs.HasChanges) resultDs.Commit();
                Microsoft.Research.Science.Data.Climate.Processing.ClimateRequestProcessor.Process(resultDs, 0);

                if (FetchClimateRequestBuilder.IsProcessingSuccessful(resultDs))
                    ;//cache.Add(ds,ComputeHash(request));
                else if (!FetchClimateRequestBuilder.IsProcessingFailed(resultDs))
                    throw new Exception("Processor hasn't finished the work.");

                resultDs.IsAutocommitEnabled = true;
                return resultDs;
            }
            catch
            {
                if (resultDs != null && !resultDs.IsDisposed) resultDs.Dispose();
                throw;
            }
        }
#endif

        private void OnDataSetCommitted(DataSetCommittedEventArgs e, OnCommittedHandler handler)
        {
            if ((e.Changes.ChangesetSource & ChangesetSource.Remote) == 0)
            {
                return; // we're waiting for remote changes
            }
            var ds = e.DataSet;
            if (FetchClimateRequestBuilder.IsProcessingSuccessful(ds) || FetchClimateRequestBuilder.IsProcessingFailed(ds))
            {
                ds.Committed -= handler.Handler;
                handler.Completed.Set();
            }
        }
    }
}

#endif
