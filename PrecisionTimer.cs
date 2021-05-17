using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace PrecisionTiming
{
    public class PrecisionTimer : IDisposable
    {
        #region [ Members ]

        // Defines constants for the multimedia Timer's event types.
        private enum TimerMode
        {
            OneShot, // Timer event occurs once.
            Periodic // Timer event occurs periodically.
        }

        // Represents the method that is called by Windows when a timer event occurs.
        private delegate void TimerProc(int hwnd, int uMsg, IntPtr idEvent, int dwTime, int WTF);

        /// <summary>
        /// Occurs when the <see cref="PrecisionTimer"/> has started.
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Occurs when the <see cref="PrecisionTimer"/> has stopped.
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Occurs when the <see cref="PrecisionTimer"/> period has elapsed.
        /// </summary>
        /// <remarks>
        /// This Event will be queued as a <see cref="Task"/> on the <see cref="ThreadPoolTaskScheduler"/>
        /// <para>If this Event throws an exception it will silently fail and block UI updates like all other windows timers.</para>
        /// <para>The timer itself may or may not keep going after an exception depending on the severity</para>
        /// <para><see cref="Task"/> also has this same "feature" so there is 2 layers blocking.</para>
        /// <para>You can still log exceptions with a Static logger inside the <see cref="Task"/></para>
        ///</remarks>
        public event EventHandler Tick;

        // Fields
        private volatile IntPtr userTimerReference = IntPtr.Zero; // We MUST hold a reference to this timer and update it when TimerEventCallback is triggered. (L340ish) // Volatile for speed the value is irrelevant

        private int m_timerID;                  // Timer identifier.
        private readonly TimerProc m_timeProc;  // Called by Windows when a timer periodic event occurs.
        private TimerMode m_mode;               // Timer mode.
        private int m_period;                   // Period between timer events in milliseconds.
        private int m_resolution;               // Timer resolution in milliseconds.
        private bool m_running;                 // Indicates whether or not the timer is running.
        private bool m_disposed;                // Indicates whether or not the timer has been disposed.
        private EventArgs m_eventArgs;          // Private user event args to pass into Ticks call

        #endregion [ Members ]

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="PrecisionTimer"/> class.
        /// </summary>
        public PrecisionTimer()
        {
            // Initialize timer with default values.
            m_mode = TimerMode.Periodic;
            m_running = false;

            m_timeProc = TimerEventCallback;
            m_period = Capabilities.PeriodMinimum;
            m_resolution = 1;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="PrecisionTimer"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~PrecisionTimer()
        {
            Dispose(false);
        }

        #endregion [ Constructors ]

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the time between <see cref="Tick"/> events, in milliseconds.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// If the timer has already been disposed.
        /// </exception>
        public int Period
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

                //if (IsRunning && m_mode == TimerMode.Periodic)
                //{
                //    Stop();
                //    Start(m_eventArgs);
                //}
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="PrecisionTimer"/> resolution, in milliseconds.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// If the timer has already been disposed.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The resolution is in milliseconds. The resolution increases  with smaller values;
        /// a resolution of 0 indicates periodic events should occur with the greatest possible
        /// accuracy. To reduce system  overhead, however, you should use the maximum value
        /// appropriate for your application.
        /// </para>
        /// <para>
        /// This property is currently ignored under Mono deployments.
        /// </para>
        /// </remarks>
        public int Resolution
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

                //if (IsRunning && m_mode == TimerMode.Periodic)
                //{
                //    Stop();
                //    Start(m_eventArgs);
                //}
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="PrecisionTimer"/> should raise the
        /// <see cref="Tick"/> event each time the specified period elapses or only after the first
        /// time it elapses.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>
        /// <c>true</c>true if the <see cref="PrecisionTimer"/> should raise the <see cref="Ticks"/>
        /// event each time the interval elapses; <c>false</c> if it should raise the event only once
        /// after the first time the interval elapses. The default is <c>true</c>.
        /// </returns>
        public bool AutoReset
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

                //if (IsRunning && m_mode == TimerMode.Periodic)
                //{
                //    Stop();
                //    Start(m_eventArgs);
                //}
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="PrecisionTimer"/> is running.
        /// </summary>
        public bool IsRunning => m_running;

        /// <summary>
        /// Gets <see cref="System.EventArgs"/> specified in <see cref="Start(System.EventArgs)"/> used to pass into <see cref="Tick"/> event.
        /// </summary>
        public EventArgs EventArgs => m_eventArgs;

        #endregion [ Properties ]

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="PrecisionTimer"/> object.
        /// </summary>
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

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="PrecisionTimer"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
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

        /// <summary>
        /// Starts the <see cref="PrecisionTimer"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// The timer has already been disposed.
        /// </exception>
        /// <exception cref="TimerStartException">
        /// The timer failed to start.
        /// </exception>
        public void Start()
        {
            Start(EventArgs.Empty);
        }

        /// <summary>
        /// Starts the <see cref="PrecisionTimer"/> with the specified <see cref="EventArgs"/>.
        /// </summary>
        /// <param name="userArgs">User defined event arguments to pass into raised <see cref="Ticks"/> event.</param>
        /// <exception cref="ObjectDisposedException">
        /// The timer has already been disposed.
        /// </exception>
        /// <exception cref="TimerStartException">
        /// The timer failed to start.
        /// </exception>
        public void Start(EventArgs userArgs)
        {
            if (m_disposed)
                throw new ObjectDisposedException("Attempted to Start PrecisionTimer when it was already Disposed");

            if (m_running)
                return;

            // Cache user event args to pass into Ticks parameter
            m_eventArgs = userArgs;

            // Create and start timer.
            m_timerID = timeSetEvent(m_period, m_resolution, m_timeProc, userTimerReference, m_mode);

            // If the timer was created successfully.
            if (m_timerID != 0)
            {
                m_running = true;

                if (Started is object)
                    Started(this, EventArgs.Empty);
            }
            else
            {
                throw new TimerStartException("Unable to start precision timer");
            }
        }

        /// <summary>
        /// Stops <see cref="PrecisionTimer"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// If the timer has already been disposed.
        /// </exception>
        public void Stop()
        {
            if (m_disposed)
                throw new ObjectDisposedException("Attempted to Stop PrecisionTimer when it was already Disposed");

            if (!m_running)
                return;

            timeKillEvent(m_timerID);
            m_timerID = 0;

            m_running = false;

            if (Stopped is object)
                Stopped(this, EventArgs.Empty);
        }

        /// <summary>
        /// TimerEventCallback Method called by the Win32-MultiMedia Timer (HPET)
        /// </summary>
        private void TimerEventCallback(int hwnd, int uMsg, IntPtr idEvent, int dwTime, int WTF)
        {
            if (Tick is object)
            {
                // Pass the Parcel: Create STRONG Reference in the GC during our TimerEventCallback
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

            // Tells the Garbage Collector to leave ALL PrecisionTimer objects alone until at least this point (Does the same thing as passing the parcel)
            GC.KeepAlive(this);

            if (m_mode == TimerMode.OneShot)
            {
                Stop();
                return;
            }

            return;
        }

        #endregion [ Methods ]

        #region [ Static ]

        private static readonly TimerCapabilities s_capabilities;

        static PrecisionTimer()
        {
            timeGetDevCaps(ref s_capabilities, Marshal.SizeOf(s_capabilities));
        }

        /// <summary>
        /// Gets the system multimedia timer capabilities.
        /// </summary>
        public static TimerCapabilities Capabilities => s_capabilities;

        /// <summary>
        /// Minimum timer resolution, in milliseconds, for the application.
        /// </summary>
        /// <param name="period">
        /// Minimum timer resolution, in milliseconds, for the application.
        /// A lower value specifies a higher (more accurate) resolution.
        /// </param>
        public static void SetMinimumTimerResolution(int period)
        {
            if (timeBeginPeriod(period) != 0)
                throw new InvalidOperationException("Specified period resolution is out of range and is not supported.");
        }

        /// <summary>
        /// Clear timer resolution specified in the previous call to the <see cref="SetMinimumTimerResolution"/> function
        /// </summary>
        /// <param name="period">Minimum timer resolution specified in the previous call to the <see cref="SetMinimumTimerResolution"/> function</param>
        public static void ClearMinimumTimerResolution(int period)
        {
            if (timeEndPeriod(period) != 0)
                throw new InvalidOperationException("Specified period resolution is out of range and is not supported.");
        }

        /// <summary>
        /// TimerCapabilities - Minimum and Maximum supported period in milliseconds.
        /// </summary>
        /// <param name="caps">Capabilities</param>
        /// <param name="sizeOfTimerCaps">Size of Caps</param>
        /// <returns></returns>
        [DllImport("winmm.dll")]
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        private static extern int timeGetDevCaps(ref TimerCapabilities caps, int sizeOfTimerCaps);

        /// <summary>
        /// TimerProc - Creates the timer
        /// </summary>
        /// <param name="delay">Event Interval</param>
        /// <param name="resolution">Event Resolution</param>
        /// <param name="proc">TimerProc event</param>
        /// <param name="user">User Reference</param>
        /// <param name="mode">TimerMode (Periodic, Oneshot)</param>
        /// <returns>High Precision Event Timer</returns>
        [DllImport("winmm.dll")]
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        private static extern int timeSetEvent(int delay, int resolution, TimerProc proc, IntPtr user, TimerMode mode);

        /// <summary>
        /// TimerKillEvent - Stops the Timer
        /// </summary>
        /// <param name="id">ID of the HPET timer you want to kill</param>
        /// <returns>Boolean</returns>
        [DllImport("winmm.dll")]
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        private static extern int timeKillEvent(int id);

        /// <summary>
        /// Requested Minimum HPET Resolution
        /// Actual resolution will differ slightly at times, You don't need to compensate for this so long as you use relative time and check if you missed events.
        /// </summary>
        /// <param name="period">Requested Resolution</param>
        /// <returns></returns>
        [DllImport("winmm.dll")]
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        private static extern int timeBeginPeriod(int period);

        /// <summary>
        /// Reset HPET Resolution
        /// </summary>
        /// <param name="period">Must match previous call to TimeBeginPeriod</param>
        /// <returns></returns>
        [DllImport("winmm.dll")]
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        private static extern int timeEndPeriod(int period);

        // Note: This timer won't work at all if the user disables "High Precision Event Timer" in the Device Manager
        // This is actually why this method isn't the default timer and partially because of availability of timer
        // and differing quality of the timing crystal based on motherboard manufacturer,
        // but mostly because the user can arbitrarily disable it.

        #endregion [ Static ]
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TimerCapabilities
    {
        /// <summary>Minimum supported period in milliseconds.</summary>
        public int PeriodMinimum;

        /// <summary>Maximum supported period in milliseconds.</summary>
        public int PeriodMaximum;
    }

    [Serializable]
    public class TimerStartException : Exception
    {
        public TimerStartException(string message)
            : base(message)
        {
        }
    }

    [Serializable]
    public class TimerTaskFailed : Exception
    {
        public TimerTaskFailed(string message)
            : base(message)
        {
        }
    }
}