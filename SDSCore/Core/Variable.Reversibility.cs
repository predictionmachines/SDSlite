// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Microsoft.Research.Science.Data
{
	public abstract partial class Variable
	{
		/// <summary>
		/// Checks whether the variable is reversible.
		/// </summary>
		/// <param name="version">Version of the variable to check.</param>
		/// <returns>Returns true if the variable is reversible.</returns>
		/// <remarks>
		/// <para>
		/// One of the ScientificDataSet consistency constraints is the constraint on axis data: 
		/// each axis of numeric and DateTime values must be reversible and an axis of any other type must have 
		/// no duplicate values. The constraint enables the reversing mechanism which makes it possible to get 
		/// a value from a variable not only by its indices but also by its coordinate values 
		/// (for details see remarks for <see cref="Microsoft.Research.Science.Data.DataSet.Commit()"/>). 
		/// The current release of the library provides a mechanism of the constraint check 
		/// only for one-dimensional axis and only for numeric and DateTime values. 
		/// This is the only implemented check for performance reasons.
		/// </para>
		/// <para>
		/// The check is implemented at the level of the <see cref="Variable{DataType}"></see> class, 
		/// the base class for all variables. Therefore each variable inherently enables the reversibility 
		/// check by default. </para>
		/// <para>
		/// There are two methods declared as abstract in the <see cref="Variable"/> class and 
		/// implemented in the <see cref="Variable{DataType}"></see> class: 
		/// public method <see cref="CheckReversibility(SchemaVersion)"/> and internal method <see cref="IndicesOf"/>.</para>
		/// <para>
		/// The <see cref="CheckReversibility(SchemaVersion)"/> method makes a check for the given schema version of 
		/// the variable and returns either true or false depending whether data is reversible or not. 
		/// The check is actually performed only for one-dimensional variables having a numeric or 
		/// DateTime data type. For any other case the method just returns true without a check. 
		/// The method is called in the commit procedure (at the precommit stage; see remarks for 
		/// <see cref="Microsoft.Research.Science.Data.DataSet.Commit()"/>) 
		/// as a part of the CheckConstaints logic of the CoordinateSystem class. 
		/// This means that it is called at the committing for each variable of a data set being an axis. 
		/// If the method returns false, the check failed.
		/// </para>
		/// <para>
		/// The current data stored in the data set’s underlying storage is always considered as a consistent, 
		/// hence when a variable is initialized as an axis on a data set loading, 
		/// it is also considered as satisfying of all constraints including reversibility 
		/// without actual check of its persistent data. If the committed variable 
		/// (that is not an axis) is added as an axis to a coordinate system, 
		/// then in the next constraint check (i.e. in the nearest commit procedure) 
		/// all persistent data of the variable is loaded and checked on reversibility. 
		/// The new data arrays those have been put to the variable are checked during the commit procedure. 
		/// All these arrays must have the same order and they are also checked in the context of 
		/// the already committed data (if needed, one value to the left and one to the right 
		/// of the proposed data array are loaded from the persistent storage and they must 
		/// be in accordance with the array order itself). The result of the check is cached, 
		/// so in the next commit the only proposed data arrays are checked.
		/// </para>
		/// </remarks>
		protected bool CheckReversibility(SchemaVersion version)
		{
			return true;
			/*Changes changes;
			switch (version)
			{
				case SchemaVersion.Committed:
					changes = null;
					break;
				case SchemaVersion.Proposed:
				case SchemaVersion.Recent:
					changes = this.changes;
					break;
				default:
					throw new ArgumentException("Unexpected version of schema");
			}
			return CheckReversibility(changes);*/
		}

		/// <summary>
		/// Checks whether the variable including the given changes is reversible .
		/// </summary>
		/// <param name="changes"></param>
		/// <returns></returns>
		protected abstract bool CheckReversibility(Changes changes);

		/// <summary>
		/// Returns indices of the variable's element corresponding to the given value.
		/// </summary>
		/// <param name="value">The value which index is to be returned.</param>
		/// <returns>If the value is not found and it is less than one or more elements in array, 
		/// a negative number which is the bitwise complement of the index of the first element 
		/// that is larger than value. If value is not found and value is greater than 
		/// any of the elements in array, a negative number which is the bitwise complement of 
		/// (the index of the last element plus 1).</returns>
		/// <remarks>
		/// See remarks for <see cref="Variable{DataType}.IndicesOf(DataType)"/>.
		/// </remarks>
		/// <seealso cref="Variable"/>
		/// <seealso cref="Variable{DataType}.GetValue(ReverseIndexSelection,CoordinateSystem,object[])"/>
		internal abstract int[] IndicesOf(object value);
	}

	public abstract partial class Variable<DataType>
	{
		/// <summary>
		/// The current order of the data (this allows not to check all data from a storage each time and cache the result).
		/// </summary>
		private ArrayOrder committedOrder = ArrayOrder.NotChecked;

		/// <summary>
		/// The current order of the proposed data (this allows not to check all data from a storage each time and cache the result).
		/// </summary>
		private ArrayOrder proposedOrder = ArrayOrder.NotChecked;

		/// <summary>
		/// Resets proposedOrder on put data
		/// </summary>
		private void OnVariableChanged(object sender, VariableChangedEventArgs e)
		{
			if (e.Action == VariableChangeAction.PutData)
				proposedOrder = ArrayOrder.NotChecked;
		}

		/// <summary>
		/// Returns indices of the variable's element corresponding to the given value.
		/// </summary>
		/// <param name="value">The value which index is to be returned.</param>
		/// <returns>If the value is not found and it is less than one or more elements in array, 
		/// a negative number which is the bitwise complement of the index of the first element 
		/// that is larger than value. If value is not found and value is greater than 
		/// any of the elements in array, a negative number which is the bitwise complement of 
		/// (the index of the last element plus 1).</returns>
		/// <remarks>
		/// See remarks for <see cref="IndicesOf(DataType)"/>.
		/// </remarks>
		/// <seealso cref="Variable"/>
		/// <seealso cref="Variable{DataType}.GetValue(ReverseIndexSelection,CoordinateSystem,object[])"/>
		internal override int[] IndicesOf(object value)
		{
			return this.IndicesOf((DataType)value);
		}

		/// <summary>
		/// Returns indices of the variable's element corresponding to the given value.
		/// </summary>
		/// <param name="value">The value which index is to be returned.</param>
		/// <returns>If the value is not found and it is less than one or more elements in array, 
		/// a negative number which is the bitwise complement of the index of the first element 
		/// that is larger than value. If value is not found and value is greater than 
		/// any of the elements in array, a negative number which is the bitwise complement of 
		/// (the index of the last element plus 1).</returns>
		/// <remarks>
		/// <para>
		/// The method accepts a value and returns the corresponding indices vector of the element 
		/// containing the given value. 
		/// Please note that the method works with the committed data only. </para>
		/// <para>
		/// If the value is not found and it is less than one or more elements in array, 
		/// a negative number which is the bitwise complement of the index of the first 
		/// element that is larger than value. If value is not found and value is greater 
		/// than any of the elements in array, a negative number which is the bitwise complement of 
		/// (the index of the last element plus 1). </para>
		/// <para>
		/// The method is used in <see cref="Variable{DataType}.GetValue(ReverseIndexSelection,CoordinateSystem,object[])"/>
		/// methods of the <see cref="Variable{DataType}"/> retrieving a value by given coordinate values.  
		/// The method is implemented for 1d-variables only and for any other rank it throws the 
		/// <see cref="NotSupportedException"/>. The method uses the order of an array computed by the
		/// <see cref="CheckReversibility"/> method during the constraints check.</para>
		/// <para>
		/// Each index in the returned array corresponds to the dimension with the same index
		/// from the collection <see cref="Variable.Dimensions"/>.
		/// </para>
		/// </remarks>
		/// <seealso cref="Variable"/>
		/// <seealso cref="Variable{DataType}.GetValue(ReverseIndexSelection,CoordinateSystem,object[])"/>
		internal int[] IndicesOf(DataType value)
		{
			if (Rank != 1)
				throw new NotSupportedException("Variable with rank 1 are supported only");
	
			DataType[] data = (DataType[])GetData();

			IComparer<DataType> cmp =  Comparer<DataType>.Default;
			ArrayOrder order;
			if (committedOrder == ArrayOrder.Ascendant || committedOrder == ArrayOrder.Descendant)
				order = committedOrder;
			else
			{
				order = ArrayOrder.Ascendant;
				if (data.Length > 1)
					order = (cmp.Compare(data[1], data[0]) > 0) ? ArrayOrder.Ascendant : ArrayOrder.Descendant;
			}

			int index;
			if(order == ArrayOrder.Ascendant)
				index = Array.BinarySearch<DataType>(data, value, cmp);
			else{
				cmp = new DescendantComparer(cmp);
				index = Array.BinarySearch<DataType>(data, value, cmp);
			}

			return new int[] { index };
		}

		private class DescendantComparer : IComparer<DataType>
		{
			private IComparer<DataType> cmp;

			public DescendantComparer()
			{
				cmp = Comparer<DataType>.Default;
			}

			public DescendantComparer(IComparer<DataType> cmp)
			{
				this.cmp = cmp;
			}

			#region IComparer<DataType> Members

			public int Compare(DataType x, DataType y)
			{
				return cmp.Compare(y, x);
			}

			#endregion
		}

		/// <summary>
		/// Checks whether the variable is reversible.
		/// </summary>
		/// <param name="changes">Changes to be applied to the variable.</param>
		/// <returns>Returns true if the variable is reversible.</returns>
		/// <remarks>
		/// The method makes actual check for 1d variables only. 
		/// For any other rank it just returns true without the check.
		/// </remarks>
		protected override bool CheckReversibility(Changes changes)
		{
			// We can make the check for 1d variables only
			if (Rank != 1)
				return true; // all others are allowed to be an axis (at least, at now)	

			// If this is the first commit of the variable and it is already an axis
			// then it's been read from the underlying storage as an axis and
			// since a storage is always consistent then the variable is ordered:
			if (DataSet.Version == 0 && IsAxis(SchemaVersion.Recent))
			{
				committedOrder = ArrayOrder.Unknown;
				proposedOrder = ArrayOrder.Unknown;
				return true;
			}

			ArrayOrder order = GetOrder1d(changes);
			return order != ArrayOrder.None;
		}

		/// <summary>
		/// <para>Gets the order of the variable (works for 1d variables only).</para>
		/// <para>Affects committedOrder and proposedOrder fields.</para>
		/// <para>Returns Ascendant, Descendant, Unknown or None.</para>
		/// </summary>
		private ArrayOrder GetOrder1d(Changes changes)
		{
			// These types cannot be monotonic
			if (!TypeUtils.IsNumeric(TypeOfData) && !TypeUtils.IsDateTime(TypeOfData))
				return ArrayOrder.Unknown;

			// We can make the check for 1d variables only
			if (Rank != 1)
				// all others are allowed to be an axis (at least, at now)			
				throw new NotSupportedException("Variables with rank 1 are supported only");

			// If we need to check the committed version
			if (changes == null)
			{
				// There is no information about the order
				if (committedOrder == ArrayOrder.NotChecked)
				{
					Array a = GetData();
					committedOrder = OrderedArray1d.IsOrdered<DataType>((DataType[])a);
				}

				return committedOrder;
			}
			else // Check of the proposed version
			{
				// If the committing changes doesn't contain any new data...
				Variable.DataChanges dataChanges = changes as Variable.DataChanges;
				if (dataChanges == null || !dataChanges.HasData)
				{
					// If still not checked - check the data in the underlying storage
					if (committedOrder == ArrayOrder.NotChecked)
					{
						// The variable is still empty
						if (this.changes.InitialSchema.Dimensions[0].Length == 0)
							committedOrder = ArrayOrder.Unknown;
						else
						{
							// If committed version is reversible then the variable is reversible 
							// since there is no new data
							return GetOrder1d(null);
						}
					}
					return committedOrder;
				} // EOF "no new data to put"
				else // dataPieces.Count > 0
				{
					/*********************************************************************/
					/* 1. An order of all data pieces must be the same.
					 * Comments:
					 * - New data pieces may fill all the existing array so we cannot
					 * use committingOrder event if is known.
					 * *******************************************************************/
					ArrayOrder dOrder = ArrayOrder.NotChecked;
					for (int i = dataChanges.Data.Count; --i >= 0; )
					{
						DataPiece piece = dataChanges.Data[i];

						ArrayOrder order = OrderedArray1d.IsOrdered<DataType>((DataType[])piece.Data);
						if (order == ArrayOrder.Unknown)
							continue; // an array is empty or has 1 element
						if (dOrder == ArrayOrder.NotChecked)
							dOrder = order;
						else if (dOrder != order)
						{
							// This data piece isn't ordered or is ordered in the different way 
							proposedOrder = ArrayOrder.None;
							return ArrayOrder.None;
						}
						// comparing to other pieces.
					}
					if (dOrder == ArrayOrder.NotChecked) // i.e. all pieces have 1 element
						dOrder = ArrayOrder.Unknown;

					/*********************************************************************/
					/* 2. Checking whether all data pieces are in accordance with 
					 * the committed data and between themselves.
					 * Comments:
					 * - Going from the last piece because we put data starting with 
					 * the first one. So, it will overlap all beneath.
					 * - TODO: optimize 2 possible this[left], this[leftleft] to 1 GetData for 2 points
					 * *******************************************************************/
					ArrayOrder cOrder = ArrayOrder.Unknown; // the order that we will get from the neighbourhood

					int shape = dataChanges.Shape[0];
					int committedShape = dataChanges.InitialSchema.Dimensions[0].Length;

					if (shape == 1) // only one point in the proposed data
					{
						proposedOrder = ArrayOrder.Unknown;
						committedOrder = ArrayOrder.Unknown;
						return ArrayOrder.Unknown;
					}

					Comparer<DataType> comparer = Comparer<DataType>.Default;
					for (int i = dataChanges.Data.Count; --i >= 0; )
					{
						// TODO: add check: if the lefter or righter point of the piece is alread checked for the previous piece, then skip it.
						DataPiece piece = dataChanges.Data[i];

						// Checking left point: trying to find the actual value to the left of the piece
						int leftIndex = piece.Origin[0] - 1;
						if (leftIndex >= 0)
						{
							int j = IndexOfDataPieceContaining(dataChanges, leftIndex);
							if (j >= 0)
							{
								// Found data piece having the left point
								DataPiece pieceJ = dataChanges.Data[j];
								DataType[] arrJ = (DataType[])pieceJ.Data;
								DataType leftValue = arrJ[leftIndex - pieceJ.Origin[0]];
								ArrayOrder order = GetOrder(leftValue, ((DataType[])piece.Data)[0], comparer);
								if (order == ArrayOrder.None) { proposedOrder = ArrayOrder.None; return ArrayOrder.None; }
								if (dOrder == ArrayOrder.Unknown) dOrder = order;
								else if (dOrder != order) { proposedOrder = ArrayOrder.None; return ArrayOrder.None; }
							}
							if (j < 0) // no data pieces with this value
							{
								if (leftIndex >= committedShape)
								{ proposedOrder = ArrayOrder.None; return ArrayOrder.None; } // the hole is found; it is inadmissable.

								DataType leftValue = this[leftIndex];
								ArrayOrder order = GetOrder(leftValue, ((DataType[])piece.Data)[0], comparer);
								if (order == ArrayOrder.None) { proposedOrder = ArrayOrder.None; return ArrayOrder.None; }
								if (dOrder == ArrayOrder.Unknown) dOrder = order;
								else if (dOrder != order) { proposedOrder = ArrayOrder.None; return ArrayOrder.None; }

								// If to the left there is one more point and we don't know the ordering of the
								// committed data we can find it out if the one more point to the left
								// also belongs to the committed data but not to a data piece.
								if (cOrder == ArrayOrder.Unknown && leftIndex > 0)
								{
									int leftleftIndex = leftIndex - 1;
									j = IndexOfDataPieceContaining(dataChanges, leftleftIndex);
									if (j < 0)
									{
										DataType leftleftValue = this[leftleftIndex];
										order = GetOrder(leftleftValue, leftValue, comparer);
										if (order == ArrayOrder.None) { committedOrder = ArrayOrder.None; proposedOrder = ArrayOrder.None; return ArrayOrder.None; }
										if (dOrder != order) { proposedOrder = ArrayOrder.None; return ArrayOrder.None; }
										cOrder = dOrder;
									}
								}
							}
						} // eof left point check
						// Checking right point: trying to find the actual value to the right of the piece
						int rightIndex = piece.Origin[0] + piece.Data.Length;
						if (rightIndex < shape)
						{
							int j = IndexOfDataPieceContaining(dataChanges, rightIndex);
							if (j >= 0)
							{
								// Found data piece having the right point
								DataPiece pieceJ = dataChanges.Data[j];
								DataType[] arrJ = (DataType[])pieceJ.Data;
								DataType rightValue = arrJ[rightIndex - pieceJ.Origin[0]];
								ArrayOrder order = GetOrder(((DataType[])piece.Data)[piece.Data.GetLength(0) - 1], rightValue, comparer);
								if (order == ArrayOrder.None) { proposedOrder = ArrayOrder.None; return ArrayOrder.None; }
								if (dOrder == ArrayOrder.Unknown) dOrder = order;
								else if (dOrder != order) { proposedOrder = ArrayOrder.None; return ArrayOrder.None; }
							}
							if (j < 0) // no data pieces with this value
							{
								if (rightIndex >= committedShape)
								{ proposedOrder = ArrayOrder.None; return ArrayOrder.None; } // the hole is found; it is inadmissable.

								DataType rightValue = this[rightIndex];
								ArrayOrder order = GetOrder(((DataType[])piece.Data)[piece.Data.GetLength(0) - 1], rightValue, comparer);
								if (order == ArrayOrder.None) { proposedOrder = ArrayOrder.None; return ArrayOrder.None; }
								if (dOrder == ArrayOrder.Unknown) dOrder = order;
								else if (dOrder != order) { proposedOrder = ArrayOrder.None; return ArrayOrder.None; }

								// If to the right there is one more point and we don't know the ordering of the
								// committed data we can find it out if the one more point to the right
								// also belongs to the committed data but not to a data piece.
								if (cOrder == ArrayOrder.Unknown && rightIndex < shape - 1)
								{
									int rightrightIndex = rightIndex + 1;
									j = IndexOfDataPieceContaining(dataChanges, rightrightIndex);
									if (j < 0)
									{
										DataType rightrightValue = this[rightrightIndex];
										order = GetOrder(rightValue, rightrightValue, comparer);
										if (order == ArrayOrder.None) { committedOrder = ArrayOrder.None; proposedOrder = ArrayOrder.None; return ArrayOrder.None; }
										if (dOrder != order) { proposedOrder = ArrayOrder.None; return ArrayOrder.None; }
										cOrder = dOrder;
									}
								}
							}
						} // eof right point check
					} // eof FOR: data pieces

					// dOrder can be either Asc or Desc
					// cOrder can be equal to dOrder or to Unknown =>
					// There were no 2 points from committed data on end:
					if (cOrder == ArrayOrder.Unknown)
					{
						cOrder = dOrder; // taking the value from the data piece
						// all gaps from old data is less than 2 points
						// out committedOrder = cOrder
					}
					else
					{
						// There were gaps more than 2 points
						// So, if we know what is in those gaps...
						if (committedOrder != ArrayOrder.NotChecked)
						{
							if (committedOrder == ArrayOrder.None)
							{
								proposedOrder = ArrayOrder.None;
								return ArrayOrder.None;
							}
							if (committedOrder == ArrayOrder.Unknown)
							{
								committedOrder = cOrder;
								proposedOrder = cOrder;
								return cOrder; // cOrder == ArrayOrder.Ascendant || cOrder == ArrayOrder.Descendant;
							}
							// the order must be the same:
							return committedOrder;
						}

						// If not, we have to find those gaps and check that all of them
						// are ordered in cOrder:
						/****************************************************************************/
						/* Checking gaps from the storage data
						 * **************************************************************************/
						// Sorting data pieces by its origin.
						List<DataPiece> pieces = dataChanges.Data;
						int[] sortedPieces = new int[pieces.Count];
						for (int i = 0; i < pieces.Count; i++)
							sortedPieces[i] = i;
						Array.Sort(sortedPieces, (int i, int j) => pieces[j].Origin[0] - pieces[i].Origin[0]);
						for (int i = 1; i < sortedPieces.Length; i++)
						{
							if (pieces[sortedPieces[i]].Origin == pieces[sortedPieces[i - 1]].Origin &&
								sortedPieces[i - 1] < sortedPieces[i])
							{
								int j = sortedPieces[i - 1];
								sortedPieces[i - 1] = sortedPieces[i];
								sortedPieces[i] = j;
								if (i > 1) i -= 2;
							}
						}

						/****************************************************************************/
						/* Checking gaps
						 * 
						 * [left,right] is an interval of committed data not overlapped by pieces
						 * Its order must be checked and be equal to cOrder
						 * **************************************************************************/
						int current = 0;
						int left = 0;
						int right = pieces[sortedPieces[current]].Origin[0] - 1;

						while (true)
						{
							if (right - left > 1)
							{
								DataType[] array = (DataType[])GetData(new int[] { left }, new int[] { right - left });
								ArrayOrder o = OrderedArray1d.IsOrdered(array);
								if (o != cOrder)
								{
									proposedOrder = ArrayOrder.None;
									return ArrayOrder.None;
								}
							}
							if (right == committedShape) break;

							left = pieces[sortedPieces[current]].Origin[0] + pieces[sortedPieces[current]].Data.Length;

							do
							{
								int index = IndexOfDataPieceContaining(dataChanges, left);
								if (index >= 0)
									left = pieces[index].Origin[0] + pieces[index].Data.Length;
								else break;
							} while (true);

							for (current++; current < sortedPieces.Length; current++)
							{
								if (pieces[sortedPieces[current]].Origin[0] > left)
									break;
							}

							if (current < sortedPieces.Length)
							{
								right = pieces[sortedPieces[current]].Origin[0] - 1;
								if (left >= committedShape || right >= committedShape)
								{
									// ArrayOrder.None - there is a gap
									proposedOrder = ArrayOrder.None;
									return ArrayOrder.None;
								}
							}
							else
							{
								right = committedShape;
								if (left > right - 1) break;
							}
						}
					} // eof checking gaps with committed data

					proposedOrder = cOrder;
					return cOrder;
				} // eof dataPieces.Count > 0
			} // eof check of the proposed version
		}

		/// <summary>
		/// Gets the index of the data piece containg specified index or -1.
		/// </summary>
		private int IndexOfDataPieceContaining(DataChanges changes, int index)
		{
			for (int i = changes.Data.Count; --i >= 0; )
			{
				DataPiece piece = changes.Data[i];
				bool contains = index >= piece.Origin[0] &&
					index - piece.Origin[0] < piece.Data.GetLength(0);
				if (contains) return i;
			}
			return -1;
		}

		private static ArrayOrder GetOrder(DataType a, DataType b, Comparer<DataType> cmp)
		{
			int res = cmp.Compare(b, a);
			if (res == 0) return ArrayOrder.None;
			return (res > 0) ? ArrayOrder.Ascendant : ArrayOrder.Descendant;
		}

	}
}

