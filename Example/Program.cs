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
using System.Threading.Tasks;

namespace Example
{
    internal class Program
    {
        private static PrecisionTimer timer = new PrecisionTimer();

        // Actions
        private static Action Action = () =>
        {
            Console.WriteLine("Some Action");
        };

        private static Task SomeOtherAction()
        {
            Console.WriteLine("Some Other Action"); return Task.CompletedTask;
        }

        // Events
        private static void Started(object sender, EventArgs args)
        {
            Console.WriteLine("Timer Started");
        }

        private static void Stop(object sender, EventArgs args)
        {
            Console.WriteLine("Timer Stopped");
        }

        private static void Tick(object sender, EventArgs args)
        {
            Console.WriteLine("Timer Ticked");
        }

        // Main
        private static void Main(string[] args)
        {
            // Subscribe Events
            timer.Started += Started;
            timer.Stopped += Stop;
            timer.Tick += Tick;

            Console.WriteLine("Periodic Test");

            //pt.SetInterval(Action, Interval, Start, Periodic, EventArgs);

            timer.SetResolution(0); // Best resolution available, still uses very little CPU
            //timer.SetResolution(1); // less CPU usage
            //timer.SetResolution(2); // etc..

            timer.SetInterval(Action, 1, Resolution: 1); // Default resolution is 0 when using SetInterval

            //ReusablePrecisionTimer.SetAction(Action);    // You can set these individually or use some combination of SetInterval
            timer.Start();

            Console.WriteLine("IsRunning: " + timer.IsRunning());

            Task.Run(async () =>
            {
                await Task.Delay(1).ConfigureAwait(false);

                timer.Stop();
                timer.SetInterval(Action, 1);
                timer.Start();
                // 1ms according to Task.Delay is about 10-15ms depending on your hardware
                await Task.Delay(1).ConfigureAwait(false);
                // So Precision Timer will tick 10-15 times depending on your hardware

                timer.Stop();
                timer.SetInterval(Action, 1);
                timer.Start();
                await Task.Delay(1).ConfigureAwait(false);
                timer.Stop();
                Console.WriteLine("-------------");
                Console.WriteLine("One Shot Test");

                timer.SetInterval(Action, 1, start: false, Periodic: false);
                timer.Start();
                await Task.Delay(1).ConfigureAwait(false);

                timer.Stop();
                timer.SetInterval(Action, 1, Periodic: false); // Set interval will call start with these settings
                timer.Start(); // calling start again by accident won't do anything now
                await Task.Delay(1).ConfigureAwait(false);

                timer.Stop();
                timer.SetInterval(Action, 1, start: false, Periodic: false);
                timer.Start();
                await Task.Delay(1).ConfigureAwait(false);

                timer.Stop();
                timer.SetInterval(() => { SomeOtherAction().ConfigureAwait(false); }, 1, start: false, Periodic: false);
                timer.Start();
                await Task.Delay(1).ConfigureAwait(false);

                timer.Stop();

                // One-shot timers Stop themselves, You can just call Start next time;
                // Do not dispose unless you really mean it. (Application is exiting)

                Console.WriteLine("-------------");
                Console.WriteLine("One Shot Test Without Stopping Manually First");

                timer.SetInterval(Action, 1, start: false, Periodic: false);
                timer.Start();
                await Task.Delay(1).ConfigureAwait(false);

                timer.SetInterval(() => { SomeOtherAction().ConfigureAwait(false); }, 1, start: false, Periodic: false);
                timer.Start();
                await Task.Delay(1).ConfigureAwait(false);

                timer.SetInterval(Action, 1, start: false, Periodic: false);
                timer.Start();
                await Task.Delay(1).ConfigureAwait(false);

                timer.Stop();

                Console.WriteLine("-------------");
                Console.WriteLine("One Shot Test With Varying Delay (GC Test)");

                int initialDelay = 5000;

                for (int i = 0; i < 25; i++)
                {
                    timer.SetInterval(Action, 1, Periodic: false); // Set Interval can call Start by itself

                    await Task.Delay(1).ConfigureAwait(false);
                    Console.WriteLine("One Shot will be fired after: " + initialDelay + "ms - " + (i + 1) + "/25");

                    timer.Stop();
                    await Task.Delay(initialDelay).ConfigureAwait(false);

                    initialDelay = initialDelay * 2;
                }

                timer.Stop();
                timer.Dispose(); // DO NOT Call Dispose unless you really mean it, It really means Destroy
                Console.WriteLine("Timer won't start again because dispose was called");
                timer.Start(); // <- This will not work
                timer = new PrecisionTimer(); // <- This will not work
                timer.Start(); // <- This will not work
            });

            Console.ReadLine();
        }
    }
}
