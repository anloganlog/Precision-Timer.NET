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
    /// Multimedia Timer Settings
    /// Allows you to configure your application to work better with high resolution timers
    /// </summary>
    public static class TimingSettings
    {
        /// <summary>
        /// Minimum timer resolution, in milliseconds, for the application.
        /// A lower value specifies a higher (more accurate) resolution.
        /// </summary>
        /// <param name="setMinimum">Minimum timer resolution in milliseconds</param>
        public static void SetMinimumTimerResolution(int setMinimum)
        {
            if (MMTimerExports.timeBeginPeriod(setMinimum) != 0)
                throw new InvalidOperationException("Specified period resolution is out of range and is not supported.");
        }

        /// <summary>
        /// Clear timer resolution specified in the previous call to the <see cref="SetMinimumTimerResolution"/>
        /// </summary>
        /// <param name="clearSetMinimum">setMinimum specified in the previous call to the <see cref="SetMinimumTimerResolution"/></param>
        public static void ClearMinimumTimerResolution(int clearSetMinimum)
        {
            if (MMTimerExports.timeEndPeriod(clearSetMinimum) != 0)
                throw new InvalidOperationException("Specified period resolution is out of range and is not supported.");
        }
    }
}
