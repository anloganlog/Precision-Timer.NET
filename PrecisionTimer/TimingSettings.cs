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