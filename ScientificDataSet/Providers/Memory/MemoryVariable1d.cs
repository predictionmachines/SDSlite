// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Research.Science.Data.Memory
{
    internal class MemoryVariable1d<DataType> : MemoryVariable<DataType>
    {
        public MemoryVariable1d(DataSet sds, string name, string[] dims)
            : base(sds, name, dims, new /*OrderedArray1d*/ArrayWrapper(1, typeof(DataType)))
        {
            if (dims.Length != 1)
                throw new Exception("Expected for 1d variable");
        }

        /*protected override void CheckConstraints(Variable.Changes proposedChanges)
        {
            base.CheckConstraints(proposedChanges);

            OrderedArray1d ordered = (OrderedArray1d)array;
            if (ordered.Order == ArrayOrder.None)
                return;

            ArrayOrder order = ordered.Order;
            if (proposedChanges.DataPieces != null)
                foreach (DataPiece piece in proposedChanges.DataPieces)
                {
                    ArrayOrder inFact = OrderedArray1d.IsOrdered<DataType>((DataType[])piece.Data);
                    if (inFact == ArrayOrder.Unknown)
                        continue;
                    if (inFact == ArrayOrder.None)
                        throw new Exception("Array " + Name + " is not ordered.");

                    if (order == ArrayOrder.Unknown)
                        order = inFact;
                    else if (order != inFact)
                        throw new Exception("Appended array " + Name + " has wrong order.");
                }
        }*/       
    }
}

