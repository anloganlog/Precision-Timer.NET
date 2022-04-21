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
    /// Occurs when there is an attempt to modify a <see cref="PrecisionTimer"/> without using SetInterval first
    /// </summary>
    [Serializable]
    public class TimerNotConfigured : Exception
    {
        /// <summary>
        /// Occurs when there is an attempt to modify a <see cref="PrecisionTimer"/> without using SetInterval first
        /// </summary>
        /// <param name="message">User friendly error message</param>
        public TimerNotConfigured(string message)
            : base(message)
        {
        }
    }
}