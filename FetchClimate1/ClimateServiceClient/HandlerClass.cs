// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Microsoft.Research.Science.Data
{
	internal sealed class OnCommittedHandler
	{
		private string hash;
		private EventWaitHandle waitHandle;
		private Action<DataSetCommittedEventArgs, OnCommittedHandler> CustomHandler;

		public OnCommittedHandler(EventWaitHandle waitHandle, Action<DataSetCommittedEventArgs, OnCommittedHandler> handler)
		{
			this.waitHandle = waitHandle;
			this.CustomHandler = handler;
		}

		public void Handler(object sender, DataSetCommittedEventArgs arg)
		{
			if (CustomHandler != null)
				CustomHandler(arg, this);
		}

		public string RequestHash
		{
			get { return hash; }
		}

		public EventWaitHandle Completed
		{
			get { return waitHandle; }
		}
	}
}

