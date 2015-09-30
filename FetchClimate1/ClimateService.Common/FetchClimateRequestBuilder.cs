using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Science.Data.Climate.Conventions;
using Microsoft.Research.Science.Data.Imperative;

namespace Microsoft.Research.Science.Data.Climate.Common
{
    public static class FetchClimateRequestBuilder
    {
        public static void CopyRequestedDataSet(DataSet inDs, DataSet outDs, bool commit)
        {
            foreach (var entry in inDs.Metadata)
                outDs.Metadata[entry.Key] = entry.Value;

            if (!outDs.Variables.Contains(Namings.VarNameLatMax))
                outDs.AddVariable<double>(Namings.VarNameLatMax, Namings.dimNameCells);
            if (!outDs.Variables.Contains(Namings.VarNameLatMin))
                outDs.AddVariable<double>(Namings.VarNameLatMin, Namings.dimNameCells);
            if (!outDs.Variables.Contains(Namings.VarNameLonMax))
                outDs.AddVariable<double>(Namings.VarNameLonMax, Namings.dimNameCells);
            if (!outDs.Variables.Contains(Namings.VarNameLonMin))
                outDs.AddVariable<double>(Namings.VarNameLonMin, Namings.dimNameCells);
            if (!outDs.Variables.Contains(Namings.VarNameDayMax))
                outDs.AddVariable<int>(Namings.VarNameDayMax, Namings.dimNameCells);
            if (!outDs.Variables.Contains(Namings.VarNameDayMin))
                outDs.AddVariable<int>(Namings.VarNameDayMin, Namings.dimNameCells);
            if (!outDs.Variables.Contains(Namings.VarNameHourMax))
                outDs.AddVariable<int>(Namings.VarNameHourMax, Namings.dimNameCells);
            if (!outDs.Variables.Contains(Namings.VarNameHourMin))
                outDs.AddVariable<int>(Namings.VarNameHourMin, Namings.dimNameCells);
            if (!outDs.Variables.Contains(Namings.VarNameYearMax))
                outDs.AddVariable<int>(Namings.VarNameYearMax, Namings.dimNameCells);
            if (!outDs.Variables.Contains(Namings.VarNameYearMin))
                outDs.AddVariable<int>(Namings.VarNameYearMin, Namings.dimNameCells);

            outDs.Variables[Namings.VarNameDayMax].Append(inDs.Variables[Namings.VarNameDayMax].GetData());
            outDs.Variables[Namings.VarNameDayMin].Append(inDs.Variables[Namings.VarNameDayMin].GetData());
            outDs.Variables[Namings.VarNameHourMax].Append(inDs.Variables[Namings.VarNameHourMax].GetData());
            outDs.Variables[Namings.VarNameHourMin].Append(inDs.Variables[Namings.VarNameHourMin].GetData());
            outDs.Variables[Namings.VarNameYearMax].Append(inDs.Variables[Namings.VarNameYearMax].GetData());
            outDs.Variables[Namings.VarNameYearMin].Append(inDs.Variables[Namings.VarNameYearMin].GetData());
            outDs.Variables[Namings.VarNameLonMax].Append(inDs.Variables[Namings.VarNameLonMax].GetData());
            outDs.Variables[Namings.VarNameLonMin].Append(inDs.Variables[Namings.VarNameLonMin].GetData());
            outDs.Variables[Namings.VarNameLatMax].Append(inDs.Variables[Namings.VarNameLatMax].GetData());
            outDs.Variables[Namings.VarNameLatMin].Append(inDs.Variables[Namings.VarNameLatMin].GetData());

            if (commit)
            {
                outDs.Commit();
            }
        }

        public static void FillDataSetWithRequest(DataSet ds, ClimateParameter parameter, double[] latmin, double[] latmax, double[] lonmin, double[] lonmax, int[] starthour, int[] stophour, int[] startday, int[] stopday, int[] startyear, int[] stopyear, FetchingOptions options)
        {
            ds.IsAutocommitEnabled = false;

            ds.Metadata[Namings.metadataNameParameter] = Namings.GetParameterName(parameter);
            ds.Metadata[Namings.metadataNameCoverage] = Namings.GetCoverageName(parameter);
            ds.Metadata[Namings.metadataNameProvenanceHint] = options.FetchClimateProvenanceControlStr;

            if (!ds.Variables.Contains(Namings.VarNameLatMax))
                ds.AddVariable<double>(Namings.VarNameLatMax, Namings.dimNameCells);
            if (!ds.Variables.Contains(Namings.VarNameLatMin))
                ds.AddVariable<double>(Namings.VarNameLatMin, Namings.dimNameCells);
            if (!ds.Variables.Contains(Namings.VarNameLonMax))
                ds.AddVariable<double>(Namings.VarNameLonMax, Namings.dimNameCells);
            if (!ds.Variables.Contains(Namings.VarNameLonMin))
                ds.AddVariable<double>(Namings.VarNameLonMin, Namings.dimNameCells);
            if (!ds.Variables.Contains(Namings.VarNameDayMax))
                ds.AddVariable<int>(Namings.VarNameDayMax, Namings.dimNameCells);
            if (!ds.Variables.Contains(Namings.VarNameDayMin))
                ds.AddVariable<int>(Namings.VarNameDayMin, Namings.dimNameCells);
            if (!ds.Variables.Contains(Namings.VarNameHourMax))
                ds.AddVariable<int>(Namings.VarNameHourMax, Namings.dimNameCells);
            if (!ds.Variables.Contains(Namings.VarNameHourMin))
                ds.AddVariable<int>(Namings.VarNameHourMin, Namings.dimNameCells);
            if (!ds.Variables.Contains(Namings.VarNameYearMax))
                ds.AddVariable<int>(Namings.VarNameYearMax, Namings.dimNameCells);
            if (!ds.Variables.Contains(Namings.VarNameYearMin))
                ds.AddVariable<int>(Namings.VarNameYearMin, Namings.dimNameCells);

            ds.Variables[Namings.VarNameDayMax].Append(stopday);
            ds.Variables[Namings.VarNameDayMin].Append(startday);
            ds.Variables[Namings.VarNameHourMax].Append(stophour);
            ds.Variables[Namings.VarNameHourMin].Append(starthour);
            ds.Variables[Namings.VarNameYearMax].Append(stopyear);
            ds.Variables[Namings.VarNameYearMin].Append(startyear);
            ds.Variables[Namings.VarNameLonMax].Append(lonmax);
            ds.Variables[Namings.VarNameLonMin].Append(lonmin);
            ds.Variables[Namings.VarNameLatMax].Append(latmax);
            ds.Variables[Namings.VarNameLatMin].Append(latmin);

            ds.Commit();
        }

        public static void FillDataSetWithRequest(DataSet ds, ClimateParameter parameter, double[] latmin, double[] latmax, double[] lonmin, double[] lonmax, int[] starthour, int[] stophour, int[] startday, int[] stopday, int[] startyear, int[] stopyear)
        {
            FillDataSetWithRequest(ds, parameter, latmin, latmax, lonmin, lonmax, starthour, stophour, startday, stopday, startyear, stopyear, new FetchingOptions());
        }

        public static void FillDataSetWithRequest(DataSet ds, ClimateParameter parameter, double latmin, double latmax, double lonmin, double lonmax, int starthour, int stophour, int startday, int stopday, int startyear, int stopyear, FetchingOptions options)
        {
            ds.IsAutocommitEnabled = false;

            ds.Metadata[Namings.metadataNameParameter] = Namings.GetParameterName(parameter);
            ds.Metadata[Namings.metadataNameCoverage] = Namings.GetCoverageName(parameter);
            ds.Metadata[Namings.metadataNameProvenanceHint] = options.FetchClimateProvenanceControlStr;

            if (!ds.Variables.Contains(Namings.VarNameLatMax))
                ds.AddVariable<double>(Namings.VarNameLatMax, Namings.dimNameCells);
            if (!ds.Variables.Contains(Namings.VarNameLatMin))
                ds.AddVariable<double>(Namings.VarNameLatMin, Namings.dimNameCells);
            if (!ds.Variables.Contains(Namings.VarNameLonMax))
                ds.AddVariable<double>(Namings.VarNameLonMax, Namings.dimNameCells);
            if (!ds.Variables.Contains(Namings.VarNameLonMin))
                ds.AddVariable<double>(Namings.VarNameLonMin, Namings.dimNameCells);
            if (!ds.Variables.Contains(Namings.VarNameDayMax))
                ds.AddVariable<int>(Namings.VarNameDayMax, Namings.dimNameCells);
            if (!ds.Variables.Contains(Namings.VarNameDayMin))
                ds.AddVariable<int>(Namings.VarNameDayMin, Namings.dimNameCells);
            if (!ds.Variables.Contains(Namings.VarNameHourMax))
                ds.AddVariable<int>(Namings.VarNameHourMax, Namings.dimNameCells);
            if (!ds.Variables.Contains(Namings.VarNameHourMin))
                ds.AddVariable<int>(Namings.VarNameHourMin, Namings.dimNameCells);
            if (!ds.Variables.Contains(Namings.VarNameYearMax))
                ds.AddVariable<int>(Namings.VarNameYearMax, Namings.dimNameCells);
            if (!ds.Variables.Contains(Namings.VarNameYearMin))
                ds.AddVariable<int>(Namings.VarNameYearMin, Namings.dimNameCells);

            ds.Variables[Namings.VarNameDayMax].Append(new int[] { stopday });
            ds.Variables[Namings.VarNameDayMin].Append(new int[] { startday });
            ds.Variables[Namings.VarNameHourMax].Append(new int[] { stophour });
            ds.Variables[Namings.VarNameHourMin].Append(new int[] { starthour });
            ds.Variables[Namings.VarNameYearMax].Append(new int[] { stopyear });
            ds.Variables[Namings.VarNameYearMin].Append(new int[] { startyear });
            ds.Variables[Namings.VarNameLonMax].Append(new double[] { lonmax });
            ds.Variables[Namings.VarNameLonMin].Append(new double[] { lonmin });
            ds.Variables[Namings.VarNameLatMax].Append(new double[] { latmax });
            ds.Variables[Namings.VarNameLatMin].Append(new double[] { latmin });

            ds.Commit();
        }

        public static void FillDataSetWithRequest(DataSet ds, ClimateParameter parameter, double latmin, double latmax, double lonmin, double lonmax, int starthour, int stophour, int startday, int stopday, int startyear, int stopyear)
        {
            FillDataSetWithRequest(ds, parameter, latmin, latmax, lonmin, lonmax, starthour, stophour, startday, stopday, startyear, stopyear, new FetchingOptions());
        }

        public static void FillDataSetWithStatusCheckParams(DataSet ds, int expectedCalculationTime, string hash,bool resendRequest, bool commit)
        {
            ds.Metadata[Namings.restApiMetadataNameExpectedCalculationTime] = expectedCalculationTime;
            ds.Metadata[Namings.restApiMetadataNameHash] = hash;
            ds.Metadata[Namings.restApiMetadataNameResendRequest] = resendRequest;

            if (commit)
                ds.Commit();
        }

        public static void GetStatusCheckParams(DataSet ds, out int expectedCalculationTime, out string hash)
        {
            expectedCalculationTime = (int)ds.Metadata[Namings.restApiMetadataNameExpectedCalculationTime];
            hash = (string)ds.Metadata[Namings.restApiMetadataNameHash];
        }

        public static bool IsStatusCheckDataSet(DataSet ds)
        {
            return ds.Metadata.ContainsKey(Namings.restApiMetadataNameExpectedCalculationTime);
        }

        public static bool IsResultDataSet(DataSet ds)
        {
            return !ds.Metadata.ContainsKey(Namings.restApiMetadataNameExpectedCalculationTime);
        }

        public static bool ResendRequest(DataSet statusCheckDataSet)
        {
            return statusCheckDataSet.Metadata.ContainsKey(Namings.restApiMetadataNameResendRequest) && (bool)statusCheckDataSet.Metadata[Namings.restApiMetadataNameResendRequest];
        }

        public static bool IsProcessingSuccessful(DataSet ds)
        {
            if (!ds.Variables.Contains(Namings.VarNameResult))
                return false;
            else
                return true;
        }

        public static bool IsProcessingFailed(DataSet ds)
        {
            if (!ds.Variables.Contains(Namings.VarNameLogMessage))
                return false;
            string[] entries = (string[])ds[Namings.VarNameLogMessageType].GetData();
            return entries.Any(s => s.Contains("FAULT"));
        }

        public static FetchClimateBatchResponce BuildBatchResult(DataSet ds)
        {
            if (IsProcessingSuccessful(ds)) // success
            {
                FetchClimateBatchResponce res = new FetchClimateBatchResponce();
                if (ds.Metadata.ContainsKey(Namings.metadataNameServiceVersion))
                    res.ServiceVersion = ds.Metadata[Namings.metadataNameServiceVersion].ToString();
                var activeParameter=Namings.GetParameterByName((string)ds.Metadata[Namings.metadataNameParameter]);
                //var activeCoverage = Namings.GetCoverageByName((string)ds.Metadata[Namings.metadataNameCoverage]);
                res.Values=null;
                double[] result = (double[])ds.Variables[Namings.VarNameResult].GetData();
                double[] uncertatinty = null;
                string[] provenance = null;
                if (ds.Variables.Contains(Namings.VarNameUncertainty))
                    uncertatinty = (double[])ds.Variables[Namings.VarNameUncertainty].GetData();
                else
                    uncertatinty = new double[result.Length];
                provenance = new string[result.Length];

                ushort[] ids = null;
                short[] provArr = null;
                string[] descs = null;

                if (ds.Variables.Contains(Namings.VarNameResultProvenance) && ds.Variables.Contains(Namings.VarNameConfigProcDesc) && ds.Variables.Contains(Namings.VarNameConfigProcId))
                {
                    ids = (ushort[])ds.Variables[Namings.VarNameConfigProcId].GetData();
                    descs = (string[])ds.Variables[Namings.VarNameConfigProcDesc].GetData();
                    provArr = (short[])ds.Variables[Namings.VarNameResultProvenance].GetData();
                    for (int i = 0; i < provArr.Length; i++)
                        provenance[i] = descs[ids[provArr[i]]];
                }
                else
                {
                    provArr=Enumerable.Repeat<short>(0, provenance.Length).ToArray();
                }
                Func<double,double,ClimateParameterValue> ClimateValueConstructor = null;                
                switch(activeParameter)
                {
                    case Service.ClimateParameter.FC_TEMPERATURE:
                        ClimateValueConstructor = (r, u) => new TemperatureValue(r, u);
                        break;
                    case Service.ClimateParameter.FC_PRECIPITATION:
                        ClimateValueConstructor =  (r, u) => new PrecipitationValue(r, u);
                        break;
                    case Service.ClimateParameter.FC_SOIL_MOISTURE:
                        ClimateValueConstructor = (r, u) => new SoilMoistureValue(r, u);
                        break;
                    case Service.ClimateParameter.FC_RELATIVE_HUMIDITY:
                        ClimateValueConstructor =  (r, u) => new RelativeHumidityValue(r, u);
                        break;
                    case Service.ClimateParameter.FC_ELEVATION:
                       ClimateValueConstructor =  (r, u) => new ElevationValue(r, u);
                        break;
                    case Service.ClimateParameter.FC_DIURNAL_TEMPERATURE_RANGE:
                        ClimateValueConstructor =  (r, u) => new DiurnalTemperatureRangeValue(r, u);
                        break;
                    case Service.ClimateParameter.FC_FROST_DAY_FREQUENCY:
                        ClimateValueConstructor =  (r, u) => new FrostDayFrequencyValue(r, u);
                        break;
                    case Service.ClimateParameter.FC_WET_DAY_FREQUENCY:
                        ClimateValueConstructor = (r, u) => new WetDayFrequencyValue(r, u);
                        break;
                    case Service.ClimateParameter.FC_WIND_SPEED:
                        ClimateValueConstructor = (r, u) => new WindSpeedValue(r, u);
                        break;
                    case Service.ClimateParameter.FC_SUN_PERCENTAGE:
                        ClimateValueConstructor = (r,u) => new SunPercentageValue(r,u);
                        break;                    
                    default:
                        throw new NotSupportedException();
                }
                res.Values = result
                    .Zip(uncertatinty, ClimateValueConstructor)
                    .Zip(provArr, (val, id) => { val.ProvenanceId = id; return val; })
                    .Select(val => { val.ProcDesc = descs; val.ProcIds = ids; return val; })
                    .Zip(provenance, (val, prov) => { val.Provenance = prov; return val; })
                    .ToArray();

                res.ProvenanceUsed = ds.Metadata.ContainsKey(Namings.metadataNameProvenanceUsed) && (ds.Metadata[Namings.metadataNameProvenanceUsed]!=null) ?
                    ds.Metadata[Namings.metadataNameProvenanceUsed].ToString()
                    : "Unknown";
                return res;
            }
            else // failure
            {
                StringBuilder sb = new StringBuilder("Computation failed");
                string[] mess = ds.GetData<string[]>(Namings.VarNameLogMessage);
                string[] types = ds.GetData<string[]>(Namings.VarNameLogMessageType);
                DateTime[] times = ds.GetData<DateTime[]>(Namings.VarNameLogMessageTime);
                for (int i = 0; i < mess.Length; i++)
                {
                    sb.AppendLine().Append(String.Format("{0}({1}):{2}", times[i], types[i], mess[i]));
                }
                throw new Exception(sb.ToString());
            }
        }

        public static FetchClimateSingleResponce BuildSingleCellResult(DataSet ds)
        {
            var resB = BuildBatchResult(ds);
            var res = new FetchClimateSingleResponce();
            res.ServiceVersion = resB.ServiceVersion;
            res.Value = resB.Values[0];
            res.ProvenanceUsed = resB.ProvenanceUsed;
            return res;
        }
    }

    public enum IntervalToSplit { Hours, Days, Years };
}
