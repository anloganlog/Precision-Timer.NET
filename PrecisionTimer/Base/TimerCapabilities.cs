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
using System.Runtime.InteropServices;

namespace PrecisionTiming
{
    /// <summary>
    /// The Min/Max supported period for the Mutlimedia Timer in milliseconds
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TimerCapabilities
    {
        /// <summary>Minimum supported period in milliseconds.</summary>
        public int PeriodMinimum;

        /// <summary>Maximum supported period in milliseconds.</summary>
        public int PeriodMaximum;
    }
}