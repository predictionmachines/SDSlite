// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data.Imperative
{
    /// <summary>
    /// Provides a set of extensions methods enabling work with <see cref="DataSet"/> in a way close
    /// to a procedural API.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The goal of the extensions is to enable simple data operations with minimum amount of code. 
    /// Additionally, it unifies the API with other languages like C, R, Python etc. 
    /// Note that these are just helper methods which will in fact call universal object model API.
    /// </para>
    /// </remarks>
    /// <seealso cref="DataSet"/>
    public static partial class DataSetExtensions
    {
        #region Add
        /// <summary>
        /// Creates new variable with initial data and adds it to the data set.
        /// </summary>
        /// <typeparam name="D">An array of variable rank and data type, or a type for scalar variable.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="variableName">Name of the new variable.</param>
        /// <param name="data">Initial data of the variable.</param>
        /// <param name="dimensionNames">Names of new variable's dimensions.</param>
        /// <returns>New variable.</returns>
        /// <remarks>
        /// <para>
        /// If <paramref name="dimensionNames"/> are absent, the 
        /// <typeparamref name="D"/> type parameter must be an array type of proper rank. 
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
        public static Variable Add<D>(this DataSet dataset, string variableName, D data, params string[] dimensionNames)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");
            if (typeof(Array).IsAssignableFrom(typeof(D)))
            {
                int rank = typeof(D).GetArrayRank();
                if (dimensionNames == null || dimensionNames.Length == 0)  // we've got non-scalar data w/o axes hence we're adding it into a default coordinate system
                {
                    return dataset.AddToDefaultCoordinateSystem(variableName, null, null, data);
                }
                if (dimensionNames.Length != rank)
                    throw new ArgumentException("The number of dimensionNames must match the rank of D");

                //if (dimensionNames == null || dimensionNames.Length == 0)
                //{
                //    Array array = (Array)(object)data;
                //    if (array == null)
                //    {
                //        dimensionNames = new string[rank];
                //        DataSet.GetAutoDimensions(dimensionNames);
                //        return dataset.AddVariable(typeof(D).GetElementType(), variableName, null, dimensionNames);
                //    }
                //}
                return dataset.AddVariable(typeof(D).GetElementType(), variableName, (Array)(object)data, dimensionNames);
            }
            else
            {
                if (dimensionNames == null || dimensionNames.Length == 0)
                {
                    // scalar variable
                    Variable var = dataset.AddVariable(typeof(D), variableName, null);
                    if ((object)data != null)
                        var.PutData(new D[] { data });
                    return var;
                }
                else
                    throw new ArgumentException("Scalar variable has no dimensions");
            }
        }

        /// <summary>
        /// Creates new variable and adds it to the data set.
        /// </summary>
        /// <typeparam name="D">A variable data type or an array of variable rank and data type.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="variableName">Name of the new variable.</param>
        /// <param name="dimensionNames">Names of new variable's dimensions.</param>
        /// <returns>New variable.</returns>
        /// <remarks>
        /// <para>
        /// If <paramref name="dimensionNames"/> are absent, the 
        /// <typeparamref name="D"/> type parameter must be an array type of proper rank. 
        /// E.g. <c>ds.Add&lt;double[,]&gt;("mat")</c> creates a variable named "mat" 
        /// of type double and of rank 2.
        /// </para>
        /// <para>
        /// It is still possible to specify just type parameter and dimensions of the variable:
        /// <c>ds.Add&lt;double&gt;("mat", "x", "y")</c> also creates a variable named "mat" 
        /// of type double and of rank 2. The syntax <c>ds.Add&lt;double[,]&gt;("mat", "x", "y")</c> 
        /// is correct, too.
        /// </para>
        /// <example>
        /// <code>
        /// // Creates new data set:
        /// var ds = DataSet.Open("test.csv?openMode=create");
        /// // Adds new string scalar variable with name "str" to the data set:
        /// ds.Add&lt;string&gt;("str"); 
        /// // Data of the variable "str" is "data string":
        /// ds.PutData("str", "data string");
        /// // Adds new variable with name "int1" that is a 1d-array of int and depends on dimension "idx".
        /// // id2 contains unique identifier of the new variable.
        /// int id2 = ds.Add&lt;int[]&gt;("int1", "idx").ID;
        /// // Data of the variable "int1" is { 9, 8, 7, 6 }:
        /// ds.PutData(id2, new int[] { 9, 8, 7, 6 });
        /// // Adds new variable "double2" depending on two dimensions:
        /// ds.Add&lt;double&gt;("double2", "i1", "i2");
        /// // Adds new variable "double3" depending on 3 default dimensions:
        /// ds.Add&lt;double[, ,]&gt;("double3");
        /// // Prints the data set brief description:
        /// Console.WriteLine(ds);
        /// </code>
        /// </example>
        /// </remarks>
        public static Variable Add<D>(this DataSet dataset, string variableName, params string[] dimensionNames)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");
            if (typeof(Array).IsAssignableFrom(typeof(D)))
            {
                int rank = typeof(D).GetArrayRank();
                if (dimensionNames != null && dimensionNames.Length > 0 && dimensionNames.Length != rank)
                    throw new ArgumentException("The number of dimensionNames must match the rank of D");
                if (dimensionNames == null || dimensionNames.Length == 0)
                {
                    dimensionNames = new string[rank];
                    DataSet.GetAutoDimensions(dimensionNames);
                }
                return dataset.AddVariable(typeof(D).GetElementType(), variableName, null, dimensionNames);
            }
            else
            {
                if (dimensionNames == null || dimensionNames.Length == 0)
                {
                    // scalar variable
                    return dataset.AddVariable(typeof(D), variableName, null);
                }
                else
                {
                    // rank is determined by dimensionNames
                    return dataset.AddVariable(typeof(D), variableName, null, dimensionNames);
                }
            }
        }

        private static int FindVariable(DataSet dataset, Func<Variable, bool> predicate)
        {
            var found = dataset.Where(predicate).ToArray();
            if (found.Length == 0)
                throw new InvalidOperationException("Requested variable does not exist in the data set");
            else if (found.Length > 1)
                throw new InvalidOperationException("Cannot unambiguously identify a variable in the data set");
            return found[0].ID;
        }

        private static int FindVariable(DataSet dataset, string name, Type dataType, int rank)
        {
            return FindVariable(dataset,
                    variable => variable.Name == name &&
                    variable.TypeOfData == dataType &&
                    variable.Rank == rank);
         }

        #endregion

        #region Put

        #region Put all
        /// <summary>
        /// Replaces all the variable data with a new one.
        /// </summary>
        /// <typeparam name="D">Must be a scalar type for a variable of rank 0 and an array type for other ranks.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="variableId"><see cref="Variable.ID"/> of the variable to put data in.</param>
        /// <param name="data">New data of the variable.</param>
        /// <remarks>
        /// <para>The shape of the data must be greater or equal to the current shape of the variable.</para>
        /// <example>
        /// <code>
        /// // Creates new data set:
        /// var ds = DataSet.Open("test.csv?openMode=create");
        /// // Adds new string scalar variable with name "str" to the data set:
        /// ds.Add&lt;string&gt;("str"); 
        /// // Data of the variable "str" is "data string":
        /// ds.PutData("str", "data string");
        /// // Adds new variable with name "int1" that is a 1d-array of int and depends on dimension "idx".
        /// // id2 contains unique identifier of the new variable.
        /// int id2 = ds.Add&lt;int[]&gt;("int1", "idx").ID;
        /// // Data of the variable "int1" is { 9, 8, 7, 6 }:
        /// ds.PutData(id2, new int[] { 9, 8, 7, 6 });
        /// // Adds new variable "double2" depending on two dimensions:
        /// ds.Add&lt;double&gt;("double2", "i1", "i2");
        /// // Adds new variable "double3" depending on 3 default dimensions:
        /// ds.Add&lt;double[, ,]&gt;("double3");
        /// // Prints the data set brief description:
        /// Console.WriteLine(ds);
        /// </code>
        /// </example>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">DataSet is disposed.</exception>
        /// <exception cref="ReadOnlyException">Variable is read-only.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">Arguments are incorrect.</exception>
        /// <exception cref="CannotPerformActionException">Cannot put data into the variable.</exception>
        /// <exception cref="KeyNotFoundException">Variable is not found.</exception>
        /// <seealso cref="PutData{D}(DataSet,string,D)"/>
        /// <seealso cref="PutData{D}(DataSet,Func{Variable,bool},D)"/>	
        public static void PutData<D>(this DataSet dataset, int variableId, D data)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");
            Array dataArray;
            if (typeof(Array).IsAssignableFrom(typeof(D)))
            {
                dataArray = (Array)(object)data;
            }
            else
            {
                dataArray = Array.CreateInstance(typeof(D), 1);
                dataArray.SetValue(data, 0);
            }
            dataset.Variables.GetByID(variableId).PutData(dataArray);
        }

        /// <summary>
        /// Replaces all the variable data with a new one.
        /// </summary>
        /// <typeparam name="D">Must be a scalar type for a variable of rank 0 and an array type for other ranks.</typeparam>
        /// <param name="dataset">Target DataSet.</param>
        /// <param name="predicate">Determines the variable to put data in.</param>
        /// <param name="data">New data of the variable.</param>
        /// <remarks>
        /// <para>The shape of the data must be greater or equal to the current shape of the variable.</para>
        /// <para><paramref name="predicate"/> must select only one variable within the DataSet.
        /// Otherwise, an exception is thrown.</para>
        /// <para>
        /// If the <paramref name="data"/> parameter is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// <example>
        /// <code>
        /// // Opens a data set:
        /// var ds = DataSet.Open("test.csv?openMode=open");		
        /// // Updates a variable that satisfies the predicate:
        /// ds.PutData(v => v.Metadata["action"] == "toUpdate", 
        ///				new int[] { 9, 8, 7, 6 });
        /// </code>
        /// </example>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Cannot unambiguously identify a variable in the data set.
        /// </exception>
        /// <exception cref="ObjectDisposedException">DataSet is disposed.</exception>
        /// <exception cref="ReadOnlyException">Variable is read-only.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">Arguments are incorrect.</exception>
        /// <exception cref="CannotPerformActionException">Cannot put data into the variable.</exception>
        /// <seealso cref="PutData{D}(DataSet,int,D)"/>
        /// <seealso cref="PutData{D}(DataSet,string,D)"/>			
        public static void PutData<D>(this DataSet dataset, Func<Variable, bool> predicate, D data)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");
            PutData<D>(dataset, FindVariable(dataset, predicate), data);
        }

        /// <summary>
        /// Replaces all the variable data with a new one.
        /// </summary>
        /// <typeparam name="D">Must be a scalar type for a variable of rank 0 and an array type for other ranks.</typeparam>
        /// <param name="dataset">Target DataSet.</param>
        /// <param name="variableName"><see cref="Variable.Name"/> of the variable to put data in.</param>
        /// <param name="data">New data of the variable.</param>
        /// <remarks>
        /// <para>The shape of the data must be greater or equal to the current shape of the variable.</para>
        /// <para>The method finds a variable with name <paramref name="variableName"/>, given rank and
        /// type of data (determined from the arguments) and puts the data into it.
        /// If a variable is not found or there are several variables with the name, an exception is thrown.</para>
        /// <para>
        /// If the <paramref name="data"/> parameter is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// <example>
        /// <code>
        /// // Opens a data set:
        /// var ds = DataSet.Open("test.csv?openMode=open");		
        /// // Updates a one-dimensional variable with name "int1" and type of data int:
        /// ds.PutData("int1", new int[] { 9, 8, 7, 6 });
        /// </code>
        /// </example>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Cannot unambiguously identify a variable in the data set.
        /// </exception>
        /// <exception cref="ObjectDisposedException">DataSet is disposed.</exception>
        /// <exception cref="ReadOnlyException">Variable is read-only.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">Arguments are incorrect.</exception>
        /// <exception cref="CannotPerformActionException">Cannot put data into the variable.</exception>
        /// <seealso cref="PutData{D}(DataSet,int,D)"/>
        /// <seealso cref="PutData{D}(DataSet,Func{Variable,bool},D)"/>		
        public static void PutData<D>(this DataSet dataset, string variableName, D data)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");
            int rank;
            Type dataType;
            if (typeof(Array).IsAssignableFrom(typeof(D)))
            {
                rank = typeof(D).GetArrayRank();
                dataType = typeof(D).GetElementType();
            }
            else
            {
                rank = 0;
                dataType = typeof(D);
            }
            PutData<D>(dataset,
                variable => variable.Name == variableName && variable.TypeOfData == dataType && variable.Rank == rank,
                data);
        }
        #endregion

        #region Put single value

        /// <summary>
        /// Sets one value in the variable data.
        /// </summary>
        /// <typeparam name="D">Must be a scalar type.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="variableId"><see cref="Variable.ID"/> of the variable to put data in.</param>
        /// <param name="data">New value.</param>
        /// <param name="indices">Indices of the value to put.</param>
        /// <remarks>
        /// <example>
        /// <code>
        /// // Creates new data set:
        /// var ds = DataSet.Open("test.csv?openMode=create");
        /// // Adds new string scalar variable with name "str" to the data set:
        /// ds.Add&lt;string&gt;("str"); 
        /// // Data of the variable "str" is "data string":
        /// ds.PutData("str", "data string");
        /// // Adds new variable with name "int1" that is a 1d-array of int and depends on dimension "idx".
        /// // id2 contains unique identifier of the new variable.
        /// int id2 = ds.Add&lt;int[]&gt;("int1", "idx").ID;
        /// // Sets first element of the id2 to 7:
        /// ds.PutData(id2, 7, 0);		
        /// </code>
        /// </example>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">DataSet is disposed.</exception>
        /// <exception cref="ReadOnlyException">Variable is read-only.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">Arguments are incorrect.</exception>
        /// <exception cref="CannotPerformActionException">Cannot put data into the variable.</exception>
        /// <exception cref="KeyNotFoundException">Variable is not found.</exception>
        /// <seealso cref="PutData{D}(DataSet,string,D)"/>
        /// <seealso cref="PutData{D}(DataSet,Func{Variable,bool},D)"/>	
        public static void PutData<D>(this DataSet dataset, int variableId, D data, params int[] indices)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");
            if (typeof(Array).IsAssignableFrom(typeof(D)))
                throw new ArgumentException("D must be a scalar type");

            var var = dataset.Variables.GetByID(variableId);
            if (indices == null || indices.Length == 0)
            {
                if (var.Rank != 0)
                    throw new ArgumentException("For a scalar variable indices must be either null or int[0]", "indices");
                var.PutData(new D[1] { data });
                return;
            }

            if (var.Rank != indices.Length)
                throw new ArgumentException("Length of indices differs from the variable rank", "indices");
            int r = var.Rank;
            int[] shape = new int[r];
            for (int i = 0; i < r; i++)
                shape[i] = 1;
            Array arr = Array.CreateInstance(typeof(D), shape);
            for (int i = 0; i < r; i++)
                shape[i] = 0;
            arr.SetValue(data, shape);
            var.PutData(indices, arr);
        }

        /// <summary>
        /// Sets one value in the variable data.
        /// </summary>
        /// <typeparam name="D">Must be a scalar type.</typeparam>
        /// <param name="dataset">Target DataSet.</param>
        /// <param name="predicate">Determines the variable to put data in.</param>
        /// <param name="data">New value.</param>
        /// <param name="indices">Indices of the value to put.</param>
        /// <remarks>
        /// <para><paramref name="predicate"/> must select only one variable within the DataSet.
        /// Otherwise, an exception is thrown.</para>	
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Cannot unambiguously identify a variable in the data set.
        /// </exception>
        /// <exception cref="ObjectDisposedException">DataSet is disposed.</exception>
        /// <exception cref="ReadOnlyException">Variable is read-only.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">Arguments are incorrect.</exception>
        /// <exception cref="CannotPerformActionException">Cannot put data into the variable.</exception>
        /// <seealso cref="PutData{D}(DataSet,int,D)"/>
        /// <seealso cref="PutData{D}(DataSet,string,D)"/>			
        public static void PutData<D>(this DataSet dataset, Func<Variable, bool> predicate, D data, params int[] indices)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");
            PutData<D>(dataset, FindVariable(dataset, predicate), data, indices);
        }

        /// <summary>
        /// Sets one value in the variable data.
        /// </summary>
        /// <typeparam name="D">Must be a scalar type.</typeparam>
        /// <param name="dataset">Target DataSet.</param>
        /// <param name="variableName"><see cref="Variable.Name"/> of the variable to put data in.</param>
        /// <param name="data">New value.</param>
        /// <param name="indices">Indices of the value to put.</param>
        /// <remarks>
        /// <para>The method finds a variable with name <paramref name="variableName"/>, given rank and
        /// type of data (determined from the arguments) and put the data into it.
        /// If a variable is not found or there are several variables with the name, an exception is thrown.</para>
        /// <example>
        /// <code>
        /// // Opens a data set:
        /// var ds = DataSet.Open("test.csv?openMode=open");		
        /// // Updates a one-dimensional variable with name "int1" and type of data int and
        /// // sets its first element value to 7:
        /// ds.PutData("int1", 7, 0);
        /// </code>
        /// </example>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Cannot unambiguously identify a variable in the data set.
        /// </exception>
        /// <exception cref="ObjectDisposedException">DataSet is disposed.</exception>
        /// <exception cref="ReadOnlyException">Variable is read-only.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">Arguments are incorrect.</exception>
        /// <exception cref="CannotPerformActionException">Cannot put data into the variable.</exception>
        /// <seealso cref="PutData{D}(DataSet,int,D)"/>
        /// <seealso cref="PutData{D}(DataSet,Func{Variable,bool},D)"/>		
        public static void PutData<D>(this DataSet dataset, string variableName, D data, params int[] indices)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");
            int rank = indices == null ? 0 : indices.Length;
            Type dataType = typeof(D);
            PutData<D>(dataset,
                variable => variable.Name == variableName && variable.TypeOfData == dataType && variable.Rank == rank,
                data, indices);
        }

        #endregion

        #region Put range

        /// <summary>
        /// Puts a range of values into a variable.
        /// </summary>
        /// <typeparam name="D">D must a scalar or an array of a proper rank.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="variableId"><see cref="Variable.ID"/> of the variable to put data in.</param>
        /// <param name="data">Data to put.</param>
        /// <param name="range">Range of the data to set.</param>
        /// <remarks>
        /// <para>
        /// Number of <paramref name="range"/> elements must be equal to the rank of a variable.
        /// </para>
        /// <para>
        /// Type <typeparamref name="D"/> is a type of the input data. Its rank depends on rank
        /// of the variable and how many "reduce" ranges are presented in the <paramref name="range"/>
        /// (see also <see cref="DataSet.ReduceDim(int)"/>).
        /// </para>
        /// <para>
        /// <see cref="Range"/> can be created using static methods of the <see cref="DataSet"/> class.
        /// Read remarks for the <see cref="Range"/> struct.
        /// </para>
        /// <para>
        /// If the <paramref name="data"/> parameter is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// </remarks>
        public static void PutData<D>(this DataSet dataset, int variableId, D data, params Range[] range)
        {
            if (dataset == null) throw new ArgumentNullException("dataset");

            var var = dataset.Variables.GetByID(variableId);
            int rank = var.Rank;
            if (rank == 0) // scalar
            {
                if (range != null && range.Length > 0)
                    throw new ArgumentException("range for a scalar variable must be either null or empty");
                if (typeof(D) != var.TypeOfData)
                    throw new ArgumentException("Type D differs from the type of the given scalar variable");
                var.PutData(new D[] { data });
                return;
            }

            if (range == null) throw new ArgumentNullException("range");
            if (rank != range.Length)
                throw new ArgumentException("Length of range elements must be equal to the rank of the variable");

            bool isSrcArray = typeof(Array).IsAssignableFrom(typeof(D)) && data != null;

            Array src = isSrcArray ? (Array)(object)data : null;
            int srcRank = isSrcArray ? src.Rank : 0;
            if (isSrcArray && typeof(D).GetElementType() != var.TypeOfData)
                throw new ArgumentException("Type of data elements differs from variable type of data");
            if (!isSrcArray && typeof(D) != var.TypeOfData)
                throw new ArgumentException("Type of data differs from variable type of data");

            int[] origin = new int[rank];
            int[] shape = new int[rank];
            bool isEmpty = false;
            int reducedRank = rank;
            for (int i = 0, j = 0; i < rank; i++)
            {
                Range r = range[i];
                origin[i] = r.Origin;

                if (r.IsReduced)
                {
                    shape[i] = 1;
                    reducedRank--;
                    continue;
                }

                if (!isSrcArray) throw new ArgumentException("data is a scalar value but not all dimensions are reduced");

                if (r.Stride != 1) throw new NotSupportedException("range has stride > 1");
                if (j >= src.Rank) throw new ArgumentException("Rank of data is less than expected");
                if (r.IsUnlimited)
                    shape[i] = src.GetLength(j);
                else
                {
                    shape[i] = r.Count;
                    if (shape[i] != src.GetLength(j))
                        throw new ArgumentException("range for dimension " + i + " differs from data length");
                }
                if (shape[i] == 0)
                    isEmpty = true; // in fact, no data to put
                j++;
            }
            if (reducedRank != srcRank)
                throw new ArgumentException("Data has wrong rank");
            if (isEmpty) return;
            if (srcRank == rank)
            {
                var.PutData(origin, src);
                return;
            }

            // Expanding source array
            Array arr = Array.CreateInstance(var.TypeOfData, shape);
            if (srcRank == 0)
            {
                Array.Clear(shape, 0, shape.Length);
                arr.SetValue(data, shape);
            }
            else
            {
                Type type = src.GetType().GetElementType();
                if (type != typeof(DateTime) && type != typeof(string) && type != typeof(bool))
                {
                    Buffer.BlockCopy(src, 0, arr, 0, Buffer.ByteLength(arr));
                }
                else
                {
                    int[] getIndex = new int[srcRank];
                    int[] setIndex = new int[rank];
                    int length = src.Length;

                    for (int i = 0; i < length; i++)
                    {
                        arr.SetValue(src.GetValue(getIndex), setIndex);
                        for (int j = 0, k = 0; j < rank; j++)
                        {
                            if (range[j].IsReduced) continue;
                            getIndex[k]++;
                            setIndex[j]++;
                            if (getIndex[k] < src.GetLength(k))
                                break;
                            setIndex[j] = 0;
                            getIndex[k++] = 0;
                        }
                    }
                }
            }
            var.PutData(origin, arr);
        }

        /// <summary>
        /// Puts a range of values into a variable.
        /// </summary>
        /// <typeparam name="D">D must a scalar or an array of a proper rank.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="predicate">Determines the variable to put data in.</param>
        /// <param name="data">Data to put.</param>
        /// <param name="range">Range of the data to set.</param>
        /// <remarks>
        /// <para>
        /// Number of <paramref name="range"/> elements must be equal to the rank of a variable.
        /// </para>
        /// <para>
        /// Type <typeparamref name="D"/> is a type of the input data. Its rank depends on rank
        /// of the variable and how many "reduce" ranges are presented in the <paramref name="range"/>
        /// (see also <see cref="DataSet.ReduceDim(int)"/>).
        /// </para>
        /// <para>
        /// <see cref="Range"/> can be created using static methods of the <see cref="DataSet"/> class.
        /// Read remarks for the <see cref="Range"/> struct.
        /// </para>
        /// <para><paramref name="predicate"/> must select only one variable within the DataSet.
        /// Otherwise, an exception is thrown.</para>
        /// <para>
        /// If the <paramref name="data"/> parameter is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// </remarks>
        public static void PutData<D>(this DataSet dataset, Func<Variable, bool> predicate, D data, params Range[] range)
        {
            PutData<D>(dataset, FindVariable(dataset, predicate), data, range);
        }

        /// <summary>
        /// Puts a range of values into a variable.
        /// </summary>
        /// <typeparam name="D">D must a scalar or an array of a proper rank.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="variableName"><see cref="Variable.Name"/> of the variable to get data from.</param>        
        /// <param name="data">Data to put.</param>
        /// <param name="range">Range of the data to set.</param>
        /// <remarks>
        /// <para>
        /// Number of <paramref name="range"/> elements must be equal to the rank of a variable.
        /// </para>
        /// <para>
        /// Type <typeparamref name="D"/> is a type of the input data. Its rank depends on rank
        /// of the variable and how many "reduce" ranges are presented in the <paramref name="range"/>
        /// (see also <see cref="DataSet.ReduceDim(int)"/>).
        /// </para>
        /// <para>
        /// <see cref="Range"/> can be created using static methods of the <see cref="DataSet"/> class.
        /// Read remarks for the <see cref="Range"/> struct.
        /// </para>
        /// <para>The method finds a variable with name <paramref name="variableName"/>, given rank and
        /// type of data (determined from the arguments) and gets the data from it.
        /// If a variable is not found, an exception is thrown.</para>	
        /// <para>
        /// If the <paramref name="data"/> parameter is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Let variable "var2d" has rank 2
        /// ds.PutData&lt;double[]&gt;( "var2d", new double[] { 1, 2, 3 }, 
        ///  DataSet.Range(0,2), // 1st dim: from 0 to 2
        ///  DataSet.ReduceDim(1)); // 2nd dim is reduced in the input data set and its index is 1
        /// </code>
        /// </example>
        public static void PutData<D>(this DataSet dataset, string variableName, D data, params Range[] range)
        {
            if (dataset == null) throw new ArgumentNullException("dataset");

            int rank = range == null ? 0 : range.Length;
            Type dataType;
            if (typeof(Array).IsAssignableFrom(typeof(D)))
                dataType = typeof(D).GetElementType();
            else
                dataType = typeof(D);

            PutData<D>(dataset, FindVariable(dataset,
                variable => variable.Name == variableName &&
                    variable.TypeOfData == dataType &&
                    variable.Rank == rank), data, range);
        }

        #endregion

        #endregion

        #region Append

        #region Append by default dimension

        /// <summary>
        /// Appends a variable with data.
        /// </summary>
        /// <typeparam name="D">Must be an array type with rank equal or one less the variable rank.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="variableId"><see cref="Variable.ID"/> of the variable to append.</param>
        /// <param name="data">Data to append.</param>
        /// <remarks>
        /// <para>The shape of the variable grows along the first dimension. 
        /// For example, for a matrix (row,column) this will be adding more rows.</para>
        /// <para>
        /// If the <paramref name="data"/> parameter is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// <example>
        /// <code>
        /// // Creates new data set:
        /// var ds = DataSet.Open("test.csv?openMode=create");
        /// // Adds new variable with name "int1" that is a 1d-array of int and depends on dimension "idx".
        /// // id2 contains unique identifier of the new variable.
        /// int id2 = ds.Add&lt;int[]&gt;("int1", "idx").ID;
        /// // Data of the variable "int1" is { 9, 8, 7, 6 }:
        /// ds.PutData(id2, new int[] { 9, 8, 7, 6 });
        /// // Adds one more value to the end of the variable:
        /// ds.Append(id2, 5);
        /// // Now id2 contains { 9, 8, 7, 6, 5 }
        /// </code>
        /// </example>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">DataSet is disposed.</exception>
        /// <exception cref="ReadOnlyException">Variable is read-only.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">Arguments are incorrect.</exception>
        /// <exception cref="CannotPerformActionException">Cannot put data into the variable.</exception>
        /// <exception cref="KeyNotFoundException">Variable is not found.</exception>
        public static void Append<D>(this DataSet dataset, int variableId, D data)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");

            var var = dataset.Variables.GetByID(variableId);
            int rank = var.Rank;
            if (rank == 0)
                throw new NotSupportedException("Cannot append scalar variable");

            if (typeof(Array).IsAssignableFrom(typeof(D)))
            {
                int drank = typeof(D).GetArrayRank();
                if (var.Rank != drank + 1 && var.Rank != drank)
                    throw new ArgumentException("Type parameter D must be an array type with rank equal or one less the variable rank");

                Array adata = (Array)(object)data;
                var.Append(adata);
            }
            else
            {
                if (var.Rank != 1)
                    throw new ArgumentException("Type parameter D must be an array type with rank equal or one less the variable rank");
                var.Append(new D[] { data });
            }
        }

        /// <summary>
        /// Appends a variable with data.
        /// </summary>
        /// <typeparam name="D">Must be an array type with rank equal or one less the variable rank.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="predicate">Determines the variable to append.</param>
        /// <param name="data">Data to append.</param>
        /// <remarks>
        /// <para>The shape of the variable grows along the first dimension. 
        /// For example, for a matrix (row,column) this will be adding more rows.</para>
        /// <para>
        /// If the <paramref name="data"/> parameter is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>	
        /// </remarks>
        /// <exception cref="ObjectDisposedException">DataSet is disposed.</exception>
        /// <exception cref="ReadOnlyException">Variable is read-only.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">Arguments are incorrect.</exception>
        /// <exception cref="CannotPerformActionException">Cannot put data into the variable.</exception>
        /// <exception cref="KeyNotFoundException">Variable is not found.</exception>	
        public static void Append<D>(this DataSet dataset, Func<Variable, bool> predicate, D data)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");
            Append<D>(dataset, FindVariable(dataset, predicate), data);
        }

        /// <summary>
        /// Appends a variable with data.
        /// </summary>
        /// <typeparam name="D">Must be an array type with rank equal or one less the variable rank.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="variableName"><see cref="Variable.Name"/> of the variable to put data in.</param>
        /// <param name="data">Data to append.</param>
        /// <remarks>
        /// <para>The shape of the variable grows along the first dimension. 
        /// For example, for a matrix (row,column) this will be adding more rows.</para>
        /// <para>
        /// If the <paramref name="data"/> parameter is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>	
        /// </remarks>
        /// <exception cref="ObjectDisposedException">DataSet is disposed.</exception>
        /// <exception cref="ReadOnlyException">Variable is read-only.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">Arguments are incorrect.</exception>
        /// <exception cref="CannotPerformActionException">Cannot put data into the variable.</exception>
        /// <exception cref="KeyNotFoundException">Variable is not found.</exception>	
        public static void Append<D>(this DataSet dataset, string variableName, D data)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");
            int rank1, rank2; // allowed ranks of variables
            Type dataType;
            if (typeof(Array).IsAssignableFrom(typeof(D)))
            {
                rank1 = typeof(D).GetArrayRank();
                rank2 = rank1 + 1;
                dataType = typeof(D).GetElementType();
            }
            else
            {
                rank1 = 1;
                rank2 = -1; // no variants
                dataType = typeof(D);
            }
            Append<D>(dataset,
                variable => variable.Name == variableName && variable.TypeOfData == dataType && (variable.Rank == rank1 || variable.Rank == rank2),
                data);
        }

        #endregion

        #region Append by dimension index

        /// <summary>
        /// Appends a variable with data.
        /// </summary>
        /// <typeparam name="D">Must be an array type with rank equal or one less the variable rank.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="variableId"><see cref="Variable.ID"/> of the variable to append.</param>
        /// <param name="data">Data to append.</param>
        /// <param name="dimension">Zero-based index of the variable's dimension to append by.</param>	
        /// <exception cref="ObjectDisposedException">DataSet is disposed.</exception>
        /// <exception cref="ReadOnlyException">Variable is read-only.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">Arguments are incorrect.</exception>
        /// <exception cref="CannotPerformActionException">Cannot put data into the variable.</exception>
        /// <exception cref="KeyNotFoundException">Variable is not found.</exception>
        /// <remarks>
        /// <para>
        /// If the <paramref name="data"/> parameter is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// </remarks>
        public static void Append<D>(this DataSet dataset, int variableId, D data, int dimension)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");

            var var = dataset.Variables.GetByID(variableId);
            int rank = var.Rank;
            if (rank == 0)
                throw new NotSupportedException("Cannot append scalar variable");

            if (typeof(Array).IsAssignableFrom(typeof(D)))
            {
                int drank = typeof(D).GetArrayRank();
                if (var.Rank != drank + 1 && var.Rank != drank)
                    throw new ArgumentException("Type parameter D must be an array type with rank equal or one less the variable rank");

                Array adata = (Array)(object)data;
                var.Append(adata, dimension);
            }
            else
            {
                if (var.Rank != 1)
                    throw new ArgumentException("Type parameter D must be an array type with rank equal or one less the variable rank");
                var.Append(new D[] { data }, dimension);
            }
        }

        /// <summary>
        /// Appends a variable with data.
        /// </summary>
        /// <typeparam name="D">Must be an array type with rank equal or one less the variable rank.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="predicate">Determines the variable to append.</param>
        /// <param name="data">Data to append.</param>
        /// <param name="dimension">Zero-based index of the variable's dimension to append by.</param>	
        /// <remarks>
        /// <para>The shape of the variable grows along the first dimension. 
        /// For example, for a matrix (row,column) this will be adding more rows.</para>
        /// <para>
        /// If the <paramref name="data"/> parameter is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>	
        /// </remarks>
        /// <exception cref="ObjectDisposedException">DataSet is disposed.</exception>
        /// <exception cref="ReadOnlyException">Variable is read-only.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">Arguments are incorrect.</exception>
        /// <exception cref="CannotPerformActionException">Cannot put data into the variable.</exception>
        /// <exception cref="KeyNotFoundException">Variable is not found.</exception>	
        public static void Append<D>(this DataSet dataset, Func<Variable, bool> predicate, D data, int dimension)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");
            Append<D>(dataset, FindVariable(dataset, predicate), data, dimension);
        }

        /// <summary>
        /// Appends a variable with data.
        /// </summary>
        /// <typeparam name="D">Must be an array type with rank equal or one less the variable rank.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="variableName"><see cref="Variable.Name"/> of the variable to put data in.</param>
        /// <param name="data">Data to append.</param>
        /// <param name="dimension">Zero-based index of the variable's dimension to append by.</param>	
        /// <remarks>
        /// <para>The shape of the variable grows along the first dimension. 
        /// For example, for a matrix (row,column) this will be adding more rows.</para>
        /// <para>
        /// If the <paramref name="data"/> parameter is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>	
        /// </remarks>
        /// <exception cref="ObjectDisposedException">DataSet is disposed.</exception>
        /// <exception cref="ReadOnlyException">Variable is read-only.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">Arguments are incorrect.</exception>
        /// <exception cref="CannotPerformActionException">Cannot put data into the variable.</exception>
        /// <exception cref="KeyNotFoundException">Variable is not found.</exception>	
        public static void Append<D>(this DataSet dataset, string variableName, D data, int dimension)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");
            int rank1, rank2; // allowed ranks
            Type dataType;
            if (typeof(Array).IsAssignableFrom(typeof(D)))
            {
                rank1 = typeof(D).GetArrayRank();
                rank2 = rank1 + 1;
                dataType = typeof(D).GetElementType();
            }
            else
            {
                rank1 = 1;
                rank2 = -1;
                dataType = typeof(D);
            }
            Append<D>(dataset,
                variable => variable.Name == variableName && variable.TypeOfData == dataType && (variable.Rank == rank1 || variable.Rank == rank2),
                data, dimension);
        }

        #endregion

        #region Append by dimension name

        /// <summary>
        /// Appends a variable with data.
        /// </summary>
        /// <typeparam name="D">Must be an array type with rank equal or one less the variable rank.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="variableId"><see cref="Variable.ID"/> of the variable to append.</param>
        /// <param name="data">Data to append.</param>
        /// <param name="dimension">Name of the variable's dimension to append by.</param>	
        /// <exception cref="ObjectDisposedException">DataSet is disposed.</exception>
        /// <exception cref="ReadOnlyException">Variable is read-only.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">Arguments are incorrect.</exception>
        /// <exception cref="CannotPerformActionException">Cannot put data into the variable.</exception>
        /// <exception cref="KeyNotFoundException">Variable is not found.</exception>
        /// <remarks>
        /// <para>
        /// If the <paramref name="data"/> parameter is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// </remarks>
        public static void Append<D>(this DataSet dataset, int variableId, D data, string dimension)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");

            var var = dataset.Variables.GetByID(variableId);
            int rank = var.Rank;
            if (rank == 0)
                throw new NotSupportedException("Cannot append scalar variable");

            if (typeof(Array).IsAssignableFrom(typeof(D)))
            {
                int drank = typeof(D).GetArrayRank();
                if (var.Rank != drank + 1 && var.Rank != drank)
                    throw new ArgumentException("Type parameter D must be an array type with rank equal or one less the variable rank");

                Array adata = (Array)(object)data;
                var.Append(adata, dimension);
            }
            else
            {
                if (var.Rank != 1)
                    throw new ArgumentException("Type parameter D must be an array type with rank equal or one less the variable rank");
                var.Append(new D[] { data }, dimension);
            }
        }

        /// <summary>
        /// Appends a variable with data.
        /// </summary>
        /// <typeparam name="D">Must be an array type with rank equal or one less the variable rank.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="predicate">Determines the variable to append.</param>
        /// <param name="data">Data to append.</param>
        /// <param name="dimension">Name of the variable's dimension to append by.</param>	
        /// <remarks>
        /// <para>The shape of the variable grows along the first dimension. 
        /// For example, for a matrix (row,column) this will be adding more rows.</para>	
        /// <para>
        /// If the <paramref name="data"/> parameter is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">DataSet is disposed.</exception>
        /// <exception cref="ReadOnlyException">Variable is read-only.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">Arguments are incorrect.</exception>
        /// <exception cref="CannotPerformActionException">Cannot put data into the variable.</exception>
        /// <exception cref="KeyNotFoundException">Variable is not found.</exception>	
        public static void Append<D>(this DataSet dataset, Func<Variable, bool> predicate, D data, string dimension)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");
            Append<D>(dataset, FindVariable(dataset, predicate), data, dimension);
        }

        /// <summary>
        /// Appends a variable with data.
        /// </summary>
        /// <typeparam name="D">Must be an array type with rank equal or one less the variable rank.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="variableName"><see cref="Variable.Name"/> of the variable to put data in.</param>
        /// <param name="data">Data to append.</param>
        /// <param name="dimension">Name of the variable's dimension to append by.</param>	
        /// <remarks>
        /// <para>The shape of the variable grows along the first dimension. 
        /// For example, for a matrix (row,column) this will be adding more rows.</para>
        /// <para>
        /// If the <paramref name="data"/> parameter is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>	
        /// </remarks>
        /// <exception cref="ObjectDisposedException">DataSet is disposed.</exception>
        /// <exception cref="ReadOnlyException">Variable is read-only.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">Arguments are incorrect.</exception>
        /// <exception cref="CannotPerformActionException">Cannot put data into the variable.</exception>
        /// <exception cref="KeyNotFoundException">Variable is not found.</exception>	
        public static void Append<D>(this DataSet dataset, string variableName, D data, string dimension)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");
            int rank1, rank2;
            Type dataType;
            if (typeof(Array).IsAssignableFrom(typeof(D)))
            {
                rank1 = typeof(D).GetArrayRank();
                rank2 = rank1 + 1;
                dataType = typeof(D).GetElementType();
            }
            else
            {
                rank1 = 1;
                rank2 = -1;
                dataType = typeof(D);
            }
            Append<D>(dataset,
                variable => variable.Name == variableName && variable.TypeOfData == dataType && (variable.Rank == rank1 || variable.Rank == rank2),
                data, dimension);
        }

        #endregion

        #endregion

        #region Get

        #region Get all

        /// <summary>
        /// Gets all data from a variable.
        /// </summary>
        /// <typeparam name="D">Must be a scalar type for a variable of rank 0 and an array type for other ranks.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="variableId"><see cref="Variable.ID"/> of the variable to get data from.</param>
        /// <returns>Data of the variable.</returns>
        /// <remarks>
        /// <example>
        /// <code>
        /// // Opens a data set:
        /// var ds = DataSet.Open("test.csv?openMode=open");		
        /// // Gets a value from a scalar variable of type double with given id:
        /// int varID = ...;
        /// double val = ds.GetData&lt;double&gt;(varID); 
        /// // Gets an array from a 1d-variable of type string:
        /// int var2ID = ...;
        /// string[] strings = ds.GetData&lt;string[]&gt;(var2ID);
        /// </code>
        /// </example>
        /// </remarks>
        public static D GetData<D>(this DataSet dataset, int variableId)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");

            var var = dataset.Variables.GetByID(variableId);
            if (typeof(Array).IsAssignableFrom(typeof(D)))
            {
                return (D)(object)var.GetData();
            }
            // Scalar:
            if (var.Rank != 0)
                throw new ArgumentException("Type parameter D has rank different than variable's rank");
            return (D)var.GetData().GetValue(0);
        }

        /// <summary>
        /// Gets all data from a variable.
        /// </summary>
        /// <typeparam name="D">Must be a scalar type for a variable of rank 0 and an array type for other ranks.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="predicate">Determines the variable to get data from.</param>
        /// <returns>Data of the variable.</returns>
        /// <remarks>
        /// <para><paramref name="predicate"/> must select only one variable within the DataSet.
        /// Otherwise, an exception is thrown.</para>
        /// </remarks>
        public static D GetData<D>(this DataSet dataset, Func<Variable, bool> predicate)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");
            return GetData<D>(dataset, FindVariable(dataset, predicate));
        }

        /// <summary>
        /// Gets all data from a variable.
        /// </summary>
        /// <typeparam name="D">Must be a scalar type for a variable of rank 0 and an array type for other ranks.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="variableName"><see cref="Variable.Name"/> of the variable to get data from.</param>
        /// <returns>Data of the variable.</returns>
        /// <remarks>
        /// <para>The method finds a variable with name <paramref name="variableName"/>, given rank and
        /// type of data (determined from the arguments) and gets the data from it.
        /// If a variable is not found or there are several variables with the name, an exception is thrown.</para>	
        /// <example>
        /// <code>
        /// // Opens a data set:
        /// var ds = DataSet.Open("test.csv?openMode=open");		
        /// // Gets a value from a scalar variable named "str" of type string:
        /// string val = ds.GetData&lt;double&gt;("str"); 
        /// // Gets an array from a 1d-variable named "int1" of type int:
        /// int[] data = ds.GetData&lt;int[]&gt;("int1");
        /// </code>
        /// </example>
        /// </remarks>
        public static D GetData<D>(this DataSet dataset, string variableName)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");

            int rank;
            Type dataType;
            if (typeof(Array).IsAssignableFrom(typeof(D)))
            {
                rank = typeof(D).GetArrayRank();
                dataType = typeof(D).GetElementType();
            }
            else
            {
                rank = 0;
                dataType = typeof(D);
            }

            return GetData<D>(dataset, FindVariable(dataset,
                variable => variable.Name == variableName &&
                    variable.TypeOfData == dataType &&
                    variable.Rank == rank));
        }

        #endregion

        #region Get single value

        /// <summary>
        /// Gets a single value from a variable.
        /// </summary>
        /// <typeparam name="D">Must be a scalar type equal to the variable data type.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="variableId"><see cref="Variable.ID"/> of the variable to get data from.</param>
        /// <param name="indices">Indices of the value to get.</param>
        /// <returns>Data of the variable.</returns>
        /// <remarks>
        /// <para>Number of indices must be equal to the rank of a variable. For a scalar variable,
        /// <paramref name="indices"/> can be either null or int[0].</para>
        /// </remarks>
        public static D GetData<D>(this DataSet dataset, int variableId, params int[] indices)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");
            if (typeof(Array).IsAssignableFrom(typeof(D)))
                throw new ArgumentException("D must be a scalar type");

            var var = dataset.Variables.GetByID(variableId);
            if (indices == null || indices.Length == 0)
            {
                if (var.Rank != 0)
                    throw new ArgumentException("For a scalar variable indices must be either null or int[0]", "indices");
                return (D)var.GetData().GetValue(0);
            }
            if (var.Rank != indices.Length)
                throw new ArgumentException("Length of indices differs from the variable rank", "indices");
            int r = var.Rank;
            int[] shape = new int[r];
            for (int i = 0; i < r; i++)
                shape[i] = 1;
            Array arr = var.GetData(indices, shape);
            for (int i = 0; i < r; i++)
                shape[i] = 0;
            return (D)arr.GetValue(shape);
        }

        /// <summary>
        /// Gets a single value from a variable.
        /// </summary>
        /// <typeparam name="D">Must be a scalar type equal to the variable data type.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="predicate">Determines the variable to get data from.</param>
        /// <param name="indices">Indices of the value to get.</param>
        /// <returns>Data of the variable.</returns>
        /// <remarks>
        /// <para>Number of indices must be equal to the rank of a variable. For a scalar variable,
        /// <paramref name="indices"/> can be either null or int[0].</para>
        /// <para><paramref name="predicate"/> must select only one variable within the DataSet.
        /// Otherwise, an exception is thrown.</para>
        /// </remarks>
        public static D GetData<D>(this DataSet dataset, Func<Variable, bool> predicate, params int[] indices)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");
            return GetData<D>(dataset, FindVariable(dataset, predicate), indices);
        }

        /// <summary>
        /// Gets a single value from a variable.
        /// </summary>
        /// <typeparam name="D">Must be a scalar type equal to the variable data type.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="variableName"><see cref="Variable.Name"/> of the variable to get data from.</param>
        /// <param name="indices">Indices of the value to get.</param>
        /// <returns>Data of the variable.</returns>
        /// <remarks>
        /// <para>Number of indices must be equal to the rank of a variable. For a scalar variable,
        /// <paramref name="indices"/> can be either null or int[0].</para>
        /// <para>The method finds a variable with name <paramref name="variableName"/>, given rank and
        /// type of data (determined from the arguments) and gets the data from it.
        /// If a variable is not found, an exception is thrown.</para>	
        /// <example>
        /// <code>
        /// // Opens a data set:
        /// var ds = DataSet.Open("test.csv?openMode=open");		
        /// // Gets a value from a scalar variable named "str" of type string:
        /// string val = ds.GetData&lt;double&gt;("str"); 
        /// // Gets the 3rd (index starts from zero) value from a 1d-variable named "int1" of type int:
        /// int data = ds.GetData&lt;int&gt;("int1", 2);
        /// </code>
        /// </example>
        /// </remarks>
        public static D GetData<D>(this DataSet dataset, string variableName, params int[] indices)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");

            int rank = indices == null ? 0 : indices.Length;
            Type dataType = typeof(D);

            return GetData<D>(dataset, FindVariable(dataset,
                variable => variable.Name == variableName &&
                    variable.TypeOfData == dataType &&
                    variable.Rank == rank), indices);
        }

        #endregion

        #region Get range

        /// <summary>
        /// Get a range of values from a variable.
        /// </summary>
        /// <typeparam name="D">D must a scalar or an array of a proper rank.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="variableId"><see cref="Variable.ID"/> of the variable to get data from.</param>
        /// <param name="range">Range of the data to get.</param>
        /// <returns>Requested data.</returns>
        /// <remarks>
        /// <para>
        /// Number of <paramref name="range"/> elements must be equal to the rank of a variable.
        /// </para>
        /// <para>
        /// Type <typeparamref name="D"/> is a type of the result data. Its rank depends on rank
        /// of the variable and how many "reduce" ranges are presented in the <paramref name="range"/>
        /// (see also <see cref="DataSet.ReduceDim(int)"/>).
        /// </para>
        /// <para>
        /// <see cref="Range"/> can be created using static methods of the <see cref="DataSet"/> class.
        /// Read remarks for the <see cref="Range"/> struct.
        /// </para>
        /// </remarks>
        public static D GetData<D>(this DataSet dataset, int variableId, params Range[] range)
        {
            if (dataset == null) throw new ArgumentNullException("dataset");

            var var = dataset.Variables.GetByID(variableId);
            int rank = var.Rank;
            if (rank == 0) // scalar
            {
                if (range != null && range.Length > 0)
                    throw new ArgumentException("range for a scalar variable must be either null or empty");
                if (typeof(D) != var.TypeOfData)
                    throw new ArgumentException("Type D differs from the type of the given scalar variable");
                return (D)var.GetData().GetValue(0);
            }

            if (range == null) throw new ArgumentNullException("range");
            if (rank != range.Length)
                throw new ArgumentException("Length of range elements must be equal to the rank of the variable");

            int[] origin = new int[rank];
            int[] stride = new int[rank];
            int[] count = new int[rank];

            int resRank = rank;
            bool isEmpty = false;
            int[] varShape = null;

            for (int i = 0; i < rank; i++)
            {
                Range r = range[i];
                origin[i] = r.Origin;
                stride[i] = r.Stride;

                if (r.IsUnlimited)
                {
                    if (varShape == null) varShape = var.GetShape();
                    count[i] = varShape[i];
                }
                else
                {
                    count[i] = r.Count;
                }
                if (r.IsReduced)
                    resRank--;
                if (r.IsEmpty)
                    isEmpty = true;
            }
            Array arr;
            if (isEmpty)
                arr = Array.CreateInstance(var.TypeOfData, count);
            else
                arr = var.GetData(origin, stride, count);

            if (resRank == rank) return (D)(object)arr;
            if (resRank == 0)
                return (D)arr.GetValue(new int[rank]);

            // Reducing rank of the array
            int[] dstShape = new int[resRank];
            for (int i = 0, j = 0; i < rank; i++)
            {
                if (range[i].IsReduced) continue;
                dstShape[j++] = count[i];
            }
            Array dst = Array.CreateInstance(var.TypeOfData, dstShape);

            Type type = dst.GetType().GetElementType();

            if (type != typeof(DateTime) && type != typeof(string) && type != typeof(bool))
            {
                Buffer.BlockCopy(arr, 0, dst, 0, Buffer.ByteLength(arr));
            }
            else
            {
                int[] getIndex = new int[rank];
                int[] setIndex = new int[resRank];
                int length = dst.Length;

                for (int i = 0; i < length; i++)
                {
                    dst.SetValue(arr.GetValue(getIndex), setIndex);
                    for (int j = 0, k = 0; j < rank; j++)
                    {
                        if (range[j].IsReduced) continue;
                        getIndex[j]++;
                        setIndex[k]++;
                        if (setIndex[k] < dst.GetLength(k))
                            break;
                        setIndex[k++] = 0;
                        getIndex[j] = 0;
                    }
                }
            }

            return (D)(object)dst;
        }

        /// <summary>
        /// Get a range of values from a variable.
        /// </summary>
        /// <typeparam name="D">D must a scalar or an array of a proper rank.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="predicate">Determines the variable to get data from.</param>
        /// <param name="range">Range of the data to get.</param>
        /// <returns>Requested data.</returns>
        /// <remarks>
        /// <para>
        /// Number of <paramref name="range"/> elements must be equal to the rank of a variable.
        /// </para>
        /// <para>
        /// Type <typeparamref name="D"/> is a type of the result data. Its rank depends on rank
        /// of the variable and how many "reduce" ranges are presented in the <paramref name="range"/>
        /// (see also <see cref="DataSet.ReduceDim(int)"/>).
        /// </para>
        /// <para>
        /// <see cref="Range"/> can be created using static methods of the <see cref="DataSet"/> class.
        /// Read remarks for the <see cref="Range"/> struct.
        /// </para>    
        /// <para><paramref name="predicate"/> must select only one variable within the DataSet.
        /// Otherwise, an exception is thrown.</para>
        /// </remarks>
        public static D GetData<D>(this DataSet dataset, Func<Variable, bool> predicate, params Range[] range)
        {
            if (dataset == null) throw new ArgumentNullException("dataset");
            return GetData<D>(dataset, FindVariable(dataset, predicate), range);
        }

        /// <summary>
        /// Get a range of values from a variable.
        /// </summary>
        /// <typeparam name="D">D must a scalar or an array of a proper rank.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="variableName"><see cref="Variable.Name"/> of the variable to get data from.</param>
        /// <param name="range">Range of the data to get.</param>
        /// <returns>Requested data.</returns>
        /// <remarks>
        /// <para>
        /// Number of <paramref name="range"/> elements must be equal to the rank of a variable.
        /// </para>
        /// <para>
        /// Type <typeparamref name="D"/> is a type of the result data. Its rank depends on rank
        /// of the variable and how many "reduce" ranges are presented in the <paramref name="range"/>
        /// (see also <see cref="DataSet.ReduceDim(int)"/>).
        /// </para>
        /// <para>
        /// <see cref="Range"/> can be created using static methods of the <see cref="DataSet"/> class.
        /// Read remarks for the <see cref="Range"/> struct.
        /// </para>
        /// <para>The method finds a variable with name <paramref name="variableName"/>, given rank and
        /// type of data (determined from the arguments) and gets the data from it.
        /// If a variable is not found, an exception is thrown.</para>	
        /// </remarks>
        public static D GetData<D>(this DataSet dataset, string variableName, params Range[] range)
        {
            if (dataset == null) throw new ArgumentNullException("dataset");

            int rank = range == null ? 0 : range.Length;
            Type dataType;
            if (typeof(Array).IsAssignableFrom(typeof(D)))
                dataType = typeof(D).GetElementType();
            else
                dataType = typeof(D);

            return GetData<D>(dataset, FindVariable(dataset,
                variable => variable.Name == variableName &&
                    variable.TypeOfData == dataType &&
                    variable.Rank == rank), range);
        }

        #endregion

        #endregion

        #region Attributes

        #region Get attribute

        /// <summary>
        /// Gets a metadata attribute value.
        /// </summary>
        /// <param name="dataset">Target data set.</param>
        /// <param name="variableId"><see cref="Variable.ID"/> of the variable to get metadata attribute from.</param>
        /// <param name="attributeName">The name of the metadata attribute to get.</param>
        /// <returns>The attribute value.</returns>
        public static object GetAttr(this DataSet dataset, int variableId, string attributeName)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");

            var var = dataset.Variables.GetByID(variableId);
            return var.Metadata[attributeName];
        }

        /// <summary>
        /// Gets a metadata attribute value.
        /// </summary>
        /// <param name="dataset">Target data set.</param>
        /// <param name="variableName"><see cref="Variable.Name"/> of the variable to get metadata attribute from.</param>
        /// <param name="attributeName">The name of the metadata attribute to get.</param>
        /// <returns>The attribute value.</returns>
        /// <remarks>
        /// <para>The method finds a variable with name <paramref name="variableName"/>
        /// and gets the attribute from it.
        /// If a variable is not found or there are several variables with the name, 
        /// an exception is thrown.</para>	
        /// <example>
        /// <code>
        /// // Opens a data set:
        /// var ds = DataSet.Open("test.csv?openMode=open");		
        /// // Gets attribute "range" from variable "var":
        /// object range = ds.GetAttr("var", "range"); 
        /// </code>
        /// </example>
        /// </remarks>
        public static object GetAttr(this DataSet dataset, string variableName, string attributeName)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");

            return GetAttr(dataset, FindVariable(dataset, variable => variable.Name == variableName), attributeName);
        }

        /// <summary>
        /// Gets a metadata attribute value.
        /// </summary>
        /// <param name="dataset">Target data set.</param>
        /// <param name="predicate">Determines the variable to get attribute from.</param>
        /// <param name="attributeName">The name of the metadata attribute to get.</param>
        /// <returns>The attribute value.</returns>
        /// <remarks>
        /// <para><paramref name="predicate"/> must select only one variable within the DataSet.
        /// Otherwise, an exception is thrown.</para>
        /// </remarks>
        public static object GetAttr(this DataSet dataset, Func<Variable, bool> predicate, string attributeName)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");

            return GetAttr(dataset, FindVariable(dataset, predicate), attributeName);
        }

        /// <summary>
        /// Gets a metadata attribute value.
        /// </summary>
        /// <typeparam name="T">Type of the attribute value.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="variableId"><see cref="Variable.ID"/> of the variable to get metadata attribute from.</param>
        /// <param name="attributeName">The name of the metadata attribute to get.</param>
        /// <returns>The attribute value.</returns>
        public static T GetAttr<T>(this DataSet dataset, int variableId, string attributeName)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");

            var var = dataset.Variables.GetByID(variableId);
            return (T)var.Metadata[attributeName];
        }

        /// <summary>
        /// Gets a metadata attribute value.
        /// </summary>
        /// <typeparam name="T">Type of the attribute value.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="variableName"><see cref="Variable.Name"/> of the variable to get metadata attribute from.</param>
        /// <param name="attributeName">The name of the metadata attribute to get.</param>
        /// <returns>The attribute value.</returns>
        /// <remarks>
        /// <para>The method finds a variable with name <paramref name="variableName"/>
        /// and gets the attribute from it.
        /// If a variable is not found or there are several variables with the name, 
        /// an exception is thrown.</para>	
        /// <example>
        /// <code>
        /// // Opens a data set:
        /// var ds = DataSet.Open("test.csv?openMode=open");		
        /// // Gets attribute "range" from variable "str":
        /// int[] range = ds.GetAttr&lt;int[]&gt;("str", "range"); 
        /// int min = range[0];
        /// int max = range[1];
        /// </code>
        /// </example>
        /// </remarks>
        public static T GetAttr<T>(this DataSet dataset, string variableName, string attributeName)
        {
            return (T)GetAttr(dataset, variableName, attributeName);
        }

        /// <summary>
        /// Gets a metadata attribute value.
        /// </summary>
        /// <typeparam name="T">Type of the attribute value.</typeparam>
        /// <param name="dataset">Target data set.</param>
        /// <param name="predicate">Determines the variable to get attribute from.</param>
        /// <param name="attributeName">The name of the metadata attribute to get.</param>
        /// <returns>The attribute value.</returns>
        /// <remarks>
        /// <para><paramref name="predicate"/> must select only one variable within the DataSet.
        /// Otherwise, an exception is thrown.</para>
        /// </remarks>
        public static T GetAttr<T>(this DataSet dataset, Func<Variable, bool> predicate, string attributeName)
        {
            return (T)GetAttr(dataset, predicate, attributeName);
        }

        #endregion

        #region Put attribute

        /// <summary>
        /// Sets a metadata attribute value.
        /// </summary>
        /// <param name="dataset">Target data set.</param>
        /// <param name="variableId"><see cref="Variable.ID"/> of the variable to set metadata attribute.</param>
        /// <param name="attributeName">The name of the metadata attribute to set.</param>
        /// <param name="value">Value of the attribute.</param>
        /// <remarks>
        /// <para>
        /// If the new <paramref name="value"/> for the key is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// </remarks>
        public static void PutAttr(this DataSet dataset, int variableId, string attributeName, object value)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");

            var var = dataset.Variables.GetByID(variableId);
            var.Metadata[attributeName] = value;
        }

        /// <summary>
        /// Sets a metadata attribute value.
        /// </summary>
        /// <param name="dataset">Target data set.</param>
        /// <param name="variableName"><see cref="Variable.Name"/> of the variable to set metadata attribute.</param>
        /// <param name="attributeName">The name of the metadata attribute to set.</param>
        /// <param name="value">Value of the attribute.</param>
        /// <remarks>
        /// <para>The method finds a variable with name <paramref name="variableName"/>
        /// and sets its attribute.
        /// If a variable is not found or there are several variables with the name, 
        /// an exception is thrown.</para>	
        /// <para>
        /// If the new <paramref name="value"/> for the key is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// <example>
        /// <code>
        /// // Opens a data set:
        /// var ds = DataSet.Open("test.csv?openMode=open");		
        /// // Sets attribute "range" of variable "var":
        /// ds.SetAttr("var", "range", new int[] { 0, 100 }); 
        /// </code>
        /// </example>
        /// </remarks>
        public static void PutAttr(this DataSet dataset, string variableName, string attributeName, object value)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");
            PutAttr(dataset, FindVariable(dataset, variable => variable.Name == variableName), attributeName, value);
        }

        /// <summary>
        /// Sets a metadata attribute value.
        /// </summary>
        /// <param name="dataset">Target data set.</param>
        /// <param name="predicate">Determines the variable to set attribute.</param>
        /// <param name="attributeName">The name of the metadata attribute to set.</param>
        /// <param name="value">Value of the attribute.</param>
        /// <remarks>
        /// <para><paramref name="predicate"/> must select only one variable within the DataSet.
        /// Otherwise, an exception is thrown.</para>
        /// <para>
        /// If the new <paramref name="value"/> for the key is an array, it is copied by the method for further use; therefore after this method call, it is allowed
        /// to change the original array without affecting the data of the variable or proposed changes to commit.
        /// </para>
        /// </remarks>
        public static void PutAttr(this DataSet dataset, Func<Variable, bool> predicate, string attributeName, object value)
        {
            if (dataset == null)
                throw new ArgumentNullException("dataset");
            PutAttr(dataset, FindVariable(dataset, predicate), attributeName, value);
        }

        #endregion

        #endregion
    }
}

