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
using System.Runtime.InteropServices;

namespace PrecisionTiming
{
    internal static class MMTimerExports
    {
        [DllImport("winmm.dll")]
        internal static extern int timeBeginPeriod(int period);

        [DllImport("winmm.dll")]
        internal static extern int timeEndPeriod(int period);

        [DllImport("winmm.dll")]
        internal static extern int timeGetDevCaps(ref TimerCapabilities caps, int sizeOfTimerCaps);

        [DllImport("winmm.dll")]
        internal static extern int timeKillEvent(int id);

        [DllImport("winmm.dll")]
        internal static extern int timeSetEvent(int delay, int resolution, TimerProc proc, IntPtr user, TimerMode mode);

        internal delegate void TimerProc(int hwnd, int uMsg, IntPtr idEvent, int dwTime, int WTFref);

        internal static TimerCapabilities Capabilities;
    }
}