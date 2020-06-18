using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Microsoft.Research.Science.Data.Imperative
{
    public static partial class DataSetExtensions
    {
        private static void AssertExistingAxis(DataSet ds, string name, Type elementType, string units, int length)
        {
            Variable v = ds[name];
            if (v.Rank != 1 || v.Dimensions[0].Name != name)
                throw new InvalidOperationException("DataSet already contains a variable with same name which is not an axis");
            if (v.TypeOfData != elementType)
                throw new InvalidOperationException("DataSet already contains the axis with different type of data");

            string munits = null;
            if (v.Metadata.ContainsKey("Units"))
                munits = v.Metadata["Units"] as string;
            if (units != munits)
                throw new InvalidOperationException("DataSet already contains the axis with different units");
            if (v.Dimensions[0].Length != length)
                throw new InvalidOperationException("DataSet already contains the axis with different length");
        }

        /// <summary>
        /// Creates new uniform axis variable of type double and adds it to the dataset.
        /// </summary>
        /// <param name="ds">Target datas set.</param>
        /// <param name="name">Name of the axis.</param>
        /// <param name="units">Units of measurement of values of the axis.</param>
        /// <param name="min">Starting axis value.</param>
        /// <param name="max">Final axis value.</param>
        /// <param name="delta">Step of values for the axis.</param>
        /// <returns>New variable.</returns>
        /// <remarks>
        /// <para>Adds new axis to the dataset, which is a 1d-variable depending on a dimension with name equal to the variable name, 
        /// and sets its "Units" attribute to given units string. 
        /// Values for the axis are uniform and are defined by a user using min,max and step values, so 
        /// that value[i] = min + step * i.</para>
        /// <para>If the dataset already contains a variable with given name, the method just returns that variable, 
        /// if it has proper type, rank and units. Values are not checked.
        /// </para>
        /// <example>
        /// <code>
        ///  DataSet ds = DataSet.Open("output.csv?openMode=create");
        ///  
        ///  ds.AddAxis("x", "m", 0, 10, 10);
        ///  ds.AddAxis("y", "m", 0.0, 10.0, 5.0);
        ///
        ///  double[,] data = new[,] { { 1.0, 2.0, 3.0 }, { 11.0, 12.0, 13.0 } };
        ///  ds.Add("z", "unitz", 999.0, data); 
        /// </code>
        /// </example>
        /// <para>Axis is a 1d-variable with name equal to its dimension name. Example: variable "lat" depending on dimension "lat" is an axis.
        /// Coordinate system is a set of axes of a dataset. Can be defined as a list of the variables names in any order.
        /// Example: "lat,lon,time", if variables lat,lon,time are axes of a dataset.
        /// Methods <see cref="GetValue(DataSet, string, object[])"/> allow to get value from a variable defined in a coordinate system
        /// from its coordinates instead of indices.</para>
        /// <para>
        /// DataSet has a default coordinate system which consists from all presented axes.
        /// If no dimensions or axes are given when adding new variable (<see cref="Add{T}(DataSet,string,string,object,T)"/>),
        /// default coordinate system is chosen, if appropriate.
        /// </para>
        /// </remarks>
        public static Variable AddAxis(this DataSet ds, string name, string units, double min, double max, double delta)
        {
            if (ds == null) throw new ArgumentNullException("ds");
            if (String.IsNullOrEmpty(name)) throw new ArgumentException("Axis name is incorrect");
            int n = checked((int)((max - min) / delta)) + 1;
            if (ds.Variables.Contains(name))
            {
                AssertExistingAxis(ds, name, typeof(double), units, n);
                return ds.Variables[name];
            }

            if (n < 0) throw new ArgumentException("Number of elements is negative");
            double[] data = new double[n];
            for (int i = 0; i < n; i++)
                data[i] = min + i * delta;

            var var = ds.Add<double[]>(name, data, name);
            if (!String.IsNullOrEmpty(units))
                var.Metadata["Units"] = units;
            return var;
        }

        /// <summary>
        /// Creates new axis variable and adds it to the dataset.
        /// </summary>
        /// <param name="ds">Target datas set.</param>
        /// <param name="name">Name of the axis.</param>
        /// <param name="units">Units of measurement of values of the axis.</param>
        /// <param name="data">Values of the axis.</param>
        /// <typeparam name="T">Full type of the axis.</typeparam>
        /// <returns>New variable.</returns>
        /// <remarks>
        /// <para>Adds new axis to the dataset, which is a 1d-variable depending on a dimension with name equal to the variable name, 
        /// and sets its "Units" attribute to given units string.</para>
        /// <para>If the dataset already contains a variable with given name, the method just returns that variable, 
        /// if it has proper type, rank and units. Values are not checked.
        /// </para>
        /// <para>
        /// If the <paramref name="data"/> parameter is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// <example>
        /// <code>
        ///  DataSet ds = DataSet.Open("output.csv?openMode=create");
        ///  
        ///  ds.AddAxis("x", "m", 0, 10, 10);
        ///  ds.AddAxis("y", "m", new double[] {0, 0.5, 2.5, 10});
        ///
        ///  double[,] data = new[,] { { 1.0, 2.0, 3.0, 4.0 }, { 11.0, 12.0, 13.0, 14.0 } };
        ///  ds.Add("z", "unitz", 999.0, data); 
        /// </code>
        /// </example>
        /// <para>Axis is a 1d-variable with name equal to its dimension name. Example: variable "lat" depending on dimension "lat" is an axis.
        /// Coordinate system is a set of axes of a dataset. Can be defined as a list of the variables names in any order.
        /// Example: "lat,lon,time", if variables lat,lon,time are axes of a dataset.
        /// Methods <see cref="GetValue(DataSet, string, object[])"/> allow to get value from a variable defined in a coordinate system
        /// from its coordinates instead of indices.</para>
        /// <para>
        /// DataSet has a default coordinate system which consists from all presented axes.
        /// If no dimensions or axes are given when adding new variable (<see cref="Add{T}(DataSet,string,string,object,T)"/>),
        /// default coordinate system is chosen, if appropriate.
        /// </para>
        /// </remarks>
        public static Variable AddAxis<T>(this DataSet ds, string name, string units, T data)
        {
            var var = ds.Add<T>(name, data, name);
            if (!String.IsNullOrEmpty(units))
                var.Metadata["Units"] = units;
            return var;
        }
        
        /// <summary>
        /// Creates new uniform axis variable of type float and adds it to the dataset.
        /// </summary>
        /// <param name="ds">Target datas set.</param>
        /// <param name="name">Name of the axis.</param>
        /// <param name="units">Units of measurement of values of the axis.</param>
        /// <param name="min">Starting axis value.</param>
        /// <param name="max">Final axis value.</param>
        /// <param name="delta">Step of values for the axis.</param>
        /// <returns>New variable.</returns>
        /// <remarks>
        /// <para>Adds new axis to the dataset, which is a 1d-variable depending on a dimension with name equal to the variable name, 
        /// and sets its "Units" attribute to given units string. 
        /// Values for the axis are uniform and are defined by a user using min,max and step values, so 
        /// that value[i] = min + step * i.</para>
        /// <para>If the dataset already contains a variable with given name, the method just returns that variable, 
        /// if it has proper type, rank and units. Values are not checked.
        /// </para>
        /// <example>
        /// <code>
        ///  DataSet ds = DataSet.Open("output.csv?openMode=create");
        ///  
        ///  ds.AddAxis("x", "m", 0, 10, 10);
        ///  ds.AddAxis("y", "m", 0.0f, 10.0f, 5.0f);
        ///
        ///  double[,] data = new[,] { { 1.0, 2.0, 3.0 }, { 11.0, 12.0, 13.0 } };
        ///  ds.Add("z", "unitz", 999.0, data); 
        /// </code>
        /// </example>
        /// <para>Axis is a 1d-variable with name equal to its dimension name. Example: variable "lat" depending on dimension "lat" is an axis.
        /// Coordinate system is a set of axes of a dataset. Can be defined as a list of the variables names in any order.
        /// Example: "lat,lon,time", if variables lat,lon,time are axes of a dataset.
        /// Methods <see cref="GetValue(DataSet, string, object[])"/> allow to get value from a variable defined in a coordinate system
        /// from its coordinates instead of indices.</para>
        /// <para>
        /// DataSet has a default coordinate system which consists from all presented axes.
        /// If no dimensions or axes are given when adding new variable (<see cref="Add{T}(DataSet,string,string,object,T)"/>),
        /// default coordinate system is chosen, if appropriate.
        /// </para>
        /// </remarks>
        public static Variable AddAxis(this DataSet ds, string name, string units, float min, float max, float delta)
        {
            if (ds == null) throw new ArgumentNullException("ds");
            if (String.IsNullOrEmpty(name)) throw new ArgumentException("Axis name is incorrect");

            int n = checked((int)((max - min) / delta)) + 1;
            if (ds.Variables.Contains(name))
            {
                AssertExistingAxis(ds, name, typeof(float), units, n);
                return ds.Variables[name];
            }

            if (n < 0) throw new ArgumentException("Number of elements is negative");
            float[] data = new float[n];
            for (int i = 0; i < n; i++)
                data[i] = min + i * delta;

            var var = ds.Add<float[]>(name, data, name);
            if (!String.IsNullOrEmpty(units))
                var.Metadata["Units"] = units;
            return var;
        }

       
        /// <summary>
        /// Creates new uniform axis variable of type <see cref="DateTime"/> and adds it to the dataset.
        /// </summary>
        /// <param name="ds">Target datas set.</param>
        /// <param name="name">Name of the axis.</param>
        /// <param name="min">Starting axis value.</param>
        /// <param name="max">Final axis value.</param>
        /// <param name="delta">Step of values for the axis.</param>
        /// <returns>New variable.</returns>
        /// <remarks>
        /// <para>Adds new axis to the dataset, which is a 1d-variable depending on a dimension with name equal to the variable name. 
        /// Values for the axis are uniform and are defined by a user using min,max and step values, so 
        /// that value[i] = min + step * i.</para>
        /// <para>If the dataset already contains a variable with given name, the method just returns that variable, 
        /// if it has proper type, rank and units. Values are not checked.
        /// </para>
        /// <example>
        /// <code>
        ///  DataSet ds = DataSet.Open("output.csv?openMode=create");
        ///  
        ///  ds.AddAxis("x", "m", 0, 10, 10);
        ///  ds.AddAxis("y", "m", new DateTime(2010,1,1), new DateTime(2010,2,1), TimeSpane.FromHours(1));
        ///
        ///  double[,] data = new[,] { { 1.0, 2.0, 3.0 }, { 11.0, 12.0, 13.0 } };
        ///  ds.Add("z", "unitz", 999.0, data); 
        /// </code>
        /// </example>
        /// <para>Axis is a 1d-variable with name equal to its dimension name. Example: variable "lat" depending on dimension "lat" is an axis.
        /// Coordinate system is a set of axes of a dataset. Can be defined as a list of the variables names in any order.
        /// Example: "lat,lon,time", if variables lat,lon,time are axes of a dataset.
        /// Methods <see cref="GetValue(DataSet, string, object[])"/> allow to get value from a variable defined in a coordinate system
        /// from its coordinates instead of indices.</para>
        /// <para>
        /// DataSet has a default coordinate system which consists from all presented axes.
        /// If no dimensions or axes are given when adding new variable (<see cref="Add{T}(DataSet,string,string,object,T)"/>),
        /// default coordinate system is chosen, if appropriate.
        /// </para>
        /// </remarks>
        public static Variable AddAxis(this DataSet ds, string name, DateTime min, DateTime max, TimeSpan delta)
        {
            if (ds == null) throw new ArgumentNullException("ds");
            if (String.IsNullOrEmpty(name)) throw new ArgumentException("Axis name is incorrect");

            int n = (int)((max - min).TotalSeconds / delta.TotalSeconds) + 1;
            if (ds.Variables.Contains(name))
            {
                AssertExistingAxis(ds, name, typeof(DateTime), "Date", n);
                return ds.Variables[name];
            }

            if (n < 0) throw new ArgumentException("Number of elements is negative");
            DateTime[] data = new DateTime[n];
            for (int i = 0; i < n; i++)
                data[i] = min.AddSeconds(i * delta.TotalSeconds);

            var var = ds.Add<DateTime[]>(name, data, name);
            return var;
        }
        /// <summary>
        /// Creates new variable with given initial data and adds it to the data set.
        /// </summary>
        /// <typeparam name="T">An array of variable rank and data type, or a type for scalar variable.</typeparam>
        /// <param name="ds">Target data set.</param>
        /// <param name="name">Name of the new variable.</param>
        /// <param name="units">Units attribute.</param>
        /// <param name="data">Initial data of the variable.</param>
        /// <param name="dimensions">Names of new variable's dimensions.</param>
        /// <returns>New variable.</returns>
        /// <remarks>
        /// <para>
        /// If <paramref name="dimensions"/> are absent, the 
        /// <typeparamref name="T"/> type parameter must be an array type of proper rank. 
        /// E.g. <c>ds.Add&lt;double[,]&gt;("mat", new double[,]{{1},{2}})</c> creates a variable named "mat" 
        /// of type double and of rank 2 in a default coordinate system. 
        /// If there is no default coordinate system, an exception is thrown.
        /// </para>
        /// <para>Axis is a 1d-variable with name equal to its dimension name. Example: variable "lat" depending on dimension "lat" is an axis.
        /// Coordinate system is a set of axes of a dataset. Can be defined as a list of the variables names in any order.
        /// Example: "lat,lon,time", if variables lat,lon,time are axes of a dataset.
        /// Methods <see cref="GetValue(DataSet, string, object[])"/> allow to get value from a variable defined in a coordinate system
        /// from its coordinates instead of indices.</para>
        /// <para>
        /// DataSet has a default coordinate system which consists from all presented axes.
        /// If no dimensions or axes are given when adding new variable (<see cref="Add{T}(DataSet,string,string,object,T)"/>),
        /// default coordinate system is chosen, if appropriate.
        /// </para>
        /// <para>
        /// It is still possible to specify type parameter and dimensions of the variable:
        /// <c>ds.Add&lt;double[,]&gt;("mat", new double[,]{{1},{2}}, "x", "y")</c> also creates a variable named "mat" 
        /// of type double and of rank 2. 
        /// The syntax <c>ds.Add&lt;double&gt;("mat", new double[,]{{1},{2}}, "x", "y")</c> 
        /// is incorrect, but <c>ds.Add&lt;double&gt;("mat", 10.0)</c> creates a scalar variable with value 10.
        /// </para>
        /// <para>
        /// If the <paramref name="data"/> parameter is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// </remarks>
        public static Variable Add<T>(this DataSet ds, string name, string units, T data, params string[] dimensions)
        {
            var var = ds.Add<T>(name, data, dimensions);
            if (!String.IsNullOrEmpty(units))
                var.Metadata["Units"] = units;
            return var;
        }
        /// <summary>
        /// Creates new variable with initial data and adds it to the data set.
        /// </summary>
        /// <typeparam name="T">An array of variable rank and data type, or a type for scalar variable.</typeparam>
        /// <param name="ds">Target data set.</param>
        /// <param name="name">Name of the new variable.</param>
        /// <param name="units">Units attribute.</param>
        /// <param name="missingValue">Missing value that is used within the data.</param>
        /// <param name="data">Initial data of the variable.</param>
        /// <param name="dimensions">Names of new variable's dimensions.</param>
        /// <returns>New variable.</returns>
        /// <remarks>
        /// <para>
        /// If <paramref name="dimensions"/> are absent, the 
        /// <typeparamref name="T"/> type parameter must be an array type of proper rank. 
        /// E.g. <c>ds.Add&lt;double[,]&gt;("mat", new double[,]{{1},{2}})</c> creates a variable named "mat" 
        /// of type double and of rank 2.
        /// </para>
        /// <para>
        /// It is still possible to specify type parameter and dimensions of the variable:
        /// <c>ds.Add&lt;double[,]&gt;("mat", new double[,]{{1},{2}}, "x", "y")</c> also creates a variable named "mat" 
        /// of type double and of rank 2. 
        /// The syntax <c>ds.Add&lt;double&gt;("mat", new double[,]{{1},{2}}, "x", "y")</c> 
        /// is incorrect, but <c>ds.Add&lt;double&gt;("mat", 10.0)</c> creates a scalar variable with value 10.
        /// </para>
        /// <para>
        /// If the <paramref name="data"/> parameter is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// </remarks>
        public static Variable Add<T>(this DataSet ds, string name, string units, object missingValue, T data, params string[] dimensions)
        {
            var var = ds.Add<T>(name, data, dimensions);
            if (!String.IsNullOrEmpty(units))
                var.Metadata["Units"] = units;
            var.MissingValue = missingValue;
            return var;
        }
        /// <summary>
        /// Creates new variable with initial data and adds it to the data set.
        /// </summary>
        /// <typeparam name="T">An array of variable rank and data type, or a type for scalar variable.</typeparam>
        /// <param name="ds">Target data set.</param>
        /// <param name="name">Name of the new variable.</param>
        /// <param name="units">Units attribute.</param>
        /// <param name="missingValue">Missing value of the variable.</param>
        /// <param name="data">Initial data of the variable.</param>
        /// <param name="axesIDs">Axes of the variable defining the coordinate system.</param>
        /// <returns>New variable.</returns>
        /// <remarks>
        /// <para>Axis is a 1d-variable with name equal to its dimension name. Example: variable "lat" depending on dimension "lat" is an axis.
        /// Coordinate system is a set of axes of a dataset. Can be defined as a list of the variables names in any order.
        /// Example: "lat,lon,time", if variables lat,lon,time are axes of a dataset.</para>
        /// <para>The method adds new variable with given units and missing value. 
        /// The variable will be defined in a coordinate system containing all given axes.</para>
        /// <para>
        /// If the <paramref name="data"/> parameter is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// </remarks>
        /// <seealso cref="AddAxis(DataSet, string , string , double , double , double )"/>
        public static Variable Add<T>(this DataSet ds, string name, string units, object missingValue, T data, params int[] axesIDs)
        {
            var var = Add<T>(ds, name, units, data, axesIDs);
            var.MissingValue = missingValue;
            return var;
        }

        /// <summary>
        /// Creates new variable with initial data in a default coordinate system and adds it to the data set.
        /// </summary>
        /// <typeparam name="T">An array of variable rank and data type, or a type for scalar variable.</typeparam>
        /// <param name="ds">Target data set.</param>
        /// <param name="name">Name of the new variable.</param>
        /// <param name="units">Units attribute.</param>
        /// <param name="data">Initial data of the variable.</param>
        /// <param name="missingValue">Missing value that is used within the data.</param>
        /// <returns>New variable.</returns>
        /// <remarks>
        /// <para>Axis is a 1d-variable with name equal to its dimension name. Example: variable "lat" depending on dimension "lat" is an axis.
        /// Coordinate system is a set of axes of a dataset. Can be defined as a list of the variables names in any order.
        /// Example: "lat,lon,time", if variables lat,lon,time are axes of a dataset.</para>
        /// <para>The method adds new variable with given units and missing value. 
        /// The variable will be defined in a default coordinate system, which is 
        /// a coordinate system consisting of all axes that already present in the DataSet (i.e. of 1d-variables whose name equal to their dimension's name).
        /// The order of axes in the coordinate system is found automatically, if possible, depending on a shape of the given 
        /// <paramref name="data"/> and lengths of the axes. Otherwise, if it is impossible to unamigiously identify the correspondence
        /// (including case when an array has at least two equal lengths along different dimensions), an exception will be thrown.
        /// </para>
        /// <example>
        /// <code>
        /// using (DataSet ds = DataSet.Open("data.nc?openMode=create"))
        /// {
        ///     ds.AddAxis("x", "1", new double[] { 1, 2 });
        ///     ds.AddAxis("y", "1", new double[] { 1, 2, 3 }); // default coordinate system is "x,y"
        ///     ds.Add("v", "kg", -1.0, new double[,] { { 1, 2, 3 }, { 1, 2, 3 } }); // "v" has shape 3x2 and hence is defined in "x,y"
        /// 
        ///     Console.WriteLine(ds);
        ///     // Prints:
        ///     // [3] v of type Double (x:2) (y:3)
        ///     // [2] y of type Double (y:3)
        ///     // [1] x of type Double (x:2)
        /// }
        /// </code>
        /// Compare with the following example, where variable "v" has shape 2x3 and hence depends on "y","x" instead of "x","y":
        /// <code>
        /// using (DataSet ds = DataSet.Open("data.nc?openMode=create"))
        /// {
        ///     ds.AddAxis("x", "1", new double[] { 1, 2, 3 });
        ///     ds.AddAxis("y", "1", new double[] { 1, 2 }); // default coordinate system is "x,y"
        ///     ds.Add("v", "kg", -1.0, new double[,] { { 1, 2, 3 }, { 1, 2, 3 } }); // "v" has shape 2x3 and hence is defined in "y,x"
        /// 
        ///     Console.WriteLine(ds);
        ///     // Prints:
        ///     // [3] v of type Double (y:2) (x:3)
        ///     // [2] y of type Double (y:2)
        ///     // [1] x of type Double (x:3)
        /// }
        /// </code>
        /// </example>
        /// <para>
        /// If the <paramref name="data"/> parameter is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// </remarks>
        /// <seealso cref="AddAxis(DataSet, string , string , double , double , double )"/>
        public static Variable Add<T>(this DataSet ds, string name, string units, object missingValue, T data)
        {
            return ds.Add<T>(name, units, missingValue, data, (string[])null);
        }

        /// <summary>
        /// Creates new variable with initial data and adds it to the data set.
        /// </summary>
        /// <typeparam name="T">An array of variable rank and data type, or a type for scalar variable.</typeparam>
        /// <param name="ds">Target data set.</param>
        /// <param name="name">Name of the new variable.</param>
        /// <param name="units">Units attribute.</param>
        /// <param name="data">Initial data of the variable.</param>
        /// <param name="axesIDs">Axes of the variable defining the coordinate system.</param>
        /// <returns>New variable.</returns>
        /// <remarks>
        /// <para>Axis is a 1d-variable with name equal to its dimension name. Example: variable "lat" depending on dimension "lat" is an axis.
        /// Coordinate system is a set of axes of a dataset. Can be defined as a list of the variables names in any order.
        /// Example: "lat,lon,time", if variables lat,lon,time are axes of a dataset.</para>
        /// <para>The method adds new variable with given units and missing value. 
        /// The variable will be defined in a coordinate system containing all given axes.</para>
        /// <para>
        /// If the <paramref name="data"/> parameter is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// </remarks>
        /// <seealso cref="AddAxis(DataSet, string , string , double , double , double )"/>
        public static Variable Add<T>(this DataSet ds, string name, string units, T data, params int[] axesIDs)
        {
            if (axesIDs == null || axesIDs.Length == 0)
            {
                if (typeof(Array).IsAssignableFrom(typeof(T))) // we've got non-scalar data w/o axes hence we're adding it into a default coordinate system
                    return ds.AddToDefaultCoordinateSystem(name, units, null, data);
                return ds.Add<T>(name, units, data, new string[0]);
            }

            if (!typeof(Array).IsAssignableFrom(typeof(T)))
                throw new ArgumentException("For the array a non-empty list of axes is given");
            int rank = typeof(T).GetArrayRank();
            if (rank != axesIDs.Length) throw new ArgumentException("Data is wrong for the given axes");

            foreach (var axisID in axesIDs)
            {
                var axis = ds.Variables.GetByID(axisID);
                if (axis.Rank != 1 || axis.Name != axis.Dimensions[0].Name)
                    throw new ArgumentException("One of the given variables is not an axis");
            }
            string[] indices = axesIDs.Select(a => ds.Variables.GetByID(a).Name).ToArray();
            return ds.Add<T>(name, units, data, indices);
        }
        /// <summary>
        /// Adds new variable using another one as a template for dimensions.
        /// </summary>
        /// <typeparam name="T">An array of variable rank and data type, or a type for scalar variable.</typeparam>
        /// <param name="ds">Target data set.</param>
        /// <param name="srcName">Name of the variable that is to be used as a template.</param>
        /// <param name="varName">Name of the new variable.</param>
        /// <param name="missingValue">Missing value of the variable.</param>
        /// <param name="units">Units attribute.</param>
        /// <param name="data">Initial data of the variable.</param>
        /// <returns>New variable.</returns>
        /// <remarks>
        /// <para>
        /// If the <paramref name="data"/> parameter is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// </remarks>
        public static Variable AddSimilar<T>(this DataSet ds, string srcName, string varName, string units, object missingValue, T data)
        {
            return ds.AddSimilar<T>(ds[srcName], varName, units, missingValue, data);
        }
        /// <summary>
        /// Adds new variable using another one as a template for dimensions.
        /// </summary>
        /// <typeparam name="T">An array of variable rank and data type, or a type for scalar variable.</typeparam>
        /// <param name="ds">Target data set.</param>
        /// <param name="srcVar">Name of the variable that is to be used as a template.</param>
        /// <param name="varName">Name of the new variable.</param>
        /// <param name="missingValue">Missing value of the variable.</param>
        /// <param name="units">Units attribute.</param>
        /// <param name="data">Initial data of the variable.</param>
        /// <returns>New variable.</returns>
        /// <remarks>
        /// <para>
        /// If the <paramref name="data"/> parameter is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// </remarks>
        public static Variable AddSimilar<T>(this DataSet ds, Variable srcVar, string varName, string units, object missingValue, T data)
        {
            string[] dims = srcVar.Dimensions.Select(p => p.Name).ToArray();
            return ds.Add(varName, units, missingValue, data, dims);
        }
        /// <summary>
        /// Adds new variable using another one as a template for dimensions.
        /// </summary>
        /// <typeparam name="T">An array of variable rank and data type, or a type for scalar variable.</typeparam>
        /// <param name="ds">Target data set.</param>
        /// <param name="srcName">Name of the variable that is to be used as a template.</param>
        /// <param name="varName">Name of the new variable.</param>
        /// <param name="units">Units attribute.</param>
        /// <param name="data">Initial data of the variable.</param>
        /// <returns>New variable.</returns>
        /// <remarks>
        /// <para>
        /// If the <paramref name="data"/> parameter is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// </remarks>
        public static Variable AddSimilar<T>(this DataSet ds, string srcName, string varName, string units, T data)
        {
            return ds.AddSimilar<T>(ds[srcName], varName, units, data);
        }
        /// <summary>
        /// Adds new variable using another one as a template for dimensions.
        /// </summary>
        /// <typeparam name="T">An array of variable rank and data type, or a type for scalar variable.</typeparam>
        /// <param name="ds">Target data set.</param>
        /// <param name="srcVar">Name of the variable that is to be used as a template.</param>
        /// <param name="varName">Name of the new variable.</param>
        /// <param name="units">Units attribute.</param>
        /// <param name="data">Initial data of the variable.</param>
        /// <returns>New variable.</returns>
        /// <remarks>
        /// <para>
        /// If the <paramref name="data"/> parameter is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// </remarks>
        public static Variable AddSimilar<T>(this DataSet ds, Variable srcVar, string varName, string units, T data)
        {
            string[] dims = srcVar.Dimensions.Select(p => p.Name).ToArray();
            return ds.Add(varName, units, data, dims);
        }

        private static Variable AddToDefaultCoordinateSystem<T>(this DataSet ds, string name, string units, object missingValue, T data)
        {
            var axes = ds.GetDefaultCoordinateSystem();
            int rank = axes.Length;
            if (rank == 0)
            {
                if (typeof(Array).IsAssignableFrom(typeof(T)))
                    throw new ArgumentException("There is no default coordinate system for the given array in the dataset");
            }
            else // rank >= 1
            {
                if (!typeof(Array).IsAssignableFrom(typeof(T)))
                    throw new ArgumentException("Cannot unambiguously identify a coordinate system of the array (given data is scalar)");
                Array array = (Array)(object)data;
                if (array.Rank != rank)
                    throw new ArgumentException("Cannot unambiguously identify a coordinate system of the array (given array and default coordinate system have different ranks)");
                int[] shape = axes.Select(a => a.GetShape()[0]).ToArray();
                if (shape.Distinct().Count() != shape.Length)
                    throw new ArgumentException("Cannot unambiguously identify a coordinate system of the array (some axes have equal lengths thus cannot determine an order of axes; specify axes explicitly)");
                int[] order = new int[rank];
                int[] arrayShape = Enumerable.Range(0, rank).Select(p => array.GetLength(p)).ToArray();
                for (int i = 0; i < rank; i++)
                {
                    order[i] = Array.IndexOf(arrayShape, shape[i]);
                    if (order[i] == -1)
                        throw new ArgumentException("Initial data has wrong length");
                }
                Array.Sort(order, axes);
            }
            return ds.Add(name, units, missingValue, data, axes.Select(p => p.ID).ToArray());
        }

        private static Variable[] GetDefaultCoordinateSystem(this DataSet ds)
        {
            return ds.Where(p => p.Rank == 1 && p.Name == p.Dimensions[0].Name).ToArray();
        }
        /// <summary>
        /// Returns a value from the variable for the given coordinates.
        /// </summary>
        /// <typeparam name="T">Type of result value (mast match data type of the variable).</typeparam>
        /// <param name="ds">Owner of the variable.</param>
        /// <param name="varName">Variable to get value from.</param>
        /// <param name="cs">Coordinate system definition (e.g. "lat,lon,time").</param>
        /// <param name="coords">Coordinate values in the order defined by <paramref name="cs"/>.</param>
        /// <returns>A value from the variable corresponding to the coordinates.</returns>
        /// <remarks>
        /// <para>Axis is a 1d-variable with name equal to its dimension name. Example: variable "lat" depending on dimension "lat" is an axis.
        /// Coordinate system is a set of axes of a dataset. Can be defined as a list of the variables names in any order. 
        /// It is defined by a string like "lat,lon,time", if variables lat,lon,time are axes of a dataset.
        /// </para>
        /// <para>The method gets nearest exact value for the given coordinates.</para>
        /// <para>If <typeparamref name="T"/> doesn't match data type of the variable, an exception is thrown.</para>
        /// <example>
        ///  DataSet ds = DataSet.Open("msds:memory");
        ///  ds.AddAxis("x", "unitx", 0.0, 3.0, 1.0);
        ///  ds.AddAxis("y", "unity", new float[] { 0f, 0.5f, 1f, 2f, 5f });
        ///  
        ///  var data = new[,] { { 0, 1, 2, 3, }, { 4, 5, 6, 7 }, { 8, 9, 10, 11 }, { 12, 13, 14, 15 }, { 16, 17, 18, 19 } };
        ///  ds.Add("z", "unitz", 999, data);
        ///  
        ///  // Explicitly specifying a coordinate system
        ///  int res = ds.GetValue&lt;int&gt;("z", "x,y", 1.6, 1.6f)); // 14
        ///  res = 14, ds.GetValue&lt;int&gt;("z", "y,x", 1.6f, 1.6)); // 14
        ///  res = 12, ds.GetValue&lt;int&gt;(ds["z"].ID, "x,y", 0.0001, 2.6f)); // 12
        ///  
        ///  // From the coordinate system of z (it is "y,x" since z is 5 x 3)
        ///  res = 14, ds.GetValue&lt;int&gt;("z", 1.6f, 1.6)); // 14
        ///  res = 14, ds.GetValue&lt;int&gt;(ds["z"].ID, 1.6f, 1.6)); // 14
        /// </example>
        /// </remarks>
        public static T GetValue<T>(this DataSet ds, string varName, string cs, params object[] coords)
        {
            return GetValue<T>(ds, ReverseIndexSelection.Nearest, varName, cs, coords);
        }
        /// <summary>
        /// Returns a value from the variable for the given coordinates.
        /// </summary>
        /// <typeparam name="T">Type of result value (mast match data type of the variable).</typeparam>
        /// <param name="ds">Owner of the variable.</param>
        /// <param name="varID">Variable to get value from.</param>
        /// <param name="cs">Coordinate system definition (e.g. "lat,lon,time").</param>
        /// <param name="coords">Coordinate values in the order defined by <paramref name="cs"/>.</param>
        /// <returns>A value from the variable corresponding to the coordinates.</returns>
        /// <remarks>
        /// <para>See remarks for <see cref="GetValue{T}(DataSet,string,string, object[])"/></para></remarks>
        public static T GetValue<T>(this DataSet ds, int varID, string cs, params object[] coords)
        {
            return GetValue<T>(ds, ReverseIndexSelection.Nearest, varID, cs, coords);
        }
        /// <summary>
        /// Returns a value from the variable for the given coordinates.
        /// </summary>
        /// <typeparam name="T">Type of result value (mast match data type of the variable).</typeparam>
        /// <param name="ds">Owner of the variable.</param>
        /// <param name="mode">Determines how to find out the value.</param>
        /// <param name="varName">Variable to get value from.</param>
        /// <param name="cs">Coordinate system definition (e.g. "lat,lon,time").</param>
        /// <param name="coords">Coordinate values in the order defined by <paramref name="cs"/>.</param>
        /// <returns>A value from the variable corresponding to the coordinates.</returns>
        /// <remarks>
        /// <para>See remarks for <see cref="GetValue{T}(DataSet,string,string, object[])"/></para></remarks>
        public static T GetValue<T>(this DataSet ds, ReverseIndexSelection mode, string varName, string cs, params object[] coords)
        {
            return GetValue<T>(ds, mode, FindVariable(ds, varName, typeof(T), coords.Length), cs, coords);
        }
        /// <summary>
        /// Returns a value from the variable for the given coordinates.
        /// </summary>
        /// <typeparam name="T">Type of result value (mast match data type of the variable).</typeparam>
        /// <param name="ds">Owner of the variable.</param>
        /// <param name="mode">Determines how to find out the value.</param>
        /// <param name="varID">Variable to get value from.</param>
        /// <param name="cs">Coordinate system definition (e.g. "lat,lon,time").</param>
        /// <param name="coords">Coordinate values in the order defined by <paramref name="cs"/>.</param>
        /// <returns>A value from the variable corresponding to the coordinates.</returns>
        /// <remarks>
        /// <para>See remarks for <see cref="GetValue{T}(DataSet,string,string, object[])"/></para></remarks>
        public static T GetValue<T>(this DataSet ds, ReverseIndexSelection mode, int varID, string cs, params object[] coords)
        {
            var var = ds.Variables.GetByID(varID);
            var varAxes = var.Dimensions.Select(d => d.Name).ToArray();
            var axNames = cs.Split(',');
            int n = axNames.Length;
            if (n != varAxes.Length) throw new ArgumentException("Number of axes in the coordinate system differs from the rank of the variable");
            
            // We should order coords as varAxes. Now they have order as axNames.
            int[] order = new int[n];
            for (int i = 0; i < n; i++)
            {
                order[i] = Array.IndexOf(varAxes, axNames[i]);
                if (order[i] < 0) throw new ArgumentException("The variable doesn't depend on at least one of axes of the given coordinate system");
            }
            Array.Sort(order, coords);
            return GetValue<T>(ds, mode, varID, coords);
        }
        /// <summary>
        /// Returns a value from the variable for the given coordinates in a coordinate system of the variable.
        /// </summary>
        /// <typeparam name="T">Type of result value (mast match data type of the variable).</typeparam>
        /// <param name="ds">Owner of the variable.</param>
        /// <param name="varName">Variable to get value from.</param>
        /// <param name="coords">Coordinate values in the order defined by the dimensions of the variable.</param>
        /// <returns>A value from the variable corresponding to the coordinates.</returns>
        /// <remarks>
        /// <para>See remarks for <see cref="GetValue{T}(DataSet,string,string, object[])"/></para></remarks>
        public static T GetValue<T>(this DataSet ds, string varName, params object[] coords)
        {
            return ds.GetValue<T>(FindVariable(ds, varName, typeof(T), coords.Length), coords);
        }
        /// <summary>
        /// Returns a value from the variable for the given coordinates in a coordinate system of the variable.
        /// </summary>
        /// <typeparam name="T">Type of result value (mast match data type of the variable).</typeparam>
        /// <param name="ds">Owner of the variable.</param>
        /// <param name="varID">Variable to get value from.</param>
        /// <param name="coords">Coordinate values in the order defined by the dimensions of the variable.</param>
        /// <returns>A value from the variable corresponding to the coordinates.</returns>
        /// <remarks>
        /// <para>See remarks for <see cref="GetValue{T}(DataSet,string,string, object[])"/></para></remarks>
        public static T GetValue<T>(this DataSet ds, int varID, params object[] coords)
        {
            var var = ds.Variables.GetByID(varID);
            var axes = var.Dimensions.Select(d => ds[d.Name]).ToArray();
            return GetValue<T>(ReverseIndexSelection.Nearest, var, axes, coords);
        }
        /// <summary>
        /// Returns a value from the variable for the given coordinates in a coordinate system of the variable.
        /// </summary>
        /// <typeparam name="T">Type of result value (mast match data type of the variable).</typeparam>
        /// <param name="ds">Owner of the variable.</param>
        /// <param name="mode">Determines how to find out the value.</param>
        /// <param name="varName">Variable to get value from.</param>
        /// <param name="coords">Coordinate values in the order defined by the dimensions of the variable.</param>
        /// <returns>A value from the variable corresponding to the coordinates.</returns>
        /// <remarks>
        /// <para>See remarks for <see cref="GetValue{T}(DataSet,string,string, object[])"/></para></remarks>
        public static T GetValue<T>(this DataSet ds, ReverseIndexSelection mode, string varName, params object[] coords)
        {
            return ds.GetValue<T>(mode, FindVariable(ds, varName, typeof(T), coords.Length), coords);
        }
        /// <summary>
        /// Returns a value from the variable for the given coordinates in a coordinate system of the variable.
        /// </summary>
        /// <typeparam name="T">Type of result value (mast match data type of the variable).</typeparam>
        /// <param name="ds">Owner of the variable.</param>
        /// <param name="mode">Determines how to find out the value.</param>
        /// <param name="varID">Variable to get value from.</param>
        /// <param name="coords">Coordinate values in the order defined by the dimensions of the variable.</param>
        /// <returns>A value from the variable corresponding to the coordinates.</returns>
        /// <remarks>
        /// <para>See remarks for <see cref="GetValue{T}(DataSet,string,string, object[])"/></para></remarks>
        public static T GetValue<T>(this DataSet ds, ReverseIndexSelection mode, int varID, params object[] coords)
        {
            var var = ds.Variables.GetByID(varID);
            var axes = var.Dimensions.Select(d => ds[d.Name]).ToArray();
            return GetValue<T>(mode, var, axes, coords);
        }

        private static T GetValue<T>(ReverseIndexSelection mode, Variable data, Variable[] axes, object[] coords)
        {
            if (data.TypeOfData != typeof(T))
                throw new ArgumentException("Variable data has type different than the result type");
            if (data.Rank != axes.Length)
                throw new ArgumentException("Wrong coordinate vector");
            if (data.Rank != coords.Length)
                throw new ArgumentException("Wrong coordinate vector");

            foreach (var axis in axes)
            {
                if (axis.Rank != 1)
                    throw new NotSupportedException("One of given axes is incorrect");
            }

            if (axes.Length == 1)
            {
                Variable axis = axes[0];
                return GetValue1d<T>(mode, (Variable<T>)data, axis, coords[0]);
            }
            else
            {
                int rank = data.Rank;
                var dimensions = data.Dimensions.Select(p => p.Name).ToArray();
                GetValueIndex[] indices = new GetValueIndex[coords.Length];
                bool interpolation = false;

                // Each axis gives an index (or a pair of nearest indices) for its coordinate value
                var tdata = (Variable<T>)data;
                for (int a = 0; a < axes.Length; a++)
                {
                    Variable axis = axes[a];
                    int i = Array.IndexOf(dimensions, axis.Dimensions[0].Name);
                    object coordValue = coords[a];

                    indices[i] = GetIndex1d(mode, tdata, axis, coordValue);
                    interpolation = interpolation || indices[i].Interpolated;
                }

                // Either exact or nearest indices found (no interpolation, just get and return)
                if (!interpolation)
                {
                    int[] getIndex = new int[rank];
                    int[] shape = new int[rank];
                    for (int i = 0; i < shape.Length; i++)
                    {
                        shape[i] = 1;
                        getIndex[i] = indices[i].IndexLeft;
                    }

                    return (T)data.GetData(getIndex, shape).GetValue(new int[shape.Length]);
                }
                else // Making interpolation
                {
                    int m = indices.Length;
                    int n = 1 << m;
                    bool[] flags = new bool[m]; // initialized with false

                    // Building a "hypercube" of data to be interpolated
                    int[] getIndex = new int[rank];
                    int[] shape = new int[rank];
                    for (int i = 0; i < shape.Length; i++)
                    {
                        getIndex[i] = indices[i].IndexLeft;
                        shape[i] = indices[i].Interpolated ? 2 : 1; // the dimension might not require the interpolation
                    }
                    int[] zero = new int[shape.Length];

                    // Getting the data that we need to proceed
                    Array hyperCube = data.GetData(getIndex, shape);

                    // Going through all points and computing value at each point with specified 
                    // interpolation coefficient and accumulating a sum.
                    double total = 0.0;
                    for (int i = 0; i < n; i++)
                    {
                        // Computing an interpolation coefficient for the current point
                        double coeff = 1.0;
                        for (int j = 0; j < m; j++)
                        {
                            coeff *= (flags[j]) ? (1.0 - indices[j].Alpha) : indices[j].Alpha;
                            if (coeff == 0) break;
                        }

                        // If coeff == 0 this point is not important for us and it may be skipped
                        if (coeff != 0)
                        {
                            for (int j = 0; j < m; j++)
                                getIndex[j] = flags[j] ? 1 : 0;
                            double val = coeff * Convert.ToDouble(hyperCube.GetValue(getIndex));
                            total += val;
                        }

                        // Next point
                        if (i < n - 1)
                            for (int k = 0; k < m; k++)
                            {
                                if (flags[k])
                                {
                                    flags[k] = false;
                                }
                                else
                                {
                                    flags[k] = true;
                                    break;
                                }
                            }
                    }
                    return (T)Convert.ChangeType(total, typeof(T));
                }
            }

            throw new NotSupportedException("Operation is not supported for this particular kind of a coordinate system.");
        }

        /// <summary>
        /// Returns value in the given axis for specified coordinate value.
        /// </summary>
        private static DataType GetValue1d<DataType>(ReverseIndexSelection mode, Variable<DataType> var, Variable axis, object coordValue)
        {
            int index = IndicesOf(axis, coordValue)[0];

            if (index >= 0) return var[index];

            if (mode == ReverseIndexSelection.Exact)
                throw new ValueNotFoundException();

            Type dataType = var.TypeOfData;
            Type axisDataType = axis.TypeOfData;

            if (mode == ReverseIndexSelection.Interpolation && TypeUtils.IsRealNumber(dataType) && TypeUtils.IsRealNumber(axisDataType))
            {
                index = ~index;
                int size = axis.GetShape()[0];
                if (index > 0 && index < size)
                {
                    // Performing interpolation
                    DataType[] data = (DataType[])var.GetData(new int[1] { index - 1 }, new int[1] { 2 });
                    Array grid = axis.GetData(new int[1] { index - 1 }, new int[1] { 2 });
                    return (DataType)Convert.ChangeType(Interpolation.Interpolate(coordValue, grid.GetValue(0), grid.GetValue(1), data[0], data[1]),
                        typeof(DataType));
                }
            }
            else if (mode == ReverseIndexSelection.Nearest &&
                (TypeUtils.IsNumeric(axisDataType) || TypeUtils.IsDateTime(axisDataType)))
            {
                index = ~index;
                int size = axis.GetShape()[0];
                if (index >= size)
                {
                    index = size - 1;
                }
                else if (index > 0)
                {
                    Array grid = axis.GetData(new int[1] { index - 1 }, new int[1] { 2 });
                    if (TypeUtils.IsNumeric(axisDataType))
                    {
                        double d1 = Convert.ToDouble(grid.GetValue(0)) - Convert.ToDouble(coordValue);
                        double d2 = Convert.ToDouble(coordValue) - Convert.ToDouble(grid.GetValue(1));
                        if (Math.Abs((double)d1) < Math.Abs((double)d2))
                            index--;
                    }
                    else
                    {
                        TimeSpan d1 = ((DateTime)grid.GetValue(0)).Subtract((DateTime)coordValue);
                        TimeSpan d2 = ((DateTime)coordValue).Subtract((DateTime)grid.GetValue(1));
                        if (Math.Abs(d1.Ticks) < Math.Abs(d2.Ticks))
                            index--;
                    }
                }
                return var[index];
            }
            throw new ValueNotFoundException("The selection mode is not supported because of the data or axis variable's type of data");
        }

        /// <summary>
        /// Returns index (or two nearest indices) in the given axis for specified coordinate value.
        /// </summary>
        private static GetValueIndex GetIndex1d<T>(ReverseIndexSelection mode, Variable<T> data, Variable axis, object coordValue)
        {
            int index = IndicesOf(axis, coordValue)[0];

            if (index >= 0)
                return new GetValueIndex(index);

            if (mode == ReverseIndexSelection.Exact)
                throw new ValueNotFoundException();

            Type dataType = data.TypeOfData;
            Type axisDataType = axis.TypeOfData;

            if (mode == ReverseIndexSelection.Interpolation && TypeUtils.IsRealNumber(dataType) && TypeUtils.IsRealNumber(axisDataType))
            {
                index = ~index;
                int size = axis.GetShape()[0];
                if (index > 0 && index < size)
                {
                    // Performing interpolation
                    Array grid = axis.GetData(new int[1] { index - 1 }, new int[1] { 2 });
                    double alpha = (Convert.ToDouble(grid.GetValue(1)) - Convert.ToDouble(coordValue)) / (Convert.ToDouble(grid.GetValue(1)) - Convert.ToDouble(grid.GetValue(0)));
                    return new GetValueIndex(index - 1, index, alpha);
                }
            }
            else if (mode == ReverseIndexSelection.Nearest &&
                (TypeUtils.IsNumeric(axisDataType) || TypeUtils.IsDateTime(axisDataType)))
            {
                index = ~index;
                int size = axis.GetShape()[0];
                if (index >= size)
                {
                    index = size - 1;
                }
                else if (index > 0)
                {
                    Array grid = axis.GetData(new int[1] { index - 1 }, new int[1] { 2 });

                    if (TypeUtils.IsNumeric(axisDataType))
                    {
                        double d1 = Convert.ToDouble(grid.GetValue(0)) - Convert.ToDouble(coordValue);
                        double d2 = Convert.ToDouble(coordValue) - Convert.ToDouble(grid.GetValue(1));

                        if (Math.Abs((double)d1) < Math.Abs((double)d2))
                            index--;
                    }
                    else
                    {
                        TimeSpan d1 = ((DateTime)grid.GetValue(0)).Subtract((DateTime)coordValue);
                        TimeSpan d2 = ((DateTime)coordValue).Subtract((DateTime)grid.GetValue(1));

                        if (Math.Abs(d1.Ticks) < Math.Abs(d2.Ticks))
                            index--;
                    }
                }
                return new GetValueIndex(index);
            }
            throw new ValueNotFoundException("The selection mode is not supported since axis has data type " + axisDataType.Name);
        }

        private static int[] IndicesOf(Variable v, object value)
        {
            if (v.Rank != 1)
                throw new NotSupportedException("Variable with rank 1 are supported only");

            var data = v.GetData();

            IComparer cmp = Comparer.Default;
            ArrayOrder order = (data.Length <= 1 || cmp.Compare(data.GetValue(1), data.GetValue(0)) > 0) ? ArrayOrder.Ascendant : ArrayOrder.Descendant;
            if (order == ArrayOrder.Descendant) cmp = new DescendantComparer(cmp);
            if (value != null && value is int) // so it is possible to write just "10" instead of "10.0"
                value = Convert.ChangeType(value, v.TypeOfData);
            int index = Array.BinarySearch(data, value, cmp);
            return new int[] { index };
        }

        private class DescendantComparer<DataType> : IComparer<DataType>
        {
            private IComparer<DataType> cmp;

            public DescendantComparer()
            {
                cmp = Comparer<DataType>.Default;
            }

            public DescendantComparer(IComparer<DataType> cmp)
            {
                this.cmp = cmp;
            }

            #region IComparer<DataType> Members

            public int Compare(DataType x, DataType y)
            {
                return cmp.Compare(y, x);
            }

            #endregion
        }

        private class DescendantComparer : IComparer
        {
            private IComparer cmp;

            public DescendantComparer()
            {
                cmp = Comparer.Default;
            }

            public DescendantComparer(IComparer cmp)
            {
                this.cmp = cmp;
            }

            #region IComparer<DataType> Members

            public int Compare(object x, object y)
            {
                return cmp.Compare(y, x);
            }

            #endregion
        }

        private static class Interpolation
        {
            public static double Interpolate(double x, double x1, double x2, double a, double b)
            {
                return a + (b - a) * (x - x1) / (x2 - x1);
            }

            public static object Interpolate(object x, object x1, object x2, object a, object b)
            {
                return (double)a + ((double)b - (double)a) * ((double)x - (double)x1) / ((double)x2 - (double)x1);
            }
        }

        /// <summary>
        /// Stores a pair of indices and interpolation coefficient.
        /// </summary>
        private struct GetValueIndex
        {
            public int IndexLeft;
            public int IndexRight;
            public double Alpha;

            public GetValueIndex(int left, int right, double alpha)
            {
                this.IndexLeft = left;
                this.IndexRight = right;
                this.Alpha = alpha;
            }

            public GetValueIndex(int index)
                : this(index, index + 1, 1.0)
            {
            }

            public bool Interpolated { get { return Alpha != 1.0; } }
        }
    }
}
