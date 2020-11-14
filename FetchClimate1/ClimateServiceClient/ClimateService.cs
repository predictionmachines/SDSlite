// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Threading;
using System.Reflection;
using System.Configuration;
using System.IO;
using Microsoft.Research.Science.Data.Climate.Conventions;
using Microsoft.Research.Science.Data.Climate.Common;
using Microsoft.Research.Science.Data.Processing;
using System.Linq;
using Microsoft.Research.Science.Data.Climate;
#if !RELEASE_ASSEMBLY
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("FetchClimateRestCoreTests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("FetchClimateLocalHostRole")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DataProcessorsTest")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Fetch_climate_cs")]
#endif
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("FetchClimateCs, PublicKey=0024000004800000940000000602000000240000525341310004000001000100FFE77CE89295F9E007DEACC009BEA989FF098FE9D1E0CC2775445942148322DB5DB33A71CACCB20AE03AFDDEAFFAF4BF48D2EEDA7A1F63D4C7226C627B51774BCC0BABD810B557AE01C3DFE7233E1B902EFCDFC4EB84518EC217465A1B4C5221145F252B44E2FBAC78FC94E779C448C1EB340F53DC523DD398804739DB1260AF")]

namespace Microsoft.Research.Science.Data
{   
    public static class ClimateService
    {
        private static string _ServiceUri = "http://fetchclimatesvc.cloudapp.net/fetch2";
        private static string _CommunicationType = "REST";

        private static ProcessingClientBase _fetchClimate;        

        private static ProcessingClientBase ClientInstance
        {
            get
            {
                if (_fetchClimate == null)
                {
#if FC_WCFCLIENT
                    if (_CommunicationType == "WCF")
                        _fetchClimate = new WcfProcessingClient(_ServiceUri);
                    else 
#endif
                        if (_CommunicationType == "REST")
                        _fetchClimate = new RestProcessingClient();
                }
                return _fetchClimate;
            }
        }

        static ClimateService()
        {
            var settings = Microsoft.Research.Science.Data.Climate.ServiceLocationConfiguration.Current;
            if (settings != null)
            {
                _ServiceUri = settings.ServiceURL;
                _CommunicationType = settings.CommunicationProtocol;
            }            
        }

        internal static void SetProcessingClient(ProcessingClientBase client)
        {
            ClimateService._fetchClimate = client;
        }

        /// <summary>
        /// Returns the provenance of the value calculated by FetchClimate of specified climate parameter for the given geographical region at the given time of the day, season and years interval.
        /// </summary>
        /// <param name="parameter">A climate variable that will be evaluated for provenance</param>
        /// <param name="latmin">Minimum latitude of the requested region</param>
        /// <param name="lonmin">Mimimun longatude of the requested region</param>
        /// <param name="latmax">Maximum latitude of the requesed region </param>
        /// <param name="lonmax">Maximum longatude of the requesed region</param>
        /// <param name="startyear">The first year that will be used to form time intervals of the request. Default value is 1961</param>
        /// <param name="startday">The first day of the year (1..365) that will be used to form time intervals of the request. Default value is 1</param>
        /// <param name="starthour">The time in hours of the day (0..24) that will be used as a lower inclusive bound to form time intervals of the request. Default value is 0</param>
        /// <param name="stopyear">The last year(inclusivly) that will be used to form time intervals for request. Default value is 1990</param>
        /// <param name="stopday">The last day of the year(inclusivly) that will be used to form time intervals of the request. Use GlobalConsts.DefaultValue to specify the last day of the year. Default value is "the last day of the year"</param>
        /// <param name="stophour">The time in hours of the day (0..24) that will be used as an upper inclusive bound to form time intervals of the request. Use 24 to specify the end of the day. Default value is 24</param>
        /// <param name="dataSource">The data source to use for calculation. By default the data source with lowest uncertainty is chosen automaticly</param>
        public static string FetchClimateProvenance(ClimateParameter parameter, double latmin, double latmax, double lonmin, double lonmax, int starthour = 0, int stophour = 24, int startday = 1, int stopday = GlobalConsts.DefaultValue, int startyear = 1961, int stopyear = 1990, EnvironmentalDataSource dataSource = EnvironmentalDataSource.ANY)
        {
            return ((FetchClimateSingleResponce)(FetchClimateEx(parameter, new FetchingOptions(dataSource), latmin, latmax, lonmin, lonmax, starthour, stophour, startday, stopday, startyear, stopyear))).Value.Provenance;
        }

        /// <summary>
        /// Returns an uncertainty of the value calculated by FetchClimate of specified climate parameter for the given geographical region at the given time of the day, season and years interval.
        /// </summary>
        /// <param name="parameter">A climate variable that will be evaluated for mean value</param>
        /// <param name="latmin">Minimum latitude of the requested region</param>
        /// <param name="lonmin">Mimimun longatude of the requested region</param>
        /// <param name="latmax">Maximum latitude of the requesed region </param>
        /// <param name="lonmax">Maximum longatude of the requesed region</param>
        /// <param name="startyear">The first year that will be used to form time intervals of the request. Default value is 1961</param>
        /// <param name="startday">The first day of the year (1..365) that will be used to form time intervals of the request. Default value is 1</param>
        /// <param name="starthour">The time in hours of the day (0..24) that will be used as a lower inclusive bound to form time intervals of the request. Default value is 0</param>
        /// <param name="stopyear">The last year(inclusivly) that will be used to form time intervals for request. Default value is 1990</param>
        /// <param name="stopday">The last day of the year(inclusivly) that will be used to form time intervals of the request. Use GlobalConsts.DefaultValue to specify the last day of the year. Default value is "the last day of the year"</param>
        /// <param name="stophour">The time in hours of the day (0..24) that will be used as an upper inclusive bound to form time intervals of the request. Use 24 to specify the end of the day. Default value is 24</param>
        /// <param name="dataSource">The data source to use for calculation. By default the data source with lowest uncertainty is chosen automaticly</param>
        public static double FetchClimateUncertainty(ClimateParameter parameter, double latmin, double latmax, double lonmin, double lonmax, int starthour = 0, int stophour = 24, int startday = 1, int stopday = GlobalConsts.DefaultValue, int startyear = 1961, int stopyear = 1990, EnvironmentalDataSource dataSource = EnvironmentalDataSource.ANY)
        {
            return ((FetchClimateSingleResponce)(FetchClimateEx(parameter, new FetchingOptions(dataSource), latmin, latmax, lonmin, lonmax, starthour, stophour, startday, stopday, startyear, stopyear))).Value.GetUncertaintyInDefaultClientUnits();
        }

        /// <summary>
        /// Returns mean value of specified climate parameter for the given geographical region at the given time of the day, season and years interval.
        /// </summary>
        /// <param name="parameter">A climate variable that will be evaluated for mean value</param>
        /// <param name="latmin">Minimum latitude of the requested region</param>
        /// <param name="lonmin">Mimimun longatude of the requested region</param>
        /// <param name="latmax">Maximum latitude of the requesed region </param>
        /// <param name="lonmax">Maximum longatude of the requesed region</param>
        /// <param name="startyear">The first year that will be used to form time intervals of the request. Default value is 1961</param>
        /// <param name="startday">The first day of the year (1..365) that will be used to form time intervals of the request. Default value is 1</param>
        /// <param name="starthour">The time in hours of the day (0..24) that will be used as a lower inclusive bound to form time intervals of the request. Default value is 0</param>
        /// <param name="stopyear">The last year(inclusivly) that will be used to form time intervals for request. Default value is 1990</param>
        /// <param name="stopday">The last day of the year(inclusivly) that will be used to form time intervals of the request. Use GlobalConsts.DefaultValue to specify the last day of the year. Default value is "the last day of the year"</param>
        /// <param name="stophour">The time in hours of the day (0..24) that will be used as an upper inclusive bound to form time intervals of the request. Use 24 to specify the end of the day. Default value is 24</param>
        /// <param name="dataSource">The data source to use for calculation. By default the data source with lowest uncertainty is chosen automaticly</param>
        public static double FetchClimate(ClimateParameter parameter, double latmin, double latmax, double lonmin, double lonmax, int starthour=0, int stophour=24, int startday=1, int stopday=GlobalConsts.DefaultValue, int startyear=1961, int stopyear = 1990,EnvironmentalDataSource dataSource=EnvironmentalDataSource.ANY)
        {
            return ((FetchClimateSingleResponce)(FetchClimateEx(parameter,new FetchingOptions(dataSource), latmin, latmax, lonmin, lonmax, starthour, stophour, startday, stopday, startyear, stopyear))).Value.GetValueInDefaultClientUnits();
        }

        /// <summary>
        /// Batch version of single fetch climate request that returns provenances of the results. The parameteras are passed as a seperate arrays.
        /// </summary>
        /// <param name="parameter">A climate parameter that will be evaluated for mean value</param>
        /// <param name="latmin">Minimum latitude of the requested region</param>
        /// <param name="lonmin">Mimimun longatude of the requested region</param>
        /// <param name="latmax">Maximum latitude of the requesed region </param>
        /// <param name="lonmax">Maximum longatude of the requesed region</param>
        /// <param name="startyear">The first year that will be used to form time intervals of the request. The null value means 1961 (default)</param>
        /// <param name="startday">The first day of the year (1..365) that will be used to form time intervals of the request. The null value means 1</param>
        /// <param name="starthour">The time in hours of the day (0..24) that will be used as a lower inclusive bound to form time intervals of the request. The null value means 0</param>
        /// <param name="stopyear">The last year(inclusivly) that will be used to form time intervals for request. The null value means 1990</param>
        /// <param name="stopday">The last day of the year(inclusivly) that will be used to form time intervals of the request. Use GlobalConsts.DefaultValue to specify the last day of the year. The null value means "the last day of the year"</param>
        /// <param name="stophour">The time in hours of the day (0..24) that will be used as an upper inclusive bound to form time intervals of the request. Use 24 to specify the end of the day. The null value means 24</param>
        /// <param name="dataSource">The data source to use for calculation. By default the data source with lowest uncertainty is chosen automaticly</param> 
        public static string[] FetchClimateProvenance(ClimateParameter parameter, double[] latmin, double[] latmax, double[] lonmin, double[] lonmax, int[] starthour = null, int[] stophour = null, int[] startday = null, int[] stopday = null, int[] startyear = null, int[] stopyear = null, EnvironmentalDataSource dataSource = EnvironmentalDataSource.ANY)
        {
            return ClientInstance.FetchClimate(parameter, latmin, latmax, lonmin, lonmax, starthour, stophour, startday, stopday, startyear, stopyear, new FetchingOptions(dataSource),ResearchVariationType.Auto).Values.Select(v => v.Provenance).ToArray();
        }

        /// <summary>
        /// Batch version of single fetch climate request that return uncertainties of the results. The parameteras are passed as a seperate arrays.
        /// </summary>
        /// <param name="parameter">A climate parameter that will be evaluated for mean value</param>
        /// <param name="latmin">Minimum latitude of the requested region</param>
        /// <param name="lonmin">Mimimun longatude of the requested region</param>
        /// <param name="latmax">Maximum latitude of the requesed region </param>
        /// <param name="lonmax">Maximum longatude of the requesed region</param>
        /// <param name="startyear">The first year that will be used to form time intervals of the request. The null value means 1961 (default)</param>
        /// <param name="startday">The first day of the year (1..365) that will be used to form time intervals of the request. The null value means 1</param>
        /// <param name="starthour">The time in hours of the day (0..24) that will be used as a lower inclusive bound to form time intervals of the request. The null value means 0</param>
        /// <param name="stopyear">The last year(inclusivly) that will be used to form time intervals for request. The null value means 1990</param>
        /// <param name="stopday">The last day of the year(inclusivly) that will be used to form time intervals of the request. Use GlobalConsts.DefaultValue to specify the last day of the year. The null value means "the last day of the year"</param>
        /// <param name="stophour">The time in hours of the day (0..24) that will be used as an upper inclusive bound to form time intervals of the request. Use 24 to specify the end of the day. The null value means 24</param>
        /// <param name="dataSource">The data source to use for calculation. By default the data source with lowest uncertainty is chosen automaticly</param> 
        public static double[] FetchClimateUncertainty(ClimateParameter parameter, double[] latmin, double[] latmax, double[] lonmin, double[] lonmax, int[] starthour = null, int[] stophour = null, int[] startday = null, int[] stopday = null, int[] startyear = null, int[] stopyear = null, EnvironmentalDataSource dataSource = EnvironmentalDataSource.ANY)
        {
            return ClientInstance.FetchClimate(parameter, latmin, latmax, lonmin, lonmax, starthour, stophour, startday, stopday, startyear, stopyear,new FetchingOptions(dataSource), ResearchVariationType.Auto).Values.Select<ClimateParameterValue,double>(v => v.GetUncertaintyInDefaultClientUnits()).ToArray();
        }

        /// <summary>
        /// Batch version of single fetch climate request. The parameteras are passed as a seperate arrays.
        /// </summary>
        /// <param name="parameter">A climate parameter that will be evaluated for mean value</param>
        /// <param name="latmin">Minimum latitude of the requested region</param>
        /// <param name="lonmin">Mimimun longatude of the requested region</param>
        /// <param name="latmax">Maximum latitude of the requesed region </param>
        /// <param name="lonmax">Maximum longatude of the requesed region</param>
        /// <param name="startyear">The first year that will be used to form time intervals of the request. The null value means 1961 (default)</param>
        /// <param name="startday">The first day of the year (1..365) that will be used to form time intervals of the request. The null value means 1</param>
        /// <param name="starthour">The time in hours of the day (0..24) that will be used as a lower inclusive bound to form time intervals of the request. The null value means 0</param>
        /// <param name="stopyear">The last year(inclusivly) that will be used to form time intervals for request. The null value means 1990</param>
        /// <param name="stopday">The last day of the year(inclusivly) that will be used to form time intervals of the request. Use GlobalConsts.DefaultValue to specify the last day of the year. The null value means "the last day of the year"</param>
        /// <param name="stophour">The time in hours of the day (0..24) that will be used as an upper inclusive bound to form time intervals of the request. Use 24 to specify the end of the day. The null value means 24</param>
        /// <param name="dataSource">The data source to use for calculation. By default the data source with lowest uncertainty is chosen automaticly</param> 
        public static double[] FetchClimate(ClimateParameter parameter, double[] latmin, double[] latmax, double[] lonmin, double[] lonmax, int[] starthour = null, int[] stophour = null, int[] startday = null, int[] stopday = null, int[] startyear = null, int[] stopyear = null, EnvironmentalDataSource dataSource = EnvironmentalDataSource.ANY)
        {
            return ClientInstance.FetchClimate(parameter, latmin, latmax, lonmin, lonmax, starthour, stophour, startday, stopday, startyear, stopyear, new FetchingOptions(dataSource),ResearchVariationType.Auto).Values.Select(v => v.GetValueInDefaultClientUnits()).ToArray();
        }

        /// <summary>
        /// Split requested area into spatial grid and returns provenance for calculated values of the requested parameter.
        /// </summary>
        /// <param name="parameter">A climate parameter that will be evaluated for mean value</param>
        /// <param name="latmin">Minimum latitude of the requested area</param>
        /// <param name="lonmin">Mimimun longatude of the requested area</param>
        /// <param name="latmax">Maximum latitude of the requesed area </param>
        /// <param name="lonmax">Maximum longatude of the requesed area</param>
        /// <param name="yearmin">The first year that will be used to form time intervals of the request</param>
        /// <param name="daymin">The first day of the year (1..365) that will be used to form time intervals of the request</param>
        /// <param name="hourmin">The time in hours of the day (0..24) that will be used as a lower inclusive bound to form time intervals of the request. Use -999 or 0 to specify the begining of the day</param>
        /// <param name="yearmax">The last year(inclusivly) that will be used to form time intervals for request</param>
        /// <param name="daymax">The last day of the year(inclusivly) that will be used to form time intervals of the request. Use -999 to specify the last day of the year.</param>
        /// <param name="hourmax">The time in hours of the day (0..24) that will be used as an upper inclusive bound to form time intervals of the request. Use -999 or 24 to specify the end of the day</param>
        /// <param name="dlat">A step along latitude that will be used to split requested area into grid</param>
        /// <param name="dlon">A step along longatude that will be used to split requested areia into grid</param>
        /// <param name="dataSource">The data source to use for calculation. By default the data source with lowest uncertainty is chosen automaticly</param> 
        public static string[,] FetchProvenanceGrid(ClimateParameter parameter, double latmin, double latmax, double lonmin, double lonmax, double dlat, double dlon, int hourmin = 0, int hourmax = 24, int daymin = 1, int daymax = GlobalConsts.DefaultValue, int yearmin = 1961, int yearmax = 1990, EnvironmentalDataSource dataSource = EnvironmentalDataSource.ANY)
        {
            return ClientInstance.FetchProvenanceGrid(parameter, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dlat, dlon, dataSource);
        }

        /// <summary>
        /// Split requested area into spatial grid and returns uncertainties for calculated values of the requested parameter.
        /// </summary>
        /// <param name="parameter">A climate parameter that will be evaluated for mean value</param>
        /// <param name="latmin">Minimum latitude of the requested area</param>
        /// <param name="lonmin">Mimimun longatude of the requested area</param>
        /// <param name="latmax">Maximum latitude of the requesed area </param>
        /// <param name="lonmax">Maximum longatude of the requesed area</param>
        /// <param name="yearmin">The first year that will be used to form time intervals of the request</param>
        /// <param name="daymin">The first day of the year (1..365) that will be used to form time intervals of the request</param>
        /// <param name="hourmin">The time in hours of the day (0..24) that will be used as a lower inclusive bound to form time intervals of the request. Use -999 or 0 to specify the begining of the day</param>
        /// <param name="yearmax">The last year(inclusivly) that will be used to form time intervals for request</param>
        /// <param name="daymax">The last day of the year(inclusivly) that will be used to form time intervals of the request. Use -999 to specify the last day of the year.</param>
        /// <param name="hourmax">The time in hours of the day (0..24) that will be used as an upper inclusive bound to form time intervals of the request. Use -999 or 24 to specify the end of the day</param>
        /// <param name="dlat">A step along latitude that will be used to split requested area into grid</param>
        /// <param name="dlon">A step along longatude that will be used to split requested areia into grid</param>
        /// <param name="dataSource">The data source to use for calculation. By default the data source with lowest uncertainty is chosen automaticly</param> 
        public static double[,] FetchUncertaintyGrid(ClimateParameter parameter, double latmin, double latmax, double lonmin, double lonmax, double dlat, double dlon, int hourmin = 0, int hourmax = 24, int daymin = 1, int daymax = GlobalConsts.DefaultValue, int yearmin = 1961, int yearmax = 1990, EnvironmentalDataSource dataSource = EnvironmentalDataSource.ANY)
        {
            return ClientInstance.FetchUncertaintyGrid(parameter, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dlat, dlon, dataSource);
        }

        /// <summary>
        /// Split requested area into spatial grid and returns mean values of the requested parameter for its cells.
        /// </summary>
        /// <param name="parameter">A climate parameter that will be evaluated for mean value</param>
        /// <param name="latmin">Minimum latitude of the requested area</param>
        /// <param name="lonmin">Mimimun longatude of the requested area</param>
        /// <param name="latmax">Maximum latitude of the requesed area </param>
        /// <param name="lonmax">Maximum longatude of the requesed area</param>
        /// <param name="yearmin">The first year that will be used to form time intervals of the request</param>
        /// <param name="daymin">The first day of the year (1..365) that will be used to form time intervals of the request</param>
        /// <param name="hourmin">The time in hours of the day (0..24) that will be used as a lower inclusive bound to form time intervals of the request. Use -999 or 0 to specify the begining of the day</param>
        /// <param name="yearmax">The last year(inclusivly) that will be used to form time intervals for request</param>
        /// <param name="daymax">The last day of the year(inclusivly) that will be used to form time intervals of the request. Use -999 to specify the last day of the year.</param>
        /// <param name="hourmax">The time in hours of the day (0..24) that will be used as an upper inclusive bound to form time intervals of the request. Use -999 or 24 to specify the end of the day</param>
        /// <param name="dlat">A step along latitude that will be used to split requested area into grid</param>
        /// <param name="dlon">A step along longatude that will be used to split requested areia into grid</param>
        /// <param name="dataSource">The data source to use for calculation. By default the data source with lowest uncertainty is chosen automaticly</param> 
        public static double[,] FetchClimateGrid(ClimateParameter parameter, double latmin, double latmax, double lonmin, double lonmax, double dlat, double dlon, int hourmin = 0, int hourmax = 24, int daymin = 1, int daymax = GlobalConsts.DefaultValue, int yearmin = 1961, int yearmax = 1990, EnvironmentalDataSource dataSource = EnvironmentalDataSource.ANY)
        {
            return ClientInstance.FetchClimateGrid(parameter, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dlat, dlon, dataSource);
        }

        /// <summary>
        /// Provenance of the time series request (splitting yearmin-yearmax interval by dyear value).
        /// </summary>
        /// <param name="p">A climate parameter that will be evaluated for mean value</param>
        /// <param name="latmin">Minimum latitude of the requested region</param>
        /// <param name="lonmin">Mimimun longatude of the requested region</param>
        /// <param name="latmax">Maximum latitude of the requesed region </param>
        /// <param name="lonmax">Maximum longatude of the requesed region</param>
        /// <param name="yearmin">The first year that will be used to form time intervals of the request. The null value means 1961 (default)</param>
        /// <param name="daymin">The first day of the year (1..365) that will be used to form time intervals of the request. The null value means 1</param>
        /// <param name="hourmin">The time in hours of the day (0..24) that will be used as a lower inclusive bound to form time intervals of the request. The null value means 0</param>
        /// <param name="stopyear">The last year(inclusivly) that will be used to form time intervals for request. The null value means 1990</param>
        /// <param name="daymax">The last day of the year(inclusivly) that will be used to form time intervals of the request. Use GlobalConsts.DefaultValue to specify the last day of the year. The null value means "the last day of the year"</param>
        /// <param name="yearmax">The time in hours of the day (0..24) that will be used as an upper inclusive bound to form time intervals of the request. Use 24 to specify the end of the day. The null value means 24</param>
        /// <param name="dyear">The step that will be used to spit [yearmin,yearmax] interval into a separate intervals. The default value is 1</param>
        /// <param name="dataSource">The data source to use for calculation. By default the data source with lowest uncertainty is chosen automaticly</param> 
        public static string[] FetchClimateYearlyProvenance(ClimateParameter p, double latmin, double latmax, double lonmin, double lonmax, int hourmin = 0, int hourmax = 24, int daymin = 1, int daymax = GlobalConsts.DefaultValue, int yearmin = 1961, int yearmax = 1990, int dyear = 1, EnvironmentalDataSource dataSource = EnvironmentalDataSource.ANY)
        {
            return ClientInstance.FetchClimateYearlyProvenance(p, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dyear, dataSource);
        }

        /// <summary>
        /// Uncertainty of the time series request (splitting yearmin-yearmax interval by dyear value).
        /// </summary>
        /// <param name="p">A climate parameter that will be evaluated for mean value</param>
        /// <param name="latmin">Minimum latitude of the requested region</param>
        /// <param name="lonmin">Mimimun longatude of the requested region</param>
        /// <param name="latmax">Maximum latitude of the requesed region </param>
        /// <param name="lonmax">Maximum longatude of the requesed region</param>
        /// <param name="yearmin">The first year that will be used to form time intervals of the request. The null value means 1961 (default)</param>
        /// <param name="daymin">The first day of the year (1..365) that will be used to form time intervals of the request. The null value means 1</param>
        /// <param name="hourmin">The time in hours of the day (0..24) that will be used as a lower inclusive bound to form time intervals of the request. The null value means 0</param>
        /// <param name="stopyear">The last year(inclusivly) that will be used to form time intervals for request. The null value means 1990</param>
        /// <param name="daymax">The last day of the year(inclusivly) that will be used to form time intervals of the request. Use GlobalConsts.DefaultValue to specify the last day of the year. The null value means "the last day of the year"</param>
        /// <param name="yearmax">The time in hours of the day (0..24) that will be used as an upper inclusive bound to form time intervals of the request. Use 24 to specify the end of the day. The null value means 24</param>
        /// <param name="dyear">The step that will be used to spit [yearmin,yearmax] interval into a separate intervals. The default value is 1</param>
        /// <param name="dataSource">The data source to use for calculation. By default the data source with lowest uncertainty is chosen automaticly</param> 
        public static double[] FetchClimateYearlyUncertainty(ClimateParameter p, double latmin, double latmax, double lonmin, double lonmax, int hourmin = 0, int hourmax = 24, int daymin = 1, int daymax = GlobalConsts.DefaultValue, int yearmin = 1961, int yearmax = 1990, int dyear = 1, EnvironmentalDataSource dataSource = EnvironmentalDataSource.ANY)
        {
            return ClientInstance.FetchClimateYearlyUncertainty(p, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dyear, dataSource);
        }

        /// <summary>
        /// Time series request (splitting yearmin-yearmax interval by dyear value).
        /// </summary>
        /// <param name="p">A climate parameter that will be evaluated for mean value</param>
        /// <param name="latmin">Minimum latitude of the requested region</param>
        /// <param name="lonmin">Mimimun longatude of the requested region</param>
        /// <param name="latmax">Maximum latitude of the requesed region </param>
        /// <param name="lonmax">Maximum longatude of the requesed region</param>
        /// <param name="yearmin">The first year that will be used to form time intervals of the request. The null value means 1961 (default)</param>
        /// <param name="daymin">The first day of the year (1..365) that will be used to form time intervals of the request. The null value means 1</param>
        /// <param name="hourmin">The time in hours of the day (0..24) that will be used as a lower inclusive bound to form time intervals of the request. The null value means 0</param>
        /// <param name="stopyear">The last year(inclusivly) that will be used to form time intervals for request. The null value means 1990</param>
        /// <param name="daymax">The last day of the year(inclusivly) that will be used to form time intervals of the request. Use GlobalConsts.DefaultValue to specify the last day of the year. The null value means "the last day of the year"</param>
        /// <param name="yearmax">The time in hours of the day (0..24) that will be used as an upper inclusive bound to form time intervals of the request. Use 24 to specify the end of the day. The null value means 24</param>
        /// <param name="dyear">The step that will be used to spit [yearmin,yearmax] interval into a separate intervals. The default value is 1</param>
        /// <param name="dataSource">The data source to use for calculation. By default the data source with lowest uncertainty is chosen automaticly</param> 
        public static double[] FetchClimateYearly(ClimateParameter p, double latmin, double latmax, double lonmin, double lonmax, int hourmin = 0, int hourmax = 24, int daymin = 1, int daymax = GlobalConsts.DefaultValue, int yearmin = 1961, int yearmax = 1990, int dyear = 1, EnvironmentalDataSource dataSource = EnvironmentalDataSource.ANY)
        {
            return ClientInstance.FetchClimateYearly(p, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dyear, dataSource);
        }

        /// <summary>
        /// Provenance of the time series request (splitting daymin-daymax interval by dyear value).
        /// </summary>
        /// <param name="p">A climate parameter that will be evaluated for mean value</param>
        /// <param name="latmin">Minimum latitude of the requested area</param>
        /// <param name="lonmin">Mimimun longatude of the requested area</param>
        /// <param name="latmax">Maximum latitude of the requesed area </param>
        /// <param name="lonmax">Maximum longatude of the requesed area</param>
        /// <param name="yearmin">The first year that will be used to form time intervals of the request</param>
        /// <param name="daymin">The first day of the year (1..365) that will be used to form time intervals of the request</param>
        /// <param name="hourmin">The time in hours of the day (0..24) that will be used as a lower inclusive bound to form time intervals of the request. Use -999 or 0 to specify the begining of the day</param>
        /// <param name="yearmax">The last year(inclusivly) that will be used to form time intervals for request</param>
        /// <param name="daymax">The last day of the year(inclusivly) that will be used to form time intervals of the request. Use -999 to specify the last day of the year.</param>
        /// <param name="hourmax">The time in hours of the day (0..24) that will be used as an upper inclusive bound to form time intervals of the request. Use -999 or 24 to specify the end of the day</param>
        /// <param name="dyear">The step that will be used to spit [daymin,daymax] interval into a separate intervals. The default value is 1</param>
        /// <param name="dataSource">The data source to use for calculation. By default the data source with lowest uncertainty is chosen automaticly</param> 
        public static string[] FetchClimateSeasonlyProvenance(ClimateParameter p, double latmin, double latmax, double lonmin, double lonmax, int hourmin = 0, int hourmax = 24, int daymin = 1, int daymax = GlobalConsts.DefaultValue, int yearmin = 1961, int yearmax = 1990, int dday = 1, EnvironmentalDataSource dataSource = EnvironmentalDataSource.ANY)
        {
            return ClientInstance.FetchClimateSeasonlyProvenance(p, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dday, dataSource);
        }

        /// <summary>
        /// Uncertatinty of the time series request (splitting daymin-daymax interval by dyear value).
        /// </summary>
        /// <param name="p">A climate parameter that will be evaluated for mean value</param>
        /// <param name="latmin">Minimum latitude of the requested area</param>
        /// <param name="lonmin">Mimimun longatude of the requested area</param>
        /// <param name="latmax">Maximum latitude of the requesed area </param>
        /// <param name="lonmax">Maximum longatude of the requesed area</param>
        /// <param name="yearmin">The first year that will be used to form time intervals of the request</param>
        /// <param name="daymin">The first day of the year (1..365) that will be used to form time intervals of the request</param>
        /// <param name="hourmin">The time in hours of the day (0..24) that will be used as a lower inclusive bound to form time intervals of the request. Use -999 or 0 to specify the begining of the day</param>
        /// <param name="yearmax">The last year(inclusivly) that will be used to form time intervals for request</param>
        /// <param name="daymax">The last day of the year(inclusivly) that will be used to form time intervals of the request. Use -999 to specify the last day of the year.</param>
        /// <param name="hourmax">The time in hours of the day (0..24) that will be used as an upper inclusive bound to form time intervals of the request. Use -999 or 24 to specify the end of the day</param>
        /// <param name="dyear">The step that will be used to spit [daymin,daymax] interval into a separate intervals. The default value is 1</param>
        /// <param name="dataSource">The data source to use for calculation. By default the data source with lowest uncertainty is chosen automaticly</param> 
        public static double[] FetchClimateSeasonlyUncertainty(ClimateParameter p, double latmin, double latmax, double lonmin, double lonmax, int hourmin = 0, int hourmax = 24, int daymin = 1, int daymax = GlobalConsts.DefaultValue, int yearmin = 1961, int yearmax = 1990, int dday = 1, EnvironmentalDataSource dataSource = EnvironmentalDataSource.ANY)
        {
            return ClientInstance.FetchClimateSeasonlyUncertainty(p, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dday, dataSource);
        }

        /// <summary>
        /// Time series request (splitting daymin-daymax interval by dyear value).
        /// </summary>
        /// <param name="p">A climate parameter that will be evaluated for mean value</param>
        /// <param name="latmin">Minimum latitude of the requested area</param>
        /// <param name="lonmin">Mimimun longatude of the requested area</param>
        /// <param name="latmax">Maximum latitude of the requesed area </param>
        /// <param name="lonmax">Maximum longatude of the requesed area</param>
        /// <param name="yearmin">The first year that will be used to form time intervals of the request</param>
        /// <param name="daymin">The first day of the year (1..365) that will be used to form time intervals of the request</param>
        /// <param name="hourmin">The time in hours of the day (0..24) that will be used as a lower inclusive bound to form time intervals of the request. Use -999 or 0 to specify the begining of the day</param>
        /// <param name="yearmax">The last year(inclusivly) that will be used to form time intervals for request</param>
        /// <param name="daymax">The last day of the year(inclusivly) that will be used to form time intervals of the request. Use -999 to specify the last day of the year.</param>
        /// <param name="hourmax">The time in hours of the day (0..24) that will be used as an upper inclusive bound to form time intervals of the request. Use -999 or 24 to specify the end of the day</param>
        /// <param name="dyear">The step that will be used to spit [daymin,daymax] interval into a separate intervals. The default value is 1</param>
        /// <param name="dataSource">The data source to use for calculation. By default the data source with lowest uncertainty is chosen automaticly</param> 
        public static double[] FetchClimateSeasonly(ClimateParameter p, double latmin, double latmax, double lonmin, double lonmax, int hourmin = 0, int hourmax = 24, int daymin = 1, int daymax = GlobalConsts.DefaultValue, int yearmin = 1961, int yearmax = 1990, int dday = 1, EnvironmentalDataSource dataSource = EnvironmentalDataSource.ANY)
        {
            return ClientInstance.FetchClimateSeasonly(p, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dday, dataSource);
        }

        /// <summary>
        /// Provenance of the time series request (splitting hourmin-hourmax interval by dyear value).
        /// </summary>
        /// <param name="p">A climate parameter that will be evaluated for mean value</param>
        /// <param name="latmin">Minimum latitude of the requested area</param>
        /// <param name="lonmin">Mimimun longatude of the requested area</param>
        /// <param name="latmax">Maximum latitude of the requesed area </param>
        /// <param name="lonmax">Maximum longatude of the requesed area</param>
        /// <param name="yearmin">The first year that will be used to form time intervals of the request</param>
        /// <param name="daymin">The first day of the year (1..365) that will be used to form time intervals of the request</param>
        /// <param name="hourmin">The time in hours of the day (0..24) that will be used as a lower inclusive bound to form time intervals of the request. Use -999 or 0 to specify the begining of the day</param>
        /// <param name="yearmax">The last year(inclusivly) that will be used to form time intervals for request</param>
        /// <param name="daymax">The last day of the year(inclusivly) that will be used to form time intervals of the request. Use -999 to specify the last day of the year.</param>
        /// <param name="hourmax">The time in hours of the day (0..24) that will be used as an upper inclusive bound to form time intervals of the request. Use -999 or 24 to specify the end of the day</param>
        /// <param name="dyear">The step that will be used to spit [hourmin,hourmax] interval into a separate intervals. The default value is 1</param>
        /// <param name="dataSource">The data source to use for calculation. By default the data source with lowest uncertainty is chosen automaticly</param> 
        public static string[] FetchClimateHourlyProvenance(ClimateParameter p, double latmin, double latmax, double lonmin, double lonmax, int hourmin = 0, int hourmax = 24, int daymin = 1, int daymax = GlobalConsts.DefaultValue, int yearmin = 1961, int yearmax = 1990, int dhour = 1, EnvironmentalDataSource dataSource = EnvironmentalDataSource.ANY)
        {
            return ClientInstance.FetchClimateHourlyProvenance(p, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dhour, dataSource);
        }

        /// <summary>
        /// Uncertainty of the time series request (splitting hourmin-hourmax interval by dyear value).
        /// </summary>
        /// <param name="p">A climate parameter that will be evaluated for mean value</param>
        /// <param name="latmin">Minimum latitude of the requested area</param>
        /// <param name="lonmin">Mimimun longatude of the requested area</param>
        /// <param name="latmax">Maximum latitude of the requesed area </param>
        /// <param name="lonmax">Maximum longatude of the requesed area</param>
        /// <param name="yearmin">The first year that will be used to form time intervals of the request</param>
        /// <param name="daymin">The first day of the year (1..365) that will be used to form time intervals of the request</param>
        /// <param name="hourmin">The time in hours of the day (0..24) that will be used as a lower inclusive bound to form time intervals of the request. Use -999 or 0 to specify the begining of the day</param>
        /// <param name="yearmax">The last year(inclusivly) that will be used to form time intervals for request</param>
        /// <param name="daymax">The last day of the year(inclusivly) that will be used to form time intervals of the request. Use -999 to specify the last day of the year.</param>
        /// <param name="hourmax">The time in hours of the day (0..24) that will be used as an upper inclusive bound to form time intervals of the request. Use -999 or 24 to specify the end of the day</param>
        /// <param name="dyear">The step that will be used to spit [hourmin,hourmax] interval into a separate intervals. The default value is 1</param>
        /// <param name="dataSource">The data source to use for calculation. By default the data source with lowest uncertainty is chosen automaticly</param> 
        public static double[] FetchClimateHourlyUncertainty(ClimateParameter p, double latmin, double latmax, double lonmin, double lonmax, int hourmin = 0, int hourmax = 24, int daymin = 1, int daymax = GlobalConsts.DefaultValue, int yearmin = 1961, int yearmax = 1990, int dhour = 1, EnvironmentalDataSource dataSource = EnvironmentalDataSource.ANY)
        {
            return ClientInstance.FetchClimateHourlyUncertainty(p, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dhour, dataSource);
        }

        /// <summary>
        /// Time series request (splitting hourmin-hourmax interval by dyear value).
        /// </summary>
        /// <param name="p">A climate parameter that will be evaluated for mean value</param>
        /// <param name="latmin">Minimum latitude of the requested area</param>
        /// <param name="lonmin">Mimimun longatude of the requested area</param>
        /// <param name="latmax">Maximum latitude of the requesed area </param>
        /// <param name="lonmax">Maximum longatude of the requesed area</param>
        /// <param name="yearmin">The first year that will be used to form time intervals of the request</param>
        /// <param name="daymin">The first day of the year (1..365) that will be used to form time intervals of the request</param>
        /// <param name="hourmin">The time in hours of the day (0..24) that will be used as a lower inclusive bound to form time intervals of the request. Use -999 or 0 to specify the begining of the day</param>
        /// <param name="yearmax">The last year(inclusivly) that will be used to form time intervals for request</param>
        /// <param name="daymax">The last day of the year(inclusivly) that will be used to form time intervals of the request. Use -999 to specify the last day of the year.</param>
        /// <param name="hourmax">The time in hours of the day (0..24) that will be used as an upper inclusive bound to form time intervals of the request. Use -999 or 24 to specify the end of the day</param>
        /// <param name="dyear">The step that will be used to spit [hourmin,hourmax] interval into a separate intervals. The default value is 1</param>
        /// <param name="dataSource">The data source to use for calculation. By default the data source with lowest uncertainty is chosen automaticly</param> 
        public static double[] FetchClimateHourly(ClimateParameter p, double latmin, double latmax, double lonmin, double lonmax, int hourmin = 0, int hourmax = 24, int daymin = 1, int daymax = GlobalConsts.DefaultValue, int yearmin = 1961, int yearmax = 1990, int dhour = 1, EnvironmentalDataSource dataSource = EnvironmentalDataSource.ANY)
        {
            return ClientInstance.FetchClimateHourly(p, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dhour, dataSource);
        }
        
        /// <summary>
        /// Returns mean value of specified climate parameter for the given geographical area at the given time of the day, season and year interval. Takes additional paramter to control the process of calculation. Returns object that contains additional information about calculated result.
        /// </summary>
        /// <param name="parameter">A climate variable that will be evaluated for mean value</param>
        /// <param name="latmin">Minimum latitude of the requested region</param>
        /// <param name="lonmin">Mimimun longatude of the requested region</param>
        /// <param name="latmax">Maximum latitude of the requesed region </param>
        /// <param name="lonmax">Maximum longatude of the requesed region</param>
        /// <param name="startyear">The first year that will be used to form time intervals of the request. Default value is 1961</param>
        /// <param name="startday">The first day of the year (1..365) that will be used to form time intervals of the request. Default value is 1</param>
        /// <param name="starthour">The time in hours of the day (0..24) that will be used as a lower inclusive bound to form time intervals of the request. Default value is 0</param>
        /// <param name="stopyear">The last year(inclusivly) that will be used to form time intervals for request. Default value is 1990</param>
        /// <param name="stopday">The last day of the year(inclusivly) that will be used to form time intervals of the request. Use GlobalConsts.DefaultValue to specify the last day of the year. Default value is "the last day of the year"</param>
        /// <param name="stophour">The time in hours of the day (0..24) that will be used as an upper inclusive bound to form time intervals of the request. Use 24 to specify the end of the day. Default value is 24</param>
        /// <param name="options">An information about how to perform the calculation</param>        
        internal static FetchClimateSingleResponce FetchClimateEx(ClimateParameter parameter, FetchingOptions options, double latmin, double latmax, double lonmin, double lonmax, int starthour = 0, int stophour = 24, int startday = 1, int stopday = GlobalConsts.DefaultValue, int startyear = 1961, int stopyear = 1990)
        {
            return ClientInstance.FetchClimate(parameter, latmin, latmax, lonmin, lonmax, starthour, stophour, startday, stopday, startyear, stopyear, options);
        }
               
        /// <summary>
        /// Batch version of single fetch climate request. The parameteras are passed as a seperate arrays. Takes additional paramter to control the process of calculation. Returns object that contains additional information about calculated result.
        /// </summary>
        /// <param name="parameter">A climate parameter that will be evaluated for mean value</param>
        /// <param name="latmin">Minimum latitude of the requested region</param>
        /// <param name="lonmin">Mimimun longatude of the requested region</param>
        /// <param name="latmax">Maximum latitude of the requesed region </param>
        /// <param name="lonmax">Maximum longatude of the requesed region</param>
        /// <param name="startyear">The first year that will be used to form time intervals of the request. The null value means 1961 (default)</param>
        /// <param name="startday">The first day of the year (1..365) that will be used to form time intervals of the request. The null value means 1</param>
        /// <param name="starthour">The time in hours of the day (0..24) that will be used as a lower inclusive bound to form time intervals of the request. The null value means 0</param>
        /// <param name="stopyear">The last year(inclusivly) that will be used to form time intervals for request. The null value means 1990</param>
        /// <param name="stopday">The last day of the year(inclusivly) that will be used to form time intervals of the request. Use GlobalConsts.DefaultValue to specify the last day of the year. The null value means "the last day of the year"</param>
        /// <param name="stophour">The time in hours of the day (0..24) that will be used as an upper inclusive bound to form time intervals of the request. Use 24 to specify the end of the day. The null value means 24</param>
        /// <param name="options">An information about how to perform the calculation</param>
        internal static FetchClimateBatchResponce FetchClimateEx(ClimateParameter parameter, FetchingOptions options, double[] latmin, double[] latmax, double[] lonmin, double[] lonmax, int[] starthour = null, int[] stophour = null, int[] startday = null, int[] stopday = null, int[] startyear = null, int[] stopyear = null)
        {
            int len = latmin.Length;
            return ClientInstance.FetchClimate(parameter, latmin, latmax, lonmin, lonmax,
                (starthour!=null)?(starthour):(Enumerable.Repeat(0,len).ToArray()),
                (stophour!=null)?(stophour):(Enumerable.Repeat(24,len).ToArray()),
                (startday != null) ? (startday) : (Enumerable.Repeat(1, len).ToArray()),
                (stopday != null) ? (stopday) : (Enumerable.Repeat(GlobalConsts.DefaultValue, len).ToArray()),
                (startyear != null) ? (startyear) : (Enumerable.Repeat(GlobalConsts.StartYear, len).ToArray()),
                (stopyear != null) ? (stopyear) : (Enumerable.Repeat(GlobalConsts.EndYear, len).ToArray()),
                options,ResearchVariationType.Auto);
        }

        /// <summary>
        /// Split requested area into spatial grid and returns mean values of the requested parameter for its cells. Takes additional paramter to control the process of calculation. Returns object that contains additional information about calculated result.
        /// </summary>
        /// <param name="parameter">A climate parameter that will be evaluated for mean value</param>
        /// <param name="latmin">Minimum latitude of the requested area</param>
        /// <param name="lonmin">Mimimun longatude of the requested area</param>
        /// <param name="latmax">Maximum latitude of the requesed area </param>
        /// <param name="lonmax">Maximum longatude of the requesed area</param>
        /// <param name="yearmin">The first year that will be used to form time intervals of the request</param>
        /// <param name="daymin">The first day of the year (1..365) that will be used to form time intervals of the request</param>
        /// <param name="hourmin">The time in hours of the day (0..24) that will be used as a lower inclusive bound to form time intervals of the request. Use -999 or 0 to specify the begining of the day</param>
        /// <param name="yearmax">The last year(inclusivly) that will be used to form time intervals for request</param>
        /// <param name="daymax">The last day of the year(inclusivly) that will be used to form time intervals of the request. Use -999 to specify the last day of the year.</param>
        /// <param name="hourmax">The time in hours of the day (0..24) that will be used as an upper inclusive bound to form time intervals of the request. Use -999 or 24 to specify the end of the day</param>
        /// <param name="dlat">A step along latitude that will be used to split requested area into grid</param>
        /// <param name="dlon">A step along longatude that will be used to split requested areia into grid</param>
        /// <param name="options">An information about how to perform the calculation</param>        
        internal static FetchClimateGridResponce FetchClimateGridEx(ClimateParameter parameter, FetchingOptions options, double latmin, double latmax, double lonmin, double lonmax, double dlat, double dlon, int hourmin = 0, int hourmax = 24, int daymin = 1, int daymax = GlobalConsts.DefaultValue, int yearmin = 1961, int yearmax = 1990)
        {
            return ClientInstance.FetchClimateGrid(parameter, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dlat, dlon, options);
        }
        
        /// <summary>
        /// Time series request (splitting yearmin-yearmax interval by dyear value). Takes additional paramter to control the process of calculation. Returns object that contains additional information about calculated result.
        /// </summary>
        /// <param name="p">A climate parameter that will be evaluated for mean value</param>
        /// <param name="latmin">Minimum latitude of the requested area</param>
        /// <param name="lonmin">Mimimun longatude of the requested area</param>
        /// <param name="latmax">Maximum latitude of the requesed area </param>
        /// <param name="lonmax">Maximum longatude of the requesed area</param>
        /// <param name="yearmin">The first year that will be used to form time intervals of the request</param>
        /// <param name="daymin">The first day of the year (1..365) that will be used to form time intervals of the request</param>
        /// <param name="hourmin">The time in hours of the day (0..24) that will be used as a lower inclusive bound to form time intervals of the request. Use -999 or 0 to specify the begining of the day</param>
        /// <param name="yearmax">The last year(inclusivly) that will be used to form time intervals for request</param>
        /// <param name="daymax">The last day of the year(inclusivly) that will be used to form time intervals of the request. Use -999 to specify the last day of the year.</param>
        /// <param name="hourmax">The time in hours of the day (0..24) that will be used as an upper inclusive bound to form time intervals of the request. Use -999 or 24 to specify the end of the day</param>
        /// <param name="dyear">The step that will be used to spit [yearmin,yearmax] interval into a separate intervals. The default value is 1</param>
        /// <param name="options">An information about how to perform the calculation</param>        
        internal static FetchClimateBatchResponce FetchClimateYearlyEx(ClimateParameter p, FetchingOptions options, double latmin, double latmax, double lonmin, double lonmax, int hourmin = 0, int hourmax = 24, int daymin = 1, int daymax = GlobalConsts.DefaultValue, int yearmin = 1961, int yearmax = 1990, int dyear = 1)
        {
            return ClientInstance.FetchClimateYearly(p, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dyear, options);
        }
        
        /// <summary>
        /// Time series request (splitting daymin-daymax interval by dyear value). Takes additional paramter to control the process of calculation. Returns object that contains additional information about calculated result.
        /// </summary>
        /// <param name="p">A climate parameter that will be evaluated for mean value</param>
        /// <param name="latmin">Minimum latitude of the requested area</param>
        /// <param name="lonmin">Mimimun longatude of the requested area</param>
        /// <param name="latmax">Maximum latitude of the requesed area </param>
        /// <param name="lonmax">Maximum longatude of the requesed area</param>
        /// <param name="yearmin">The first year that will be used to form time intervals of the request</param>
        /// <param name="daymin">The first day of the year (1..365) that will be used to form time intervals of the request</param>
        /// <param name="hourmin">The time in hours of the day (0..24) that will be used as a lower inclusive bound to form time intervals of the request. Use -999 or 0 to specify the begining of the day</param>
        /// <param name="yearmax">The last year(inclusivly) that will be used to form time intervals for request</param>
        /// <param name="daymax">The last day of the year(inclusivly) that will be used to form time intervals of the request. Use -999 to specify the last day of the year.</param>
        /// <param name="hourmax">The time in hours of the day (0..24) that will be used as an upper inclusive bound to form time intervals of the request. Use -999 or 24 to specify the end of the day</param>
        /// <param name="dyear">The step that will be used to spit [daymin,daymax] interval into a separate intervals. The default value is 1</param>
        /// <param name="options">An information about how to perform the calculation</param>        
        internal static FetchClimateBatchResponce FetchClimateSeasonlyEx(ClimateParameter p, FetchingOptions options, double latmin, double latmax, double lonmin, double lonmax, int hourmin = 0, int hourmax = 24, int daymin = 1, int daymax = GlobalConsts.DefaultValue, int yearmin = 1961, int yearmax = 1990, int dday = 1)
        {
            return ClientInstance.FetchClimateSeasonly(p, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dday, options);
        }
      
        /// <summary>
        /// Time series request (splitting hourmin-hourmax interval by dyear value). Takes additional paramter to control the process of calculation. Returns object that contains additional information about calculated result.
        /// </summary>
        /// <param name="p">A climate parameter that will be evaluated for mean value</param>
        /// <param name="latmin">Minimum latitude of the requested area</param>
        /// <param name="lonmin">Mimimun longatude of the requested area</param>
        /// <param name="latmax">Maximum latitude of the requesed area </param>
        /// <param name="lonmax">Maximum longatude of the requesed area</param>
        /// <param name="yearmin">The first year that will be used to form time intervals of the request</param>
        /// <param name="daymin">The first day of the year (1..365) that will be used to form time intervals of the request</param>
        /// <param name="hourmin">The time in hours of the day (0..24) that will be used as a lower inclusive bound to form time intervals of the request. Use -999 or 0 to specify the begining of the day</param>
        /// <param name="yearmax">The last year(inclusivly) that will be used to form time intervals for request</param>
        /// <param name="daymax">The last day of the year(inclusivly) that will be used to form time intervals of the request. Use -999 to specify the last day of the year.</param>
        /// <param name="hourmax">The time in hours of the day (0..24) that will be used as an upper inclusive bound to form time intervals of the request. Use -999 or 24 to specify the end of the day</param>
        /// <param name="dyear">The step that will be used to spit [hourmin,hourmax] interval into a separate intervals. The default value is 1</param>
        /// <param name="options">An information about how to perform the calculation</param>        
        internal static FetchClimateBatchResponce FetchClimateHourlyEx(ClimateParameter p, FetchingOptions options, double latmin, double latmax, double lonmin, double lonmax, int hourmin = 0, int hourmax = 24, int daymin = 1, int daymax = GlobalConsts.DefaultValue, int yearmin = 1961, int yearmax = 1990, int dhour = 1)
        {
            return ClientInstance.FetchClimateHourly(p, latmin, latmax, lonmin, lonmax, hourmin, hourmax, daymin, daymax, yearmin, yearmax, dhour, options);
        }
       
        /// <summary>
        /// Deletes all calculation resultes cached in FetchClimate client-side cache
        /// </summary>
        public static void ClearCache()
        {
            ClientInstance.ClearCache();
        }

    }
}


