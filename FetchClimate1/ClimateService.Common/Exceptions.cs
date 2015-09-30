using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data.Climate.Common
{
    public class DataProcComputationException : Exception
    {
        public DataProcComputationException(string mess)
            : base(mess)
        { }
    }
    public class NoProcessorAvailableException : Exception
    {
        public NoProcessorAvailableException(string mess)
            : base(mess)
        { }
    }

    public class MissingValuePresentException : DataProcComputationException
    {
        public MissingValuePresentException(string mess)
            : base(mess)
        { }
    }

    public class TooLargeDataException : DataProcComputationException
    {
        public TooLargeDataException(string mess)
            : base(mess)
        { }
    }

    public class DataAggregationException : DataProcComputationException
    {
        public DataAggregationException(string mess)
            : base(mess)
        { }
    }
}
