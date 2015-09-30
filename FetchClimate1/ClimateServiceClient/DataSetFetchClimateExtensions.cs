using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Science.Data.Utilities;
using Microsoft.Research.Science.Data.Climate;
using Microsoft.Research.Science.Data.Climate.Common;

namespace Microsoft.Research.Science.Data.Imperative
{
    /// <summary>
    /// Extends SDS Imperative API to enable fetching climate data.
    /// </summary>
    public static class DataSetFetchClimateExtensions
    {
        #region AddAxisCells related
        /// <summary>
        /// Creates new axis variable of type double and and adds it to the dataset. The values of the variable correspond to centers of the cell. Also the method adds another variable depicting cells bounds according to CF conventions 1.5
        /// </summary>
        /// <param name="ds">Target datas set</param>
        /// <param name="name">Name of the axis</param>
        /// <param name="units">Units of measurement of values of the axis</param>
        /// <param name="min">The lower bound of the first cell</param>
        /// <param name="max">The upper bound of the last cell</param>
        /// <param name="delta">The size of cells</param>
        /// <param name="boundsVariableName">A name of varaible containing bounds of the cell. If it is omited the name will be chosen automaticly</param>
        /// <returns>New axis variable</returns>
        public static Variable AddAxisCells(this DataSet ds, string name, string units, double min, double max, double delta, string boundsVariableName = null)
        {
            var names = EnsureAddAxisCellsNames(ds, name, boundsVariableName);

            var axis = ds.AddAxis(name, units, min + delta / 2, max - delta / 2, delta);
            axis.Metadata["bounds"] = names.Item1;
            int n = axis.Dimensions[0].Length;
            double[,] boundsData = new double[n, 2];
            for (int i = 0; i < n; i++)
            {
                boundsData[i, 0] = min + delta * i;
                boundsData[i, 1] = min + delta * (i + 1);
            }
            ds.AddVariable<double>(names.Item1, boundsData, axis.Dimensions[0].Name, names.Item2);

            return axis;

        }

        /// <summary>
        /// Creates new axis variable of type float and and adds it to the dataset. The values of the variable correspond to centers of the cell. Also the method adds another variable depicting cells bounds according to CF conventions 1.5
        /// </summary>
        /// <param name="ds">Target datas set</param>
        /// <param name="name">Name of the axis</param>
        /// <param name="units">Units of measurement of values of the axis</param>
        /// <param name="min">The lower bound of the first cell</param>
        /// <param name="max">The upper bound of the last cell</param>
        /// <param name="delta">The size of cells</param>
        /// <param name="boundsVariableName">A name of varaible containing bounds of the cell. If it is omited the name will be chosen automaticly</param>
        /// <returns>New axis variable</returns>
        public static Variable AddAxisCells(this DataSet ds, string name, string units, float min, float max, float delta, string boundsVariableName = null)
        {
            var names = EnsureAddAxisCellsNames(ds, name, boundsVariableName);

            var axis = ds.AddAxis(name, units, min + delta / 2, max - delta / 2, delta);
            axis.Metadata["bounds"] = names.Item1;
            int n = axis.Dimensions[0].Length;
            float[,] boundsData = new float[n, 2];
            for (int i = 0; i < n; i++)
            {
                boundsData[i, 0] = min + delta * i;
                boundsData[i, 1] = min + delta * (i + 1);
            }
            ds.AddVariable<float>(names.Item1, boundsData, axis.Dimensions[0].Name, names.Item2);

            return axis;

        }
        /// <summary>
        /// Returns a name for the CellBoundsVariable * ServiceDimensionForCellBoundsVar
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="name"></param>
        /// <param name="boundsVariableName"></param>
        /// <returns></returns>
        private static Tuple<string, string> EnsureAddAxisCellsNames(DataSet ds, string name, string boundsVariableName)
        {
            string boundsVarName = boundsVariableName;
            if (boundsVarName == null)
            {
                boundsVarName = string.Format("{0}_bnds", name);
                if (ds.Variables.Contains(boundsVarName))
                {
                    int tryNum = 1;
                    while (ds.Variables.Contains(string.Format("{0}_bnds{1}", boundsVarName, tryNum)))
                        tryNum++;
                    boundsVarName = string.Format("{0}_bnds{1}", boundsVarName, tryNum);
                }
            }

            if (ds.Variables.Contains(boundsVarName))
                throw new ArgumentException(string.Format("The dataset is already contains a variable with a name {0}. Please specyfy another one or omit it for automatic name choice", boundsVarName));

            string sndDim = "nv";
            if (ds.Dimensions.Contains(sndDim) && ds.Dimensions[sndDim].Length != 2)
            {
                int num = 1;
                while (ds.Dimensions.Contains(string.Format("{0}{1}", sndDim, num)))
                    num++;
                sndDim = string.Format("{0}{1}", sndDim, num);
            }
            return Tuple.Create(boundsVarName, sndDim);
        }

        #endregion

        /// <summary>
        /// Creates new variable with data from the FetchClimate service for the current lat/lon coordinate system and given time.
        /// </summary>
        /// <param name="ds">Target DataSet.</param>
        /// <param name="parameter">A climate parameter to fetch.</param>
        /// <param name="name">Name of the variable.</param>
        /// <param name="time">Time moment to fetch data for.</param>
        /// <returns>New variable instance.</returns>
        /// <remarks>
        /// <para>
        /// The method looks up the <paramref name="ds"/> for the lat/lon coordinate system.
        /// An axis is considered as a latitude grid if at least one of the following conditions are satisfied 
        /// (case is ignored in all rules):
        /// <list type="bullet">
        /// <item><description>axis name starts with either "lat" or "_lat";</description></item>
        /// <item><description>axis name contains substring "latitude";</description></item>
        /// <item><description>axis has attribute Units containing substring "degree" and ends with "n" or "north".</description></item>
        /// </list>
        /// Similar rules for longitude axis:
        /// <list type="bullet">
        /// <item><description>axis name starts with either "lon" or "_lon";</description></item>
        /// <item><description>axis name contains substring "longitude";</description></item>
        /// <item><description>axis has attribute Units containing substring "degree" and ends with "e" or "east".</description></item>
        /// </list>       
        /// </para>
        /// <para>If the axes not found, an exception is thrown.</para>
        /// <para>When a coordinate system is found, the Fetch Climate service is requested using a single batch request;
        /// result is added to the DataSet as 2d-variable depending on lat/lon axes. 
        /// The DisplayName, long_name, Units, MissingValue, Provenance and Time attributes of the variable are set.
        /// </para>
        /// <example>
        /// <code>
        ///  // Fetching climate parameters for fixed time moment
        ///  using (var ds = DataSet.Open("msds:memory"))
        ///  {
        ///      Console.WriteLine("Filling dataset...");
        ///      ds.AddAxis("lon", "degrees East", -12.5, 20.0, 0.5);
        ///      ds.AddAxis("lat", "degrees North", 35.0, 60.0, 0.5);
        ///      
        ///      ds.Fetch(ClimateParameter.FC_TEMPERATURE, "airt", new DateTime(2000, 7, 19, 11, 0, 0)); // time is fixed hence airt is 2d (depends on lat and lon)
        ///      ds.Fetch(ClimateParameter.FC_SOIL_MOISTURE, "soilm", new DateTime(2000, 7, 19, 11, 0, 0));
        ///
        ///      Console.WriteLine("Running DataSet Viewer...");
        ///      ds.View(@"airt(lon,lat) Style:Colormap; Palette:-5=Blue,White=0,#F4EF2F=5,Red=20; MapType:Aerial; Transparency:0.57000000000000006;;soilm(lon,lat) Style:Colormap; Palette:0=#00000000,#827DAAEC=300,#0000A0=800; Transparency:0; MapType:Aerial");
        ///  }
        ///
        ///  // Fetching climate parameters for different time moments
        ///  using (var ds = DataSet.Open("msds:memory"))
        ///  {
        ///      Console.WriteLine("Filling dataset...");
        ///      ds.AddAxis("lon", "degrees East", -12.5, 20.0, 2.0);
        ///      ds.AddAxis("lat", "degrees North", 35.0, 60.0, 2.0);
        ///      ds.AddAxis("time", new DateTime(2000, 7, 19, 0, 0, 0), new DateTime(2000, 7, 19, 23, 0, 0), TimeSpan.FromHours(2));
        ///
        ///      ds.Fetch(ClimateParameter.FC_TEMPERATURE, "airt"); // airt depends on lat,lon,time
        ///      ds.Fetch(ClimateParameter.FC_SOIL_MOISTURE, "soilm");
        ///
        ///      Console.WriteLine("Running DataSet Viewer...");
        ///      ds.View(@"airt(lon,lat) Style:Colormap; Palette:-5=Blue,White=0,#F4EF2F=5,Red=20; MapType:Aerial; Transparency:0.57000000000000006;;soilm(lon,lat) Style:Colormap; Palette:0=#00000000,#827DAAEC=300,#0000A0=800; Transparency:0; MapType:Aerial");
        ///  }
        /// </code>
        /// </example>
        /// </remarks>
        public static Variable Fetch(this DataSet ds, ClimateParameter parameter, string name, DateTime time, string nameUncertainty = null, string nameProvenance = null, EnvironmentalDataSource dataSource = EnvironmentalDataSource.ANY)
        {
            Variable lat = null, lon = null;
            var axisVars = GetDefaultCoordinateSystem(ds);
            if (axisVars.Length > 1)
            {
                for (int i = 0; i < axisVars.Length; i++)
                {
                    var var = axisVars[i];
                    if (lat == null && GeoConventions.IsLatitude(var))
                        lat = var;
                    else if (lon == null && GeoConventions.IsLongitude(var))
                        lon = var;
                }
            }
            else
            {//looking for pointset
                var vars = ds.Variables.Where(v => v.Rank == 1).ToArray();

                for (int i = 0; i < vars.Length; i++)
                {
                    if (!GeoConventions.IsLatitude(vars[i]))
                        continue;
                    lat = vars[i];
                    foreach (Variable posLon in vars.Where((u, j) => j != i))
                        if (GeoConventions.IsLongitude(posLon))
                        {
                            lon = posLon;
                            break;
                        }
                }
            }
            return Fetch(ds, parameter, name, lat, lon, time, nameUncertainty: nameUncertainty, nameProvenance: nameProvenance, dataSource: dataSource);
        }

        /// <summary>
        /// Creates new variable with data from the FetchClimate service for the current lat/lon/time coordinate system.
        /// </summary>
        /// <param name="ds">Target DataSet.</param>
        /// <param name="parameter">A climate parameter to fetch.</param>
        /// <param name="name">Name of the variable.</param>
        /// <returns>New variable instance.</returns>
        /// <remarks>
        /// <para>
        /// The method looks up the <paramref name="ds"/> for the lat/lon/time coordinate system.
        /// An axis is considered as a latitude grid if at least one of the following conditions are satisfied 
        /// (case is ignored in all rules):
        /// <list type="bullet">
        /// <item><description>axis name starts with either "lat" or "_lat";</description></item>
        /// <item><description>axis name contains substring "latitude";</description></item>
        /// <item><description>axis has attribute Units containing substring "degree" and ends with "n" or "north".</description></item>
        /// </list></para>
        /// <para>
        /// Similar rules for the longitude axis:
        /// <list type="bullet">
        /// <item><description>axis name starts with either "lon" or "_lon";</description></item>
        /// <item><description>axis name contains substring "longitude";</description></item>
        /// <item><description>axis has attribute Units containing substring "degree" and ends with "e" or "east".</description></item>
        /// </list></para>
        /// <para>
        /// Rules for the time axis:
        /// <list type="bullet">
        /// <item><description>axis name starts with "time".</description></item>
        /// </list>       
        /// </para>
        /// <para>If the axes not found, an exception is thrown.</para>
        /// <para>When a coordinate system is found, the Fetch Climate service is requested using a single batch request;
        /// result is added to the DataSet as 3d-variable depending on time,lat,lon axes.
        /// The DisplayName, long_name, Units, MissingValue and Provenance attributes of the variable are set.
        /// </para>
        /// <example>
        /// <code>
        ///  // Fetching climate parameters for fixed time moment
        ///  using (var ds = DataSet.Open("msds:memory"))
        ///  {
        ///      Console.WriteLine("Filling dataset...");
        ///      ds.AddAxis("lon", "degrees East", -12.5, 20.0, 0.5);
        ///      ds.AddAxis("lat", "degrees North", 35.0, 60.0, 0.5);
        ///      
        ///      ds.Fetch(ClimateParameter.FC_TEMPERATURE, "airt", new DateTime(2000, 7, 19, 11, 0, 0)); // time is fixed hence airt is 2d (depends on lat and lon)
        ///      ds.Fetch(ClimateParameter.FC_SOIL_MOISTURE, "soilm", new DateTime(2000, 7, 19, 11, 0, 0));
        ///
        ///      Console.WriteLine("Running DataSet Viewer...");
        ///      ds.View(@"airt(lon,lat) Style:Colormap; Palette:-5=Blue,White=0,#F4EF2F=5,Red=20; MapType:Aerial; Transparency:0.57000000000000006;;soilm(lon,lat) Style:Colormap; Palette:0=#00000000,#827DAAEC=300,#0000A0=800; Transparency:0; MapType:Aerial");
        ///  }
        ///
        ///  // Fetching climate parameters for different time moments
        ///  using (var ds = DataSet.Open("msds:memory"))
        ///  {
        ///      Console.WriteLine("Filling dataset...");
        ///      ds.AddAxis("lon", "degrees East", -12.5, 20.0, 2.0);
        ///      ds.AddAxis("lat", "degrees North", 35.0, 60.0, 2.0);
        ///      ds.AddAxis("time", new DateTime(2000, 7, 19, 0, 0, 0), new DateTime(2000, 7, 19, 23, 0, 0), TimeSpan.FromHours(2));
        ///
        ///      ds.Fetch(ClimateParameter.FC_TEMPERATURE, "airt"); // airt depends on lat,lon,time
        ///      ds.Fetch(ClimateParameter.FC_SOIL_MOISTURE, "soilm");
        ///
        ///      Console.WriteLine("Running DataSet Viewer...");
        ///      ds.View(@"airt(lon,lat) Style:Colormap; Palette:-5=Blue,White=0,#F4EF2F=5,Red=20; MapType:Aerial; Transparency:0.57000000000000006;;soilm(lon,lat) Style:Colormap; Palette:0=#00000000,#827DAAEC=300,#0000A0=800; Transparency:0; MapType:Aerial");
        ///  }
        /// </code>
        /// </example>
        /// </remarks>
        public static Variable Fetch(this DataSet ds, ClimateParameter parameter, string name, string nameUncertainty = null, string nameProvenance = null, EnvironmentalDataSource dataSource = EnvironmentalDataSource.ANY)
        {
            Variable lat = null, lon = null, times = null;
            TimeBounds[] climBounds = GetClimatologyBounds(ds);
            var axisVars = GetDefaultCoordinateSystem(ds);
            if (axisVars.Length > 1)
            {
                for (int i = 0; i < axisVars.Length; i++)
                {
                    var var = axisVars[i];
                    if (lat == null && GeoConventions.IsLatitude(var))
                        lat = var;
                    else if (lon == null && GeoConventions.IsLongitude(var))
                        lon = var;
                    else if (times == null && IsTimes(var))
                        times = var;
                }
            }
            else
            {//looking for pointset
                var vars = ds.Variables.Where(v => v.Rank == 1).ToArray();

                for (int i = 0; i < vars.Length; i++)
                {
                    if (times == null && IsTimes(vars[i]))
                    {
                        times = vars[i];
                        continue;
                    }
                    if (!GeoConventions.IsLatitude(vars[i]))
                        continue;
                    lat = vars[i];
                    foreach (Variable posLon in vars.Take(i).Union(vars.Skip(i + 1)))
                        if (GeoConventions.IsLongitude(posLon))
                        {
                            lon = posLon;
                            break;
                        }
                }
            }
            if (times == null)
                throw new InvalidOperationException("The dataset doesn't contain a time information. Please add time axis and climatology bounds if needed");
            if (lat == null)
                throw new InvalidOperationException("The dataset doesn't contain a latitude information. Please add latitude axis to the dataset beforehand");
            if (lon == null)
                throw new InvalidOperationException("The dataset doesn't contain a longitude information. Please add longitude axis to the dataset beforehand");
            if (climBounds.Length == 0)
                return Fetch(ds, parameter, name, lat, lon, times, nameUncertainty, nameProvenance, dataSource);
            else
                return Fetch(ds, parameter, name, lat, lon, times, nameUncertainty, nameProvenance, dataSource, climatologyBounds: climBounds);
        }

        /// <summary>
        /// Creates new variable with data from the FetchClimate service for the current lat/lon coordinate system and given time.
        /// </summary>
        /// <param name="ds">Target DataSet.</param>
        /// <param name="parameter">A climate parameter to fetch.</param>
        /// <param name="name">Name of the variable.</param>
        /// <param name="lat">Name of the variable that is a latutude axis.</param>
        /// <param name="lon">Name of the variable that is a longitude axis.</param>
        /// <param name="time">Time moment to fetch data for.</param>
        /// <returns>New variable instance.</returns>
        /// <remarks>
        /// <para>
        /// The method allows to explicitly specify axes of the coordinate system for the new variable.
        /// See remarks for <see cref="Fetch(DataSet, ClimateParameter, string, DateTime)"/>
        /// </para>
        /// </remarks>
        public static Variable Fetch(this DataSet ds, ClimateParameter parameter, string name, string lat, string lon, DateTime time, string nameUncertainty = null, string nameProvenance = null, EnvironmentalDataSource dataSource = EnvironmentalDataSource.ANY)
        {
            return Fetch(ds, parameter, name, ds[lat], ds[lon], time, nameUncertainty, nameProvenance, dataSource);
        }

        /// <summary>
        /// Creates new variable with data from the FetchClimate service for the current lat/lon coordinate system and given time.
        /// </summary>
        /// <param name="ds">Target DataSet.</param>
        /// <param name="parameter">A climate parameter to fetch.</param>
        /// <param name="name">Name of the variable.</param>
        /// <param name="latID">ID of the variable that is a latutude axis.</param>
        /// <param name="lonID">ID of the variable that is a longitude axis.</param>
        /// <param name="time">Time moment to fetch data for.</param>
        /// <returns>New variable instance.</returns>
        /// <remarks>
        /// <para>
        /// The method allows to explicitly specify axes of the coordinate system for the new variable.
        /// See remarks for <see cref="Fetch(DataSet, ClimateParameter, string, DateTime)"/>
        /// </para>
        /// </remarks>
        public static Variable Fetch(this DataSet ds, ClimateParameter parameter, string name, int latID, int lonID, DateTime time, string nameUncertainty = null, string nameProvenance = null, EnvironmentalDataSource dataSource = EnvironmentalDataSource.ANY)
        {
            return Fetch(ds, parameter, name, ds.Variables.GetByID(latID), ds.Variables.GetByID(lonID), time, nameUncertainty, nameProvenance, dataSource);
        }
        /// <summary>
        /// Creates new variable with data from the FetchClimate service for the current lat/lon coordinate system and given time.
        /// </summary>
        /// <param name="ds">Target DataSet.</param>
        /// <param name="parameter">A climate parameter to fetch.</param>
        /// <param name="name">Name of the variable.</param>
        /// <param name="lat">Name of the variable that is a latutude axis.</param>
        /// <param name="lon">Name of the variable that is a longitude axis.</param>
        /// <param name="times">Name of the variable that is a time axis.</param>
        /// <returns>New variable instance.</returns>
        /// <remarks>
        /// <para>
        /// The method allows to explicitly specify axes of the coordinate system for the new variable.
        /// See remarks for <see cref="Fetch(DataSet, ClimateParameter, string)"/>
        /// </para>
        /// </remarks>
        public static Variable Fetch(this DataSet ds, ClimateParameter parameter, string name, string lat, string lon, string times, string nameUncertainty = null, string nameProvenance = null, EnvironmentalDataSource dataSource = EnvironmentalDataSource.ANY)
        {
            TimeBounds[] tb = GetClimatologyBounds(ds, ds[times].ID);
            return Fetch(ds, parameter, name, ds[lat], ds[lon], ds[times], nameUncertainty, nameProvenance, dataSource, tb.Length == 0 ? null : tb);
        }
        /// <summary>
        /// Creates new variable with data from the FetchClimate service for the current lat/lon coordinate system and given time.
        /// </summary>
        /// <param name="ds">Target DataSet.</param>
        /// <param name="parameter">A climate parameter to fetch.</param>
        /// <param name="name">Name of the variable.</param>
        /// <param name="latID">ID of the variable that is a latutude axis.</param>
        /// <param name="lonID">ID of the variable that is a longitude axis.</param>
        /// <param name="timeID">ID of the variable that is a time axis.</param>
        /// <returns>New variable instance.</returns>
        /// <remarks>
        /// <para>
        /// The method allows to explicitly specify axes of the coordinate system for the new variable.
        /// See remarks for <see cref="Fetch(DataSet, ClimateParameter, string, string, string)"/>
        /// </para>
        /// </remarks>
        public static Variable Fetch(this DataSet ds, ClimateParameter parameter, string name, int latID, int lonID, int timeID, string nameUncertainty = null, string nameProvenance = null, EnvironmentalDataSource dataSource = EnvironmentalDataSource.ANY)
        {
            TimeBounds[] tb = GetClimatologyBounds(ds, timeID);
            if (tb.Length == 0) tb = null;
            return Fetch(ds, parameter, name, ds.Variables.GetByID(latID), ds.Variables.GetByID(lonID), ds.Variables.GetByID(timeID), climatologyBounds: tb, nameUncertainty: nameUncertainty, nameProvenance: nameProvenance, dataSource: dataSource);
        }

        private static Variable Fetch(this DataSet ds, ClimateParameter parameter, string name, Variable lat, Variable lon, DateTime time, string nameUncertainty, string nameProvenance, EnvironmentalDataSource dataSource)
        {
            if (ds == null) throw new ArgumentNullException("ds");
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("name is incorrect");
            if (lat == null) throw new ArgumentNullException("lat");
            if (lon == null) throw new ArgumentNullException("lon");
            if (lat.Rank != 1) throw new ArgumentException("lat is not one-dimensional");
            if (lon.Rank != 1) throw new ArgumentException("lon is not one-dimensional");

            DataRequest[] req = new DataRequest[2];
            req[0] = DataRequest.GetData(lat);
            req[1] = DataRequest.GetData(lon);
            var resp = ds.GetMultipleData(req);

            double[] _lats = GetDoubles(resp[lat.ID].Data);
            double[] _lons = GetDoubles(resp[lon.ID].Data);
            return Fetch(ds, parameter, name, _lats, _lons, lat.Dimensions[0].Name, lon.Dimensions[0].Name, nameUncertainty: nameUncertainty, nameProvenance: nameProvenance, dataSource: dataSource, timeSlices: new DateTime[] { time });
        }

        private static Variable Fetch(this DataSet ds, ClimateParameter parameter, string name, Variable lat, Variable lon, Variable timeSlices, string nameUncertainty, string nameProvenance, EnvironmentalDataSource dataSource, TimeBounds[] climatologyBounds = null)
        {
            if (ds == null) throw new ArgumentNullException("ds");
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("name is incorrect");
            if (lat == null) throw new ArgumentNullException("lat");
            if (lon == null) throw new ArgumentNullException("lon");
            if (timeSlices == null) throw new ArgumentNullException("timeSlices");
            if (lat.Rank != 1) throw new ArgumentException("lat is not one-dimensional");
            if (lon.Rank != 1) throw new ArgumentException("lon is not one-dimensional");
            if (timeSlices.Rank != 1) throw new ArgumentException("time is not one-dimensional");

            DataRequest[] req = null;
            MultipleDataResponse resp = null;
            if (climatologyBounds == null)
            {
                req = new DataRequest[3];
                req[0] = DataRequest.GetData(lat);
                req[1] = DataRequest.GetData(lon);
                req[2] = DataRequest.GetData(timeSlices);
                resp = ds.GetMultipleData(req);
            }
            else
            {
                req = new DataRequest[2];
                req[0] = DataRequest.GetData(lat);
                req[1] = DataRequest.GetData(lon);
                resp = ds.GetMultipleData(req);
            }

            double[] _latmaxs = null;
            double[] _lonmaxs = null;
            double[] _latmins = null;
            double[] _lonmins = null;
            if (lat.Metadata.ContainsKey("bounds") && lon.Metadata.ContainsKey("bounds")) //case of cells
            {
                Variable latBounds = ds.Variables[(string)lat.Metadata["bounds"]];
                Variable lonBounds = ds.Variables[(string)lon.Metadata["bounds"]];
                if (latBounds.Rank == 2 && lonBounds.Rank == 2
                    && lonBounds.Dimensions[0].Name == lon.Dimensions[0].Name
                    && latBounds.Dimensions[0].Name == lat.Dimensions[0].Name)
                {
                    Array latBoundsData = latBounds.GetData();
                    Array lonBoundsData = lonBounds.GetData();
                    int dimLatLen = latBounds.Dimensions[0].Length;
                    int dimLonLen = lonBounds.Dimensions[0].Length;
                    _latmins = new double[dimLatLen];
                    _latmaxs = new double[dimLatLen];
                    _lonmins = new double[dimLonLen];
                    _lonmaxs = new double[dimLonLen];
                    for (int i = 0; i < dimLatLen; i++)
                    {
                        _latmins[i] = Convert.ToDouble(latBoundsData.GetValue(i, 0));
                        _latmaxs[i] = Convert.ToDouble(latBoundsData.GetValue(i, 1));
                    }
                    for (int i = 0; i < dimLonLen; i++)
                    {
                        _lonmins[i] = Convert.ToDouble(lonBoundsData.GetValue(i, 0));
                        _lonmaxs[i] = Convert.ToDouble(lonBoundsData.GetValue(i, 1));
                    }
                }
            }
            if (_latmins == null || _lonmins == null) //case of grid without cells
            {
                _latmins = GetDoubles(resp[lat.ID].Data);
                _lonmins = GetDoubles(resp[lon.ID].Data);
            }
            DateTime[] _times = null;
            if (climatologyBounds == null)
                _times = (DateTime[])resp[timeSlices.ID].Data;

            if (climatologyBounds != null)
                return Fetch(ds, parameter, name, _latmins, _lonmins, lat.Dimensions[0].Name, lon.Dimensions[0].Name, dimTime: timeSlices.Dimensions[0].Name, latmaxs: _latmaxs, lonmaxs: _lonmaxs, climatologyIntervals: climatologyBounds, nameUncertainty: nameUncertainty, nameProvenance: nameProvenance, dataSource: dataSource);
            else
                return Fetch(ds, parameter, name, _latmins, _lonmins, lat.Dimensions[0].Name, lon.Dimensions[0].Name, dimTime: timeSlices.Dimensions[0].Name, latmaxs: _latmaxs, lonmaxs: _lonmaxs, timeSlices: _times, nameUncertainty: nameUncertainty, nameProvenance: nameProvenance, dataSource: dataSource);
        }

        private static Variable Fetch(this DataSet ds, ClimateParameter parameter, string name, double[] latmins, double[] lonmins, string dimLat, string dimLon, double[] latmaxs = null, double[] lonmaxs = null, string dimTime = null, DateTime[] timeSlices = null, TimeBounds[] climatologyIntervals = null, string nameUncertainty = null, string nameProvenance = null, EnvironmentalDataSource dataSource = EnvironmentalDataSource.ANY)
        {
            if (timeSlices == null && climatologyIntervals == null)
                throw new ArgumentNullException("Both timeSlices and ClimatologyIntervals are null");
            if (latmaxs == null ^ lonmaxs == null)
                throw new ArgumentException("Only one of latmax and lonmax is set. please set both of them or none");
            object mv = double.NaN;
            string longName = GetLongName(parameter);

            bool isFetchingGrid = dimLat != dimLon; // otherwise, fetching point set, when all axes depend on the same dimension

            // Preparing FetchClimate method parameters
            int index = 0;
            int[] starthour, stophour, startday, stopday, startyear, stopyear;
            double[] minLats, minLons, maxLats, maxLons;

            int timesN = (climatologyIntervals != null) ? (climatologyIntervals.Length) : (timeSlices.Length);
            if (isFetchingGrid)
            {
                #region Preparing request for grid
                int latN = latmins.Length;
                int lonN = lonmins.Length;
                if (latN <= 0)
                    throw new ArgumentException("lats. Too short latitude axis");
                if (lonN <= 0)
                    throw new ArgumentException("lons. Too short longitude axis");
                int n = latN * lonN * timesN;
                if (n == 0) throw new ArgumentException("Empty region is requested");
                starthour = new int[n]; stophour = new int[n]; startday = new int[n]; stopday = new int[n]; startyear = new int[n]; stopyear = new int[n];
                minLats = new double[n]; minLons = new double[n];
                maxLats = new double[n]; maxLons = new double[n];
                if (climatologyIntervals != null)
                {
                    for (int j = 0; j < latN; j++)
                    {
                        double latmax = (latmaxs == null) ? latmins[j] : latmaxs[j];
                        double latmin = latmins[j];
                        for (int k = 0; k < lonN; k++)
                        {
                            double lonmax = (lonmaxs == null) ? lonmins[k] : lonmaxs[k];
                            double lonmin = lonmins[k];
                            for (int i = 0; i < timesN; i++)
                            {
                                startyear[index] = climatologyIntervals[i].MinYear;
                                startday[index] = climatologyIntervals[i].MinDay;
                                starthour[index] = climatologyIntervals[i].MinHour;

                                stopyear[index] = climatologyIntervals[i].MaxYear;
                                stopday[index] = climatologyIntervals[i].MaxDay;
                                stophour[index] = climatologyIntervals[i].MaxHour;

                                minLats[index] = latmin;
                                minLons[index] = lonmin;

                                maxLats[index] = latmax;
                                maxLons[index] = lonmax;

                                index++;
                            }
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < latN; j++)
                    {
                        double latmin = latmins[j];
                        double latmax = (latmaxs == null) ? latmins[j] : latmaxs[j];
                        for (int k = 0; k < lonN; k++)
                        {
                            double lonmax = (lonmaxs == null) ? lonmins[k] : lonmaxs[k];
                            double lonmin = lonmins[k];

                            for (int i = 0; i < timesN; i++)
                            {
                                starthour[index] = stophour[index] = timeSlices[i].Hour;
                                startday[index] = stopday[index] = timeSlices[i].DayOfYear;
                                startyear[index] = stopyear[index] = timeSlices[i].Year;

                                minLats[index] = latmin;
                                minLons[index] = lonmin;

                                maxLats[index] = latmax;
                                maxLons[index] = lonmax;

                                index++;
                            }
                        }
                    }
                }
                #endregion
            }
            else // preparing request for pointset 
            {
                #region Preparing request for pointset
                int coordN = latmins.Length;
                int n = coordN * timesN;
                if (n == 0) throw new ArgumentException("Empty region is requested");
                if (latmaxs != null || lonmaxs != null)
                    throw new InvalidOperationException("In case of pointset fetching latmaxs and lonmaxs must be omited");
                starthour = new int[n]; stophour = new int[n]; startday = new int[n]; stopday = new int[n]; startyear = new int[n]; stopyear = new int[n];
                minLats = new double[n]; minLons = new double[n];
                maxLats = new double[n]; maxLons = new double[n];
                if (climatologyIntervals != null)
                {
                    for (int i = 0; i < timesN; i++)
                        for (int j = 0; j < coordN; j++)
                        {
                            startyear[index] = climatologyIntervals[i].MinYear;
                            startday[index] = climatologyIntervals[i].MinDay;
                            starthour[index] = climatologyIntervals[i].MinHour;

                            stopyear[index] = climatologyIntervals[i].MaxYear;
                            stopday[index] = climatologyIntervals[i].MaxDay;
                            stophour[index] = climatologyIntervals[i].MaxHour;

                            minLats[index] = maxLats[index] = latmins[j];
                            minLons[index] = maxLons[index] = lonmins[j];
                            index++;
                        }
                }
                else
                {
                    for (int i = 0; i < timesN; i++)
                        for (int j = 0; j < coordN; j++)
                        {
                            starthour[index] = stophour[index] = timeSlices[i].Hour;
                            startday[index] = stopday[index] = timeSlices[i].DayOfYear;
                            startyear[index] = stopyear[index] = timeSlices[i].Year;
                            minLats[index] = maxLats[index] = latmins[j];
                            minLons[index] = maxLons[index] = lonmins[j];
                            index++;
                        }
                }
                #endregion
            }

            // Fetching the data
            var resp = ClimateService.FetchClimateEx(parameter, new FetchingOptions() { DataSourceToUse = dataSource }, minLats, maxLats, minLons, maxLons, starthour, stophour, startday, stopday, startyear, stopyear);
            ClimateParameterValue[] climateData = resp.Values;
            string units = climateData.First().DefaultClientUnitsString;

            // Saving result in the dataset
            bool _ac = ds.IsAutocommitEnabled;
            Variable varData = null, varUncertainty = null, varProv = null;
            bool saveUncertainty = !String.IsNullOrWhiteSpace(nameUncertainty);
            bool saveProv = !String.IsNullOrWhiteSpace(nameProvenance);

            try
            {
                ds.IsAutocommitEnabled = false;
                index = 0;
                if (isFetchingGrid)
                {
                    #region Putting grid data into DataSet
                    int latN = latmins.Length;
                    int lonN = lonmins.Length;
                    if (dimTime != null)
                    {
                        double[, ,] data = new double[timesN, latmins.Length, lonmins.Length];
                        double[, ,] uncertainty = saveUncertainty ? new double[timesN, latmins.Length, lonmins.Length] : null;
                        string[, ,] provenance = saveProv ? new string[timesN, latmins.Length, lonmins.Length] : null;                        
                            for (int j = 0; j < latN; j++)
                                for (int k = 0; k < lonN; k++)
                                    for (int i = 0; i < timesN; i++)                        
                                {
                                    var cd = climateData[index++];
                                    data[i, j, k] = cd.GetValueInDefaultClientUnits();
                                    if (saveUncertainty)
                                        uncertainty[i, j, k] = cd.GetUncertaintyInDefaultClientUnits();
                                    if (saveProv)
                                        provenance[i, j, k] = cd.Provenance;
                                }                        
                        varData = ds.Add(name, units, mv, data, dimTime, dimLat, dimLon);
                        if (saveUncertainty) varUncertainty = ds.Add(nameUncertainty, units, mv, uncertainty, dimTime, dimLat, dimLon);
                        if (saveProv) varProv = ds.Add(nameProvenance, provenance, dimTime, dimLat, dimLon);
                    }
                    else
                    {
                        double[,] data = new double[latmins.Length, lonmins.Length];
                        double[,] uncertainty = saveUncertainty ? new double[latmins.Length, lonmins.Length] : null;
                        string[,] provenance = saveProv ? new string[latmins.Length, lonmins.Length] : null;
                        for (int i = 0; i < latN; i++)
                            for (int j = 0; j < lonN; j++)
                            {
                                var cd = climateData[index++];
                                data[i, j] = cd.GetValueInDefaultClientUnits();
                                if (saveUncertainty)
                                    uncertainty[i, j] = cd.GetUncertaintyInDefaultClientUnits();
                                if (saveProv)
                                    provenance[i, j] = cd.Provenance;
                            }
                        varData = ds.Add(name, units, mv, data, dimLat, dimLon);
                        varData.Metadata["Time"] = timeSlices[0];
                        if (saveUncertainty)
                        {
                            varUncertainty = ds.Add(nameUncertainty, units, mv, uncertainty, dimLat, dimLon);
                            varUncertainty.Metadata["Time"] = timeSlices[0];
                        }
                        if (saveProv)
                        {
                            varProv = ds.Add(nameProvenance, provenance, dimLat, dimLon);
                            varProv.Metadata["Time"] = timeSlices[0];
                        }
                    }
                    #endregion
                }
                else // saving pointset into the dataset
                {
                    #region Putting pointset data into Dataset
                    int coordN = latmins.Length;
                    if (dimTime != null)
                    {
                        double[,] data = new double[timesN, coordN];
                        double[,] uncertainty = saveUncertainty ? new double[timesN, coordN] : null;
                        string[,] provenance = saveProv ? new string[timesN, coordN] : null;
                        for (int i = 0; i < timesN; i++)
                            for (int j = 0; j < coordN; j++)
                            {
                                var cd = climateData[index++];
                                data[i, j] = cd.GetValueInDefaultClientUnits();
                                if (saveUncertainty)
                                    uncertainty[i, j] = cd.GetUncertaintyInDefaultClientUnits();
                                if (saveProv)
                                    provenance[i, j] = cd.Provenance;
                            }
                        varData = ds.Add(name, units, mv, data, dimTime, dimLat);
                        if (saveUncertainty) varUncertainty = ds.Add(nameUncertainty, units, mv, uncertainty, dimTime, dimLat);
                        if (saveProv) varProv = ds.Add(nameProvenance, provenance, dimTime, dimLat);
                    }
                    else
                    {
                        double[] data = new double[coordN];
                        double[] uncertainty = saveUncertainty ? new double[coordN] : null;
                        string[] provenance = saveProv ? new string[coordN] : null;
                        for (int i = 0; i < coordN; i++)
                        {
                            var cd = climateData[index++];
                            data[i] = cd.GetValueInDefaultClientUnits();
                            if (saveUncertainty)
                                uncertainty[i] = cd.GetUncertaintyInDefaultClientUnits();
                            if (saveProv)
                                provenance[i] = cd.Provenance;
                        }
                        varData = ds.Add(name, units, mv, data, dimLat);
                        varData.Metadata["Time"] = timeSlices[0];
                        if (saveUncertainty)
                        {
                            varUncertainty = ds.Add(nameUncertainty, units, mv, uncertainty, dimLat);
                            varUncertainty.Metadata["Time"] = timeSlices[0];
                        }
                        if (saveProv)
                        {
                            varProv = ds.Add(nameProvenance, provenance, dimLat);
                            varProv.Metadata["Time"] = timeSlices[0];
                        }
                    }
                    #endregion
                }

                FillMetadata(varData, climatologyIntervals, resp, longName);
                if (saveUncertainty) FillUncertaintyMetadata(varUncertainty, resp, longName);
                if (saveProv) FillProvenanceMetadata(varProv, resp, longName);
                ds.Commit();
            }
            finally
            {
                ds.IsAutocommitEnabled = _ac;
            }
            return varData;
        }

        private static void FillMetadata(Variable v, TimeBounds[] climatlogyIntervals, Climate.Conventions.FetchClimateBatchResponce response, string longName)
        {
            if (climatlogyIntervals != null)
                v.Metadata["cell_methods"] = "time: mean within days  time: mean over days  time: mean over years";

            string[] dataSources = response.Values.Select(r => r.Provenance).Distinct().ToArray();
            StringBuilder bld = new StringBuilder();
            bool firstDataSource = true;
            for (int i = 0; i < dataSources.Length; i++)
            {
                if (firstDataSource)
                    firstDataSource = false;
                else
                    bld.Append(", ");
                bld.Append(dataSources[i]);
            }
            string sources = bld.ToString();

            v.Metadata["long_name"] = longName;
            v.Metadata["DisplayName"] = longName;
            v.Metadata["Provenance"] = String.Format("Data sources: {0}; served by FetchClimate Service (http://fetchclimate.cloudapp.net/)", sources);
            v.Metadata["references"] = "http://fetchclimate.cloudapp.net/";
            v.Metadata["source"] = string.Format("Interpolated from {0}", sources);
            v.Metadata["institution"] = string.Format("FetchClimate service ({0})", response.ServiceVersion);
        }

        private static void FillUncertaintyMetadata(Variable v, Climate.Conventions.FetchClimateBatchResponce response, string longName)
        {
            string[] dataSources = response.Values.Select(r => r.Provenance).Distinct().ToArray();
            StringBuilder bld = new StringBuilder();
            bool firstDataSource = true;
            for (int i = 0; i < dataSources.Length; i++)
            {
                if (firstDataSource)
                    firstDataSource = false;
                else
                    bld.Append(", ");
                bld.Append(dataSources[i]);
            }
            string sources = bld.ToString();
            longName = String.Format("Uncertainty ({0})", longName);
            v.Metadata["long_name"] = longName;
            v.Metadata["DisplayName"] = longName;
            v.Metadata["Provenance"] = String.Format("Data sources: {0}; served by FetchClimate Service (http://fetchclimate.cloudapp.net/)", sources);
            v.Metadata["references"] = "http://fetchclimate.cloudapp.net/";
            v.Metadata["source"] = string.Format("Calculated for {0}", sources);
            v.Metadata["institution"] = string.Format("FetchClimate service ({0})", response.ServiceVersion);
        }

        private static void FillProvenanceMetadata(Variable v, Climate.Conventions.FetchClimateBatchResponce response, string longName)
        {
            string[] dataSources = response.Values.Select(r => r.Provenance).Distinct().ToArray();
            StringBuilder bld = new StringBuilder();
            bool firstDataSource = true;
            for (int i = 0; i < dataSources.Length; i++)
            {
                if (firstDataSource)
                    firstDataSource = false;
                else
                    bld.Append(", ");
                bld.Append(dataSources[i]);
            }
            string sources = bld.ToString();
            longName = String.Format("Provenance ({0})", longName);
            v.Metadata["long_name"] = longName;
            v.Metadata["DisplayName"] = longName;
            v.Metadata["Provenance"] = String.Format("Data sources: {0}; served by FetchClimate Service (http://fetchclimate.cloudapp.net/)", sources);
            v.Metadata["references"] = "http://fetchclimate.cloudapp.net/";
            v.Metadata["source"] = "FetchClimate service";
            v.Metadata["institution"] = string.Format("FetchClimate service ({0})", response.ServiceVersion);
        }

        private static string GetLongName(ClimateParameter parameter)
        {
            switch (parameter)
            {
                case ClimateParameter.FC_TEMPERATURE:
                    return "Air temperature near surface";
                case ClimateParameter.FC_LAND_AIR_TEMPERATURE:
                    return "Air temperature near surface (land area)";
                case ClimateParameter.FC_OCEAN_AIR_TEMPERATURE:
                    return "Air temperature near surface (ocean area)";
                case ClimateParameter.FC_PRECIPITATION:
                    return "Precipitation rate";
                case ClimateParameter.FC_ELEVATION:
                    return "Earth surface elevation";
                case ClimateParameter.FC_LAND_ELEVATION:
                    return "Land elevation above sea level";
                case ClimateParameter.FC_LAND_FROST_DAY_FREQUENCY:
                    return "Frost day frequency";
                case ClimateParameter.FC_LAND_WET_DAY_FREQUENCY:
                    return "Wet day frequency";
                case ClimateParameter.FC_OCEAN_DEPTH:
                    return "Bathymetry";
                case ClimateParameter.FC_SOIL_MOISTURE:
                    return "Soil moisture";
                case ClimateParameter.FC_RELATIVE_HUMIDITY:
                    return "Air relative humidity";
                case ClimateParameter.FC_LAND_AIR_RELATIVE_HUMIDITY:
                    return "Air relative humidity (land area)";
                case ClimateParameter.FC_LAND_WIND_SPEED:
                    return "Near surface wind speed";
                case ClimateParameter.FC_LAND_DIURNAL_TEMPERATURE_RANGE:
                    return "Diurnal temperature range";
                case ClimateParameter.FC_LAND_SUN_PERCENTAGE:
                    return "Percent of maximum possible sunshine";
                default:
                    return parameter.ToString();
            }
        }

        private static double[] GetDoubles(Array array)
        {
            if (array.Rank != 1) throw new ArgumentException("array is not 1d");
            var type = array.GetType();
            if (type == typeof(double[])) return (double[])array;

            int n = array.Length;
            double[] res = new double[n];
            for (int i = 0; i < n; i++)
                res[i] = Convert.ToDouble(array.GetValue(i));
            return res;
        }

        /// <summary>
        /// Adds a climatology bounds information according to "Climate and Forecast Conventions". Climatology bounds will depict timeseries enumerating hourss within each day from <paramref name="hourmin"/> to <paramref name="hourmax"/> inclusivly while keeping fixed years interval and days subinterval within each year
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="yearmin">An inclusive earlier bound of years to cover with timeseries</param>
        /// <param name="yearmax">An inclusive latter bound of years to cover with timeseries</param>
        /// <param name="daymin">An inclusive earlier bound of days in year to cover with timeseries</param>
        /// <param name="daymax">An inclusive latter bound of days in year to cover with timeseries</param>
        /// <param name="hourmin">An inclusive earlier bound of hours in day to cover with timeseries</param>
        /// <param name="hourmax">An inclusive latter bound of hours in day to cover with timeseries</param>
        /// <param name="hourStep">A step of hours to enumarate through <paramref name="hourmin"/> - <paramref name="hourmax"/>interval. The last interval might be trimmed not to exceed the <paramref name="hourmax"/> limit</param>
        /// <param name="timeAxisName">A name of the variable that would be used as time variable. Passing NULL will cause an automatic detection of a sutable varaible</param>
        /// <param name="climatologyAxisName">A name of the new variable that would be used as climatology axis</param>
        public static void AddClimatologyAxisHourly(this DataSet ds, int yearmin = 1961, int yearmax = 1990, int daymin = 1, int daymax = 365, int hourmin = 0, int hourmax = 24, int hourStep = 1, string timeAxisName = null, string climatologyAxisName = "climatology_bounds")
        {
            ds.AddClimatologyAxis(timeAxisName, climatologyAxisName, yearmin, yearmax, daymin, daymax, hourmin, hourmax, IntervalToSplit.Hours, hourStep);
        }

        /// <summary>
        /// Adds a climatology bounds information according to "Climate and Forecast Conventions". Climatology bounds will depict timeseries enumerating days within each year from <paramref name="daymin"/> to <paramref name="daymax"/> inclusivly while keeping fixed years interval and hours subinterval within each day
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="yearmin">An inclusive earlier bound of years to cover with timeseries</param>
        /// <param name="yearmax">An inclusive latter bound of years to cover with timeseries</param>
        /// <param name="daymin">An inclusive earlier bound of days in year to cover with timeseries</param>
        /// <param name="daymax">An inclusive latter bound of days in year to cover with timeseries</param>
        /// <param name="hourmin">An inclusive earlier bound of hours in day to cover with timeseries</param>
        /// <param name="hourmax">An inclusive latter bound of hours in day to cover with timeseries</param>
        /// <param name="dayStep">A step of days to enumarate through <paramref name="daymin"/> - <paramref name="daymax"/>interval. The last interval might be trimmed not to exceed the <paramref name="daymax"/> limit</param>
        /// <param name="timeAxisName">A name of the variable that would be used as time variable. Passing NULL will cause an automatic detection of a sutable varaible</param>
        /// <param name="climatologyAxisName">A name of the new variable that would be used as climatology axis</param>
        public static void AddClimatologyAxisSeasonly(this DataSet ds, int yearmin = 1961, int yearmax = 1990, int daymin = 1, int daymax = 365, int hourmin = 0, int hourmax = 24, int dayStep = 1, string timeAxisName = null, string climatologyAxisName = "climatology_bounds")
        {
            ds.AddClimatologyAxis(timeAxisName, climatologyAxisName, yearmin, yearmax, daymin, daymax, hourmin, hourmax, IntervalToSplit.Days, dayStep);
        }

        /// <summary>
        /// Adds a climatology bounds information according to "Climate and Forecast Conventions". Climatology bounds will depict timeseries enumerating years from <paramref name="yearmin"/> to <paramref name="yearmax"/> inclusivly while keeping fixed days subinterval within each year and hours subinterval within each day
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="yearmin">An inclusive earlier bound of years to cover with timeseries</param>
        /// <param name="yearmax">An inclusive latter bound of years to cover with timeseries</param>
        /// <param name="daymin">An inclusive earlier bound of days in year to cover with timeseries</param>
        /// <param name="daymax">An inclusive latter bound of days in year to cover with timeseries</param>
        /// <param name="hourmin">An inclusive earlier bound of hours in day to cover with timeseries</param>
        /// <param name="hourmax">An inclusive latter bound of hours in day to cover with timeseries</param>
        /// <param name="yearStep">A step of years to enumarate through <paramref name="yearmin"/> - <paramref name="yearmax"/>interval. The last interval might be trimmed not to exceed the <paramref name="yearmax"/> limit</param>
        /// <param name="timeAxisName">A name of the variable that would be used as time variable. Passing NULL will cause an automatic detection of a sutable varaible</param>
        /// <param name="climatologyAxisName">A name of the new variable that would be used as climatology axis</param>
        public static void AddClimatologyAxisYearly(this DataSet ds, int yearmin = 1961, int yearmax = 1990, int daymin = 1, int daymax = 365, int hourmin = 0, int hourmax = 24, int yearStep = 1, string timeAxisName = null, string climatologyAxisName = "climatology_bounds")
        {
            ds.AddClimatologyAxis(timeAxisName, climatologyAxisName, yearmin, yearmax, daymin, daymax, hourmin, hourmax, IntervalToSplit.Years, yearStep);
        }

        /// <summary>
        /// Adds a climatology bounds information according to "Climate and Forecast Conventions". Climatology bounds will depict timeseries enumerating all months in year while keeping fixed years interval and hours subinterval within each day
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="yearmin">An inclusive earlier bound of years to cover with timeseries</param>
        /// <param name="yearmax">An inclusive latter bound of years to cover with timeseries</param>
        /// <param name="hourmin">An inclusive earlier bound of hours in day to cover with timeseries. valid range 1..12</param>
        /// <param name="hourmax">An inclusive latter bound of hours in day to cover with timeseries</param>        
        /// <param name="timeAxisName">A name of the variable that would be used as time variable. Passing NULL will cause an automatic detection of a sutable varaible</param>
        /// <param name="climatologyAxisName">A name of the new variable that would be used as climatology axis</param>
        public static void AddClimatologyAxisMonthly(this DataSet ds, int yearmin = 1961, int yearmax = 1990, int hourmin = 0, int hourmax = 24, string timeAxisName = null, string climatologyAxisName = "climatology_bounds")
        {
            //System.Globalization.DateTimeFormatInfo.GetInstance().
            //DateTime.DaysInMonth(            
            ds.AddClimatologyAxis(timeAxisName, climatologyAxisName, yearmin, yearmax, 1, 1, hourmin, hourmax, calculateMonthlyIntervals: true);
        }


        private static void AddClimatologyAxis(this DataSet ds, string timeAxis, string climatologyAxis, int yearmin, int yearmax, int daymin, int daymax, int hourmin, int hourmax, Microsoft.Research.Science.Data.Climate.Common.IntervalToSplit its = IntervalToSplit.Days, int step = 0, bool calculateMonthlyIntervals = false)
        {
            #region arguments check
            if (daymin < 0 || daymax > 366)
            {
                throw new ArgumentException(String.Format("The minimum day in the year doesn't fall into permited interval. You've entered {0}. However it must be set to 1..366 or to GlobalConsts.DefaultValue.", daymax));
            }
            if (daymin < 0 || daymin > 366)
            {
                throw new ArgumentException(String.Format("The maximum day in the year doesn't fall into permited interval. You've entered {0}. However it must be set to 1..366 or to GlobalConsts.DefaultValue.", daymin));
            }
            if (hourmin < 0 || hourmin > 24)
            {
                throw new ArgumentException(String.Format("starting hour in the day doesn't fall into permited interval. You've entered {0}. However it must be set to 0..24 or to GlobalConsts.DefaultValue.", hourmin));
            }
            if (hourmax < 0 || hourmax > 24)
            {
                throw new ArgumentException(String.Format("The last hour in the day doesn't fall into permited interval. You've entered {0}. However it must be set to 0..24 or to GlobalConsts.DefaultValue.", hourmax));
            }

            if (yearmin > yearmax)
            {
                throw new ArgumentException(String.Format("The minimum requested year(you've entered {0}) must be less or equel to maximum requested year(you've entered {1}).", yearmin, yearmax));
            }

            if (hourmin > hourmax)
            {
                throw new ArgumentException("Hour min is more then hour max");
            }
            if (daymin > daymax)
            {
                throw new ArgumentException("Daymin is more then day max");
            }
            if (calculateMonthlyIntervals && its != IntervalToSplit.Days && step == 0)
                throw new InvalidOperationException("monthlyDaysSteps should be specified only for ");
            int[] monthDays = null;
            if (calculateMonthlyIntervals)
                monthDays = Enumerable.Range(1, 12).Select(i => DateTime.DaysInMonth(2001, i)).ToArray();
            if (ds.Variables.Contains(climatologyAxis))
                throw new InvalidOperationException("Dataset already contains specified climatology axis");
            if (timeAxis == null)
                timeAxis = ds.Variables.Where(v => v.TypeOfData == typeof(DateTime) && v.Rank == 1).Select(v => v.Name).FirstOrDefault();
            if (timeAxis == null)
            {
                if (ds.Variables.Contains("time"))
                {
                    int num = 1;
                    while (ds.Variables.Contains(string.Format("time{0}", num)))
                        num++;
                    timeAxis = string.Format("time{0}", num);
                }
                else
                    timeAxis = "time";
            }
            #endregion

            int n = 0;
            if (calculateMonthlyIntervals)//monthly timeseries
                n = 12;
            else
                switch (its)
                {
                    case IntervalToSplit.Years: n = (int)Math.Ceiling((yearmax - yearmin) / (double)step); break;
                    case IntervalToSplit.Days: n = (int)Math.Ceiling((daymax - daymin) / (double)step); break;
                    case IntervalToSplit.Hours: n = (int)Math.Ceiling((hourmax - hourmin) / (double)step); break;
                    default: throw new NotImplementedException();
                }
            if (ds.Variables.Contains(timeAxis))
            {
                if (ds.Variables[timeAxis].Rank > 1)
                    throw new ArgumentException("Specified time axis has rank more than 1");
                int len = ds.Variables[timeAxis].Dimensions[0].Length;
                if (len != 0 && len != n)
                    throw new ArgumentException("Specified time axis has length more than zero and not equal to supposed climatology axis length");
            }

            bool autocommit = ds.IsAutocommitEnabled;
            try
            {
                ds.IsAutocommitEnabled = false;

                int[] hourmins = new int[n];
                int[] hourmaxs = new int[n];
                int[] daymins = new int[n];
                int[] daymaxs = new int[n];
                int[] yearmins = new int[n];
                int[] yearmaxs = new int[n];
                DateTime[,] timeBounds = new DateTime[n, 2];
                DateTime[] times = new DateTime[n];


                int yi = (its == IntervalToSplit.Years) ? 1 : 0;
                int di = (its == IntervalToSplit.Days) ? 1 : 0;
                int hi = (its == IntervalToSplit.Hours) ? 1 : 0;

                int day = 1;
                for (int i = 0; i < n; i++)
                {
                    hourmins[i] = Math.Min((hi == 0) ? hourmin : (hourmin + hi * i * step), 24);
                    hourmaxs[i] = Math.Min((hi == 0) ? hourmax : (hourmin + hi * ((i + 1) * step - 1)), 24);
                    yearmins[i] = (yi == 0) ? yearmin : (yearmin + yi * i * step);
                    yearmaxs[i] = (yi == 0) ? yearmax : (yearmin + yi * ((i + 1) * step - 1));
                    if (!calculateMonthlyIntervals) //seasonly timeseries
                    {
                        daymins[i] = Math.Min((di == 0) ? daymin : (daymin + di * (i * step)), (yearmins[i] == yearmaxs[i] && DateTime.IsLeapYear(yearmins[i])) ? 366 : 365);
                        daymaxs[i] = Math.Min((di == 0) ? daymax : (daymin + di * ((i + 1) * step - 1)), (yearmins[i] == yearmaxs[i] && DateTime.IsLeapYear(yearmins[i])) ? 366 : 365);
                    }
                    else //monthly timeseries
                    {
                        daymins[i] = day;
                        daymaxs[i] = day + monthDays[i] - 1;
                        day += monthDays[i];
                    }
                    timeBounds[i, 0] = new DateTime(yearmins[i], 1, 1).AddDays(daymins[i] - 1).AddHours(hourmins[i]);
                    timeBounds[i, 1] = new DateTime(yearmaxs[i], 1, 1).AddDays(daymaxs[i] - 1).AddHours(hourmaxs[i]);
                    times[i] = new DateTime((yearmaxs[i] + yearmins[i]) / 2, 1, 1).AddDays((daymaxs[i] + daymins[i]) / 2).AddHours((hourmaxs[i] + hourmins[i]) / 2);
                }
                string sndDim = "nv";
                if (ds.Dimensions.Contains(sndDim))
                {
                    int num = 1;
                    while (ds.Dimensions.Contains(string.Format("{0}{1}", sndDim, num)))
                        num++;
                    sndDim = string.Format("{0}{1}", sndDim, num);
                }
                if (!ds.Variables.Contains(timeAxis))
                {
                    ds.Add(timeAxis, times, timeAxis);
                }
                else if (ds.Variables[timeAxis].Dimensions[0].Length == 0)
                {
                    ds.Variables[timeAxis].Append(times);
                }
                ds.Add(climatologyAxis, timeBounds, ds.Variables[timeAxis].Dimensions[0].Name, sndDim);
                ds.Variables[timeAxis].Metadata["climatology"] = climatologyAxis;
                ds.Commit();
            }
            finally
            {
                ds.IsAutocommitEnabled = autocommit;
            }
        }

        private static TimeBounds[] GetClimatologyBounds(DataSet ds, int timeAxisID = -1)
        {
            List<TimeBounds> tbList = new List<TimeBounds>();
            IEnumerable<Variable> q = timeAxisID < 0 ? ds.Variables.Where(va => va.Rank == 1 && va.Metadata.ContainsKey("climatology")) :
                new Variable[] { ds.Variables.GetByID(timeAxisID) };
            foreach (Variable timeVar in q)
            {
                Dimension timeDim = timeVar.Dimensions[0];
                var climAxes = ds.Variables.Where(cv => cv.Rank == 2 && cv.TypeOfData == typeof(DateTime) && cv.Dimensions[0].Name == timeDim.Name && cv.Dimensions[1].Length == 2);
                if (climAxes.Count() > 1)
                    throw new ArgumentException(string.Format("There are {0} possible climatological axes. Ambiguous specification", climAxes.Count()));
                Variable climatologyAxis = climAxes.FirstOrDefault();
                if (climatologyAxis == null)
                    continue;
                DateTime[,] bounds = (DateTime[,])climatologyAxis.GetData();
                for (int i = 0; i < bounds.GetLength(0); i++)
                {
                    TimeBounds tb = new TimeBounds()
                    {
                        MinYear = bounds[i, 0].Year,
                        MinDay = bounds[i, 0].DayOfYear,
                        MinHour = bounds[i, 0].Hour,
                        MaxYear = bounds[i, 1].Year,
                        MaxDay = bounds[i, 1].DayOfYear,
                        MaxHour = bounds[i, 1].Hour
                    };
                    if (tb.MaxDay == 1 && tb.MaxHour == 0)
                    {
                        tb.MaxDay = 365;
                        tb.MaxHour = 24;
                        tb.MaxYear--;
                    }
                    if (tb.MaxHour == tb.MinHour)
                    {
                        tb.MinHour = 0;
                        tb.MaxHour = 24;
                    }
                    tbList.Add(tb);
                }
                break;
            }
            return tbList.ToArray();
        }

        private static bool IsTimes(Variable var)
        {
            string name = var.Name.ToLower();
            if (name.StartsWith("time")) return true;
            return false;
        }

        private static Variable[] GetDefaultCoordinateSystem(this DataSet ds)
        {
            return ds.Where(p => p.Rank == 1 && p.Name == p.Dimensions[0].Name).ToArray();
        }
    }
}
