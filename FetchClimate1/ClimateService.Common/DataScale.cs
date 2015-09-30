using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data.Climate.Common
{
    public struct DataScale
    {
        public double AddOffset, ScaleFactor, MissingValue;
        public DataScale(Variable v)
        {
            double add_offset = 0;
            double scale_factor = 1.0;
            double missingValue = double.NaN;

            string[] AddOffsetKeys = new string[] { "add_offset", "AddOffset" };
            string[] MissingValueKeys = new string[] { "missing_value", "MissingValue" };
            string[] scaleFactorKeys = new string[] { "scale_factor", "ScaleFactor" };

            foreach (string ao_key in AddOffsetKeys)
                if (v.Metadata.ContainsKey(ao_key))
                {
                    add_offset = Convert.ToDouble(v.Metadata[ao_key]);
                    break;
                }

            foreach (string sf_key in scaleFactorKeys)
                if (v.Metadata.ContainsKey(sf_key))
                {
                    scale_factor = Convert.ToDouble(v.Metadata[sf_key]);
                    break;
                }

            foreach (string mv_key in MissingValueKeys)
                if (v.Metadata.ContainsKey(mv_key))
                {
                    missingValue = Convert.ToDouble(v.Metadata[mv_key]);
                    break;
                }

            MissingValue = missingValue;
            AddOffset = add_offset;
            ScaleFactor = scale_factor;
        }
    }
}
