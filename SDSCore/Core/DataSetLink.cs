// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data
{
	internal class DataSetLink
	{
		public bool Committed = false;
		private Variable reference;
		private Variable target;

		public DataSetLink(Variable reference, Variable target)
		{
			if (reference == null)
				throw new ArgumentNullException("reference");
			if (target == null)
				throw new ArgumentNullException("target");
			this.reference = reference;
			this.target = target;
		}

		public Variable Reference { get { return reference; } }

		public Variable Target { get { return target; } }

		public DataSet SourceDataSet { get { return Reference.DataSet; } }

		public DataSet TargetDataSet { get { return Target.DataSet; } }

		/// <summary>
		/// Gets the value indicating that this reference has changes.
		/// </summary>
		public bool IsActive { get { return Reference.HasChanges || Target.HasChanges; } }

		public override string ToString()
		{
			return String.Format("{0} refers {1}", Reference, Target);
		}

		public override bool Equals(object obj)
		{
			DataSetLink l = obj as DataSetLink;
			if (object.Equals(l, null)) return false;

			return Reference == l.Reference && Target == l.Target;
		}

		public static bool operator==(DataSetLink l1, DataSetLink l2)
		{
			if (object.Equals(l1, null)) return object.Equals(l2, null);
			return l1.Equals(l2);
		}

		public static bool operator !=(DataSetLink l1, DataSetLink l2)
		{
			if (object.Equals(l1, null)) return !object.Equals(l2, null);
			return !l1.Equals(l2);
		}

		public override int GetHashCode()
		{
			return Reference.GetHashCode() ^ Target.GetHashCode();
		}
	}

	internal class DataSetLinkCollection : IEnumerable<DataSetLink>
	{
		private List<DataSetLink> links = new List<DataSetLink>();

		public DataSetLinkCollection()
		{
		}

		public DataSetLink AddLink(Variable reference, Variable target)
		{
			foreach (var item in links)
			{
				if (item.Reference == reference && item.Target == target) return item;
			}
			var link = new DataSetLink(reference, target);
			links.Add(link);
			return link;
		}

		public void RemoveLinks(DataSet srcDataSet)
		{
			for (int i = links.Count; --i >= 0; )
			{
				if (links[i].SourceDataSet == srcDataSet)
					links.RemoveAt(i);
			}
		}

		public void RemoveLink(DataSetLink link)
		{
			links.Remove(link);
		}

		public int Count { get { return links.Count; } }

		public void CommitLinks()
		{
			foreach (var item in links)
			{
				item.Committed = true;
			}
		}

		public void Rollback()
		{
			for (int i = links.Count; --i >= 0; )
			{
				if (!links[i].Committed)
					links.RemoveAt(i);
			}
		}

		public override string ToString()
		{
			return links.Count.ToString() + " links";
		}

		#region IEnumerable<DataSetLink> Members

		public IEnumerator<DataSetLink> GetEnumerator()
		{
			return links.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return links.GetEnumerator();
		}

		#endregion

	}
}

