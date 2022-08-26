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

using PrecisionTiming;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Example
{
    /// <summary>
    /// Using the Action() is faster than the Tick EventHandler
    /// If you use both the Tick comes after the Action()
    /// </summary>
    internal class Program
    {
        private static PrecisionTimer PrecisionTimer = new PrecisionTimer();                // You should always make PrecisionTimers static if you can
        private static PrecisionTimerEvent PrecisionTimerEvent = new PrecisionTimerEvent(); // Event only version of PrecisionTimer

        // Actions
        private static Action Action = () =>
        {
            // timer.Tick = null;
            // Using an Action is the fastest option
            ActionCount++;

            Console.WriteLine("[" + ActionCount + "] Some Action");
        };

        private static Action SomeOtherAction = () =>
        {
            ActionCount++;
            Console.WriteLine("[" + ActionCount + "] Some Other Action");
        };

        // Events
        private static void Started(object sender, EventArgs args)
        {
            Console.WriteLine("Timer Started");
        }

        private static void Stop(object sender, EventArgs args)
        {
            Console.WriteLine("Timer Stopped");
        }

        // You could use PrecisionTimerEvent instead if you only want a 1ms Tick Event and don't use Action()
        private static void Tick(object sender, EventArgs args)
        {
            // Tick Event triggers after Action();

            try
            {
                //List<bool> f = new List<bool>(); f.Where(c => f.Equals(true)).Single();
            }
            catch
            {
                // InvalidOperationException would be Ignored
            }

            //List<bool> f = new List<bool>(); f.Where(c => f.Equals(true)).Single();  // InvalidOperationException Crash
            // PrecisionTimer.NET does not handle exceptions for you.

            Console.WriteLine("Timer Ticked");
        }

        // Main
        private static void Main(string[] args)
        {
            //PrecisionTimerSettings.SetMinimumTimerResolution(0);

            Console.WindowHeight = 50;
            Console.WindowWidth = 56;

            Task.Run(async () =>
            {
                // Subscribe Events
                PrecisionTimer.Started += Started;
                PrecisionTimer.Stopped += Stop;
                //PrecisionTimer.Tick += Tick;

                PrecisionTimer.SetResolution(0); // Best resolution available, still uses very little CPU
                //PrecisionTimer.Resolution = 1; // less CPU usage
                //PrecisionTimer.Resolution = 2; // etc..

                //pt.SetInterval(Action, Interval, Start, Periodic, EventArgs, Resolution);
                PrecisionTimer.SetInterval(Action, 1, resolution: 1, start: false); // Default resolution is 0 when using SetInterval
                //PrecisionTimer.Configure(Action, 1, resolution: 1, start: false); // You can also use Configure in place of SetInterval

                await PeriodicTest();

                await OneShotTests();

                await DisposeTest(false);

                await PrecisionTimerEventTest();

                await TestGarbageCollector();

                Console.WriteLine("Test Completed");
                //PrecisionTimerSettings.ClearMinimumTimerResolution(0);
            });


            Console.ReadLine();
        }

        /// <summary>
        /// Start a PrecisionTimer and then wait 100ms with Task.Delay to see how many times the callback fires
        /// <para>Task.Delay isn't very accurate sometimes so depending on your hardware the callback will fire 95-120 times</para>
        /// </summary>
        /// <returns></returns>
        private static async Task PeriodicTest()
        {
            ActionCount = 0;

            Console.WriteLine("-------------");
            Console.WriteLine("Periodic Test");
            Console.WriteLine("How long does Task.Delay think 100ms is right now?");
            Console.WriteLine("-------------");
            await Task.Delay(1000);

            PrecisionTimer.Start();
            await Task.Delay(100);
            PrecisionTimer.Stop();

            await Task.Delay(2);
            Console.WriteLine(ActionCount + "/100ish"); ActionCount = 0;

            Console.WriteLine("-------------");
        }

        /// <summary>
        /// Start a PrecisionTimer as a one-shot timer and then use Task.Delay to simulate some other work before it was needed again.
        /// It is not recommended to use Task.Delay for small delays
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static async Task OneShotTests()
        {
            Console.WriteLine("-------------");
            Console.WriteLine("One-Shot Test");
            Console.WriteLine("One-Shot Timers Stop themselves, You can just call Start next time");
            Console.WriteLine("-------------");

            await Task.Delay(1000);
            PrecisionTimer.SetInterval(Action, 1, start: false, periodic: false);
            if (PrecisionTimer.Start())
            {
                Console.WriteLine("-------1-------");
            }

            // Timers must be stopped to reconfigure some settings or will throw a TimerRunningException, eg.
            // PrecisionTimer.SetInterval(1);
            // PrecisionTimer.SetAutoResetMode(false);
            // PrecisionTimer.SetResolution(0);

            await Task.Delay(2);
            PrecisionTimer.SetInterval(Action, 1, periodic: false);
            if (PrecisionTimer.Start())
            {
                Console.WriteLine("-------2-------" + " | IsRunning: " + PrecisionTimer.IsRunning());
            }

            await Task.Delay(2);
            PrecisionTimer.SetInterval(Action, 1, periodic: false); // SetInterval will call start for you with these settings
            if (PrecisionTimer.Start()) // If you start the timer with SetInterval, this will just confirm the timer IsRunning
            {
                Console.WriteLine("-------3-------");
            }

            await Task.Delay(2);
            PrecisionTimer.Stop(); // Does nothing after the callback because this is a one-shot timer and it already stopped
            PrecisionTimer.SetInterval(SomeOtherAction, 1, start: false, periodic: false);
            if (PrecisionTimer.Start())
            {
                Console.WriteLine("-------4-------");
            }

            await Task.Delay(2);
            PrecisionTimer.SetAction(Action); // One-shot can configure Action() without stopping
            if (PrecisionTimer.Start())
            {
                Console.WriteLine("-------5-------");
            }

            await Task.Delay(2);
            PrecisionTimer.SetInterval(SomeOtherAction, 1, start: false, periodic: false);
            if (PrecisionTimer.Start())
            {
                Console.WriteLine("-------6-------");
            }

            await Task.Delay(2);
            PrecisionTimer.SetInterval(Action, 1, start: false, periodic: false);
            if (PrecisionTimer.Start())
            {
                Console.WriteLine("-------7-------");
            }

            await Task.Delay(2);
            Console.WriteLine(ActionCount + "/7");
            if (ActionCount < 7)
            {
                Console.WriteLine("Failed"); // Task.Delay(0);
                throw new Exception("Failed"); // Task.Delay is literally broken
            }

            ActionCount = 0;

            Console.WriteLine("-------------");
            Console.WriteLine("One-Shot Test");
            Console.WriteLine("One-Shot Timers Stop themselves, You can just call Start next time");
            Console.WriteLine("-------------");
            await Task.Delay(1000).ConfigureAwait(false);

            PrecisionTimer.SetInterval(Action, 1, periodic: false);
            await Task.Delay(35).ConfigureAwait(false);
            Console.WriteLine("-------1-------");

            PrecisionTimer.SetAction(SomeOtherAction);  // You can set just the Action and then Start again
            PrecisionTimer.Start();
            await Task.Delay(22).ConfigureAwait(false);
            Console.WriteLine("-------2-------");

            PrecisionTimer.SetInterval(Action, 20, periodic: false);
            PrecisionTimer.Start();
            await Task.Delay(20).ConfigureAwait(false);
            Console.WriteLine("-------3-------");

            PrecisionTimer.SetInterval(Action, 40, periodic: false);
            PrecisionTimer.Start();
            await Task.Delay(40).ConfigureAwait(false);
            Console.WriteLine("-------4-------");

            PrecisionTimer.SetInterval(Action, 40, periodic: false);
            PrecisionTimer.Start();
            await Task.Delay(40).ConfigureAwait(false);
            Console.WriteLine("-------5-------");

            PrecisionTimer.SetAction(SomeOtherAction);
            PrecisionTimer.Start();
            await Task.Delay(144).ConfigureAwait(false);
            Console.WriteLine("-------6-------");

            PrecisionTimer.Stop();

            Console.WriteLine(ActionCount + "/6");
            if (ActionCount < 6)
            {
                Console.WriteLine("Failed");
                throw new Exception("Failed");
            }

            ActionCount = 0;
            Console.WriteLine("-------------");
        }

        /// <summary>
        /// Start a PrecisionTimerEvent and then wait 100ms with Task.Delay to see how many times the callback fires
        /// <para>Task.Delay isn't very accurate sometimes so depending on your hardware the callback will fire 95-120 times</para>
        /// </summary>
        /// <returns></returns>
        private static async Task PrecisionTimerEventTest()
        {
            ActionCount = 0;

            Console.WriteLine("-------------");
            Console.WriteLine("PrecisionTimerEvent Test");
            Console.WriteLine("How long does Task.Delay think 100ms is right now using Only Events?");
            Console.WriteLine("-------------");
            await Task.Delay(1000);

            PrecisionTimerEvent.Tick += TickEventOnly;

            PrecisionTimerEvent.Start();
            await Task.Delay(100);
            PrecisionTimerEvent.Stop();

            Console.WriteLine(eventTestCount + "/100ish"); eventTestCount = 0;

            PrecisionTimerEvent.Dispose();

            Console.WriteLine("-------------");
        }


        /// <summary>
        /// Deliberately try provoke the garbage collector into hating on us because of an edge case
        /// <para>This test fails if the next shot is never fired (it will)</para>
        /// </summary>
        /// <returns></returns>
        private static async Task TestGarbageCollector()
        {
            Console.WriteLine("One Shot Test With Varying Delay (GC Test)");
            await Task.Delay(1000).ConfigureAwait(false);

            int initialDelay = 1;

            for (int i = 0; i < 27; i++)
            {
                PrecisionTimer.SetInterval(Bang, 1, periodic: false);

                await Task.Delay(2).ConfigureAwait(false);
                Console.WriteLine("One Shot will be fired after: " + initialDelay + "ms - " + (i + 1) + "/27");

                PrecisionTimer.Stop();
                await Task.Delay(initialDelay + 1).ConfigureAwait(false);

                initialDelay *= 2;
            }

            Console.WriteLine("-------------");
        }

        /// <summary>
        /// Dispose of the PrecisionTimer and then create a new one and Run a PeriodicTest()
        /// </summary>
        /// <param name="lastTest">true if this is the last test</param>
        /// <returns></returns>
        private static async Task DisposeTest(bool lastTest)
        {
            try
            {
                PrecisionTimer.Stop();
                PrecisionTimer.Dispose();
                Console.WriteLine("This timer won't start again because dispose was called");
                PrecisionTimer.Start();               // <- This will not work and will throw Object Disposed Exception
            }
            catch
            {
                Trace.WriteLine("Ignored Exception"); // Object Disposed Exception Ignored
            }

            ActionCount = 0;

            Console.WriteLine("Created New Timer");
            PrecisionTimer = new PrecisionTimer();  // <- This will work with the new garbage collection changes
            PrecisionTimer.SetAction(Action);       // <- This will work
            await PeriodicTest();
            if (lastTest)
            {
                PrecisionTimer.Dispose();
            }
        }

        public static Action Bang = () =>
        {
            Console.WriteLine("------Bang-------");
        };

        // You could use PrecisionTimerEvent instead if you only want a 1ms Tick Event and don't use Action()
        private static void TickEventOnly(object sender, EventArgs args)
        {
            eventTestCount++;
            Console.WriteLine("[" + eventTestCount + "] Timer Ticked");
        }

        static volatile int ActionCount = 0;
        static volatile int eventTestCount = 0;
    }
}
