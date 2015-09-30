// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data
{
	/// <summary>
	/// RefVariableMetadata is a wrapper of an existing variable's metadata with 
	/// some additional capabilities.
	/// </summary>
	/// <remarks>
	/// <para>The <paramref name="hiddenEntries"/> and <paramref name="readonlyEntries"/> parameters 
	/// allow to make some metadata entries indpendent from the metadata of the underlying variable.
	/// These parameters can be null and this will be considered as an empty collection.</para>
	/// <para>Two entries are always independent from the underlying metadata. These are
	/// the name and the provenance entries of the variable's metadata.</para>
	/// </remarks>
	/// <seealso cref="MetadataDictionary"/>
	internal class RefVariableMetadata : MetadataDictionary
	{
		#region Private fields

		/// <summary>The target variable that contains referred metadata.</summary>
		private Variable target;

		/// <summary>Collection of keys which are not to be inherited from the underlying metadata.
		/// These entries are changed independently.</summary>
		private List<string> hiddenEntries;

		/// <summary>Collection of keys that cannot be changed through this collection.</summary>
		private List<string> readonlyEntries;

		/// <summary>Collection of entries currently being handled. This collection should
		/// enable correct concurrent work on simultaneous changing of the target and this collections.</summary>
		private List<KeyValuePair<string, object>> proposedMetadataEntries = new List<KeyValuePair<string, object>>();

		#endregion

		#region Constructors

		/// <summary>
		/// Creates an instance of the RefVariableMetadata class.
		/// </summary>
		/// <param name="var">The target variable that contains referred metadata.</param>
		/// <remarks>
		/// See remarks for the <see cref="RefVariableMetadata"/> class.
		/// </remarks>
		public RefVariableMetadata(Variable var)
			: this(var, null, null)
		{
		}

		/// <summary>
		/// Creates an instance of the RefVariableMetadata class.
		/// </summary>
		/// <param name="var">The target variable that contains referred metadata.</param>
		/// <param name="hiddenEntries">Collection of keys which are not to be inherited from the underlying metadata.
		/// These entries are changed independently.</param>
		/// <remarks>
		/// See remarks for the <see cref="RefVariableMetadata"/> class.
		/// </remarks>
		public RefVariableMetadata(Variable var, IList<string> hiddenEntries)
			: this(var, hiddenEntries, null)
		{
		}

		/// <summary>
		/// Creates an instance of the RefVariableMetadata class.
		/// </summary>
		/// <param name="var">The target variable that contains referred metadata.</param>
		/// <param name="hiddenEntries">Collection of keys which are not to be inherited from the underlying metadata.
		/// These entries are changed independently.</param>
		/// <param name="readonlyEntries">Collection of keys that cannot be changed through this collection.</param>
		/// <remarks>
		/// See remarks for the <see cref="RefVariableMetadata"/> class.
		/// </remarks>
		public RefVariableMetadata(Variable var, IList<string> hiddenEntries, IList<string> readonlyEntries)
		{
			if (var == null)
				throw new ArgumentNullException("var");
			this.target = var;

			this.hiddenEntries = new List<string>();
			if (hiddenEntries != null)
				this.hiddenEntries.AddRange(hiddenEntries);
			this.hiddenEntries.Add(this.KeyForName);

			this.readonlyEntries = new List<string>();
			if (readonlyEntries != null)
				this.readonlyEntries.AddRange(readonlyEntries);

			// Initializing the instance
			target.Metadata.ForEach(
				delegate(KeyValuePair<string, object> spair)
				{
					if (!IsHiddenEntry(spair.Key))
						this[spair.Key] = spair.Value;
				}, SchemaVersion.Recent);
		} 

		#endregion
		
		#region Source Variable Events Handling

		// TODO:srcM.Changing can fail after our Changing is done. 
		// For example, if another subsciber is called after this and it cancels the update.
		// Then the target will have the entry, this one - doesn't.
		private void MetadataChanging(object sender, VariableMetadataChangingEventArgs e)
		{
			if (IsReadOnlyEntry(e.Key))
			{
				e.Cancel = true;
				return;
			}

			if (!IsHiddenEntry(e.Key))
			{
				if (proposedMetadataEntries.Exists(entry =>
						entry.Key == e.Key && AreEquals(entry.Value, e.ProposedValue)))
					return;

				var pair = new KeyValuePair<string, object>(e.Key, e.ProposedValue);
				try
				{
					proposedMetadataEntries.Add(pair);
					target.Metadata[e.Key] = e.ProposedValue;
				}
				finally
				{
					proposedMetadataEntries.Remove(pair);
				}
			}
		}

		private void TargetVariableChanging(object sender, VariableChangingEventArgs e)
		{
			if (e.Action == VariableChangeAction.UpdateMetadata)
			{
				foreach (KeyValuePair<string, object> item in e.ProposedChanges.MetadataChanges)
				{
					if (IsHiddenEntry(item.Key)) continue;
					if (IsReadOnlyEntry(item.Key)) continue;

					if (proposedMetadataEntries.Exists(entry =>
						entry.Key == item.Key && AreEquals(entry.Value, item.Value)))
						continue;

					var pair = new KeyValuePair<string, object>(item.Key, item.Value);
					try
					{
						proposedMetadataEntries.Add(pair);
						this[item.Key] = item.Value;
					}
					finally
					{
						proposedMetadataEntries.Remove(pair);
					}
				}
			}
		}

		private bool IsHiddenEntry(string key)
		{
			return hiddenEntries.Contains(key);
		}

		private bool IsReadOnlyEntry(string key)
		{
			return readonlyEntries.Contains(key);	
		}

		internal MetadataDictionary FilterChanges(MetadataDictionary metadata)
		{
			MetadataDictionary md = new MetadataDictionary();
			if (metadata.Count == 0 && !metadata.HasChanges)
				return md;

			Dictionary<string, object> dict = metadata.AsDictionary(SchemaVersion.Recent);
			foreach (var item in dict)
			{
				if (!IsHiddenEntry(item.Key))
				{
					md[item.Key] = item.Value;
				}
			}
			return md;
		}

		#endregion

		internal void Subscribe()
		{
			this.target.Changing += new VariableChangingEventHandler(TargetVariableChanging);
			this.Changing += new VariableMetadataChangingEventHandler(MetadataChanging);
		}

        internal void Unsubscribe()
        {
            this.target.Changing -= new VariableChangingEventHandler(TargetVariableChanging);
            this.Changing -= MetadataChanging;
        }
	}
}

