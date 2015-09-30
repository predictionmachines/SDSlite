using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Microsoft.Research.Science.Data.Climate.Conventions;

namespace Microsoft.Research.Science.Data.Climate.Common
{   
    public class MultipleRequestDescriptor
    {
        private int timeStep;

        public int TimeStep
        {
            get { return timeStep; }
            set { timeStep = value; }
        }
        protected ClimateParameter parameter;

        public ClimateParameter Parameter
        {
            get { return parameter; }
            set { parameter = value; }
        }
        protected TimeBounds timeBounds;

        public TimeBounds TimeBounds
        {
            get { return timeBounds; }
            set { timeBounds = value; }
        }
        protected FetchingTimeModes timeMode;

        public FetchingTimeModes TimeMode
        {
            get { return timeMode; }
            set { timeMode = value; }
        }
        protected List<SpatialGrid> regions = new List<SpatialGrid>();

        public List<SpatialGrid> Regions
        {
            get { return regions; }
            set { regions = value; }
        }

        public MultipleRequestDescriptor(ClimateParameter p, TimeBounds tb, FetchingTimeModes mode)
        {
            parameter = p;
            timeBounds = tb;
            timeMode = mode;
        }

        public void AddRegion(SpatialGrid sp)
        {
            regions.Add(sp);
        }


        public string ToString(char separator, char regionSeparator = ':')
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Conventions.Namings.ParameterShortNames[Parameter]);
            if (TimeMode != FetchingTimeModes.Single)
            {
                sb.Append(separator);
                switch (TimeMode)
                {
                    case FetchingTimeModes.TsHourly:
                        sb.Append("/th");
                        break;
                    case FetchingTimeModes.TsSeasonly:
                        sb.Append("/ts");
                        break;
                    case FetchingTimeModes.TsYearly:
                        sb.Append("/ty");
                        break;
                }
                sb.Append(separator);
                sb.Append(TimeStep);
            }            
                
            if (TimeBounds.MinYear != GlobalConsts.StartYear || TimeBounds.MaxYear != GlobalConsts.EndYear)
            {
                sb.Append(separator);
                sb.Append("/years");
                sb.Append(separator);
                sb.Append(TimeBounds.MinYear);
                sb.Append(separator);
                sb.Append(TimeBounds.MaxYear);
            }
            if (!(TimeBounds.MinDay == GlobalConsts.DefaultValue && TimeBounds.MaxDay == GlobalConsts.DefaultValue) &&
                !(TimeBounds.MinDay == 1 && TimeBounds.MaxDay == 365 && TimeBounds.MinYear != TimeBounds.MaxYear) &&
                !(TimeBounds.MinDay == 1 && TimeBounds.MaxDay == 365 && TimeBounds.MinYear == TimeBounds.MaxYear && !DateTime.IsLeapYear(TimeBounds.MaxYear)) &&
                !(TimeBounds.MinDay == 1 && TimeBounds.MaxDay == 366 && TimeBounds.MinYear == TimeBounds.MaxYear && DateTime.IsLeapYear(TimeBounds.MaxYear)))
            {
                sb.Append(separator);
                sb.Append("/days");
                sb.Append(separator);
                sb.Append(TimeBounds.MinDay);
                sb.Append(separator);
                sb.Append(TimeBounds.MaxDay);
            }
            if (!(TimeBounds.MinHour == GlobalConsts.DefaultValue && TimeBounds.MaxHour == GlobalConsts.DefaultValue) &&
                !(TimeBounds.MinHour == 0 && TimeBounds.MaxHour == 24))
            {
                sb.Append(separator);
                sb.Append("/hours");
                sb.Append(separator);
                sb.Append(TimeBounds.MinHour);
                sb.Append(separator);
                sb.Append(TimeBounds.MaxHour);
            }
            foreach (var reg in Regions)
            {
                sb.Append(separator);
                sb.Append(reg.LatMin.ToString(CultureInfo.InvariantCulture));
                if (!double.IsNaN(reg.StepLat))
                {
                    sb.Append(regionSeparator);
                    sb.Append(reg.StepLat.ToString(CultureInfo.InvariantCulture));
                }
                if (reg.LatMax != reg.LatMin)
                {
                    sb.Append(regionSeparator);
                    sb.Append(reg.LatMax.ToString(CultureInfo.InvariantCulture));
                }

                sb.Append(separator);
                sb.Append(reg.LonMin.ToString(CultureInfo.InvariantCulture));
                if (!double.IsNaN(reg.StepLon))
                {
                    sb.Append(regionSeparator);
                    sb.Append(reg.StepLon.ToString(CultureInfo.InvariantCulture));
                }
                if (reg.LonMax != reg.LonMin)
                {
                    sb.Append(regionSeparator);
                    sb.Append(reg.LonMax.ToString(CultureInfo.InvariantCulture));
                }
            }
            return sb.ToString();
        }
    }
}
