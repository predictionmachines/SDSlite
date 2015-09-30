using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sdsutil
{
    interface IMetadataConflictResolver
    {
        /// <summary>
        /// Resolves conflicting attribute presented in two merged DataSets.
        /// </summary>
        /// <param name="attribute">Conflicting attribute value.</param>
        /// <param name="value1">First alternative.</param>
        /// <param name="value2">Second alternative.</param>
        /// <returns></returns>
        object Resolve(string attribute, object value1, object value2);
    }

    class WarningConflictResolver : IMetadataConflictResolver
    {
        #region IMetadataConflictResolver Members

        public object Resolve(string attribute, object value1, object value2)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Error.WriteLine("Attribute " + attribute + " is conflicting; first value is chosen.");
            Console.ResetColor();
            return value1;
        }

        #endregion
    }
}
