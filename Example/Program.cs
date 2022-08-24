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
        private static PrecisionTimer pt = new PrecisionTimer();

        private static void Main(string[] args)
        {
            pt.Started += Started;
            pt.Stopped += Stop;
            pt.Tick += Tick;

            Console.WriteLine("Periodic Test");

            //pt.SetInterval(Action, Interval, Start, Periodic, EventArgs);
            pt.SetInterval(1);
            pt.Start();            

            Task.Run(async () =>
            {
                for (int i = 0; i < 1; i++)
                {
                    // 1ms according to Task.Delay is about 10-15ms depending on your hardware               
                    await Task.Delay(1).ConfigureAwait(false);
                    // So Precision Timer will tick 10-15 times depending on your hardware
                }

                pt.Stop();

                pt.Start();
                for (int i = 0; i < 1; i++)
                {
                    // 1ms according to Task.Delay is about 10-15ms depending on your hardware
                    await Task.Delay(1).ConfigureAwait(false);
                    // So Precision Timer will tick 10-15 times depending on your hardware
                }

                pt.Stop();
                pt.Start();
                for (int i = 0; i < 1; i++)
                {
                    // 1ms according to Task.Delay is about 10-15ms depending on your hardware
                    await Task.Delay(1).ConfigureAwait(false);
                    // So Precision Timer will tick 10-15 times depending on your hardware
                }

                pt.Stop();

                Console.WriteLine("-------------");
                Console.WriteLine("One Shot Test");

                pt.SetPeriodic(false);

                pt.Start();

                for (int i = 0; i < 1; i++)
                {
                    await Task.Delay(1).ConfigureAwait(false);
                }

                pt.Stop();

                pt.Start();

                for (int i = 0; i < 1; i++)
                {
                    await Task.Delay(1).ConfigureAwait(false);
                }

                pt.Stop();

                pt.Start();

                for (int i = 0; i < 1; i++)
                {
                    await Task.Delay(1).ConfigureAwait(false);
                }

                pt.Stop();

                pt.Start();

                for (int i = 0; i < 1; i++)
                {
                    await Task.Delay(1).ConfigureAwait(false);
                }

                pt.Stop();

                // One-shot timers Stop themselves, You can just call Start next time;
                // Do not dispose unless you really mean it. (Application is exiting)

                Console.WriteLine("-------------");
                Console.WriteLine("One Shot Test Without Stopping First");

                pt.Start();

                for (int i = 0; i < 1; i++)
                {
                    await Task.Delay(1).ConfigureAwait(false);
                }

                pt.Start();

                for (int i = 0; i < 1; i++)
                {
                    await Task.Delay(1).ConfigureAwait(false);
                }

                pt.Start();

                for (int i = 0; i < 1; i++)
                {
                    await Task.Delay(1).ConfigureAwait(false);
                }

                pt.Stop();

                Console.WriteLine("-------------");
                Console.WriteLine("One Shot Test With Varying Delay");

                int initialDelay = 5000;

                for (int i = 0; i < 25; i++)
                {                  
                    pt.Start();
                    await Task.Delay(1).ConfigureAwait(false);
                    Console.WriteLine("One Shot will be fired after: " + initialDelay + "ms - " + (i + 1) + "/100");
                      
                    pt.Stop();
                    await Task.Delay(initialDelay).ConfigureAwait(false);  

                    initialDelay = initialDelay * 2;
                }

                pt.Stop();
                pt.Dispose(); // DO NOT Call Dispose unless you really mean it.
                Console.WriteLine("Timer won't start again because dispose was called");
                pt.Start(); // <- This will not work
                pt = new PrecisionTimer(); // <- This will not work
                pt.Start(); // <- This will not work
            });

            Console.ReadLine();
        }

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
    }
}
