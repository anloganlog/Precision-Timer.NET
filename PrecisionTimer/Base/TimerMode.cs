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

namespace PrecisionTiming
{
    /// <summary>
    /// The Mode for the <see cref="PrecisionTimer"/>
    /// </summary>
    public enum TimerMode
    {
        ///<summary> Timer event occurs once.</summary>
        OneShot,

        /// <summary>Timer event occurs periodically.</summary>
        Periodic
    }
}