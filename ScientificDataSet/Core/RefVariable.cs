// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Microsoft.Research.Science.Data
{
	/// <summary>
	/// RefVariable is a special kind of variables whose purpose is to refer another variable.
	/// <remarks>
	/// It enables adding of a variable to not native data set.
	/// Data operation applied to the reference are translated to the target variable.
	/// Both target and reference variables share the data changing as the change of its inner state.
	/// Both target and reference variables share the metadata collection.
	/// Names of dimensions might change independently and don't cause another variable to be changed.
	/// RefVariable allows to extend a collection of coordinate systems of the target variable with its own coordinate system.
	/// </remarks>
	/// </summary>
	internal sealed class RefVariable<DataType> : Variable<DataType>, IRefVariable, IDataRequestable
	{
		private Variable<DataType> refVariable;

		public RefVariable(DataSet sds, Variable<DataType> variable, string[] dimensions)
			: base(sds, dimensions)
		{
			refVariable = variable;
			refVariable.Changed += new VariableChangedEventHandler(RefVariableChanged);
			refVariable.RolledBack += new VariableRolledBackEventHandler(RefVariableRolledBack);
			//refVariable.CoordinateSystemAdded += new CoordinateSystemAddedEventHandler(RefVariableCoordinateSystemAdded);

			this.ID = ~this.ID;

			Initialize();
		}

		public RefVariable(DataSet sds, Variable<DataType> variable)
			: this(sds, variable, variable.Dimensions.AsNamesArray())
		{
		}

		/// <summary>
		/// Gets the refenced variable instance.
		/// </summary>
		public Variable<DataType> ReferencedVariable
		{
			get { return refVariable; }
		}

		/// <summary>
		/// Gets the shared metadata collection.
		/// </summary>
		public override MetadataDictionary Metadata
		{
			get { return refVariable.Metadata; }
		}

		protected internal override void CheckOnAddCoordinateSystem(CoordinateSystem cs)
		{
			// Disabled for Release 1.0
			// It is allowed to add coordinate systems from the data set of referenced variable.
			//if (cs.DataSet == refVariable.DataSet)
			//    return;

			base.CheckOnAddCoordinateSystem(cs);
		}

		private void RefVariableCoordinateSystemAdded(object sender, CoordinateSystemAddedEventArgs e)
		{
			// Disabled for Release 1.0
			//AddCoordinateSystem(e.CoordinateSystem);
		}

		private void RefVariableRolledBack(object sender, VariableRolledBackEventArgs e)
		{
			Rollback();
		}

		private void RefVariableChanged(object sender, VariableChangedEventArgs e)
		{
			if (e.Action == VariableChangeAction.PutData)
			{
				StartChanges();
				changes.Shape = e.Changes.Shape;
				changes.AffectedRectangle = e.Changes.AffectedRectangle;

				FireEventVariableChanged(VariableChangeAction.PutData);
			}
		}

		internal protected override void OnRollback(Variable.Changes proposedChanges)
		{
			try
			{
				refVariable.RolledBack -= RefVariableRolledBack;
				refVariable.Rollback(); 
			}
			finally
			{
				refVariable.RolledBack += RefVariableRolledBack;
			}

            if (Version == 0) // the variable is added and rolled back
            {
                refVariable.Changed -= RefVariableChanged;
                refVariable.RolledBack -= RefVariableRolledBack;
            }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="a"></param>
		public override void PutData(int[] origin, Array a)
		{
			refVariable.PutData(origin, a);
		}

		public override void Append(Array a, int dimToAppend)
		{
			refVariable.Append(a, dimToAppend);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected override int[] ReadShape()
		{
			return refVariable.GetShape();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="shape"></param>
		/// <returns></returns>
		public override Array GetData(int[] origin, int[] shape)
		{
			return refVariable.GetData(origin, shape);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return ToString("@");
		}

		#region Explicit implementation of IRefVariable

		Variable IRefVariable.ReferencedVariable
		{
			get { return refVariable; }
		}


		/// <summary>
		/// Updates the reference variable on the basis of custom changes of
		/// its referenced variable.
		/// </summary>
		/// <param name="customChanges"></param>
		void IRefVariable.UpdateChanges(Variable.Changes customChanges)
		{
			StartChanges();
			changes.Shape = customChanges.Shape;
			changes.AffectedRectangle = customChanges.AffectedRectangle;
			changes.MetadataChanges = customChanges.MetadataChanges;

			FireEventVariableChanged(VariableChangeAction.PutData);
		}

		#endregion

		#region IDataRequestable Members

		void IDataRequestable.RequestData(int[] origin, int[] shape, VariableResponseHandler responseHandler)
		{
			((IDataRequestable)this).RequestData(origin, null, shape, responseHandler);
		}

		void IDataRequestable.RequestData(int[] origin, int[] stride, int[] shape, VariableResponseHandler responseHandler)
		{
			if (this.refVariable is IDataRequestable)
			{
				VariableResponseHandler myResponseHandler = new VariableResponseHandler(
					delegate(VariableResponse resp)
					{
						VariableResponse response = new VariableResponse(this, origin, stride, resp.Data, Version);
						responseHandler(response);
					});

				((IDataRequestable)refVariable).RequestData(origin, stride, shape, myResponseHandler);
				return;
			}

			ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
				{
					try
					{
						Array a = GetData(origin, stride, shape);
						responseHandler(new VariableResponse(this, origin, stride, a, Version));
					}
					catch (Exception ex)
					{
						responseHandler(new VariableResponse(this, origin, stride, ex));
					}
				}));
		}

		#endregion
	}

	/// <summary>
	/// Provides non-generic access to the reference variable.
	/// </summary>
	public interface IRefVariable
	{
		/// <summary>
		/// Gets the referenced variable.
		/// </summary>
		Variable ReferencedVariable { get; }

		/// <summary>
		/// Updates the reference variable on the basis of custom changes of
		/// its referenced variable.
		/// </summary>
		/// <param name="changes"></param>
		void UpdateChanges(Variable.Changes changes);
	}
}


