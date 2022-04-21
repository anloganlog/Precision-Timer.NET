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

namespace PrecisionTiming
{
    /// <summary>
    /// High Resolution Multimedia Timer Wrapper
    /// </summary>
    public class PrecisionTimer
    {
        /// <summary>
        /// Occurs when the <see cref="PrecisionTimer"/> has started.
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Occurs when the <see cref="PrecisionTimer"/> has stopped.
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Set the Interval and Tick Event of the <see cref="PrecisionTimer"/> and decide if it should start automatically
        /// Start the timer with the default settings
        /// </summary>
        /// <param name="TimerTask">The Action</param>
        /// <param name="Interval">The Interval for the TimerTask in Milliseconds</param>
        /// <param name="Start">True if the timer should start automatically with the default settings, false if you are going to configure/start it later</param>
        /// <param name="args">Optional user provided EventArgs</param>
        public void SetInterval(Action TimerTask, int Interval, bool Start = true, EventArgs args = null)
        {            
            Timer = new MMTimer();
            Timer.Tick += (sender, args) => { TimerTask(); };
            Timer.AutoReset = true;
            Timer.Period = Interval;
            Timer.Resolution = 0;

            if (Start)
            {
                Timer.Start(args);
            }
        }

        /// <summary>
        /// True if the Timer is running
        /// Will also be false if no <see cref="PrecisionTimer"/> is configured
        /// </summary>
        /// <returns></returns>
        public bool IsRunning()
        {
            return Timer != null ? Timer.IsRunning : false;
        }

        /// <summary>
        /// Start the <see cref="PrecisionTimer"/>
        /// </summary>
        public void Start()
        {
            if (CheckTimerValid())
            {
                if (Timer.Start())
                {
                    if (Started is object)
                        Started(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Stop the <see cref="PrecisionTimer"/>
        /// </summary>
        public void Stop()
        {
            if (CheckTimerValid())
            {
                Timer.Stop();

                if (Stopped is object)
                    Stopped(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Set the Action of the <see cref="PrecisionTimer"/>
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
        /// Set the Period of the <see cref="PrecisionTimer"/>
        /// </summary>
        public void SetPeriod(int Interval)
        {
            if (CheckTimerValid())
            {
                Timer.Period = Interval;
            }
        }

        /// <summary>
        /// Set the Resolution of the <see cref="PrecisionTimer"/>
        /// <para>Default: 0</para>
        /// </summary>
        public void SetResolution(int Resolution)
        {
            if (CheckTimerValid())
            {
                Timer.Resolution = Resolution;
            }
        }

        /// <summary>
        /// Set to True if the TimerTask should reset after it runs or only fire once (Repeat)
        /// <para>Default: True</para>
        /// </summary>
        public void SetAutoReset(bool AutoReset)
        {
            if (CheckTimerValid())
            {
                Timer.AutoReset = AutoReset;
            }
        }

        /// <summary>
        /// Release all resources for this <see cref="PrecisionTimer"/>
        /// </summary>
        public void Dispose()
        {
            Timer?.Dispose();
        }

        internal volatile MMTimer Timer;

        internal bool CheckTimerValid()
        {
            if (Timer == null)
            {
                throw new TimerNotConfigured("You must configure the PrecisionTimer with SetInterval before you can use Set/Start/Stop");
            }

            return true;
        }
    }
}