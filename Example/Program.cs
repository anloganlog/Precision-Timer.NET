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
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            pt.Start();

            Task.Run(async () =>
            {
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

                pt.Dispose(); // DO NOT Call Dispose unless you really mean it.
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
