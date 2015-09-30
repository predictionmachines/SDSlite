using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Science.Data.Climate.Conventions;

namespace Microsoft.Research.Science.Data.Climate.Common
{
    public abstract class ProcessingClientBase
    {
        private const string CacheFolder = @"FcClientCache\";

        protected DataSetDiskCache dataSetCache;

        public void ClearCache()
        {
            dataSetCache.FullCacheClear();
        }

        public ProcessingClientBase()
        {
            string appFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            this.dataSetCache = new DataSetDiskCache(System.IO.Path.Combine(appFolder, CacheFolder));
        }

        public DataSet ServerProcess(DataSet ds)
        {
            if (ds == null)
                throw new ArgumentNullException("ds");

            string hash = DataSetDiskCache.ComputeHash(ds);

            var cachedDs = this.dataSetCache.Get(hash);

            if (cachedDs != null)
                return cachedDs;

            else
            {
                var result = this.ServerProcessInternal(ds);

                if (FetchClimateRequestBuilder.IsProcessingSuccessful(result))
                {
                    this.dataSetCache.Add(result);
                }
                return result;
            }
        }

        protected abstract DataSet ServerProcessInternal(DataSet ds);

        protected virtual DataSet LocalProcess(DataSet ds)
        {
            throw new NotImplementedException();
        }

        private void AssertFcParameters(double latmin, double latmax, double lonmin, double lonmax, int starthour, int stophour, int startday, int stopday, int startyear, int stopyear)
        {
            if (Math.Abs(latmax) > 90)
                throw new ArgumentException(string.Format("latmax must be in [-90;90]. You've passed {0}", latmax));
            if (Math.Abs(latmin) > 90)
                throw new ArgumentException(string.Format("latmin must be in [-90;90]. You've passed {0}", latmin));
            if (latmin > latmax)
                throw new ArgumentException(string.Format("latmin must be less or equel to latmax. You've passed {0} and {1} respectivly", latmin, latmax));
            if (lonmin < -180 || lonmin > 360)
                throw new ArgumentException(string.Format("lonmin must be in [-180;360]. You've passed {0}", lonmin));
            if (lonmax < -180 || lonmax > 360)
                throw new ArgumentException(string.Format("lonmax must be in [-180;360]. You've passed {0}", lonmax));

            if (startyear < 1)
                startyear = 1;
            if (startyear > 9999)
                startyear = 9999;
            if (stopyear < 1)
                stopyear = 1;
            if (stopyear > 9999)
                stopyear = 9999;


            int maxday = (startyear == stopyear && DateTime.IsLeapYear(startyear)) ? 366 : 365;

            if ((stopday < 0 || stopday > maxday) && (stopday != GlobalConsts.DefaultValue))
            {
                throw new ArgumentException(String.Format("The minimum day in the year doesn't fall into permited interval. You've entered {0}. However it must be set to 1..{1} or to GlobalConsts.DefaultValue.", stopday, maxday));
            }
            if ((startday < 0 || startday > maxday) && (startday != GlobalConsts.DefaultValue))
            {
                throw new ArgumentException(String.Format("The maximum day in the year doesn't fall into permited interval. You've entered {0}. However it must be set to 1..{1} or to GlobalConsts.DefaultValue.", startday,maxday));
            }
            if ((starthour < 0 || starthour > 24) && starthour != GlobalConsts.DefaultValue)
            {
                throw new ArgumentException(String.Format("starting hour in the day doesn't fall into permited interval. You've entered {0}. However it must be set to 0..24 or to GlobalConsts.DefaultValue.", starthour));
            }
            if (stophour != GlobalConsts.DefaultValue && (stophour < 0 || stophour > 24))
            {
                throw new ArgumentException(String.Format("The last hour in the day doesn't fall into permited interval. You've entered {0}. However it must be set to 0..24 or to GlobalConsts.DefaultValue.", stophour));
            }

            if (startyear > stopyear)
            {
                throw new ArgumentException(String.Format("The minimum requested year(you've entered {0}) must be less or equel to maximum requested year(you've entered {1}).", startyear, stopyear));
            }


            if ((stophour != GlobalConsts.DefaultValue && starthour != GlobalConsts.DefaultValue) && (starthour > stophour))
            {
                throw new ArgumentException("Hour min is more then hour max");
            }
            if ((startday != GlobalConsts.DefaultValue && stopday != GlobalConsts.DefaultValue) && (startday > stopday))
            {
                throw new ArgumentException("Daymin is more then day max");
            }

        }

        public double FetchClimate(ClimateParameter parameter, double latmin, double latmax, double lonmin, double lonmax, int starthour, int stophour, int startday, int stopday, int startyear, int stopyear)
        {
            AssertFcParameters(latmin, latmax, lonmin, lonmax, starthour, stophour, startday, stopday, startyear, stopyear);
            return (double)(((FetchClimateSingleResponce)FetchClimate(parameter, latmin, latmax, lonmin, lonmax, starthour, stophour, startday, stopday, startyear, stopyear, new FetchingOptions())).Value.GetValueInDefaultClientUnits());
        }

        public FetchClimateSingleResponce FetchClimate(ClimateParameter parameter, double latmin, double latmax, double lonmin, double lonmax, int starthour, int stophour, int startday, int stopday, int startyear, int stopyear, FetchingOptions options)
        {
            AssertFcParameters(latmin, latmax, lonmin, lonmax, starthour, stophour, startday, stopday, startyear, stopyear);
            using (var requestDs = DataSet.Open("msds:memory"))
            {
                FetchClimateRequestBuilder.FillDataSetWithRequest(requestDs, parameter, latmin, latmax, lonmin, lonmax, starthour, stophour, startday, stopday, startyear, stopyear, options);
                using (var result = ServerProcess(requestDs))
                {
                    return FetchClimateRequestBuilder.BuildSingleCellResult(result);
                }
            }
        }

        internal double[] FetchClimate(ClimateParameter parameter, double[] latmin, double[] latmax, double[] lonmin, double[] lonmax, int[] starthour, int[] stophour, int[] startday, int[] stopday, int[] startyear, int[] stopyear)
        {
            if (startday == null)
                startday = Enumerable.Repeat(GlobalConsts.DefaultValue, latmin.Length).ToArray();
            if (stopday == null)
                stopday = Enumerable.Repeat(GlobalConsts.DefaultValue, latmin.Length).ToArray();
            if (starthour == null)
                starthour = Enumerable.Repeat(GlobalConsts.DefaultValue, latmin.Length).ToArray();
            if (stophour == null)
                stophour = Enumerable.Repeat(GlobalConsts.DefaultValue, latmin.Length).ToArray();
            if (startyear == null)
                startyear = Enumerable.Repeat(GlobalConsts.StartYear, latmin.Length).ToArray();
            if (stopyear == null)
                stopyear = Enumerable.Repeat(GlobalConsts.EndYear, latmin.Length).ToArray();
            
            for (int i = 0; i < latmin.Length; i++)
                AssertFcParameters(latmin[i], latmax[i], lonmin[i], lonmax[i], starthour[i], stophour[i], startday[i], stopday[i], startyear[i], stopyear[i]);
            using (var requestDs = DataSet.Open("msds:memory"))
            {
                FetchClimateRequestBuilder.FillDataSetWithRequest(requestDs, parameter, latmin, latmax, lonmin, lonmax, starthour, stophour, startday, stopday, startyear, stopyear, new FetchingOptions());
                using (var result = ServerProcess(requestDs))
                {
                    return FetchClimateRequestBuilder.BuildBatchResult(result).Values.Select(v => v.GetValueInDefaultClientUnits()).ToArray();
                }
            }
        }

        public FetchClimateBatchResponce FetchClimate(ClimateParameter parameter, double[] latmin, double[] latmax, double[] lonmin, double[] lonmax, int[] starthour, int[] stophour, int[] startday, int[] stopday, int[] startyear, int[] stopyear, FetchingOptions options, ResearchVariationType variationType)
        {
            if (startday == null)
                startday = Enumerable.Repeat(GlobalConsts.DefaultValue, latmin.Length).ToArray();
            if (stopday == null)
                stopday = Enumerable.Repeat(GlobalConsts.DefaultValue, latmin.Length).ToArray();
            if (starthour == null)
                starthour = Enumerable.Repeat(GlobalConsts.DefaultValue, latmin.Length).ToArray();
            if (stophour == null)
                stophour = Enumerable.Repeat(GlobalConsts.DefaultValue, latmin.Length).ToArray();
            if (startyear == null)
                startyear = Enumerable.Repeat(GlobalConsts.StartYear, latmin.Length).ToArray();
            if (stopyear == null)
                stopyear = Enumerable.Repeat(GlobalConsts.EndYear, latmin.Length).ToArray();

            for (int i = 0; i < latmin.Length; i++)
                AssertFcParameters(latmin[i], latmax[i], lonmin[i], lonmax[i], starthour[i], stophour[i], startday[i], stopday[i], startyear[i], stopyear[i]);
            
            using (var requestDs = DataSet.Open("msds:memory"))
            {
                FetchClimateRequestBuilder.FillDataSetWithRequest(requestDs, parameter, latmin, latmax, lonmin, lonmax, starthour, stophour, startday, stopday, startyear, stopyear, options);
                requestDs.Metadata[Namings.metadataNameVariationType]=variationType.ToString();
                requestDs.Commit();
                using (var result = ServerProcess(requestDs))
                {
                    return FetchClimateRequestBuilder.BuildBatchResult(result);
                }
            }
        }

        public string[] FetchClimateYearlyProvenance(ClimateParameter p, double latmin, double latmax, double lonmin, double lonmax, int hourmin, int hourmax, int daymin, int daymax, int yearmin, int yearmax, int dyear, EnvironmentalDataSource dataSource)
        {
            return (string[])((FetchClimateBatchResponce)FetchClimateYearly(p, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dyear, new FetchingOptions(dataSource))).Values.Select(v => v.Provenance).ToArray();
        }


        public double[] FetchClimateYearlyUncertainty(ClimateParameter p, double latmin, double latmax, double lonmin, double lonmax, int hourmin, int hourmax, int daymin, int daymax, int yearmin, int yearmax, int dyear, EnvironmentalDataSource dataSource)
        {            
            return (double[])((FetchClimateBatchResponce)FetchClimateYearly(p, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dyear, new FetchingOptions(dataSource))).Values.Select(v => v.GetUncertaintyInDefaultClientUnits()).ToArray();
        }

        public double[] FetchClimateYearly(ClimateParameter p, double latmin, double latmax, double lonmin, double lonmax, int hourmin, int hourmax, int daymin, int daymax, int yearmin, int yearmax, int dyear,EnvironmentalDataSource dataSource)
        {            
            return (double[])((FetchClimateBatchResponce)FetchClimateYearly(p, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dyear, new FetchingOptions(dataSource))).Values.Select(v => v.GetValueInDefaultClientUnits()).ToArray();
        }

        public FetchClimateBatchResponce FetchClimateYearly(ClimateParameter p, double latmin, double latmax, double lonmin, double lonmax, int hourmin, int hourmax, int daymin, int daymax, int yearmin, int yearmax, int dyear, FetchingOptions options)
        {
            AssertFcParameters(latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax);
            
            return FetchClimateTimeSeries(p, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dyear, IntervalToSplit.Years, options);
        }

        public string[] FetchClimateSeasonlyProvenance(ClimateParameter p, double latmin, double latmax, double lonmin, double lonmax, int hourmin, int hourmax, int daymin, int daymax, int yearmin, int yearmax, int dday, EnvironmentalDataSource dataSource)
        {
            return (string[])((FetchClimateBatchResponce)FetchClimateSeasonly(p, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dday, new FetchingOptions(dataSource))).Values.Select(v => v.Provenance).ToArray();
        }


        public double[] FetchClimateSeasonlyUncertainty(ClimateParameter p, double latmin, double latmax, double lonmin, double lonmax, int hourmin, int hourmax, int daymin, int daymax, int yearmin, int yearmax, int dday, EnvironmentalDataSource dataSource)
        {
            return (double[])((FetchClimateBatchResponce)FetchClimateSeasonly(p, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dday, new FetchingOptions(dataSource))).Values.Select(v => v.GetUncertaintyInDefaultClientUnits()).ToArray();
        }

        public double[] FetchClimateSeasonly(ClimateParameter p, double latmin, double latmax, double lonmin, double lonmax, int hourmin, int hourmax, int daymin, int daymax, int yearmin, int yearmax, int dday, EnvironmentalDataSource dataSource)
        {
            return (double[])((FetchClimateBatchResponce)FetchClimateSeasonly(p, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dday, new FetchingOptions(dataSource))).Values.Select(v => v.GetValueInDefaultClientUnits()).ToArray();
        }

        public FetchClimateBatchResponce FetchClimateSeasonly(ClimateParameter p, double latmin, double latmax, double lonmin, double lonmax, int hourmin, int hourmax, int daymin, int daymax, int yearmin, int yearmax, int dday, FetchingOptions options)
        {
            AssertFcParameters(latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax);
            return FetchClimateTimeSeries(p, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dday, IntervalToSplit.Days, options);
        }

        public string[] FetchClimateHourlyProvenance(ClimateParameter p, double latmin, double latmax, double lonmin, double lonmax, int hourmin, int hourmax, int daymin, int daymax, int yearmin, int yearmax, int dhour, EnvironmentalDataSource dataSource)
        {
            return (string[])((FetchClimateBatchResponce)FetchClimateHourly(p, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dhour, new FetchingOptions(dataSource))).Values.Select(v => v.Provenance).ToArray();
        }


        public double[] FetchClimateHourlyUncertainty(ClimateParameter p, double latmin, double latmax, double lonmin, double lonmax, int hourmin, int hourmax, int daymin, int daymax, int yearmin, int yearmax, int dhour, EnvironmentalDataSource dataSource)
        {            
            return (double[])((FetchClimateBatchResponce)FetchClimateHourly(p, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dhour, new FetchingOptions(dataSource))).Values.Select(v => v.GetUncertaintyInDefaultClientUnits()).ToArray();
        }

        public double[] FetchClimateHourly(ClimateParameter p, double latmin, double latmax, double lonmin, double lonmax, int hourmin, int hourmax, int daymin, int daymax, int yearmin, int yearmax, int dhour, EnvironmentalDataSource dataSource)
        {            
            return (double[])((FetchClimateBatchResponce)FetchClimateHourly(p, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dhour, new FetchingOptions(dataSource))).Values.Select(v => v.GetValueInDefaultClientUnits()).ToArray();
        }


        public FetchClimateBatchResponce FetchClimateHourly(ClimateParameter p, double latmin, double latmax, double lonmin, double lonmax, int hourmin, int hourmax, int daymin, int daymax, int yearmin, int yearmax, int dhour, FetchingOptions options)
        {
            AssertFcParameters(latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax);
            return FetchClimateTimeSeries(p, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dhour, IntervalToSplit.Hours, options);
        }

        private FetchClimateBatchResponce FetchClimateTimeSeries(ClimateParameter p, double latmin, double latmax, double lonmin, double lonmax, int hourmin, int hourmax, int daymin, int daymax, int yearmin, int yearmax, int divider, IntervalToSplit intervalToSplit, FetchingOptions options)
        {
            int n;
            if (hourmax == GlobalConsts.DefaultValue)
                hourmax = 24;
            if (hourmin == GlobalConsts.DefaultValue)
                hourmin = 0;
            bool isReplaceDayMaxWithDefVal = (yearmax!=yearmin && (daymax==GlobalConsts.DefaultValue || daymax==365));            
            if (daymin == GlobalConsts.DefaultValue)
                daymin = 1;
            int effDayMax = daymax;
            if (effDayMax == GlobalConsts.DefaultValue)
                effDayMax = (yearmin == yearmax) ? (DateTime.IsLeapYear(yearmin) ? 366 : 365) : 365;
            ResearchVariationType rvt = ResearchVariationType.Auto;
            switch (intervalToSplit)
            {
                case IntervalToSplit.Years: n = (int)Math.Ceiling((yearmax - yearmin + 1) / (double)divider); rvt = ResearchVariationType.Yearly; break;
                case IntervalToSplit.Days: n = (int)Math.Ceiling((effDayMax - daymin + 1) / (double)divider); rvt = ResearchVariationType.Seasonly; break;
                case IntervalToSplit.Hours: n = (int)Math.Ceiling((hourmax - hourmin + 1) / (double)divider); rvt = ResearchVariationType.Hourly; break;
                default: throw new NotImplementedException();
            }
            double[] latmaxs = new double[n];
            double[] latmins = new double[n];
            double[] lonmaxs = new double[n];
            double[] lonmins = new double[n];
            int[] hourmins = new int[n];
            int[] hourmaxs = new int[n];
            int[] daymins = new int[n];
            int[] daymaxs = new int[n];
            int[] yearmins = new int[n];
            int[] yearmaxs = new int[n];

            int yi = (intervalToSplit == IntervalToSplit.Years) ? 1 : 0;
            int di = (intervalToSplit == IntervalToSplit.Days) ? 1 : 0;
            int hi = (intervalToSplit == IntervalToSplit.Hours) ? 1 : 0;

            for (int i = 0; i < n; i++)
            {
                latmaxs[i] = latmax;
                latmins[i] = latmin;
                lonmaxs[i] = lonmax;
                lonmins[i] = lonmin;
                hourmins[i] = Math.Min((hi==0)?hourmin:(hourmin + hi * i * divider),24);
                hourmaxs[i] = Math.Min((hi == 0) ? hourmax : (hourmin + hi * ((i + 1) * divider-1)), 24);
                yearmins[i] = (yi == 0) ? yearmin : (yearmin + yi * i * divider);
                yearmaxs[i] = (yi == 0) ? yearmax:(yearmin + yi * ((i + 1) * divider-1));
                daymins[i] = Math.Min((di==0)?daymin:(daymin + di *(i * divider)),(yearmins[i]==yearmaxs[i] && DateTime.IsLeapYear(yearmins[i]))?366:365);
                if (isReplaceDayMaxWithDefVal && intervalToSplit!=IntervalToSplit.Days)                                    
                    daymaxs[i] = GlobalConsts.DefaultValue;
                else
                    daymaxs[i] = Math.Min((di == 0) ? daymax : (daymin + di * ((i + 1) * divider - 1)), (yearmins[i] == yearmaxs[i] && DateTime.IsLeapYear(yearmins[i])) ? 366 : 365);
                
            }
            return FetchClimate(p, latmins, latmaxs, lonmins, lonmaxs, hourmins, hourmaxs, daymins, daymaxs, yearmins, yearmaxs, options,rvt);
        }

        public string[,] FetchProvenanceGrid(ClimateParameter parameter, double latmin, double latmax, double lonmin, double lonmax, int hourmin, int hourmax, int daymin, int daymax, int yearmin, int yearmax, double dlat, double dlon, EnvironmentalDataSource ds)
        {
            ClimateParameterValue[,] vals = FetchClimateGrid(parameter, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dlat, dlon, new FetchingOptions(ds)).Values;
            int l0 = vals.GetLength(0);
            int l1 = vals.GetLength(1);
            string[,] doubleVals = new string[l0, l1];
            for (int i = 0; i < l0; i++)
                for (int j = 0; j < l1; j++)
                    doubleVals[i, j] = vals[i, j].Provenance;
            return doubleVals;
        }

        public double[,] FetchUncertaintyGrid(ClimateParameter parameter, double latmin, double latmax, double lonmin, double lonmax, int hourmin, int hourmax, int daymin, int daymax, int yearmin, int yearmax, double dlat, double dlon, EnvironmentalDataSource ds)
        {
            ClimateParameterValue[,] vals = FetchClimateGrid(parameter, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dlat, dlon, new FetchingOptions(ds)).Values;
            int l0 = vals.GetLength(0);
            int l1 = vals.GetLength(1);
            double[,] doubleVals = new double[l0, l1];
            for (int i = 0; i < l0; i++)
                for (int j = 0; j < l1; j++)
                    doubleVals[i, j] = vals[i, j].GetUncertaintyInDefaultClientUnits();
            return doubleVals;
        }

        public double[,] FetchClimateGrid(ClimateParameter parameter, double latmin, double latmax, double lonmin, double lonmax, int hourmin, int hourmax, int daymin, int daymax, int yearmin, int yearmax, double dlat, double dlon,EnvironmentalDataSource ds)
        {
            ClimateParameterValue[,] vals = FetchClimateGrid(parameter, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dlat, dlon, new FetchingOptions(ds)).Values;
            int l0 = vals.GetLength(0);
            int l1 = vals.GetLength(1);
            double[,] doubleVals = new double[l0, l1];
            for (int i = 0; i < l0; i++)
                for (int j = 0; j < l1; j++)
                    doubleVals[i, j] = vals[i, j].GetValueInDefaultClientUnits();
            return doubleVals;
        }

        public FetchClimateGridResponce FetchClimateGrid(ClimateParameter parameter, double latmin, double latmax, double lonmin, double lonmax, int hourmin, int hourmax, int daymin, int daymax, int yearmin, int yearmax, double dlat, double dlon, FetchingOptions options)
        {
            int nlat = (int)Math.Round((latmax - latmin) / dlat);
            int nlon = (int)Math.Round((lonmax - lonmin) / dlon);
            double[] latsGrid = new double[nlat + 1];
            double[] lonsGrid = new double[nlon + 1];
            for (int i = 0; i < latsGrid.Length; i++)
                latsGrid[i] = latmin + i * dlat;
            for (int i = 0; i < lonsGrid.Length; i++)
                lonsGrid[i] = lonmin + i * dlon;
            int[] hourmins = new int[nlat * nlon];
            int[] hourmaxs = new int[nlat * nlon];
            int[] daymins = new int[nlat * nlon];
            int[] daymaxs = new int[nlat * nlon];
            int[] yearmins = new int[nlat * nlon];
            int[] yearmaxs = new int[nlat * nlon];
            double[] latmins = new double[nlat * nlon];
            double[] latmaxs = new double[nlat * nlon];
            double[] lonmins = new double[nlat * nlon];
            double[] lonmaxs = new double[nlat * nlon];
            int rowIndex, colIndex;
            for (int i = 0; i < nlat * nlon; i++)
            {
                rowIndex = i / nlon;
                colIndex = i % nlon;

                lonmins[i] = lonsGrid[colIndex];
                lonmaxs[i] = lonsGrid[colIndex + 1];
                latmins[i] = latsGrid[rowIndex];
                latmaxs[i] = latsGrid[rowIndex + 1];

                hourmins[i] = hourmin;
                hourmaxs[i] = hourmax;
                daymins[i] = daymin;
                daymaxs[i] = daymax;
                yearmins[i] = yearmin;
                yearmaxs[i] = yearmax;
            }

            var resB = FetchClimate(parameter, latmins, latmaxs, lonmins, lonmaxs, hourmins, hourmaxs, daymins, daymaxs, yearmins, yearmaxs, options, ResearchVariationType.Spatial);

            ClimateParameterValue[] result = resB.Values;
            ClimateParameterValue[,] result2D = new ClimateParameterValue[nlat, nlon];
            for (int i = 0; i < nlat * nlon; i++)
            {
                rowIndex = i / nlon;
                colIndex = i % nlon;
                result2D[rowIndex, colIndex] = result[i];
            }
            FetchClimateGridResponce res = new FetchClimateGridResponce();
            res.Values = result2D;
            res.ServiceVersion = resB.ServiceVersion;
            return res;
        }

        protected DataSet GetFromCache(string hash)
        {
            var dataSetCache = this.dataSetCache;
            var ds = dataSetCache.Get(hash);

            return ds;
        }

        protected void PutToCache(DataSet ds)
        {
            this.dataSetCache.Add(ds);
        }
    }
}
