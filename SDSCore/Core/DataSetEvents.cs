// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Microsoft.Research.Science.Data
{
    /// <summary>
    /// Kind of changes in a data set.
    /// </summary>
    public enum DataSetChangeAction
    {
        /// <summary>New name is given to a data set.</summary>
        RenameOfDataSet,
        /// <summary>New variable is added to a data set.</summary>
        NewVariable,
        /// <summary>Variable is changed.</summary>
        UpdateOfVariable,
        /// <summary>New coordinate system is added.</summary>
        NewCoordinateSystem
    }

	/// <summary>
	/// Describes the source of the changeset.
	/// </summary>
	/// <remarks>
	/// Value <see cref="ChangesetSource.Remote"/> can be used by proxy DataSet 
	/// if it receives a changeset from its storage.
	/// Furthermore, both values can be used in a combination (via bitwise AND operation) 
	/// when proxy combines changes received from storage and induced through local DataSet API.
	/// </remarks>
	[Flags]
	public enum ChangesetSource
	{
		/// <summary>The changeset contains changes induced locally.</summary>
		Local = 1,
		/// <summary>The changeset contains changes induced remotely.</summary>
		/// <remarks>
		/// This value can be used, for example, by proxy DataSet if it receives a changeset from its storage service.
		/// </remarks>
		Remote = 2,
		/// <summary>The changeset contains both local and remote changes.</summary>
		LocalAndRemote = 3
	}

    internal delegate void DataSetCommittingEventHandler(object sender, DataSetCommittingEventArgs e);

    internal class DataSetCommittingEventArgs : EventArgs
    {
        private DataSet sds;

        private DataSet.Changes changes;

        private bool cancel;

        public DataSetCommittingEventArgs(DataSet sds, DataSet.Changes changes)
        {
            this.sds = sds;
            this.cancel = false;
            this.changes = changes;
        }

        /// <summary>
        /// Gets the changes to commit.
        /// </summary>
        public DataSet.Changes Changes
        {
            get { return changes; }
        }

        /// <summary>
        /// Gets the data set that is being committed.
        /// </summary>
        public DataSet DataSet
        {
            get { return sds; }
        }

        /// <summary>
        /// Gets or sets the value which indicates whether to cancel the commit or not.
        /// </summary>
        public bool Cancel
        {
            get { return cancel; }
            set { cancel = value; }
        }
    }

    internal delegate void DataSetRolledBackEventHandler(object sender, DataSetRolledBackEventArgs e);

	internal class DataSetRolledBackEventArgs : EventArgs
    {
        private DataSet sds;

        public DataSetRolledBackEventArgs(DataSet sds)
        {
            this.sds = sds;
        }

        /// <summary>
        /// Gets the DataSet that has just been rolled back.
        /// </summary>
        public DataSet DataSet
        {
            get { return sds; }
        }
    }

    /// <summary>
    /// The delegate for DataSet.Committed event handlers.
    /// </summary>
    /// <param name="sender">The committed data set.</param>
    /// <param name="e">Contains the description of the succeeded committing procedure.</param>
    public delegate void DataSetCommittedEventHandler(object sender, DataSetCommittedEventArgs e);

    /// <summary>
    /// Contains the description of the succeeded committing procedure.
    /// </summary>
    public class DataSetCommittedEventArgs : EventArgs
    {
        private DataSet sds;

        private DataSetChangeset changes;

        private DataSetSchema committedSchema;

        /// <summary>
        /// Initializes the instance of the class.
        /// </summary>
        /// <param name="sds">The committed data set.</param>
        /// <param name="changes">Committed changes.</param>
        /// <param name="committedSchema">DataSet schema after commit</param>
        internal DataSetCommittedEventArgs(DataSet sds, DataSetChangeset changes, DataSetSchema committedSchema)
        {
            this.sds = sds;
            this.changes = changes;
            this.committedSchema = committedSchema;
        }

        /// <summary>
        /// Gets just committed changes.
        /// </summary>
        public DataSetChangeset Changes
        {
            get { return changes; }
        }

        /// <summary>
        /// Gets the DataSet that has just been committed.
        /// </summary>
        public DataSet DataSet
        {
            get { return sds; }
        }

        /// <summary>Gets copy of DataSet schema after commit</summary>
        public DataSetSchema CommittedSchema
        {
            get { return committedSchema; }
        }
    }

	internal delegate void DataSetChangedEventHandler(object sender, DataSetChangedEventArgs e);

	internal class DataSetChangedEventArgs : EventArgs
    {
        private DataSet sds;

        private object target;

        private DataSetChangeAction action;

        private DataSetChangeset changes;

        public DataSetChangedEventArgs(DataSet sds, DataSetChangeAction action, object target, DataSetChangeset changes)
        {
            this.sds = sds;
            this.action = action;
            this.target = target;

            this.changes = changes;
        }

        /// <summary>
        /// Gets the recent changes in the data set.
        /// </summary>
        public DataSetChangeset Changes
        {
            get { return changes; }
        }

        /// <summary>
        /// Gets the DataSet that is changed (but not committed yet).
        /// </summary>
        public DataSet DataSet
        {
            get { return sds; }
        }

        /// <summary>
        /// Gets the kind of the change.
        /// </summary>
        public DataSetChangeAction Action
        {
            get { return action; }
        }

        /// <summary>
        /// Get the target of the changes, its particular type depends on kind of the action.
        /// </summary>
        public object Target
        {
            get { return target; }
        }
    }

	internal delegate void DataSetChangingEventHandler(object sender, DataSetChangingEventArgs e);

	internal class DataSetChangingEventArgs : EventArgs
    {
        private DataSet sds;

        private object target;

        private DataSetChangeAction action;

        private bool cancel;


        public DataSetChangingEventArgs(DataSet sds, DataSetChangeAction action, object target)
        {
            this.sds = sds;
            this.target = target;
            this.cancel = false;
            this.action = action;
        }

        /// <summary>
        /// Gets the DataSet that is changed (but not committed yet).
        /// </summary>
        public DataSet DataSet
        {
            get { return sds; }
        }

        /// <summary>
        /// Gets the kind of the change.
        /// </summary>
        public DataSetChangeAction Action
        {
            get { return action; }
        }

        /// <summary>
        /// Get the target of the changes, its particular type depends on kind of the action.
        /// </summary>
        public object Target
        {
            get { return target; }
        }

        /// <summary>
        /// Gets or sets the value which indicates whether to cancel the commit or not.
        /// </summary>
        public bool Cancel
        {
            get { return cancel; }
            set { cancel = value; }
        }
    }
}

