// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Threading;

namespace Microsoft.Research.Science.Data.Memory
{
	/// <summary>
	/// MemoryVariable stores all data in memory.
	/// </summary>
	/// <typeparam name="DataType"></typeparam>
	internal class MemoryVariable<DataType> : DataAccessVariable<DataType>, IMemoryVariable, IInternalMemoryVariable
	{
		/// <summary>Array holding actual data for variable. 
		/// It is <see cref="T:Microsoft.Research.Science.Data.ArrayWrapper"/> so it holds information
		/// about rank and size even if there is no data values.</summary>
		protected ArrayWrapper array;

		/// <summary>Committed data (enables rollback).</summary>
		private ArrayWrapper copyArray = null;

		private DataChanges recentChanges;
		private bool getRecentDataCalled;

		/// <summary>
		/// Initializes an instance of the variable.
		/// </summary>
		/// <param name="sds"></param>
		/// <param name="name"></param>
		/// <param name="dims"></param>
		public MemoryVariable(DataSet sds, string name, string[] dims)
			: base(sds, name, dims)
		{
			this.array = new ArrayWrapper(dims.Length, typeof(DataType));
			Initialize();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sds"></param>
		/// <param name="name"></param>
		/// <param name="dims"></param>
		/// <param name="wrapper"></param>
		protected MemoryVariable(DataSet sds, string name, string[] dims, ArrayWrapper wrapper)
			: base(sds, name, dims)
		{
			if (wrapper == null)
				throw new ArgumentNullException("wrapper");
			this.array = wrapper;
			Initialize();
		}

		/// <summary>
		/// Gets the recent data from the variable.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// The method is to be called from the DataSet.OnPrecommit method.
		/// </remarks>
		Array IInternalMemoryVariable.GetRecentData()
		{
			if (recentChanges == null)
			{
				var data = array.Data;
				if (data == null)
					return Array.CreateInstance(TypeOfData, new int[Rank]);
				return data;
			}
			else
			{
				if (getRecentDataCalled) return copyArray.Data;
				int n = Rank;
				if (copyArray == null) // no enlarge
				{
					copyArray = new ArrayWrapper(n, TypeOfData);
					copyArray.Data = (Array)array.Data.Clone();
				}
				else if (array.Data != null)
					copyArray.PutData(null, array.Data);
				for (int i = 0; i < recentChanges.Data.Count; i++)
				{
					DataPiece piece = recentChanges.Data[i];
					copyArray.PutData(piece.Origin, piece.Data);
				}
				getRecentDataCalled = true;
				return copyArray.Data;
			}
		}

		#region Low-level data access

		protected override void BeginWriteTransaction(DataChanges changes)
		{
			if (changes == null || changes.Shape == null) return;
			if (changes.Shape.Length != Rank)
				throw new Exception("Changes has wrong shape");

			copyArray = null;
			recentChanges = changes;
			getRecentDataCalled = false;

			bool needEnlarge = false;
			int n = Rank;
			int[] shape = array.GetShape();
			for (int i = 0; i < n; i++)
			{
				if (changes.Shape[i] > shape[i])
				{
					needEnlarge = true;
					break;
				}
			}

			if (needEnlarge)
			{
				copyArray = new ArrayWrapper(n, TypeOfData);
				copyArray.Data = Array.CreateInstance(copyArray.DataType, changes.Shape);
			}
		}

		protected override void CommitWrite(DataChanges proposedChanges)
		{
			if (proposedChanges != null && proposedChanges.HasData)
			{
				if (getRecentDataCalled)
					array = copyArray;
				else
				{
					int n = proposedChanges.Data.Count;
					if (copyArray != null)
					{
						if (array.Data != null)
							copyArray.PutData(null, array.Data);
						array = copyArray;
					}
					for (int i = 0; i < n; i++)
					{
						DataPiece piece = proposedChanges.Data[i];
						array.PutData(piece.Origin, piece.Data);
					}
				}
			}
			recentChanges = null;
			getRecentDataCalled = false;
		}

		protected override void RollbackWrite()
		{
			copyArray = null;
			recentChanges = null;
			getRecentDataCalled = false;
		}

		/// <summary>
		/// Writes the data to the variable's underlying storage starting with the specified origin indices.
		/// Each operation is a part of a transaction, opened with the BeginWriteTransaction() method.
		/// *It is called in the precommit stage*
		/// </summary>
		/// <remarks>
		/// <para>A sequence of such outputs with same transaction can be either committed by CommitWrite() or
		/// rolled back by RollbackWrite().</para>
		/// <para>Parameter <paramref name="origin"/> for data piece produced by Append operation
		/// in given to the method already transformed and contains actual values.</para>
		/// </remarks>
		/// <param name="origin">Indices to start adding of data. Null means all zeros.</param>
		/// <param name="a">Data to add to the variable.</param>
		protected override void WriteData(int[] origin, Array a)
		{
		}

		protected override int[] ReadShape()
		{
			if (array == null)
				return null;
			return array.GetShape();
		}

		/// <summary>Reads array of values from variable</summary>
		/// <param name="origin">Origin of region to read. Null origin means all zeros</param>
		/// <param name="shape">Shape of region to read. Null shape means entire array of data.</param>
		/// <returns>Array of values loaded from variable</returns>
		protected override Array ReadData(int[] origin, int[] shape)
		{
			return array.GetData(origin, shape);
		}

		#endregion

		#region Serialization

		public void SaveAsXml(XmlWriter w)
		{
			StringBuilder shape = new StringBuilder();
			foreach (Dimension d in Dimensions)
			{
				shape.Append(d.Name);
				shape.Append(' ');
			}

			w.WriteStartElement("variable", MemoryDataSet.XmlNamespace);
			w.WriteAttributeString("name", Name);
			w.WriteAttributeString("type", TypeOfData.ToString());
			w.WriteAttributeString("shape", shape.ToString());

			WriteAttributes(w);

			w.WriteStartElement("values", MemoryDataSet.XmlNamespace);
			w.WriteAttributeString("separator", " ");
			w.WriteAttributeString("culture", Thread.CurrentThread.CurrentCulture.Name);
			WriteValues(w);
			w.WriteEndElement();

			w.WriteEndElement();
		}

		private void WriteAttributes(XmlWriter w)
		{

			foreach (var item in Metadata)
			{
				WriteAttribute(w, item.Key, item.Value);
			}
		}

		private void WriteAttribute(XmlWriter w, string name, object value)
		{
			w.WriteStartElement("attribute", MemoryDataSet.XmlNamespace);
			w.WriteAttributeString("name", name);
			if (value != null)
			{
				if (!(value is String))
					w.WriteAttributeString("type", value.GetType().FullName);
				w.WriteValue(value.ToString());
			}
			else
			{
				w.WriteValue("");
			}
			w.WriteEndElement();
		}

		private void WriteValues(XmlWriter w)
		{
			if (array == null || array.Data == null)
				return;

			if (array.Rank == 1)
			{
				Array data = array.Data;
				for (int i = 0; i < data.Length; i++)
				{
					w.WriteString(data.GetValue(i).ToString() + ' ');
				}
			}
			else
			{
				Array data = array.Data;
				int[] getIndex = new int[data.Rank];
				int length = data.Length;

				for (int i = 0; i < length; i++)
				{
					object o = data.GetValue(getIndex);
					w.WriteString(o.ToString() + ' ');

					for (int j = 0; j < getIndex.Length; j++)
					{
						getIndex[j]++;
						if (getIndex[j] < data.GetLength(j))
							break;
						getIndex[j] = 0;
					}
				}
			}
		}

		#endregion
	}

	internal interface IInternalMemoryVariable
	{
		void SaveAsXml(XmlWriter w);

		/// <summary>
		/// Gets the recent data from the variable.
		/// </summary>
		/// <returns></returns>
		Array GetRecentData();
	}

	/// <summary>
	/// The variable implementing the interface is a MemoryVariable.
	/// </summary>
	public interface IMemoryVariable
	{
	}
}

