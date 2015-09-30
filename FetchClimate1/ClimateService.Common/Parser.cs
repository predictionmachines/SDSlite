using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Microsoft.Research.Science.Data.Climate.Conventions;

namespace Microsoft.Research.Science.Data.Climate.Common
{
    public class Parser
    {
        /// <summary>
        /// Parse the parameters and returns filled MultiRequestStructure
        /// </summary>
        /// <param name="args">A paramters to parse</param>
        /// <param name="regDelimiter">A delimiter in the latitude values (start:step:stop - ':' is a delimiter)</param>
        /// <returns></returns>
        public static MultipleRequestDescriptor Parse(string[] args,char regDelimiter=':')
        {
            List<SpatialGrid> queries = new List<SpatialGrid>();
            ClimateParameter parameter = ClimateParameter.FC_TEMPERATURE;
            Tuple<int, int> days = new Tuple<int, int>(GlobalConsts.DefaultValue, GlobalConsts.DefaultValue);
            Tuple<int, int> hours = new Tuple<int, int>(GlobalConsts.DefaultValue, GlobalConsts.DefaultValue);
            Tuple<int, int> years = Tuple.Create(GlobalConsts.StartYear, GlobalConsts.EndYear);
            FetchingTimeModes activeMode = FetchingTimeModes.Single;
            int currentArgNum = 0;
            int timeStep = 1;
            int paramsEnteredCounter = 0;
            while (currentArgNum != args.Length)
            {
                switch (args[currentArgNum++].ToLower()) //  <---- moves the cursor!!!
                {
                    
                    case "/ty":
                        if (!int.TryParse(args[currentArgNum++], out timeStep))
                            throw new ArgumentException("Years step is not an integer in the yearly timeseries specification. Syntax is [/ty YearsStep]");
                        activeMode = FetchingTimeModes.TsYearly;
                        continue;
                    case "/ts":
                        if (!int.TryParse(args[currentArgNum++], out timeStep))
                            throw new ArgumentException("Days step is not an integer in the seasonly timeseries specification. Syntax is [/ts DaysStep]");
                        activeMode = FetchingTimeModes.TsSeasonly;
                        continue;
                    case "/th":
                        if (!int.TryParse(args[currentArgNum++], out timeStep))
                            throw new ArgumentException("Hourss step is not an integer in the hourly timeseries specification. Syntax is [/th HoursStep]");
                        activeMode = FetchingTimeModes.TsHourly;
                        continue;
                    case "/years":
                        years = ParseYears(ref currentArgNum, args);
                        continue;
                    case "/days":
                        days = ParseDays(ref currentArgNum, args);
                        continue;
                    case "/hours":
                        hours = ParseHours(ref currentArgNum, args);
                        continue;
                    default:
                        currentArgNum--; // <---- false cursor moving, moving it back
                        break;
                }
                if (paramsEnteredCounter == 0)
                {
                    parameter = ParseParam(ref currentArgNum, args);
                    paramsEnteredCounter++;
                    continue;
                }
                if (paramsEnteredCounter == 1)
                {
                    queries.Add(ParseRegion(ref currentArgNum, args));
                    paramsEnteredCounter++;
                    continue;
                }
                queries.Add(ParseRegion(ref currentArgNum, args));
            }
            if (paramsEnteredCounter < 1)
                throw new ArgumentException("Climate variable is not set.");
            MultipleRequestDescriptor mrd = new MultipleRequestDescriptor(parameter, new TimeBounds() { MinDay = days.Item1, MaxDay = days.Item2, MinYear = years.Item1, MaxYear = years.Item2, MinHour = hours.Item1, MaxHour = hours.Item2 }, activeMode);
            foreach (var q in queries)
                mrd.AddRegion(q);
            mrd.TimeStep = timeStep;
            return mrd;
        }


        public static Tuple<int, int> ParseDays(ref int pos, string[] args)
        {
            int startday = ParseDay(args[pos++]);
            if (pos == args.Length)
                throw new ArgumentException("Not enough values for days specification. Start day is specified but stop day is not.");
            int stopday = ParseDay(args[pos++], startday);
            return new Tuple<int, int>(startday, stopday);
        }

        public static Tuple<int, int> ParseYears(ref int pos, string[] args)
        {
            if (pos == args.Length)
                throw new ArgumentException("Years bounds are not set");
            int startyear = ParseYear(args[pos++]);
            if (pos == args.Length)
                throw new ArgumentException("Not enough values for days specification. Start day is specified but stop day is not.");
            int stopyear = ParseYear(args[pos++], startyear);
            return new Tuple<int, int>(startyear, stopyear);
        }

        public static Tuple<int, int> ParseHours(ref int pos, string[] args)
        {
            int starthour = ParseHour(args[pos++]);
            if (pos == args.Length)
                throw new ArgumentException("Not enough values for hours specification. Start hour is specifed but stop hour is not.");
            int stophour = ParseHour(args[pos++], starthour);
            return new Tuple<int, int>(starthour, stophour);
        }

        public static Tuple<double, double, double> ParseLats(ref int pos, string[] args, char regDelimiter = ':')
        {
            if (pos == args.Length)
                throw new ArgumentException("Latitude is not set");
            string s = args[pos++];
            string[] lats = s.Split(new char[] { regDelimiter }, StringSplitOptions.RemoveEmptyEntries);
            double latmin = ParseLat(lats[0]);
            double latmax = (lats.Length >= 2) ? (ParseLat(lats[lats.Length - 1], latmin)) : latmin;
            double step = (lats.Length > 2) ? (double.Parse(lats[1], NumberStyles.Float, CultureInfo.InvariantCulture)) : double.NaN;
            return new Tuple<double, double, double>(latmin, step, latmax);
        }


        public static Tuple<double, double, double> ParseLons(ref int pos, string[] args, char regDelimiter = ':')
        {
            if (pos == args.Length)
                throw new ArgumentException("Longatude is not set");

            string s = args[pos++];
            string[] lons = s.Split(new char[] { regDelimiter }, StringSplitOptions.RemoveEmptyEntries);
            double lonmin = ParseLon(lons[0]);
            double lonmax = (lons.Length >= 2) ? (ParseLon(lons[lons.Length - 1], lonmin)) : lonmin;
            double step = (lons.Length > 2) ? (double.Parse(lons[1],NumberStyles.Float,CultureInfo.InvariantCulture)) : double.NaN;
            return new Tuple<double, double, double>(lonmin, step, lonmax);
        }

        public static SpatialGrid ParseRegion(ref int pos, string[] args,char regDelimiter = ':')
        {
            if (pos == args.Length)
                throw new ArgumentException("Spatial region is not specified");
            var lats = ParseLats(ref pos, args,regDelimiter);
            var lons = ParseLons(ref pos, args,regDelimiter);
            if (lats.Item1 == lats.Item3 ^ lons.Item1 == lons.Item3)
                throw new ArgumentException("You've selected a region with zero area. Please select either point or non-empty region");
            if (double.IsNaN(lats.Item2) ^ double.IsNaN(lons.Item2))
                throw new ArgumentException("Please specify a grid step for both latitude and longatude");
            return new SpatialGrid(lats.Item1, lats.Item3, lons.Item1, lons.Item3, lats.Item2, lons.Item2);
        }

        public static ClimateParameter ParseParam(ref int paramNum, string[] args)
        {
            if (paramNum == args.Length)
                throw new ArgumentException("Parameter type is not present");
            string str = args[paramNum++].ToUpper();

            try
            {
                if(Conventions.Namings.ParameterShortNames.Values.Select(v=> v.ToUpper()).Contains(str))
                {
                    foreach(var p in Conventions.Namings.ParameterShortNames)
                    {
                        if (str == p.Value)
                            return p.Key;
                    }
                }
                ClimateParameter pa = (ClimateParameter)(Enum.Parse(typeof(ClimateParameter), str, true));
                    return pa;
            }
            catch (ArgumentException exc)
            {                
                    throw new ArgumentException("Invalid climate parameter name. Check syntax.");
            }
            

        }

        public static double ParseLon(string arg)
        {
            double lon;
            if (!Double.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out lon))
                throw new ArgumentException(string.Format("Specified longitude is not a double(you've entered {0})", arg));
            if (lon < -180)
                throw new ArgumentException("Longitude value cannot be less than -180");
            else if (lon > 360)
                throw new ArgumentException("Longitude value cannot be greated than 360");
            return lon;
        }

        public static double ParseLon(string arg, double lonfrom)
        {
            double lonto = ParseLon(arg);
            double min = Math.Min(lonfrom, lonto);
            double max = Math.Max(lonfrom, lonto);
            if (min >= 0 && max <= 360 || min >= -180 && max <= 180)
                return lonto;
            else
                throw new ArgumentException("Longitude values shoud both be in [-180,180] or in [0,360] ranges");
        }

        public static double ParseLat(string arg)
        {
            double lat;
            if (!Double.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out lat))
                throw new ArgumentException(string.Format("Specified latitude is not a double(you've entered {0})", arg));
            if (lat < -90)
                throw new ArgumentException(string.Format("Latitude value cannot be less than -90 (you've entered {0})", arg));
            else if (lat > 90)
                throw new ArgumentException(string.Format("Latiitude value cannot be greated than 90 (you've entered {0})", arg));
            return lat;
        }

        public static double ParseLat(string arg, double latmin)
        {
            double latmax = ParseLat(arg);
            if (latmax >= latmin)
                return latmax;
            else
                throw new ArgumentException(string.Format("latmax should be greater or equal to latmin (you've entered {0} to {1})", latmin, arg));
        }

        public static int ParseHour(string arg)
        {
            int hour;
            if (!Int32.TryParse(arg, out hour))
                throw new ArgumentException(string.Format("Specified hour is not an integer(you've entered {0})", arg));
            if (hour == GlobalConsts.DefaultValue)
                hour = 0;
            if (hour < 0 || hour > 24)
                throw new ArgumentException(String.Format("Hour value should be 0..24 or {0} (you've entered {1})", GlobalConsts.DefaultValue, arg));
            return hour;
        }

        public static int ParseHour(string arg, int hourmin)
        {
            int hour;
            if (!Int32.TryParse(arg, out hour))
                throw new ArgumentException(string.Format("Specified hour is not an integer(you've entered {0})", arg));
            if (hour == GlobalConsts.DefaultValue)
                hour = 24;
            if (hour < 0 || hour > 24)
                throw new ArgumentException(String.Format("Hour value should be 0..24 or {0}", GlobalConsts.DefaultValue));
            if (hour < hourmin)
                throw new ArgumentException("hourmax should be greated or equal to hourmin");
            return hour;
        }

        public static int ParseDay(string arg)
        {
            int day;
            if (!Int32.TryParse(arg, out day))
                throw new ArgumentException(string.Format("Specified day number is not an integer(you've entered {0})", arg));
            if (day != GlobalConsts.DefaultValue && (day < 1 || day > 366))
                throw new ArgumentException(String.Format("Day value should be 1..366 or {0}", GlobalConsts.DefaultValue));
            return day;
        }

        public static int ParseDay(string arg, int daymin)
        {
            int daymax;
            if (!Int32.TryParse(arg, out daymax))
                throw new ArgumentException(string.Format("Specified day number is not an integer(you've entered {0})", arg));
            if (daymax == GlobalConsts.DefaultValue)
                return daymax;
            if (daymax < 1 || daymax > 366)
                throw new ArgumentException("Day value should be 1..366 or GlobalConsts.DefaultValue");
            if (daymin > daymax)
                throw new ArgumentException("daymax should be greater or equal to daymin");
            return daymax;
        }

        public static int ParseYear(string arg)
        {
            int yearmin;
            if (!Int32.TryParse(arg, out yearmin))
                throw new ArgumentException(string.Format("Specified year is not an integer(you've entered {0})", arg));
            return yearmin;
        }

        public static int ParseYear(string arg, int yearmin)
        {
            int yearmax;
            if (!Int32.TryParse(arg, out yearmax))
                throw new ArgumentException(string.Format("Specified year is not an integer(you've entered {0})", arg));
            if (yearmin > yearmax)
                throw new ArgumentException("yearmax should be greate or equal to yearmin");
            return yearmax;
        }

    }
}
