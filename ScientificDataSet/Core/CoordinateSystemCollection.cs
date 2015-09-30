// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Microsoft.Research.Science.Data
{
    /// <summary>Provides abstract base class for CoordinateSystem collections.</summary>
	public abstract class CoordinateSystemCollectionBase : ICollection<CoordinateSystem>
	{
		internal CoordinateSystemCollectionBase()
		{
		}

		/// <summary>
		/// Gets the committed coordinate system with the given index.
		/// </summary>
		/// <param name="index">Zero-based index of the coordinate system to get.</param>
		/// <returns>The coordinate system.</returns>
		/// <exception cref="IndexOutOfRangeException">The exception is thrown if 
		/// the index is out of range.</exception>
		public CoordinateSystem this[int index]
		{
			get
			{
				if (Collection is IList<CoordinateSystem>)
					return ((IList<CoordinateSystem>)Collection)[index];

				foreach (CoordinateSystem cs in Collection)
				{
					if (index-- == 0) return cs;
				}
				throw new IndexOutOfRangeException();
			}
		}

		/// <summary>
		/// Gets the committed coordinate system with the given name.
		/// </summary>
		/// <param name="name">Name of the coordinate system to get.</param>
		/// <returns>The coordinate system.</returns>
		/// <exception cref="ValueNotFoundException">The exception is thrown if 
		/// the coordinate system is not found.</exception>
		public CoordinateSystem this[string name]
		{
			get
			{
				return this[name, SchemaVersion.Committed];
			}
		}

		/// <summary>
		/// Gets the coordinate system with the given name from the specified schema version.
		/// </summary>
		/// <param name="name">Name of the coordinate system to get.</param>
		/// <param name="version">Version of the schema to look in.</param>
		/// <returns>The coordinate system.</returns>
		/// <exception cref="ValueNotFoundException">The exception is thrown if 
		/// the coordinate system is not found.</exception>
		public CoordinateSystem this[string name, SchemaVersion version]
		{
			get
			{
				if (version == SchemaVersion.Committed)
				{
					foreach (CoordinateSystem cs in Collection)
					{
						if (cs.Name == name && !cs.HasChanges) return cs;
					}
					throw new ValueNotFoundException("There is no requested coordinate system.");
				}
				if (version == SchemaVersion.Proposed)
				{
					foreach (CoordinateSystem cs in Collection)
					{
						if (cs.Name == name && cs.HasChanges) return cs;
					}
					throw new ValueNotFoundException("There is no requested coordinate system.");
				}
				//if (version == SchemaVersion.Recent)
				foreach (CoordinateSystem cs in Collection)
				{
					if (cs.Name == name) return cs;
				}
				throw new ValueNotFoundException("There is no requested coordinate system.");
			}
		}

        /// <summary>Gets count of coordinate systems in the collection.</summary>
		public int Count
		{
			get { return Collection.Count; }
		}

        /// <summary>Checks whether collection contains specified coordinate system</summary>
        /// <param name="cs">Coordinate system to locate in collection</param>
        /// <returns>True, if coordinate system is in the collection or false otherwise</returns>
		public bool Contains(CoordinateSystem cs)
		{
			return Collection.Contains(cs);
		}

        /// <summary>Checks whether collection contains coordinate system with specified name</summary>
        /// <param name="name">Name of coordinate system to locate in collection</param>
        /// <returns>True, if coordinate system is in the collection or false otherwise</returns>
        public bool Contains(string name)
		{
			return Contains(name, SchemaVersion.Committed);
		}

        /// <summary>Checks whether specified version of collection contains coordinate system with specified name</summary>
        /// <param name="name">Name of coordinate system to locate in collection</param>
        /// <param name="version">Version of collection to look in</param>
        /// <returns>True, if coordinate system is in the collection or false otherwise</returns>
        public bool Contains(string name, SchemaVersion version)
		{
			if (version == SchemaVersion.Committed)
			{
				foreach (CoordinateSystem cs in Collection)
				{
					if (cs.Name == name && !cs.HasChanges)
						return true;
				}
				return false;
			}
			if (version == SchemaVersion.Proposed)
			{
				foreach (CoordinateSystem cs in Collection)
				{
					if (cs.Name == name && cs.HasChanges) 
						return true;
				}
				return false;
			}
			if (version == SchemaVersion.Recent)
				foreach (CoordinateSystem cs in Collection)
				{
					if (cs.Name == name) 
						return true;
				}
			return false;
		}

		/// <summary>
		/// Provides an access to internal collection.
		/// </summary>
		protected abstract ICollection<CoordinateSystem> Collection { get; }

        /// <summary>Gets a value indicating whether the this collection is read-only.</summary>
		protected abstract bool IsReadOnly { get; }

        /// <summary>Returns string with brief information about all coordinate systems in collection</summary>
        /// <returns>A String that represents the current collection.</returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder("( ");
			foreach (CoordinateSystem cs in Collection)
			{
				sb.Append(cs.Name);
				sb.Append(' ');
			}
			sb.Append(')');
			return sb.ToString();
		}

		#region ICollection<CoordinateSystem> Members

		void ICollection<CoordinateSystem>.Add(CoordinateSystem item)
		{
			if (IsReadOnly)
				throw new NotSupportedException("Collection is read only");
			Collection.Add(item);
		}

		void ICollection<CoordinateSystem>.Clear()
		{
			if (IsReadOnly)
				throw new NotSupportedException("Collection is read only");
			Collection.Clear();
		}

		bool ICollection<CoordinateSystem>.Contains(CoordinateSystem item)
		{
			return Contains(item);
		}

		void ICollection<CoordinateSystem>.CopyTo(CoordinateSystem[] array, int arrayIndex)
		{
			Collection.CopyTo(array, arrayIndex);
		}

		int ICollection<CoordinateSystem>.Count
		{
			get { return Collection.Count; }
		}

		bool ICollection<CoordinateSystem>.IsReadOnly
		{
			get { return IsReadOnly; }
		}

		bool ICollection<CoordinateSystem>.Remove(CoordinateSystem item)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IEnumerable<CoordinateSystem> Members

		IEnumerator<CoordinateSystem> IEnumerable<CoordinateSystem>.GetEnumerator()
		{
			return Collection.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return Collection.GetEnumerator();
		}

		#endregion
	}

	/// <summary>
	/// Represents a read-only coordinate system collection with supporting routines.
	/// </summary>
    [Obsolete("Coordinate systems will be removed from the future release. Use metadata attributes instead.")]
	public class ReadOnlyCoordinateSystemCollection : CoordinateSystemCollectionBase
	{
		private ICollection<CoordinateSystem> list;

		internal ReadOnlyCoordinateSystemCollection(ICollection<CoordinateSystem> vc)
		{
			list = vc;
		}

		internal ReadOnlyCoordinateSystemCollection()
		{
			list = new Collection<CoordinateSystem>();
		}
		
		/// <summary>
		/// Gets the access to the internal collection.
		/// </summary>
		protected override ICollection<CoordinateSystem> Collection
		{
			get { return list; }
		}

        /// <summary>Gets a value indicating whether the this collection is read-only.</summary>
        /// <remarks>Always returns true</remarks>
		protected override bool IsReadOnly
		{
			get { return true; }
		}

		internal static ReadOnlyCoordinateSystemCollection Combine(ICollection<CoordinateSystem> c1, ICollection<CoordinateSystem> c2)
		{
			return new ReadOnlyCoordinateSystemCollection(
				new ReadOnlyCollectionCombination<CoordinateSystem>(c1, c2));
		}
	}

	/// <summary>
	/// Represents a dynamic coordinate system collection with supporting routines.
	/// </summary>
	public class CoordinateSystemCollection : CoordinateSystemCollectionBase
	{
		private Collection<CoordinateSystem> list;

        /// <summary>Initializes an instance of CoordinateSystemCollection with no elements</summary>
		public CoordinateSystemCollection()
		{
			list = new Collection<CoordinateSystem>();
		}

        /// <summary>Initializes an instance of CoordinateSystemCollection with contents taken from 
        /// specified collection</summary>
        /// <param name="collection">Collection to copy items from</param>
        public CoordinateSystemCollection(CoordinateSystemCollectionBase collection)
		{
			list = new Collection<CoordinateSystem>();
			foreach (CoordinateSystem cs in collection)
			{
				list.Add(cs);
			}
		}

		/// <summary>
		/// Wraps the collection into read-only collection and returns it.
		/// </summary>
		/// <returns></returns>
		public ReadOnlyCoordinateSystemCollection GetReadOnlyCollection()
		{
			ReadOnlyCoordinateSystemCollection roVars = new ReadOnlyCoordinateSystemCollection(list);
			return roVars;
		}

		/// <summary>
		/// Adds the coordinate system to the collection.
		/// </summary>
		/// <param name="cs"></param>
		/// <exception cref="Exception">If the collection already contains coordinate system with the same name.</exception>
		public void Add(CoordinateSystem cs)
		{
			foreach (CoordinateSystem item in list)
			{
				if (item == cs) return;
				if (item.Name == cs.Name)
					throw new Exception("Duplicate coordinate systems");
			}
			list.Add(cs);
		}

		/// <summary>
		/// Adds a range of coordinate systems to the collection.
		/// </summary>
		/// <param name="coordinateSystemCollection"></param>
		/// <exception cref="Exception">If the collection already contains coordinate system with the same name.</exception>
		internal void AddRange(CoordinateSystemCollection coordinateSystemCollection)
		{
			foreach (var item in coordinateSystemCollection)
			{
				Add(item);
			}
		}

		/// <summary>
		/// Provides direct access to the inner collection.
		/// </summary>
		protected override ICollection<CoordinateSystem> Collection
		{
			get { return list; }
		}

		/// <summary>
		/// Gets the value indicating whether the collection is read only.
		/// </summary>
		protected override bool IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		/// Returns an array of strings with names of coordinate systems.
		/// </summary>
		/// <returns>Array of names of coordinate systems.</returns>
		public string[] AsNameArray()
		{
			string[] names = new string[list.Count];
			for (int i = 0; i < names.Length; i++)
			{
				names[i] = list[i].Name;
			}
			return names;
		}

		/// <summary>
		/// Represents the collection as an array.
		/// </summary>
		/// <returns>Array of coordinate systems.</returns>
		public CoordinateSystem[] AsArray()
		{
			CoordinateSystem[] cs = new CoordinateSystem[list.Count];
			for (int i = 0; i < cs.Length; i++)
			{
				cs[i] = list[i];
			}
			return cs;
		}

		/// <summary>
		/// Removes all coordinate systems from the collection.
		/// </summary>
		public void Clear()
		{
			list.Clear();
		}
	}
}

