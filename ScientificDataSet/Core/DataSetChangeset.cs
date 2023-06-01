// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Microsoft.Research.Science.Data
{
	/// <summary>
	/// Represents changes in a DataSet.
	/// </summary>
	public class DataSetChangeset
	{
		private int changeSetId;

		private Variable.Changes[] varAdded;
		private Variable.Changes[] varUpdated;
        private Variable.Changes[] varAffected;

		private bool initialized = false;
		private DataSet.Changes changes;
		private DataSet dataSet;

		private ChangesetSource source;

		internal DataSetChangeset(DataSet sds, DataSet.Changes changes)
			: this(sds, changes, false)
		{
		}

		internal DataSetChangeset(DataSet sds, DataSet.Changes changes, bool initializeAtOnce)
		{
			if (changes == null)
				throw new ArgumentNullException("changes");
			lock (sds)
			{
				this.changes = changes;
				this.dataSet = sds;
				this.changeSetId = sds.Version;// +1;
				this.source = changes.ChangesetSource;

				if (initializeAtOnce)
					Initialize();
			}
		}

		/// <summary>
		/// Gets the changed DataSet instance.
		/// </summary>
		public DataSet DataSet
		{
			get { return dataSet; }
		}

		/// <summary>
		/// Gets the source of the changeset: it can be either local or remote or both.
		/// </summary>
		public ChangesetSource ChangesetSource
		{
			get { return source; }
		}

		/// <summary>
		/// Get the version number proposed for this changeset.
		/// </summary>
		public int ProposedVersion { get { return changeSetId; } }

		/// <summary>
		/// Returns changes for the variable with given id.
		/// If this variable hasn't been updated, returns null.
		/// </summary>
		/// <param name="varId"></param>
		/// <returns></returns>
		internal Variable.Changes GetChanges(int varId)
		{
			Variable.Changes vc = null;
			if (varAdded != null)
			{
				vc = Array.Find(varAdded, p => p.ID == varId);
				if (vc != null)
					return vc;
			}
			if (varUpdated != null)
			{
				vc = Array.Find(varUpdated, p => p.ID == varId);
			}
			return vc;
		}

		private void Initialize()
		{
			if (initialized) return;

			/* Variables */
			List<Variable.Changes> added = new List<Variable.Changes>();
			List<Variable.Changes> updated = new List<Variable.Changes>();

			VariableSchema[] initialVars =
				(changes.InitialSchema == null || changes.InitialSchema.Variables == null || changes.InitialSchema.Variables.Length == 0)
				? null : changes.InitialSchema.Variables;
			Variable.Changes vch = null;
			foreach (Variable v in changes.Variables)
			{
				if (initialVars == null || !Array.Exists<VariableSchema>(initialVars, iv => iv.ID == v.ID))
				{
					// New variable
					added.Add(changes.GetVariableChanges(v.ID).Clone());
				}
				else if ((vch = changes.GetVariableChanges(v.ID)) != null)
				{
					// Updated
					updated.Add(vch.Clone());
				}
			}
			varAdded = added.ToArray();
			varUpdated = updated.ToArray();


			initialized = true;
		}

		/// <summary>
		/// Gets an array of added variables.
		/// </summary>
		/// <seealso cref="UpdatedVariables"/>
		public Variable.Changes[] AddedVariables
		{
			get
			{
				Initialize();
				return varAdded;
			}
		}

		/// <summary>
		/// Gets an array of updated variables.
		/// </summary>
		/// <remarks>
		/// The array returned by this property doesn't include added variables
		/// even if they are updated just after they have been added. 
		/// To get an array of added variables, see property <see cref="AddedVariables"/>.
		/// </remarks>
		/// <seealso cref="AddedVariables"/>
		public Variable.Changes[] UpdatedVariables
		{
			get
			{
				Initialize();
				return varUpdated;
			}
		}

		/// <summary>
		/// Gets an array of variables affected by the DataSet changes.
		/// </summary>
		/// <remarks>
		/// The array returned by this property includes both added variables (see
		/// <see cref="AddedVariables"/>) and updated variables (<see cref="UpdatedVariables"/>).
		/// </remarks>
		/// <seealso cref="AddedVariables"/>
		/// <seealso cref="UpdatedVariables"/>
		public Variable.Changes[] AllAffectedVariables
		{
			get{
                if (varAffected == null)
                {
                    Initialize();
                    varAffected = new Variable.Changes[varAdded.Length + varUpdated.Length];
                    varAdded.CopyTo(varAffected, 0);
                    varUpdated.CopyTo(varAffected, varAdded.Length);
                }
                return varAffected;
			}
		}

		/// <summary>
		/// Returns brief desrcription of the changeset.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return String.Format("Changes: variables: {0} added, {1} updated",
				AddedVariables.Length, UpdatedVariables.Length);
		}
	}

}

