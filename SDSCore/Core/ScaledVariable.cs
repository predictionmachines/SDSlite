// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Microsoft.Research.Science.Data
{
	/// <summary>
	/// Represents a variable which values are linear function of another underlying variable.
	/// </summary>
	/// <typeparam name="DataType">Natural data representation: Single, Double or DateTime</typeparam>
	/// <typeparam name="RawType">Storage data representation: Single, Double, or any integer type except Decimal and UInt64</typeparam>
	/// <remarks>
	/// <para>Changes to this variable propagate to the underlying variable. 
	/// Changes to the underlying variable are reflected in this variable.</para>
	/// </remarks>
	internal class ScaledVariable<DataType, RawType> : TransformationVariable<DataType, RawType>
		where DataType : IConvertible
		where RawType : IConvertible
	{
		private DataType scale;
		private DataType offset;

		/// <summary>
		/// Initializes an instance of ScaledVariable.
		/// </summary>
		/// <param name="rawVariable">Underlying storage variable</param>
		/// <param name="scale">Scale factor</param>
		/// <param name="offset">Offset</param>
		/// <param name="name">Scaled variable name</param>
		/// <param name="hiddenEntries">Attributes that are not shared with the source variable.</param>
		/// <param name="readonlyEntries">Shared with source variable attributes that cannot be changed through this variable.</param>
		/// <remarks>
		/// <para>Only limited set of primitive blittable type parameters are supported by this generic variable. 
		/// Use <see cref="LambdaTransformVariable{DataType, RawType}"/> for other data types.</para>
		/// </remarks>
        internal ScaledVariable(Variable<RawType> rawVariable, DataType scale, DataType offset, string name, IList<string> hiddenEntries, IList<string> readonlyEntries)
			: base(rawVariable.DataSet, name, rawVariable, rawVariable.Dimensions.AsNamesArray(),
				hiddenEntries, readonlyEntries)
		{
			if (typeof(DataType) != typeof(double) &&
				typeof(DataType) != typeof(float))
				throw new NotImplementedException("Only Single and Double are supported as a DataType type parameter for a ScaledVariable");
			if (typeof(RawType) != typeof(byte) &&
				typeof(RawType) != typeof(sbyte) &&
				typeof(RawType) != typeof(short) &&
				typeof(RawType) != typeof(ushort) &&
				typeof(RawType) != typeof(int) &&
				typeof(RawType) != typeof(uint) &&
				typeof(RawType) != typeof(long) &&
				typeof(RawType) != typeof(ulong) &&
				typeof(RawType) != typeof(double) &&
				typeof(RawType) != typeof(float))
				throw new NotImplementedException("Only Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, Single and Double are supported as a RawType type parameter for a ScaledVariable");
			IsReadOnly = rawVariable.IsReadOnly;

			this.scale = scale;
			this.offset = offset;

			Initialize();
		}

		#region Transformations

		protected override Array Transform(int[] origin, Array rawData)
		{
			int[] shape = new int[rawData.Rank];
			for (int i = 0; i < shape.Length; i++)
				shape[i] = rawData.GetLength(i);

			Array data = Array.CreateInstance(typeof(DataType), shape);
			System.Diagnostics.Debug.Assert(rawData.LongLength == data.LongLength);

			unsafe
			{
				GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
				IntPtr dataPtr = dataHandle.AddrOfPinnedObject();
				GCHandle rawHandle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
				IntPtr rawPtr = rawHandle.AddrOfPinnedObject();
				switch (Type.GetTypeCode(typeof(DataType)))
				{
					case TypeCode.Double:
						{
							double typedOffset = offset.ToDouble(null);
							double typedScale = scale.ToDouble(null);
							switch (Type.GetTypeCode(typeof(RawType)))
							{
								case TypeCode.Byte:
									if (this.SourceVariable.MissingValue == null)
										scaling(data.LongLength, (byte*)rawPtr, (double*)dataPtr, typedScale, typedOffset);
									else scaling(data.LongLength, (byte*)rawPtr, (double*)dataPtr, typedScale, typedOffset, (byte)this.SourceVariable.MissingValue);
									break;
								case TypeCode.Double:
									if (this.SourceVariable.MissingValue == null)
										scaling(data.LongLength, (double*)rawPtr, (double*)dataPtr, typedScale, typedOffset);
									else scaling(data.LongLength, (double*)rawPtr, (double*)dataPtr, typedScale, typedOffset, (double)this.SourceVariable.MissingValue);
									break;
								case TypeCode.Int16:
									if (this.SourceVariable.MissingValue == null)
										scaling(data.LongLength, (short*)rawPtr, (double*)dataPtr, typedScale, typedOffset);
									else scaling(data.LongLength, (short*)rawPtr, (double*)dataPtr, typedScale, typedOffset, (short)this.SourceVariable.MissingValue);
									break;
								case TypeCode.Int32:
									if (this.SourceVariable.MissingValue == null)
										scaling(data.LongLength, (int*)rawPtr, (double*)dataPtr, typedScale, typedOffset);
									else scaling(data.LongLength, (int*)rawPtr, (double*)dataPtr, typedScale, typedOffset, (int)this.SourceVariable.MissingValue);
									break;
								case TypeCode.Int64:
									if (this.SourceVariable.MissingValue == null)
										scaling(data.LongLength, (long*)rawPtr, (double*)dataPtr, typedScale, typedOffset);
									else scaling(data.LongLength, (long*)rawPtr, (double*)dataPtr, typedScale, typedOffset, (long)this.SourceVariable.MissingValue);
									break;
								case TypeCode.SByte:
									if (this.SourceVariable.MissingValue == null)
										scaling(data.LongLength, (sbyte*)rawPtr, (double*)dataPtr, typedScale, typedOffset);
									else scaling(data.LongLength, (sbyte*)rawPtr, (double*)dataPtr, typedScale, typedOffset, (sbyte)this.SourceVariable.MissingValue);
									break;
								case TypeCode.Single:
									if (this.SourceVariable.MissingValue == null)
										scaling(data.LongLength, (float*)rawPtr, (double*)dataPtr, typedScale, typedOffset);
									else scaling(data.LongLength, (float*)rawPtr, (double*)dataPtr, typedScale, typedOffset, (float)this.SourceVariable.MissingValue);
									break;
								case TypeCode.UInt16:
									if (this.SourceVariable.MissingValue == null)
										scaling(data.LongLength, (ushort*)rawPtr, (double*)dataPtr, typedScale, typedOffset);
									else scaling(data.LongLength, (ushort*)rawPtr, (double*)dataPtr, typedScale, typedOffset, (ushort)this.SourceVariable.MissingValue);
									break;
								case TypeCode.UInt32:
									if (this.SourceVariable.MissingValue == null)
										scaling(data.LongLength, (uint*)rawPtr, (double*)dataPtr, typedScale, typedOffset);
									else scaling(data.LongLength, (uint*)rawPtr, (double*)dataPtr, typedScale, typedOffset, (uint)this.SourceVariable.MissingValue);
									break;
								case TypeCode.UInt64:
									if (this.SourceVariable.MissingValue == null)
										scaling(data.LongLength, (ulong*)rawPtr, (double*)dataPtr, typedScale, typedOffset);
									else scaling(data.LongLength, (ulong*)rawPtr, (double*)dataPtr, typedScale, typedOffset, (ulong)this.SourceVariable.MissingValue);
									break;
								default:
									dataHandle.Free();
									rawHandle.Free();
									throw new NotSupportedException("ScaledVariable is not supported for this combination of DataType and RawType");
							}
						}
						break;
					case TypeCode.Single:
						{
							float typedOffset = offset.ToSingle(null);
							float typedScale = scale.ToSingle(null);
							switch (Type.GetTypeCode(typeof(RawType)))
							{
								case TypeCode.Byte:
									if (this.SourceVariable.MissingValue == null)
										scaling(data.LongLength, (byte*)rawPtr, (float*)dataPtr, typedScale, typedOffset);
									else scaling(data.LongLength, (byte*)rawPtr, (float*)dataPtr, typedScale, typedOffset, (byte)this.SourceVariable.MissingValue);
									break;
								case TypeCode.Double:
									if (this.SourceVariable.MissingValue == null)
										scaling(data.LongLength, (double*)rawPtr, (float*)dataPtr, typedScale, typedOffset);
									else scaling(data.LongLength, (double*)rawPtr, (float*)dataPtr, typedScale, typedOffset, (double)this.SourceVariable.MissingValue);
									break;
								case TypeCode.Int16:
									if (this.SourceVariable.MissingValue == null)
										scaling(data.LongLength, (short*)rawPtr, (float*)dataPtr, typedScale, typedOffset);
									else scaling(data.LongLength, (short*)rawPtr, (float*)dataPtr, typedScale, typedOffset, (short)this.SourceVariable.MissingValue);
									break;
								case TypeCode.Int32:
									if (this.SourceVariable.MissingValue == null)
										scaling(data.LongLength, (int*)rawPtr, (float*)dataPtr, typedScale, typedOffset);
									else scaling(data.LongLength, (int*)rawPtr, (float*)dataPtr, typedScale, typedOffset, (int)this.SourceVariable.MissingValue);
									break;
								case TypeCode.Int64:
									if (this.SourceVariable.MissingValue == null)
										scaling(data.LongLength, (long*)rawPtr, (float*)dataPtr, typedScale, typedOffset);
									else scaling(data.LongLength, (long*)rawPtr, (float*)dataPtr, typedScale, typedOffset, (long)this.SourceVariable.MissingValue);
									break;
								case TypeCode.SByte:
									if (this.SourceVariable.MissingValue == null)
										scaling(data.LongLength, (sbyte*)rawPtr, (float*)dataPtr, typedScale, typedOffset);
									else scaling(data.LongLength, (sbyte*)rawPtr, (float*)dataPtr, typedScale, typedOffset, (sbyte)this.SourceVariable.MissingValue);
									break;
								case TypeCode.Single:
									if (this.SourceVariable.MissingValue == null)
										scaling(data.LongLength, (float*)rawPtr, (float*)dataPtr, typedScale, typedOffset);
									else scaling(data.LongLength, (float*)rawPtr, (float*)dataPtr, typedScale, typedOffset, (float)this.SourceVariable.MissingValue);
									break;
								case TypeCode.UInt16:
									if (this.SourceVariable.MissingValue == null)
										scaling(data.LongLength, (ushort*)rawPtr, (float*)dataPtr, typedScale, typedOffset);
									else scaling(data.LongLength, (ushort*)rawPtr, (float*)dataPtr, typedScale, typedOffset, (ushort)this.SourceVariable.MissingValue);
									break;
								case TypeCode.UInt32:
									if (this.SourceVariable.MissingValue == null)
										scaling(data.LongLength, (uint*)rawPtr, (float*)dataPtr, typedScale, typedOffset);
									else scaling(data.LongLength, (uint*)rawPtr, (float*)dataPtr, typedScale, typedOffset, (uint)this.SourceVariable.MissingValue);
									break;
								case TypeCode.UInt64:
									if (this.SourceVariable.MissingValue == null)
										scaling(data.LongLength, (ulong*)rawPtr, (float*)dataPtr, typedScale, typedOffset);
									else scaling(data.LongLength, (ulong*)rawPtr, (float*)dataPtr, typedScale, typedOffset, (ulong)this.SourceVariable.MissingValue);
									break;
								default:
									dataHandle.Free();
									rawHandle.Free();
									throw new NotSupportedException("ScaledVariable is not supported for this combination of DataType and RawType");
							}
						}
						break;
					default:
						dataHandle.Free();
						rawHandle.Free();
						throw new NotSupportedException("ScaledVariable is not supported for this DataType");
				}
			}
			return data;
		}

		protected override Array ReverseTransform(int[] origin, Array data)
		{
			int rank = data.Rank;
			int[] shape = new int[rank];
			for (int i = 0; i < shape.Length; i++)
				shape[i] = data.GetLength(i);

			Array rawData = Array.CreateInstance(typeof(RawType), shape);
			System.Diagnostics.Debug.Assert(rawData.LongLength == data.LongLength);

			unsafe
			{
				GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
				IntPtr dataPtr = dataHandle.AddrOfPinnedObject();
				GCHandle rawHandle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
				IntPtr rawPtr = rawHandle.AddrOfPinnedObject();
				switch (Type.GetTypeCode(typeof(DataType)))
				{
					case TypeCode.Double:
						{
							double typedOffset = offset.ToDouble(null);
							double typedScale = scale.ToDouble(null);
							switch (Type.GetTypeCode(typeof(RawType)))
							{
								case TypeCode.Byte:
									if (this.MissingValue == null)
										descaling(data.LongLength, (byte*)rawPtr, (double*)dataPtr, typedScale, typedOffset);
									else descaling(data.LongLength, (byte*)rawPtr, (double*)dataPtr, typedScale, typedOffset, (byte)this.MissingValue);
									break;
								case TypeCode.Double:
									if (this.MissingValue == null)
										descaling(data.LongLength, (double*)rawPtr, (double*)dataPtr, typedScale, typedOffset);
									else descaling(data.LongLength, (double*)rawPtr, (double*)dataPtr, typedScale, typedOffset, (double)this.MissingValue);
									break;
								case TypeCode.Int16:
									if (this.MissingValue == null)
										descaling(data.LongLength, (short*)rawPtr, (double*)dataPtr, typedScale, typedOffset);
									else descaling(data.LongLength, (short*)rawPtr, (double*)dataPtr, typedScale, typedOffset, (short)this.MissingValue);
									break;
								case TypeCode.Int32:
									if (this.MissingValue == null)
										descaling(data.LongLength, (int*)rawPtr, (double*)dataPtr, typedScale, typedOffset);
									else descaling(data.LongLength, (int*)rawPtr, (double*)dataPtr, typedScale, typedOffset, (int)this.MissingValue);
									break;
								case TypeCode.Int64:
									if (this.MissingValue == null)
										descaling(data.LongLength, (long*)rawPtr, (double*)dataPtr, typedScale, typedOffset);
									else descaling(data.LongLength, (long*)rawPtr, (double*)dataPtr, typedScale, typedOffset, (long)this.MissingValue);
									break;
								case TypeCode.SByte:
									if (this.MissingValue == null)
										descaling(data.LongLength, (sbyte*)rawPtr, (double*)dataPtr, typedScale, typedOffset);
									else descaling(data.LongLength, (sbyte*)rawPtr, (double*)dataPtr, typedScale, typedOffset, (sbyte)this.MissingValue);
									break;
								case TypeCode.Single:
									if (this.MissingValue == null)
										descaling(data.LongLength, (float*)rawPtr, (double*)dataPtr, typedScale, typedOffset);
									else descaling(data.LongLength, (float*)rawPtr, (double*)dataPtr, typedScale, typedOffset, (float)this.MissingValue);
									break;
								case TypeCode.UInt16:
									if (this.MissingValue == null)
										descaling(data.LongLength, (ushort*)rawPtr, (double*)dataPtr, typedScale, typedOffset);
									else descaling(data.LongLength, (ushort*)rawPtr, (double*)dataPtr, typedScale, typedOffset, (ushort)this.MissingValue);
									break;
								case TypeCode.UInt32:
									if (this.MissingValue == null)
										descaling(data.LongLength, (uint*)rawPtr, (double*)dataPtr, typedScale, typedOffset);
									else descaling(data.LongLength, (uint*)rawPtr, (double*)dataPtr, typedScale, typedOffset, (uint)this.MissingValue);
									break;
								case TypeCode.UInt64:
									if (this.MissingValue == null)
										descaling(data.LongLength, (ulong*)rawPtr, (double*)dataPtr, typedScale, typedOffset);
									else descaling(data.LongLength, (ulong*)rawPtr, (double*)dataPtr, typedScale, typedOffset, (ulong)this.MissingValue);
									break;
								default:
									dataHandle.Free();
									rawHandle.Free();
									throw new NotSupportedException("ScaledVariable is not supported for this combination of DataType and RawType");
							}
							break;
						}
					case TypeCode.Single:
						{
							float typedOffset = offset.ToSingle(null);
							float typedScale = scale.ToSingle(null);
							switch (Type.GetTypeCode(typeof(RawType)))
							{
								case TypeCode.Byte:
									if (this.MissingValue == null)
										descaling(data.LongLength, (byte*)rawPtr, (float*)dataPtr, typedScale, typedOffset);
									else descaling(data.LongLength, (byte*)rawPtr, (float*)dataPtr, typedScale, typedOffset, (byte)this.MissingValue);
									break;
								case TypeCode.Double:
									if (this.MissingValue == null)
										descaling(data.LongLength, (double*)rawPtr, (float*)dataPtr, typedScale, typedOffset);
									else descaling(data.LongLength, (double*)rawPtr, (float*)dataPtr, typedScale, typedOffset, (float)(double)this.MissingValue);
									break;
								case TypeCode.Int16:
									if (this.MissingValue == null)
										descaling(data.LongLength, (short*)rawPtr, (float*)dataPtr, typedScale, typedOffset);
									else descaling(data.LongLength, (short*)rawPtr, (float*)dataPtr, typedScale, typedOffset, (short)this.MissingValue);
									break;
								case TypeCode.Int32:
									if (this.MissingValue == null)
										descaling(data.LongLength, (int*)rawPtr, (float*)dataPtr, typedScale, typedOffset);
									else descaling(data.LongLength, (int*)rawPtr, (float*)dataPtr, typedScale, typedOffset, (int)this.MissingValue);
									break;
								case TypeCode.Int64:
									if (this.MissingValue == null)
										descaling(data.LongLength, (long*)rawPtr, (float*)dataPtr, typedScale, typedOffset);
									else descaling(data.LongLength, (long*)rawPtr, (float*)dataPtr, typedScale, typedOffset, (long)this.MissingValue);
									break;
								case TypeCode.SByte:
									if (this.MissingValue == null)
										descaling(data.LongLength, (sbyte*)rawPtr, (float*)dataPtr, typedScale, typedOffset);
									else descaling(data.LongLength, (sbyte*)rawPtr, (float*)dataPtr, typedScale, typedOffset, (sbyte)this.MissingValue);
									break;
								case TypeCode.Single:
									if (this.MissingValue == null)
										descaling(data.LongLength, (float*)rawPtr, (float*)dataPtr, typedScale, typedOffset);
									else descaling(data.LongLength, (float*)rawPtr, (float*)dataPtr, typedScale, typedOffset, (float)this.MissingValue);
									break;
								case TypeCode.UInt16:
									if (this.MissingValue == null)
										descaling(data.LongLength, (ushort*)rawPtr, (float*)dataPtr, typedScale, typedOffset);
									else descaling(data.LongLength, (ushort*)rawPtr, (float*)dataPtr, typedScale, typedOffset, (ushort)this.MissingValue);
									break;
								case TypeCode.UInt32:
									if (this.MissingValue == null)
										descaling(data.LongLength, (uint*)rawPtr, (float*)dataPtr, typedScale, typedOffset);
									else descaling(data.LongLength, (uint*)rawPtr, (float*)dataPtr, typedScale, typedOffset, (uint)this.MissingValue);
									break;
								case TypeCode.UInt64:
									if (this.MissingValue == null)
										descaling(data.LongLength, (ulong*)rawPtr, (float*)dataPtr, typedScale, typedOffset);
									else descaling(data.LongLength, (ulong*)rawPtr, (float*)dataPtr, typedScale, typedOffset, (ulong)this.MissingValue);
									break;
								default:
									dataHandle.Free();
									rawHandle.Free();
									throw new NotSupportedException("ScaledVariable is not supported for this combination of DataType and RawType");
							}
							break;
						}
					default:
						dataHandle.Free();
						rawHandle.Free();
						throw new NotSupportedException("ScaledVariable is not supported for this DataType");
				}
				dataHandle.Free();
				rawHandle.Free();
			}
			return rawData;
		}

		#endregion

		#region Scaling and descaling routines

		private static unsafe void scaling(long length, double* from, double* to, double scale, double offset)
		{ while (length-- > 0) *to++ = *from++ * scale + offset; }

		private static unsafe void scaling(long length, double* from, double* to, double scale, double offset, double mv)
		{
			while (length-- > 0) if (*from != mv)
					*to++ = *from++ * scale + offset;
				else *to++ = *from++;
		}

		private static unsafe void scaling(long length, float* from, double* to, double scale, double offset)
		{ while (length-- > 0) *to++ = *from++ * scale + offset; }

		private static unsafe void scaling(long length, float* from, double* to, double scale, double offset, float mv)
		{
			while (length-- > 0) if (*from != mv)
					*to++ = *from++ * scale + offset;
				else *to++ = *from++;
		}

		private static unsafe void scaling(long length, byte* from, double* to, double scale, double offset)
		{ while (length-- > 0) *to++ = *from++ * scale + offset; }

		private static unsafe void scaling(long length, byte* from, double* to, double scale, double offset, byte mv)
		{
			while (length-- > 0) if (*from != mv)
					*to++ = *from++ * scale + offset;
				else *to++ = *from++;
		}

		private static unsafe void scaling(long length, sbyte* from, double* to, double scale, double offset)
		{ while (length-- > 0) *to++ = *from++ * scale + offset; }

		private static unsafe void scaling(long length, sbyte* from, double* to, double scale, double offset, sbyte mv)
		{
			while (length-- > 0) if (*from != mv)
					*to++ = *from++ * scale + offset;
				else *to++ = *from++;
		}

		private static unsafe void scaling(long length, short* from, double* to, double scale, double offset)
		{ while (length-- > 0) *to++ = *from++ * scale + offset; }

		private static unsafe void scaling(long length, short* from, double* to, double scale, double offset, short mv)
		{
			while (length-- > 0) if (*from != mv)
					*to++ = *from++ * scale + offset;
				else *to++ = *from++;
		}

		private static unsafe void scaling(long length, ushort* from, double* to, double scale, double offset)
		{ while (length-- > 0) *to++ = *from++ * scale + offset; }

		private static unsafe void scaling(long length, ushort* from, double* to, double scale, double offset, ushort mv)
		{
			while (length-- > 0) if (*from != mv)
					*to++ = *from++ * scale + offset;
				else *to++ = *from++;
		}

		private static unsafe void scaling(long length, int* from, double* to, double scale, double offset)
		{ while (length-- > 0) *to++ = *from++ * scale + offset; }

		private static unsafe void scaling(long length, int* from, double* to, double scale, double offset, int mv)
		{
			while (length-- > 0) if (*from != mv)
					*to++ = *from++ * scale + offset;
				else *to++ = *from++;
		}

		private static unsafe void scaling(long length, uint* from, double* to, double scale, double offset)
		{ while (length-- > 0) *to++ = *from++ * scale + offset; }

		private static unsafe void scaling(long length, uint* from, double* to, double scale, double offset, uint mv)
		{
			while (length-- > 0) if (*from != mv)
					*to++ = *from++ * scale + offset;
				else *to++ = *from++;
		}

		private static unsafe void scaling(long length, long* from, double* to, double scale, double offset)
		{ while (length-- > 0) *to++ = *from++ * scale + offset; }

		private static unsafe void scaling(long length, long* from, double* to, double scale, double offset, long mv)
		{
			while (length-- > 0) if (*from != mv)
					*to++ = *from++ * scale + offset;
				else *to++ = *from++;
		}

		private static unsafe void scaling(long length, ulong* from, double* to, double scale, double offset)
		{ while (length-- > 0) *to++ = *from++ * scale + offset; }

		private static unsafe void scaling(long length, ulong* from, double* to, double scale, double offset, ulong mv)
		{
			while (length-- > 0) if (*from != mv)
					*to++ = *from++ * scale + offset;
				else *to++ = *from++;
		}


		private static unsafe void scaling(long length, double* from, float* to, float scale, float offset)
		{ while (length-- > 0) *to++ = (float)(*from++ * scale + offset); }

		private static unsafe void scaling(long length, double* from, float* to, float scale, float offset, double mv)
		{
			while (length-- > 0) if (*from != mv)
					*to++ = (float)(*from++ * scale + offset);
				else *to++ = (float)*from++;
		}

		private static unsafe void scaling(long length, float* from, float* to, float scale, float offset)
		{ while (length-- > 0) *to++ = *from++ * scale + offset; }

		private static unsafe void scaling(long length, float* from, float* to, float scale, float offset, float mv)
		{
			while (length-- > 0) if (*from != mv)
					*to++ = *from++ * scale + offset;
				else *to++ = *from++;
		}

		private static unsafe void scaling(long length, byte* from, float* to, float scale, float offset)
		{ while (length-- > 0) *to++ = *from++ * scale + offset; }

		private static unsafe void scaling(long length, byte* from, float* to, float scale, float offset, byte mv)
		{
			while (length-- > 0) if (*from != mv)
					*to++ = *from++ * scale + offset;
				else *to++ = *from++;
		}

		private static unsafe void scaling(long length, sbyte* from, float* to, float scale, float offset)
		{ while (length-- > 0) *to++ = *from++ * scale + offset; }

		private static unsafe void scaling(long length, sbyte* from, float* to, float scale, float offset, sbyte mv)
		{
			while (length-- > 0) if (*from != mv)
					*to++ = *from++ * scale + offset;
				else *to++ = *from++;
		}

		private static unsafe void scaling(long length, short* from, float* to, float scale, float offset)
		{ while (length-- > 0) *to++ = *from++ * scale + offset; }

		private static unsafe void scaling(long length, short* from, float* to, float scale, float offset, short mv)
		{
			while (length-- > 0) if (*from != mv)
					*to++ = *from++ * scale + offset;
				else *to++ = *from++;
		}

		private static unsafe void scaling(long length, ushort* from, float* to, float scale, float offset)
		{ while (length-- > 0) *to++ = *from++ * scale + offset; }

		private static unsafe void scaling(long length, ushort* from, float* to, float scale, float offset, ushort mv)
		{
			while (length-- > 0) if (*from != mv)
					*to++ = *from++ * scale + offset;
				else *to++ = *from++;
		}

		private static unsafe void scaling(long length, int* from, float* to, float scale, float offset)
		{ while (length-- > 0) *to++ = *from++ * scale + offset; }

		private static unsafe void scaling(long length, int* from, float* to, float scale, float offset, int mv)
		{
			while (length-- > 0) if (*from != mv)
					*to++ = *from++ * scale + offset;
				else *to++ = *from++;
		}

		private static unsafe void scaling(long length, uint* from, float* to, float scale, float offset)
		{ while (length-- > 0) *to++ = *from++ * scale + offset; }

		private static unsafe void scaling(long length, uint* from, float* to, float scale, float offset, uint mv)
		{
			while (length-- > 0) if (*from != mv)
					*to++ = *from++ * scale + offset;
				else *to++ = *from++;
		}

		private static unsafe void scaling(long length, long* from, float* to, float scale, float offset)
		{ while (length-- > 0) *to++ = *from++ * scale + offset; }

		private static unsafe void scaling(long length, long* from, float* to, float scale, float offset, long mv)
		{
			while (length-- > 0) if (*from != mv)
					*to++ = *from++ * scale + offset;
				else *to++ = *from++;
		}

		private static unsafe void scaling(long length, ulong* from, float* to, float scale, float offset)
		{ while (length-- > 0) *to++ = *from++ * scale + offset; }

		private static unsafe void scaling(long length, ulong* from, float* to, float scale, float offset, ulong mv)
		{
			while (length-- > 0) if (*from != mv)
					*to++ = *from++ * scale + offset;
				else *to++ = *from++;
		}


		private static unsafe void descaling(long length, double* from, double* to, double scale, double offset)
		{ while (length-- > 0) *from++ = (*to++ - offset) / scale; }

		private static unsafe void descaling(long length, double* from, double* to, double scale, double offset, double mv)
		{
			while (length-- > 0) if (*to != mv)
					*from++ = (*to++ - offset) / scale;
				else *from++ = *to++;
		}

		private static unsafe void descaling(long length, float* from, double* to, double scale, double offset)
		{ while (length-- > 0) *from++ = (float)((*to++ - offset) / scale); }

		private static unsafe void descaling(long length, float* from, double* to, double scale, double offset, double mv)
		{
			while (length-- > 0) if (*to != mv)
					*from++ = (float)((*to++ - offset) / scale);
				else *from++ = (float)*to++;
		}

		private static unsafe void descaling(long length, byte* from, double* to, double scale, double offset)
		{ while (length-- > 0) *from++ = (byte)((*to++ - offset) / scale); }

		private static unsafe void descaling(long length, byte* from, double* to, double scale, double offset, double mv)
		{
			while (length-- > 0) if (*to != mv)
					*from++ = (byte)((*to++ - offset) / scale);
				else *from++ = (byte)*to++;
		}

		private static unsafe void descaling(long length, sbyte* from, double* to, double scale, double offset)
		{ while (length-- > 0) *from++ = (sbyte)((*to++ - offset) / scale); }

		private static unsafe void descaling(long length, sbyte* from, double* to, double scale, double offset, double mv)
		{
			while (length-- > 0) if (*to != mv)
					*from++ = (sbyte)((*to++ - offset) / scale);
				else *from++ = (sbyte)*to++;
		}

		private static unsafe void descaling(long length, short* from, double* to, double scale, double offset)
		{ while (length-- > 0) *from++ = (short)((*to++ - offset) / scale); }

		private static unsafe void descaling(long length, short* from, double* to, double scale, double offset, double mv)
		{
			while (length-- > 0) if (*to != mv)
					*from++ = (short)((*to++ - offset) / scale);
				else *from++ = (short)*to++;
		}

		private static unsafe void descaling(long length, ushort* from, double* to, double scale, double offset)
		{ while (length-- > 0) *from++ = (ushort)((*to++ - offset) / scale); }

		private static unsafe void descaling(long length, ushort* from, double* to, double scale, double offset, double mv)
		{
			while (length-- > 0) if (*to != mv)
					*from++ = (ushort)((*to++ - offset) / scale);
				else *from++ = (ushort)*to++;
		}

		private static unsafe void descaling(long length, int* from, double* to, double scale, double offset)
		{ while (length-- > 0) *from++ = (int)((*to++ - offset) / scale); }

		private static unsafe void descaling(long length, int* from, double* to, double scale, double offset, double mv)
		{
			while (length-- > 0) if (*to != mv)
					*from++ = (int)((*to++ - offset) / scale);
				else *from++ = (int)*to++;
		}

		private static unsafe void descaling(long length, uint* from, double* to, double scale, double offset)
		{ while (length-- > 0) *from++ = (uint)((*to++ - offset) / scale); }

		private static unsafe void descaling(long length, uint* from, double* to, double scale, double offset, double mv)
		{
			while (length-- > 0) if (*to != mv)
					*from++ = (uint)((*to++ - offset) / scale);
				else *from++ = (uint)*to++;
		}

		private static unsafe void descaling(long length, long* from, double* to, double scale, double offset)
		{ while (length-- > 0) *from++ = (long)((*to++ - offset) / scale); }

		private static unsafe void descaling(long length, long* from, double* to, double scale, double offset, double mv)
		{
			while (length-- > 0) if (*to != mv)
					*from++ = (long)((*to++ - offset) / scale);
				else *from++ = (long)*to++;
		}

		private static unsafe void descaling(long length, ulong* from, double* to, double scale, double offset)
		{ while (length-- > 0) *from++ = (ulong)((*to++ - offset) / scale); }

		private static unsafe void descaling(long length, ulong* from, double* to, double scale, double offset, double mv)
		{
			while (length-- > 0) if (*to != mv)
					*from++ = (ulong)((*to++ - offset) / scale);
				else *from++ = (ulong)*to++;
		}


		private static unsafe void descaling(long length, double* from, float* to, float scale, float offset)
		{ while (length-- > 0) *from++ = (*to++ - offset) / scale; }

		private static unsafe void descaling(long length, double* from, float* to, float scale, float offset, float mv)
		{
			while (length-- > 0) if (*to != mv)
					*from++ = ((*to++ - offset) / scale);
				else *from++ = *to++;
		}

		private static unsafe void descaling(long length, float* from, float* to, float scale, float offset)
		{ while (length-- > 0) *from++ = (*to++ - offset) / scale; }

		private static unsafe void descaling(long length, float* from, float* to, float scale, float offset, float mv)
		{
			while (length-- > 0) if (*to != mv)
					*from++ = ((*to++ - offset) / scale);
				else *from++ = *to++;
		}

		private static unsafe void descaling(long length, byte* from, float* to, float scale, float offset)
		{ while (length-- > 0) *from++ = (byte)((*to++ - offset) / scale); }

		private static unsafe void descaling(long length, byte* from, float* to, float scale, float offset, float mv)
		{
			while (length-- > 0) if (*to != mv)
					*from++ = (byte)((*to++ - offset) / scale);
				else *from++ = (byte)*to++;
		}

		private static unsafe void descaling(long length, sbyte* from, float* to, float scale, float offset)
		{ while (length-- > 0) *from++ = (sbyte)((*to++ - offset) / scale); }

		private static unsafe void descaling(long length, sbyte* from, float* to, float scale, float offset, float mv)
		{
			while (length-- > 0) if (*to != mv)
					*from++ = (sbyte)((*to++ - offset) / scale);
				else *from++ = (sbyte)*to++;
		}

		private static unsafe void descaling(long length, short* from, float* to, float scale, float offset)
		{ while (length-- > 0) *from++ = (short)((*to++ - offset) / scale); }

		private static unsafe void descaling(long length, short* from, float* to, float scale, float offset, float mv)
		{
			while (length-- > 0) if (*to != mv)
					*from++ = (short)((*to++ - offset) / scale);
				else *from++ = (short)*to++;
		}

		private static unsafe void descaling(long length, ushort* from, float* to, float scale, float offset)
		{ while (length-- > 0) *from++ = (ushort)((*to++ - offset) / scale); }

		private static unsafe void descaling(long length, ushort* from, float* to, float scale, float offset, float mv)
		{
			while (length-- > 0) if (*to != mv)
					*from++ = (ushort)((*to++ - offset) / scale);
				else *from++ = (ushort)*to++;
		}

		private static unsafe void descaling(long length, int* from, float* to, float scale, float offset)
		{ while (length-- > 0) *from++ = (int)((*to++ - offset) / scale); }

		private static unsafe void descaling(long length, int* from, float* to, float scale, float offset, float mv)
		{
			while (length-- > 0) if (*to != mv)
					*from++ = (int)((*to++ - offset) / scale);
				else *from++ = (int)*to++;
		}

		private static unsafe void descaling(long length, uint* from, float* to, float scale, float offset)
		{ while (length-- > 0) *from++ = (uint)((*to++ - offset) / scale); }

		private static unsafe void descaling(long length, uint* from, float* to, float scale, float offset, float mv)
		{
			while (length-- > 0) if (*to != mv)
					*from++ = (uint)((*to++ - offset) / scale);
				else *from++ = (uint)*to++;
		}

		private static unsafe void descaling(long length, long* from, float* to, float scale, float offset)
		{ while (length-- > 0) *from++ = (long)((*to++ - offset) / scale); }

		private static unsafe void descaling(long length, long* from, float* to, float scale, float offset, float mv)
		{
			while (length-- > 0) if (*to != mv)
					*from++ = (long)((*to++ - offset) / scale);
				else *from++ = (long)*to++;
		}

		private static unsafe void descaling(long length, ulong* from, float* to, float scale, float offset)
		{ while (length-- > 0) *from++ = (ulong)((*to++ - offset) / scale); }

		private static unsafe void descaling(long length, ulong* from, float* to, float scale, float offset, float mv)
		{
			while (length-- > 0) if (*to != mv)
					*from++ = (ulong)((*to++ - offset) / scale);
				else *from++ = (ulong)*to++;
		}

		#endregion
	}
}

