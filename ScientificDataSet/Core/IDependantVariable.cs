// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data
{
	internal interface IDependantVariable
	{
		void UpdateChanges(DataSet.Changes changes);
	}
}

