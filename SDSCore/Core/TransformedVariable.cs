// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Microsoft.Research.Science.Data
{
    /// <summary>
    /// TransformedVariable&lt;DataType&gt; class is a base abstract class for all computational variables.
    /// </summary>
    /// <typeparam name="DataType">Type of an element of the variable.</typeparam>
    internal abstract class TransformedVariable<DataType> : Variable<DataType>, ITransformedVariable
    {
        #region Private fields

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of the variable.
        /// </summary>
        /// <param name="dims"></param>
        /// <param name="dataSet"></param>
        protected TransformedVariable(DataSet dataSet, string[] dims) :
            base(dataSet, dims)
        {
            if (dataSet == null)
                throw new ArgumentNullException("dataSet");
            this.ID = ~this.ID;
        }

        #endregion
    }

    /// <summary>
    /// Untyped representation of a transform variable.
    /// </summary>
    internal interface ITransformedVariable
    {
    }

    /// <summary>
    /// Represents an abstract computational variable performing
    /// a reversible or non-reversible transformation of a single source variable.
    /// </summary>
    /// <typeparam name="SourceDataType">Type of data for the underlying variable.</typeparam>
    /// <typeparam name="ResultDataType">Type of data for the output variable.</typeparam>
    /// <remarks>
    /// <para>TransformationVariable "mirrors" the data in underlying variable. 
    /// All requests to data translate to corresponding requests for underlying variable.
    /// Changes to underlying variable generate corresponding events for the transformation variable, etc.</para>
    /// <para>If transformation is reversible, the data is "mirrored" in both ways. 
    /// Calls to Variable.PutData() overloads or setting the Variable[indices] property 
    /// translate to corresponding changes of underlying variable data.</para>
    /// <para>
    /// Metadata of the underilying variable propagate to metadata for the TransformationVariable except some
    /// certain entries, name and the provenance being an example of such exception. Therefore, changes of the replicated
    /// entries in this variable update the same entries in the underlying variable and vice versa.
    /// The extra parameters in the constuctor allow specifying the list of independent entries.
    /// See more in remarks for the constructor.
    /// </para>
    /// </remarks>
    internal abstract class TransformationVariable<ResultDataType, SourceDataType> : TransformedVariable<ResultDataType>, IDependantVariable
    {
        #region Private fields

        /// <summary>
        /// The variable this computational variable is based on.
        /// </summary>
        private Variable<SourceDataType> sourceVariable;

        /// <summary>
        /// The metadata collection is based on the source variable.
        /// </summary>
        private RefVariableMetadata metadata;

        /// <summary>Collection of proposed changes currently being handled. This collection should
        /// enable correct concurrent work on simultaneous changing of the underlying and this variables.</summary>
        private List<Changes> proposedChangesList = new List<Changes>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of the variable.
        /// </summary>
        /// <param name="sds"></param>
        /// <param name="name"></param>
        /// <param name="dims"></param>
        /// <param name="sourceVariable">The variable this computational variable is based on.</param>
        protected TransformationVariable(DataSet sds, string name, Variable<SourceDataType> sourceVariable, string[] dims) :
            this(sds, name, sourceVariable, dims, null, null)
        {
        }

        /// <summary>
        /// Initializes an instance of the variable.
        /// </summary>
        /// <param name="hiddenEntries">Collection of keys which are not to be inherited from the underlying metadata.
        /// These entries are changed independently.</param>
        /// <param name="readonlyEntries">Collection of keys that cannot be changed through this collection.</param>
        /// <param name="name"></param>
        /// <param name="dims"></param>
        /// <param name="dataSet"></param>
        /// <param name="sourceVariable">The variable this computational variable is based on.</param>
        /// <remarks>
        /// <para>The <paramref name="hiddenEntries"/> and <paramref name="readonlyEntries"/> parameters 
        /// allow to make some metadata entries indpendent from the metadata of the underlying variable.
        /// These parameters can be null and this will be considered as an empty collection.</para>
        /// <para>Two entries are always independent from the underlying metadata. These are
        /// the name and the provenance entries of the variable's metadata.</para>
        /// </remarks>
        protected TransformationVariable(DataSet dataSet, string name, Variable<SourceDataType> sourceVariable, string[] dims,
            IList<string> hiddenEntries, IList<string> readonlyEntries) :
            base(dataSet, dims)
        {
            if (sourceVariable == null)
                throw new ArgumentNullException("sourceVariable");
            if (dataSet != sourceVariable.DataSet)
                throw new ArgumentException("Source variable must belong to the same DataSet");
            this.sourceVariable = sourceVariable;

            this.metadata = new RefVariableMetadata(sourceVariable, hiddenEntries, readonlyEntries);
            this.metadata[this.metadata.KeyForName] = name;
        }

        protected override void Initialize()
        {
            base.Initialize();

            // Watching for the source variable
            this.sourceVariable.Changing += new VariableChangingEventHandler(SourceVariableChanging);
            this.sourceVariable.Changed += new VariableChangedEventHandler(SourceVariableChanged);
            this.sourceVariable.RolledBack += new VariableRolledBackEventHandler(SourceVariableRolledBack);

            ((RefVariableMetadata)metadata).Subscribe();

            changes.Shape = TransformIndexRectangle(
                new Rectangle(new int[sourceVariable.Rank],
                sourceVariable.GetShape(SchemaVersion.Recent))).Shape;
            changes.AffectedRectangle = new Rectangle(new int[Rank], changes.Shape);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the underlying variable that is being transformed.
        /// </summary>
        public Variable<SourceDataType> SourceVariable
        {
            get { return sourceVariable; }
        }

        #endregion

        #region Relations with the source variable

        /// <summary>
        /// Transforms underlying data from rawData array to TransformedVariable data
        /// </summary>
        /// <param name="origin">Origin of rawData in underlying variable.</param>
        /// <param name="rawData">Data to be transformed.</param>
        /// <returns></returns>
        protected abstract Array Transform(int[] origin, Array rawData);

        /// <summary>
        /// Transforms the underlying variable's rectangle into the rectangle 
        /// for this transformation variable.
        /// </summary>
        protected virtual Rectangle TransformIndexRectangle(Rectangle underlyingRect)
        {
            return underlyingRect;
        }

        /// <summary>
        /// Transforms TransformedVariable data back into underlying variable
        /// </summary>
        /// <param name="origin">Origin of data in TransformedVariable.</param>
        /// <param name="data">Data to be transformed back into underlying variable.</param>
        /// <returns></returns>
        protected virtual Array ReverseTransform(int[] origin, Array data)
        {
            if (IsReadOnly)
                throw new NotSupportedException("TransformationVariable is irreversible.");

            throw new NotImplementedException("Reverse transform is not implemented.");
        }

        /// <summary>
        /// Transforms the transformation variable's rectangle into the rectangle 
        /// for its underlying variable.
        /// </summary>
        private/*protected virtual*/ Rectangle ReverseTransformIndexRectangle(Rectangle rect)
        {
            return rect;
        }

        #region Source Variable Events Handling

        private void SourceVariableChanging(object sender, VariableChangingEventArgs e)
        {
            switch (e.Action)
            {
                case VariableChangeAction.PutData:

                    // TODO: add check for proposedChangesList
                    var ar = TransformIndexRectangle(e.ProposedChanges.AffectedRectangle);
                    if (!ar.IsEmpty)
                    {
                        var shape = TransformIndexRectangle(new Rectangle(new int[sourceVariable.Rank], e.ProposedChanges.Shape)).Shape;
                        Changes c = new Changes(Version, GetSchema(), null, null, shape, ar);

                        e.Cancel = !FireEventVariableChanging(VariableChangeAction.PutData, c);
                    }
                    break;
                case VariableChangeAction.UpdateMetadata:
                    break;
            }
        }

        private void SourceVariableChanged(object sender, VariableChangedEventArgs e)
        {
            switch (e.Action)
            {
                case VariableChangeAction.UpdateMetadata:
                    break;

                case VariableChangeAction.PutData:
                    Rectangle ar = TransformIndexRectangle(e.Changes.AffectedRectangle);
                    if (!ar.IsEmpty)
                    {
                        StartChanges();
                        changes.AffectedRectangle = ar;
                        changes.Shape = TransformIndexRectangle(new Rectangle(new int[sourceVariable.Rank], e.Changes.Shape)).Shape;

                        FireEventVariableChanged(VariableChangeAction.PutData);
                    }
                    break;

                default:
                    throw new Exception("Unhandled action");
            }
        }

        private void SourceVariableRolledBack(object sender, VariableRolledBackEventArgs e)
        {
            Rollback();
        }

        #endregion

        #endregion

        #region Metadata

        /// <summary>
        /// Gets the metadata collection of the computational variable.
        /// </summary>
        public override MetadataDictionary Metadata
        {
            get { return metadata; }
        }

        #endregion

        #region Input/Output

        protected virtual void UpdateChanges(DataSet.Changes changes)
        {
            if (changes == null) return;

            if (sourceVariable != null && sourceVariable is IDependantVariable)
                ((IDependantVariable)sourceVariable).UpdateChanges(changes);

            Variable.Changes myChanges = changes.GetVariableChanges(ID);
            /*if (myChanges == null)
                return; // doesn't depend*/
            Variable.Changes srcChanges = changes.GetVariableChanges(sourceVariable.ID);
            if (srcChanges == null)
            {
                return;
            }

            bool srcMetadataChanged = srcChanges.MetadataChanges.Count != 0 || srcChanges.MetadataChanges.HasChanges;

            var affectedRectangle = TransformIndexRectangle(srcChanges.AffectedRectangle);
            int[] shape = null;

            if (myChanges == null && affectedRectangle.IsEmpty && !srcMetadataChanged) return; // no changes
            if (!affectedRectangle.IsEmpty)
                shape = TransformIndexRectangle(new Rectangle(new int[sourceVariable.Rank], srcChanges.Shape)).Shape;

            if (myChanges == null)
            {
                var metadataChanges = metadata.FilterChanges(srcChanges.MetadataChanges);
                if (shape == null)
                    shape = GetShape(); // and affectedRect is not empty

                myChanges = new Changes(Version + 1, GetSchema(SchemaVersion.Committed), metadataChanges, null,
                    shape, affectedRectangle);
                changes.UpdateChanges(myChanges);
            }
            else
            {
                if (shape != null) // there are changes in source var 
                {
                    myChanges.AffectedRectangle = affectedRectangle;
                    myChanges.Shape = shape;
                }
            }
        }

        #region PutData

        public override void PutData(int[] origin, Array data)
        {
            if (DataSet.IsDisposed)
                throw new ObjectDisposedException("DataSet is disposed");
            if (IsReadOnly)
                throw new NotSupportedException("TransformationVariable is irreversible.");
            if (data == null)
                throw new ArgumentNullException("data");

            int rank = Rank;
            if (origin != null && origin.Length != rank)
                throw new ArgumentException("Origin contains incorrect number of dimensions");
            if ((rank == 0 && data.Rank != 1) || (rank != 0 && data.Rank != rank))
                throw new ArgumentException("Wrong data rank");
            if (origin == null) origin = new int[Rank];

            /* Both Changing and Changed events will happen when 
             * we put data to the source variable: 
             * - first, it fires Changing, we're computing proposedChanges and fire the own event.
             * - secont, it fires Changed, we're propogating it.
             * If Changing is cancelled (either our or source's) both operations will be cancelled.
            */

            // Putting data
            int[] trOrigin = null;
            if (rank != 0)
            {
                Rectangle thisRectangle = new Rectangle(origin, new int[origin.Length]);
                Rectangle srcRectangle = ReverseTransformIndexRectangle(thisRectangle);
                trOrigin = srcRectangle.Origin;
            }

            sourceVariable.PutData(
                trOrigin,
                ReverseTransform(origin, data));

            //Debug.Assert(HasChanges, "SourceVariable.Changed event hasn't worked properly.");
        }

        public override void Append(Array data, int dimToAppend)
        {
            if (IsReadOnly)
                throw new NotSupportedException("TransformationVariable is irreversible.");

            if (data == null)
                throw new ArgumentNullException("data");
            if (dimToAppend < 0 || dimToAppend >= Rank)
                throw new ArgumentException("Wrong dimension index to append.");
            if (data.Rank != Rank)
                throw new ArgumentException("Data array rank must be equal to theis variable rank.");

            /* Both Changing and Changed events will happen when 
             * we put data to the source variable: 
             * - first, it fires Changing, we're computing proposedChanges and fire the own event.
             * - secont, it fires Changed, we're propogating it.
             * If Changing is cancelled (either our or source's) both operations will be cancelled.
            */

            // TODO: Append now works ONLY FOR IDENTITY spatial transform!

            // Appending
            sourceVariable.Append(ReverseTransform(null, data), dimToAppend);

            //Debug.Assert(HasChanges, "SourceVariable.Changed event hasn't worked properly.");
        }

        private int[] GetShapeOfArray(Array a)
        {
            int[] shape = new int[a.Rank];
            for (int i = 0; i < a.Rank; i++)
                shape[i] = a.GetLength(i);
            return shape;
        }

        protected internal override void OnRollback(Variable.Changes proposedChanges)
        {
            base.OnRollback(proposedChanges);
            sourceVariable.Rollback();

            if (Version == 0) // the variable is added and rolled back
            {
                this.sourceVariable.Changing -= new VariableChangingEventHandler(SourceVariableChanging);
                this.sourceVariable.Changed -= new VariableChangedEventHandler(SourceVariableChanged);
                this.sourceVariable.RolledBack -= new VariableRolledBackEventHandler(SourceVariableRolledBack);
                ((RefVariableMetadata)metadata).Unsubscribe();
            }
        }

        #endregion

        #region GetData

        public override Array GetData(int[] origin, int[] shape)
        {
            if (Rank == 0) // scalar
            {
                if (origin != null && origin.Length > 0)
                    throw new ArgumentException("origin is incorrect");
                if (shape != null && shape.Length != 0)
                    throw new ArgumentException("shape is incorrect");

                Array scalar = sourceVariable.GetData(null, null);
                return Transform(new int[] { 0 }, scalar);
            }

            if (origin == null)
                origin = new int[Rank];

            if (origin.Length != Rank)
                throw new ArgumentException("origin is incorrect");
            if (shape != null && shape.Length != origin.Length)
                throw new ArgumentException("shape is incorrect");

            if (shape == null)
            {
                shape = new int[origin.Length];
                int[] totalShape = ReadShape();
                for (int i = 0; i < shape.Length; i++)
                    shape[i] = totalShape[i] - origin[i];
            }

            Rectangle rect = ReverseTransformIndexRectangle(new Rectangle(origin, shape));
            Array array = sourceVariable.GetData(rect.Origin, rect.Shape);
            return Transform(rect.Origin, array);
        }

        #endregion

        #region ReadShape

        /// <summary>
        /// Returns the shape that is a result of transformation of source variable's shape through the 
        /// <see cref="TransformIndexRectangle"/> method.
        /// </summary>
        /// <returns>The committed shape of the variable.</returns>
        protected override int[] ReadShape()
        {
            return TransformIndexRectangle(
                new Rectangle(new int[Rank], sourceVariable.GetShape(SchemaVersion.Committed))).Shape;
        }

        #endregion

        #endregion

        #region Transactions

        protected internal override void OnPrecommit(Variable.Changes proposedChanges)
        {
            base.OnPrecommit(proposedChanges);

            if (sourceVariable.DataSet != DataSet)
                sourceVariable.DataSet.Commit();
        }

        #endregion

        #region IDependantVariable Members

        void IDependantVariable.UpdateChanges(DataSet.Changes changes)
        {
            UpdateChanges(changes);
        }

        #endregion
    }

    /// <summary>
    /// Represents an abstract computational variable performing a reversible or not reversible
    /// point-to-point transformation of a source variable.
    /// </summary>
    /// <typeparam name="SourceDataType">Type of data for the underlying variable.</typeparam>
    /// <typeparam name="ResultDataType">Type of data for the output variable.</typeparam>
    internal abstract class PointToPointTransformationVariable<ResultDataType, SourceDataType> : TransformationVariable<ResultDataType, SourceDataType>
    {
        #region Constructors

        /// <summary>
        /// Initializes an instance of the variable.
        /// </summary>
        /// <param name="sourceVariable">The variable this computational variable is based on.</param>
        /// <param name="dims"></param>
        /// <param name="name"></param>
        /// <param name="sds">
        /// </param>
        protected PointToPointTransformationVariable(DataSet sds, string name, Variable<SourceDataType> sourceVariable, string[] dims) :
            this(sds, name, sourceVariable, dims, null, null)
        {
        }

        /// <summary>
        /// Initializes an instance of the variable.
        /// </summary>
        /// <param name="hiddenEntries">Collection of keys which are not to be inherited from the underlying metadata.
        /// These entries are changed independently.</param>
        /// <param name="readonlyEntries">Collection of keys that cannot be changed through this collection.</param>
        /// <param name="sourceVariable">The variable this computational variable is based on.</param>
        /// <param name="sds"></param>
        /// <param name="name"></param>
        /// <param name="dims"></param>
        /// <remarks>
        /// <para>The <paramref name="hiddenEntries"/> and <paramref name="readonlyEntries"/> parameters 
        /// allow to make some metadata entries indpendent from the metadata of the underlying variable.
        /// These parameters can be null and this will be considered as an empty collection.</para>
        /// <para>Two entries are always independent from the underlying metadata. These are
        /// the name and the provenance entries of the variable's metadata.</para>
        /// </remarks>
        protected PointToPointTransformationVariable(DataSet sds, string name, Variable<SourceDataType> sourceVariable, string[] dims,
            IList<string> hiddenEntries, IList<string> readonlyEntries) :
            base(sds, name, sourceVariable, dims, hiddenEntries, readonlyEntries)
        {
        }

        #endregion
    }

    /// <summary>
    /// Represents an abstract computational variable that produces data not based on an underlying variable.
    /// </summary>
    /// <typeparam name="DataType">Type of the output data.</typeparam>
    internal abstract class PureComputationalVariable<DataType> : TransformedVariable<DataType>, IDependantVariable
    {
        #region Private fields

        private MetadataDictionary metadata;

        /// <summary>Committed shape of the variable.</summary>
        private int[] shape;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of the variable.
        /// </summary>
        /// <param name="sds"></param>
        /// <param name="dims"></param>
        /// <param name="name"></param>
        protected PureComputationalVariable(DataSet sds, string name, string[] dims) :
            base(sds, dims)
        {
            this.metadata = new MetadataDictionary();
            this.Name = name;
            shape = new int[dims.Length];
        }

        protected override void Initialize()
        {
            base.Initialize();

            // Getting initial shape from our neighboors.
            int[] proposedShape = new int[Rank];
            var sch = DataSet.GetSchema(SchemaVersion.Recent);
            ReadOnlyDimensionList dl = this.Dimensions;
            int j = 0;
            foreach (var v in sch.Variables)
            {
                if (v.ID == DataSet.GlobalMetadataVariableID || v.ID == ID) continue;
                for (int k = 0; k < v.Dimensions.Count; k++)
                {
                    var d = v.Dimensions[k];
                    if (dl.Contains(d.Name))
                    {
                        j = dl.FindIndex(d.Name);
                        if (proposedShape[j] < d.Length) proposedShape[j] = d.Length;
                    }
                }
            }
            changes.Shape = proposedShape;
            changes.AffectedRectangle = new Rectangle(new int[Rank], proposedShape);
        }

        #endregion

        protected internal override void OnCommit(Variable.Changes proposedChanges)
        {
            // Committing changes:
            if (proposedChanges != null && proposedChanges.Shape != null)
                shape = proposedChanges.Shape;
        }

        protected virtual void UpdateChanges(DataSet.Changes changes)
        {
            if (changes == null) return;

            // Building proposed changes for this variable depending 
            // on shape of other variables of the same data set 
            // sharing the same dimensions.

            bool shapeUpdated = false;
            int rank = Rank;
            int[] proposedShape = null;

            ReadOnlyDimensionList dl = this.Dimensions;
            int j = 0;
            foreach (var v in changes.Variables)
            {
                if (v.ID == DataSet.GlobalMetadataVariableID || v.ID == ID) continue;
                var vch = changes.GetVariableChanges(v.ID);
                if (vch == null) continue;

                if (proposedShape == null)
                    proposedShape = this.GetShape(); // committed shape to update

                var dims = vch.InitialSchema.Dimensions.AsNamesArray();
                for (int k = 0; k < vch.Shape.Length; k++)
                {
                    string name = dims[k];
                    int d = vch.Shape[k];
                    if (dl.Contains(name))
                    {
                        j = dl.FindIndex(name);
                        if (proposedShape[j] < d)
                        {
                            proposedShape[j] = d;
                            shapeUpdated = true;
                        }
                    }
                }
            }
            if (!shapeUpdated)
                return;

            Variable.Changes myChanges = changes.GetVariableChanges(ID);
            if (myChanges != null)
            {
                shapeUpdated = false;
                int[] prevPropShape = myChanges.Shape;
                for (int i = 0; i < rank; i++)
                    if (proposedShape[i] != prevPropShape[i])
                    {
                        shapeUpdated = true;
                        break;
                    }
                if (!shapeUpdated) return;
            }

            int[] ar_origin = new int[rank];
            int[] ar_shape = new int[rank];
            bool hadChanges = false;
            for (int i = 0; i < rank; i++)
            {
                if (proposedShape[i] == shape[i] // no changes for dim #i
                    || hadChanges)
                {
                    ar_origin[i] = 0;
                    ar_shape[i] = proposedShape[i];
                }
                else // changes for dim #i
                {
                    hadChanges = true;
                    ar_origin[i] = shape[i];
                    ar_shape[i] = proposedShape[i] - shape[i];
                }
            }

            if (myChanges == null)
            {
                var ar = new Rectangle(ar_origin, ar_shape);
                var shape = proposedShape;

                myChanges = new Changes(Version + 1, GetSchema(SchemaVersion.Committed), new MetadataDictionary(),
                    null, shape, ar);

                changes.UpdateChanges(myChanges);
            }
            else
            {
                myChanges.AffectedRectangle = new Rectangle(ar_origin, ar_shape);
                myChanges.Shape = proposedShape;
            }
        }

        /// <summary>
        /// Returns shape looking for length of dimemsions of other variable of data set.
        /// </summary>
        /// <returns></returns>
        protected override int[] ReadShape()
        {
            return shape;
        }

        #region Metadata

        /// <summary>
        /// Gets the metadata collection of the computational variable.
        /// </summary>
        public override MetadataDictionary Metadata
        {
            get { return metadata; }
        }

        #endregion

        #region IDependantVariable Members

        void IDependantVariable.UpdateChanges(DataSet.Changes changes)
        {
            UpdateChanges(changes);
        }

        #endregion
    }
}

