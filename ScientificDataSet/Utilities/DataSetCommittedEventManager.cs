// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Microsoft.Research.Science.Data.Utilities
{
    public class DataSetCommittedEventManager : WeakEventManager
    {
        private DataSetCommittedEventManager()
        {
        }

        public static void AddListener(DataSet dataSet, IWeakEventListener listener)
        {
            DataSetCommittedEventManager.CurrentManager.ProtectedAddListener(dataSet, listener);
        }

        public static void RemoveListener(DataSet dataSet, IWeakEventListener listener)
        {
            DataSetCommittedEventManager.CurrentManager.ProtectedRemoveListener(dataSet, listener);
        }

        private void OnDataSetCommitted(object sender, DataSetCommittedEventArgs args)
        {
            base.DeliverEvent(sender, args);
        }

        protected override void StartListening(Object source)
        {
            DataSet dataSet = (DataSet)source;
            dataSet.Committed += this.OnDataSetCommitted;
        }

        protected override void StopListening(Object source)
        {
            DataSet dataSet = (DataSet)source;
            dataSet.Committed -= this.OnDataSetCommitted;
        }

        private static DataSetCommittedEventManager CurrentManager
        {
            get
            {
                Type managerType = typeof(DataSetCommittedEventManager);
                DataSetCommittedEventManager manager = (DataSetCommittedEventManager)WeakEventManager.GetCurrentManager(managerType);
                if (manager == null)
                {
                    manager = new DataSetCommittedEventManager();
                    WeakEventManager.SetCurrentManager(managerType, manager);
                }
                return manager;
            }
        }
    }
}

