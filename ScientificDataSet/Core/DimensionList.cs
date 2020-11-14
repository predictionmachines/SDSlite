// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Microsoft.Research.Science.Data
{
	/// <summary>
	/// List of dimensions.
	/// </summary>
	/// <seealso cref="Dimension"/>
    public class DimensionList : IEnumerable<Dimension>, IList<Dimension>, ICloneable
    {
        private List<Dimension> dims = new List<Dimension>();

		/// <summary>
		/// 
		/// </summary>
        public DimensionList()
        {
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dims"></param>
        public DimensionList(IList<Dimension> dims)
        {
            this.dims.AddRange(dims);
        }

		/// <summary>
		/// Adds a dimension to the list.
		/// </summary>
		/// <param name="dim"></param>
		/// <remarks>
		/// <para>
		/// If the dimension with the same name as the <paramref name="dim"/> is alredy
		/// contained in the list, and their lengths differ, then
		/// the length of the dimension with this name is reset to -1.
		/// No exception is thrown.
		/// </para>
		/// </remarks>
        public void Add(Dimension dim)
        {
            if (dims.Contains(dim))
                return;

            int iFound = -1;
            for (int i = 0; i < dims.Count; i++)
            {
                if (dims[i].Name == dim.Name)
                {
                    iFound = i;
                    break;
                }
            }

            if (iFound >= 0)
            {
                Dimension sameName = dims[iFound];
                if (sameName.Length != dim.Length)
                {
                    dim.Length = -1;
					System.Diagnostics.Debug.WriteLineIf(DataSet.TraceDataSet.TraceWarning, "Constaint FAILED for dimension " + dim.Length);
                    //   throw new Exception("Shape incosistency");
                }
                else
                {
                    return;
                }
            }

            dims.Add(dim);
        }

		/// <summary>
		/// Gets the count of dimensions.
		/// </summary>
        public int Count
        {
            get { return dims.Count; }
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
        public Dimension this[int index]
        {
            get { return dims[index]; }
            set { dims[index] = value; }
        }

		/// <summary>
		/// Gets the dimension by its name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
        public Dimension this[string name]
        {
            get { return dims.Find(d => d.Name == name); }
            set { dims[dims.FindIndex(d => d.Name == name)] = value; }
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
        public int FindIndex(string name)
        {
            int index = dims.FindIndex(dim => dim.Name == name);
            return index;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dims"></param>
        public void AddRange(IList<Dimension> dims)
        {
            for (int i = 0; i < dims.Count; i++)
            {
                Add(dims[i]);
            }
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
        public bool IsSubsetOf(DimensionList list)
        {
            return false;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dim"></param>
		/// <returns></returns>
        public bool Contains(string dim)
        {
            return dims.Exists(d => dim == d.Name);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dim"></param>
		/// <returns></returns>
        public int GetLength(string dim)
        {
            return dims.Find(d => dim == d.Name).Length;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        public string[] AsStringArray()
        {
            string[] arr = new string[dims.Count];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = dims[i].Name;
            }
            return arr;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dim"></param>
		/// <returns></returns>
        public int IndexOf(string dim)
        {
            for (int i = 0; i < dims.Count; i++)
            {
                if (dims[i].Name == dim)
                    return i;
            }
            throw new Exception("Dimension not found");
        }

        #region IList<Dimension> Members

        int IList<Dimension>.IndexOf(Dimension item)
        {
            return dims.IndexOf(item);
        }

        void IList<Dimension>.Insert(int index, Dimension item)
        {
            dims.Insert(index, item);
        }

        void IList<Dimension>.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        Dimension IList<Dimension>.this[int index]
        {
            get
            {
                return dims[index];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection<Dimension> Members

        void ICollection<Dimension>.Add(Dimension item)
        {
            this.Add(item);
        }

        void ICollection<Dimension>.Clear()
        {
            dims.Clear();
        }

        bool ICollection<Dimension>.Contains(Dimension item)
        {
            return this.Contains(item);
        }

        void ICollection<Dimension>.CopyTo(Dimension[] array, int arrayIndex)
        {
            for (int i = 0; i < dims.Count; i++)
            {
                array[i + arrayIndex] = dims[i];
            }
        }

        int ICollection<Dimension>.Count
        {
            get { return this.Count; }
        }

        bool ICollection<Dimension>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<Dimension>.Remove(Dimension item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<Dimension> Members

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        public IEnumerator<Dimension> GetEnumerator()
        {
            return dims.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return dims.GetEnumerator();
        }

        #endregion

		/// <summary>
		/// Represents the list as an array.
		/// </summary>
		/// <returns></returns>
        public Dimension[] ToArray()
        {
            Dimension[] array = new Dimension[dims.Count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = dims[i];
            }
            return array;
        }

		/// <summary>
		/// Makes a copy of the list.
		/// </summary>
		/// <returns></returns>
        public DimensionList Clone()
        {
            DimensionList dlist = new DimensionList(this);
            return dlist;
        }

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion

        #region Object overrided methods

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is IList<Dimension>))
                return false;

            DimensionList dl2 = (DimensionList)this;
            IList<Dimension> dl1 = (IList<Dimension>)obj;

            if (dl1.Count != dl2.Count) return false;

            for (int i = 0; i < dl1.Count; i++)
            {
                Dimension d1 = dl1[i];

                int j = dl2.FindIndex(d1.Name);
                if (j < 0)
                    return false;

                Dimension d2 = dl2[d1.Name];
                if (d1.Length != d2.Length)
                    return false;
            }
            return true;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        public override int GetHashCode()
        {
            int result = 0;
            foreach (Dimension d in this)
                result = result ^ d.Length ^ d.Name.GetHashCode();
            return result;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dl1"></param>
		/// <param name="dl2"></param>
		/// <returns></returns>
        public static bool operator ==(DimensionList dl1, DimensionList dl2)
        {
            return Equals(dl1, dl2);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dl1"></param>
		/// <param name="dl2"></param>
		/// <returns></returns>
        public static bool operator !=(DimensionList dl1, DimensionList dl2)
        {
            return !Equals(dl1, dl2);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("<");

            bool first = true;
            foreach (Dimension dim in dims)
            {
                if (!first)
                    sb.Append(", ");
                first = false;

                sb.Append(dim.ToString());
            }

            sb.Append('>');
            return sb.ToString();
        }

        #endregion
    }

	/// <summary>
	/// Read-only collection of dimensions.
	/// </summary>
    public class ReadOnlyDimensionList : ICollection<Dimension>, IList<Dimension>, IEnumerable<Dimension>, IEnumerable
    {
        private IList<Dimension> dims;

        internal ReadOnlyDimensionList(DimensionList dims)
        {
            this.dims = dims;
        }

        internal ReadOnlyDimensionList(IList<Dimension> dims)
        {
            this.dims = dims;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
        public Dimension this[int index]
        {
            get { return dims[index]; }
        }

		/// <summary>
		/// Gets the dimension with a given <paramref name="name"/>.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="Exception">Dimension not found.</exception>
        public Dimension this[string name]
        {
            get
            {
                for (int i = 0; i < dims.Count; i++)
                {
                    if (dims[i].Name == name)
                        return dims[i];
                }
                throw new Exception("Dimension not found");
            }
            set
            {
                for (int i = 0; i < dims.Count; i++)
                {
                    if (dims[i].Name == name)
                    {
                        dims[i] = value;
                        return;
                    }
                }
                throw new Exception("Dimension not found");
            }
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
        public bool Contains(string name)
        {
            return FindIndex(name) >= 0;
        }

		/// <summary>
		/// Finds a dimension with the given <paramref name="name"/>.
		/// </summary>
		/// <param name="name"></param>
		/// <returns>The index of the dimension in the list if found; otherwise, -1.</returns>
        public int FindIndex(string name)
        {
            for (int i = 0; i < dims.Count; i++)
            {
                if (dims[i].Name == name)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Returns an array of names of dimensions.
        /// </summary>
		/// <remarks></remarks>
        public string[] AsNamesArray()
        {
            string[] names = new string[dims.Count];
            for (int i = 0; i < names.Length; i++)
            {
                names[i] = dims[i].Name;
            }
            return names;
        }

		/// <summary>
		/// Returns the shape as an array of lengths.
		/// </summary>
		/// <returns></returns>
		public int[] AsShape()
		{
			int[] shape = new int[dims.Count];
			for (int i = 0; i < dims.Count; i++)
			{
				shape[i] = dims[i].Length;
			}
			return shape;
		}

        #region Object overrided methods

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is IList<Dimension>))
                return false;

            ReadOnlyDimensionList dl2 = (ReadOnlyDimensionList)this;
            IList<Dimension> dl1 = (IList<Dimension>)obj;

            if (dl1.Count != dl2.Count) return false;

            for (int i = 0; i < dl1.Count; i++)
            {
                Dimension d1 = dl1[i];

                int j = dl2.FindIndex(d1.Name);
                if (j < 0)
                    return false;

                Dimension d2 = dl2[d1.Name];
                if (d1.Length != d2.Length)
                    return false;
            }
            return true;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        public override int GetHashCode()
        {
            int result = 0;
            foreach (Dimension d in this)
                result = result ^ d.Length ^ d.Name.GetHashCode();
            return result;
        }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="dl1"></param>
		/// <param name="dl2"></param>
		/// <returns></returns>
        public static bool operator ==(ReadOnlyDimensionList dl1, ReadOnlyDimensionList dl2)
        {
            return Equals(dl1, dl2);
        }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="dl1"></param>
		/// <param name="dl2"></param>
		/// <returns></returns>
        public static bool operator !=(ReadOnlyDimensionList dl1, ReadOnlyDimensionList dl2)
        {
            return !Equals(dl1, dl2);
        }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="dl1"></param>
		/// <param name="dl2"></param>
		/// <returns></returns>
        public static bool operator ==(DimensionList dl1, ReadOnlyDimensionList dl2)
        {
            return Equals(dl1, dl2);
        }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="dl1"></param>
		/// <param name="dl2"></param>
		/// <returns></returns>
        public static bool operator !=(DimensionList dl1, ReadOnlyDimensionList dl2)
        {
            return !Equals(dl1, dl2);
        }
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("<");

            bool first = true;
            foreach (Dimension dim in dims)
            {
                if (!first)
                    sb.Append(", ");
                first = false;

                sb.Append(dim.ToString());
            }

            sb.Append('>');
            return sb.ToString();
        }

        #endregion

        #region Collection interfaces

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dims.GetEnumerator();
        }

        #endregion

        #region IEnumerable<Dimension> Members

        IEnumerator<Dimension> IEnumerable<Dimension>.GetEnumerator()
        {
            return dims.GetEnumerator();
        }

        #endregion

        #region ICollection<Dimension> Members

        void ICollection<Dimension>.Add(Dimension item)
        {
            throw new NotImplementedException();
        }

        void ICollection<Dimension>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<Dimension>.Contains(Dimension item)
        {
            throw new NotImplementedException();
        }

        void ICollection<Dimension>.CopyTo(Dimension[] array, int arrayIndex)
        {
            for (int i = 0; i < dims.Count; i++)
            {
                array[arrayIndex + i] = dims[i];
            }
        }

		/// <summary>
		/// Gets the number of dimensions.
		/// </summary>
        public int Count
        {
            get { return dims.Count; }
        }

        bool ICollection<Dimension>.IsReadOnly
        {
            get { return true; }
        }

        bool ICollection<Dimension>.Remove(Dimension item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IList<Dimension> Members

        int IList<Dimension>.IndexOf(Dimension item)
        {
            throw new NotImplementedException();
        }

        void IList<Dimension>.Insert(int index, Dimension item)
        {
            throw new NotImplementedException();
        }

        void IList<Dimension>.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        Dimension IList<Dimension>.this[int index]
        {
            get
            {
                return dims[index];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection<Dimension> Members


        int ICollection<Dimension>.Count
        {
            get { return dims.Count; }
        }

        #endregion

        #endregion
    }
}

