// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;

namespace Microsoft.Research.Science.Data.Utilities
{
    public class IntBox
    {
        private int[] min;
        private int[] max;

        public IntBox(int[] min, int[] max)
        {
            if (min.Length != max.Length)
                throw new ArgumentException("Arrays lengthes do not match");
            for (int i = 0; i < min.Length; i++)
                if (min[i] > max[i])
                    throw new ArgumentException(
                        String.Format("Min[{0}] is greater than max[{0}]", i));
            this.min = min;
            this.max = max;
        }

        public int[] Min
        {
            get
            {
                return (int[])min.Clone(); // We'll keep Box strictly immutable
            }
        }

        public int[] Max
        {
            get
            {
                return (int[])max.Clone(); // We'll keep Box strictly immutable
            }
        }
       
        public bool IsEmpty
        {
            get
            {
                for (int i = 0; i < min.Length; i++)
                    if (min[i] >= max[i])
                        return true;
                return false;
            }
        }

        /// <summary>Subtracts argument from this box</summary>
        /// <param name="second">Box to be subtracted</param>
        /// <returns>Sequence of boxes resulting from subtraction</returns>
        public IEnumerable<IntBox> Subtract(IntBox second)
        {
            throw new NotSupportedException();
        }
    }
}

