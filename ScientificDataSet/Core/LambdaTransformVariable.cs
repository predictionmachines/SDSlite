// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Research.Science.Data
{
	internal class LambdaTransformVariable<DataType, RawType> : TransformationVariable<DataType, RawType>
	{
		private readonly Func<RawType, DataType> forwardLambda;
		private readonly Func<DataType, RawType> backwardLambda;

		internal LambdaTransformVariable(
			string name,
			Variable<RawType> rawVariable,
			Func<RawType, DataType> forwardTransform,
			Func<DataType, RawType> backwardTransform,
			IList<string> hiddenEntries,
			IList<string> readonlyEntries)
			: base(rawVariable.DataSet, name, rawVariable, rawVariable.Dimensions.AsNamesArray(), hiddenEntries, readonlyEntries)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (rawVariable == null) throw new ArgumentNullException("rawVariable");
			if (forwardTransform == null) throw new ArgumentNullException("forwardTransform");
			if (rawVariable.Rank > 6)
				throw new NotSupportedException("LambdaTransformVariable doesn't support ranks more than 6");

			forwardLambda = forwardTransform;
			backwardLambda = backwardTransform;

			Initialize();
			if (backwardTransform == null)
				IsReadOnly = true;
		}
		protected override Array Transform(int[] origin, Array rawData)
		{
			int[] shape = new int[rawData.Rank];
			for (int i = 0; i < shape.Length; i++)
				shape[i] = rawData.GetLength(i);

			Array data = Array.CreateInstance(typeof(DataType), shape);
			System.Diagnostics.Debug.Assert(rawData.LongLength == data.LongLength);
			switch (rawData.Rank)
			{
				case 1:
					loop(forwardLambda, (RawType[])rawData, (DataType[])data, shape[0]);
					break;
				case 2:
					loop(forwardLambda, (RawType[,])rawData, (DataType[,])data, shape[0], shape[1]);
					break;
				case 3:
					loop(forwardLambda, (RawType[, ,])rawData, (DataType[, ,])data, shape[0], shape[1], shape[2]);
					break;
				case 4:
					loop(forwardLambda, (RawType[, , ,])rawData, (DataType[, , ,])data, shape[0], shape[1], shape[2], shape[3]);
					break;
				case 5:
					loop(forwardLambda, (RawType[, , , ,])rawData, (DataType[, , , ,])data, shape[0], shape[1], shape[2], shape[3], shape[4]);
					break;
				case 6:
					loop(forwardLambda, (RawType[, , , , ,])rawData, (DataType[, , , , ,])data, shape[0], shape[1], shape[2], shape[3], shape[4], shape[5]);
					break;
				default:
					throw new NotSupportedException("LambdaTransformVariable doesn't support ranks more than 6");
			}
			return data;
		}

		protected override Array ReverseTransform(int[] origin, Array data)
		{
			if (backwardLambda == null)
				throw new InvalidOperationException("Backward transformation wasn't supplied for LambdaTransformationVariable");
			int rank = data.Rank;
			int[] shape = new int[rank];
			for (int i = 0; i < shape.Length; i++)
				shape[i] = data.GetLength(i);

			Array rawData = Array.CreateInstance(typeof(RawType), shape);
			System.Diagnostics.Debug.Assert(rawData.LongLength == data.LongLength);
			switch (rank)
			{
				case 1:
					loop(backwardLambda, (DataType[])data, (RawType[])rawData, shape[0]);
					break;
				case 2:
					loop(backwardLambda, (DataType[,])data, (RawType[,])rawData, shape[0], shape[1]);
					break;
				case 3:
					loop(backwardLambda, (DataType[, ,])data, (RawType[, ,])rawData, shape[0], shape[1], shape[2]);
					break;
				case 4:
					loop(backwardLambda, (DataType[, , ,])data, (RawType[, , ,])rawData, shape[0], shape[1], shape[2], shape[3]);
					break;
				case 5:
					loop(backwardLambda, (DataType[, , , ,])data, (RawType[, , , ,])rawData, shape[0], shape[1], shape[2], shape[3], shape[4]);
					break;
				case 6:
					loop(backwardLambda, (DataType[, , , , ,])data, (RawType[, , , , ,])rawData, shape[0], shape[1], shape[2], shape[3], shape[4], shape[5]);
					break;
				default:
					throw new NotSupportedException("LambdaTransformVariable doesn't support ranks more than 6");
			}
			return rawData;
		}

		#region loop routines
		private void loop<FROM, TO>(Func<FROM, TO> lambda, FROM[] from, TO[] to, int l0)
		{
			for (int i0 = 0; i0 < l0; i0++)
				to[i0] = lambda(from[i0]);
		}
		private void loop<FROM, TO>(Func<FROM, TO> lambda, FROM[,] from, TO[,] to, int l0, int l1)
		{
			for (int i0 = 0; i0 < l0; i0++)
				for (int i1 = 0; i1 < l1; i1++)
					to[i0, i1] = lambda(from[i0, i1]);
		}
		private void loop<FROM, TO>(Func<FROM, TO> lambda, FROM[, ,] from, TO[, ,] to, int l0, int l1, int l2)
		{
			for (int i0 = 0; i0 < l0; i0++)
				for (int i1 = 0; i1 < l1; i1++)
					for (int i2 = 0; i2 < l2; i2++)
						to[i0, i1, i2] = lambda(from[i0, i1, i2]);
		}
		private void loop<FROM, TO>(Func<FROM, TO> lambda, FROM[, , ,] from, TO[, , ,] to, int l0, int l1, int l2, int l3)
		{
			for (int i0 = 0; i0 < l0; i0++)
				for (int i1 = 0; i1 < l1; i1++)
					for (int i2 = 0; i2 < l2; i2++)
						for (int i3 = 0; i3 < l3; i3++)
							to[i0, i1, i2, i3] = lambda(from[i0, i1, i2, i3]);
		}
		private void loop<FROM, TO>(Func<FROM, TO> lambda, FROM[, , , ,] from, TO[, , , ,] to, int l0, int l1, int l2, int l3, int l4)
		{
			for (int i0 = 0; i0 < l0; i0++)
				for (int i1 = 0; i1 < l1; i1++)
					for (int i2 = 0; i2 < l2; i2++)
						for (int i3 = 0; i3 < l3; i3++)
							for (int i4 = 0; i4 < l4; i4++)
								to[i0, i1, i2, i3, i4] = lambda(from[i0, i1, i2, i3, i4]);
		}
		private void loop<FROM, TO>(Func<FROM, TO> lambda, FROM[, , , , ,] from, TO[, , , , ,] to, int l0, int l1, int l2, int l3, int l4, int l5)
		{
			for (int i0 = 0; i0 < l0; i0++)
				for (int i1 = 0; i1 < l1; i1++)
					for (int i2 = 0; i2 < l2; i2++)
						for (int i3 = 0; i3 < l3; i3++)
							for (int i4 = 0; i4 < l4; i4++)
								for (int i5 = 0; i5 < l5; i5++)
									to[i0, i1, i2, i3, i4, i5] = lambda(from[i0, i2, i3, i4, i4, i5]);
		}
		#endregion

	}
}

