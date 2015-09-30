using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Microsoft.Research.Science.Data.Climate.Service
{
    /// <summary>
    /// Semaphore with prioritization. Provides thread safe functionality for resource managing.
    /// </summary>
    public class PrioritySemaphore : IDisposable
    {
        private volatile int currentTicket;
        private volatile int processingCount;
        private Dictionary<int, Queue<int>> ticketsDictionary;
        private object monitor;

        private bool disposed = false;

        Dictionary<int, AutoResetEvent> ticketsLocks;

        private int resourcesCount;

        /// <summary>
        /// Gets total amount of resources.
        /// </summary>
        public int ResourcesCount
        {
            get { return this.resourcesCount; }
        }

        /// <summary>
        /// Gets amount of processing resources.
        /// </summary>
        public int ProcessingCount
        {
            get { return this.processingCount; }
        }

        /// <summary>
        /// Creates new instance of <see cref="PrioritySemaphore"/>.
        /// </summary>
        /// <param name="resourcesCount">Amount of available resources.</param>
        public PrioritySemaphore(int resourcesCount)
        {
            this.ticketsDictionary = new Dictionary<int, Queue<int>>();
            this.ticketsLocks = new Dictionary<int, AutoResetEvent>();
            this.monitor = new object();

            this.resourcesCount = resourcesCount;
            this.processingCount = 0;
            this.currentTicket = 0;
        }

        /// <summary>
        /// Waits for available resource.
        /// </summary>
        /// <param name="priority">
        /// Priority of request. 
        /// The higher is priority, the sooner resource will be achieved.
        /// </param>
        public void WaitOne(int priority)
        {
            AutoResetEvent ticketLock = null;

            lock (monitor)
            {
                if (processingCount < resourcesCount)
                {
                    processingCount++;
                    return;
                }
                else
                {
                    int ticket = currentTicket++;

                    if (!this.ticketsDictionary.ContainsKey(priority))
                        this.ticketsDictionary[priority] = new Queue<int>();
                    this.ticketsDictionary[priority].Enqueue(ticket);

                    ticketLock = new AutoResetEvent(false);
                    this.ticketsLocks[ticket] = ticketLock;
                }
            }

            if (ticketLock != null)
            {
                ticketLock.WaitOne();
                ticketLock.Dispose();
            }
        }

        /// <summary>
        /// Releases resource.
        /// </summary>
        public void Set()
        {
            int ticket = -1;

            lock (monitor)
            {
                foreach (var queue in ticketsDictionary.OrderByDescending(x => x.Key).Select(x => x.Value))
                {
                    if (queue.Count != 0)
                    {
                        ticket = queue.Dequeue();
                        break;
                    }
                }
                if (ticket < 0)
                {
                    processingCount--;
                }
            }

            if (ticket >= 0)
            {
                var ticketLock = ticketsLocks[ticket];
                ticketLock.Set();
                ticketsLocks.Remove(ticket);
            }
        }

        /// <summary>
        /// Disposes this <see cref="PrioritySemaphore"/> instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if(!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if(disposing)
                {
                    foreach (var tickLock in this.ticketsLocks.Values)
                    {
                        tickLock.Dispose();
                    }
                }
                // Note disposing has been done.
                disposed = true;
            }
        }
    }
}
