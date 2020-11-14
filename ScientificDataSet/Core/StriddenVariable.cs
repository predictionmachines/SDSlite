// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Microsoft.Research.Science.Data
{
    /// <summary>
    /// The computational variable that represents a result of striding of another variable.
    /// </summary>
    /// <typeparam name="DataType">Type of a data element.</typeparam>
    internal class StriddenVariable<DataType> : TransformationVariable<DataType, DataType>, IPrivateStriddenVariable, IStriddenVariable
    {
        private int[] origin;
        private int[] stride;
        private int[] count;

        /// <summary>
        /// Instantiates and returns a StriddenVariable object.
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="dims"></param>
        /// <param name="name"></param>
        /// <param name="sourceVariable">The variable that is a data source for the computation.</param>
        /// <param name="origin">origin indices for striding.</param>
        /// <param name="stride">A stride.</param>
        /// <param name="count">A number of values to get from the source variable. If null, always takes as much as possible.</param>
        /// <param name="hiddenEntries">Attributes that are not shared with the source variable.</param>
        /// <param name="readonlyEntries">Shared with source variable attributes that cannot be changed through this variable.</param>
        internal StriddenVariable(DataSet dataSet, string name, string[] dims, Variable<DataType> sourceVariable, int[] origin, int[] stride, int[] count,
            IList<string> hiddenEntries, IList<string> readonlyEntries)
            : base(dataSet, name, sourceVariable, dims, PrepareHiddenEntries(hiddenEntries), PrepareReadonlyEntries(readonlyEntries))
        {
            int rank = sourceVariable.Rank;
            if (origin == null)
            {
                origin = new int[rank]; // filled with zeros
            }
            if (stride == null)
            {
                stride = new int[rank];
                for (int i = 0; i < rank; i++) stride[i] = 1;
            }
            if (count == null)
            {
                count = new int[rank];
                for (int i = 0; i < rank; i++)
                    if (stride[i] == 0) count[i] = 1; // else equals to 0
            }

            if (origin.Length != rank)
                throw new ArgumentException("Wrong origin indices");
            if (stride.Length != rank)
                throw new ArgumentException("Wrong stride indices");
            if (count.Length != rank)
                throw new ArgumentException("Wrong stride indices");

            for (int i = 0; i < rank; i++)
            {
                if (origin[i] < 0) throw new ArgumentOutOfRangeException("origin is negative");
                if (stride[i] < 0) throw new ArgumentOutOfRangeException("stride is negative");
                if (count != null && count[i] < 0) throw new ArgumentOutOfRangeException("count is negative");
                if (stride[i] == 0 && count[i] != 1) throw new Exception("count[" + i + "] must be equal 1 because stride[" + i.ToString() + "] =0");
            }

            this.origin = origin;
            this.stride = stride;
            this.count = count;

            SetMetadata(origin, stride, count, sourceVariable);

            // Initializing the schema
            IsReadOnly = true; // it is not reversible transformation!

            Initialize();
        }

        /// <summary>
        /// Adds special metadata attributes describing the striding. See also StriddenVariableKeys class.
        /// </summary>
        /// <param name="hiddenEntries"></param>
        /// <returns>Updates entries.</returns>
        private static IList<string> PrepareHiddenEntries(IList<string> hiddenEntries)
        {
            // Mandatory attributes
            string[] mandatory = { StriddenVariableKeys.KeyForOrigin,
                                    StriddenVariableKeys.KeyForCount,
                                    StriddenVariableKeys.KeyForStride,
                                    StriddenVariableKeys.KeyForSourceDims};
            return AddMandatory(hiddenEntries, mandatory);
        }

        /// <summary>
        /// Adds special metadata attributes describing the striding. See also StriddenVariableKeys class.
        /// </summary>
        /// <param name="readonlyEntries"></param>
        /// <returns>Updates entries.</returns>
        private static IList<string> PrepareReadonlyEntries(IList<string> readonlyEntries)
        {
            // Mandatory attributes
            string[] mandatory = {  StriddenVariableKeys.KeyForCount,
                                     StriddenVariableKeys.KeyForStride,
                                     StriddenVariableKeys.KeyForSourceDims };
            return AddMandatory(readonlyEntries, mandatory);
        }

        private static IList<string> AddMandatory(IList<string> current, IList<string> mandatory)
        {
            if (current == null) return mandatory;
            else
            {
                List<string> newHidden = new List<string>(current);
                foreach (var attr in mandatory)
                {
                    if (!newHidden.Contains(attr))
                        newHidden.Add(attr);
                }
                return newHidden;
            }
        }

        void IPrivateStriddenVariable.AddCoordinateSystem(CoordinateSystem cs)
        {
            IsReadOnly = false;
            base.AddCoordinateSystem(cs);
            IsReadOnly = true;
        }

        void IStriddenVariable.SetIndexSpaceOrigin(int[] origin)
        {
            this.Metadata[StriddenVariableKeys.KeyForOrigin] = origin;
        }

        private void SetMetadata(int[] origin, int[] stride, int[] count, Variable sourceVar)
        {
            this.Metadata[StriddenVariableKeys.KeyForOrigin] = origin;
            this.Metadata[StriddenVariableKeys.KeyForStride] = stride;
            this.Metadata[StriddenVariableKeys.KeyForCount] = count;
            this.Metadata[StriddenVariableKeys.KeyForSourceDims] = sourceVar.dimensions;
        }

        protected override void UpdateChanges(DataSet.Changes changes)
        {
            base.UpdateChanges(changes);

            Variable.Changes varChanges = changes.GetVariableChanges(ID);
            if (varChanges != null && varChanges.MetadataChanges.ContainsKey(StriddenVariableKeys.KeyForOrigin))
            {
                int[] prOrigin;
                object mvalue = varChanges.MetadataChanges[StriddenVariableKeys.KeyForOrigin];
                if (mvalue == null)
                {
                    prOrigin = null;
                }
                else
                {
                    int[] t = mvalue as int[];
                    if (t == null)
                        throw new ConstraintsFailedException("Value for attribute \"" + StriddenVariableKeys.KeyForOrigin + "\" must be int[" + SourceVariable.Rank + "]");
                    if (t.Length != SourceVariable.Rank)
                        throw new ConstraintsFailedException("Value for attribute \"" + StriddenVariableKeys.KeyForOrigin + "\" must be int[" + SourceVariable.Rank + "]");
                    prOrigin = t;
                }
                try
                {
                    InnerUpdateChanges(changes, prOrigin);
                }
                catch (Exception ex)
                {
                    throw new ConstraintsFailedException(StriddenVariableKeys.KeyForOrigin + " attribute value is incorrect", ex);
                }
            }
        }

        protected internal override void OnCommit(Variable.Changes proposedChanges)
        {
            base.OnCommit(proposedChanges);
            //SetStride() version
            this.origin = (int[])Metadata[StriddenVariableKeys.KeyForOrigin];
            if (this.origin == null)
                this.origin = new int[Rank]; // zeros
        }

        #region Transformations

        protected override Rectangle TransformIndexRectangle(Rectangle srcAffectedRect)
        {
            if (srcAffectedRect.Origin == null)
                return new Rectangle(Rank);

            int[] aorigin = new int[Rank];
            int[] ashape = new int[Rank];
            for (int i = 0, j = 0; i < this.SourceVariable.Rank; i++)
            {
                if (this.stride[i] == 0) // this dimensions is removed by the striding
                {
                    // corresponding origin must be within the srcAffectedRect for this dimension
                    int or = origin[i];
                    if (or < srcAffectedRect.Origin[i] ||
                        or >= srcAffectedRect.Origin[i] + srcAffectedRect.Shape[i])
                        return new Rectangle(Rank); // no intersection
                    continue;
                }
                aorigin[j] = (int)Math.Ceiling((srcAffectedRect.Origin[i] - origin[i]) / (double)stride[i]);
                if (aorigin[j] < 0)
                    aorigin[j] = 0;

                int last = (int)Math.Floor((srcAffectedRect.Origin[i] + srcAffectedRect.Shape[i] - 1 - origin[i]) / (double)stride[i]);
                ashape[j] = last - aorigin[j] + 1;

                if (this.count[i] > 0) // i.e. length is fixed
                {
                    if (aorigin[j] >= this.count[i])
                        return new Rectangle(Rank);
                    if (last >= this.count[i])
                        ashape[j] = this.count[i] - aorigin[j];
                }

                j++;
            }

            Rectangle affectedRect = new Rectangle(aorigin, ashape);
            return affectedRect;
        }

        /// <summary>
        /// Completely updates shape and affected rect when "origin" changes and 
        /// possibly source variable changed.
        /// </summary>
        /// <param name="changes"></param>
        /// <param name="origin"></param>
        private void InnerUpdateChanges(DataSet.Changes changes, int[] origin)
        {
            int srank = SourceVariable.Rank;
            if (origin == null)
                origin = new int[srank];
            if (origin.Length != srank)
                throw new ArgumentException("Wrong origin indices");

            int[] sourceShape;
            Variable.Changes srcChanges = changes.GetVariableChanges(SourceVariable.ID);
            if (srcChanges == null)
                sourceShape = SourceVariable.GetShape();
            else
                sourceShape = srcChanges.Shape;

            for (int i = 0; i < srank; i++)
            {
                if (origin[i] < 0) throw new ArgumentOutOfRangeException("origin is negative");
                if ((count[i] == 0) && (origin[i] != this.origin[i]))
                    throw new ArgumentException("Can't set origin[" + i + "]new value because count[" + i + "]=0");
                if ((count[i] > 0) &&
                    ((origin[i] + stride[i] * (count[i] - 1)) >= sourceShape[i]))
                    //	|| (origin[i] > sourceShape[i])))
                    //if ((((count[i] > 0) && (origin[i] + stride[i] * (count[i] - 1)) >= sourceShape[i]))
                    //    || ((count[i] > 0) && origin[i] > sourceShape[i]))
                    throw new ArgumentException("Source variable does not contain this range of data");
            }

            //**********************************
            int rank = Rank;
            int[] aorigin = new int[rank];
            int[] ashape = new int[rank];
            for (int i = 0, k = 0; i < stride.Length; i++)
                if (stride[i] > 0)
                {
                    if (count[i] > 0)
                        ashape[k] = count[i];
                    else
                    {
                        int t = (sourceShape[i] - origin[i] - 1) / stride[i];
                        if (t < 0) t = 0;
                        if (sourceShape[i] > origin[i])
                            ashape[k] = 1 + t;
                        else ashape[k] = 0;
                    }
                    k++;
                }

            Rectangle AffectedRect = new Rectangle(aorigin, ashape);
            /***********************************************************************/
            // Making changes
            var myChanges = changes.GetVariableChanges(ID);
            myChanges.Shape = ashape;
            myChanges.AffectedRectangle = AffectedRect;
            changes.UpdateChanges(myChanges);
        }

        public override Array GetData(int[] strideOrigin, int[] strideShape)
        {
            int rank = Rank;
            if (strideOrigin == null)
                strideOrigin = new int[rank];

            if (strideOrigin.Length != rank)
                throw new ArgumentException("origin is incorrect");
            if (strideShape != null && strideShape.Length != rank)
                throw new ArgumentException("shape is incorrect");

            int[] actualShape = GetShape(SchemaVersion.Committed);
            if (strideShape == null)
            {
                strideShape = new int[rank];
                for (int i = 0; i < strideShape.Length; i++)
                    strideShape[i] = actualShape[i] - strideOrigin[i];
            }
            else
                for (int i = 0; i < strideShape.Length; i++)
                {
                    if (strideOrigin[i] + strideShape[i] > actualShape[i])
                        throw new ArgumentException("Specified origin and shape describe an area larger than the variable's shape");
                }

            int srcRank = SourceVariable.Rank;
            int[] srcOrigin = new int[srcRank];
            int[] srcShape = new int[srcRank];

            int[] stride2;
            if (srcRank > rank) // reducing dimensions
                stride2 = (int[])stride.Clone(); // will be updated
            else
                stride2 = stride;
            for (int i = 0, j = 0; i < srcRank; i++)
            {
                if (stride[i] == 0) // dimension i is to be removed
                {
                    srcOrigin[i] = this.origin[i];
                    srcShape[i] = 1;
                    stride2[i] = 1; // instead of 0
                }
                else
                {
                    srcOrigin[i] = this.origin[i] + this.stride[i] * strideOrigin[j];
                    srcShape[i] = strideShape[j];
                    j++;
                }
            }
            Array array = SourceVariable.GetData(srcOrigin, stride2, srcShape);
            if (rank == srcRank)
                return array;

            // Some dimensions are to be removed. 
            return RemoveDimensions(array);
        }

        public override Array GetData(int[] strideOrigin, int[] strideStride, int[] strideShape)
        {
            int rank = Rank;
            if (rank == 0)
            {
                if (strideOrigin != null && strideOrigin.Length > 0)
                    throw new ArgumentException("Origin in GetData() for scalar variable must be null or empty", "origin");
                if (strideShape != null && strideShape.Length > 0)
                    throw new ArgumentException("Count in GetData() for scalar variable must be null or empty", "count");
                if (strideStride != null && strideStride.Length > 0)
                    throw new ArgumentException("Stride in GetData() for scalar variable must be null or empty", "stride");
                return GetData(null, null);
            }

            if (strideOrigin == null)
                strideOrigin = new int[rank];

            if (strideOrigin.Length != rank)
                throw new ArgumentException("origin is incorrect");
            if (strideShape != null && strideShape.Length != rank)
                throw new ArgumentException("shape is incorrect");

            if (strideShape == null)
            {
                int[] actualShape = GetShape(SchemaVersion.Committed);
                strideShape = new int[rank];
                for (int i = 0; i < strideShape.Length; i++)
                    strideShape[i] = 1 + (actualShape[i] - 1 - strideOrigin[i]) / strideStride[i];
            }
            else if (Array.IndexOf(strideShape, 0) >= 0)
                return Array.CreateInstance(TypeOfData, strideShape);

            int srcRank = SourceVariable.Rank;
            int[] srcOrigin = new int[srcRank];
            int[] srcShape = new int[srcRank];
            int[] srcStride = new int[srcRank];
            for (int i = 0, j = 0; i < srcRank; i++)
            {
                if (stride[i] == 0) // dimension i is to be removed
                {
                    srcOrigin[i] = this.origin[i];
                    srcShape[i] = 1;
                    srcStride[i] = 1; // instead of 0
                }
                else
                {
                    srcOrigin[i] = this.origin[i] + this.stride[i] * strideOrigin[j];
                    srcStride[i] = this.stride[i] * strideStride[j];
                    srcShape[i] = strideShape[j];
                    j++;
                }
            }
            Array array = SourceVariable.GetData(srcOrigin, srcStride, srcShape);
            if (rank == srcRank)
                return array;

            // Some dimensions are to be removed. 
            return RemoveDimensions(array);
        }

        private Array RemoveDimensions(Array array)
        {
            int rank = SourceVariable.Rank;
            int resRank = Rank; // resRank <= rank

            if (resRank == 0)
            {
                return new DataType[] { (DataType)array.GetValue(new int[rank]) };
            }

            int[] resShape = new int[resRank];
            int n = 1;
            for (int i = 0, j = 0; i < rank; i++)
            {
                if (stride[i] > 0)
                {
                    resShape[j] = array.GetLength(i);
                    n *= resShape[j++];
                }
            }

            int[] index = new int[rank];
            int[] resIndex = new int[resRank];

            Array output = Array.CreateInstance(TypeOfData, resShape);
            for (int i = 0; i < n; i++)
            {
                output.SetValue(array.GetValue(index), resIndex);
                for (int j = 0, k = 0; j < rank; j++)
                {
                    if (stride[j] > 0)
                    {
                        index[j]++;
                        resIndex[k]++;
                    }
                    else continue; // skip the dimension
                    if (resIndex[k] < resShape[k]) break;
                    index[j] = 0;
                    resIndex[k] = 0;
                    k++;
                }
            }
            return output;
        }

        protected override Array Transform(int[] origin, Array array)
        {
            throw new NotImplementedException("It is not used since the GetData method is overriden.");
        }

        protected override int[] ReadShape()
        {
            int dimCount = 0;
            for (int i = 0; i < stride.Length; i++)
                if (stride[i] > 0) dimCount++;
            int[] sh = new int[dimCount];
            for (int i = 0, k = 0; i < stride.Length; i++)
                if (stride[i] > 0)
                {
                    if (count[i] > 0)
                        sh[k] = count[i];
                    else
                    {
                        int t = (SourceVariable.Dimensions[i].Length - origin[i] - 1) / stride[i];
                        if (t < 0) t = 0;
                        if (SourceVariable.Dimensions[i].Length > origin[i])
                            sh[k] = 1 + t;
                        else sh[k] = 0;
                    }
                    k++;
                }
            return sh;
        }
        #endregion
    }

    /// <summary>
    /// Contains metadata attributes names for a stridden variable those
    /// describe a striding.
    /// </summary>
    /// <remarks>
    /// <para>Read more in <see cref="DataSet.StrideVariable{DataType}(Variable{DataType},int[],int[],int[],string,IList{string},IList{string})"/></para>
    /// </remarks>
    public static class StriddenVariableKeys
    {
        /// <summary>
        /// Name of an attribute describing the origin parameter of striding.
        /// </summary>
        /// <remarks>
        /// <para>Read more in <see cref="DataSet.StrideVariable{DataType}(Variable{DataType},int[],int[],int[],string,IList{string},IList{string})"/></para>
        /// </remarks>
        public const string KeyForOrigin = "indexSpaceOrigin";
        /// <summary>
        /// Name of an attribute describing the string parameter of striding.
        /// </summary>
        /// <remarks>
        /// <para>Read more in <see cref="DataSet.StrideVariable{DataType}(Variable{DataType},int[],int[],int[],string,IList{string},IList{string})"/></para>
        /// </remarks>
        public const string KeyForStride = "indexSpaceStride";
        /// <summary>
        /// Name of an attribute describing the count parameter of striding.
        /// </summary>
        /// <remarks>
        /// <para>Read more in <see cref="DataSet.StrideVariable{DataType}(Variable{DataType},int[],int[],int[],string,IList{string},IList{string})"/></para>
        /// </remarks>
        public const string KeyForCount = "indexSpaceCount";
        /// <summary>
        /// Name of an attribute containing dimensions names of the source variable.
        /// </summary>
        /// <remarks>
        /// <para>Read more in <see cref="DataSet.StrideVariable{DataType}(Variable{DataType},int[],int[],int[],string,IList{string},IList{string})"/></para>
        /// </remarks>
        public const string KeyForSourceDims = "indexSpaceDims";
    }

    /// <summary>
    /// Represents a stridden transformation variable.
    /// </summary>
    public interface IStriddenVariable
    {
        /// <summary>
        /// Updates the origin parameter of the striding.
        /// </summary>
        /// <param name="origin">Starting indices for a stridden variable.</param>
        /// <remarks>
        /// <para>Read more in <see cref="DataSet.StrideVariable{DataType}(Variable{DataType},int[],int[],int[],string,IList{string},IList{string})"/></para>
        /// </remarks>
        void SetIndexSpaceOrigin(int[] origin);
    }

    internal interface IPrivateStriddenVariable
    {
        void AddCoordinateSystem(CoordinateSystem cs);
    }
}

