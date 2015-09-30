using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Science.Data.Climate.Common;
using Microsoft.Research.Science.Data.Climate.Conventions;

namespace Microsoft.Research.Science.Data.Climate
{
    public static class Extensions
    {
        public static SpatialCell CorrectLonsTo0_360(this SpatialCell cq)
        {
            if (Math.Abs(cq.LonMin - cq.LonMax) == 360)
            {
                cq.LonMin = 0;
                cq.LonMax = 360;
                return cq;
            }
            if ((cq.LonMax == 0 && cq.LonMin == 0))
                return cq;
            if ((cq.LonMax == 360 && cq.LonMin == 360))
            {
                cq.LonMin = cq.LonMax = 0;
                return cq;
            }
            if (cq.LonMax <= 0)
                cq.LonMax += 360;
            if (cq.LonMin < 0)
                cq.LonMin += 360;
            if (cq.LonMax >= 0 && cq.LonMax <= 360 && cq.LonMin == 360)
                cq.LonMin = 0;
            return cq;
        }

        public static SpatialCell CorrectLonsTo180_180(this SpatialCell cq)
        {
            if (cq.LonMax - cq.LonMin == 360)
            {
                cq.LonMin = -180;
                cq.LonMax = 180;
                return cq;
            }
            if (cq.LonMin == -180 && cq.LonMax == -180)
                return cq;
            if (cq.LonMax == 180 && cq.LonMin == 180)
            {
                cq.LonMin = cq.LonMax = -180;
                return cq;
            }
            if (cq.LonMax > 180)
                cq.LonMax -= 360;
            if (cq.LonMin >= 180)
                cq.LonMin -= 360;



            

            return cq;
        }

        public static CellQuery TransformDefaultToExact(this CellQuery cq)
        {
            if (cq.HourMin == GlobalConsts.DefaultValue)
                cq.HourMin = 0;
            if (cq.HourMax == GlobalConsts.DefaultValue)
                cq.HourMax = 24;
            if (cq.DayMin == GlobalConsts.DefaultValue)
                cq.DayMin = 1;
            if (cq.DayMax == GlobalConsts.DefaultValue)
                cq.DayMax = 360;
            return cq;
        }
    }
}
