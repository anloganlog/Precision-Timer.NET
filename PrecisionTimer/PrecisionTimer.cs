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

namespace PrecisionTiming
{
    /// <summary>
    /// High Resolution Multimedia Timer Wrapper
    /// <para>The default timer will Fire the Tick event every 1 Millisecond forever</para>
    /// <para>Subscribe to <see cref="EventHandler"/> <see cref="Tick"/></para>
    /// <para>You can also provide a Task using <see cref="SetAction"/> or <see cref="SetInterval(Action, int, bool, bool, EventArgs)"/></para>
    /// </summary>
    public class PrecisionTimer
    {
        /// <summary>
        /// Occurs when the <see cref="PrecisionTimer"/> has started.
        /// </summary>
        public EventHandler Started;

        /// <summary>
        /// Occurs when the <see cref="PrecisionTimer"/> has stopped.
        /// </summary>
        public EventHandler Stopped;

        /// <summary>
        /// Occurs when the <see cref="PrecisionTimer"/> Ticks at the Interval.
        /// </summary>
        public EventHandler Tick;

        /// <summary>
        /// True if the Timer is running
        /// Will also be false if no <see cref="PrecisionTimer"/> is configured
        /// </summary>
        /// <returns></returns>
        public bool? IsRunning()
        {
            return Timer?.IsRunning;
        }

        /// <summary>
        /// Set the Interval and Tick Event of the <see cref="PrecisionTimer"/> and decide if it should start automatically
        /// Start the timer with the default settings
        /// </summary>
        /// <param name="TimerTask">The Action</param>
        /// <param name="Interval">The Interval for the TimerTask in Milliseconds</param>
        /// <param name="Periodic">True if Periodic / False if OneShot</param>
        /// <param name="Start">True if the timer should start automatically with the default settings, false if you are going to configure/start it later</param>
        /// <param name="args">Optional user provided EventArgs</param>
        public void SetInterval(Action TimerTask, int Interval, bool Start = true, bool Periodic = true, EventArgs args = null)
        {
            Timer = new MMTimer();
            Timer.Tick += (sender, args) => { TimerTask(); };
            Timer.SetAutoReset = Periodic;
            Timer.SetResolution = 0;
            Timer.SetPeriod = Interval;
            Timer.SetArgs = args;

            if (Start)
            {
                Timer.Start();
            }
        }

        /// <summary>
        /// Set the Action of the <see cref="PrecisionTimer"/> before you Start the Timer
        /// <para>You could also subscribe to the Event <see cref="Tick"/></para>
        /// </summary>
        /// <param name="TimerTask">The Action</param>
        public void SetAction(Action TimerTask)
        {
            if (CheckTimerValid())
            {
                Timer.Tick += (sender, args) => { TimerTask(); };
            }
        }

        /// <summary>
        /// Start the <see cref="PrecisionTimer"/>
        /// </summary>
        public void Start(EventArgs args = null)
        {
            if (CheckTimerValid())
            {
                Timer.Started += Started;
                Timer.Stopped += Stopped;
                Timer.Tick += Tick;
                Timer.Start(args);
            }
        }

        /// <summary>
        /// Stop the <see cref="PrecisionTimer"/>
        /// </summary>
        public void Stop(EventArgs args = null)
        {
            if (Timer != null)
            {
                Timer.Stop();

                Timer.Tick = null;
                Timer.Started = null;
                Timer.Stopped = null;
            }
        }

        /// <summary>
        /// Set the Interval (Period) of the <see cref="PrecisionTimer"/> before you Start the Timer
        /// </summary>
        public void SetInterval(int Interval)
        {
            if (CheckTimerValid())
            {
                Timer.SetPeriod = Interval;
            }
        }

        /// <summary>
        /// Get the Interval (Period) of the <see cref="PrecisionTimer"/>
        /// </summary>
        public int? GetInterval => Timer?.GetPeriod;

        /// <summary>
        /// Set the Resolution of the <see cref="PrecisionTimer"/> before you Start the Timer
        /// <para>Default: 0</para>
        /// </summary>
        public void SetResolution(int Resolution)
        {
            if (CheckTimerValid())
            {
                Timer.SetResolution = Resolution;
            }
        }

        /// <summary>
        /// Get the Resolution of the <see cref="PrecisionTimer"/>
        /// <para>Default: 0</para>
        /// </summary>
        public int? GetResolution => Timer?.GetResolution;

        /// <summary>
        /// Set the Periodic/OneShot Mode of the <see cref="PrecisionTimer"/> before you Start the Timer
        /// <para>Default:True (Periodic)</para>
        /// </summary>
        public void SetPeriodic(bool periodic)
        {
            if (CheckTimerValid())
            {
                Timer.SetAutoReset = periodic;
            }
        }

        /// <summary>
        /// Get the Periodic/OneShot Mode of the <see cref="PrecisionTimer"/>
        /// <para>Default:True (Periodic)</para>
        /// </summary>
        public bool? GetPeriodic => Timer?.GetAutoReset;

        /// <summary>
        /// Set Event Args of the <see cref="PrecisionTimer"/> before you Start the Timer
        /// </summary>
        public void SetEventArgs(EventArgs args)
        {
            if (CheckTimerValid())
            {
                Timer.SetArgs = args;
            }
        }

        /// <summary>
        /// Get Event Args for the Timer
        /// </summary>
        public EventArgs GetEventArgs => Timer?.GetArgs;

        /// <summary>
        /// <para>DESTROY</para>
        /// Release all resources for this <see cref="PrecisionTimer"/>
        /// <para>If you call Dispose when you intend to create a new Timer you will have a bad time..r</para>
        /// </summary>
        public void Dispose()
        {
            if (Timer != null)
            {
                Timer.Dispose();
            }
        }

        internal volatile MMTimer Timer;

        internal bool CheckTimerValid()
        {
            if (Timer == null)
            {
                Timer = new MMTimer();
            }

            return true;
        }
    }
}
