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
    /// <summary>
    /// High Resolution Multimedia Timer Wrapper
    /// <para>The default timer will Fire the Tick event every 1 Millisecond forever</para>
    /// <para>Subscribe to <see cref="EventHandler"/> <see cref="Tick"/></para>
    /// <para>You can also provide an Action using <see cref="SetAction"/> or <see cref="SetInterval(Action, int, bool, bool, int)"/></para>
    /// </summary>
    public class PrecisionTimer : IDisposable
    {
        /// <summary>
        /// Occurs when the <see cref="PrecisionTimer"/> has started.
        /// </summary>
        public volatile EventHandler Started;

        /// <summary>
        /// Occurs when the <see cref="PrecisionTimer"/> has stopped.
        /// </summary>
        public volatile EventHandler Stopped;

        /// <summary>
        /// Occurs when the <see cref="PrecisionTimer"/> Ticks at the Interval.
        /// </summary>
        public volatile EventHandler Tick;

        #region [ Members ]

        private volatile int m_timerID;                  // Timer identifier.
        private volatile int m_period;                   // Period between timer events in milliseconds.
        private volatile int m_resolution;               // Timer resolution in milliseconds.
        private volatile bool m_running;                 // Indicates whether or not the timer is running.
        private volatile bool m_disposed;                // Indicates whether or not the timer has been disposed.
        private volatile object m_lock = new object();   // Lock
        private volatile EventArgs m_eventArgs;          // Private user event args to pass into Ticks call
        private volatile Action m_action = null;         // The Action
        private volatile TimerProc m_timeProc;           // Called by Windows when a timer periodic event occurs.
        private volatile TimerMode m_mode;               // Timer mode.

        #endregion [ Members ]

        #region [ Constructors / Destructor ]

        /// <summary>
        /// Create a PrecisionTimer and tell the Garbage Collector to leave it alone
        /// </summary>
        public PrecisionTimer()
        {
            GC.SuppressFinalize(this);
            timeGetDevCaps(ref Capabilities, Marshal.SizeOf(Capabilities));

            // Initialize timer with default values.
            m_mode = TimerMode.Periodic;
            m_running = false;
            m_period = Capabilities.PeriodMinimum;
            m_resolution = Capabilities.PeriodMinimum;
        }

        /// <summary>
        /// Deconstruct
        /// </summary>
        ~PrecisionTimer()
        {
            Dispose(false);
            GC.Collect();
        }

        #endregion [ Constructors / Destructor ]

        #region [ Callbacks ]

        internal void TimerCallback(int hwnd, int uMsg, IntPtr passThePointer, int dwTime, int WTF)
        {
            if (m_action != null)
            {
                m_action();
            }

            if (Tick is object)
            {
                Tick(this, m_eventArgs);
            }
        }

        internal void TimerCallbackOneShot(int hwnd, int uMsg, IntPtr passThePointer, int dwTime, int WTF)
        {
            lock (m_lock)
            {
                if (m_action != null)
                {
                    m_action();
                }

                if (Tick is object)
                {
                    Tick(this, m_eventArgs);
                }

                Stop();
            }
        }

        #endregion [ Callbacks ]

        /// <summary>
        /// True if the Timer is running
        /// Will also be false if no <see cref="PrecisionTimer"/> is configured
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">Timer was already disposed</exception>
        public bool IsRunning()
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException(Constants.disposedIsRunningString);
            }

            lock (m_lock)
            {
                return m_running;
            }
        }

        /// <summary>
        /// Get the Current Interval (Period)
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">Timer was already disposed</exception>
        public int GetPeriod()
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException(Constants.disposedPeriodString);
            }

            lock (m_lock)
            {
                return m_period;
            }
        }

        /// <summary>
        /// Set the Current Interval (Period)
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ObjectDisposedException">Timer was already disposed</exception>
        /// <exception cref="TimerRunningException">Attempted to configure a timer that was running</exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetPeriod(int value)
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException(Constants.disposedPeriodString);
            }

            if (m_running)
            {
                throw new TimerRunningException(Constants.timerRunningString);
            }

            if (value > Capabilities.PeriodMaximum)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, Constants.outOfRangeMaxPeriodString + Capabilities.PeriodMaximum);
            }

            if (value < Capabilities.PeriodMinimum)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, Constants.outOfRangeMinPeriodString + Capabilities.PeriodMinimum);
            }

            lock (m_lock)
            {
                m_period = value;
            }
        }

        /// <summary>
        /// Get the Current Resolution
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">Timer was already disposed</exception>
        public int GetResolution()
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException(Constants.disposedResolutionString);
            }

            lock (m_lock)
            {
                return m_resolution;
            }
        }

        /// <summary>
        /// Set the Current Resolution
        /// </summary>
        /// <param name="value">Desired resolution in milliseconds</param>
        /// <exception cref="ObjectDisposedException">Timer was already disposed</exception>
        /// <exception cref="TimerRunningException">Attempted to configure a timer that was running</exception>
        /// <exception cref="ArgumentOutOfRangeException">Desired resolution is out of the range of the devcaps</exception>
        public void SetResolution(int value)
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException(Constants.disposedResolutionString);
            }

            if (m_running)
            {
                throw new TimerRunningException(Constants.timerRunningString);
            }

            if (value > Capabilities.PeriodMaximum)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, Constants.outOfRangeMaxResolutionString + Capabilities.PeriodMaximum);
            }

            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, Constants.outOfRangeMinResolutionString);
            }

            lock (m_lock)
            {
                m_resolution = value;
            }
        }

        /// <summary>
        /// Get the Current User supplied EventArgs
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">Timer was already disposed</exception>
        public EventArgs GetEventArgs()
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException(Constants.disposedEventArgsString);
            }

            lock (m_lock)
            {
                return m_eventArgs;
            }
        }

        /// <summary>
        /// Set the User supplied EventArgs
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ObjectDisposedException">Timer was already disposed</exception>
        /// <exception cref="PeriodicTimerRunningException">Attempted to configure a pediodic timer that was running</exception>
        public void SetEventArgs(EventArgs value)
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException(Constants.disposedEventArgsString);
            }

            if (m_mode == TimerMode.Periodic && m_running)
            {
                throw new PeriodicTimerRunningException(Constants.timerRunningString);
            }

            if (value != null)
            {
                lock (m_lock)
                {
                    m_eventArgs = value;
                }
            }
        }

        /// <summary>
        /// Get the Current Timer Auto Reset Mode (Periodic/Oneshot)
        /// <para>True for periodic</para>
        /// <para>False for one-shot</para>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">Timer was already disposed</exception>
        public bool GetAutoResetMode()
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException(Constants.disposedAutoResetString);
            }

            lock (m_lock)
            {
                return m_mode == TimerMode.Periodic;
            }
        }

        /// <summary>
        /// Set the Current Timer Auto Reset Mode (Periodic/Oneshot)
        /// <para>True for periodic</para>
        /// <para>False for one-shot</para>
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ObjectDisposedException">Timer was already disposed</exception>
        /// <exception cref="TimerRunningException">Attempted to configure a timer that was running</exception>
        public void SetAutoResetMode(bool value)
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException(Constants.disposedAutoResetString);
            }

            if (m_running)
            {
                throw new TimerRunningException(Constants.timerRunningString);
            }

            lock (m_lock)
            {
                m_mode = value ? TimerMode.Periodic : TimerMode.OneShot;
            }
        }

        /// <summary>
        /// Set the Action of the <see cref="PrecisionTimer"/> before you Start the Timer
        /// <para>You could also subscribe to the Event <see cref="Tick"/></para>
        /// </summary>
        /// <param name="TimerTask">The Action</param>
        /// <exception cref="ObjectDisposedException">Timer was already disposed</exception>
        /// <exception cref="PeriodicTimerRunningException">Attempted to configure a pediodic timer that was running</exception>
        public void SetAction(Action TimerTask)
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException(Constants.disposedAutoResetString);
            }

            if (m_mode == TimerMode.Periodic && m_running)
            {
                throw new PeriodicTimerRunningException(Constants.timerRunningString);
            }

            lock (m_lock)
            {
                m_action = TimerTask;
            }
        }

        /// <summary>
        /// Set the Interval and Tick Event of the <see cref="PrecisionTimer"/> and decide if it should start automatically
        /// <para>Configures the timer with the default settings unless you change them</para>
        /// <para>If you call this with <see href="start = true"/> on a timer that is already running then it will restart, you can check <see cref="IsRunning()"/> to be sure if you don't want this to happen</para>
        /// </summary>
        /// <param name="timerTask">The Action</param>
        /// <param name="interval">The Interval for the TimerTask in Milliseconds</param>
        /// <param name="periodic">True if Periodic / False if OneShot</param>
        /// <param name="resolution">Resolution for Events in milliseconds - 0 is best available on platform - 1 uses even less CPU</param>
        /// <param name="start">True if the timer should start automatically with the default settings, false if you are going to configure/start it later</param>
        /// <exception cref="TimerStartException">Timer failed to start</exception>
        /// <exception cref="ObjectDisposedException">Timer was already disposed</exception>
        public void SetInterval(Action timerTask = null, int interval = 1, bool start = true, bool periodic = true, int resolution = 0)
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException(Constants.disposedStartString);
            }

            lock (m_lock)
            {
                m_action = timerTask;
                m_mode = periodic ? TimerMode.Periodic : TimerMode.OneShot;
                m_resolution = resolution;
                m_period = interval;

                if (start)
                {
                    if (!m_running)
                    {
                        Start();
                    }
                    else
                    {
                        // Restart
                        Stop();
                        Start();
                    }
                }
            }
        }

        /// <summary>
        /// Start the <see cref="PrecisionTimer"/>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">Timer was already disposed</exception>
        /// <exception cref="TimerStartException">Timer failed to start</exception>
        public bool Start()
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException(Constants.disposedStartString);
            }

            if (m_running)
            {
                return true;
            }

            lock (m_lock)
            {
                if (m_mode == TimerMode.Periodic)
                {
                    m_timeProc = TimerCallback;
                }
                else
                {
                    m_timeProc = TimerCallbackOneShot;
                }

                // Create and start timer.
                m_timerID = timeSetEvent(m_period, m_resolution, m_timeProc, IntPtr.Zero, m_mode);

                // If the timer was created successfully.
                if (m_timerID != 0)
                {
                    m_running = true;

                    if (Started is object)
                    {
                        Started(this, null);
                    }

                    return true;
                }
                else
                {
                    throw new TimerStartException(Constants.unableToStartString);
                }
            }
        }

        /// <summary>
        /// Stop the <see cref="PrecisionTimer"/>
        /// </summary>
        /// <exception cref="ObjectDisposedException">Timer was already Disposed</exception>
        public void Stop()
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException(Constants.disposedStopString);
            }

            if (!m_running)
            {
                return;
            }

            lock (m_lock)
            {
                timeKillEvent(m_timerID);

                m_timerID = 0;
                m_running = false;

                if (Stopped is object)
                {
                    Stopped(this, null);
                }
            }
        }

        /// <summary>
        /// <para>DESTROY</para>
        /// Release all resources for this <see cref="PrecisionTimer"/>
        /// </summary>
        /// <exception cref="ObjectDisposedException">Timer was already Diposed</exception>
        public void Dispose()
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException(Constants.disposedString);
            }

            Dispose(true);
        }

        protected private virtual void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (m_running)
                    {
                        Stop();
                    }

                    Started = null;
                    Stopped = null;
                    Tick = null;

                    m_timerID = 0;
                    m_period = 0;
                    m_resolution = 0;
                    m_timeProc = null;
                    m_eventArgs = null;
                    m_action = null;
                    m_lock = null;

                    GC.ReRegisterForFinalize(this);
                }
            }
            finally
            {
                m_disposed = true;
            }
        }

        #region [ Overloads ]

        /// <inheritdoc cref="SetInterval(Action, int, bool, bool, int)"/>
        public void Configure(Action timerTask = null, int interval = 1, bool start = true, bool periodic = true, int resolution = 0) => SetInterval(timerTask, interval, start, periodic, resolution);

        /// <inheritdoc cref="GetPeriod"/>
        public void GetInterval() => GetPeriod();

        /// <inheritdoc cref="SetPeriod(int)"/>
        public void SetInterval(int value) => SetPeriod(value);

        #endregion [ Overloads ]
    }
}
