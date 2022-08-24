/*
*MIT License
*
*Copyright (c) 2022 S Christison
*
*Permission is hereby granted, free of charge, to any person obtaining a copy
*of this software and associated documentation files (the "Software"), to deal
*in the Software without restriction, including without limitation the rights
*to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
*copies of the Software, and to permit persons to whom the Software is
*furnished to do so, subject to the following conditions:
*
*The above copyright notice and this permission notice shall be included in all
*copies or substantial portions of the Software.
*
*THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
*IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
*FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
*AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
*LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
*OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
*SOFTWARE.
*/

using System;
using System.Runtime.InteropServices;
using static PrecisionTiming.MMTimerExports;

namespace PrecisionTiming
{
    internal class MMTimer : IDisposable
    {
        #region [ Members ]

        internal EventHandler Started;
        internal EventHandler Stopped;

        internal EventHandler Tick;

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
            m_resolution = Capabilities.PeriodMinimum;
        }

        ~MMTimer()
        {
            Dispose(false);
        }

        public bool IsRunning => m_running;
        public EventArgs EventArgs => m_eventArgs;

        /// <summary>
        /// Get the Current Interval (Period)
        /// </summary>
        internal int GetPeriod
        {
            get
            {
                if (m_disposed)
                    throw new ObjectDisposedException("PrecisionTimer");

                return m_period;
            }
        }

        /// <summary>
        /// Set the Current Interval (Period)
        /// </summary>
        internal int SetPeriod
        {
            set
            {
                if (m_disposed)
                    throw new ObjectDisposedException("PrecisionTimer");

                if (value > Capabilities.PeriodMaximum)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Multimedia Timer period out of range, max value is: " + Capabilities.PeriodMaximum);
                }

                if (value < Capabilities.PeriodMinimum)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Multimedia Timer period out of range, min value is: " + Capabilities.PeriodMinimum);
                }

                m_period = value;
            }
        }

        /// <summary>
        /// Get the Current Resolution
        /// </summary>
        internal int GetResolution
        {
            get
            {
                if (m_disposed)
                    throw new ObjectDisposedException("PrecisionTimer");

                return m_resolution;
            }
        }

        /// <summary>
        /// Set the Current Resolution
        /// </summary>
        internal int SetResolution
        {
            set
            {
                if (m_disposed)
                    throw new ObjectDisposedException("PrecisionTimer");

                if (value > Capabilities.PeriodMaximum)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Multimedia Timer resolution out of range, max value is: " + Capabilities.PeriodMaximum);
                }

                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Multimedia Timer resolution out of range, min value is 0");
                }

                m_resolution = value;
            }
        }

        /// <summary>
        /// Get the Current EventArgs
        /// </summary>
        internal EventArgs GetArgs
        {
            get
            {
                if (m_disposed)
                    throw new ObjectDisposedException("PrecisionTimer");

                return m_eventArgs;
            }
        }

        /// <summary>
        /// Set the Current EventArgs
        /// </summary>
        internal EventArgs SetArgs
        {
            set
            {
                if (m_disposed)
                    throw new ObjectDisposedException("PrecisionTimer");

                if (value != null)
                {
                    m_eventArgs = value;
                }
            }
        }

        /// <summary>
        /// Get the Current Timer Reset Mode (Periodic/Oneshot)
        /// </summary>
        internal bool GetAutoReset
        {
            get
            {
                if (m_disposed)
                    throw (new ObjectDisposedException("PrecisionTimer"));

                return (m_mode == TimerMode.Periodic);
            }
        }

        /// <summary>
        /// Set the Current Timer Reset Mode (Periodic/Oneshot)
        /// </summary>
        internal bool SetAutoReset
        {
            set
            {
                if (m_disposed)
                    throw (new ObjectDisposedException("PrecisionTimer"));

                m_mode = (value ? TimerMode.Periodic : TimerMode.OneShot);
            }
        }

        internal bool Start(EventArgs userArgs = null)
        {
            if (m_disposed)
                throw new ObjectDisposedException("Attempted to Start PrecisionTimer when it was already Disposed");

            if (m_running)
                return false;

            if (userArgs == null) { userArgs = EventArgs.Empty; }

            // Cache user event args to pass into Ticks parameter if it wasn't already set
            if (m_eventArgs == null)
            {
                m_eventArgs = userArgs;
            }

            // Create and start timer.
            m_timerID = timeSetEvent(m_period, m_resolution, m_timeProc, userTimerReference, m_mode);

            // If the timer was created successfully.
            if (m_timerID != 0)
            {
                m_running = true;
                if (Started is object)
                    Started(this, null);
                return true;
            }
            else
            {
                throw new TimerStartException("Unable to start the Precision Timer");
            }
        }

        /// <summary>
        /// Stop the Timer
        /// </summary>
        /// <exception cref="ObjectDisposedException">Timer was already Disposed</exception>
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
                if (Stopped is object)
                    Stopped(this, null);
                m_running = false;

                Stopped = null;
                Started = null;
                Tick = null;
            }
            catch { }
        }

        /// <summary>
        /// Dispose the Timer
        /// </summary>
        /// <exception cref="ObjectDisposedException">Timer was already Diposed</exception>
        public void Dispose()
        {
            if (!m_disposed)
            {
                Tick = null;
                Started = null;
                Stopped = null;
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

            // Tells the Garbage Collector to leave ALL PrecisionTimer objects alone.
            GC.KeepAlive(this);

            if (m_mode == TimerMode.OneShot)
            {
                Stop();
            }

            return;
        }

        #endregion [ Constructors / Destructor ]
    }
}
