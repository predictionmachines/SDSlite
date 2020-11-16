// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Linq;
using Microsoft.Research.Science.Data;
using System.Windows.Threading;
using System.Collections.Generic;

using WPFDispatcher = System.Windows.Threading.Dispatcher;
using System.Threading;
using System.Diagnostics;
using System.Windows;

namespace Microsoft.Research.Science.Data.Utilities
{
    class ReplicationTarget
    {
        /// <summary>Weak reference to replica DataSet</summary>
        private WeakReference targetDataSet;

        /// <summary>Maps source variable ID to target (replica) variable ID</summary>
        private Dictionary<int, int> targetVars;

        public ReplicationTarget(DataSet td, Dictionary<int, int> tv)
        {
            targetDataSet = new WeakReference(td);
            targetVars = tv;
        }

        public int[] GetSourceIDs()
        {
            return targetVars.Keys.ToArray();
        }

        /// <summary>Gets target DataSet for replication or null if
        /// target was disposed or garbage collected</summary>
        public DataSet Target
        {
            get
            {
                object target = targetDataSet.Target;
                if (target != null)
                {
                    DataSet tds = (DataSet)target;
                    if (!tds.IsDisposed)
                        return tds;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the ID of the replicated variable that corresponds given source variable.
        /// </summary>
        /// <param name="sourceVarID"></param>
        /// <returns></returns>
        public int GetReplicaVarID(int sourceVarID)
        {
            int rid;
            if (!targetVars.TryGetValue(sourceVarID, out rid))
                throw new ArgumentException("There is no replicated variable for the given source variable");
            return rid;
        }

        public int GetSourceVarID(int replicaVarID)
        {
            return targetVars.First(pair => pair.Value == replicaVarID).Key;
        }

        public void ApplyMetadataChanges(int id, MetadataDictionary mc)
        {
            DataSet targetDS = Target;
            if (targetDS != null)
            {
                int targetID;
                if (targetVars.TryGetValue(id, out targetID))
                {
                    Variable v = targetDS.Variables.GetByID(targetID);
                    foreach (var p in mc)
                        v.Metadata[p.Key] = p.Value;
                }
            }
        }

        public void ApplyDataChanges(int sourceId, DataResponse response)
        {
            DataSet targetDS = Target;
            if (targetDS != null)
            {
                int targetID;
                if (targetVars.TryGetValue(sourceId, out targetID))
                    targetDS.Variables.GetByID(targetID).PutData(response.DataRequest.Origin, response.Data);
            }
        }
    }

	/// <summary>
	/// Data set replicator.
	/// </summary>
    public class DataSetReplicator : IDisposable, IWeakEventListener
    {

        /// <summary>Dispatcher to perform syncronization with UI thread</summary>
        private Dispatcher wpfDispatcher;

        /// <summary>Replication targets</summary>
        private List<ReplicationTarget> replicas = new List<ReplicationTarget>();

        /// <summary>Replication source</summary>
        private DataSet source;

        private List<DataSetCommittedEventArgs> commits = new List<DataSetCommittedEventArgs>();

        /// <summary>Data request that are under way right now</summary>
        private List<DataRequest> activeRequests = new List<DataRequest>();

        /// <summary>Metadata changes that will be applied when request finishes</summary>
        private List<Variable.Changes> changesToApply = new List<Variable.Changes>();

        /// <summary>Requests waiting to be posted</summary>
        private List<DataRequest> pendingRequests = new List<DataRequest>();

        /// <summary>List of variable IDs to request entire data array</summary>
        private List<int> entireDataRequests = new List<int>();

        /// <summary>True if there is request to call Sync in Dispatcher thread or false otherwise</summary>
        private bool syncEnqueued = false;

        private const DispatcherPriority Priority = DispatcherPriority.Input;

        private bool isDisposed = false;

        private delegate void SyncHandler();
        private delegate void SyncResponseHandler(MultipleDataResponse e);
        private delegate void SyncFailureHandler(Exception e);

		/// <summary>
		/// Initializes a new instance of the <see cref="Microsoft.Research.Science.Data.Utilities.DataSetReplicator"/> class.
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="wpfDispatcher">Wpf dispatcher.</param>
        public DataSetReplicator(DataSet source, WPFDispatcher wpfDispatcher)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (wpfDispatcher == null)
                throw new ArgumentNullException("wpfDispatcher");

            this.source = source;
            this.wpfDispatcher = wpfDispatcher;

            DataSetCommittedEventManager.AddListener(source, this);
        }

        /// <summary>
        /// Gets the DataSet that is a source for the replicator.
        /// </summary>
        public DataSet Source
        {
            get { return source; }
        }

        /// <summary>Creates data set with replicas of specified variables</summary>
		/// <param name="sourceVars">Variables from source data set to replicate</param>
        /// <returns>New data set with replicated variables</returns>
        /// <remarks>This method is not threadsafe and should be called from Dispatcher thread</remarks>
        public DataSet CreateReplica(params Variable[] sourceVars)
        {
            if (isDisposed)
                throw new ObjectDisposedException("DataSetReplicator");
            if (source.IsDisposed)
                throw new ObjectDisposedException("Source DataSet is already disposed");

            // Check prerequisites
            foreach (var v in sourceVars)
                if (v.DataSet != source)
                    throw new ArgumentException("Cannot replicate variable from DataSet other than source");

            // Create data set with variables with same names and dimensions
            DataSet targetDataSet = DataSet.Open("msds:memory");
            targetDataSet.IsAutocommitEnabled = false;
            Dictionary<int, int> targetVariables = new Dictionary<int, int>();
            foreach (var sv in sourceVars)
            {
                if (targetVariables.ContainsKey(sv.ID))
                    continue; // the variable is already added
                Variable tv = targetDataSet.AddVariable(sv.TypeOfData, sv.Name, null, null, sv.Dimensions.Select(d => d.Name).ToArray());
                targetVariables.Add(sv.ID, tv.ID);
                foreach (var mr in sv.Metadata)
                    tv.Metadata[mr.Key] = mr.Value;
                AddDataRequest(sv.ID, null, null); // Request entire data
            }

            // Create replication target structure and ask for data
            replicas.Add(new ReplicationTarget(targetDataSet, targetVariables));
            if (activeRequests.Count == 0)
                BeginDataRequest();

            return targetDataSet;
        }

        /// <summary>
        /// Gets the the replicated variable that corresponds given source variable.
        /// </summary>
        /// <param name="replica">Replicated DataSet.</param>
        /// <param name="sourceVarID">ID of the source variable.</param>
        /// <returns></returns>
        public Variable GetReplicaVariable(DataSet replica, int sourceVarID)
        {
            var rt = replicas.Find(p => p.Target == replica);
            if (rt == null) throw new ArgumentException("Given DataSet is not handled by the replicator");
            return replica.Variables.GetByID(rt.GetReplicaVarID(sourceVarID));
        }

        public IEnumerable<Variable> GetSourceVariables(DataSet replica, IEnumerable<Variable> replicaVars)
        {
            var rt = replicas.Find(p => p.Target == replica);
            if (rt == null) throw new ArgumentException("Given DataSet is not handled by the replicator");
            return replicaVars.Select(r => source[rt.GetSourceVarID(r.ID)]);
        }

        public bool ContainsTarget(DataSet replica)
        {
            return replicas.Any(p => p.Target == replica);
        }

        private void AddDataRequest(int sourceID, int[] origin, int[] shape)
        {
            if (!entireDataRequests.Contains(sourceID))
            {
                if (origin == null || shape == null)
                {
                    foreach (var dr in pendingRequests.Where(dr => dr.Variable.ID == sourceID).ToArray())
                        pendingRequests.Remove(dr);
                    pendingRequests.Add(DataRequest.GetData(source.Variables.GetByID(sourceID), null, null));
                    entireDataRequests.Add(sourceID);
                }
                else
                    pendingRequests.Add(DataRequest.GetData(source.Variables.GetByID(sourceID), origin, shape));
            }
        }

        private void BeginDataRequest()
        {
            if (isDisposed || source.IsDisposed)
                return;

            // Debug.WriteLine("DSR: Issuing requests " + pendingRequests.Count);

            // Copy requests from pending to active
            activeRequests.AddRange(pendingRequests);
            pendingRequests.Clear();
            entireDataRequests.Clear();
            wpfDispatcher.BeginInvoke(Priority, new Action<DataRequest[]>(
                (reqs) =>
                {
                    try
                    {
                        OnRequestCompleted(source.GetMultipleData(reqs));
                    }
                    catch (Exception exc)
                    {
                        OnRequestFailed(exc);
                    }
                }), (object)activeRequests.ToArray());
        }

        private void OnRequestCompleteAsync(AsyncMultipleDataResponse ar)
        {
            if (isDisposed)
                return;
            if (ar.IsSuccess)
                wpfDispatcher.BeginInvoke(Priority, 
                    new SyncResponseHandler(OnRequestCompleted),                    
                    ar.Response);
            else
                wpfDispatcher.BeginInvoke(Priority, 
                    new SyncFailureHandler(OnRequestFailed), 
                    ar.Exception);
        }

        private void OnRequestCompleted(MultipleDataResponse mdr)
        {
            if (isDisposed) 
                return;

            List<ReplicationTarget> entries = CaptureEntries();
            DataResponse[] responses = mdr;
            foreach (var re in entries)
            {
                foreach (var id in re.GetSourceIDs())
                {
                    foreach (var resp in responses.Where(r => r.DataRequest.Variable.ID == id))
                        re.ApplyDataChanges(id, resp);
                    foreach (var changes in changesToApply.Where(c => c.ID == id && c.MetadataChanges != null))
                        re.ApplyMetadataChanges(id, changes.MetadataChanges);
                }
            }
            activeRequests.Clear();
            changesToApply.Clear();

            CommitTargets(entries);

            // Process enqueued commits from proxy
            int commitCount = 0;
            lock (commits)
                commitCount = commits.Count;
            if (commitCount > 0 && !syncEnqueued)
                wpfDispatcher.BeginInvoke(Priority, new SyncHandler(Sync));

            // Start next requests if any
            if (pendingRequests.Count > 0)
                BeginDataRequest();
        }

        private void OnRequestFailed(Exception exc)
        {
            // Issue warning
            Trace.WriteLine(String.Format("Error reading from {0}: {1}", source.URI, exc.Message));

            if (isDisposed || source.IsDisposed)
                return;

            // Copy failed requests back to pending
            foreach (var dr in activeRequests)
                AddDataRequest(dr.Variable.ID, dr.Origin, dr.Shape);
            activeRequests.Clear();
        }

        /// <summary>This method is called when source DataSet is committed. It may be called
        /// in thread different from UI</summary>
        /// <param name="sender">DataSet that was committed</param>
        /// <param name="args">Information about DataSet changes</param>
        private void OnDataSetCommittedAsync(object sender, DataSetCommittedEventArgs args)
        {
            int activeRequestCount;
            lock (commits)
            {
                commits.Add(args);
                activeRequestCount = activeRequests.Count;
            }
            if (wpfDispatcher.Thread == Thread.CurrentThread) // This event is in UI thread
                Sync();
            else if (!syncEnqueued && activeRequestCount == 0)
            {
                syncEnqueued = true;
                wpfDispatcher.BeginInvoke(Priority, new SyncHandler(
                    () =>
                    {
                        try
                        {
                            if(!isDisposed)
                                Sync();
                        }
                        finally
                        {
                            syncEnqueued = false;
                        }
                    }
                ));
            }
        }

        private void Sync()
        {
            if (isDisposed || source.IsDisposed) return;

            // Capture changing states - replicas and commit records
            List<ReplicationTarget> entries = CaptureEntries();
            DataSetCommittedEventArgs[] capturedCommits;
            lock (commits)
                capturedCommits = commits.ToArray();
            // Debug.WriteLine("DSR: Commit count = " + capturedCommits.Length);
            int[] refids = GetReferencedIDs(entries).ToArray();

            // Gather data requests for all changes
            foreach (var ca in capturedCommits) // For all commits in queue
            {
                foreach (var id in refids) // For each reference variable
                {
                    if (!entireDataRequests.Contains(id)) // There are no requests for entire variable
                    {
                        Variable.Changes vc = ca.Changes.UpdatedVariables.FirstOrDefault(c => c.ID == id);
                        if (vc != null) // There is changes for variable
                        {
                            changesToApply.Add(vc);
                            // Some logic to figure out data requests to entire variable
                            int[] origin = vc.AffectedRectangle.Origin;
                            if (origin != null && origin.All(coord => coord == 0))
                                origin = null;
                            int[] shape = vc.AffectedRectangle.Shape;
                            if (shape != null)
                            {
                                bool entireShape = true;
                                for(int i =0;i<shape.Length;i++)
                                    if (shape[i] != vc.Shape[i])
                                    {
                                        entireShape = false;
                                        break;
                                    }
                                if (entireShape)
                                    shape = null; 
                            }
                            AddDataRequest(id, origin, shape);
                        }
                    }
                }
            }

            // Remove processed commit records from list
            lock (commits)
                foreach (var ca in capturedCommits)
                    commits.Remove(ca);

            // Issue data request if possible
            if (activeRequests.Count == 0)
                BeginDataRequest();
        }

        private void CommitTargets(List<ReplicationTarget> entries)
        {
            if (isDisposed) return;

            foreach (var e in entries)
                if (e.Target != null && e.Target.HasChanges)
                    try
                    {
                        e.Target.Commit();
                    }
                    catch // Exception when committing
                    {
                        if (pendingRequests.Count == 0 && // No requests are pending - no chance to fix situation automatically
                            !source.IsDisposed) 
                        {
                            foreach (var id in e.GetSourceIDs())
                                AddDataRequest(id, null, null); // Request entire data
                        }
                    }
        }

        private List<int> GetReferencedIDs(List<ReplicationTarget> entries)
        {
            List<int> refids = new List<int>();
            foreach (var e in entries)
            {
                object targetEntry = e.Target;
                if (targetEntry != null)
                    foreach (var id in e.GetSourceIDs())
                        if (!refids.Contains(id))
                            refids.Add(id);
            }
            return refids;
        }


        private List<ReplicationTarget> CaptureEntries()
        {
            List<ReplicationTarget> result = new List<ReplicationTarget>(replicas.Count);
            for (int i = 0; i < replicas.Count; )
            {
                DataSet targetDS = replicas[i].Target;
                if (targetDS != null)
                {
                    result.Add(replicas[i]);
                    i++;
                }
                else
                    replicas.RemoveAt(i); // Remove references that were garbage collected
            }
            return result;
        }

        #region IDisposable Members

        public void Dispose()
        {
            isDisposed = true;
            DataSetCommittedEventManager.RemoveListener(source, this);
            source = null;
            lock (commits)
            {
                commits.Clear();
                activeRequests.Clear();
                pendingRequests.Clear();
                entireDataRequests.Clear();               
            }

            foreach (var r in replicas)
            {
                DataSet target = r.Target;
                if (target != null)
                    target.Dispose();
            }
        }

        #endregion

        #region IWeakEventListener Members

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof(DataSetCommittedEventManager))
            {
                if(!isDisposed)
                    OnDataSetCommittedAsync(sender, (DataSetCommittedEventArgs)e);
                return true;
            }
            else
                return false;
        }

        #endregion
    }
}

