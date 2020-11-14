// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data
{
	/// <summary>
	/// Enables smooth iteration through two collections as a single one.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class ReadOnlyCollectionCombination<T> : ICollection<T>
	{
		private ICollection<T> c1;
		private ICollection<T> c2;

		public ReadOnlyCollectionCombination(ICollection<T> c1, ICollection<T> c2)
		{
			this.c1 = c1;
			this.c2 = c2;
		}

		#region ICollection<T> Members

		public void Add(T item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public bool Contains(T item)
		{
			if (c1 != null && c1.Contains(item))
				return true;
			if (c2 != null && c2.Contains(item))
				return true;

			return false;
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			if (c1 != null)
			{
				c1.CopyTo(array, arrayIndex);
				arrayIndex += c1.Count;
			}
			if (c2 != null)
			{
				c2.CopyTo(array, arrayIndex);
			}
		}

		public int Count
		{
			get
			{
				int n = 0;
				if (c1 != null) n += c1.Count;
				if (c2 != null) n += c2.Count;
				return n;
			}
		}

		public bool IsReadOnly
		{
			get { return true; }
		}

		public bool Remove(T item)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region IEnumerable<T> Members

		public IEnumerator<T> GetEnumerator()
		{
			return new Enumerator(this);
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return new Enumerator(this);
		}

		#endregion

		private class Enumerator : IEnumerator<T>
		{
			private ReadOnlyCollectionCombination<T> c;
			private ICollection<T> currentCollection = null;
			private IEnumerator<T> enumerator = null;
			private bool finish = false;

			internal Enumerator(ReadOnlyCollectionCombination<T> c)
			{
				this.c = c;
			}

			#region IEnumerator<T> Members

			public T Current
			{
				get
				{
					if (finish || enumerator == null)
						throw new Exception("Enumeration finished");
					return enumerator.Current;
				}
			}

			#endregion

			#region IDisposable Members

			public void Dispose()
			{
				if (enumerator != null)
					enumerator.Dispose();
			}

			#endregion

			#region IEnumerator Members

			object System.Collections.IEnumerator.Current
			{
				get { throw new NotImplementedException(); }
			}

			public bool MoveNext()
			{
				if (finish) return false;

				if (currentCollection == null)
				{
					currentCollection = c.c1;
					if (currentCollection == null)
					{
						currentCollection = c.c2;
					}
					if (currentCollection == null)
					{
						finish = true;
						return false;
					}

					enumerator = currentCollection.GetEnumerator();
				}

				if (enumerator != null)
				{
					bool res = enumerator.MoveNext();
					if (!res && currentCollection == c.c1)
					{
						currentCollection = c.c2;
						if (currentCollection == null)
						{
							finish = true;
							return false;
						}
						if (enumerator != null)
							enumerator.Dispose();
						enumerator = currentCollection.GetEnumerator();
						return MoveNext();
					}
					if (res == false) finish = true;
					return res;
				}
				else throw new Exception("Enumerator is null");
			}

			public void Reset()
			{
				throw new NotSupportedException();
			}

			#endregion
		}
	}

	internal class ReadOnlyEnumerableCombination<T> : IEnumerable<T>
	{
		private IEnumerable<T> c1;
		private IEnumerable<T> c2;

		public ReadOnlyEnumerableCombination(IEnumerable<T> c1, IEnumerable<T> c2)
		{
			this.c1 = c1;
			this.c2 = c2;
		}

		#region IEnumerable<T> Members

		public IEnumerator<T> GetEnumerator()
		{
			return new Enumerator(this);
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return new Enumerator(this);
		}

		#endregion

		private class Enumerator : IEnumerator<T>
		{
			private ReadOnlyEnumerableCombination<T> c;
			private IEnumerable<T> currentCollection = null;
			private IEnumerator<T> enumerator = null;
			private bool finish = false;

			internal Enumerator(ReadOnlyEnumerableCombination<T> c)
			{
				this.c = c;
			}

			#region IEnumerator<T> Members

			public T Current
			{
				get
				{
					if (finish || enumerator == null)
						throw new Exception("Enumeration finished");
					return enumerator.Current;
				}
			}

			#endregion

			#region IDisposable Members

			public void Dispose()
			{
				if (enumerator != null)
					enumerator.Dispose();
			}

			#endregion

			#region IEnumerator Members

			object System.Collections.IEnumerator.Current
			{
				get { throw new NotImplementedException(); }
			}

			public bool MoveNext()
			{
				if (finish) return false;

				if (currentCollection == null)
				{
					currentCollection = c.c1;
					if (currentCollection == null)
					{
						currentCollection = c.c2;
					}
					if (currentCollection == null)
					{
						finish = true;
						return false;
					}

					enumerator = currentCollection.GetEnumerator();
				}

				if (enumerator != null)
				{
					bool res = enumerator.MoveNext();
					if (!res && currentCollection == c.c1)
					{
						currentCollection = c.c2;
						if (currentCollection == null)
						{
							finish = true;
							return false;
						}
						if (enumerator != null)
							enumerator.Dispose();
						enumerator = currentCollection.GetEnumerator();
						return MoveNext();
					}
					if (res == false) finish = true;
					return res;
				}
				else throw new Exception("Enumerator is null");
			}

			public void Reset()
			{
				throw new NotSupportedException();
			}

			#endregion
		}
	}
}

