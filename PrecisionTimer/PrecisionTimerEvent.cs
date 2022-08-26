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
    /// Long Running <see cref="PrecisionTimerEvent"/> that procs every 1ms forever and can be subscribed to with <see cref="PrecisionTimerEvent.Tick"/> += Method();
    /// <para>Callback contains only an <see cref="EventHandler"/></para>
    /// <para>See <see cref="PrecisionTimerEvent.Stop"/> when you want to stop the long running event</para>
    /// </summary>
    public class PrecisionTimerEvent : IDisposable
    {
        /// <summary>
        /// Occurs when the <see cref="PrecisionTimerEvent"/> has started.
        /// </summary>
        public volatile EventHandler Started;

        /// <summary>
        /// Occurs when the <see cref="PrecisionTimerEvent"/> has stopped.
        /// </summary>
        public volatile EventHandler Stopped;

        /// <summary>
        /// Occurs when the <see cref="PrecisionTimerEvent"/> Ticks, this occurs once every millisecond
        /// </summary>
        public volatile EventHandler Tick;

        #region [ Members ]

        private volatile int m_timerID;                  // Timer identifier.
        private volatile bool m_running;                 // Indicates whether or not the timer is running.
        private volatile bool m_disposed;                // Indicates whether or not the timer has been disposed.
        private volatile EventArgs m_eventArgs;          // Private user event args to pass into Ticks call

        #endregion [ Members ]

        #region [ Constructors / Destructor ]

        /// <summary>
        /// Create a PrecisionTimerEvent and tell the Garbage Collector to leave it alone
        /// </summary>
        public PrecisionTimerEvent()
        {
            GC.SuppressFinalize(this);
            timeGetDevCaps(ref Capabilities, Marshal.SizeOf(Capabilities));
            m_running = false;
        }

        /// <summary>
        /// Deconstruct
        /// </summary>
        ~PrecisionTimerEvent()
        {
            Dispose(false);
            GC.Collect();
        }

        #endregion [ Constructors / Destructor ]

        #region [ Callbacks ]

        internal void TimerCallbackEventOnly(int hwnd, int uMsg, IntPtr passThePointer, int dwTime, int WTF)
        {
            if (Tick is object)
            {
                Tick(this, m_eventArgs);
            }
        }

        #endregion [ Callbacks ]

        /// <summary>
        /// True if the Timer is running
        /// Will also be false if no <see cref="PrecisionTimerEvent"/> is configured
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">Timer was already disposed</exception>
        public bool IsRunning()
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException(Constants.disposedIsRunningString);
            }

            return m_running;
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

            return m_eventArgs;
        }

        /// <summary>
        /// Set the User supplied EventArgs
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ObjectDisposedException">Timer was already disposed</exception>
        /// <exception cref="LongRunningTimerException">Attempted to configure a <see cref="PrecisionTimerEvent"/> that was running</exception>
        public void SetEventArgs(EventArgs value)
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException(Constants.disposedEventArgsString);
            }

            if (m_running)
            {
                throw new LongRunningTimerException(Constants.timerEventRunningString);
            }

            if (value != null)
            {
                m_eventArgs = value;
            }
        }

        /// <summary>
        /// Creates a Long Running <see cref="PrecisionTimerEvent"/> that procs every 1ms forever and can be subscribed to with <see cref="PrecisionTimerEvent.Tick"/> += Method();
        /// <para>Callback contains only an <see cref="EventHandler"/></para>
        /// <para>See <see cref="PrecisionTimerEvent.Stop"/> when you want to stop the long running event</para>
        /// </summary>
        public void Start()
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException(Constants.disposedStartString);
            }

            if (m_running)
            {
                return;
            }

            // Create and start timer.
            m_timerID = timeSetEvent(1, 0, TimerCallbackEventOnly, IntPtr.Zero, TimerMode.Periodic);

            // If the timer was created successfully.
            if (m_timerID != 0)
            {
                m_running = true;

                if (Started is object)
                {
                    Started(this, null);
                }

                return;
            }
            else
            {
                throw new TimerStartException(Constants.unableToStartString);
            }
        }

        /// <summary>
        /// Stop the Long Running <see cref="PrecisionTimerEvent"/>
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

            timeKillEvent(m_timerID);

            m_timerID = 0;
            m_running = false;

            if (Stopped is object)
            {
                Stopped(this, null);
            }
        }

        /// <summary>
        /// <para>DESTROY</para>
        /// Release all resources for this <see cref="PrecisionTimerEvent"/>
        /// <para>If you call Dispose when you intend to create a new Timer you will have a bad time..r</para>
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
                    m_eventArgs = null;

                    GC.ReRegisterForFinalize(this);
                }
            }
            finally
            {
                m_disposed = true;
            }
        }
    }
}
