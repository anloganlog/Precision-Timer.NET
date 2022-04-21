//******************************************************************************************************
//  Copyright © 2022, S Christison. No Rights Reserved.
//
//  Licensed to [You] under one or more License Agreements.
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//
//******************************************************************************************************

using System;
using System.Runtime.InteropServices;
using static PrecisionTiming.MMTimerExports;

namespace PrecisionTiming
{
    internal class MMTimer : IDisposable
    {
        #region [ Members ]

        internal event EventHandler Tick;

        private volatile IntPtr userTimerReference = IntPtr.Zero; // We MUST hold a reference to this timer

        private int m_timerID;                  // Timer identifier.
        private readonly TimerProc m_timeProc;  // Called by Windows when a timer periodic event occurs.
        private TimerMode m_mode;               // Timer mode.
        private int m_period;                   // Period between timer events in milliseconds.
        private int m_resolution;               // Timer resolution in milliseconds.
        private bool m_running;                 // Indicates whether or not the timer is running.
        private bool m_disposed;                // Indicates whether or not the timer has been disposed.
        private EventArgs m_eventArgs;          // Private user event args to pass into Ticks call

        #endregion [ Members ]

        #region [ Constructors / Destructor ]

        internal MMTimer()
        {
            timeGetDevCaps(ref Capabilities, Marshal.SizeOf(Capabilities));

            // Initialize timer with default values.
            m_mode = TimerMode.Periodic;
            m_running = false;
            m_timeProc = TimerEventCallback;
            m_period = Capabilities.PeriodMinimum;
            m_resolution = 1;
        }

        ~MMTimer()
        {
            Dispose(false);
        }

        internal bool IsRunning => m_running;
        internal EventArgs EventArgs => m_eventArgs;
        internal int Period
        {
            get
            {
                if (m_disposed)
                    throw new ObjectDisposedException("PrecisionTimer");

                return m_period;
            }
            set
            {
                if (m_disposed)
                    throw new ObjectDisposedException("PrecisionTimer");

                if (value < Capabilities.PeriodMinimum || value > Capabilities.PeriodMaximum)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Multimedia Timer period out of range");

                m_period = value;
            }
        }
        internal int Resolution
        {
            get
            {
                if (m_disposed)
                    throw new ObjectDisposedException("PrecisionTimer");

                return m_resolution;
            }
            set
            {
                if (m_disposed)
                    throw new ObjectDisposedException("PrecisionTimer");

                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Multimedia timer resolution out of range");

                m_resolution = value;
            }
        }
        internal bool AutoReset
        {
            get
            {
                if (m_disposed)
                    throw (new ObjectDisposedException("PrecisionTimer"));

                return (m_mode == TimerMode.Periodic);
            }
            set
            {
                if (m_disposed)
                    throw (new ObjectDisposedException("PrecisionTimer"));

                m_mode = (value ? TimerMode.Periodic : TimerMode.OneShot);
            }
        }

        public void Dispose()
        {
            if (!m_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            else
            {
                throw new ObjectDisposedException("Tried to Dipose PrecisionTimer when it was already Disposed");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (IsRunning)
                            Stop();
                    }
                }
                finally
                {
                    m_disposed = true;
                }
            }
            else
            {
                throw new ObjectDisposedException("Tried to Dipose PrecisionTimer when it was already Disposed");
            }
        }

        internal bool Start(EventArgs userArgs = null)
        {

            if (m_disposed)
                throw new ObjectDisposedException("Attempted to Start PrecisionTimer when it was already Disposed");

            if (m_running)
                return false;

            if (userArgs == null) { userArgs = EventArgs.Empty; }

            // Cache user event args to pass into Ticks parameter
            m_eventArgs = userArgs;

            // Create and start timer.
            m_timerID = timeSetEvent(m_period, m_resolution, m_timeProc, userTimerReference, m_mode);

            // If the timer was created successfully.
            if (m_timerID != 0)
            {
                m_running = true;
                return true;
            }
            else
            {
                throw new TimerStartException("Unable to start the Precision Timer");
            }
        }

        internal void Stop()
        {
            if (m_disposed)
                throw new ObjectDisposedException("Attempted to Stop PrecisionTimer when it was already Disposed");

            if (!m_running)
                return;

            try
            {
                timeKillEvent(m_timerID);
                m_timerID = 0;
                m_running = false;
            }
            catch { }
        }

        internal void TimerEventCallback(int hwnd, int uMsg, IntPtr idEvent, int dwTime, int WTF)
        {
            if (Tick is object)
            {
                // Pass the Parcel
                userTimerReference = idEvent;
                try
                {
                    // Actual Tick Event
                    Tick(this, m_eventArgs);
                }
                catch
                {
                    throw new TimerTaskFailed("Timer Task Failed [Inner]");
                }
            }

            if (m_mode == TimerMode.OneShot)
            {
                Stop();
                return;
            }

            // Tells the Garbage Collector to leave ALL PrecisionTimer objects alone.
            GC.KeepAlive(this);

            return;
        }

        #endregion [ Methods ]
    }
}