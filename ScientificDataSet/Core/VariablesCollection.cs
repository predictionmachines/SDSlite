// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Microsoft.Research.Science.Data
{
	/// <summary>
	/// Represents a dynamic variable collection with notifications.
	/// </summary>
	public class VariableCollection : ICollection<Variable>
	{
		private List<Variable> variables = new List<Variable>();

		/// <summary>
		/// Fires when the collection is changed.
		/// </summary>
		internal event VariableCollectionChangedEventHandler CollectionChanged;

		/// <summary>
		/// 
		/// </summary>
		public VariableCollection()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="collection"></param>
		public VariableCollection(IEnumerable<Variable> collection)
		{
			variables.AddRange(collection);
		}

		/// <summary>
		/// Gets the number of variables in the collection.
		/// </summary>
		public int Count
		{
			get { return variables.Count; }
		}

		/// <summary>
		/// Gets the variable by its zero-based index.        
		/// </summary>
		/// <remarks>
		/// An exception is thrown if the variable is not found.
		/// </remarks>
		/// <seealso cref="VariableCollection.TryGetById"/>     
		public Variable this[int index]
		{
			get
			{
				return variables[index];
			}
		}

		/// <summary>
		/// Gets the variable by its ID.        
		/// </summary>
		/// <remarks>
		/// An exception is thrown if the variable is not found.
		/// </remarks>
		/// <seealso cref="VariableCollection.TryGetById"/> 
		public Variable GetByID(int id)
		{
			try
			{
				return variables.First(v => v.ID == id);
			}
			catch (Exception)
			{
				throw new KeyNotFoundException("Variable with id " + id + " not found.");
			}
		}

		/// <summary>
		/// Gets the variable with specified committed name.
		/// </summary>
		public Variable this[string name]
		{
			get
			{
				return this[name, SchemaVersion.Committed];
			}
		}

		/// <summary>
		/// Gets the variable with specified name for specified version of schema.
		/// </summary>
		public Variable this[string name, SchemaVersion schema]
		{
			get
			{
				try
				{
					if (schema == SchemaVersion.Committed)
						return variables.First(v => v.Name == name);

					if (schema == SchemaVersion.Proposed)
						return variables.First(v =>
						{
							if (v.HasChanges)
							{
								return v.ProposedName == name;
							}
							return false;
						});

					if (schema == SchemaVersion.Recent)
						return variables.First(v =>
						{
							if (v.HasChanges)
							{
								return v.ProposedName == name;
							}
							else
							{
								return v.Name == name;
							}
						});

					throw new Exception("Unexpected schema version");
				}
				catch (Exception ex)
				{
					throw new KeyNotFoundException("Variable " + name + " not found for " + schema + " schema version.", ex);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool Contains(string name)
		{
			foreach (Variable v in variables)
			{
				if (v.Name == name) return true;
			}
			return false;
		}

		/// <summary>
		/// Returns the value, indicating whether the collection contains specified variable
		/// (references are included to search).
		/// </summary>
		public bool Contains(Variable variable)
		{
			Variable v1, v2;
			foreach (Variable v in variables)
			{
				if (v == variable) return true;

				v1 = (v is IRefVariable) ? ((IRefVariable)v).ReferencedVariable : v;
				v2 = (variable is IRefVariable) ? ((IRefVariable)variable).ReferencedVariable : variable;
				if (v1 == v2) return true;
			}
			return false;
		}


		/// <summary>
		/// Gets the variable by its ID.
		/// </summary>
		public bool TryGetById(int id, out Variable variable)
		{
			try
			{
				variable = variables.FirstOrDefault(v => v.ID == id);
				return variable != null;
			}
			catch (Exception)
			{
				variable = null;
				return false;
			}
		}

		/// <summary>
		/// Adds the variable to the beggining of the collection.
		/// </summary>
		/// <param name="var"></param>
		/// <remarks>
		/// Adds the variable to the beginning of the collection so it is stored in the inverse order.
		/// </remarks>
		public void InsertFirst(Variable var)
		{
			variables.Insert(0, var);
			if (CollectionChanged != null)
				CollectionChanged(this, new VariableCollectionChangedEventArgs(VariableCollectionChangedAction.Add, var));
		}

		/// <summary>
		/// Adds the variable to the collection.
		/// </summary>
		/// <param name="var"></param>
		/// <remarks>
		/// Adds the variable to the collection.
		/// </remarks>
		public void Add(Variable var)
		{
			variables.Add(var);
			if (CollectionChanged != null)
				CollectionChanged(this, new VariableCollectionChangedEventArgs(VariableCollectionChangedAction.Add, var));
		}

		/// <summary>
		/// Removes all the elements from the collection.
		/// </summary>
		public void Clear()
		{
			variables.Clear();
			if (CollectionChanged != null)
				CollectionChanged(this, new VariableCollectionChangedEventArgs(VariableCollectionChangedAction.Clear, null));
		}

		/// <summary>
		/// Makes the read only copy of the collection.
		/// </summary>
		/// <returns></returns>
		public ReadOnlyVariableCollection GetReadOnlyCollection()
		{
			ReadOnlyVariableCollection readOnly = new ReadOnlyVariableCollection(variables);
			return readOnly;
		}

		#region IEnumerable<Variable> Members
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public IEnumerator<Variable> GetEnumerator()
		{
			return variables.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return variables.GetEnumerator();
		}

		#endregion

		#region ICollection<Variable> Members

		/// <summary>
		/// Copies the elements of the ICollection to an Array, starting at a particular Array index.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="arrayIndex"></param>
		public void CopyTo(Variable[] array, int arrayIndex)
		{
			variables.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Returns false.
		/// </summary>
		public bool IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		/// This implementation always throws NotSupportedException.
		/// </summary>
		/// <param name="item"></param>
		public bool Remove(Variable item)
		{
			throw new NotSupportedException();
		}

		#endregion
	}


	internal delegate void VariableCollectionChangedEventHandler(object sender, VariableCollectionChangedEventArgs e);

	internal class VariableCollectionChangedEventArgs : EventArgs
	{
		private VariableCollectionChangedAction action;
		private Variable var;

		public VariableCollectionChangedEventArgs(VariableCollectionChangedAction action, Variable addedVar)
		{
			this.action = action;
			this.var = addedVar;
		}

		/// <summary>
		/// Gets the action that caused the event.
		/// </summary>
		public VariableCollectionChangedAction Action { get { return action; } }

		/// <summary>
		/// Gets the added variable.
		/// </summary>
		public Variable AddedVariable { get { return var; } }
	}

	/// <summary>
	/// Determines the type of changes.
	/// </summary>
	public enum VariableCollectionChangedAction
	{
		/// <summary>
		/// All variables removed from the collection.
		/// </summary>
		Clear,
		/// <summary>
		/// A variable is added to the collection.
		/// </summary>
		Add
	}

	/// <summary>
	/// Represents a read-only variable collection with supporting routines.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The collection hides some variables from a user. These are: a variable to
	/// store global metadata (<see cref="DataSet.Metadata"/>) and hidden variables.
	/// To get all variables, use <see cref="All"/> property.
	/// Also note, the <see cref="DataSet.GetSchema()"/> methods return a schema
	/// containing description of all variables, including hidden and a global metadata variable.
	/// </para>
	/// </remarks>
	public class ReadOnlyVariableCollection : ICollection<Variable>
	{
		private Variable[] variables;
		private int indexOfHidden = -1;

		internal ReadOnlyVariableCollection()
		{
			variables = new Variable[0];
		}

		internal ReadOnlyVariableCollection(IEnumerable<Variable> vars)
		{
			List<Variable> list = new List<Variable>();
			int i = 0;
			foreach (var item in vars)
			{
				list.Add(item);
				if (item.ID == DataSet.GlobalMetadataVariableID)
					indexOfHidden = i;
				i++;
			}
			variables = list.ToArray();
		}

		internal ReadOnlyVariableCollection(List<Variable> vars)
		{
			variables = new Variable[vars.Count];
			int i = 0;
			foreach (var item in vars)
			{
				if (item.ID == DataSet.GlobalMetadataVariableID)
					indexOfHidden = i;
				variables[i++] = item;
			}
		}

		internal ReadOnlyVariableCollection(VariableCollection vars)
		{
			variables = new Variable[vars.Count];
			int i = 0;
			foreach (var item in vars)
			{
				if (item.ID == DataSet.GlobalMetadataVariableID)
					indexOfHidden = i;
				variables[i++] = item;
			}
		}

		/// <summary>
		/// Gets the number of variables in the collection.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This number doesn't include variable containing global metadata.
		/// </para>
		/// </remarks>
		public int Count
		{
			get
			{
				return (indexOfHidden >= 0) ? variables.Length - 1 : variables.Length;
			}
		}

		/// <summary>
		/// Returns the array of variables.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// <para>
		/// A variable containing global metadata is excluded from the array.
		/// </para>
		/// </remarks>
		public Variable[] ToArray()
		{
			if (indexOfHidden < 0)
				return (Variable[])variables.Clone();

			int count = variables.Length;
			Variable[] a = new Variable[count - 1];
			for (int i = 0, j = 0; i < count; i++)
			{
				if (i != indexOfHidden)
					a[j++] = variables[i];
			}
			return a;
		}

		/// <summary>
		/// Gets the variable with specified committed name.
		/// </summary>
		public Variable this[string name]
		{
			get
			{
				return this[name, SchemaVersion.Committed];
			}
		}

		/// <summary>
		/// Gets the variable by its zero-based index.
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <seealso cref="ReadOnlyVariableCollection.TryGetByID"/>  
		public Variable this[int index]
		{
			get
			{
				if (indexOfHidden < 0 || index < indexOfHidden)
					return variables[index];
				return variables[index + 1];
			}
		}

		/// <summary>
		/// Gets the variable by its ID.
		/// </summary>
		/// <remarks>
		/// An exception is thrown if not found.
		/// </remarks>
		/// <seealso cref="ReadOnlyVariableCollection.TryGetByID"/>  
		/// <exception cref="KeyNotFoundException">Variable not found.</exception>
		public Variable GetByID(int id)
		{
			try
			{
				return variables.First(v => v.ID == id);
			}
			catch (Exception)
			{
				throw new Exception("Variable with id " + id + " not found.");
			}
		}

		/// <summary>
		/// Tries to get the variable by its ID.
		/// </summary>
		public bool TryGetByID(int id, out Variable variable)
		{
			try
			{
				variable = variables.FirstOrDefault(v => v.ID == id);
				return variable != null;
			}
			catch (Exception)
			{
				variable = null;
				return false;
			}
		}

		/// <summary>
		/// Gets the variable with specified name for specified version of schema.
		/// </summary>
		public Variable this[string name, SchemaVersion schema]
		{
			get
			{
				try
				{
					if (schema == SchemaVersion.Committed)
						return variables.First(v => v.ID != DataSet.GlobalMetadataVariableID && v.Name == name);

					if (schema == SchemaVersion.Proposed)
						return variables.First(v =>
						{
							if (v.ID != DataSet.GlobalMetadataVariableID && v.HasChanges)
							{
								return v.ProposedName == name;
							}
							return false;
						});

					if (schema == SchemaVersion.Recent)
						return variables.First(v =>
						{
							if (v.ID == DataSet.GlobalMetadataVariableID) return false;
							if (v.HasChanges)
							{
								return v.ProposedName == name;
							}
							else
							{
								return v.Name == name;
							}
						});

					throw new Exception("Unexpected schema version");
				}
				catch (Exception ex)
				{
					throw new KeyNotFoundException("Variable " + name + " not found for " + schema + " schema version.", ex);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool Contains(string name)
		{
			foreach (Variable v in this)
			{
				if (v.ID != DataSet.GlobalMetadataVariableID && v.Name == name) return true;
			}
			return false;
		}

		/// <summary>
		/// Returns the value, indicating whether the collection contains specified variable
		/// (references are included to search).
		/// </summary>
		public bool Contains(Variable variable)
		{
			Variable v1, v2;
			foreach (Variable v in this)
			{
				if (v == variable) return true;

				v1 = (v is IRefVariable) ? ((IRefVariable)v).ReferencedVariable : v;
				v2 = (variable is IRefVariable) ? ((IRefVariable)variable).ReferencedVariable : variable;
				if (v1 == v2) return true;
			}
			return false;
		}

		/// <summary>
		/// Iterates through the collection of committed variables.
		/// </summary>
		internal IEnumerable<Variable> Committed
		{
			get
			{
				if (Count == 0)
					yield break;

				DataSetSchema sdsSch = this[0].DataSet.GetSchema(SchemaVersion.Committed);
				foreach (VariableSchema vs in sdsSch.Variables)
				{
					yield return this.GetByID(vs.ID);
				}
			}
		}

		#region IEnumerable<Variable> Members

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Doesn't enumerate global metadata variable.
		/// </remarks>
		public IEnumerator<Variable> GetEnumerator()
		{
			return new ArrayEnumerator(variables, false);
		}

		/// <summary>
		/// Enumerates all variables of the collection.
		/// </summary>
		/// <remarks>
		/// The enumeration includes global metadata variable and hidden variables.
		/// </remarks>
		public IEnumerable<Variable> All
		{
			get
			{
				for (int i = 0; i < variables.Length; i++)
				{
					yield return variables[i];
				}
				yield break;
			}
		}

		private class ArrayEnumerator : IEnumerator<Variable>
		{
			Variable[] vars;
			int pos;
			bool enumerateAll;

			public ArrayEnumerator(Variable[] vars, bool enumerateAll)
			{
				this.vars = vars;
				pos = -1;
				this.enumerateAll = enumerateAll;
			}

			#region IEnumerator<Variable> Members

			public Variable Current
			{
				get
				{
					try
					{
						return vars[pos];
					}
					catch (IndexOutOfRangeException)
					{
						throw new InvalidOperationException();
					}
				}
			}

			#endregion

			#region IDisposable Members

			public void Dispose()
			{
			}

			#endregion

			#region IEnumerator Members

			object System.Collections.IEnumerator.Current
			{
				get
				{
					try
					{
						return vars[pos];
					}
					catch (IndexOutOfRangeException)
					{
						throw new InvalidOperationException();
					}
				}
			}

			public bool MoveNext()
			{
				if (enumerateAll)
					pos++;
				else
					do
					{
						pos++;
					} while (pos < vars.Length && vars[pos].ID == DataSet.GlobalMetadataVariableID);
				return pos < vars.Length;
			}

			public void Reset()
			{
				pos = -1;
			}

			#endregion
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return new ArrayEnumerator(variables, false);
		}

		#endregion

		#region ICollection<Variable> Members

		/// <summary>
		/// This implementation always throws NotSupportedException.
		/// </summary>
		/// <param name="item"></param>
		public void Add(Variable item)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// This implementation always throws NotSupportedException.
		/// </summary>
		public void Clear()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Copies all variables to the <paramref name="array"/> starting at the <paramref name="arrayIndex"/> .
		/// </summary>
		/// <param name="array"></param>
		/// <param name="arrayIndex"></param>
		/// <remarks>
		/// The variable containing global metadata is not copied.
		/// </remarks>
		public void CopyTo(Variable[] array, int arrayIndex)
		{
			if (indexOfHidden < 0)
				variables.CopyTo(array, arrayIndex);
			else
			{
				int count = variables.Length;
				for (int i = arrayIndex, j = 0; j < count; j++)
				{
					if (j == indexOfHidden)
						continue;
					array[i++] = variables[j];
				}
			}
		}

		/// <summary>
		/// Always returns true.
		/// </summary>
		public bool IsReadOnly
		{
			get { return true; }
		}

		/// <summary>
		/// This implementation always throws NotSupportedException.
		/// </summary>
		/// <param name="item"></param>
		public bool Remove(Variable item)
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}

