using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Science.Data.Climate;
using Microsoft.Research.Science.Data.Climate.Service;
using Microsoft.Research.Science.Data.Climate.Conventions;

namespace Microsoft.Research.Science.Data.Climate.Service
{    
    public class TimeBounds
    {
        int minday = 1, maxday = 360, minhour = 0, maxhour = 24;

        public int MinYear { get; set; }
        public int MaxYear { get; set; }
        /// <summary>
        /// 1..360
        /// </summary>
        public int MinDay
        {
            get
            {
                return minday;
            }
            set
            {
                if ((value < 1 || value > 360) && value != GlobalConsts.DefaultValue)
                    throw new ArgumentException("Day cant be less then 1 or more then 360");
                minday = value;
            }
        }

        /// <summary>
        /// 1..360
        /// </summary>
        public int MaxDay
        {
            get
            {
                return maxday;
            }
            set
            {
                if ((value < 1 || value > 360) && value != GlobalConsts.DefaultValue)
                    throw new ArgumentException("Day number cant be less then 1 or more then 360");
                maxday = value;
            }
        }

        /// <summary>
        /// 0..24
        /// </summary>
        public int MaxHour
        {
            get
            { return maxhour; }
            set
            {
                if ((value < 0 || value > 24) && value != GlobalConsts.DefaultValue)
                    throw new ArgumentException("Hour cant be less then 0 or more then 24");
                maxhour = value;
            }
        }

        /// <summary>
        /// 0..24
        /// </summary>
        public int MinHour
        {
            get
            {
                return minhour;
            }
            set
            {
                if ((value < 0 || value > 24) && value != GlobalConsts.DefaultValue)
                    throw new ArgumentException("Hour cant be less then 0 or more then 24");
                minhour = value;
            }
        }

        public TimeBounds Clone()
        {
            return (TimeBounds)MemberwiseClone();
        }
    }
}

namespace Microsoft.Research.Science.Data.Climate
{
    /// <summary>
    /// A variation that is primary for user to reflect
    /// </summary>
    public enum ResearchVariationType { Auto, Spatial, Yearly, Seasonly, Hourly };    


    public class TimeBounds : IEquatable<TimeBounds>
    {
        int minday = 1, maxday = 365, minhour = 0, maxhour = 24;

        public int MinYear { get; set; }
        public int MaxYear { get; set; }
        /// <summary>
        /// 1..366
        /// </summary>
        public int MinDay
        {
            get
            {
                return minday;
            }
            set
            {
                if ((value < 1 || value > 366) && value != GlobalConsts.DefaultValue)
                    throw new ArgumentException("Day cant be less then 1 or more then 366");
                minday = value;
            }
        }

        /// <summary>
        /// 1..366
        /// </summary>
        public int MaxDay
        {
            get
            {
                return maxday;
            }
            set
            {
                if ((value < 1 || value > 366) && value != GlobalConsts.DefaultValue)
                    throw new ArgumentException("Day number cant be less then 1 or more then 366");
                maxday = value;
            }
        }

        /// <summary>
        /// 0..24
        /// </summary>
        public int MaxHour
        {
            get
            { return maxhour; }
            set
            {
                if ((value < 0 || value > 24) && value != GlobalConsts.DefaultValue)
                    throw new ArgumentException("Hour cant be less then 0 or more then 24");
                maxhour = value;
            }
        }

        /// <summary>
        /// 0..24
        /// </summary>
        public int MinHour
        {
            get
            {
                return minhour;
            }
            set
            {
                if ((value < 0 || value > 24) && value != GlobalConsts.DefaultValue)
                    throw new ArgumentException("Hour cant be less then 0 or more then 24");
                minhour = value;
            }
        }

        public TimeBounds Clone()
        {
            return (TimeBounds)MemberwiseClone();
        }

        public override bool Equals(object obj)
        {
            if(obj is TimeBounds)
            {
                return ((TimeBounds)obj).Equals(this);
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return MaxYear.GetHashCode()>>1 ^
                MinYear.GetHashCode() >> 2 ^
                MaxDay.GetHashCode() << 1 ^
                MinDay.GetHashCode() << 4 ^
                MaxHour.GetHashCode() >> 2 ^
                MinHour.GetHashCode() >> 1 ;
        }
        #region IEquatable<TimeBounds> Members

        public bool Equals(TimeBounds first)
        {
            return (MaxYear    == first.MaxYear  &&
                        MinYear == first.MinYear &&
                        MaxDay  == first.MaxDay  &&
                        MinDay  == first.MinDay  &&
                        MaxHour == first.MaxHour &&
                        MinHour == first.MinHour);
        }

        #endregion
    }

}

namespace Microsoft.Research.Science.Data.Climate.Common
{

    public class SpatialCell
    {
        public SpatialCell(double latmin, double latmax, double lonmin, double lonmax)
        {
            this.latmin = latmin;
            this.latmax = latmax;
            this.lonmin = lonmin;
            this.lonmax = lonmax;
        }

        public bool Equals(SpatialCell second)
        {
            if (second == null) return false;
            return (latmax == second.latmax) &&
                (latmin == second.latmin) &&
                (lonmax == second.lonmax) &&
                (lonmin == second.lonmin);
        }

        override public bool Equals(object obj)
        {
            if (obj is SpatialCell)
                return Equals((SpatialCell)obj);
            else return false;
        }

        public static bool operator ==(SpatialCell first, SpatialCell second)
        {
            if (first is null) return second is null;
            return first.Equals(second);
        }
        public static bool operator !=(SpatialCell first, SpatialCell second) => !(first == second);

        protected int hash = 0;
        protected bool isChecked = false;
        protected bool isCoordinatesSynchronized = false;

        protected void Rehash()
        {
            hash = GetHashCode();
        }

        public bool IsChecked
        {
            get { return isChecked; }
        }

        public bool IsCoordinatesSyncronized
        {
            get { return isCoordinatesSynchronized; }
        }

        public void SetChecked()
        {
            isChecked = true;
        }

        public void SetCoordinatesSynchronized()
        {
            isCoordinatesSynchronized = true;
        }

        protected double latmin, latmax, lonmin, lonmax;

        public double LonMax
        {
            get { return lonmax; }
            set
            {
                if (lonmax != value)
                {
                    lonmax = value;
                    Rehash();
                }
            }
        }
        public double LonMin
        {
            get { return lonmin; }
            set
            {
                if (lonmin != value)
                {
                    lonmin = value; Rehash();
                }
            }
        }
        public double LatMax
        {
            get { return latmax; }
            set
            {
                if (latmax != value)
                {
                    latmax = value; Rehash();
                }
            }
        }

        public double LatMin
        {
            get { return latmin; }
            set
            {
                if (latmin != value)
                {
                    latmin = value; Rehash();
                }
            }
        }

        public override int GetHashCode()
        {
            //TODO save hashing results
            int r =
                latmin.GetHashCode() ^
                latmax.GetHashCode() >> 1 ^
                lonmin.GetHashCode() ^
                lonmax.GetHashCode() << 3;
            return r;
        }

        public override string ToString()
        {
            return String.Format("Lats({0}:{1}),Lons({2}:{3})", LatMin, LatMax, LonMin, LonMax);
        }
    }

    public enum LandOceanCoverage { Undefined, LandOceanArea, LandArea, OceanArea };

    public class SpatialGrid : Microsoft.Research.Science.Data.Climate.Common.SpatialCell
    {
        public SpatialGrid(double latmin, double latmax, double lonmin, double lonmax, double stepLat = double.NaN, double stepLon = double.NaN)
            : base(latmin, latmax, lonmin, lonmax)
        {
            StepLat = stepLat;
            StepLon = stepLon;
        }

        public double StepLat { get; set; }
        public double StepLon { get; set; }

        public override string ToString()
        {
            if (latmin == latmax && lonmin == lonmax)
                return String.Format("point {0}; {1}", latmin, lonmin);
            else if (double.IsNaN(StepLat) && double.IsNaN(StepLon))
                return String.Format("region {0};{1}/{2};{3}", latmin, lonmin, latmax, lonmax);
            else return String.Format("grid {0};{1}/{2};{3} with a step {4} by lat and {5} by lon", latmin, lonmin, latmax, lonmax, StepLat, StepLon);
        }
    }

    public enum FetchingTimeModes { TsYearly, TsSeasonly, TsHourly, Single };

    public class CellQuery : SpatialCell
    {
        public int ID;

        /// <summary>
        /// Returns a parent request GUID. The GUID of the dataset that contains the request
        /// </summary>
        public Guid ParentRequestGuid
        {
            get
            {
                return parentRequestGuid;
            }
        }


        protected Guid parentRequestGuid;
        protected Service.ClimateParameter parameter;

        public LandOceanCoverage CoverageType
        {
            get
            {
                return landOceanCoverage;
            }
            set
            {
                if (landOceanCoverage != value)
                {
                    landOceanCoverage = value; Rehash();
                }
            }
        }

        public bool IsFieldsChangeControlEnabled
        { get; set; }

        protected void MarkAsChanged()
        {
            if (IsFieldsChangeControlEnabled)
                isChecked = false;
            //isCoordinatesSynchronized = false;
        }

        protected TimeBounds tb = new TimeBounds();

        public TimeBounds TimeBounds
        {
            get
            {
                return tb;
            }
        }
        public int YearMax
        {
            get { return tb.MaxYear; ; }
            set
            {
                if (tb.MaxYear != value)
                {
                    tb.MaxYear = value; Rehash();
                    MarkAsChanged();
                }
            }
        }
        public int YearMin
        {
            get { return tb.MinYear; }
            set
            {
                if (tb.MinYear != value)
                { tb.MinYear = value; Rehash(); MarkAsChanged(); }
            }
        }

        /// <summary>
        /// Inclusive 1..360
        /// </summary>
        public int DayMax
        {
            get { return tb.MaxDay; }
            set
            {
                if (tb.MaxDay != value)
                {
                    tb.MaxDay = value; Rehash(); MarkAsChanged();
                }
            }
        }

        /// <summary>
        /// Inclusive 1..360
        /// </summary>
        public int DayMin
        {
            get { return tb.MinDay; }
            set
            {
                if (tb.MinDay != value)
                {
                    tb.MinDay = value; Rehash(); MarkAsChanged();
                }
            }
        }

        /// <summary>
        /// 0..24
        /// </summary>
        public int HourMax
        {
            get { return tb.MaxHour; }
            set
            {
                if (tb.MaxHour != value)
                {
                    tb.MaxHour = value; Rehash(); MarkAsChanged();
                }
            }
        }
        /// <summary>
        /// 0..24
        /// </summary>
        public int HourMin
        {
            get { return tb.MinHour; }
            set
            {
                if (tb.MinHour != value)
                {
                    tb.MinHour = value; Rehash(); MarkAsChanged();
                }
            }
        }
        public CellQuery Clone()
        {
            return (CellQuery)base.MemberwiseClone();
        }

        protected LandOceanCoverage landOceanCoverage = LandOceanCoverage.LandOceanArea;

        public Service.ClimateParameter Parameter
        {
            get { return parameter; }
            set
            {
                if (parameter != value)
                {
                    parameter = value; Rehash();
                }
            }
        }

        public CellQuery(Service.ClimateParameter parameter, LandOceanCoverage coverage, double latmin, double latmax, double lonmin, double lonmax, int hourmin, int hourmax, int daymin, int daymax, int yearmin, int yearmax, Guid parentRequestGuid, int ID = -1)
            :this(parameter,coverage,latmin,latmax,lonmin,lonmax,hourmin,hourmax,daymin,daymax,yearmin,yearmax,ID)
        {
            this.parentRequestGuid = parentRequestGuid;
        }

        public CellQuery(Service.ClimateParameter parameter, LandOceanCoverage coverage, double latmin, double latmax, double lonmin, double lonmax, int hourmin, int hourmax, int daymin, int daymax, int yearmin, int yearmax,int ID=-1)
            : base(latmin, latmax, lonmin, lonmax)
        {
            this.parameter = parameter;
            this.landOceanCoverage = coverage;
            HourMax = hourmax;
            HourMin = hourmin;
            DayMax = daymax;
            DayMin = daymin;
            YearMax = yearmax;
            YearMin = yearmin;
            this.ID = ID;            
            Rehash();
            IsFieldsChangeControlEnabled = true;
        }

        //public override bool Equals(object obj)
        //{
        //    CellQuery snd = obj as CellQuery;
        //    return Equals2(snd);
        //}

        //public bool Equals2(CellQuery other)
        //{
        //    if (other == null)
        //        return false;
        //    if (hash != other.hash)
        //        return false;
        //    bool b = (other.latmax != latmax) ||
        //         (other.latmin != latmin) ||
        //         (other.lonmax != lonmax) ||
        //         (other.lonmin != lonmin) ||
        //         (other.DayMax != DayMax) ||
        //         (other.DayMin != DayMin) ||
        //         (other.HourMax != HourMax) ||
        //         (other.HourMin != HourMin) ||
        //         (other.YearMax != YearMax) ||
        //         (other.YearMin != YearMin) ||
        //         (other.parameter != parameter);
        //    return !b;
        //}


        public override int GetHashCode()
        {
            //TODO save hashing results
            int r = (int)parameter << 2 ^
                base.GetHashCode() ^
                YearMin.GetHashCode() ^
                YearMax.GetHashCode() << 1 ^
                DayMin.GetHashCode() ^
                DayMax.GetHashCode() << 2 ^
                HourMin.GetHashCode() ^
                HourMax.GetHashCode() >> 2 ^
                (int)landOceanCoverage << 3;
            return r;
        }

        public override string ToString()
        {
            return String.Format("Lats({0}|{1}),Lons({2}|{3}),Times({4}:{5}:{6}|{7}:{8}:{9})", LatMin, LatMax, LonMin, LonMax, YearMin, DayMin, HourMin, YearMax, DayMax, HourMax);
        }
    }
}

namespace Microsoft.Research.Science.Data.Climate.Utils
{
    public class Converters
    {
        static readonly int[] dayInMonth =
            new int[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        static readonly Dictionary<int, int> toRealDays = new Dictionary<int, int>(360);
        static readonly Dictionary<int, int> toRealDaysLeap = new Dictionary<int, int>(360);
        static readonly Dictionary<int, int> to360Days = new Dictionary<int, int>(365);
        static readonly Dictionary<int, int> to360DaysLeap = new Dictionary<int, int>(366);

        public static int ConvertFromRealDayNumberTo360DayNumber(int dayNumber, bool isLeapYear)
        {
            if (dayNumber == GlobalConsts.DefaultValue)
                return GlobalConsts.DefaultValue;
            if (isLeapYear)
            {
                if (!to360DaysLeap.ContainsKey(dayNumber))
                    throw new ArgumentException(string.Format("{0} is not a valid day namber", dayNumber));
            }
            else
                if (!to360Days.ContainsKey(dayNumber))
                    throw new ArgumentException(string.Format("{0} is not a valid day namber", dayNumber));
            return isLeapYear ? to360DaysLeap[dayNumber] : to360Days[dayNumber];
        }

        static Converters()
        {
            for (int dayNumber = 1; dayNumber <= 365; dayNumber++)
            {
                int res360 = (int)Math.Round((double)dayNumber / 365.0 * 360.0);
                to360Days.Add(dayNumber, res360);
                if (!toRealDays.ContainsKey(res360))
                    toRealDays.Add(res360, dayNumber);
            }

            for (int dayNumber = 1; dayNumber <= 366; dayNumber++)
            {
                int res360 = (int)Math.Round((double)dayNumber / 366.0 * 360.0);
                to360DaysLeap.Add(dayNumber, res360);
                if (!toRealDaysLeap.ContainsKey(res360))
                    toRealDaysLeap.Add(res360, dayNumber);
            }
        }

        public static int ConvertFrom360DayNumberToRealDayNumber(int dayNumber, bool isLeapYear)
        {
            if (dayNumber == GlobalConsts.DefaultValue)
                return GlobalConsts.DefaultValue;
            if (isLeapYear)
            {
                if (!toRealDaysLeap.ContainsKey(dayNumber))
                    throw new ArgumentException(string.Format("{0} is not a valid day namber", dayNumber));
            }
            else
                if (!toRealDays.ContainsKey(dayNumber))
                    throw new ArgumentException(string.Format("{0} is not a valid day namber", dayNumber));
            return isLeapYear ? toRealDaysLeap[dayNumber] : toRealDays[dayNumber];
        }
    }
}
