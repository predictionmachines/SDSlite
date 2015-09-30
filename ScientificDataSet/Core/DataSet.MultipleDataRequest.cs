// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data
{
	public partial class DataSet
	{
		/// <summary>
		/// Gets data from several variables atomically with a guarantee that the data belong to the same DataSet version.
		/// </summary>
		/// <param name="requests">Data requests describing what data is to be returned.</param>
		/// <returns>All requested data.</returns>
		/// <remarks>
		/// <para>
		/// The problem is to get consistent data from one version of a DataSet.
		/// A typical program flow for working with data from several related variables 
		/// involve several consequent calls of the <see cref="Variable.GetData()"/>. 
		/// If the data set is being changed in the background by another process, 
		/// the data returned may come from different versions (see <see cref="DataSet.Version"/>). 
		/// This is inappropriate for many applications.
		/// </para>
		/// <para>
		/// The solution is to use <see cref="GetMultipleData"/> method which performs several
		/// <see cref="Variable.GetData()"/> operations atomically and guarantees that 
		/// all returned arrays belong to the same DataSet version.
		/// </para>
		/// <example>
		/// The following example gets data from two variables atomically:
		/// <code>
		/// DataSet ds = . . .;
		/// var v1 = ds.AddVariable&lt;string&gt;("v1", "x");
		/// var v2 = ds.AddVariable&lt;double&gt;("v2", "x", "y");
		/// 
		/// . . .
		/// 
		/// MultipleDataResponse response = ds.GetMultipleData(
		///			// requesting 10 first elements from "v1":
		///			DataRequest.GetData(v1, null, new int[1] { 10 }), 
		///			// requesting 10x20 first elements from "v2":
		///			DataRequest.GetData(v2, null, new int[2] { 10, 20 }));
		///
		/// DataResponse r1 = response[v1.ID];
		/// DataResponse r2 = response[v2.ID];
		///	
		/// // All the data belong to the version "response.Version".
		/// string[] d1 = (string[]) r1.Data;
		/// double[,] d2 = (double[,]) r2.Data;
		/// </code>
		/// </example>
		/// </remarks>
		/// <seealso cref="DataRequest"/>
		/// <seealso cref="MultipleDataResponse"/>
		/// <exception cref="ArgumentNullException"><paramref name="requests"/> or one of its elements is null.</exception>
		/// <exception cref="ArgumentException">One of requested variable belongs to another DataSet.</exception>
		/// <exception cref="ApplicationException">Version of the DataSet unexpectedly changed during getting data.</exception>
		public virtual MultipleDataResponse GetMultipleData(params DataRequest[] requests)
		{
			if (requests == null)
				throw new ArgumentNullException("requests");
			if (requests.Length == 0)
				return new MultipleDataResponse(Version, new DataResponse[0]);

			foreach (var r in requests)
			{
				if (r == null)
					throw new ArgumentException("One of parameter's elements is null", "requests");
				if (r.Variable.DataSet != this)
					throw new ArgumentException("Cannot get data from another's variable");
			}

			DataResponse[] responses = new DataResponse[requests.Length];

			lock (this)
			{
				int version = this.Version;
				for (int i = 0; i < requests.Length; i++)
				{
					DataRequest r = requests[i];
					Array data;
					if (r.Stride == null)
						data = r.Variable.GetData(r.Origin, r.Shape);
					else
						data = r.Variable.GetData(r.Origin, r.Stride, r.Shape);
					DataResponse resp = new DataResponse(r, data);
					responses[i] = resp;
				}

				if (this.Version != version)
					throw new CannotPerformActionException("Version has changed");

				return new MultipleDataResponse(version, responses);
			}
		}
	}

	/// <summary>
	/// Contains reponses on multiple data requests.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The <see cref="MultipleDataResponse"/> class contains responses for each data request
	/// specified in the <see cref="DataSet.GetMultipleData"/> method.
	/// Each response is represented by a <see cref="DataResponse"/> class instance.
	/// The class also contains a <see cref="Version"/> number of the <see cref="DataSet"/>
	/// the data is taken from. 
	/// </para>
	/// <para>
	/// See remarks for the <see cref="DataSet.GetMultipleData"/> method.
	/// </para>
	/// </remarks>
	public class MultipleDataResponse
	{
		private int version;
		private DataResponse[] responses;

		internal MultipleDataResponse(int version, DataResponse[] responses)
		{
			this.responses = responses;
			this.version = version;
		}

		/// <summary>
		/// Gets the version of the <see cref="DataSet"/> the data is taken from.
		/// </summary>
		public int Version { get { return version; } }

		/// <summary>
		/// Converts the <see cref="MultipleDataResponse"/> into an array <c>DataResponse[]</c>.
		/// </summary>
		/// <param name="response">Response to convert.</param>
		/// <returns>An array of <see cref="DataResponse"/>.</returns>
		/// <remarks>
		/// <para>The conversion is to be used when the multiple data request
		/// contains several requests for one variable and thus
		/// the actual responses cannot be retrieved through the variable ID.</para>
		/// </remarks>
		public static implicit operator DataResponse[](MultipleDataResponse response)
		{
			return response.responses;
		}

		/// <summary>
		/// Gets an array of <c>DataResponse[]</c>.
		/// </summary>
		/// <returns>An array of <see cref="DataResponse"/>.</returns>
		/// <remarks>
		/// <para>The conversion is to be used when the multiple data request
		/// contains several requests for one variable and thus
		/// the actual responses cannot be retrieved through the variable ID.</para>
		/// </remarks>
		public DataResponse[] ToDataResponses()
		{
			return (DataResponse[])this;
		}

		/// <summary>
		/// Gets the number of individual responses contained in the <see cref="MultipleDataResponse"/>.
		/// </summary>
		public int Count { get { return responses.Length; } }

		/// <summary>
		/// Gets the response for the variable with the given <paramref name="variableID"/>.
		/// </summary>
		/// <param name="variableID">The <see cref="Variable.ID"/> of the variable the data is taken from.</param>
		/// <returns>The response for the particular variable.</returns>
		public DataResponse this[int variableID]
		{
			get
			{
				for (int i = 0; i < responses.Length; i++)
				{
					if (responses[i].DataRequest.Variable.ID == variableID)
						return responses[i];
				}
				throw new KeyNotFoundException("Variable with given ID " + variableID + " not found");
			}
		}
	}

	/// <summary>
	/// Describes a request for data of a variable.
	/// </summary>
	public class DataRequest
	{
		private Variable var;
		private int[] origin;
		private int[] shape;
		private int[] stride;

		private DataRequest(Variable var, int[] origin, int[] stride, int[] shape)
		{
			if (var == null)
				throw new ArgumentNullException("var");
			this.var = var;
			this.origin = origin;
			this.stride = stride;
			this.shape = shape;
		}

		/// <summary>
		/// Gets the variable that is to be requested.
		/// </summary>
		public Variable Variable { get { return var; } }
		/// <summary>
		/// Gets the origin of requested data.
		/// </summary>
		/// <remarks>
		/// <para>Null means all zeros.</para>
		/// </remarks>
		public int[] Origin { get { return origin; } }
		/// <summary>
		/// Gets the stride of requested data.
		/// </summary>
		/// <remarks>
		/// <para>Null means "no stride", i.e. an array of <c>1</c>.</para>
		/// </remarks>
		public int[] Stride { get { return stride; } }
		/// <summary>
		/// Gets the shape of requested data.
		/// </summary>
		/// <remarks>
		/// <para>Null means "as much as available".</para>
		/// </remarks>
		public int[] Shape { get { return shape; } }

		/// <summary>
		/// Gets the request for the entire data of the variable <paramref name="var"/>.
		/// </summary>
		/// <param name="var">The variable to get data from.</param>
		/// <returns>Request for data.</returns>
		public static DataRequest GetData(Variable var)
		{
			return new DataRequest(var, null, null, null);
		}
		/// <summary>
		/// Gets the request for the particular data from the variable <paramref name="var"/>.
		/// </summary>
		/// <param name="var">The variable to get data from.</param>
		/// <param name="origin">The origin of the requested data.</param>
		/// <param name="shape">The shape of the requested data.</param>
		/// <returns>Request for data.</returns>
		/// <remarks>
		/// <para>
		/// If <paramref name="origin"/> is <c>null</c>, all zeros are inferred.
		/// If <paramref name="shape"/> is <c>null</c>, as much as available data is to
		/// be requested.
		/// </para>
		/// </remarks>
		public static DataRequest GetData(Variable var, int[] origin, int[] shape)
		{
			return new DataRequest(var, origin, null, shape);
		}
		/// <summary>
		/// Gets the request for the particular data  fromt the variable <paramref name="var"/>.
		/// </summary>
		/// <param name="var">The variable to get data from.</param>
		/// <param name="origin">The origin of the requested data.</param>
		/// <param name="stride">The steps to subsample the data.</param>
		/// <param name="shape">The shape of the requested data.</param>
		/// <returns>Request for data.</returns>
		/// <remarks>
		/// <para>
		/// If <paramref name="origin"/> is <c>null</c>, all zeros are inferred.
		/// If <paramref name="stride"/> is <c>null</c>, no stride is done (i.e. steps equal 
		/// to <c>1</c> are to be used).
		/// If <paramref name="shape"/> is <c>null</c>, as much as available data is to
		/// be requested.
		/// </para>
		/// </remarks>
		public static DataRequest GetData(Variable var, int[] origin, int[] stride, int[] shape)
		{
			return new DataRequest(var, origin, stride, shape);
		}
	}

	/// <summary>
	/// Contains response on a request for a variable data.
	/// </summary>
	public class DataResponse
	{
		private DataRequest request;
		private Array data;

		internal DataResponse(DataRequest request, Array data)
		{
			this.data = data;
			this.request = request;
		}

		/// <summary>
		/// Gets the request for this response.
		/// </summary>
		public DataRequest DataRequest { get { return request; } }

		/// <summary>
		/// Gets the data contained in the response.
		/// </summary>
		public Array Data { get { return data; } }
	}
}

