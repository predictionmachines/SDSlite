// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Research.Science.Data
{
	/// <summary>
	/// Represents a dictionary of attributes attached to a variable.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Variable metadata is a dictionary of keys and values.
	/// Key is always a string with length limited by 256 characters.
	/// Metadata key cannot be null or an empty string.
	/// It cannot contain following Unicode symbols: <c>'/'</c>, <c>'0x00'-'0x1F'</c>, <c>'0x7F'-'0xFF'</c>.
	/// Value is an instance of any of supported type, listed in the table below: 
	/// <list type="bullet">
	/// <item><description>all supported data types (listed in the remarks for <see cref="Variable"/>);</description></item>
	/// <item><description>one-dimensional array of supported data types.</description></item>
	/// </list>
	/// </para>   
	/// <para>
	/// There are two predefined keys to be used for a variable name (<see cref="MetadataDictionary.KeyForName"/>)
	/// and a missing value attribute (<see cref="MetadataDictionary.KeyForMissingValue"/>). 
	/// The <see cref="Variable"/> class has appropriate properties to get an access to these values.
	/// </para>
	/// </remarks>
	/// <seealso cref="Variable"/>
	/// <seealso cref="Variable.Name"/>
	/// <seealso cref="Variable.MissingValue"/>
	public class MetadataDictionary : IEnumerable<KeyValuePair<string, object>>
	{
		/// <summary>Key for the Name entry.</summary>
		public readonly string KeyForName = "Name";
		/// <summary>Key for the MissingValue entry.</summary>
		public readonly string KeyForMissingValue = "MissingValue";

        /// <summary>Committed metadata attributes.</summary>
		protected Dictionary<string, object> dictionary;
        /// <summary>Modified attributes (added or changed). May be null.</summary>
		protected Dictionary<string, object> modified;
		private bool readOnly = false;

		/// <summary>
		/// Occurs after an entry has been added or changed.
		/// </summary>
		internal event VariableMetadataChangedEventHandler Changed;
		/// <summary>
		/// Occurs before an entry is about to be added or changed.
		/// </summary>
		internal event VariableMetadataChangingEventHandler Changing;

		/// <summary>
		/// Creates an instance of the MetadataDictionary class.
		/// </summary>
		/// <remarks>
		/// The new instance enables both reading and changing.
		/// <para>
		/// To create, consider use factories: Variable.CreateMetadata(),
		/// DataSet.CreateMetadata().</para>
		/// </remarks>
		protected internal MetadataDictionary()
			: this(false)
		{
		}

		/// <summary>
		/// Creates an instance of the MetadataDictionary class.
		/// </summary>
		/// <param name="readOnly">If true, returned collection is read only.</param>
		protected internal MetadataDictionary(bool readOnly)
		{
			this.readOnly = readOnly;
			dictionary = new Dictionary<string, object>();
			modified = null;
		}

		/// <summary>
		/// Creates an instance of the MetadataDictionary class.
		/// </summary>
		/// <param name="readOnly"></param>
		/// <param name="dict"></param>
		protected MetadataDictionary(bool readOnly, Dictionary<string, object> dict)
		{
			if (dict == null)
				throw new ArgumentNullException("dict");
			this.readOnly = readOnly;

			if (readOnly)
			{
				this.dictionary = dict;
				modified = null;
			}
			else
			{
				this.dictionary = new Dictionary<string, object>();
				modified = dict;
			}
		}

		/// <summary>
		/// Gets the value indicating whether the metadata collection is read only or not.
		/// </summary>
		public bool ReadOnly
		{
			get { return readOnly; }
			internal set
			{
				if (HasChanges)
					throw new Exception("Cannot set read only flag to Metadata while it has changes");
				readOnly = true;
			}
		}

		/// <summary>
		/// Gets the value indicating whether the metadata collection has changes or not.
		/// </summary>
		/// <remarks>
		/// If a user changes metadata collection of a variable, the metadata collection,
		/// the variable and the DataSet become modified (i.e. their property HasChanges returns true).
		/// To commit changes, use <see cref="DataSet.Commit"/>; to rollback them, use
		/// <see cref="DataSet.Rollback"/> or <see cref="Variable.Rollback"/>.
		/// </remarks>
		public bool HasChanges
		{
			get { return modified != null; }
		}

		/// <summary>
		/// Gets the number of metadata entries contained in the dictionary.
		/// </summary>
		public int Count
		{
			get
			{
				return dictionary.Count;
			}
		}

		/// <summary>
		/// Gets the metadata value for the given key and schema version.
		/// </summary>
		/// <param name="key">The name of the metadata entry to get.</param>
		/// <param name="version">Version of the DataSet schema.</param>
		/// <returns>Value corresponding to the given key.</returns>
		/// <remarks>
		/// <para>
		/// Type of the key is always <see cref="string"/>.
		/// Type of the property value is <see cref="object"/>, but supported metadata value types 
		/// are constrained. See documentation for <see cref="MetadataDictionary"/> for the
		/// specification.
		/// </para>
		/// <para>
		/// Gets the value of the metadata entry corresponding to the given <paramref name="key"/>.
		/// The value is taken from the specified version of the schema (see <see cref="SchemaVersion"/>).
		/// If the <paramref name="version"/> is SchemaVersion.Proposed and the collection has no changes,
		/// an exception is thrown.
		/// If the <paramref name="version"/> is SchemaVersion.Committed and the entry is just added
		/// to the collection and is not committed yet, an exception is thrown.
		/// </para>
		/// <para>
		/// If the given <paramref name="key"/> not found in the collection, an exception
		/// <see cref="KeyNotFoundException"/> is thrown.
		/// </para>
		/// <example>
		/// <code>
		/// dataSet.IsAutocommitEnabled = false;
		/// . . .
		/// Variable v = dataSet["var"];
		/// v.Metadata["custom"] = 10;
		/// dataSet.Commit();
		/// 
		/// Console.WriteLine(v.Metadata["custom"]); // prints "10"
		/// 
		/// v.Metadata["custom"] = 11;
		/// /* Now: v.HasChanges, v.Metadata.HasChanges and dataSet.HasChanges are true */
		/// 
		/// Console.WriteLine(v.Metadata["custom"]); // prints committed value "10"
		/// Console.WriteLine(v.Metadata["custom", SchemaVersion.Proposed]); // prints "11"
		/// 
		/// dataSet.Rollback();
		/// Console.WriteLine(v.Metadata["custom"]); // prints "10"
		/// // Console.WriteLine(v.Metadata["custom", SchemaVersion.Proposed]); // would throw an exception
		/// </code>
		/// </example>
		/// </remarks>
		/// <seealso cref="this[string]"/>
		/// <seealso cref="this[string,bool]"/>
		/// <seealso cref="this[string,SchemaVersion,bool]"/>
		/// <seealso cref="SchemaVersion"/>
		/// <seealso cref="Variable"/>
		public object this[string key, SchemaVersion version]
		{
			get
			{
				if (version == SchemaVersion.Committed)
				{
					return MakeReturnedValue(dictionary[key]);
				}

				if (version == SchemaVersion.Proposed && modified == null)
					throw new Exception("There is no proposed version.");

				if (modified != null && modified.ContainsKey(key))
					return MakeReturnedValue(modified[key]);
				if (version == SchemaVersion.Proposed)
					throw new KeyNotFoundException("Key not found in proposed version.");

				var value = dictionary[key];
				return MakeReturnedValue(value);
			}
		}

		private static object MakeReturnedValue(object value)
		{
			if (value is ICloneable) return ((ICloneable)value).Clone();
			return value;
		}

		/// <summary>
		/// Gets the metadata value or sets new value for the given key (possibly case-insensitive).
		/// </summary>
		/// <param name="key">The name of the metadata attribute to get.</param>
		/// <param name="ignoreCase">The value determines whether the key is case-insensitive or not.</param>
		/// <returns>Value corresponding to the given key.</returns>
		/// <remarks>
		/// <para>
        /// The indexer returns committed values.
		/// See also remarks for <see cref="this[string, SchemaVersion]"/>.
		/// </para>
		/// <example>
		/// The example demonstrates the capabilities of the indexer:
		/// <code>
		/// MetadataDictionary metadata = . . . ;
		/// 
		/// metadata["Units"] = "meters";
		/// 
		/// if(metadata.ContainsKey("units", true)) // case-insensitive
		/// {
		///		Console.WriteLine(metadata["Units"]); // prints "meters"
		///		Console.WriteLine(metadata["units"]); // throws an exception KeyNotFoundException
		///		Console.WriteLine(metadata["units", true]); // prints "meters"
		///		Console.WriteLine(metadata["UNITS", true]); // prints "meters"
		///		Console.WriteLine(metadata["UnitS", true]); // prints "meters"		
		///	}
		/// </code>
		/// </example>
		/// </remarks>
		/// <seealso cref="ContainsKey(string,bool)"/>
		/// <seealso cref="this[string]"/>
		/// <seealso cref="this[string,SchemaVersion]"/>
		/// <seealso cref="this[string,SchemaVersion,bool]"/>
		/// <seealso cref="SchemaVersion"/>
		/// <seealso cref="Variable"/>
		public object this[string key, bool ignoreCase]
		{
			get
			{
				return this[key, SchemaVersion.Committed, true];
			}
		}

		/// <summary>
		/// Gets the metadata value or sets new value for the given key (possibly case-insensitive).
		/// </summary>
		/// <param name="key">The name of the metadata attribute to get.</param>
		/// <param name="ignoreCase">The value determines whether the key is case-insensitive or not.</param>
		/// <param name="version">Version of the DataSet schema.</param>
		/// <returns>Value corresponding to the given key.</returns>
		/// <remarks>
		/// <para>
		/// See remarks for <see cref="this[string, SchemaVersion]"/>
		/// and example for <see cref="this[string, bool]"/>.
		/// </para>
		/// </remarks>
		/// <seealso cref="ContainsKey(string,bool)"/>
		/// <seealso cref="this[string]"/>
		/// <seealso cref="this[string,bool]"/>
		/// <seealso cref="this[string,SchemaVersion]"/>
		/// <seealso cref="SchemaVersion"/>
		/// <seealso cref="Variable"/>
		public object this[string key, SchemaVersion version, bool ignoreCase]
		{
			get
			{
				if (!ignoreCase)
					return this[key, version];

				object value = null;
				if (version == SchemaVersion.Committed)
				{
					if (GetValueByKeyCaseInsensitive(dictionary, key, out value))
						return MakeReturnedValue(value);
					throw new KeyNotFoundException("Key not found in the committed version.");
				}

				if (version == SchemaVersion.Proposed && modified == null)
					throw new Exception("There is no proposed version.");

				if (modified != null && GetValueByKeyCaseInsensitive(modified, key, out value))
					return MakeReturnedValue(value);

				if (version == SchemaVersion.Proposed)
					throw new KeyNotFoundException("Key not found in the proposed version.");

				if (GetValueByKeyCaseInsensitive(dictionary, key, out value))
					return MakeReturnedValue(value);
				throw new KeyNotFoundException("Key not found in the committed version.");
			}
		}

		private bool GetValueByKeyCaseInsensitive(Dictionary<string, object> dictionary, string key, out object value)
		{
			if (dictionary.ContainsKey(key))
			{
				value = dictionary[key];
				return true;
			}
			foreach (var item in dictionary.Keys)
			{
				if (String.Compare(key, item, true) == 0)
				{
					value = dictionary[item];
					return true;
				}
			}
			value = null;
			return false;
		}

		/// <summary>
		/// Gets the metadata value or sets new value for the given key.
		/// </summary>
		/// <param name="key">The name of the metadata attribute to get.</param>
		/// <returns>Value corresponding to the given key.</returns>
		/// <remarks>
		/// <para>
        /// The indexer returns committed values.
		/// See remarks for <see cref="this[string, SchemaVersion]"/>.
		/// </para>
        /// <para>
        /// If the new value for the key is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>
		/// </remarks>
		/// <seealso cref="this[string, SchemaVersion]"/>
		/// <seealso cref="SchemaVersion"/>
		/// <seealso cref="Variable"/>
		public virtual object this[string key]
		{
			get
			{
				return this[key, SchemaVersion.Committed];
			}
			set
			{
				if (readOnly)
					throw new Exception("MetadataDictionary is read only");

				CheckKey(key);
				CheckValue(value);

				// Constraints on name
				if (key == KeyForName)
				{
					if (value != null && !(value is string))
						throw new Exception("Name of a variable must be a string");
				}

				object old = null;
				object cloneValue = null;
				if (value is ICloneable) cloneValue = ((ICloneable)value).Clone();
				else cloneValue = value;
				bool changed = true;
				if (HasChanges)
				{
					if (modified.ContainsKey(key))
					{
						old = modified[key];
						changed = !AreEquals(old, cloneValue);
					}
					else if (dictionary.ContainsKey(key))
					{
						old = dictionary[key];
						changed = !AreEquals(old, cloneValue);
					}
				}
				else if (dictionary.ContainsKey(key))
					changed = !AreEquals(old = dictionary[key], cloneValue);

				if (!changed)
					return;

				VariableMetadataChangingEventArgs e = new VariableMetadataChangingEventArgs(
					key, cloneValue, old);
				if (Changing != null)
				{
					Changing(this, e);
					if (e.Cancel)
						throw new Exception("Metadata changing is cancelled.");
				}

				if (modified == null)
					modified = new Dictionary<string, object>();

				modified[key] = e.ProposedValue;

				if (Changed != null)
					Changed(this, new VariableMetadataChangedEventArgs(key, e.ProposedValue));
			}
		}

        /// <summary>
        /// Compares two metadata values (either null, scalars, or 1d-array).
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
		protected static bool AreEquals(object v1, object v2)
		{
			if (v1 == null)
				return v2 == null;
			if (v2 == null)
				return v1 == null;
            if (v1.GetType().IsArray)
            {
                if (!v2.GetType().IsArray) return false;
                Array a1 = (Array)v1;
                Array a2 = (Array)v2;
                if (a1.Rank != 1 || a2.Rank != 1) 
                    throw new ArgumentException("Metadata value cannot be a multidimensional array");
                if (a1.Length != a2.Length) return false;
                for (int i = 0; i < a1.Length; i++)
                {
                    object e1 = a1.GetValue(i);
                    object e2 = a2.GetValue(i);
                    if (e1 == null)
                        if (e2 == null) continue;
                        else return false;
                    if (!e1.Equals(e2)) return false;
                }
                return true;
            }
			return v1.Equals(v2);
		}

		/// <summary>
		/// Checks whether the value satisfies constraints.
		/// If not, throws an exception.
		/// </summary>
		/// <param name="value"></param>
		protected void CheckValue(object value)
		{
			if (value == null) return;
			if (value is string) return;
			Type type = value.GetType();
			if (type.IsArray) // 1d array
			{
				type = type.GetElementType();
				if (!DataSet.IsSupported(type))
					throw new NotSupportedException("Type " + value.GetType() + " is not supported by metadata dictionary");

				Array array = (Array)value;
				if (array.Rank != 1) 
					throw new NotSupportedException("Metadata supports only 1d-arrays");
				if (type == typeof(string))
				{
					if (!Array.TrueForAll((string[])array, p => p != null))
						throw new NotSupportedException("String array in metadata cannot contain null");
				}
			}
			else // scalar
			{
				if (!DataSet.IsSupported(type))
					throw new NotSupportedException("Type " + value.GetType() + " is not supported by metadata dictionary");
			}			
		}

		/// <summary>
		/// Checks whether the key satisfies constraints.
		/// If not, throws an exception.
		/// </summary>
		/// <param name="key"></param>
		protected void CheckKey(string key)
		{
			if (key == null) throw new ArgumentNullException("key");
			if (String.IsNullOrEmpty(key))
				throw new ArgumentException("Key cannot be an empty string");
			if (key.Length > 256)
				throw new ArgumentException("Key length must be less or equal to 256");

			foreach (char c in key)
			{
				if (c == '/' ||
					(c >= '\x00' && c <= '\x1F') ||
					(c >= '\x7F' && c <= '\xFF'))
					throw new NotSupportedException("Key contains illegal chars");
			}
		}

		/// <summary>
		/// Determines whether the metadata contains the committed entry with the given <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The case sensitive key to locate in the collection.</param>
		/// <returns><value>true</value> if the collection contains an entry with the given key.</returns>
		/// <remarks>
		/// See remarks for <see cref="ContainsKey(string,SchemaVersion,bool)"/>.
		/// </remarks>
		/// <seealso cref="ContainsKey(string,bool)"/>
		/// <seealso cref="ContainsKey(string,SchemaVersion)"/>
		/// <seealso cref="ContainsKey(string,SchemaVersion,bool)"/>
		public bool ContainsKey(string key)
		{
			return ContainsKey(key, SchemaVersion.Committed);
		}

		/// <summary>
		/// Determines whether the metadata contains the committed entry with the given <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key to locate in the collection.</param>
		/// <param name="ignoreCase">The value indicating whether the method should ignore the case of the key or not.</param>
		/// <returns><value>true</value> if the collection contains an entry with the given key.</returns>
		/// <remarks>
		/// See remarks for <see cref="ContainsKey(string,SchemaVersion,bool)"/>.
		/// </remarks>
		/// <seealso cref="ContainsKey(string)"/>
		/// <seealso cref="ContainsKey(string,SchemaVersion)"/>
		/// <seealso cref="ContainsKey(string,SchemaVersion,bool)"/>
		public bool ContainsKey(string key, bool ignoreCase)
		{
			return ContainsKey(key, SchemaVersion.Committed, ignoreCase);
		}

		/// <summary>
		/// Determines whether the metadata contains the committed entry with the given <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The case sensitive to locate in the collection.</param>
		/// <param name="version">Version of the schema to locate the key in.</param>
		/// <returns><value>true</value> if the collection contains an entry with the given key; otherwise, <value>false</value>.</returns>
		/// <remarks>
		/// See remarks for <see cref="ContainsKey(string,SchemaVersion,bool)"/>.
		/// </remarks>
		/// <seealso cref="ContainsKey(string)"/>
		/// <seealso cref="ContainsKey(string,bool)"/>
		/// <seealso cref="ContainsKey(string,SchemaVersion,bool)"/>
		public bool ContainsKey(string key, SchemaVersion version)
		{
			if (version == SchemaVersion.Committed)
			{
				return dictionary.ContainsKey(key);
			}

			if (version == SchemaVersion.Proposed && modified == null)
				return false;

			if (modified != null && modified.ContainsKey(key))
				return true;
			if (version == SchemaVersion.Proposed)
				return false;

			return dictionary.ContainsKey(key);
		}

		/// <summary>
		/// Determines whether the metadata contains the committed entry with the given <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The case sensitive to locate in the collection.</param>
		/// <param name="version">Version of the schema to locate the key in.</param>
		/// <param name="ignoreCase">The value indicating whether the method should ignore the case of the key or not.</param>
		/// <returns><value>true</value> if the collection contains an entry with the given key; otherwise, <value>false</value>.</returns>
		/// <remarks>
		/// <example>
		/// <code>
		/// dataSet.IsAutocommitEnabled = false;
		/// . . .
		/// 
		/// Variable v = dataSet["var"];
		/// v.Metadata["custom"] = 10;
		/// 
		/// Console.WriteLine(v.Metadata.ContainsKey("custom", SchemaVersion.Proposed)); // prints "True"
		/// dataSet.Commit();
		/// 
		/// //Console.WriteLine(v.Metadata.ContainsKey("custom", SchemaVersion.Proposed)); // throws an exception
		/// Console.WriteLine(v.Metadata.ContainsKey("custom")); // prints "True"
		/// Console.WriteLine(v.Metadata.ContainsKey("Custom")); // prints "False"
		/// Console.WriteLine(v.Metadata.ContainsKey("Custom", false)); // prints "True"
		/// </code>
		/// </example>
		/// </remarks>
		/// <seealso cref="ContainsKey(string)"/>
		/// <seealso cref="ContainsKey(string,bool)"/>
		/// <seealso cref="ContainsKey(string,SchemaVersion)"/>
		public bool ContainsKey(string key, SchemaVersion version, bool ignoreCase)
		{
			if (!ignoreCase)
				return ContainsKey(key, version);

			if (version == SchemaVersion.Committed)
			{
				return DictContainsKey(dictionary, key);
			}

			if (version == SchemaVersion.Proposed && modified == null)
				return false;

			if (modified != null && DictContainsKey(modified, key))
				return true;
			if (version == SchemaVersion.Proposed)
				return false;

			return DictContainsKey(dictionary, key);
		}

		private bool DictContainsKey(Dictionary<string, object> dict, string key)
		{
			foreach (var item in dict.Keys)
			{
				if (String.Compare(key, item, true) == 0)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Performs the specified action on each element of the metadata collection. 
		/// </summary>
		/// <param name="action">The Action{KeyValuePair{string,object}} to perform on each element of array.</param>
		/// <param name="version">The version of the metadata schema to get elements from.</param>
		/// <remarks>
		/// <example>
		/// Prints to the console all metadata entries of the variable with name "var":
		/// <code>
		/// Variable v = dataSet.Variables["var"];
		/// v.Metadata.ForEach( entry => Console.WriteLine(entry.Key + ": " + entry.Value), SchemaVersion.Committed );
		/// </code>
		/// </example>
		/// </remarks>
		public void ForEach(Action<KeyValuePair<string, object>> action, SchemaVersion version)
		{
			if (version == SchemaVersion.Committed)
			{
				foreach (var item in dictionary)
					action(item);
			}
			else if (version == SchemaVersion.Proposed)
			{
				if (modified != null)
					foreach (var item in modified)
						action(item);
			}
			else if (version == SchemaVersion.Recent)
			{
				if (modified != null)
				{
					foreach (var item in dictionary)
						if (!modified.ContainsKey(item.Key))
							action(item);
					foreach (var item in modified)
						action(item);
				}
				else
				{
					foreach (var item in dictionary)
						action(item);
				}
			}
		}

		/// <summary>
		/// If there is a committed version for the given key, the method returns it.
		/// If not, it returns the proposed version (or throws an exception if there is no one).
		/// </summary>
		/// <typeparam name="VType">Type of the resulting value for the given key.</typeparam>
		protected internal VType GetComittedOtherwiseProposedValue<VType>(string key)
		{
			if (ContainsKey(key, SchemaVersion.Committed))
				return (VType)this[key, SchemaVersion.Committed];

			return (VType)this[key, SchemaVersion.Proposed];
		}

		/// <summary>
		/// If there is a committed version for the given key, the method returns it.
		/// If not, it returns the proposed version (or returns specified default value).
		/// </summary>
		/// <typeparam name="VType">Type of the resulting value for the given key.</typeparam>
		/// <param name="key">Key to get value for.</param>
		/// <param name="defaultValue">If the key not found returns this value.</param>
		protected internal VType GetComittedOtherwiseProposedValue<VType>(string key, VType defaultValue)
		{
			if (ContainsKey(key, SchemaVersion.Committed))
				return (VType)this[key, SchemaVersion.Committed];

			if (ContainsKey(key, SchemaVersion.Proposed))
				return (VType)this[key, SchemaVersion.Proposed];

			return defaultValue;
		}

		/// <summary>
		/// Represents the metadata collection as a Dictionary{string,object}.
		/// </summary>
		/// <param name="version">Version of the schema to get elements from.</param>
		/// <returns>An instance of a <see cref="System.Collections.Generic.Dictionary{TKey,TValue}"/> containing 
		/// all elements from the metadata collection.</returns>
		/// <remarks>
		/// <para>
		/// The method creates new instance of the <see cref="System.Collections.Generic.Dictionary{TKey,TValue}"/> and
		/// copies all elements from the specified version of the metadata collection into
		/// that instance. Therefore, modification of the dictionary will not affect the metadata
		/// collection and vice versa.
		/// </para>
		/// </remarks>
		/// <seealso cref="AsDictionary()"/>
		public Dictionary<string, object> AsDictionary(SchemaVersion version)
		{
			if (version == SchemaVersion.Committed)
				return Copy(dictionary);

			if (version == SchemaVersion.Proposed && modified == null)
				throw new Exception("There is no proposed version.");

			if (modified != null)
			{
				Dictionary<string, object> copy = new Dictionary<string, object>(dictionary);
				CopyToDict1(copy, modified);
				return copy;
			}

			if (version == SchemaVersion.Proposed)
				throw new Exception("Key not found in the proposed version.");

			Dictionary<string, object> dc = new Dictionary<string, object>(dictionary);
			return dc;
		}

		/// <summary>
		/// Represents the metadata collection as a <b>Dictionary{string,object}</b>.
		/// </summary>
		/// <returns>An instance of a <see cref="System.Collections.Generic.Dictionary{TKey,TValue}"/> containing 
		/// all elements from the metadata collection.</returns>
		/// <remarks>
		/// See remarks for <see cref="AsDictionary(SchemaVersion)"/>.
		/// </remarks>
		/// <seealso cref="AsDictionary(SchemaVersion)"/>
		public Dictionary<string, object> AsDictionary()
		{
			return AsDictionary(SchemaVersion.Recent);
		}

		/// <summary>
		/// Wraps committed elements of the metadata collection into 
		/// new read-only instance of the <see cref="MetadataDictionary"/> class.
		/// </summary>
		/// <returns>Read-only <see cref="MetadataDictionary"/> instance containg same inner collection.</returns>
		/// <remarks>
		/// <para>
		/// The returned metadata collection and the current one share the same inner collection
		/// of the committed elements. Therefore, after the source metadata collection is committed,
		/// new elements become available from the returned instance.
		/// </para>
		/// <para>
		/// The goal of the method is to create a read-only copy of the existing metadata collection that is always
		/// up-to-date.
		/// </para>
		/// </remarks>
		/// <seealso cref="Clone(SchemaVersion, bool)"/>
		public MetadataDictionary AsReadOnly()
		{
			// The name must be in the output collection even if 
			// is in the modified part only
			if (dictionary.ContainsKey(KeyForName))
				return new MetadataDictionary(true, dictionary);

			if (!modified.ContainsKey(KeyForName))
				throw new Exception("Variable has no name");

			var dicEx = new Dictionary<string, object>(dictionary);
			dicEx[KeyForName] = modified[KeyForName];
			return new MetadataDictionary(true, dicEx);
		}

		/// <summary>
		/// Creates a shallow copy of the metadata collection.
		/// </summary>
		/// <param name="version">The version of the schema to clone.</param>
		/// <param name="readOnly">The value that determines that the resulting 
		/// collection is read only or not.</param>
		/// <returns>MetadataDictionary instance containing the metadata copy.</returns>
		/// <remarks>
		/// <para>
		/// If the <paramref name="version"/> is SchemaVersion.Proposed or SchemaVersion.Recent and
		///  the collection is modified, resulting collection contains both committed elements and
		///  proposed and is unchanged.
		/// </para>
		/// <para>
		/// If the <paramref name="version"/> is SchemaVersion.Proposed and the collection has no changes,
		/// an exception is thrown.
		/// </para>
		/// </remarks>
		/// <seealso cref="AsReadOnly()"/>
		public MetadataDictionary Clone(SchemaVersion version, bool readOnly)
		{
			if (version == SchemaVersion.Committed)
				return new MetadataDictionary(readOnly, Copy(dictionary));

			if (version == SchemaVersion.Proposed && modified == null)
				throw new Exception("There is no proposed version.");

			if (modified != null)
			{
				Dictionary<string, object> copy = new Dictionary<string, object>(dictionary);
				CopyToDict1(copy, modified);
				return new MetadataDictionary(readOnly, copy);
			}

			if (version == SchemaVersion.Proposed)
				throw new Exception("Key not found in the proposed version.");

			Dictionary<string, object> dc = new Dictionary<string, object>(dictionary);
			return new MetadataDictionary(readOnly, dc);
		}

		internal MetadataChanges StartChanges()
		{
			if (modified == null)
				modified = new Dictionary<string, object>();

			return new MetadataChanges(modified);
		}

		private static Dictionary<string, object> Copy(Dictionary<string, object> dict)
		{
			Dictionary<string, object> d2 = new Dictionary<string, object>(dict);
			return d2;
		}

		private static void CopyToDict1(Dictionary<string, object> dict1, Dictionary<string, object> dict2)
		{
			if (dict2 == null)
				return;
			if (dict1 == null)
				throw new ArgumentNullException("dict1");

			foreach (var item in dict2)
			{
				dict1[item.Key] = item.Value;
			}
		}

		/// <summary>
		/// Commits accumulated changes.
		/// </summary>
		protected internal void Commit()
		{
			if (!HasChanges) return;

			dictionary = new Dictionary<string, object>(dictionary);
			CopyToDict1(dictionary, modified);
			modified = null;
		}

		/// <summary>
		/// Applies given collection of changes (both committed and modified parts of its).
		/// </summary>
		/// <param name="proposedChanges"></param>
		internal void ApplyChanges(MetadataDictionary proposedChanges)
		{
			bool localChanges = proposedChanges.dictionary == modified;
			bool dictCreated = false;
			if (HasChanges)
			{
				dictCreated = true;
				dictionary = new Dictionary<string, object>(dictionary);
				CopyToDict1(dictionary, modified);
				modified = null;
			}

			if (!localChanges)
			{
				if (!dictCreated)
				{
					dictionary = new Dictionary<string, object>(dictionary);
				}
				CopyToDict1(dictionary, proposedChanges.dictionary);
				CopyToDict1(dictionary, proposedChanges.modified);
			}
		}
		/// <summary>
		/// 
		/// </summary>
		protected internal void Rollback()
		{
			if (!HasChanges) return;

			modified = null;
		}

		/// <summary>Determines whether specified key is valid for the metadata collection.</summary>
		/// <param name="key">The strings to determine whether it can be a key.</param>
		/// <returns><value>true</value> if name is valid or false otherwise.</returns>
		public static bool IsValidKey(string key)
		{
			if (key == null) return false;
			if (String.IsNullOrEmpty(key))
				return false;
			if (key.Length > 256)
				return false;

			foreach (char c in key)
			{
				if (c == '/' ||
					(c >= '\x00' && c <= '\x1F') ||
					(c >= '\x7F' && c <= '\xFF'))
					return false;
			}
			return true;
		}

		/// <summary>Checks whether specified Unicode category is valid for first indentifier 
		/// symbol according to Python 3 specification</summary>
		/// <param name="cat">Unicode category</param>
		/// <returns>True for valid category or false otherwise</returns>
		internal static bool IsValidFirstNameSymbolCategory(UnicodeCategory cat)
		{
			return cat == UnicodeCategory.UppercaseLetter ||
				cat == UnicodeCategory.LowercaseLetter ||
				cat == UnicodeCategory.TitlecaseLetter ||
				cat == UnicodeCategory.ModifierLetter ||
				cat == UnicodeCategory.OtherLetter ||
				cat == UnicodeCategory.LetterNumber;
		}

		/// <summary>Checks whether specified symbol can be used as first indentifier 
		/// symbol according to Python 3 specification</summary>
		/// <param name="ch">Character</param>
		/// <returns>True for valid symbol or false otherwise</returns>
		internal static bool IsValidFirstNameSymbol(char ch)
		{
			UnicodeCategory cat = Char.GetUnicodeCategory(ch);
			return IsValidFirstNameSymbolCategory(cat) || ch == '_';
		}

		/// <summary>Checks whether specified symbol can be used as non-first indentifier 
		/// symbol according to Python 3 specification</summary>
		/// <param name="ch">Character</param>
		/// <returns>True for valid symbol or false otherwise</returns>
		internal static bool IsValidNextNameSymbol(char ch)
		{
			UnicodeCategory cat = Char.GetUnicodeCategory(ch);
			return IsValidFirstNameSymbol(ch) ||
				cat == UnicodeCategory.NonSpacingMark ||
				cat == UnicodeCategory.DecimalDigitNumber ||
				cat == UnicodeCategory.ConnectorPunctuation ||
				cat == UnicodeCategory.SpacingCombiningMark ||
				ch == '_';
		}

		#region IEnumerable<KeyValuePair<string,object>> Members

		IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
		{
			return dictionary.GetEnumerator();
		}

		/// <summary>
		/// Iterates through modified entries of the metadata dictionary.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<KeyValuePair<string, object>> GetModified()
		{
			if (modified == null)
				yield break;
			foreach (var item in modified)
			{
				yield return item;
			}
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return dictionary.GetEnumerator();
		}

		#endregion
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public delegate void VariableMetadataChangedEventHandler(object sender, VariableMetadataChangedEventArgs e);

	/// <summary>
	/// 
	/// </summary>
	public class VariableMetadataChangedEventArgs : EventArgs
	{
		private string key;
		private object value;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public VariableMetadataChangedEventArgs(string key, object value)
		{
			this.key = key;
			this.value = value;
		}
		/// <summary>
		/// 
		/// </summary>
		public string Key { get { return key; } }
		/// <summary>
		/// 
		/// </summary>
		public object Value { get { return value; } }
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public delegate void VariableMetadataChangingEventHandler(object sender, VariableMetadataChangingEventArgs e);

	/// <summary>
	/// 
	/// </summary>
	public class VariableMetadataChangingEventArgs : EventArgs
	{
		private string key;
		private object proposed, old;
		private bool cancel;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="proposed"></param>
		/// <param name="old"></param>
		public VariableMetadataChangingEventArgs(string key, object proposed, object old)
		{
			this.key = key;
			this.proposed = proposed;
			this.old = old;
		}
		/// <summary>
		/// 
		/// </summary>
		public string Key { get { return key; } }
		/// <summary>
		/// 
		/// </summary>
		public object ProposedValue { get { return proposed; } set { proposed = value; } }
		/// <summary>
		/// 
		/// </summary>
		public object OldValue { get { return old; } }
		/// <summary>
		/// 
		/// </summary>
		public bool Cancel
		{
			get { return cancel; }
			set { cancel = value; }
		}
	}
}

