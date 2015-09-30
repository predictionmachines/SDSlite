using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data
{
    /// <summary>
    /// The instance, implementing this interface, allows to adjust chunk sizes.
    /// </summary>
    public interface IChunkSizesAdjustable
    {
        /// <summary>
        /// Sets the chunk sizes (in data elements) to be used when new variables of the rank equal to length of the <paramref name="sizes"/> are added.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If a <see cref="DataSet"/> instance implements this interface, it stores data by chunks and allows a user to change the chunk sizes.
        /// The argument of the <see cref="SetChunkSizes"/> method affects new variables when they are being added to the <see cref="DataSet"/>, 
        /// if their rank is equal to the length of the <paramref name="sizes"/> array.
        /// If not, <see cref="DataSet"/> uses default chunk sizes.
        /// </para>
        /// <para>
        /// Each i-th value of the <paramref name="sizes"/> array is a number of elements in a single chunk by index i.
        /// </para>
        /// <example>
        /// <code>
        /// DataSet ds = DataSet.Open("data.nc?openMode=create");
        /// IChunkSizesAdjustable adjustChunkSize = (IChunkSizesAdjustable)ds;
        /// adjustChunkSize.SetChunkSizes(new int[] { 100000 });
        /// ds.AddVariable&lt;double&gt;("var1", "i");  // chunk size for variable "var1" is 100000 of numbers
        /// adjustChunkSize.SetChunkSizes(new int[] { 10000, 10 });
        /// ds.AddVariable&lt;double&gt;("var2", "i", "j");  // chunk size for variable "var1" is 10000 x 10 of numbers
        /// </code>
        /// </example>
        /// </remarks>
        void SetChunkSizes(int[] sizes);
    }
}
