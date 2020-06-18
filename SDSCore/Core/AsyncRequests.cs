// Copyright © 2010 Microsoft Corporation, All Rights Reserved.
// This code released under the terms of the Microsoft Research License Agreement (MSR-LA, http://sds.codeplex.com/License)
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Research.Science.Data
{
	/// <summary>
	/// Supports asynchronous data requests.
	/// </summary>
	public interface IDataRequestable
	{
		///<summary>
		/// Starts an asynchronous get operation.
		///</summary>
		///<param name="origin">The origin of the window (left-bottom corner). Null means all zeros.</param>
		///<param name="shape">The shape of the corned. Null means maximal shape.</param>
		///<param name="responseHandler">Delegate for the request completion notification.</param>
		void RequestData(int[] origin, int[] shape, VariableResponseHandler responseHandler);

		///<summary>
		/// Starts an asynchronous get operation.
		///</summary>
		///<param name="origin">The origin of the window (left-bottom corner). Null means all zeros.</param>
		///<param name="shape">The shape of the corned. Null means maximal shape.</param>
		///<param name="stride">Steps to stride the variable.</param>
		///<param name="responseHandler">Delegate for the request completion notification.</param>
		void RequestData(int[] origin, int[] stride, int[] shape, VariableResponseHandler responseHandler);
	}

	/// <summary>
	/// Supports asynchronous multiple data requests.
	/// </summary>
	/// <remarks>
	/// Asynchronously invokes the <see cref="DataSet.GetMultipleData"/> method.
	/// </remarks>
	public interface IMultipleDataRequestable
	{
		/// <summary>
		/// Asynchronous request for multiple data of the proxy. 
		/// </summary>
		/// <param name="requests">Data requests describing what data is to be returned.</param>
		/// <param name="responseHandler">The handler to receive either response of a fault.</param>
		void RequestMultipleData(AsyncMultipleDataResponseHandler responseHandler, params DataRequest[] requests);
	}

	/// <summary>
	/// The handler for asynchronous variable data requests.
	/// </summary>
	/// <param name="response">Result of the request.</param>
	public delegate void VariableResponseHandler(VariableResponse response);

	/// <summary>
	/// The handler for asynchronous GetMultipleData requests.
	/// </summary>
	/// <param name="response">Result of the request.</param>
	/// <seealso cref="DataSet.GetMultipleData"/>
	public delegate void AsyncMultipleDataResponseHandler(AsyncMultipleDataResponse response);

	/// <summary>
	/// Result of an asynchronous request for multiple data.
	/// </summary>
	/// <remarks>
	/// <para>
	/// See remarks for the <see cref="DataSet.GetMultipleData"/> and 
	/// <see cref="IMultipleDataRequestable.RequestMultipleData"/> method.
	/// </para>
	/// </remarks>
	public class AsyncMultipleDataResponse
	{
		private MultipleDataResponse response;
		private Exception exc;

		/// <summary>
		/// Creates an instance in case of success.
		/// </summary>
		/// <param name="response"></param>
		public AsyncMultipleDataResponse(MultipleDataResponse response)
		{
			this.response = response;
		}

		/// <summary>
		/// Creates an instance in case of failure.
		/// </summary>
		public AsyncMultipleDataResponse(Exception fault)
		{
			this.exc = fault;
		}

		/// <summary>
		/// Gets the response containing data for all requests.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The property returns <c>null</c>, if <see cref="IsSuccess"/> is <c>false</c>.
		/// </para>
		/// </remarks>
		public MultipleDataResponse Response
		{
			get { return response; }
		}

		/// <summary>
		/// Gets the value indicating whether the operation has been successful or not.
		/// </summary>
		public bool IsSuccess
		{
			get { return exc == null; }
		}

		/// <summary>
		/// Gets the exception describing failure in case when <see cref="IsSuccess"/> property is false.
		/// </summary>
		public Exception Exception
		{
			get { return exc; }
		}
	}

	/// <summary>
	/// Represents the result of an asynchronous request for variable's data.
	/// </summary>
	public class VariableResponse
	{
		private Variable var;
		private int[] origin;
		private int[] stride;
		private Array data;
		private Exception exception;
		private int version;		

		/// <summary>
		/// Use on success.
		/// </summary>
		public VariableResponse(Variable variable, int[] origin, int[] stride, Array data, int version)
		{
			if (variable == null)
				throw new ArgumentNullException("variable");
			if (origin == null)
				origin = new int[variable.Rank];
			if (data == null)
				throw new ArgumentNullException("data");

			this.var = variable;
			this.origin = origin;
			this.stride = stride;
			this.data = data;
			this.version = version;
			this.exception = null;
		}

		/// <summary>
		/// Use on failure.
		/// </summary>
		public VariableResponse(Variable variable, int[] origin, int[] stride, Exception exception)
		{
			this.var = variable;
			this.origin = origin;
			this.stride = stride;
			this.data = null;
			this.version = -1;
			this.exception = exception;
		}

		/// <summary>
		/// Gets the value indicating whether the operation has been successful or not.
		/// </summary>
		public bool IsSuccess
		{
			get { return exception == null; }
		}

		/// <summary>
		/// Gets the exception describing failure in case when <see cref="IsSuccess"/> property is false.
		/// </summary>
		public Exception Exception
		{
			get { return exception; }
		}

		/// <summary>
		/// Gets the variable that is the target of the request.
		/// </summary>
		public Variable Variable { get { return var; } }

		/// <summary>
		/// Gets the origin of the requested region.
		/// </summary>
		public int[] Origin { get { return origin; } }

		/// <summary>
		/// Gets the stride of the requested region.
		/// </summary>
		/// <remarks>
		/// If the request has had no stride, gets null.
		/// </remarks>
		public int[] Stride { get { return stride; } }

		/// <summary>
		/// Gets the shape of the requested region.
		/// </summary>
		public int[] Shape
		{
			get
			{
				if (!IsSuccess)
					throw new NotSupportedException("Operation failed and shape is unknown");
                if (data.Length == 0)
                    return new int[var.Rank];

				int[] shape = new int[origin.Length];
				for (int i = 0; i < shape.Length; i++)
				{
					shape[i] = data.GetLength(i);
				}
				return shape;
			}
		}

		/// <summary>
		/// Gets the requested data.
		/// </summary>
		public Array Data
		{
			get { return data; }
		}

		/// <summary>Gets version number this response is originating from.</summary>
		public int Version
		{
			get { return version; }
		}
	}
}


