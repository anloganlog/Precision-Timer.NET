namespace PrecisionTiming
{
    static class Constants
    {
        internal const string disposedString = "Tried to Dipose PrecisionTimer when it was already Disposed";
        internal const string disposedStopString = "Tried to Stop PrecisionTimer when it was already Disposed";
        internal const string disposedStartString = "Attempted to Start PrecisionTimer when it was already Disposed";
        internal const string disposedAutoResetString = "Tried to change AutoReset on PrecisionTimer when it was already Disposed";
        internal const string disposedEventArgsString = "Tried to change EventArgs on PrecisionTimer when it was already Disposed";
        internal const string disposedResolutionString = "Tried to change Resolution on PrecisionTimer when it was already Disposed";
        internal const string disposedPeriodString = "Tried to change Period on PrecisionTimer when it was already Disposed";
        internal const string disposedIsRunningString = "Tried to check if Timer was running when it was already Disposed";

        internal const string outOfRangeMaxPeriodString = "Multimedia Timer period out of range, max value is: ";
        internal const string outOfRangeMinPeriodString = "Multimedia Timer period out of range, min value is: ";
        internal const string outOfRangeMaxResolutionString = "Multimedia Timer resolution out of range, max value is: ";
        internal const string outOfRangeMinResolutionString = "Multimedia Timer resolution out of range, min value is 0";

        internal const string timerRunningString = "Periodic Timers must be stopped before you can configure them";
        internal const string timerEventRunningString = "Precision Timer Events must be stopped before you can configure them";

        internal const string windowsMultimediaAPIString = "winmm.dll";
        internal const string unableToStartString = "Unable to start the Precision Timer, please reconfigure, you may also be at the limit for your platform";
        internal const string resolutionNotSupportedString = "Specified period resolution is out of range and is not supported.";
    }
}
