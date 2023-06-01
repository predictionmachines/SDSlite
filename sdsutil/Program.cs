// Copyright © Microsoft Corporation, All Rights Reserved.
using Microsoft.Research.Science.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Range = Microsoft.Research.Science.Data.Range;

namespace sdsutil
{
    class Program
    {
        static DataSet OpenDataSet(string uri)
        {
            DataSet ds = DataSet.Open(uri);
            ds.IsAutocommitEnabled = false;
            return ds;
        }

        static void DoList(string uri)
        {
            var ds = OpenDataSet(uri);
            foreach (var item in ds.Variables)
                PrintVariable(item);

        }

        static void DoMeta(string uri, params string[] args)
        {
            var ds = OpenDataSet(uri);
            if (args == null || args.Length == 0)
            {
                // printing global metadata
                if (ds.Metadata.Count == 0)
                    Console.WriteLine("No global metadata");
                else
                    PrintMetadata(ds.Metadata);
            }
            else
            {
                // process list of <var> from command line parameters
                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i];
                    Variable v = GetVar(ds, arg);
                    if (v != null)
                    {
                        PrintVariable(v);
                        PrintMetadata(v.Metadata);
                    }
                    else
                        WriteError("No such variable: " + args[i]);
                }
            }
        }

        static void DoUpdate(string uri, string[] args)
        {
            var ds = OpenDataSet(uri);
            // "update" command - update variables metadata
            try
            {
                UpdateMetadata(ds, args);
            }
            catch (IndexOutOfRangeException)
            {
                WriteError("Missing arguments.");
            }
            catch (FormatException fe)
            {
                WriteError("Cannot parse an argument: " + fe.Message);
            }
            catch (Exception ex)
            {
                WriteError("Exception: " + ex.ToString());
            }
        }

        static void DoData(string uri, params string[] args)
        {
            var ds = OpenDataSet(uri);
            // "data" command - print variables metadata
            if (args == null || args.Length == 0)
            {
                // empty list of variables == all variables
                foreach (var item in ds.Variables)
                {
                    PrintVariable(item);
                    PrintMetadata(item.Metadata);
                    PrintData(item, null, null);
                }
            }
            else
            {
                // process list of <var> from command line parameters
                for (int i = 0; i < args.Length; i++)
                {
                    Variable v = null;
                    string varstride = args[i];
                    string range = null;
                    string format = null;
                    int p = varstride.IndexOf('[');
                    if (p >= 0)
                    {
                        // data range specified
                        int p2 = varstride.IndexOf(']', p);
                        if (p2 < p) varstride = null;
                        else
                        {
                            range = varstride.Substring(p + 1, p2 - p - 1);
                            varstride = varstride.Remove(p, p2 - p + 1);
                        }
                    }
                    if (varstride != null)
                    {
                        p = varstride.IndexOf('{');
                        if (p >= 0)
                        {
                            // format specified
                            int p2 = varstride.IndexOf('}', p);
                            if (p2 < p) varstride = null;
                            else
                            {
                                format = varstride.Substring(p + 1, p2 - p - 1);
                                varstride = varstride.Remove(p, p2 - p + 1);
                            }
                        }
                    }
                    if (varstride == null)
                        WriteError("Format error in " + args[i]);
                    else
                    {
                        if (varstride.All(c => char.IsDigit(c)))
                            v = (from item in ds.Variables where item.ID == int.Parse(varstride) select item).FirstOrDefault();
                        else
                            v = (from item in ds.Variables where item.Name == varstride select item).FirstOrDefault(); ;
                        if (v != null)
                        {
                            PrintVariable(v);
                            PrintMetadata(v.Metadata);
                            PrintData(v, range, format);
                        }
                        else
                            WriteError("No such variable: " + varstride);
                    }
                }
            }
        }

        static void DoSlice(string uri, Dictionary<string, Range> ranges, string outputUri)
        {
            DataSetUri dstUri = DataSetUri.Create(outputUri);
            if (dstUri.ProviderName.StartsWith("memory"))
                throw new NotSupportedException("Copying to memory is not supported by the utility.");

            using (DataSet src = DataSet.Open(uri))
            {
                using (DataSet mds = DataSet.Open("msds:memory"))
                using (DataSet mds2 = DataSet.Open("msds:memory"))
                {
                    mds.IsAutocommitEnabled = false;
                    mds2.IsAutocommitEnabled = false;
                    foreach (var var in src.Variables)
                    {
                        bool doSlice = false;
                        foreach (var dim in var.Dimensions)
                            if (var.Dimensions.Contains(dim.Name))
                            {
                                doSlice = true;
                                break;
                            }
                        if (doSlice)
                        {
                            var refVar = mds.AddVariableByReference(var);
                            int rank = refVar.Rank;
                            int[] origin = new int[rank];
                            int[] stride = new int[rank];
                            int[] count = new int[rank];
                            for (int i = 0; i < rank; i++)
                            {
                                Range r;
                                if (ranges.TryGetValue(var.Dimensions[i].Name, out r))
                                {
                                    if (r.IsReduced)
                                    {
                                        origin[i] = r.Origin;
                                        stride[i] = 0;
                                        count[i] = 1;
                                    }
                                    else
                                        throw new NotSupportedException("sds slice supports only dimension fixing");
                                }
                                else
                                {
                                    origin[i] = 0;
                                    stride[i] = 1;
                                    count[i] = 0;
                                }
                            }
                            Variable strVar = StrideVariable(refVar, origin, stride, count);
                            mds2.AddVariableByReference(strVar);
                        }
                        else
                        {
                            mds2.AddVariableByReference(var);
                        }
                    }
                    // mds2 is ready to be committed
                    try
                    {
                        mds2.Commit();
                    }
                    catch (DistributedCommitFailedException dex)
                    {
                        if (dex.InnerException is ConstraintsFailedException)
                        {
                            string error = String.Format("Input DataSets are incompatible: {0}", ((ConstraintsFailedException)(dex.InnerException)).Message);
                            WriteError(error);
                            return;
                        }
                        throw;
                    }
                    catch (ConstraintsFailedException cex)
                    {
                        string error = String.Format("Input DataSets are incompatible: {0}", cex.Message);
                        WriteError(error);
                        return;
                    }
                    // mds2 is ready to be cloned
                    Microsoft.Research.Science.Data.Utilities.DataSetCloning.Clone(mds2, dstUri,
                        Microsoft.Research.Science.Data.Utilities.DataSetCloning.DefaultUpdater).Dispose();
                }
            }
        }

        private static Variable StrideVariable(Variable var, int[] origin, int[] stride, int[] count)
        {
            MethodInfo strideMethod = _strideMethodGeneric.MakeGenericMethod(var.TypeOfData);
            return (Variable)strideMethod.Invoke(var.DataSet, new object[] { var, origin, stride, count, var.Name });
        }

        private static readonly MethodInfo _strideMethodGeneric =
            typeof(DataSet).GetMethods().FirstOrDefault(mi => mi.Name == "StrideVariable" && mi.GetParameters().Length == 5);

        static void DoMerge(string[] URIs, string output)
        {
            DataSetUri dstUri = DataSetUri.Create(output);
            if (dstUri.ProviderName.StartsWith("memory"))
                throw new NotSupportedException("Copying to memory is not supported by the utility.");

            DataSet[] dss = new DataSet[URIs.Length];
            int i = 0;
            try
            {
                for (i = 0; i < dss.Length; i++)
                    dss[i] = DataSet.Open(URIs[i]);
            }
            catch
            {
                for (; --i >= 0; )
                    dss[i].Dispose();
                throw;
            }

            using (DataSet mds = DataSet.Open("msds:memory"))
            {
                mds.IsAutocommitEnabled = false;
                IMetadataConflictResolver conflictResolver = new WarningConflictResolver();
                foreach (var ds in dss)
                {
                    // Global metadata
                    foreach (var attr in ds.Metadata)
                    {
                        object val;
                        if (mds.Metadata.ContainsKey(attr.Key, SchemaVersion.Recent))
                            val = conflictResolver.Resolve(attr.Key, mds.Metadata[attr.Key, SchemaVersion.Recent], attr.Value);
                        else
                            val = attr.Value;
                        mds.Metadata[attr.Key] = val;
                    }

                    // Variables
                    foreach (var var in ds.Variables)
                        mds.AddVariableByReference(var);
                }
                try
                {
                    mds.Commit();
                }
                catch (DistributedCommitFailedException dex)
                {
                    if (dex.InnerException is ConstraintsFailedException)
                    {
                        string error = String.Format("Input DataSets are incompatible: {0}", ((ConstraintsFailedException)(dex.InnerException)).Message);
                        WriteError(error);
                        return;
                    }
                    throw;
                }
                catch (ConstraintsFailedException cex)
                {
                    string error = String.Format("Input DataSets are incompatible: {0}", cex.Message);
                    WriteError(error);
                    return;
                }
                Microsoft.Research.Science.Data.Utilities.DataSetCloning.Clone(mds, dstUri,
                    Microsoft.Research.Science.Data.Utilities.DataSetCloning.DefaultUpdater).Dispose();
            }
        }

        static void DoCopy(string uri, string uri2)
        {
            var ds = OpenDataSet(uri);

            Console.Write("Copying . . . ");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            DataSetUri dstUri = DataSetUri.Create(uri2);
            if (dstUri.ProviderName.StartsWith("memory"))
                throw new NotSupportedException("Copying to memory is not supported by the utility.");
            DataSet ds2 = Microsoft.Research.Science.Data.Utilities.DataSetCloning.Clone(ds, dstUri); //ds.Clone(args[2]);
            ds2.Dispose();
            sw.Stop();
            Console.Write(String.Format("({0}) ", sw.Elapsed));
        }

        static bool AssertNumberOfArgs(string[] args, int min, int max)
        {
            if (args.Length < min)
            {
                WriteError("Not enough of arguments.");
                return false;
            }
            else if (args.Length > max)
            {
                WriteError("Too many arguments.");
                return false;
            }
            return true;
        }

        static bool AssertNumberOfArgs(string[] args, int n)
        {
            return AssertNumberOfArgs(args, n, n);
        }

        static string[] GetCommandArgs(string[] args, int startIndex)
        {
            if (startIndex >= args.Length) return new string[0];
            string[] vargs = new string[args.Length - startIndex];
            Array.Copy(args, startIndex, vargs, 0, vargs.Length);
            return vargs;
        }

        static void Main(string[] args)
        {
            if (args.Length == 0) { PrintUsage(); return; }
            try
            {
                string command = args[0];
                if (args.Length == 1 && (command.Contains('.') || command.Contains(':')))
                {
                    // Command cannot . or : for these are mean either extension or schema prefix.
                    DoList(args[0]);
                    return;
                }
                if (command == "list")
                {
                    if (!AssertNumberOfArgs(args, 2)) return;
                    DoList(args[1]);
                }
                else if (command == "meta")
                {
                    if (!AssertNumberOfArgs(args, 2, int.MaxValue)) return;
                    DoMeta(args[1], GetCommandArgs(args, 2));
                }
                else if (command == "update")
                {
                    if (!AssertNumberOfArgs(args, 4, int.MaxValue)) return;
                    DoUpdate(args[1], GetCommandArgs(args, 2));
                }
                else if (command == "data")
                {
                    if (!AssertNumberOfArgs(args, 2, int.MaxValue)) return;
                    DoData(args[1], GetCommandArgs(args, 2));
                }
                else if (command == "copy")
                {
                    if (!AssertNumberOfArgs(args, 3, int.MaxValue)) return;
                    if (args.Length == 3)
                        DoCopy(args[1], args[2]);
                    else // slicing
                    {
                        string srcUri = args[1];
                        Dictionary<string, Range> ranges = new Dictionary<string, Range>();
                        int i;
                        try
                        {
                            for (i = 2; i < args.Length - 1; i++)
                            {
                                string s = args[i];
                                string[] parts = s.Split('=');
                                Range r = DataSet.ReduceDim(int.Parse(parts[1]));
                                ranges[parts[0]] = r;
                            }
                        }
                        catch
                        {
                            WriteError("Range arguments are incorrect.");
                            return;
                        }
                        string dstUri = args[i];
                        DoSlice(srcUri, ranges, dstUri);
                    }
                }
                else if (command == "merge")
                {
                    if (!AssertNumberOfArgs(args, 4, int.MaxValue)) return;
                    string[] src = new string[args.Length - 2];
                    Array.Copy(args, 1, src, 0, src.Length);
                    DoMerge(src, args[args.Length - 1]);
                }
                else if (command == "info")
                {
                    if (!AssertNumberOfArgs(args, 1)) return;
                    DoInfo();
                }
                else if (command == "/?" || command == "help")
                {
                    PrintUsage();
                }
                else
                    WriteError("Unknown command.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("\nFAILED: ");
                do
                {
#if DEBUG
                    Console.Error.WriteLine(ex);
#else
                    Console.Error.WriteLine(ex.Message);
#endif
                    ex = ex.InnerException;
                } while (ex != null);
                Console.ResetColor();
            }
        }

        private static void UpdateMetadata(DataSet ds, string[] args)
        {
            // sds update air.nc /g val "-50.0" 
            // sds update air.nc /g /t:Double "min" "-50.0"  
            // sds update air.nc air /t:Double "min" "-50.0" 
            // sds update air.nc air "min" "-50.0" 
            // sds update air.nc air /t:Double "min" "-50.0" "-50.2"
            Variable v = null;
            int pi = 0;
            if (args[pi] != "/g" && !args[pi].StartsWith("/t")) // not global metadata => var 
            {
                string avar = args[pi++];
                v = GetVar(ds, avar);
                if (v == null)
                {
                    WriteError("No such variable: " + avar);
                    return;
                }
            }
            else if (args[pi] == "/g") // global 
                pi++;
            string stype = null;
            if (args[pi].StartsWith("/t:"))
            {
                stype = args[pi].Substring(3);
                pi++;
            }

            // Key
            string key = args[pi++];
            object value = null;
            Type type = null;
            if (stype != null)
            {
                type = GetType(stype);
                if (type == null) return;
            }

            // Values
            ArrayList values = new ArrayList();
            Type elType = type;
            if (type != null && type.IsArray)
                elType = type.GetElementType();
            while (pi < args.Length)
            {
                string svalue = args[pi++];
                value = GetValue(svalue, ref elType);
                values.Add(value);
                if (values.Count > 1 && (stype != null && !type.IsArray))
                {
                    WriteError("Too many values specified.");
                    return;
                }
            }
            if (values.Count == 0 && (type == null || !type.IsArray))
            {
                WriteError("No data specified.");
                return;
            }
            if ((stype != null && type.IsArray) || values.Count > 1) // array
            {
                Array arr = Array.CreateInstance(elType, values.Count);
                for (int i = 0; i < arr.Length; i++)
                    arr.SetValue(values[i], i);
                value = arr;
            }
            else
            {
                value = values[0];
            }

            Console.WriteLine();
            try
            {
                Console.WriteLine("Key: " + key);
                Console.WriteLine("Type: " + value.GetType());
                Console.Write("Value: ");
                foreach (var item in values)
                    Console.Write(item + " ");
                Console.WriteLine();

                if (v != null)
                    v.Metadata[key] = value;
                else
                    ds.Metadata[key] = value;
                ds.Commit();
                Console.WriteLine("\nMetadata updated.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("\nFAILED: " + ex);
                Console.ResetColor();
            }
        }

        private static object GetValue(string value, ref Type type)
        {
            if (type == null)
            {
                object o = InferValue(value);
                type = o.GetType();
                return o;
            }
            return TypeUtils.Parse(value, type);
        }

        private static Type GetType(string stype)
        {
            if (!stype.StartsWith("System."))
                stype = "System." + stype;
            try
            {
                return Type.GetType(stype);
            }
            catch (Exception)
            {
                WriteError("Unexpected type of the variable.");
                return null;
            }
        }

        /// <summary>Displays DS Core version and list of available providers.</summary>
        private static void DoInfo()
        {
            string coreVer = typeof(DataSet).Assembly.GetName().Version.ToString();
            Console.WriteLine("DataSet Core version {0}", coreVer);
            Console.WriteLine();
            Console.WriteLine(Microsoft.Research.Science.Data.Factory.DataSetFactory.RegisteredToString());
        }

        private static object InferValue(string svalue)
        {
            DateTime dt;
            Double dbl;
            bool b;
            if (Double.TryParse(svalue, System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out dbl))
                return dbl;
            if (bool.TryParse(svalue, out b))
                return b;
            if (DateTime.TryParse(svalue, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt;
            return svalue;
        }

        private static Variable GetVar(DataSet ds, string arg)
        {
            Variable v;
            if (arg.All(c => char.IsDigit(c)))
                v = (from item in ds.Variables where item.ID == int.Parse(arg) select item).FirstOrDefault();
            else
                v = (from item in ds.Variables where item.Name == arg select item).FirstOrDefault();

            return v;
        }

        static void PrintUsage()
        {
            Console.Error.WriteLine(@"Usage: sds [<command>] [<arguments...>]
    sds <dsuri>
    sds list <dsuri> - list dataset variables

    sds meta <dsuri> [ <var> ... ] - print variable metadata, 
		where <var> can be a name or id;
		if <var> is not specified, global metadata is printed.

	Examples:	sds meta air.nc
			sds meta air.nc air

    sds update <dsuri> [/g|<var>] [/t:<Type>] <key> <value> [<value> ...] - update variable metadata
        /g - updates global metadata
        /t:<Type> - type is specified
        Either <var> or /g must be specified.
        If /t is not specified, types Double, DateTime, Boolean, String and
        arrays of them are inferred.

        Examples:   sds update air.nc air avg 12.5
                    sds update air.nc air /t:String descr 100.0 
                    sds update air.nc air range -50.0 50.0
                    sds update air.nc /g /t:Double[] dx 100.02 101.02

    sds data <dsuri> [ <varstride> ... ] - print variable data, 
        where <varstride> == <var>[<range1>,...]{<cellsize>:<dataformat>};
        if no <varstride> specified, prints data of all variables.

	Example:    sds data air.nc air
                    sds data air.nc air[1:2,10:15,10:18]{6} lon{:F2}

    sds copy <srcDsUri> [<dim1>=<val1> ...] <dstDsUri> - copy source dataset into destination dataset.
        If <dim1>=<val1> ... are specified, slices the dataset by those dimensions.

	Example:    sds copy air.nc air.csv?openMode=create
                    sds copy air.nc time=0 airTime0.nc

    sds merge <srcUri1> <srcUri2> ... <dstUri> - merge two or more datasets into single dataset.

        Example:    sds merge air0.nc air1.nc air.nc?openMode=create&enableRollback=false

    sds info - print DataSet Core version and list of available providers.
");
        }
        static void PrintVariable(Variable v)
        {
            var savedColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(v);
            Console.ForegroundColor = savedColor;
        }

        static void PrintMetadata(MetadataDictionary metadata)
        {
            foreach (var item in metadata)
            {
                Console.Write("{0,20} =", item.Key);
                if (item.Value is IEnumerable && !(item.Value is string))
                    foreach (var value in item.Value as IEnumerable)
                        Console.Write(" {0}", value);
                else
                    Console.Write(" {0}", item.Value);
                Console.WriteLine();
            }
        }

        static void PrintData(Variable v, string range, string format)
        {
            try
            {
                // Find out which data to take
                int rank = v.Rank;
                var shape = v.GetShape();
                var origin = rank == 0 ? null : new int[rank];
                var count = rank == 0 ? null : new int[rank];
                var stride = rank == 0 ? null : new int[rank];
                for (int i = 0; i < rank; i++)
                {
                    origin[i] = 0;
                    count[i] = shape[i];
                    stride[i] = 1;
                }
                if (range != null)
                {
                    // parse range specification 'from:to' or 'from:step:to'
                    var ranges = range.Split(',');
                    for (int i = 0; i < rank && i < ranges.Length; i++)
                    {
                        var fromto = ranges[i].Split(':');
                        if (!int.TryParse(fromto[0], out origin[i])) origin[i] = 0;
                        if (origin[i] < 0) origin[i] = 0;
                        if (origin[i] >= shape[i]) origin[i] = shape[i] - 1;
                        if (fromto.Length < 2) count[i] = 1;
                        else
                        {
                            if (!int.TryParse(fromto[fromto.Length - 1], out count[i])) count[i] = 0;
                            if (count[i] == 0 || count[i] >= shape[i]) count[i] = shape[i] - 1;
                            if (count[i] < origin[i]) count[i] = origin[i];
                            if (fromto.Length > 2)
                            {
                                if (!int.TryParse(fromto[1], out stride[i])) stride[i] = 1;
                                if (stride[i] <= 0) stride[i] = 1;
                            }
                            count[i] = 1 + (count[i] - origin[i]) / stride[i];
                        }
                    }
                }
                // Now get the data
                Array data = v.GetData(origin, stride, count);
                if (data == null || (rank > 0 && data.Rank != rank))
                    Console.Error.WriteLine("No data");
                else
                {
                    int prefix = string.Join(",", Array.ConvertAll(shape, i => i.ToString())).Length + 2;
                    // parse format
                    int cell = 8;
                    string frm = "{0,8}";
                    if (format != null)
                    {
                        int p = format.IndexOf(':');
                        if (p < 0)
                        {
                            if (!int.TryParse(format, out cell)) cell = 8;
                            frm = "{0," + cell + "}";
                        }
                        else
                        {
                            if (!int.TryParse(format.Substring(0, p), out cell)) cell = 8;
                            frm = "{0," + cell + format.Substring(p) + "}";
                        }
                    }

                    // Now print the data
                    if (rank == 0) // scalar variable
                    {
                        Console.WriteLine();
                        string item = string.Format(frm, data.GetValue(0));
                        // check that item fits a cell
                        if (item.Length > cell) item = item.Substring(0, cell - 1) + "#";
                        Console.Write(" "); Console.Write(item);
                    }
                    else // has rank > 0 (array)
                    {
                        int[] indices = new int[data.Rank];
                        int[] varIndices = new int[rank];
						int bufferWidth = Math.Max(80, Console.BufferWidth);
						// .. to allow for non-console output
                        string lineSeparator = Environment.NewLine;
                        while (indices[0] < data.GetLength(0))
                        {
                            // print last dimension
                            Console.Write(lineSeparator);
                            lineSeparator = string.Empty;
                            int lastIndex = data.Rank - 1;
                            indices[lastIndex] = 0;
                            while (indices[lastIndex] < data.GetLength(lastIndex))
                            {
                                // print line of cells
                                // print prefix of current indices
                                for (int i = 0; i < rank; i++)
                                    varIndices[i] = origin[i] + indices[i] * stride[i];
                                string item = ("[" + string.Join(",", Array.ConvertAll(varIndices, i => i.ToString())) + "]").PadRight(prefix);
                                Console.Write(item);
                                int left = bufferWidth - item.Length;
                                while (left > cell && indices[lastIndex] < data.GetLength(lastIndex))
                                {
                                    item = string.Format(frm, data.GetValue(indices));
                                    // check that item fits a cell
                                    if (item.Length > cell) item = item.Substring(0, cell - 1) + "#";
                                    Console.Write(" "); Console.Write(item);
                                    left -= item.Length + 1;
                                    indices[lastIndex]++;
                                    varIndices[lastIndex] += stride[lastIndex];
                                }
                                Console.WriteLine();
                                if (indices[lastIndex] < data.GetLength(lastIndex))
                                    lineSeparator = Environment.NewLine;
                            }
                            lastIndex -= 1;
                            // increment recursively other dimensions
                            while (lastIndex >= 0 && ++indices[lastIndex] >= data.GetLength(lastIndex))
                            {
                                if (lastIndex > 0) indices[lastIndex] = 0;
                                lastIndex -= 1;
                            }
                        }
                    }
                }
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error while printing data: " + e.Message);
            }
        }

        public static void WriteError(string s)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(s);
            Console.ResetColor();
        }
    }
}

