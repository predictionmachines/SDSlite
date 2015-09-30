using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Research.Science.Data.Climate.Common;
using System.Text;

namespace Microsoft.Research.Science.Data
{
    /// <summary>
    /// An enumeration of the Climate parameter that incapsulates within itself a climate parameter for fetching and corresponding spatial coverage type
    /// </summary>
    /// <remarks>It is supposed to be used at the client side</remarks>
    public enum ClimateParameter
    {
        FC_TEMPERATURE,
        FC_PRECIPITATION,
        FC_LAND_AIR_TEMPERATURE,
        FC_OCEAN_AIR_TEMPERATURE,
        FC_ELEVATION,
        FC_LAND_ELEVATION,
        FC_OCEAN_DEPTH,
        FC_SOIL_MOISTURE,
        FC_RELATIVE_HUMIDITY,
        FC_LAND_AIR_RELATIVE_HUMIDITY,
        FC_LAND_WIND_SPEED,
        FC_LAND_DIURNAL_TEMPERATURE_RANGE,
        FC_LAND_FROST_DAY_FREQUENCY,
        FC_LAND_WET_DAY_FREQUENCY,
        FC_LAND_SUN_PERCENTAGE
    }

    namespace Climate.Service
    {
        /// <summary>
        /// An enumeration that is used at ser server side to distinguish differnet "pure" climate parameters (without assossiated spatial coverage type)
        /// </summary>
        public enum ClimateParameter
        {
            FC_TEMPERATURE,
            FC_PRECIPITATION,
            FC_SOIL_MOISTURE,
            FC_RELATIVE_HUMIDITY,
            FC_ELEVATION,
            FC_WIND_SPEED,
            FC_DIURNAL_TEMPERATURE_RANGE,
            FC_FROST_DAY_FREQUENCY,
            FC_WET_DAY_FREQUENCY,
            FC_SUN_PERCENTAGE
        }
    }

    /// <summary>
    /// Describe the data source for FetchClimate
    /// </summary>
    public enum EnvironmentalDataSource
    {
        /// <summary>
        /// The data source with lowest uncertainty
        /// </summary>
        ANY,
        /// <summary>
        /// High-resolution grid of the average climate in the recent past. CRU CL 2.0
        /// </summary>
        CRU_CL_2_0,
        /// <summary>
        /// NCEP/NCAR Reanalysis 1 model output
        /// </summary>
        NCEP_REANALYSIS_1,
        /// <summary>
        /// The Global Historical Climatology Network. The dataset version 2.
        /// </summary> 
        GHCNv2,
        /// <summary>
        /// The monthly data set consists of a file containing monthly averaged soil moisture water height equivalents.
        /// </summary>
        CPC_SOIL_MOSITURE,
        /// <summary>
        /// ETOPO1 is a 1 arc-minute global relief model of Earth's surface that integrates land topography and ocean bathymetry.
        /// </summary>
        ETOPO1_ICE_SHEETS,
        /// <summary>
        /// A global digital elevation model GTOPO30
        /// </summary>
        GTOPO30,
        /// <summary>
        /// WorldClim 1.4 - Global Climate Data. 30 sec grid
        /// </summary>
        WORLD_CLIM_1_4
    }

    public class FetchingOptions
    {
        public FetchingOptions(EnvironmentalDataSource dataSourceToUse = EnvironmentalDataSource.ANY)
        {
            this.dataSourceToUse = dataSourceToUse;
        }        

        /// <summary>
        /// Used in inner provenance control of the FetchClimate
        /// </summary>
        public String FetchClimateProvenanceControlStr
        {
            get
            {
                switch (dataSourceToUse)
                {
                    case EnvironmentalDataSource.ANY: return "any";
                    case EnvironmentalDataSource.CPC_SOIL_MOSITURE: return "cpc";
                    case EnvironmentalDataSource.CRU_CL_2_0: return "CRU";
                    case EnvironmentalDataSource.ETOPO1_ICE_SHEETS: return "etopo1_is";
                    case EnvironmentalDataSource.GHCNv2: return "GHCNv2";
                    case EnvironmentalDataSource.GTOPO30: return "gtopo30";
                    case EnvironmentalDataSource.NCEP_REANALYSIS_1: return "NCEP/R,NCEP/G";
                    case EnvironmentalDataSource.WORLD_CLIM_1_4: return "WorldClim";
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private EnvironmentalDataSource dataSourceToUse = EnvironmentalDataSource.ANY;
        public EnvironmentalDataSource DataSourceToUse
        {
            get
            {

                return dataSourceToUse;
            }
            set
            {
                dataSourceToUse = value;
            }
        }
    }

    /// <summary>
    /// Represents a climate parameter value and its uncertatinty
    /// </summary>
    public abstract class ClimateParameterValue
    {
        /// <summary>
        /// Check if the passed value is missing value
        /// </summary>
        /// <param name="value">value to check</param>
        /// <returns></returns>
        public static bool CheckIfMissingValue(double value)
        {
            return double.IsNaN(value);
        }


        /*
        ///<summary>
        /// Check if the passed value is missing value
        ///</summary>
        ///<param name="value">value to check</param>
        ///<returns></returns>
        public static bool IsMissingValue(ClimateParameterValue value)
        {
            return value.IsMissingValue;
        }*/

        /// <summary>
        /// Indicates whether the value is missing value
        /// </summary>
        public bool IsMissingValue
        {
            get
            {
                return unifiedValue == missingValue;
            }
        }
        protected Climate.Service.ClimateParameter parameter;
        protected double unifiedUncertainty;


        /// <summary>
        /// Internal missing value representation that is used only within ClimateParamterValue objects
        /// </summary>
        protected double missingValue;

        /// <summary>
        /// The value that is used for internal value storing and for dataset serialization
        /// </summary>
        protected double unifiedValue;

        /// <summary>
        /// An internal unified value that is considered to be Missing Value
        /// </summary>
        public double InternalMissingValue
        {
            get
            {
                return missingValue;
            }
        }

        /// <summary>
        /// The data source that has been used for calculating the result
        /// </summary>
        public string Provenance
        {
            get;
            set;
        }

        /// <summary>
        /// The data source id that has been used for calculating the result
        /// </summary>
        public short ProvenanceId
        {
            get;
            set;
        }

        public string[] ProcDesc
        {
            get;
            set;
        }

        public ushort[] ProcIds
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the value as a double in default user units according to documentation
        /// </summary>
        /// <returns></returns>
        public abstract double GetValueInDefaultClientUnits();

        public abstract double GetUncertaintyInDefaultClientUnits();

        /// <summary>
        /// The units in which the user gets the result calling double returning functions
        /// </summary>
        public abstract string DefaultClientUnitsString { get; }

        public abstract Enum DefaultClientUnits { get; }
        public abstract Enum InternalUnits { get; }

        public override string ToString()
        {
            return (IsMissingValue) ? "[m/v]" :
                string.Format("{0:F3} {1}", GetValueInDefaultClientUnits(), DefaultClientUnitsString);
        }

        public ClimateParameterValue(Climate.Service.ClimateParameter p)
        {
            parameter = p;
        }

        /// <summary>
        /// A value that is expressed in a internal number representation
        /// </summary>
        public double UnifiedValue
        {
            get
            {
                if (IsMissingValue)
                    return missingValue;
                return unifiedValue;
            }
        }

        /// <summary>
        /// a value of uncertainty that is used as an 
        /// </summary>
        public double UnifiedUncertainty
        {
            get
            {
                return unifiedUncertainty;
            }
        }

        /// <summary>
        /// A represented climate prameter
        /// </summary>
        public Climate.Service.ClimateParameter Parameter
        {
            get
            {
                return parameter;
            }
        }
    }

    public abstract class TypedClimateParameterValue<TypeOfUnits> : ClimateParameterValue
    {
        protected readonly TypeOfUnits clientUnits;
        protected readonly TypeOfUnits internalUnits;

        protected TypedClimateParameterValue(Climate.Service.ClimateParameter paramter, TypeOfUnits defaultClientUnits, TypeOfUnits internalUnits, double missingValue)
            : base(paramter)
        {
            this.missingValue = missingValue;
            clientUnits = defaultClientUnits;
            this.internalUnits = internalUnits;
        }

        public sealed override Enum InternalUnits
        {
            get { return internalUnits as Enum; }
        }

        public sealed override Enum DefaultClientUnits
        {
            get { return clientUnits as Enum; }
        }

        public sealed override double GetValueInDefaultClientUnits()
        {
            if (IsMissingValue)
                return double.NaN;
            return GetValueAs(clientUnits);

        }

        public sealed override double GetUncertaintyInDefaultClientUnits()
        {
            if (IsMissingValue)
                return double.NaN;
            return GetUncertatintyAs(clientUnits);
        }

        public abstract double GetValueAs(TypeOfUnits units);

        public abstract double GetUncertatintyAs(TypeOfUnits units);
    }

    public enum ConcentrationUnits { Part, Percentage }
    public enum MoistureUnits { VolumetricPart, VolumetricPercentage, Mm }
    public enum TemperatureUnits { DegreesKelvin, DegreesCelsius };
    public enum PrecipitationRateUnits { MmPerMonth, KilogramPerSquereMeterPerSecond, GramPerSquereMeterPerHour }
    public enum ElevationUnits { MetersAboveOceanLevel, MetersBelowOceanLevel }
    public enum WindSpeedUnits { MetresPerSecond, Knots }
    public enum DaysFreqUnits { DaysCount, MonthsPart }


    public class ElevationValue : TypedClimateParameterValue<ElevationUnits>
    {
        public override string DefaultClientUnitsString
        {
            get { return "meters"; }
        }

        public ElevationValue(double value, double uncertainty, ElevationUnits units = ElevationUnits.MetersAboveOceanLevel)
            : base(Climate.Service.ClimateParameter.FC_ELEVATION, ElevationUnits.MetersAboveOceanLevel, ElevationUnits.MetersAboveOceanLevel, -99999)
        {
            if (double.MinValue == value || value == missingValue)
            {
                unifiedValue = missingValue;
            }
            else
            {
                switch (units)
                {
                    case ElevationUnits.MetersAboveOceanLevel:
                        unifiedUncertainty = uncertainty;
                        unifiedValue = value;
                        break;
                    case ElevationUnits.MetersBelowOceanLevel:
                        unifiedValue = -value;
                        unifiedUncertainty = uncertainty;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public override double GetValueAs(ElevationUnits units)
        {
            if (IsMissingValue)
                return double.NaN;
            switch (units)
            {
                case ElevationUnits.MetersAboveOceanLevel:
                    return unifiedValue;
                case ElevationUnits.MetersBelowOceanLevel:
                    return -unifiedValue;
                default: throw new NotFiniteNumberException();
            }
        }

        public override double GetUncertatintyAs(ElevationUnits units)
        {
            if (IsMissingValue)
                return double.NaN;
            switch (units)
            {
                case ElevationUnits.MetersAboveOceanLevel:
                case ElevationUnits.MetersBelowOceanLevel:
                    return unifiedUncertainty;
                default: throw new NotImplementedException();
            }
        }
    }

    /// <summary>
    /// Contatins an information about the soil moisture value and its uncertatinty
    /// </summary>
    public class SoilMoistureValue : TypedClimateParameterValue<MoistureUnits>
    {
        public override string DefaultClientUnitsString
        {
            get { return "mm/m"; }
        }

        /// <summary>
        /// Pass double.minvalue as a value to set up missing value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="uncertainty"></param>
        /// <param name="units">units in which the value and uncertainty is given, if it's omited default internal representation will be used</param>
        public SoilMoistureValue(double value, double uncertainty, MoistureUnits units = MoistureUnits.VolumetricPart)
            : base(Climate.Service.ClimateParameter.FC_SOIL_MOISTURE, MoistureUnits.Mm, MoistureUnits.VolumetricPart, -1.0)
        {
            if (double.MinValue == value || value == missingValue)
            {
                unifiedValue = missingValue;
            }
            else
            {
                switch (units)
                {
                    case MoistureUnits.Mm:
                        unifiedValue = value / 1000.0;
                        unifiedUncertainty = uncertainty / 1000.0;
                        break;
                    case MoistureUnits.VolumetricPart:
                        unifiedValue = value;
                        unifiedUncertainty = uncertainty;
                        break;
                    case MoistureUnits.VolumetricPercentage:
                        unifiedValue = value / 100.0;
                        unifiedUncertainty = uncertainty / 100.0;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Get the moisture value in a specified units
        /// </summary>
        /// <param name="units">Units of the result</param>
        /// <returns></returns>
        public override double GetValueAs(MoistureUnits units)
        {
            if (IsMissingValue)
                return double.NaN;
            switch (units)
            {
                case MoistureUnits.Mm:
                    return unifiedValue * 1000.0;
                case MoistureUnits.VolumetricPart:
                    return unifiedValue;
                case MoistureUnits.VolumetricPercentage:
                    return unifiedValue * 100.0;
                default:
                    throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Get moisture uncertainty in a specified units
        /// </summary>
        /// <param name="units">Units of the result</param>
        /// <returns></returns>
        public override double GetUncertatintyAs(MoistureUnits units)
        {
            if (IsMissingValue)
                return double.NaN;
            switch (units)
            {
                case MoistureUnits.Mm:
                    return unifiedUncertainty * 1000.0;
                case MoistureUnits.VolumetricPart:
                    return unifiedUncertainty;
                case MoistureUnits.VolumetricPercentage:
                    return unifiedUncertainty * 100.0;
                default:
                    throw new NotImplementedException();
            }
        }

    }



    /// <summary>
    /// Contatins an information about the soil moisture value and its uncertatinty
    /// </summary>
    public class WindSpeedValue : TypedClimateParameterValue<WindSpeedUnits>
    {
        public override string DefaultClientUnitsString
        {
            get { return "m/s"; }
        }

        /// <summary>
        /// Pass double.minvalue as a value to set up missing value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="uncertainty"></param>
        /// <param name="units">units in which the value and uncertainty is given, if it's omited default internal representation will be used</param>
        public WindSpeedValue(double value, double uncertainty, WindSpeedUnits units = WindSpeedUnits.MetresPerSecond)
            : base(Climate.Service.ClimateParameter.FC_WIND_SPEED, WindSpeedUnits.MetresPerSecond, WindSpeedUnits.MetresPerSecond, -1.0)
        {
            if (double.MinValue == value || value == missingValue)
            {
                unifiedValue = missingValue;
            }
            else
            {
                switch (units)
                {
                    case WindSpeedUnits.MetresPerSecond:
                        unifiedValue = value;
                        unifiedUncertainty = uncertainty;
                        break;
                    case WindSpeedUnits.Knots:
                        unifiedValue = (1.0 / 0.514) * value;
                        unifiedUncertainty = (1.0 / 0.514) * value;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Get the moisture value in a specified units
        /// </summary>
        /// <param name="units">Units of the result</param>
        /// <returns></returns>
        public override double GetValueAs(WindSpeedUnits units)
        {
            if (IsMissingValue)
                return double.NaN;
            switch (units)
            {
                case WindSpeedUnits.MetresPerSecond:
                    return unifiedValue;
                case WindSpeedUnits.Knots:
                    return unifiedValue * (1.0 / 0.514);
                default:
                    throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Get moisture uncertainty in a specified units
        /// </summary>
        /// <param name="units">Units of the result</param>
        /// <returns></returns>
        public override double GetUncertatintyAs(WindSpeedUnits units)
        {
            if (IsMissingValue)
                return double.NaN;
            switch (units)
            {
                case WindSpeedUnits.MetresPerSecond:
                    return unifiedUncertainty;
                case WindSpeedUnits.Knots:
                    return unifiedUncertainty * (1.0 / 0.514);
                default:
                    throw new NotImplementedException();
            }
        }

    }



    /// <summary>
    /// Contatins an information about the temperature value and its uncertatinty
    /// </summary>
    public class TemperatureValue : TypedClimateParameterValue<TemperatureUnits>
    {
        public override string DefaultClientUnitsString
        {
            get { return "°C"; }
        }

        public TemperatureValue(double value, double uncertainty, TemperatureUnits units = TemperatureUnits.DegreesCelsius)
            : base(Climate.Service.ClimateParameter.FC_TEMPERATURE, TemperatureUnits.DegreesCelsius, TemperatureUnits.DegreesCelsius, -5000.0)
        {
            if (double.MinValue == value || value == missingValue)
            {
                unifiedValue = missingValue;
            }
            else
                switch (units)
                {
                    case TemperatureUnits.DegreesCelsius://DEFAULT! Internal unified format!
                        unifiedValue = value;
                        unifiedUncertainty = uncertainty;
                        break;
                    case TemperatureUnits.DegreesKelvin:
                        unifiedValue = value - 273.15;
                        unifiedUncertainty = uncertainty;
                        break;
                    default:
                        throw new NotImplementedException();
                }
        }

        /// <summary>
        /// Get the temperature value in a specified units
        /// </summary>
        /// <param name="units">Units of the result</param>
        /// <returns></returns>
        public override double GetValueAs(TemperatureUnits units)
        {
            if (IsMissingValue)
                return double.NaN;
            switch (units)
            {
                case TemperatureUnits.DegreesCelsius:
                    return unifiedValue;
                case TemperatureUnits.DegreesKelvin:
                    return unifiedValue + 273.15;
                default:
                    throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Get Temperature uncertainty in a specified units
        /// </summary>
        /// <param name="units">Units of the result</param>
        /// <returns></returns>
        public override double GetUncertatintyAs(TemperatureUnits units)
        {
            if (IsMissingValue)
                return double.NaN;
            switch (units)
            {
                case TemperatureUnits.DegreesCelsius:
                case TemperatureUnits.DegreesKelvin:
                    return unifiedUncertainty;
                default:
                    throw new NotImplementedException();
            }
        }

    }

    /// <summary>
    /// Contains an information about the precipitation rate value and its uncertainty
    /// </summary>
    public class PrecipitationValue : TypedClimateParameterValue<PrecipitationRateUnits>
    {
        public override string DefaultClientUnitsString
        {
            get { return "mm/month"; }
        }

        public PrecipitationValue(double value, double uncertainty, PrecipitationRateUnits units = PrecipitationRateUnits.GramPerSquereMeterPerHour)
            : base(Climate.Service.ClimateParameter.FC_PRECIPITATION, PrecipitationRateUnits.MmPerMonth, PrecipitationRateUnits.GramPerSquereMeterPerHour, -1.0)
        {
            if (double.MinValue == value || value == missingValue)
            {
                unifiedValue = missingValue;
            }
            else
                switch (units)
                {
                    case PrecipitationRateUnits.KilogramPerSquereMeterPerSecond:
                        unifiedValue = value * 36000000;
                        unifiedUncertainty = uncertainty * 36000000;
                        break;
                    case PrecipitationRateUnits.GramPerSquereMeterPerHour: //DEFAULT! Internal unified format!
                        unifiedValue = value;
                        unifiedUncertainty = uncertainty;
                        break;
                    case PrecipitationRateUnits.MmPerMonth:
                        unifiedValue = value * 13.8888888888888889;
                        unifiedUncertainty = uncertainty * 13.8888888888888889;
                        break;
                    default: throw new NotImplementedException();
                }
        }

        /// <summary>
        /// Get the precipitation rate value in a specified units
        /// </summary>
        /// <param name="units">Units of the result</param>
        /// <returns></returns>
        public override double GetValueAs(PrecipitationRateUnits units)
        {
            if (IsMissingValue)
                return double.NaN;
            switch (units)
            {
                case PrecipitationRateUnits.GramPerSquereMeterPerHour:
                    return unifiedValue;                    
                case PrecipitationRateUnits.KilogramPerSquereMeterPerSecond:
                    return unifiedValue / 36000000;                    
                case PrecipitationRateUnits.MmPerMonth:
                    return unifiedValue / 13.888888888888888888888889;
                default:
                    throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Get precipitation rate uncertainty in a specified units
        /// </summary>
        /// <param name="units">Units of the result</param>
        /// <returns></returns>
        public override double GetUncertatintyAs(PrecipitationRateUnits units)
        {
            if (IsMissingValue)
                return double.NaN;
            switch (units)
            {
                case PrecipitationRateUnits.GramPerSquereMeterPerHour:
                    return unifiedUncertainty;
                    break;
                case PrecipitationRateUnits.KilogramPerSquereMeterPerSecond:
                    return unifiedUncertainty / 36000000;
                    break;
                case PrecipitationRateUnits.MmPerMonth:
                    return unifiedUncertainty / 13.888888888888888888888889;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class RelativeHumidityValue : TypedClimateParameterValue<ConcentrationUnits>
    {
        public RelativeHumidityValue(double value, double uncertainty, ConcentrationUnits units = ConcentrationUnits.Part)
            : base(Climate.Service.ClimateParameter.FC_RELATIVE_HUMIDITY, ConcentrationUnits.Percentage, ConcentrationUnits.Part, -1.0)
        {
            if (double.MinValue == value || value == missingValue)
            {
                unifiedValue = missingValue;
            }
            else
                switch (units)
                {
                    case ConcentrationUnits.Part:
                        unifiedUncertainty = uncertainty;
                        unifiedValue = value;
                        break;
                    case ConcentrationUnits.Percentage:
                        unifiedValue = value / 100.0;
                        unifiedUncertainty = uncertainty / 100.0;
                        break;
                    default: throw new NotImplementedException();
                }
        }

        public override double GetValueAs(ConcentrationUnits units)
        {
            switch (units)
            {
                case ConcentrationUnits.Part:
                    return unifiedValue;
                case ConcentrationUnits.Percentage:
                    return unifiedValue * 100.0;
                default: throw new NotImplementedException();
            }
        }

        public override double GetUncertatintyAs(ConcentrationUnits units)
        {
            switch (units)
            {
                case ConcentrationUnits.Part:
                    return unifiedUncertainty;
                case ConcentrationUnits.Percentage:
                    return unifiedUncertainty * 100.0;
                default: throw new NotImplementedException();
            }
        }

        public override string DefaultClientUnitsString
        {
            get { return "%"; }
        }
    }

    public class DiurnalTemperatureRangeValue : TypedClimateParameterValue<TemperatureUnits>
    {
        public override string DefaultClientUnitsString
        {
            get { return "°C"; }
        }

        public DiurnalTemperatureRangeValue(double value, double uncertainty, TemperatureUnits units = TemperatureUnits.DegreesCelsius)
            : base(Climate.Service.ClimateParameter.FC_DIURNAL_TEMPERATURE_RANGE, TemperatureUnits.DegreesCelsius, TemperatureUnits.DegreesCelsius, -99999)
        {
            if (double.MinValue == value || value == missingValue)
            {
                unifiedValue = missingValue;
            }
            else
                switch (units)
                {
                    case TemperatureUnits.DegreesCelsius://DEFAULT! Internal unified format!
                        unifiedValue = value;
                        unifiedUncertainty = uncertainty;
                        break;
                    case TemperatureUnits.DegreesKelvin:
                        unifiedValue = value - 273.15;
                        unifiedUncertainty = uncertainty;
                        break;
                    default:
                        throw new NotImplementedException();
                }
        }

        /// <summary>
        /// Get the temperature value in a specified units
        /// </summary>
        /// <param name="units">Units of the result</param>
        /// <returns></returns>
        public override double GetValueAs(TemperatureUnits units)
        {
            if (IsMissingValue)
                return double.NaN;
            switch (units)
            {
                case TemperatureUnits.DegreesCelsius:
                    return unifiedValue;
                case TemperatureUnits.DegreesKelvin:
                    return unifiedValue + 273.15;
                default:
                    throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Get Temperature uncertainty in a specified units
        /// </summary>
        /// <param name="units">Units of the result</param>
        /// <returns></returns>
        public override double GetUncertatintyAs(TemperatureUnits units)
        {
            if (IsMissingValue)
                return double.NaN;
            switch (units)
            {
                case TemperatureUnits.DegreesCelsius:
                case TemperatureUnits.DegreesKelvin:
                    return unifiedUncertainty;
                default:
                    throw new NotImplementedException();
            }
        }
    }


    public class FrostDayFrequencyValue : TypedClimateParameterValue<DaysFreqUnits>
    {
        public override string DefaultClientUnitsString
        {
            get { return Enum.GetName(typeof(DaysFreqUnits), DaysFreqUnits.DaysCount); }
        }

        public FrostDayFrequencyValue(double value, double uncertainty, DaysFreqUnits units = DaysFreqUnits.DaysCount)
            : base(Climate.Service.ClimateParameter.FC_FROST_DAY_FREQUENCY, DaysFreqUnits.DaysCount, DaysFreqUnits.DaysCount, -99999)
        {
            if (double.MinValue == value || value == missingValue)
            {
                unifiedValue = missingValue;
            }
            else
            {
                switch (units)
                {
                    case DaysFreqUnits.DaysCount:
                        unifiedUncertainty = uncertainty;
                        unifiedValue = value;
                        break;
                    case DaysFreqUnits.MonthsPart:
                        unifiedValue = value / 30;
                        unifiedUncertainty = uncertainty / 30;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public override double GetValueAs(DaysFreqUnits units)
        {
            if (IsMissingValue)
                return double.NaN;
            switch (units)
            {
                case DaysFreqUnits.DaysCount:
                    return unifiedValue;
                case DaysFreqUnits.MonthsPart:
                    return unifiedValue / 30;
                default: throw new NotFiniteNumberException();
            }
        }

        public override double GetUncertatintyAs(DaysFreqUnits units)
        {
            if (IsMissingValue)
                return double.NaN;
            switch (units)
            {
                case DaysFreqUnits.DaysCount:
                    return unifiedUncertainty;
                case DaysFreqUnits.MonthsPart:
                    return unifiedUncertainty / 30;
                default: throw new NotImplementedException();
            }
        }
    }

    public class WetDayFrequencyValue : TypedClimateParameterValue<DaysFreqUnits>
    {
        public override string DefaultClientUnitsString
        {
            get { return Enum.GetName(typeof(DaysFreqUnits), DaysFreqUnits.DaysCount); }
        }

        public WetDayFrequencyValue(double value, double uncertainty, DaysFreqUnits units = DaysFreqUnits.DaysCount)
            : base(Climate.Service.ClimateParameter.FC_FROST_DAY_FREQUENCY, DaysFreqUnits.DaysCount, DaysFreqUnits.DaysCount, -99999)
        {
            if (double.MinValue == value || value == missingValue)
            {
                unifiedValue = missingValue;
            }
            else
            {
                switch (units)
                {
                    case DaysFreqUnits.DaysCount:
                        unifiedUncertainty = uncertainty;
                        unifiedValue = value;
                        break;
                    case DaysFreqUnits.MonthsPart:
                        unifiedValue = value / 30;
                        unifiedUncertainty = uncertainty / 30;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public override double GetValueAs(DaysFreqUnits units)
        {
            if (IsMissingValue)
                return double.NaN;
            switch (units)
            {
                case DaysFreqUnits.DaysCount:
                    return unifiedValue;
                case DaysFreqUnits.MonthsPart:
                    return unifiedValue / 30;
                default: throw new NotFiniteNumberException();
            }
        }

        public override double GetUncertatintyAs(DaysFreqUnits units)
        {
            if (IsMissingValue)
                return double.NaN;
            switch (units)
            {
                case DaysFreqUnits.DaysCount:
                    return unifiedUncertainty;
                case DaysFreqUnits.MonthsPart:
                    return unifiedUncertainty / 30;
                default: throw new NotImplementedException();
            }
        }
    }

    public class SunPercentageValue : TypedClimateParameterValue<ConcentrationUnits>
    {
        public override string DefaultClientUnitsString
        {
            get { return "%"; }
        }

        public SunPercentageValue(double value, double uncertainty, ConcentrationUnits units = ConcentrationUnits.Percentage)
            : base(Climate.Service.ClimateParameter.FC_SUN_PERCENTAGE, ConcentrationUnits.Percentage, ConcentrationUnits.Percentage, -99999)
        {
            if (double.MinValue == value || value == missingValue)
            {
                unifiedValue = missingValue;
            }
            else
            {
                switch (units)
                {
                    case ConcentrationUnits.Percentage:
                        unifiedUncertainty = uncertainty;
                        unifiedValue = value;
                        break;
                    case ConcentrationUnits.Part:
                        unifiedValue = value / 100;
                        unifiedUncertainty = uncertainty / 100;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public override double GetValueAs(ConcentrationUnits units)
        {
            if (IsMissingValue)
                return double.NaN;
            switch (units)
            {
                case ConcentrationUnits.Percentage:
                    return unifiedValue;
                case ConcentrationUnits.Part:
                    return unifiedValue / 100;
                default: throw new NotFiniteNumberException();
            }
        }

        public override double GetUncertatintyAs(ConcentrationUnits units)
        {
            if (IsMissingValue)
                return double.NaN;
            switch (units)
            {
                case ConcentrationUnits.Percentage:
                    return unifiedUncertainty;
                case ConcentrationUnits.Part:
                    return unifiedUncertainty / 100;
                default: throw new NotFiniteNumberException();
            }
        }
    }
}

namespace Microsoft.Research.Science.Data.Climate.Conventions
{
    public abstract class GlobalConsts
    {
        public const int DefaultValue = -999;
        public const int StartYear = 1961;
        public const int EndYear = 1990;
        public const double tempSpatDerivative = 0.1 / 0.008333;
        public const double tempSpatSecondDerivative = 0.0001 / 0.008333;
        public const double tempTimeSecondDerivative = 5.0 * Math.PI * Math.PI / 144.0;
    }

    public abstract class Namings
    {
        // public static readonly Dictionary<Argument, string> Arguments = new Dictionary<Argument, string>();

        public const string VarNameLogMessage = "Log_mess";
        public const string VarNameLogMessageType = "Log_mess_class";
        public const string VarNameLogMessageTime = "Log_mess_time";

        public const string VarNameConfigProcId = "Config_proc_id";
        public const string VarNameConfigProcName = "Config_proc_name";
        public const string VarNameConfigProcDesc = "Config_proc_desc";
        public const string VarNameConfigProcSettings = "Config_proc_settings";

        public const string VarNameDayMax = "StopDay";
        public const string VarNameDayMin = "StartDay";
        public const string VarNameHourMax = "StopHour";
        public const string VarNameHourMin = "StartHour";
        public const string VarNameLatMax = "LatMax";
        public const string VarNameLatMin = "LatMin";
        public const string VarNameLonMax = "LonMax";
        public const string VarNameLonMin = "LonMin";
        public const string VarNameYearMax = "StopYear";
        public const string VarNameYearMin = "StartYear";

        public const string VarNameResult = "Result";
        public const string VarNameUncertainty = "Uncertainty";
        public const string VarNameResultProvenance = "Provenance";

        public const string dimNameLog = "log_dim";
        public const string dimNameCells = "cells_dim";
        public const string dimNameConf = "conf_dim";

        public const string metadataNameProvenanceHint = "ProvenanceToUse";
        public const string metadataNameSupportedParams = "SupportedParameters";
        public const string metadataNameServiceVersion = "ServiceVersion";
        public const string metadataNameProvenanceUsed = "ProvenanceUsed";
        public const string metadataNameProgress = "Progress";
        public const string metadataNameParameter = "RequestedParameter";
        public const string metadataNameCoverage = "RequestedCoverage";
        public const string metadataNameHash = "Hash";
        public const string metadataNameVariationType = "ResearchVariationType";

        //Only in response datasets with request status for REST API.
        public const string restApiMetadataNameExpectedCalculationTime = "ExpectedCalculationTime";
        public const string restApiMetadataNameHash = "Hash";
        public const string restApiMetadataNameResendRequest = "ResendRequest";

        public static readonly Dictionary<ClimateParameter, string> ParameterShortNames = new Dictionary<ClimateParameter, string>();

        /// <summary>
        /// Mapping of pure parameter and coverage type into client-side paramter
        /// </summary>
        static Dictionary<Tuple<Service.ClimateParameter, LandOceanCoverage>, ClimateParameter> dictParameterCoverageFusion = new Dictionary<Tuple<Service.ClimateParameter, LandOceanCoverage>, ClimateParameter>();

        static Namings()
        {
            dictParameterCoverageFusion.Add(new Tuple<Service.ClimateParameter, LandOceanCoverage>(Service.ClimateParameter.FC_TEMPERATURE, LandOceanCoverage.LandArea), ClimateParameter.FC_LAND_AIR_TEMPERATURE);
            dictParameterCoverageFusion.Add(new Tuple<Service.ClimateParameter, LandOceanCoverage>(Service.ClimateParameter.FC_TEMPERATURE, LandOceanCoverage.LandOceanArea), ClimateParameter.FC_TEMPERATURE);
            dictParameterCoverageFusion.Add(new Tuple<Service.ClimateParameter, LandOceanCoverage>(Service.ClimateParameter.FC_TEMPERATURE, LandOceanCoverage.OceanArea), ClimateParameter.FC_OCEAN_AIR_TEMPERATURE);
            dictParameterCoverageFusion.Add(new Tuple<Service.ClimateParameter, LandOceanCoverage>(Service.ClimateParameter.FC_PRECIPITATION, LandOceanCoverage.LandOceanArea), ClimateParameter.FC_PRECIPITATION);
            dictParameterCoverageFusion.Add(new Tuple<Service.ClimateParameter, LandOceanCoverage>(Service.ClimateParameter.FC_SOIL_MOISTURE, LandOceanCoverage.LandArea), ClimateParameter.FC_SOIL_MOISTURE);
            dictParameterCoverageFusion.Add(new Tuple<Service.ClimateParameter, LandOceanCoverage>(Service.ClimateParameter.FC_RELATIVE_HUMIDITY, LandOceanCoverage.LandOceanArea), ClimateParameter.FC_RELATIVE_HUMIDITY);
            dictParameterCoverageFusion.Add(new Tuple<Service.ClimateParameter, LandOceanCoverage>(Service.ClimateParameter.FC_RELATIVE_HUMIDITY, LandOceanCoverage.LandArea), ClimateParameter.FC_LAND_AIR_RELATIVE_HUMIDITY);
            dictParameterCoverageFusion.Add(new Tuple<Service.ClimateParameter, LandOceanCoverage>(Service.ClimateParameter.FC_ELEVATION, LandOceanCoverage.LandOceanArea), ClimateParameter.FC_ELEVATION);
            dictParameterCoverageFusion.Add(new Tuple<Service.ClimateParameter, LandOceanCoverage>(Service.ClimateParameter.FC_ELEVATION, LandOceanCoverage.LandArea), ClimateParameter.FC_LAND_ELEVATION);
            dictParameterCoverageFusion.Add(new Tuple<Service.ClimateParameter, LandOceanCoverage>(Service.ClimateParameter.FC_ELEVATION, LandOceanCoverage.OceanArea), ClimateParameter.FC_OCEAN_DEPTH);
            dictParameterCoverageFusion.Add(new Tuple<Service.ClimateParameter, LandOceanCoverage>(Service.ClimateParameter.FC_WIND_SPEED, LandOceanCoverage.LandArea), ClimateParameter.FC_LAND_WIND_SPEED);
            dictParameterCoverageFusion.Add(new Tuple<Service.ClimateParameter, LandOceanCoverage>(Service.ClimateParameter.FC_DIURNAL_TEMPERATURE_RANGE, LandOceanCoverage.LandArea), ClimateParameter.FC_LAND_DIURNAL_TEMPERATURE_RANGE);
            dictParameterCoverageFusion.Add(new Tuple<Service.ClimateParameter, LandOceanCoverage>(Service.ClimateParameter.FC_FROST_DAY_FREQUENCY, LandOceanCoverage.LandArea), ClimateParameter.FC_LAND_FROST_DAY_FREQUENCY);
            dictParameterCoverageFusion.Add(new Tuple<Service.ClimateParameter, LandOceanCoverage>(Service.ClimateParameter.FC_WET_DAY_FREQUENCY, LandOceanCoverage.LandArea), ClimateParameter.FC_LAND_WET_DAY_FREQUENCY);
            dictParameterCoverageFusion.Add(new Tuple<Service.ClimateParameter, LandOceanCoverage>(Service.ClimateParameter.FC_SUN_PERCENTAGE, LandOceanCoverage.LandArea), ClimateParameter.FC_LAND_SUN_PERCENTAGE);

            ParameterShortNames.Add(ClimateParameter.FC_LAND_AIR_TEMPERATURE, "LT");
            ParameterShortNames.Add(ClimateParameter.FC_OCEAN_AIR_TEMPERATURE, "OT");
            ParameterShortNames.Add(ClimateParameter.FC_TEMPERATURE, "T");
            ParameterShortNames.Add(ClimateParameter.FC_PRECIPITATION, "P");
            ParameterShortNames.Add(ClimateParameter.FC_SOIL_MOISTURE, "SM");
            ParameterShortNames.Add(ClimateParameter.FC_RELATIVE_HUMIDITY, "RH");
            ParameterShortNames.Add(ClimateParameter.FC_LAND_AIR_RELATIVE_HUMIDITY, "LRH");
            ParameterShortNames.Add(ClimateParameter.FC_ELEVATION, "E");
            ParameterShortNames.Add(ClimateParameter.FC_LAND_ELEVATION, "LE");
            ParameterShortNames.Add(ClimateParameter.FC_OCEAN_DEPTH, "OD");
            ParameterShortNames.Add(ClimateParameter.FC_LAND_WIND_SPEED, "LWND");
            ParameterShortNames.Add(ClimateParameter.FC_LAND_DIURNAL_TEMPERATURE_RANGE, "LDTR");
            ParameterShortNames.Add(ClimateParameter.FC_LAND_FROST_DAY_FREQUENCY, "LFDF");
            ParameterShortNames.Add(ClimateParameter.FC_LAND_WET_DAY_FREQUENCY, "LWDF");
            ParameterShortNames.Add(ClimateParameter.FC_LAND_SUN_PERCENTAGE, "LSP");
        }

        private Dictionary<Service.ClimateParameter, Enum> internalUnits = new Dictionary<Service.ClimateParameter, Enum>();

        public static Enum GetInternalUnits(Service.ClimateParameter p)
        {
            switch (p)
            {
                case Service.ClimateParameter.FC_ELEVATION: return new ElevationValue(0, 0).InternalUnits;
                case Service.ClimateParameter.FC_PRECIPITATION: return new PrecipitationValue(0, 0).InternalUnits;
                case Service.ClimateParameter.FC_RELATIVE_HUMIDITY: return new RelativeHumidityValue(0, 0).InternalUnits;
                case Service.ClimateParameter.FC_SOIL_MOISTURE: return new SoilMoistureValue(0, 0).InternalUnits;
                case Service.ClimateParameter.FC_TEMPERATURE: return new TemperatureValue(0, 0).InternalUnits;
                case Service.ClimateParameter.FC_WIND_SPEED: return new WindSpeedValue(0, 0).InternalUnits;
                case Service.ClimateParameter.FC_DIURNAL_TEMPERATURE_RANGE: return new DiurnalTemperatureRangeValue(0, 0).InternalUnits;
                case Service.ClimateParameter.FC_FROST_DAY_FREQUENCY: return new FrostDayFrequencyValue(0, 0).InternalUnits;
                case Service.ClimateParameter.FC_WET_DAY_FREQUENCY: return new WetDayFrequencyValue(0, 0).InternalUnits;
                case Service.ClimateParameter.FC_SUN_PERCENTAGE: return new SunPercentageValue(0, 0).InternalUnits;
                default:
                    throw new NotImplementedException();
            }
        }

        public static ClimateParameter GetParameterByName(string serviceParameterName, string coverageName)
        {
            Tuple<Service.ClimateParameter, LandOceanCoverage> key
            = new Tuple<Service.ClimateParameter, LandOceanCoverage>(GetParameterByName(serviceParameterName), GetCoverageByName(coverageName));
            if (!dictParameterCoverageFusion.ContainsKey(key))
                throw new NotSupportedException();
            return dictParameterCoverageFusion[key];
        }

        public static Service.ClimateParameter GetParameterByName(string name)
        {
            return (Service.ClimateParameter)Enum.Parse(typeof(Service.ClimateParameter), name, false);
        }

        public static string GetParameterName(ClimateParameter parameter)
        {
            switch (parameter)
            {
                case ClimateParameter.FC_TEMPERATURE:
                case ClimateParameter.FC_LAND_AIR_TEMPERATURE:
                case ClimateParameter.FC_OCEAN_AIR_TEMPERATURE:
                    return "FC_TEMPERATURE";
                case ClimateParameter.FC_ELEVATION:
                case ClimateParameter.FC_LAND_ELEVATION:
                case ClimateParameter.FC_OCEAN_DEPTH:
                    return "FC_ELEVATION";
                case ClimateParameter.FC_PRECIPITATION:
                    return "FC_PRECIPITATION";
                case ClimateParameter.FC_SOIL_MOISTURE:
                    return "FC_SOIL_MOISTURE";
                case ClimateParameter.FC_RELATIVE_HUMIDITY:
                case ClimateParameter.FC_LAND_AIR_RELATIVE_HUMIDITY:
                    return "FC_RELATIVE_HUMIDITY";
                case ClimateParameter.FC_LAND_WIND_SPEED:
                    return "FC_WIND_SPEED";
                case ClimateParameter.FC_LAND_DIURNAL_TEMPERATURE_RANGE:
                    return "FC_DIURNAL_TEMPERATURE_RANGE";
                case ClimateParameter.FC_LAND_FROST_DAY_FREQUENCY:
                    return "FC_FROST_DAY_FREQUENCY";
                case ClimateParameter.FC_LAND_WET_DAY_FREQUENCY:
                    return "FC_WET_DAY_FREQUENCY";
                case ClimateParameter.FC_LAND_SUN_PERCENTAGE:
                    return "FC_SUN_PERCENTAGE";
                default:
                    throw new NotSupportedException("Parameter " + parameter + " is not supported");
            }
        }

        public static LandOceanCoverage GetCoverageByName(string coverageName)
        {
            switch (coverageName)
            {
                case "Land": return LandOceanCoverage.LandArea;
                case "Ocean": return LandOceanCoverage.OceanArea;
                case "All": return LandOceanCoverage.LandOceanArea;
                default:
                    throw new NotImplementedException();
            }
        }

        public static string GetCoverageName(ClimateParameter parameter)
        {
            switch (parameter)
            {
                case ClimateParameter.FC_SOIL_MOISTURE:
                case ClimateParameter.FC_LAND_AIR_TEMPERATURE:
                case ClimateParameter.FC_LAND_AIR_RELATIVE_HUMIDITY:
                case ClimateParameter.FC_LAND_ELEVATION:
                case ClimateParameter.FC_LAND_DIURNAL_TEMPERATURE_RANGE:
                case ClimateParameter.FC_LAND_FROST_DAY_FREQUENCY:
                case ClimateParameter.FC_LAND_WET_DAY_FREQUENCY:
                case ClimateParameter.FC_LAND_SUN_PERCENTAGE:
                case ClimateParameter.FC_LAND_WIND_SPEED:
                    return "Land";
                case ClimateParameter.FC_OCEAN_AIR_TEMPERATURE:
                case ClimateParameter.FC_OCEAN_DEPTH:
                    return "Ocean";
                case ClimateParameter.FC_TEMPERATURE:
                case ClimateParameter.FC_PRECIPITATION:
                case ClimateParameter.FC_RELATIVE_HUMIDITY:
                case ClimateParameter.FC_ELEVATION:
                    return "All";
                default:
                    throw new NotSupportedException("Coverage for " + parameter + " is not supported");
            }
        }

    }

    public abstract class FetchClimateResponse
    {

        /// <summary>
        /// A short name of the data source that has been used for calculation of the request. "mixed" means that different datasources were used
        /// </summary>
        public string ProvenanceUsed { get; set; }
        public string ServiceVersion { get; set; }
    }

    public class FetchClimateSingleResponce : FetchClimateResponse
    {
        /// <summary>
        /// A value that contains a requesed data.
        /// </summary>
        public ClimateParameterValue Value { get; set; }

        public override string ToString()
        {
            return Value.IsMissingValue ? "[m/v]" : String.Format("{0:F3}+-{1:F3} {2}",
                Value.GetValueInDefaultClientUnits(), Value.GetUncertaintyInDefaultClientUnits(), Value.DefaultClientUnitsString);
        }
    }

    public class FetchClimateBatchResponce : FetchClimateResponse
    {
        /// <summary>
        /// A values that contains a requesed data.
        /// </summary>
        public ClimateParameterValue[] Values { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            for (int i = 0; i < Values.Length; i++)
            {
                if (first)
                    first = false;
                else
                    sb.Append('\t');
                sb.Append(Values[i].IsMissingValue ? " [m/v]" : String.Format("{0:F3}+-{1:F3}", Values[i].GetValueInDefaultClientUnits(), Values[i].GetUncertaintyInDefaultClientUnits()));
            }
            sb.Append(String.Format(" {0}", Values[0].DefaultClientUnitsString));
            return sb.ToString();
        }
    }

    public class FetchClimateGridResponce : FetchClimateResponse
    {
        /// <summary>
        /// A values that contains a requesed data.
        /// </summary>
        public ClimateParameterValue[,] Values { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Format("Grid {0}x{1} Units: {2}", Values.GetLength(0), Values.GetLength(1), Values[0, 0].DefaultClientUnitsString));
            for (int i = 0; i < Values.GetLength(0); i++)
            {
                for (int j = 0; j < Values.GetLength(1); j++)
                    sb.Append(Values[i, j].IsMissingValue ? "[m/v]\t" : string.Format("{0:F3}+-{1:F3}\t", Values[i, j].GetValueInDefaultClientUnits(), Values[i, j].GetUncertaintyInDefaultClientUnits()));
                sb.AppendLine();
            }
            return sb.ToString();

        }
    }
}


//namespace Microsoft.Research.Science.Data.Cliamte.Presenters
//{
//    public class Conventions
//    {
//        public enum ClimateParameter { NearGroundTemperature, PrecipitationRate }
//        public enum Axis { Time, Longitude, Latitude }
//        public enum Aggregation { Mean, Max, Min, Point }

//        public static Dictionary<string, object> GetUnifiedMetadata(ClimateParameter p)
//        {
//            Dictionary<string, object> md = new Dictionary<string, object>();
//            switch (p)
//            {
//                case ClimateParameter.NearGroundTemperature:
//                    md["Name"] = "air0";
//                    md["long_name"] = "Temperature near surface";
//                    md["units"] = "degK";
//                    md["scale_factor"] = 1.0;
//                    md["add_offset"] = 0.0;
//                    break;
//                case ClimateParameter.PrecipitationRate:
//                    md["Name"] = "prate";
//                    md["long_name"] = "Precipitation Rate";
//                    md["units"] = "g/m^2/min";
//                    md["scale_factor"] = 1.0;
//                    md["add_offset"] = 0.0;
//                    break;
//            }
//            md["missing_value"] = GetMissingValueForParameter(p);
//            return md;
//        }

//        public static string GetAxisName(Axis axis)
//        {
//            switch (axis)
//            {
//                case Axis.Time: return "time";
//                case Axis.Latitude: return "lat";
//                case Axis.Longitude: return "lon";
//                default: throw new NotImplementedException();
//            }
//        }

//        public static double GetMissingValueForParameter(ClimateParameter p)
//        {
//            switch (p)
//            {
//                case ClimateParameter.NearGroundTemperature: return -1.0;
//                case ClimateParameter.PrecipitationRate: return -1.0;
//                default: throw new NotImplementedException();
//            }
//        }

//        static Conventions()
//        {
//            AxisNameConvertionDictionary.Add("lon", Axis.Longitude);
//            AxisNameConvertionDictionary.Add("Longitude", Axis.Longitude);
//            AxisNameConvertionDictionary.Add("lat", Axis.Latitude);
//            AxisNameConvertionDictionary.Add("Latitude", Axis.Latitude);
//            AxisNameConvertionDictionary.Add("time", Axis.Time);
//            AxisNameConvertionDictionary.Add("Time", Axis.Time);

//            ClimateParameterNames.Add("air", ClimateParameter.NearGroundTemperature); //NCEP
//            ClimateParameterNames.Add("air0", ClimateParameter.NearGroundTemperature); //NCEP
//            ClimateParameterNames.Add("prate", ClimateParameter.PrecipitationRate); //NCEP
//        }

//        protected static readonly Dictionary<string, Axis> axisNameConvertionDictionary = new Dictionary<string, Axis>();
//        public static Dictionary<string, Axis> AxisNameConvertionDictionary { get { return axisNameConvertionDictionary; } }


//        protected static readonly Dictionary<string, ClimateParameter> climateParameterNames = new Dictionary<string, ClimateParameter>();
//        public static Dictionary<string, ClimateParameter> ClimateParameterNames { get { return climateParameterNames; } }

//    }
//}
