// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data
{
	/// <summary>
	/// Base class for variables granting an access to various data storages.
	/// </summary>
	/// <typeparam name="DataType">Type of data for the variable.</typeparam>
    /// <remarks>
    /// The class contains logic for data acquiring and updating in related data storage.
    /// </remarks>
	public abstract class DataAccessVariable<DataType> : Variable<DataType>
	{
		#region Private & Protected fields

		private bool writeTransactionOpened = false;

		private MetadataDictionary metadata;

		#endregion

		#region Constructors

        /// <summary>
        /// Initializes a DataAccessVariable.
        /// </summary>
        /// <param name="dataSet">The owner of the variable.</param>
        /// <param name="dims">Dimensions of the variable.</param>
        /// <param name="assignID">Assign ID automatically or not.</param>
        /// <remarks>
        /// Method creates metadata collection.
        /// </remarks>
        protected DataAccessVariable(DataSet dataSet, string[] dims, bool assignID) :
            base(dataSet, dims, assignID)
        {
            this.metadata = new MetadataDictionary();
        }


		/// <summary>
		/// Initializes a DataAccessVariable.
		/// </summary>
		/// <param name="dataSet">The owner of the variable.</param>
		/// <param name="name">Name of the variable.</param>
		/// <param name="dims">Dimensions of the variable.</param>
		/// <remarks>
		/// Method creates metadata collection and initializes the Name.
		/// </remarks>
		protected DataAccessVariable(DataSet dataSet, string name, string[] dims) :
			this(dataSet, name, dims, true)
		{
		}

		/// <summary>
		/// Initializes a DataAccessVariable.
		/// </summary>
		/// <param name="dataSet">The owner of the variable.</param>
		/// <param name="name">Name of the variable.</param>
		/// <param name="dims">Dimensions of the variable.</param>
		/// <param name="assignID">Assign ID automatically or not.</param>
		/// <remarks>
		/// Method creates metadata collection and initializes the Name.
		/// </remarks>
		protected DataAccessVariable(DataSet dataSet, string name, string[] dims, bool assignID) :
			base(dataSet, dims, assignID)
		{
			this.metadata = new MetadataDictionary();
			this.Name = name;
		}


		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the metadata associated with the variable.
		/// </summary>
		public override MetadataDictionary Metadata
		{
			get { return metadata; }
		}

		#endregion

		#region Transactions

		/// <summary>
		/// The method is called at the precommit stage of the variable. 
        /// </summary>
        /// <remarks>
        /// <see cref="DataAccessVariable{DataType}"/> opens a write-transaction and actually updates the related storage with
		/// accumulated changes.
        /// </remarks>
		internal protected override void OnPrecommit(Variable.Changes proposedChanges)
		{
			/* Changing state of the variable */
			DataChanges dataChanges = proposedChanges as DataChanges;
			if (dataChanges != null && dataChanges.HasData)
			{
				// Writing data 
				BeginWriteTransaction(dataChanges);
				writeTransactionOpened = true;

				if (dataChanges.Data != null)
				{
					int n = dataChanges.Data.Count;
					for (int i = 0; i < n; i++)
					{
						DataPiece piece = dataChanges.Data[i];
						WriteData(piece.Origin, piece.Data);
					}
				}
			}
		}

		/// <summary>
        /// The method is called at the commit stage of the variable. 
        /// </summary>
        /// <remarks>
        /// <see cref="DataAccessVariable{DataType}"/>
		/// closes a write-transaction thus confirming its success.
		/// </remarks>
		internal protected override void OnCommit(Variable.Changes proposedChanges)
		{
			/* Committing written data */
			DataChanges dataChanges = proposedChanges as DataChanges;
			if (dataChanges != null && dataChanges.HasData)
			{
				CommitWrite(dataChanges);
			}
			writeTransactionOpened = false;
		}

		/// <summary>
        /// The method is called at the rollback stage of the variable.
        /// </summary>
        /// <remarks>
        /// <see cref="DataAccessVariable{DataType}"/> rolls an open transaction back, thus eliminating all changes since last successful committing.
		/// </remarks>
		internal protected override void OnRollback(Variable.Changes proposedChanges)
		{
			if (writeTransactionOpened)
			{
				RollbackWrite();
				writeTransactionOpened = false;
			}
		}

		/// <summary>
		/// Opens new write-transaction and thus prepares to a sequence of <see cref="WriteData"/>.
        /// </summary>
        /// <remarks>
		/// If the variable can support write-transactions it shall override this method.
        /// </remarks>
		protected virtual void BeginWriteTransaction(DataChanges proposedChanges)
		{
		}


		/// <summary>
        /// The methods is called right after all data in transaction is written with <see cref="WriteData"/>
		/// and before the call of Commit().
		/// </summary>
		protected virtual void OnWriteFinished(DataChanges proposedChanges)
		{
		}

		/// <summary>
        /// Commits a sequence of write-operations performed as a part of a transaction.
        /// </summary>
        /// <remarks>
        /// If the variable can support write-transactions it shall override this method.
        /// </remarks>
		protected virtual void CommitWrite(DataChanges proposedChanges)
		{
		}

		/// <summary>
        /// Rolls back a sequence of write-operations performed as a part of a transaction.
        /// </summary>
        /// <remarks>
        /// If the variable can support write-transactions it shall override this method.
        /// </remarks>
		protected virtual void RollbackWrite()
		{
		}

		#endregion

		#region Data Access

		/// <summary>
		/// Actually reads the data from the variable's underlying storage.
		/// </summary>
		/// <param name="origin">The origin of the rectangle (e.g., the left-bottom corner). Null means all zeros.</param>
		/// <param name="shape">The shape of the corned. Null means maximal possible shape.</param>
        /// <returns>An array of data from the specified rectangle.</returns>
		protected abstract Array ReadData(int[] origin, int[] shape);

		/// <summary>
		/// Actually reads the data from the variable's underlying storage.
		/// </summary>
        /// <param name="origin">The origin of the rectangle (e.g., the left-bottom corner). Null means all zeros.</param>
		/// <param name="stride">The steps to get slices from the array.</param>
		/// <param name="count">The shape of the corned. Null means maximal possible shape.</param>
        /// <returns>An array of data from the specified rectangle.</returns>
		protected virtual Array ReadData(int[] origin, int[] stride, int[] count)
		{
			int rank = Rank;
			int[] shape = new int[rank];
			for (int i = 0; i < rank; i++)
				shape[i] = (count[i] - 1) * stride[i] + 1;

			Array entireData = GetData(origin, shape);

			// Extracting required elements
			return GetStride(entireData, stride, count);
		}

		/// <summary>
		/// Writes the data to the variable's underlying storage starting with the specified origin indices.
		/// </summary>
		/// <remarks>
        /// <para>
        /// Each operation is a part of a transaction, opened with the <see cref="BeginWriteTransaction"/> method.
        /// *It is called in the precommit stage*
        /// </para>
        /// <para>A sequence of such outputs with same transaction can be either committed by <see cref="CommitWrite"/> or
        /// rolled back by <see cref="RollbackWrite"/>.</para>
		/// <para>Parameter <paramref name="origin"/> for data piece produced by <see cref="Variable.Append(Array)"/> operation
		/// is transformed and contains actual values.</para>
		/// </remarks>
		/// <param name="origin">Indices to start adding of data. Null means all zeros.</param>
		/// <param name="data">Data to add to the variable.</param>
		protected abstract void WriteData(int[] origin, Array data);


		/// <summary>
		/// Gets the data for the variable in specified rectangular region.
		/// </summary>
		/// <param name="origin">The origin of the rectangle (left-bottom corner). Null means all zeros.</param>
		/// <param name="shape">The shape of the region. Null means maximal shape.</param>
        /// <returns>An array of data from specified rectangle.</returns>
		public override Array GetData(int[] origin, int[] shape)
		{
			if (Rank == 0)
			{
				if (origin != null && origin.Length > 0)
					throw new ArgumentException("origin is incorrect");
				if (shape != null && shape.Length > 0)
					throw new ArgumentException("shape is incorrect");
			}
            if (shape != null && Array.IndexOf(shape, 0) >= 0)
                return Array.CreateInstance(TypeOfData, shape);
			return ReadData(origin, shape);
		}

		/// <summary>
		/// Gets the data for the variable from specified stridden slices.
		/// </summary>
        /// <param name="origin">The origin of the rectangle (left-bottom corner). Null means all zeros.</param>
		/// <param name="count">The shape of the result.</param>
		/// <param name="stride">Steps to stride the variable.</param>
        /// <returns>An array of data from specified rectangle.</returns>
		/// <seealso cref="Microsoft.Research.Science.Data.DataSet.StrideVariable{DataType}(Variable{DataType},int[],int[],int[],string)"/>
		public override Array GetData(int[] origin, int[] stride, int[] count)
		{
			if (Rank == 0)
			{
				if (origin != null && origin.Length > 0)
					throw new ArgumentException("Origin in GetData() for scalar variable must be null or empty", "origin");
				if (count != null && count.Length > 0)
					throw new ArgumentException("Count in GetData() for scalar variable must be null or empty", "count");
				if (stride != null && stride.Length > 0)
					throw new ArgumentException("Stride in GetData() for scalar variable must be null or empty", "stride");
				return GetData(null, null);
			}

			if (stride == null)
			{
				// default stride is 1
				return GetData(origin, count);
			}

			int rank = Rank;

			// If stride == 1 we can use simple GetData method
			int k = 0;
			for (k = 0; k < rank; k++)
			{
				if (stride[k] != 1) break;
			}
			if (k == rank)
				return GetData(origin, count);

			// Going further...
			if (origin == null)
				origin = new int[rank];

			if (count == null)
			{
				// taking count as large as possible
				count = new int[rank];
				int[] entireShape = GetShape(SchemaVersion.Committed);
				for (int i = 0; i < rank; i++)
				{
                    count[i] = 1 + (entireShape[i] - 1 - origin[i]) / stride[i];
				}
            }
            else if (Array.IndexOf(count, 0) >= 0)
                return Array.CreateInstance(TypeOfData, count);

			return ReadData(origin, stride, count);
		}

		#endregion
	}
}

